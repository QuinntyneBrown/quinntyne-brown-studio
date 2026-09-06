using MediatR;
using QuinntyneBrownStudio.Application.Clients;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Api.Models;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(ISender sender)
    : ControllerBase
{
    [HttpGet("antiforgery")]
    public Task<QuinntyneBrownStudio.Domain.Models.AntiforgeryToken> Token() => sender.Send(new GetAntiforgeryToken());

    [HttpGet("session")]
    public Task<QuinntyneBrownStudio.Domain.Models.AccountSession> Session() => sender.Send(new GetAccountSession());

    [HttpPost("login")]
    public Task<object> Login(LoginInput input) => sender.Send(new SignInAccount(input.Email, input.Password));

    [HttpPost("logout")]
    public async Task<object> Logout()
    {
        return await sender.Send(new SignOutAccount());
    }

    [HttpPost("accept-invitation")]
    public Task<object> Accept(TokenInput input) =>
        sender.Send(new AcceptAccountToken(input.Token, input.Password, "invitation"));

    [HttpPost("reset-password")]
    public Task<object> Reset(TokenInput input) =>
        sender.Send(new AcceptAccountToken(input.Token, input.Password, "recovery"));

    [HttpPost("recovery")]
    public async Task<IActionResult> Recover(EmailInput input)
    {
        return Accepted(await sender.Send(new RecoverAccount(input.Email)));
    }

    [Authorize(Roles = "Administrator"), HttpPost("/api/admin/invitations")]
    public async Task<IActionResult> Invite(EmailInput input) =>
        Accepted(await sender.Send(new InviteClientAccount(input.Email)));
}
