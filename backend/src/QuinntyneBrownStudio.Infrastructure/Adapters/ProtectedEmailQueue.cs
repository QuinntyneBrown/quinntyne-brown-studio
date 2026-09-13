using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Infrastructure.Adapters;

/// <summary>
/// Saves an Email background job whose payload is protected the same way account emails are,
/// for the worker's JobProcessor to deliver through IEmailSender.
/// </summary>
public sealed class ProtectedEmailQueue(IDataProtectionProvider protection, IClock clock) : IEmailQueue
{
    public Task Queue(
        IStudioTransaction tx,
        string recipient,
        string subject,
        string body,
        Guid resourceId,
        CancellationToken ct
    )
    {
        var payload = JsonSerializer.Serialize(new { recipient, subject, body });
        var job = new BackgroundJob
        {
            Kind = "Email",
            ResourceId = resourceId,
            AvailableAt = clock.UtcNow,
            Payload = protection.CreateProtector("qbs-email-v1").Protect(payload),
        };
        return tx.Save(job, 0);
    }
}
