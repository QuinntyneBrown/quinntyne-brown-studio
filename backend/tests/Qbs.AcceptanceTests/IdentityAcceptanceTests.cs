using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.AcceptanceTests;

public sealed class IdentityAcceptanceTests
{
    private sealed class CookieFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.UseEnvironment("Testing");
    }

    private static async Task Antiforgery(HttpClient client)
    {
        var token = await client.GetFromJsonAsync<JsonElement>("/api/auth/antiforgery");
        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        client.DefaultRequestHeaders.Add(
            "X-XSRF-TOKEN",
            token.GetProperty("requestToken").GetString()
        );
    }

    [Fact]
    public async Task AC_L2_003_01_AC_L2_032_01_AC_L2_032_02_Real_identity_cookie_enforces_roles_credentials_and_logout()
    {
        using var factory = new CookieFactory();
        using var client = factory.CreateClient(
            new() { BaseAddress = new("https://localhost"), AllowAutoRedirect = false }
        );
        await using var scope = factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();
        var user = new IdentityUser<Guid>
        {
            Id = Guid.NewGuid(),
            UserName = "client@example.test",
            Email = "client@example.test",
            EmailConfirmed = true,
        };
        Assert.True((await users.CreateAsync(user, "Test-only!12345")).Succeeded);
        Assert.True((await users.AddToRoleAsync(user, "Client")).Succeeded);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.GetAsync("/api/client/galleries")).StatusCode
        );
        await Antiforgery(client);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (
                await client.PostAsJsonAsync(
                    "/api/auth/login",
                    new { email = user.Email, password = "wrong" }
                )
            ).StatusCode
        );
        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = user.Email, password = "Test-only!12345" }
        );
        login.EnsureSuccessStatusCode();
        var cookie = Assert.Single(
            login.Headers.GetValues("Set-Cookie"),
            x => x.StartsWith("__Host-qbs=")
        );
        Assert.Contains("secure", cookie);
        Assert.Contains("httponly", cookie);
        Assert.DoesNotContain("domain=", cookie);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.GetAsync("/api/client/galleries")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await client.GetAsync("/api/admin/equipment")).StatusCode
        );
        await Antiforgery(client);
        (await client.PostAsJsonAsync("/api/auth/logout", new { })).EnsureSuccessStatusCode();
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.GetAsync("/api/client/galleries")).StatusCode
        );
    }

    [Fact]
    public async Task AC_L2_062_01_Invitation_and_recovery_are_single_use_and_recovery_is_account_neutral()
    {
        using var factory = new CookieFactory();
        using var client = factory.CreateClient(
            new() { BaseAddress = new("https://localhost"), AllowAutoRedirect = false }
        );
        await using var scope = factory.Services.CreateAsyncScope();
        var accounts =
            scope.ServiceProvider.GetRequiredService<Qbs.Infrastructure.IdentityAccounts>();
        var store = scope.ServiceProvider.GetRequiredService<IStudioStore>();
        var protector = scope
            .ServiceProvider.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("qbs-email-v1");
        await accounts.Invite("invitee@example.test");
        async Task<string> LatestToken(string purpose)
        {
            var tokens = await store.Run("fixture", tx => tx.List<AccountToken>());
            var entry = Assert.Single(tokens, x => x.Purpose == purpose);
            var jobs = await store.Run("fixture", tx => tx.List<BackgroundJob>());
            var job = Assert.Single(jobs, x => x.ResourceId == entry.Id);
            var payload = JsonSerializer.Deserialize<JsonElement>(protector.Unprotect(job.Payload));
            return Regex
                .Match(payload.GetProperty("body").GetString()!, @"token=([A-F0-9]+)")
                .Groups[1]
                .Value;
        }
        var invitation = await LatestToken("invitation");
        await Antiforgery(client);
        (
            await client.PostAsJsonAsync(
                "/api/auth/accept-invitation",
                new { token = invitation, password = "Test-only!12345" }
            )
        ).EnsureSuccessStatusCode();
        await Antiforgery(client);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await client.PostAsJsonAsync(
                    "/api/auth/accept-invitation",
                    new { token = invitation, password = "Test-only!12345" }
                )
            ).StatusCode
        );
        var existing = await client.PostAsJsonAsync(
            "/api/auth/recovery",
            new { email = "invitee@example.test" }
        );
        var absent = await client.PostAsJsonAsync(
            "/api/auth/recovery",
            new { email = "absent@example.test" }
        );
        Assert.Equal(HttpStatusCode.Accepted, existing.StatusCode);
        Assert.Equal(
            await existing.Content.ReadAsStringAsync(),
            await absent.Content.ReadAsStringAsync()
        );
        var recovery = await LatestToken("recovery");
        (
            await client.PostAsJsonAsync(
                "/api/auth/reset-password",
                new { token = recovery, password = "Replacement!12345" }
            )
        ).EnsureSuccessStatusCode();
        await Antiforgery(client);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await client.PostAsJsonAsync(
                    "/api/auth/reset-password",
                    new { token = recovery, password = "Replacement!12345" }
                )
            ).StatusCode
        );
    }
}
