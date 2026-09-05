using Qbs.Domain;

namespace Qbs.Application;

public sealed class PhotoWorkflows(IStudioStore store, IPhotoStorage storage, IClock clock)
{
    private static readonly HashSet<string> Extensions =
    [
        ".jpg",
        ".jpeg",
        ".cr2",
        ".cr3",
        ".nef",
        ".arw",
        ".dng",
    ];

    public Task<object> Create(Guid sessionId, UploadEntry[] files, CancellationToken ct) =>
        store.Run<object>(
            "media",
            async tx =>
            {
                Rules.Require(
                    files.Length > 0 && files.Length <= 1000,
                    "Select between 1 and 1,000 files.",
                    "files"
                );
                Rules.Require(
                    files.Select(x => x.ClientFileId).Distinct().Count() == files.Length,
                    "File identifiers must be unique."
                );
                var session =
                    await tx.Get<Session>(sessionId)
                    ?? throw new StudioException(404, "Session not found.");
                Rules.Require(
                    session.RetentionState is not "Deleted" and not "DeletionPending",
                    "Cannot upload to a deleted session."
                );
                var batch = new UploadBatch { SessionId = sessionId, Files = files };
                var grants = new Dictionary<Guid, StorageGrant>();
                foreach (var file in files)
                {
                    file.PhotoId = null;
                    file.Rejection = null;
                    file.Name = Path.GetFileName(file.Name);
                    var ext = Path.GetExtension(file.Name).ToLowerInvariant();
                    if (
                        !Extensions.Contains(ext)
                        || file.Size <= 0
                        || file.Size > 250000000
                        || !System.Text.RegularExpressions.Regex.IsMatch(
                            file.Sha256,
                            "^[a-fA-F0-9]{64}$"
                        )
                    )
                    {
                        file.Rejection =
                            "Unsupported format, invalid digest, or file exceeds 250 MB.";
                        continue;
                    }
                    var photo = new SessionPhoto
                    {
                        SessionId = sessionId,
                        BatchId = batch.Id,
                        Name = file.Name,
                        Sha256 = file.Sha256.ToUpperInvariant(),
                        Size = file.Size,
                        State = PhotoState.Uploading,
                    };
                    photo.StagingKey = $"staging/{photo.Id}";
                    photo.OriginalKey = $"originals/{photo.Id}";
                    file.PhotoId = photo.Id;
                    await tx.Save(photo, 0);
                    grants[photo.Id] = await storage.Grant(photo.StagingKey, ct);
                }
                await tx.Save(batch, 0);
                return new
                {
                    batch.Id,
                    batch.SessionId,
                    batch.Files,
                    grants,
                };
            },
            ct
        );

    public Task<object> Status(Guid id) =>
        store.Run<object>(
            "uploads:" + id,
            async tx =>
            {
                var batch =
                    await tx.Get<UploadBatch>(id)
                    ?? throw new StudioException(404, "Batch not found.");
                var entries = new List<object>();
                var states = new List<PhotoState>();
                foreach (var file in batch.Files)
                {
                    var photo =
                        file.PhotoId == null
                            ? null
                            : await tx.Get<SessionPhoto>(file.PhotoId.Value);
                    if (photo != null)
                        states.Add(photo.State);
                    entries.Add(
                        new
                        {
                            file.ClientFileId,
                            file.Name,
                            file.Size,
                            file.Sha256,
                            file.PhotoId,
                            file.Rejection,
                            state = photo?.State.ToString() ?? "Rejected",
                            failure = photo?.Failure,
                        }
                    );
                }
                var failed =
                    batch.Files.Any(x => x.Rejection != null)
                    || states.Any(x => x is PhotoState.Failed or PhotoState.Rejected);
                var state =
                    states.Any(x => x == PhotoState.Uploading) ? "Uploading"
                    : states.Any(x => x == PhotoState.Processing) ? "Processing"
                    : failed ? "PartialFailure"
                    : "Complete";
                return new
                {
                    batch.Id,
                    batch.SessionId,
                    state,
                    files = entries,
                };
            }
        );

    public Task<StorageGrant> Renew(Guid batch, Guid id, CancellationToken ct) =>
        store.Run(
            "uploads:" + batch,
            async tx =>
            {
                var p = await tx.Get<SessionPhoto>(id);
                if (p == null || p.BatchId != batch)
                    throw new StudioException(404, "Photo not found.");
                Rules.Require(
                    p.State == PhotoState.Uploading,
                    "This upload has already been finalized."
                );
                return await storage.Grant(p.StagingKey, ct);
            },
            ct
        );

    public Task<object> Finalize(Guid batch, Guid id, CancellationToken ct) =>
        store.Run<object>(
            "media",
            async tx =>
            {
                var p = await tx.Get<SessionPhoto>(id);
                if (p == null || p.BatchId != batch)
                    throw new StudioException(404, "Photo not found.");
                if (p.State != PhotoState.Uploading)
                    return new { id = p.Id, state = p.State.ToString() };
                var s =
                    await tx.Get<Session>(p.SessionId)
                    ?? throw new StudioException(404, "Session not found.");
                Rules.Require(
                    s.RetentionState is not "Deleted" and not "DeletionPending",
                    "Session is being deleted."
                );
                try
                {
                    await storage.Finalize(p, ct);
                }
                catch (StudioException ex) when (ex.Status == 400)
                {
                    p.State = PhotoState.Rejected;
                    p.Failure = ex.Message;
                    await tx.Save(p, p.Version);
                    return new
                    {
                        id = p.Id,
                        state = "Rejected",
                        failure = p.Failure,
                    };
                }
                p.State = PhotoState.Processing;
                p.UploadedAt = clock.UtcNow;
                await tx.Save(p, p.Version);
                await Queue(tx, "Preview", p.Id, p.Version);
                var local = Rules.Local(p.UploadedAt);
                var localExpiry = local.DateTime.AddMonths(s.RetentionMonths);
                var expiry = new DateTimeOffset(
                    localExpiry,
                    Rules.Toronto.GetUtcOffset(localExpiry)
                );
                if (s.ExpiresAt == null || expiry > s.ExpiresAt)
                {
                    s.ExpiresAt = expiry;
                    s.ExpiryRevision++;
                    await tx.Save(s, s.Version);
                }
                return new { id = p.Id, state = "Processing" };
            },
            ct
        );

    public Task<object> Retry(Guid id) =>
        store.Run<object>(
            "media",
            async tx =>
            {
                var p =
                    await tx.Get<SessionPhoto>(id)
                    ?? throw new StudioException(404, "Photo not found.");
                Rules.Require(p.State == PhotoState.Failed, "Only failed previews can be retried.");
                p.State = PhotoState.Processing;
                p.Failure = null;
                await tx.Save(p, p.Version);
                var job = await Queue(tx, "Preview", p.Id, p.Version);
                return new { jobId = job.Id };
            }
        );

    public Task<object> Photos(Guid sessionId, string? cursor) =>
        store.Run<object>(
            "photos:" + sessionId,
            async tx =>
            {
                if (await tx.Get<Session>(sessionId) == null)
                    throw new StudioException(404, "Session not found.");
                var list = (await tx.List<SessionPhoto>())
                    .Where(x => x.SessionId == sessionId)
                    .OrderBy(x => x.Id)
                    .ToArray();
                var offset = 0;
                if (cursor != null)
                {
                    Rules.Require(Guid.TryParse(cursor, out var after), "Invalid cursor.");
                    offset = Array.FindIndex(list, x => x.Id == after) + 1;
                    Rules.Require(offset > 0, "Invalid cursor.");
                }
                var page = list.Skip(offset).Take(50).ToArray();
                return new
                {
                    photos = page.Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.State,
                        x.Failure,
                        thumbnailUrl = x.State == PhotoState.Ready
                            ? $"/api/admin/photos/{x.Id}/preview?thumbnail=true"
                            : null,
                        url = x.State == PhotoState.Ready
                            ? $"/api/admin/photos/{x.Id}/preview"
                            : null,
                    }),
                    nextCursor = offset + page.Length < list.Length
                        ? page.Last().Id.ToString()
                        : null,
                };
            }
        );

    public async Task<MediaFile> Preview(
        Guid id,
        Guid? client,
        string? slug,
        CancellationToken ct,
        bool thumbnail = false
    )
    {
        var key = await store.Run(
            "media",
            async tx =>
            {
                SessionPhoto photo;
                if (client != null)
                    photo = await PhotoAccess.Require(tx, id, client.Value, clock.UtcNow);
                else
                {
                    photo =
                        await tx.Get<SessionPhoto>(id)
                        ?? throw new StudioException(404, "Photo not found.");
                    if (photo.State != PhotoState.Ready)
                        throw new StudioException(404, "Preview not available.");
                }
                if (
                    slug != null
                    && !(await tx.List<PublicGallery>()).Any(x =>
                        x.Slug == slug && x.Published && x.PhotoIds.Contains(id)
                    )
                )
                    throw new StudioException(404, "Photo not found.");
                return (thumbnail ? photo.ThumbnailKey ?? photo.PreviewKey : photo.PreviewKey)
                    ?? throw new StudioException(404, "Preview not available.");
            },
            ct
        );
        return new(await storage.Read(key, ct), "image/jpeg");
    }

    public async Task<BackgroundJob> Queue(
        IStudioTransaction tx,
        string kind,
        Guid id,
        long revision
    )
    {
        var job = new BackgroundJob
        {
            Kind = kind,
            ResourceId = id,
            ExpectedRevision = revision,
            AvailableAt = clock.UtcNow,
        };
        await tx.Save(job, 0);
        return job;
    }
}
