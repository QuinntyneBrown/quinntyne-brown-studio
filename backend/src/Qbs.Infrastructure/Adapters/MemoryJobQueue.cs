using System.Collections.Concurrent;
using Qbs.Application.Ports;

namespace Qbs.Infrastructure.Adapters;

public sealed class MemoryJobQueue : IJobQueue
{
    private readonly ConcurrentQueue<Guid> queue = new();

    public Task Send(Guid id, CancellationToken ct)
    {
        queue.Enqueue(id);
        return Task.CompletedTask;
    }

    public Task<(Guid Id, string Receipt)?> Receive(CancellationToken ct) =>
        Task.FromResult<(Guid, string)?>(queue.TryDequeue(out var id) ? (id, id.ToString()) : null);

    public Task Complete(string receipt, CancellationToken ct) => Task.CompletedTask;
}
