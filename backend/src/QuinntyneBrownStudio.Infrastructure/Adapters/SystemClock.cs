using QuinntyneBrownStudio.Application.Ports;

namespace QuinntyneBrownStudio.Infrastructure.Adapters;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
