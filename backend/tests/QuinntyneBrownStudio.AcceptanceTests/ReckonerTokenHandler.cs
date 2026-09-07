using System.Net;
using System.Net.Http.Json;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class ReckonerTokenHandler(string mode) : HttpMessageHandler
{
    public static string Secret => "sk_" + new string('s', 43);
    public static string Token => "at_" + new string('t', 43);
    public int Calls { get; private set; }
    public string? Authorization { get; private set; }
    public string? Target { get; private set; }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++; Authorization = request.Headers.Authorization?.ToString(); Target = request.RequestUri?.AbsoluteUri;
        var response = mode switch
        {
            "stalled" => new HttpResponseMessage(HttpStatusCode.Created) { Content = new StalledTokenContent() },
            "oversized" => new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent(new string('x', 16385)) },
            "denied" => new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("Provider private detail") },
            "malformed" => new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent("not json") },
            _ => new HttpResponseMessage(HttpStatusCode.Created) { Content = JsonContent.Create(new { token = Token, expiresAt = DateTimeOffset.UtcNow.AddMinutes(mode == "expired" ? -1 : 60) }) }
        };
        return Task.FromResult(response);
    }
}
