using Qbs.Domain;

namespace Qbs.Application;

public interface IStudioStore
{
    Task<T> Run<T>(
        string lockKey,
        Func<IStudioTransaction, Task<T>> action,
        CancellationToken cancellationToken = default
    );
}
