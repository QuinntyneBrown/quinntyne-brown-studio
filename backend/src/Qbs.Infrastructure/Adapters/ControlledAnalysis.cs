using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Infrastructure;

public sealed class ControlledAnalysis : IPhotoAnalysisService
{
    public Task<PhotoAnalysis> Analyze(Guid id, Stream preview, CancellationToken ct) =>
        Task.FromResult(
            new PhotoAnalysis(
                id,
                [
                    new(
                        "sharpness",
                        FindingOutcome.Uncertain,
                        "Controlled example; photographic quality is not evaluated."
                    ),
                ],
                "Review manually.",
                "controlled-example",
                "rubric-v1"
            )
        );
}
