using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Enums;
using QuinntyneBrownStudio.Domain.Exceptions;
using QuinntyneBrownStudio.Domain.Policies;
using QuinntyneBrownStudio.Infrastructure.Adapters;
using QuinntyneBrownStudio.Infrastructure.Serialization;
using QuinntyneBrownStudio.Infrastructure.Storage;

namespace QuinntyneBrownStudio.Infrastructure.Processing;

public sealed class JobProcessor(
    IStudioStore store,
    IJobQueue queue,
    IPhotoStorage storage,
    IRawPreviewConverter converter,
    IPhotoAnalysisService analysis,
    IEmailSender email,
    IClock clock,
    IDataProtectionProvider protection,
    IConfiguration config
)
{
    public async Task Cycle(CancellationToken ct)
    {
        var ready = await store.Run(
            "outbox",
            async tx =>
            {
                var jobs = (await tx.List<BackgroundJob>())
                    .Where(x =>
                        (x.State == "Queued" || x.State == "Running" && x.LeaseUntil < clock.UtcNow)
                        && x.AvailableAt <= clock.UtcNow
                        && (!x.Relayed || x.LeaseUntil < clock.UtcNow)
                    )
                    .Take(25)
                    .ToArray();
                return jobs.Select(x => x.Id).ToArray();
            },
            ct
        );
        foreach (var id in ready)
        {
            await queue.Send(id, ct);
            await store.Run(
                "job:" + id,
                async tx =>
                {
                    var job = await tx.Get<BackgroundJob>(id);
                    if (
                        job != null
                        && (
                            job.State == "Queued"
                            || job.State == "Running" && job.LeaseUntil < clock.UtcNow
                        )
                    )
                    {
                        // Requeue expired execution before assigning a relay deadline. An
                        // active execution lease belongs to its worker and is left alone.
                        job.State = "Queued";
                        job.Relayed = true;
                        job.LeaseUntil = clock.UtcNow.AddMinutes(5);
                        await tx.Save(job, job.Version);
                    }
                    return true;
                },
                ct
            );
        }
        var message = await queue.Receive(ct);
        if (message != null)
        {
            await Process(message.Value.Id, ct);
            await queue.Complete(message.Value.Receipt, ct);
        }
    }

    public async Task Process(Guid id, CancellationToken ct)
    {
        var job = await store.Run(
            "job:" + id,
            async tx =>
            {
                var j = await tx.Get<BackgroundJob>(id);
                if (
                    j == null
                    || j.State is "Succeeded" or "Failed"
                    || j.State == "Running" && j.LeaseUntil > clock.UtcNow
                    || j.AvailableAt > clock.UtcNow
                )
                    return null;
                j.State = "Running";
                j.Attempt++;
                j.LeaseUntil = clock.UtcNow.AddMinutes(5);
                await tx.Save(j, j.Version);
                return j;
            },
            ct
        );
        if (job == null)
            return;
        try
        {
            string? result = null;
            if (job.Kind is "Preview" or "Analysis")
            {
                var p =
                    await store.Run(
                        "photo:" + job.ResourceId,
                        tx => tx.Get<SessionPhoto>(job.ResourceId),
                        ct
                    ) ?? throw new StudioException(404, "Photo no longer exists.");
                if (p.State is PhotoState.Deleted or PhotoState.DeletionPending)
                    throw new StudioException(409, "Photo is being deleted.");
                if (job.Kind == "Preview")
                {
                    await using var original = await storage.Read(p.OriginalKey, ct);
                    await PhotoIntegrity.Validate(original, p, ct);
                    await using var preview = await converter.Convert(original, p.Name, ct);
                    var key = $"previews/{p.Id}/{job.Id}-{job.Attempt}.jpg";
                    await storage.Write(key, preview, ct);
                    preview.Position = 0;
                    await using var thumbnail = Thumbnail.Create(preview);
                    var thumbnailKey = $"thumbnails/{p.Id}/{job.Id}-{job.Attempt}.jpg";
                    await storage.Write(thumbnailKey, thumbnail, ct);
                    var linked = await store.Run(
                        "media",
                        async tx =>
                        {
                            var current = await tx.Get<SessionPhoto>(p.Id);
                            if (
                                current == null
                                || current.Version != job.ExpectedRevision
                                || current.State != PhotoState.Processing
                            )
                                return false;
                            current.PreviewKey = key;
                            current.ThumbnailKey = thumbnailKey;
                            current.State = PhotoState.Ready;
                            current.Failure = null;
                            await tx.Save(current, current.Version);
                            return true;
                        },
                        ct
                    );
                    if (!linked)
                    {
                        await storage.Delete(key, ct);
                        await storage.Delete(thumbnailKey, ct);
                    }
                }
                else
                {
                    if (p.State != PhotoState.Ready || p.PreviewKey == null)
                        throw new StudioException(409, "Photo preview is not ready.");
                    await using var preview = await storage.Read(p.PreviewKey, ct);
                    result = JsonSerializer.Serialize(
                        PhotoAnalysisPolicy.Validate(p.Id, await analysis.Analyze(p.Id, preview, ct)),
                        StudioJson.Options
                    );
                }
            }
            else if (job.Kind == "Delete")
            {
                var photos = await store.Run(
                    "media",
                    async tx =>
                        (await tx.List<SessionPhoto>())
                            .Where(x => x.SessionId == job.ResourceId)
                            .ToArray(),
                    ct
                );
                foreach (var p in photos)
                {
                    if (p.State == PhotoState.Deleted)
                        continue;
                    await storage.Delete(p.OriginalKey, ct);
                    if (p.PreviewKey != null)
                        await storage.Delete(p.PreviewKey, ct);
                    if (p.ThumbnailKey != null)
                        await storage.Delete(p.ThumbnailKey, ct);
                    await storage.Delete(p.StagingKey, ct);
                }
                await store.Run(
                    "media",
                    async tx =>
                    {
                        foreach (var p in photos)
                        {
                            var current = await tx.Get<SessionPhoto>(p.Id);
                            if (current == null)
                                continue;
                            current.State = PhotoState.Deleted;
                            current.PreviewKey = null;
                            current.ThumbnailKey = null;
                            await tx.Save(current, current.Version);
                        }
                        var session = await tx.Get<Session>(job.ResourceId);
                        if (session != null)
                        {
                            session.RetentionState = "Deleted";
                            await tx.Save(session, session.Version);
                        }
                        return true;
                    },
                    ct
                );
            }
            else if (job.Kind == "Email")
            {
                var payload = JsonSerializer.Deserialize<JsonElement>(
                    protection.CreateProtector("qbs-email-v1").Unprotect(job.Payload)
                );
                await email.Send(
                    payload.GetProperty("recipient").GetString()!,
                    payload.GetProperty("subject").GetString()!,
                    payload.GetProperty("body").GetString()!,
                    job.Id.ToString(),
                    ct
                );
            }
            else
                throw new StudioException(400, "Unknown job kind.");
            await Finish(id, job.Attempt, "Succeeded", null, result, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await Finish(
                id,
                job.Attempt,
                job.Attempt >= 5 ? "Failed" : "Queued",
                ex is StudioException
                    ? ex.Message
                    : "Processing failed. Retry is available after the final attempt.",
                null,
                ct
            );
        }
    }

    private Task<bool> Finish(
        Guid id,
        int attempt,
        string state,
        string? error,
        string? result,
        CancellationToken ct
    ) =>
        store.Run(
            "job:" + id,
            async tx =>
            {
                var job =
                    await tx.Get<BackgroundJob>(id)
                    ?? throw new StudioException(404, "Job not found.");
                if (job.Attempt != attempt || job.State != "Running")
                    return false;
                job.State = state;
                job.Error = error;
                job.Result = result;
                job.Relayed = false;
                job.LeaseUntil = null;
                job.AvailableAt = clock.UtcNow.AddSeconds(Math.Pow(2, job.Attempt));
                await tx.Save(job, job.Version);
                if (state == "Failed" && job.Kind == "Preview")
                {
                    var p = await tx.Get<SessionPhoto>(job.ResourceId);
                    if (p?.State == PhotoState.Processing)
                    {
                        p.State = PhotoState.Failed;
                        p.Failure = error;
                        await tx.Save(p, p.Version);
                    }
                }
                return true;
            },
            ct
        );

    public Task<bool> Retention(CancellationToken ct) =>
        store.Run(
            "retention-notices",
            async tx =>
            {
                foreach (var s in await tx.List<Session>())
                {
                    if (s.ExpiresAt == null || s.RetentionState is "Deleted" or "DeletionPending")
                        continue;
                    var changed = false;
                    if (s.ExpiresAt <= clock.UtcNow && s.RetentionState != "Expired")
                    {
                        s.RetentionState = "Expired";
                        changed = true;
                    }
                    if (
                        s.ExpiresAt <= clock.UtcNow.AddDays(30)
                        && s.NoticeRevision != s.ExpiryRevision
                    )
                    {
                        var recipient = config["Retention:AdministratorEmail"];
                        if (!string.IsNullOrWhiteSpace(recipient))
                        {
                            var payload = JsonSerializer.Serialize(
                                new
                                {
                                    recipient,
                                    subject = "Session retention notice",
                                    body = $"Session {s.Id} expires at {s.ExpiresAt:O}. Review retention in studio administration.",
                                }
                            );
                            var job = new BackgroundJob
                            {
                                Kind = "Email",
                                ResourceId = s.Id,
                                ExpectedRevision = s.ExpiryRevision,
                                AvailableAt = clock.UtcNow,
                                Payload = protection
                                    .CreateProtector("qbs-email-v1")
                                    .Protect(payload),
                            };
                            await tx.Save(job, 0);
                            s.NoticeRevision = s.ExpiryRevision;
                            changed = true;
                        }
                    }
                    if (changed)
                        await tx.Save(s, s.Version);
                }
                return true;
            },
            ct
        );
}
