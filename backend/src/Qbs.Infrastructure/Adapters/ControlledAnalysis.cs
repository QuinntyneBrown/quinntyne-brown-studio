using Qbs.Application.Ports;
using Qbs.Domain.Enums;
using Qbs.Domain.Models;

namespace Qbs.Infrastructure.Adapters;

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
                    new("exposure", FindingOutcome.Uncertain, "Controlled example; review manually."),
                    new("closed-eyes", FindingOutcome.NotApplicable, "Controlled example; review manually."),
                ],
                "Review manually.",
                "controlled-example",
                "rubric-v1"
            )
        );
}
