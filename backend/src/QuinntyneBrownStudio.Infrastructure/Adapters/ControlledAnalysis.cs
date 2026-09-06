using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Enums;
using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Infrastructure.Adapters;

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
