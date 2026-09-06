using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Qbs.Domain;

namespace Qbs.Application;

public sealed class ClientWorkflows(IStudioStore store, IClock clock)
{
    public Task<PrintRequest> PrintRequest(Guid client, Guid id) =>
        store.Run(
            "client:" + client,
            async tx =>
            {
                var request = await tx.Get<PrintRequest>(id);
                if (request == null || request.ClientId != client)
                    throw new StudioException(404, "Request not found.");
                return request;
            }
        );

    public Task<object> Galleries(Guid client, Guid? sessionId = null) =>
        store.Run<object>(
            "client:" + client,
            async tx =>
            {
                var sessions = (await tx.List<Session>()).Where(x =>
                    PhotoAccess.SessionAllowed(x, client, clock.UtcNow)
                );
                if (sessionId == null)
                    return (object)
                        sessions
                            .Select(x => new
                            {
                                x.Id,
                                x.Name,
                                x.StartsAt,
                                x.ExpiresAt,
                            })
                            .ToArray();
                var session =
                    sessions.SingleOrDefault(x => x.Id == sessionId)
                    ?? throw new StudioException(404, "Gallery not found.");
                return new
                {
                    session.Id,
                    session.Name,
                    photos = (await tx.List<SessionPhoto>())
                        .Where(x => x.SessionId == session.Id && x.State == PhotoState.Ready)
                        .Select(x => new
                        {
                            x.Id,
                            x.Name,
                            thumbnailUrl = $"/api/client/photos/{x.Id}/preview?thumbnail=true",
                            url = $"/api/client/photos/{x.Id}/preview",
                        }),
                };
            }
        );

    public Task<Session> Assign(Guid id, Guid[] clients, long expected) =>
        store.Run(
            "media",
            async tx =>
            {
                var session =
                    await tx.Get<Session>(id)
                    ?? throw new StudioException(404, "Session not found.");
                session.ClientIds = clients.Distinct().ToArray();
                await tx.Save(session, expected);
                return session;
            }
        );

    public Task<Album> SaveAlbum(Guid client, Guid? id, Album input) =>
        store.Run(
            "client:" + client,
            async tx =>
            {
                var previous = id == null ? null : await tx.Get<Album>(id.Value);
                if (id != null && (previous == null || previous.ClientId != client))
                    throw new StudioException(404, "Album not found.");
                Rules.Text(input.Name, "name", 200);
                var ids = input.OrderedPhotoIds ?? input.PhotoIds;
                Rules.Require(
                    ids.Length > 0 || previous != null,
                    "Select at least one photo.",
                    "photoIds"
                );
                Rules.Require(
                    ids.Distinct().Count() == ids.Length,
                    "Photos must be unique.",
                    "photoIds"
                );
                foreach (var photo in ids.Except(previous?.PhotoIds ?? []))
                    await PhotoAccess.Require(tx, photo, client, clock.UtcNow);
                input.Id = id ?? Guid.NewGuid();
                input.ClientId = client;
                input.PhotoIds = ids;
                input.OrderedPhotoIds = null;
                await tx.Save(input, id == null ? 0 : input.ExpectedVersion);
                return input;
            }
        );

    public Task<object> Albums(Guid client, Guid? id = null) =>
        store.Run<object>(
            "client:" + client,
            async tx =>
            {
                if (id == null)
                    return (object)
                        (await tx.List<Album>())
                            .Where(x => x.ClientId == client)
                            .Select(x => new
                            {
                                x.Id,
                                x.Name,
                                x.Version,
                                count = x.PhotoIds.Length,
                            })
                            .ToArray();
                var album = await tx.Get<Album>(id.Value);
                if (album == null || album.ClientId != client)
                    throw new StudioException(404, "Album not found.");
                var photos = new List<object>();
                foreach (var photoId in album.PhotoIds)
                {
                    try
                    {
                        var photo = await PhotoAccess.Require(tx, photoId, client, clock.UtcNow);
                        photos.Add(
                            new
                            {
                                id = photoId,
                                photo.Name,
                                available = true,
                                thumbnailUrl = $"/api/client/photos/{photoId}/preview?thumbnail=true",
                                url = $"/api/client/photos/{photoId}/preview",
                            }
                        );
                    }
                    catch (StudioException ex) when (ex.Status == 404)
                    {
                        photos.Add(
                            new
                            {
                                id = photoId,
                                name = "Unavailable photo",
                                available = false,
                            }
                        );
                    }
                }
                return new
                {
                    album.Id,
                    album.Name,
                    album.Version,
                    photos,
                };
            }
        );

    public Task<PrintRequest> Submit(Guid client, PrintRequest input) =>
        store.Run(
            "media",
            async tx =>
            {
                Rules.Text(input.IdempotencyKey, "idempotencyKey", 100);
                Rules.Require(
                    input.Lines.Length > 0 && input.Lines.Length <= 1000,
                    "Select print lines.",
                    "lines"
                );
                Rules.Require(
                    input.Notes == null || input.Notes.Length <= 5000,
                    "Notes are too long.",
                    "notes"
                );
                var hash = Convert.ToHexString(
                    SHA256.HashData(
                        Encoding.UTF8.GetBytes(
                            JsonSerializer.Serialize(
                                new
                                {
                                    lines = input.Lines.Select(x => new
                                    {
                                        x.PhotoId,
                                        x.OptionId,
                                        x.Quantity,
                                        x.OptionRevision,
                                    }),
                                    input.Notes,
                                }
                            )
                        )
                    )
                );
                var previous = (await tx.List<PrintRequest>()).SingleOrDefault(x =>
                    x.ClientId == client && x.IdempotencyKey == input.IdempotencyKey
                );
                if (previous != null)
                {
                    if (previous.PayloadHash != hash)
                        throw new StudioException(
                            409,
                            "This submission key was already used for different selections."
                        );
                    return previous;
                }
                foreach (var line in input.Lines)
                {
                    await PhotoAccess.Require(tx, line.PhotoId, client, clock.UtcNow);
                    Rules.Require(line.Quantity > 0, "Quantity must be positive.", "quantity");
                    var option = await tx.Get<PrintOption>(line.OptionId);
                    if (option == null || !option.Enabled || option.Version != line.OptionRevision)
                        throw new StudioException(
                            409,
                            "Print pricing changed. Review the refreshed summary before submitting."
                        );
                    line.Name = option.Name;
                    line.Dimensions = option.Dimensions;
                    line.Finish = option.Finish;
                    line.UnitPrice = option.UnitPrice;
                    line.Amount = Rules.Round(checked(option.UnitPrice * line.Quantity));
                }
                input.Id = Guid.NewGuid();
                input.ClientId = client;
                input.PayloadHash = hash;
                input.Total = input.Lines.Sum(x => x.Amount);
                input.State = "Submitted";
                input.ReviewedAt = null;
                input.ReviewedBy = null;
                await tx.Save(input, 0);
                return input;
            }
        );

    public Task<PrintRequest> Review(Guid id, Guid admin, long expected) =>
        store.Run(
            "media",
            async tx =>
            {
                var request =
                    await tx.Get<PrintRequest>(id)
                    ?? throw new StudioException(404, "Request not found.");
                request.State = "Reviewed";
                request.ReviewedBy = admin;
                request.ReviewedAt = clock.UtcNow;
                await tx.Save(request, expected);
                return request;
            }
        );
}
