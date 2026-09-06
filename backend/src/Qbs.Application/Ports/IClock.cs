using Qbs.Domain;

namespace Qbs.Application;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
