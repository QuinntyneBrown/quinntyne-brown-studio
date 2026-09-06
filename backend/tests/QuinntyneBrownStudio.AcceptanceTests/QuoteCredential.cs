using Azure.Core;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class QuoteCredential : TokenCredential
{
    public override AccessToken GetToken(TokenRequestContext context, CancellationToken ct) => new("controlled-credential", DateTimeOffset.MaxValue);
    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext context, CancellationToken ct) => ValueTask.FromResult(GetToken(context, ct));
}
