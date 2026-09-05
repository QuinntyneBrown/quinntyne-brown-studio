using System.Text.Json;
using Qbs.Domain;

namespace Qbs.Application;

public sealed class AnalysisWorkflows(IStudioStore store, PhotoWorkflows photos)
{
    public Task<object> Request(Guid sessionId, Guid[] ids) =>
        store.Run<object>(
            "analysis:" + sessionId,
            async tx =>
            {
                Rules.Require(ids.Length > 0 && ids.Length <= 1000, "Select ready photos.");
                var jobs = new List<Guid>();
                foreach (var id in ids.Distinct())
                {
                    var photo = await tx.Get<SessionPhoto>(id);
                    Rules.Require(
                        photo?.SessionId == sessionId && photo.State == PhotoState.Ready,
                        "Select ready photos from this session."
                    );
                    jobs.Add((await photos.Queue(tx, "Analysis", id, photo!.Version)).Id);
                }
                var batch = new AnalysisBatch { SessionId = sessionId, JobIds = jobs.ToArray() };
                await tx.Save(batch, 0);
                return new { id = batch.Id };
            }
        );

    public Task<object> Status(Guid id) =>
        store.Run<object>(
            "analysis:" + id,
            async tx =>
            {
                var batch =
                    await tx.Get<AnalysisBatch>(id)
                    ?? throw new StudioException(404, "Analysis not found.");
                var jobs = new List<object>();
                foreach (var jobId in batch.JobIds)
                {
                    var job = await tx.Get<BackgroundJob>(jobId);
                    if (job != null)
                        jobs.Add(
                            new
                            {
                                id = job.Id,
                                photoId = job.ResourceId,
                                job.State,
                                job.Error,
                                result = job.Result == null
                                    ? (JsonElement?)null
                                    : JsonSerializer.Deserialize<JsonElement>(job.Result),
                            }
                        );
                }
                return new
                {
                    batch.Id,
                    batch.SessionId,
                    photos = jobs,
                };
            }
        );

    public async Task<object> Retry(Guid id, Guid[] failed)
    {
        var session = await store.Run(
            "analysis:" + id,
            async tx =>
            {
                var batch =
                    await tx.Get<AnalysisBatch>(id)
                    ?? throw new StudioException(404, "Analysis not found.");
                var jobs = new List<BackgroundJob>();
                foreach (var jobId in batch.JobIds)
                {
                    var job = await tx.Get<BackgroundJob>(jobId);
                    if (job != null)
                        jobs.Add(job);
                }
                Rules.Require(
                    failed.All(x => jobs.Any(j => j.ResourceId == x && j.State == "Failed")),
                    "Only failed photos in this batch may be retried."
                );
                return batch.SessionId;
            }
        );
        return await Request(session, failed);
    }
}
