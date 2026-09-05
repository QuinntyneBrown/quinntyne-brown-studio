using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Qbs.Application;
using Qbs.Domain;
using Qbs.Infrastructure;

namespace Qbs.AcceptanceTests;

public sealed class WorkerRecoveryTests
{
    [Fact]
    public async Task Expired_execution_lease_is_reprocessed_after_a_worker_crash()
    {
        using var factory = new StudioFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IStudioStore>();
        var protector = scope
            .ServiceProvider.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("qbs-email-v1");
        var job = new BackgroundJob
        {
            Kind = "Email",
            State = "Running",
            Attempt = 1,
            Relayed = true,
            LeaseUntil = DateTimeOffset.UtcNow.AddMinutes(-1),
            AvailableAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            Payload = protector.Protect(
                JsonSerializer.Serialize(
                    new
                    {
                        recipient = "recovery@example.test",
                        subject = "Retry",
                        body = "Captured local message",
                    }
                )
            ),
        };
        await store.Run(
            "fixture",
            async tx =>
            {
                await tx.Save(job, 0);
                return true;
            }
        );
        await scope
            .ServiceProvider.GetRequiredService<JobProcessor>()
            .Cycle(CancellationToken.None);
        var saved = (await store.Run("fixture", tx => tx.Get<BackgroundJob>(job.Id)))!;
        Assert.Equal("Succeeded", saved.State);
        Assert.Equal(2, saved.Attempt);
        Assert.Single(
            ((ControlledEmail)scope.ServiceProvider.GetRequiredService<IEmailSender>()).Messages
        );
    }
}
