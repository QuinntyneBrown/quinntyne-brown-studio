using MediatR;
using Qbs.Application.Clients;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Api.Models;
using Qbs.Application.Ports;
using Qbs.Domain.Entities;

namespace Qbs.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(ISender sender)
    : ControllerBase
{
    [HttpGet("antiforgery")]
    public Task<Qbs.Domain.Models.AntiforgeryToken> Token() => sender.Send(new GetAntiforgeryToken());

    [HttpGet("session")]
    public Task<Qbs.Domain.Models.AccountSession> Session() => sender.Send(new GetAccountSession());

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
