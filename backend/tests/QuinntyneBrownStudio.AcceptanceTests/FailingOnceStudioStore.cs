using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.AcceptanceTests;

/// <summary>A store whose first unit of work under one lock key fails as an outage; later work succeeds.</summary>
public sealed class FailingOnceStudioStore(IStudioStore inner, string lockKey) : IStudioStore
{
    private int failures;

    public Task<T> Run<T>(string key, Func<IStudioTransaction, Task<T>> action, CancellationToken ct = default)
    {
        if (key == lockKey && Interlocked.Exchange(ref failures, 1) == 0)
            throw new StudioException(503, "The studio database is temporarily unavailable.");
        return inner.Run(key, action, ct);
    }
}
