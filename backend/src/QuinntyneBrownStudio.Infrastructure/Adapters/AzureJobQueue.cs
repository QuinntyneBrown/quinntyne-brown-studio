using Azure.Identity;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Infrastructure.Adapters;

public sealed class AzureJobQueue(IConfiguration config) : IJobQueue
{
    private QueueClient Client =>
        config["Azure:StorageConnectionString"] is { } connection
            ? new(connection, "processing")
            : new(
                new Uri(
                    config["Azure:QueueEndpoint"]
                        ?? throw new StudioException(503, "Processing queue is not configured.")
                ),
                new DefaultAzureCredential()
            );

    public async Task Send(Guid id, CancellationToken ct) =>
        await Client.SendMessageAsync(id.ToString(), ct);

    public async Task<(Guid Id, string Receipt)?> Receive(CancellationToken ct)
    {
        var response = await Client.ReceiveMessageAsync(TimeSpan.FromMinutes(5), ct);
        var m = response.Value;
        return m == null ? null : (Guid.Parse(m.MessageText), m.MessageId + ":" + m.PopReceipt);
    }

    public async Task Complete(string receipt, CancellationToken ct)
    {
        var parts = receipt.Split(':', 2);
        await Client.DeleteMessageAsync(parts[0], parts[1], ct);
    }
}
