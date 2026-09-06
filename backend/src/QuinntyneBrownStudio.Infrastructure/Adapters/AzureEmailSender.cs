using Azure;
using Azure.Communication.Email;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Infrastructure.Adapters;

public sealed class AzureEmailSender(IConfiguration config) : IEmailSender
{
    public async Task Send(
        string recipient,
        string subject,
        string body,
        string id,
        CancellationToken ct
    )
    {
        var client = new EmailClient(
            new Uri(
                config["Azure:EmailEndpoint"]
                    ?? throw new StudioException(503, "Email is not configured.")
            ),
            new DefaultAzureCredential()
        );
        var message = new EmailMessage(
            config["Azure:EmailSender"]
                ?? throw new StudioException(503, "Email sender is not configured."),
            recipient,
            new EmailContent(subject) { PlainText = body }
        );
        await client.SendAsync(WaitUntil.Completed, message, Guid.Parse(id), ct);
    }
}
