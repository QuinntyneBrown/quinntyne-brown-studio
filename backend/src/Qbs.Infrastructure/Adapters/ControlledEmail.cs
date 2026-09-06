using System.Collections.Concurrent;
using Qbs.Application;

namespace Qbs.Infrastructure;

public sealed class ControlledEmail : IEmailSender
{
    public ConcurrentDictionary<string, string> Messages { get; } = new();

    public Task Send(string recipient, string subject, string body, string id, CancellationToken ct)
    {
        Messages.TryAdd(id, body);
        return Task.CompletedTask;
    }
}
