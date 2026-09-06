using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qbs.Application.Ports;
using Qbs.Domain.Entities;
using Qbs.Infrastructure.DependencyInjection;

namespace Qbs.AcceptanceTests;

public sealed class LocalDbAcceptanceTests
{
    private const string Email = "localdb-acceptance@example.test";
    private const string Password = "Acceptance-only!1945";

    [LocalDbFact]
    public Task DB01_Development_configuration_and_identity_survive_restart() => Reopen("Development");

    [LocalDbFact]
    public Task DB02_Production_configuration_and_identity_survive_restart() => Reopen("Production");

    private static async Task Reopen(string environment)
    {
        await using var database = new LocalDbTestDatabase();
        await database.Migrate();
        using (var first = new RuntimeStudioFactory(environment, database.ConnectionString))
        {
            using var client = first.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
            await using var scope = first.Services.CreateAsyncScope();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            if (!await roles.RoleExistsAsync("Administrator"))
                Assert.True((await roles.CreateAsync(new("Administrator"))).Succeeded);
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();
            var user = new IdentityUser<Guid> { Id = Guid.NewGuid(), UserName = Email, Email = Email, EmailConfirmed = true };
            Assert.True((await users.CreateAsync(user, Password)).Succeeded);
            Assert.True((await users.AddToRoleAsync(user, "Administrator")).Succeeded);
            await Login(client);
            (await client.PostAsJsonAsync("/api/admin/studios", new { name = "Persisted base", resolvedAddress = new { label = "Base", latitude = 43.6, longitude = -79.4 }, hourlyFee = "20", enabled = true, isBase = true })).EnsureSuccessStatusCode();
            (await client.PutAsJsonAsync("/api/admin/rates", new { serviceRates = new { Wedding = "120" }, costRates = new { travel = "2", equipment = "0", assistant = "0", lunch = "0" }, expectedVersion = 0 })).EnsureSuccessStatusCode();
        }
        using var second = new RuntimeStudioFactory(environment, database.ConnectionString);
        using var reopened = second.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        await Login(reopened);
        Assert.Equal("Persisted base", Assert.Single((await reopened.GetFromJsonAsync<JsonElement[]>("/api/public/studios"))!).GetProperty("name").GetString());
        var quote = await reopened.PostAsJsonAsync("/api/public/quotes/calculate", new
        {
            service = "Wedding", startsAt = "2027-06-01T10:00:00-04:00", endsAt = "2027-06-01T11:00:00-04:00",
            locations = new[] { new { location = new { label = "Venue", latitude = 43.7, longitude = -79.3 }, parkingAmount = "0" } }, inputRevision = 1,
        });
        quote.EnsureSuccessStatusCode();
        Assert.Equal("144.00", (await quote.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("total").GetProperty("amount").GetString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Server=.\\SQLEXPRESS;Database=Qbs;Integrated Security=true")]
    [InlineData("Server=tcp:example.database.windows.net;Database=Qbs;Integrated Security=true")]
    [InlineData("Server=(localdb)\\MSSQLLocalDB;Database=Qbs;User ID=example;Password=not-a-real-password")]
    [InlineData("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true")]
    [InlineData("not a connection string")]
    public void DB04_Unsupported_or_missing_production_connection_fails_startup(string? connection)
    {
        using var host = new RuntimeStudioFactory("Production", connection);
        var error = Assert.ThrowsAny<Exception>(() => host.CreateClient());
        Assert.Contains("LocalDB", error.ToString());
        Assert.DoesNotContain("not-a-real-password", error.ToString());
    }

    [LocalDbFact]
    public async Task DB05_Missing_database_is_not_created_by_normal_startup()
    {
        await using var database = new LocalDbTestDatabase();
        using var host = new RuntimeStudioFactory("Development", database.ConnectionString);
        var error = Assert.ThrowsAny<Exception>(() => host.CreateClient());
        Assert.Contains("--migrate", error.ToString());
        await using var db = database.Open();
        Assert.False(await db.Database.CanConnectAsync());
    }

    [LocalDbFact]
    public async Task DB05_Unapplied_migrations_are_not_applied_by_normal_startup()
    {
        await using var database = new LocalDbTestDatabase();
        await using var db = database.Open();
        await db.Database.EnsureCreatedAsync();
        using var host = new RuntimeStudioFactory("Development", database.ConnectionString);
        var error = Assert.ThrowsAny<Exception>(() => host.CreateClient());
        Assert.Contains("--migrate", error.ToString());
        Assert.Empty(await db.Database.GetAppliedMigrationsAsync());
    }

    [LocalDbFact]
    public async Task DB03_Committed_API_jobs_are_visible_to_a_fresh_worker_composition()
    {
        await using var database = new LocalDbTestDatabase();
        await database.Migrate();
        var job = new BackgroundJob { Kind = "Preview", ResourceId = Guid.NewGuid(), AvailableAt = DateTimeOffset.UtcNow.AddDays(1) };
        using (var api = new RuntimeStudioFactory("Development", database.ConnectionString))
        {
            using var client = api.CreateClient();
            await using var scope = api.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<IStudioStore>().Run("job:" + job.Id, async tx => { await tx.Save(job, 0); return true; });
        }
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Studio"] = database.ConnectionString }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStudio(config, false);
        await using var worker = services.BuildServiceProvider();
        await using var workerScope = worker.CreateAsyncScope();
        var saved = await workerScope.ServiceProvider.GetRequiredService<IStudioStore>().Run("job:" + job.Id, tx => tx.Get<BackgroundJob>(job.Id));
        Assert.NotNull(saved);
        Assert.Equal("Queued", saved.State);
        Assert.Equal(job.ResourceId, saved.ResourceId);
    }

    private static async Task Login(HttpClient client)
    {
        await Csrf(client);
        (await client.PostAsJsonAsync("/api/auth/login", new { email = Email, password = Password })).EnsureSuccessStatusCode();
        await Csrf(client);
    }

    [LocalDbFact]
    public async Task DB06_Migration_and_provisioning_commands_are_repeatable_and_preserve_data()
    {
        await using var database = new LocalDbTestDatabase();
        foreach (var command in new[] { "--migrate", "--provision-admin", "--provision-admin" })
        {
            var result = await ApiCommand.Run(database.ConnectionString, command);
            Assert.True(result.ExitCode == 0, result.Output);
        }
        using (var first = new RuntimeStudioFactory("Production", database.ConnectionString))
        {
            using var client = first.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
            await Login(client);
            (await client.PostAsJsonAsync("/api/admin/studios", new { name = "Migration survivor", resolvedAddress = new { label = "Base", latitude = 43.6, longitude = -79.4 }, hourlyFee = "20", enabled = true, isBase = true })).EnsureSuccessStatusCode();
        }
        var repeated = await ApiCommand.Run(database.ConnectionString, "--migrate");
        Assert.True(repeated.ExitCode == 0, repeated.Output);
        using var second = new RuntimeStudioFactory("Production", database.ConnectionString);
        using var reopened = second.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        await Login(reopened);
        Assert.Equal("Migration survivor", Assert.Single((await reopened.GetFromJsonAsync<JsonElement[]>("/api/public/studios"))!).GetProperty("name").GetString());
    }

    private static async Task Csrf(HttpClient client)
    {
        var token = await client.GetFromJsonAsync<JsonElement>("/api/auth/antiforgery");
        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token.GetProperty("requestToken").GetString());
    }
}
