using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Qbs.Application.Ports;
using Qbs.Domain.Exceptions;
using Qbs.Domain.ValueObjects;
using Qbs.Infrastructure.Adapters;

namespace Qbs.AcceptanceTests;

// Q-04/Q-05: the real Maps adapter participates; only its credential and HTTP boundary are controlled.
public sealed class QuoteMapsAcceptanceTests
{
    [Theory]
    [InlineData("{\"results\":[]}", 200)]
    [InlineData("{\"results\":[{\"address\":{\"freeformAddress\":\"Venue\"},\"position\":{\"lat\":43.7,\"lon\":-79.3}}]}", 200)]
    [InlineData("{}", 503)]
    [InlineData("not-json", 503)]
    [InlineData("{\"results\":[{\"address\":{\"freeformAddress\":\"Venue\"},\"position\":{\"lat\":100,\"lon\":0}}]}", 503)]
    public async Task Q05_AC_L2_056_01_Address_results_or_controlled_provider_failure(string payload, int status)
    {
        using var handler = new QuoteHttpHandler
        {
            Respond = (request, _) =>
        {
            Assert.Contains("query=Venue%20%26%20Park", request.RequestUri!.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json") });
        }
        };
        using var http = new HttpClient(handler);
        var maps = new AzureMapsRoutes(http, Options.Create(new AzureMapsOptions { MapsClientId = "controlled" }), new QuoteCredential());
        using var root = new StudioFactory();
        using var factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<IRouteDistanceService>(maps)));
        using var visitor = await LiveQuoteAcceptanceTests.Actor(factory);
        var response = await visitor.PostAsJsonAsync("/api/public/locations/resolve", new { address = "Venue & Park" });
        Assert.Equal(status, (int)response.StatusCode);
        if (status == 200) Assert.Equal(JsonDocument.Parse(payload).RootElement.GetProperty("results").GetArrayLength(), (await response.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength());
    }

    [Theory]
    [InlineData("{\"routes\":[]}")]
    [InlineData("{\"routes\":[{\"legs\":[]}]}")]
    [InlineData("{\"routes\":[{\"legs\":[{\"summary\":{\"lengthInMeters\":-1}}]}]}")]
    [InlineData("{}")]
    public async Task Q04_AC_L2_056_01_Malformed_route_never_falls_back_to_zero(string payload)
    {
        using var handler = new QuoteHttpHandler { Respond = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json") }) };
        using var http = new HttpClient(handler);
        var maps = new AzureMapsRoutes(http, Options.Create(new AzureMapsOptions { MapsClientId = "controlled" }), new QuoteCredential());
        var error = await Assert.ThrowsAsync<StudioException>(() => maps.Metres([new(), new()], default));
        Assert.Equal(503, error.Status);
    }

    [Fact]
    public async Task Q01_AC_L2_056_01_Route_legs_sum_without_intermediate_rounding()
    {
        using var handler = new QuoteHttpHandler
        {
            Respond = (request, _) =>
        {
            Assert.Contains("traffic=false&computeBestOrder=false", request.RequestUri!.Query);
            Assert.Contains("43.6%2C-79.4%3A43.7%2C-79.3%3A43.6%2C-79.4", request.RequestUri.AbsoluteUri);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { routes = new[] { new { legs = new[] { new { summary = new { lengthInMeters = 1234.5m } }, new { summary = new { lengthInMeters = 2222.6m } } } } } }) });
        }
        };
        using var http = new HttpClient(handler);
        var maps = new AzureMapsRoutes(http, Options.Create(new AzureMapsOptions { MapsClientId = "controlled" }), new QuoteCredential());
        var origin = new ResolvedLocation { Latitude = 43.6m, Longitude = -79.4m };
        Assert.Equal(3457.1m, await maps.Metres([origin, new() { Latitude = 43.7m, Longitude = -79.3m }, origin], default));
    }

    [Fact]
    public async Task Q04_Timeout_is_unavailable_but_caller_cancellation_is_preserved()
    {
        using var handler = new QuoteHttpHandler { Respond = (_, _) => throw new TaskCanceledException() };
        using var http = new HttpClient(handler);
        var maps = new AzureMapsRoutes(http, Options.Create(new AzureMapsOptions { MapsClientId = "controlled" }), new QuoteCredential());
        Assert.Equal(503, (await Assert.ThrowsAsync<StudioException>(() => maps.Resolve("Venue", default))).Status);
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => maps.Resolve("Venue", cancellation.Token));
    }
}
