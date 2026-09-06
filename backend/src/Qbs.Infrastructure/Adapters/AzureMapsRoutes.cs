using System.Net.Http.Json;
using System.Text.Json;
using Azure.Core;
using Microsoft.Extensions.Options;
using Qbs.Application.Ports;
using Qbs.Domain.Exceptions;
using Qbs.Domain.ValueObjects;

namespace Qbs.Infrastructure.Adapters;

public sealed class AzureMapsRoutes(HttpClient http, IOptions<AzureMapsOptions> options, TokenCredential credential) : IRouteDistanceService
{
    private async Task<JsonElement> Get(string path, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(options.Value.MapsClientId))
            throw new StudioException(503, "Address routing is not configured.");
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://atlas.microsoft.com/" + path);
        request.Headers.Add("x-ms-client-id", options.Value.MapsClientId);
        var token = await credential.GetTokenAsync(new TokenRequestContext(["https://atlas.microsoft.com/.default"]), ct);
        request.Headers.Authorization = new("Bearer", token.Token);
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) throw new StudioException(503, "Address routing is temporarily unavailable.");
        return await response.Content.ReadFromJsonAsync<JsonElement>(ct);
    }

    private static async Task<T> Read<T>(Func<Task<T>> action, CancellationToken ct)
    {
        try { return await action(); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        { throw new StudioException(503, "Address routing timed out. Please try again."); }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException
            or IndexOutOfRangeException or FormatException or OverflowException or HttpRequestException)
        { throw new StudioException(503, "Address routing returned no usable result. Please try again."); }
    }

    public Task<ResolvedLocation[]> Resolve(string address, CancellationToken ct) => Read(async () =>
    {
        var json = await Get("search/address/json?api-version=1.0&countrySet=CA&query=" + Uri.EscapeDataString(address), ct);
        return json.GetProperty("results").EnumerateArray().Select(x =>
        {
            var candidate = new ResolvedLocation
            {
                Label = x.GetProperty("address").GetProperty("freeformAddress").GetString()!,
                Latitude = x.GetProperty("position").GetProperty("lat").GetDecimal(),
                Longitude = x.GetProperty("position").GetProperty("lon").GetDecimal(),
            };
            if (string.IsNullOrWhiteSpace(candidate.Label) || candidate.Latitude is < -90 or > 90 || candidate.Longitude is < -180 or > 180)
                throw new StudioException(503, "Address lookup returned an invalid location.");
            return candidate;
        }).ToArray();
    }, ct);

    public Task<decimal> Metres(ResolvedLocation[] route, CancellationToken ct) => Read(async () =>
    {
        var points = string.Join(":", route.Select(x => FormattableString.Invariant($"{x.Latitude},{x.Longitude}")));
        var json = await Get("route/directions/json?api-version=1.0&traffic=false&computeBestOrder=false&query=" + Uri.EscapeDataString(points), ct);
        var legs = json.GetProperty("routes")[0].GetProperty("legs").EnumerateArray()
            .Select(x => x.GetProperty("summary").GetProperty("lengthInMeters").GetDecimal()).ToArray();
        if (legs.Length != route.Length - 1 || legs.Any(x => x < 0))
            throw new StudioException(503, "Driving distance could not be calculated.");
        return legs.Sum();
    }, ct);
}
