using System.Collections.Concurrent;
using QuinntyneBrownStudio.Application.Ports;

namespace QuinntyneBrownStudio.Infrastructure.Adapters;

public sealed class ControlledEmail : IEmailSender
{
    public ConcurrentDictionary<string, string> Messages { get; } = new();

    public Task Send(string recipient, string subject, string body, string id, CancellationToken ct)
    {
        Messages.TryAdd(id, body);
        return Task.CompletedTask;
    }
}
