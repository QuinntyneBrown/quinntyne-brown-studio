using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Qbs.AcceptanceTests;

public sealed class StudioFactory : WebApplicationFactory<Program>
{
    public Qbs.Application.Ports.IPhotoAnalysisService? PhotoAnalysis { get; init; }
    public Action<IServiceCollection>? ConfigurePersistence { get; init; }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services => services.AddFakePersistence());
        builder.ConfigureServices(services => ConfigurePersistence?.Invoke(services));
        builder.ConfigureServices(services => { if (PhotoAnalysis != null) services.AddSingleton(PhotoAnalysis); });
        builder.ConfigureServices(services =>
            services
                .AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = "Acceptance";
                    o.DefaultChallengeScheme = "Acceptance";
                    o.DefaultForbidScheme = "Acceptance";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "Acceptance",
                    _ => { }
                )
        );
    }

    public async Task<HttpClient> Actor(string? role = "Administrator", string? id = null)
    {
        var client = CreateClient(
            new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false }
        );
        if (role != null)
            client.DefaultRequestHeaders.Add(
                "X-Test-Actor",
                $"{role}:{id ?? "00000000-0000-0000-0000-000000000001"}"
            );
        var response = await client.GetAsync("/api/auth/antiforgery");
        if (response.IsSuccessStatusCode)
        {
            var token = await response.Content.ReadFromJsonAsync<JsonElement>();
            client.DefaultRequestHeaders.Add(
                "X-XSRF-TOKEN",
                token.GetProperty("requestToken").GetString()
            );
        }
        return client;
    }
}
