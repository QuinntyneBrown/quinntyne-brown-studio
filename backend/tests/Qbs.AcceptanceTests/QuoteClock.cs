using Qbs.Application.Ports;

namespace Qbs.AcceptanceTests;

public sealed class QuoteClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.Parse("2027-03-03T17:00:00Z");
}
