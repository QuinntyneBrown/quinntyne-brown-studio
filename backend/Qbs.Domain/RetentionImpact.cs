namespace Qbs.Domain;

public sealed record RetentionImpact(
    Guid Id,
    int Months,
    DateTimeOffset? ExpiresAt,
    long Version,
    string State,
    string ImpactRevision,
    int PhotoCount,
    int PublishedReferences,
    int UnreviewedRequests
);
