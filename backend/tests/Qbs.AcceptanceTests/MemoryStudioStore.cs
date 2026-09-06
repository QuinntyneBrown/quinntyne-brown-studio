using System.Text.Json;
using Qbs.Application.Ports;

namespace Qbs.AcceptanceTests;

public sealed class MemoryStudioStore : IStudioStore
{
    private readonly SemaphoreSlim mutex = new(1, 1);
    private Dictionary<(Type, Guid), string> data = [];

    public async Task<T> Run<T>(
        string key,
        Func<IStudioTransaction, Task<T>> action,
        CancellationToken ct = default
    )
    {
        await mutex.WaitAsync(ct);
        try
        {
            var tx = new MemoryStudioTransaction(new(data));
            var result = await action(tx);
            data = tx.Data;
            return result;
        }
        finally
        {
            mutex.Release();
        }
    }
}
