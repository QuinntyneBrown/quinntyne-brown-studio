using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuinntyneBrownStudio.Application.Photos;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Infrastructure.Processing;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class RetentionAcceptanceTests
{
    [Fact]
    public async Task AC_L2_061_01_AC_L2_034_01_Expiry_is_enforced_without_scheduler_and_notices_are_once_per_revision()
    {
        using var factory = new StudioFactory();
        var clientId = Guid.NewGuid();
        using var client = await factory.Actor("Client", clientId.ToString());
        await using var scope = factory.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IConfiguration>()["Retention:AdministratorEmail"] =
            "retention@example.test";
        var store = scope.ServiceProvider.GetRequiredService<IStudioStore>();
        var expired = new Session
        {
            Name = "Expired",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            ExpiryRevision = 1,
            ClientIds = [clientId],
        };
        var upcoming = new Session
        {
            Name = "Upcoming",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(20),
            ExpiryRevision = 1,
        };
        await store.Run(
            "fixture",
            async tx =>
            {
                await tx.Save(expired, 0);
                await tx.Save(upcoming, 0);
                return true;
            }
        );
        // The worker has never run, but access is already expired.
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/client/galleries/{expired.Id}")).StatusCode
        );
        var processor = scope.ServiceProvider.GetRequiredService<JobProcessor>();
        await processor.Retention(CancellationToken.None);
        var version = (await store.Run("fixture", tx => tx.Get<Session>(upcoming.Id)))!.Version;
        await processor.Retention(CancellationToken.None);
        Assert.Equal(2, (await store.Run("fixture", tx => tx.List<BackgroundJob>())).Length);
        Assert.Equal(
            version,
            (await store.Run("fixture", tx => tx.Get<Session>(upcoming.Id)))!.Version
        );
        await scope
            .ServiceProvider.GetRequiredService<RetentionWorkflows>()
            .Extend(upcoming.Id, 12, upcoming.ExpiresAt!.Value.AddDays(5), version);
        await processor.Retention(CancellationToken.None);
        Assert.Equal(3, (await store.Run("fixture", tx => tx.List<BackgroundJob>())).Length);
    }
}
