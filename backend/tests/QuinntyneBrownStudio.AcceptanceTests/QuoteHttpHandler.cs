namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class QuoteHttpHandler : HttpMessageHandler
{
    public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Respond { get; set; } =
        (_, _) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => Respond(request, ct);
}
