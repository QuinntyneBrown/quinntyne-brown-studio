namespace Qbs.Domain;

public sealed record AvailabilityResult(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    bool Available,
    Guid[] PhotographerIds,
    string? ReasonCode
);
