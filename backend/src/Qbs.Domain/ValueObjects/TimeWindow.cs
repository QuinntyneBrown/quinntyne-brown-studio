namespace Qbs.Domain.ValueObjects;

public sealed class TimeWindow
{
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
}
