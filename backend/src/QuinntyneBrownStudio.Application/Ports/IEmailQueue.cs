namespace QuinntyneBrownStudio.Application.Ports;

/// <summary>
/// Queues one plain-text email as a durable background job inside the caller's unit of work,
/// so the record that caused the email commits before any delivery is attempted.
/// </summary>
public interface IEmailQueue
{
    Task Queue(
        IStudioTransaction tx,
        string recipient,
        string subject,
        string body,
        Guid resourceId,
        CancellationToken ct
    );
}
