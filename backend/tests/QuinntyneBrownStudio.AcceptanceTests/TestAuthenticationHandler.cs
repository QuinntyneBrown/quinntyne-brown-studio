using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var actor = Request.Headers["X-Test-Actor"].ToString().Split(':');
        if (actor.Length != 2)
            return Task.FromResult(AuthenticateResult.NoResult());
        var identity = new ClaimsIdentity(
            [new(ClaimTypes.NameIdentifier, actor[1]), new(ClaimTypes.Role, actor[0])],
            Scheme.Name
        );
        return Task.FromResult(
            AuthenticateResult.Success(new(new ClaimsPrincipal(identity), Scheme.Name))
        );
    }
}
