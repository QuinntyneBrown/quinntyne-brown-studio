using QuinntyneBrownStudio.Application.Ports;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class QuoteClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.Parse("2027-03-03T17:00:00Z");
}
