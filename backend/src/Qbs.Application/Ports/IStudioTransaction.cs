using Qbs.Domain;

namespace Qbs.Application;

public interface IStudioTransaction
{
    Task<T?> Get<T>(Guid id)
        where T : Entity;
    Task<T[]> List<T>()
        where T : Entity;
    Task Save<T>(T value, long expectedVersion)
        where T : Entity;
}
