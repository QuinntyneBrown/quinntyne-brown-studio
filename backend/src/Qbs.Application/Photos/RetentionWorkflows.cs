using Qbs.Domain;

namespace Qbs.Application;

public sealed class RetentionWorkflows(IStudioStore store, IClock clock, PhotoWorkflows photos)
{
    public Task<object> Get(Guid id) =>
        store.Run<object>(
            "media",
            async tx =>
            {
                var s =
                    await tx.Get<Session>(id)
                    ?? throw new StudioException(404, "Session not found.");
                return await Impact(tx, s);
            }
        );

    public Task<Session> Extend(Guid id, int months, DateTimeOffset? expires, long expected) =>
        store.Run(
            "media",
            async tx =>
            {
                Rules.Require(
                    months > 0 && months <= 120,
                    "Retention must be between 1 and 120 months."
                );
                var s =
                    await tx.Get<Session>(id)
                    ?? throw new StudioException(404, "Session not found.");
                Rules.Require(
                    s.RetentionState is not "Deleted" and not "DeletionPending",
                    "Deleted photos cannot be restored."
                );
                s.RetentionMonths = months;
                if (expires != null)
                {
                    Rules.Require(
                        expires > s.ExpiresAt && expires > clock.UtcNow,
                        "An extension must move expiry later."
                    );
                    s.ExpiresAt = expires;
                    s.ExpiryRevision++;
                }
                s.RetentionState = "Active";
                await tx.Save(s, expected);
                return s;
            }
        );

    public Task<object> Delete(Guid id, string revision, bool confirm) =>
        store.Run<object>(
            "media",
            async tx =>
            {
                Rules.Require(confirm, "Confirm the reference impact before deletion.");
                var s =
                    await tx.Get<Session>(id)
                    ?? throw new StudioException(404, "Session not found.");
                var impact = await Impact(tx, s);
                if (impact.ImpactRevision != revision)
                    throw new StudioException(
                        409,
                        "References changed. Review the current impact report."
                    );
                if (impact.PublishedReferences > 0 || impact.UnreviewedRequests > 0)
                    throw new StudioException(
                        409,
                        "Unpublish photos and review outstanding print requests before deletion."
                    );
                Rules.Require(s.RetentionState != "Deleted", "Photos are already deleted.");
                s.RetentionState = "DeletionPending";
                await tx.Save(s, s.Version);
                foreach (var p in (await tx.List<SessionPhoto>()).Where(x => x.SessionId == id))
                {
                    p.State = PhotoState.DeletionPending;
                    await tx.Save(p, p.Version);
                }
                var job = await photos.Queue(tx, "Delete", id, s.Version);
                return new { jobId = job.Id };
            }
        );

    private static async Task<RetentionImpact> Impact(IStudioTransaction tx, Session s)
    {
        var ids = (await tx.List<SessionPhoto>())
            .Where(x => x.SessionId == s.Id && x.State != PhotoState.Deleted)
            .Select(x => x.Id)
            .ToHashSet();
        var galleries = (await tx.List<PublicGallery>())
            .Where(x => x.Published && x.PhotoIds.Any(ids.Contains))
            .ToArray();
        var requests = (await tx.List<PrintRequest>())
            .Where(x => x.State == "Submitted" && x.Lines.Any(l => ids.Contains(l.PhotoId)))
            .ToArray();
        var text = string.Join(
            "|",
            new[] { s.Version.ToString() }
                .Concat(ids.Order().Select(x => x.ToString()))
                .Concat(galleries.OrderBy(x => x.Id).Select(x => $"{x.Id}:{x.Version}"))
                .Concat(requests.OrderBy(x => x.Id).Select(x => $"{x.Id}:{x.Version}"))
        );
        var hash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text))
        );
        return new(
            s.Id,
            s.RetentionMonths,
            s.ExpiresAt,
            s.Version,
            s.RetentionState,
            hash,
            ids.Count,
            galleries.Length,
            requests.Length
        );
    }
}
