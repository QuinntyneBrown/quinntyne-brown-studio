// Reckoner L2-041,043,082: Given an authenticated Studio administrator, mint only a short-lived browser token.
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class ReckonerAccessTests
{
    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("Client", HttpStatusCode.Forbidden)]
    public async Task GivenNoAdministratorWhenMintingThenAccessIsDenied(string? role, HttpStatusCode expected)
    {
        using var factory = new StudioFactory(); using var actor = await factory.Actor(role);
        Assert.Equal(expected, (await actor.PostAsJsonAsync("/api/admin/reckoner/token", new { })).StatusCode);
    }

    [Fact]
    public async Task GivenAnAdministratorWithoutAntiforgeryWhenMintingThenNoTokenIsIssued()
    {
        using var factory = new StudioFactory(); using var actor = await factory.Actor();
        actor.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        Assert.Equal(HttpStatusCode.BadRequest, (await actor.PostAsJsonAsync("/api/admin/reckoner/token", new { })).StatusCode);
    }

    [Theory]
    [InlineData("valid", HttpStatusCode.Created)]
    [InlineData("denied", HttpStatusCode.ServiceUnavailable)]
    [InlineData("malformed", HttpStatusCode.ServiceUnavailable)]
    [InlineData("expired", HttpStatusCode.ServiceUnavailable)]
    [InlineData("stalled", HttpStatusCode.ServiceUnavailable)]
    [InlineData("oversized", HttpStatusCode.ServiceUnavailable)]
    public async Task GivenServerCredentialsWhenExchangingThenOnlyAValidShortLivedTokenReachesTheBrowser(string mode, HttpStatusCode expected)
    {
        using var provider = new ReckonerTokenHandler(mode);
        using var factory = new StudioFactory { ConfigurePersistence = services => services.AddHttpClient("ReckonerAdmin").ConfigurePrimaryHttpMessageHandler(() => provider) };
        using var host = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Reckoner:ApiBaseUrl"] = "https://reckoner.acceptance.example",
            ["Reckoner:SecretKey"] = ReckonerTokenHandler.Secret,
        })));
        using var actor = host.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        actor.DefaultRequestHeaders.Add("X-Test-Actor", "Administrator:00000000-0000-0000-0000-000000000001");
        var csrf = await actor.GetFromJsonAsync<JsonElement>("/api/auth/antiforgery");
        actor.DefaultRequestHeaders.Add("X-XSRF-TOKEN", csrf.GetProperty("requestToken").GetString());
        var response = await actor.PostAsJsonAsync("/api/admin/reckoner/token", new { });
        Assert.Equal(expected, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(ReckonerTokenHandler.Secret, body);
        Assert.DoesNotContain("Provider private detail", body);
        Assert.Equal(1, provider.Calls);
        Assert.Equal("Bearer " + ReckonerTokenHandler.Secret, provider.Authorization);
        Assert.Equal("https://reckoner.acceptance.example/api/v1/admin/tokens", provider.Target);
        if (expected == HttpStatusCode.Created)
        {
            var result = JsonDocument.Parse(body).RootElement;
            Assert.Equal(ReckonerTokenHandler.Token, result.GetProperty("adminToken").GetString());
            Assert.Equal("https://reckoner.acceptance.example", result.GetProperty("apiBaseUrl").GetString());
            Assert.True(response.Headers.CacheControl?.NoStore);
        }
    }

    [Fact]
    public async Task GivenMissingConnectionSettingsWhenMintingThenTheAdministratorGetsAReadableUnavailableResponse()
    {
        using var factory = new StudioFactory(); using var actor = await factory.Actor();
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await actor.PostAsJsonAsync("/api/admin/reckoner/token", new { })).StatusCode);
    }
}
