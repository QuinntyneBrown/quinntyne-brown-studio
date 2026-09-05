using Qbs.Domain;

namespace Qbs.Application;

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
