namespace Qbs.Application.Ports;

public interface IEmailSender
{
    Task Send(
        string recipient,
        string subject,
        string body,
        string deduplicationId,
        CancellationToken ct
    );
}
