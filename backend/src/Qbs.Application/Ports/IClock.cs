namespace Qbs.Application.Ports;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
