using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.ReckonerAccess;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Infrastructure.Adapters.Reckoner;

public sealed class ReckonerAccess(IHttpClientFactory clients, IOptionsMonitor<ReckonerOptions> options, IHostEnvironment environment, IClock clock) : IReckonerAccess
{
    public async Task<ReckonerAdminSession> Mint(CancellationToken cancellationToken)
    {
        var settings = options.CurrentValue;
        if (!Uri.TryCreate(settings.ApiBaseUrl, UriKind.Absolute, out var uri)
            || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment)
            || !(uri.Scheme == "https" || uri.Scheme == "http" && uri.IsLoopback && (environment.IsDevelopment() || environment.IsEnvironment("Testing")))
            || !Credential(settings.SecretKey, "sk_"))
            throw new StudioException(503, "Quote administration has not been configured.");
        var apiBaseUrl = settings.ApiBaseUrl.TrimEnd('/');
        using var client = clients.CreateClient("ReckonerAdmin");
        using var request = new HttpRequestMessage(HttpMethod.Post, apiBaseUrl + "/api/v1/admin/tokens");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SecretKey);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(10));
        try
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
            if (!response.IsSuccessStatusCode) throw Unavailable();
            await response.Content.LoadIntoBufferAsync(16384, deadline.Token);
            var grant = await response.Content.ReadFromJsonAsync<ReckonerTokenGrant>(deadline.Token);
            if (grant is null || !Credential(grant.Token, "at_") || grant.ExpiresAt <= clock.UtcNow || grant.ExpiresAt > clock.UtcNow.AddMinutes(65)) throw Unavailable();
            return new(apiBaseUrl, grant.Token!, grant.ExpiresAt);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { throw Unavailable(); }
        catch (Exception error) when (error is HttpRequestException or JsonException or NotSupportedException) { throw Unavailable(); }
    }

    private static bool Credential(string? value, string prefix) => value is { Length: 46 } && value.StartsWith(prefix, StringComparison.Ordinal)
        && value.AsSpan(3).ContainsAnyExcept("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".AsSpan()) == false;
    private static StudioException Unavailable() => new(503, "Quote administration is temporarily unavailable. Try again.");
}
