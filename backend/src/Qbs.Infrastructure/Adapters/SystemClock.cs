using Qbs.Application.Ports;

namespace Qbs.Infrastructure.Adapters;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
