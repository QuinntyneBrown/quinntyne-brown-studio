namespace Qbs.Domain.Models;

public sealed record PhotoAnalysis(
    Guid PhotoId,
    PhotoFinding[] Findings,
    string Recommendation,
    string ModelVersion,
    string PromptVersion
);
