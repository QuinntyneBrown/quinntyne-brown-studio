using System.Net.Http.Json;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Infrastructure;

public sealed class AzureMapsRoutes(HttpClient http, IConfiguration config) : IRouteDistanceService
{
    private async Task Authorize(CancellationToken ct)
    {
        http.DefaultRequestHeaders.Remove("x-ms-client-id");
        http.DefaultRequestHeaders.Add(
            "x-ms-client-id",
            config["Azure:MapsClientId"]
                ?? throw new StudioException(503, "Address routing is not configured.")
        );
        var token = await new DefaultAzureCredential().GetTokenAsync(
            new TokenRequestContext(["https://atlas.microsoft.com/.default"]),
            ct
        );
        http.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);
    }

    public async Task<ResolvedLocation[]> Resolve(string address, CancellationToken ct)
    {
        await Authorize(ct);
        var response = await http.GetAsync(
            "https://atlas.microsoft.com/search/address/json?api-version=1.0&countrySet=CA&query="
                + Uri.EscapeDataString(address),
            ct
        );
        if (!response.IsSuccessStatusCode)
            throw new StudioException(503, "Address lookup is unavailable.");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        return json.GetProperty("results")
            .EnumerateArray()
            .Select(x => new ResolvedLocation
            {
                Label = x.GetProperty("address").GetProperty("freeformAddress").GetString()!,
                Latitude = x.GetProperty("position").GetProperty("lat").GetDecimal(),
                Longitude = x.GetProperty("position").GetProperty("lon").GetDecimal(),
            })
            .ToArray();
    }

    public async Task<decimal> Metres(ResolvedLocation[] route, CancellationToken ct)
    {
        await Authorize(ct);
        var points = string.Join(
            ":",
            route.Select(x => FormattableString.Invariant($"{x.Latitude},{x.Longitude}"))
        );
        var response = await http.GetAsync(
            "https://atlas.microsoft.com/route/directions/json?api-version=1.0&traffic=false&computeBestOrder=false&query="
                + Uri.EscapeDataString(points),
            ct
        );
        if (!response.IsSuccessStatusCode)
            throw new StudioException(503, "Driving distance could not be calculated.");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        return json.GetProperty("routes")[0]
            .GetProperty("legs")
            .EnumerateArray()
            .Sum(x => x.GetProperty("summary").GetProperty("lengthInMeters").GetDecimal());
    }
}
