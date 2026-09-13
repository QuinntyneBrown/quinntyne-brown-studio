using QuinntyneBrownStudio.Application.Ports;

namespace QuinntyneBrownStudio.AcceptanceTests;

/// <summary>An email provider that refuses every delivery, so the worker's retry path can be observed.</summary>
public sealed class ThrowingEmailSender : IEmailSender
{
    public int Attempts { get; private set; }

    public Task Send(string recipient, string subject, string body, string deduplicationId, CancellationToken ct)
    {
        Attempts++;
        throw new HttpRequestException("The email provider rejected the delivery.");
    }
}
