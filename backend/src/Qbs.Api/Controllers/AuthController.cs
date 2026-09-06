using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Api.Models;
using Qbs.Application.Ports;
using Qbs.Domain.Entities;

namespace Qbs.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(IIdentityAccounts accounts, IAntiforgery antiforgery)
    : ControllerBase
{
    [HttpGet("antiforgery")]
    public object Token()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return new { tokens.RequestToken };
    }

    [HttpGet("session")]
    public object Session() =>
        new
        {
            authenticated = User.Identity?.IsAuthenticated ?? false,
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            roles = User.FindAll(ClaimTypes.Role).Select(x => x.Value),
        };

    [HttpPost("login")]
    public Task<object> Login(LoginInput input) => accounts.Login(input.Email, input.Password);

    [HttpPost("logout")]
    public async Task<object> Logout()
    {
        await accounts.Logout();
        return new { authenticated = false };
    }

    [HttpPost("accept-invitation")]
    public Task<object> Accept(TokenInput input) =>
        accounts.Accept(input.Token, input.Password, "invitation");

    [HttpPost("reset-password")]
    public Task<object> Reset(TokenInput input) =>
        accounts.Accept(input.Token, input.Password, "recovery");

    [HttpPost("recovery")]
    public async Task<IActionResult> Recover(EmailInput input)
    {
        await accounts.Recover(input.Email);
        return Accepted(
            new { message = "If the account is eligible, recovery instructions will be sent." }
        );
    }

    [Authorize(Roles = "Administrator"), HttpPost("/api/admin/invitations")]
    public async Task<IActionResult> Invite(EmailInput input) =>
        Accepted(new { invitationId = await accounts.Invite(input.Email), status = "Queued" });
}
