using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Enums;
using QuinntyneBrownStudio.Domain.Exceptions;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.Policies;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed class ClientWorkflows(IStudioStore store, IClock clock)
{
    public Task<PrintPreview> Preview(Guid client, PrintPreviewInput input) =>
        store.Run("media", async tx =>
        {
            Rules.Require(input.InputRevision >= 0, "Invalid input revision.", "inputRevision");
            var lines = await PriceLines(tx, client, input.Lines, false);
            return new PrintPreview(input.InputRevision, lines, lines.Sum(x => x.Amount));
        });

    private async Task<PrintLine[]> PriceLines(IStudioTransaction tx, Guid client, PrintLine[] selections, bool requireRevision)
    {
        Rules.Require(selections is { Length: > 0 and <= 1000 }, "Select between 1 and 1,000 print lines.", "lines");
        var lines = new List<PrintLine>();
        foreach (var selection in selections)
        {
            if (selection == null) throw new StudioException(400, "Select a print option and photo.");
            await PhotoAccess.Require(tx, selection.PhotoId, client, clock.UtcNow);
            Rules.Require(selection.Quantity > 0, "Quantity must be positive.", "quantity");
            var option = await tx.Get<PrintOption>(selection.OptionId);
            if (option == null || !option.Enabled || (requireRevision && option.Version != selection.OptionRevision))
                throw new StudioException(409, "Print pricing changed. Review the refreshed summary before submitting.");
            lines.Add(new PrintLine
            {
                PhotoId = selection.PhotoId,
                OptionId = option.Id,
                OptionRevision = option.Version,
                Quantity = selection.Quantity,
                Name = option.Name,
                Dimensions = option.Dimensions,
                Finish = option.Finish,
                UnitPrice = option.UnitPrice,
                Amount = Rules.Round(checked(option.UnitPrice * selection.Quantity))
            });
        }
        return lines.ToArray();
    }

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
                input.Lines = await PriceLines(tx, client, input.Lines, true);
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
