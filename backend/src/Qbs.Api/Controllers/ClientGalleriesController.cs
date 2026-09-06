using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Api.Controllers;

[ApiController]
public sealed class ClientGalleriesController(ClientWorkflows clients, IIdentityAccounts accounts)
    : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = "Client"), HttpGet("api/client/galleries")]
    public Task<object> Galleries() => clients.Galleries(UserId);

    [Authorize(Roles = "Client"), HttpGet("api/client/galleries/{id:guid}")]
    public Task<object> Gallery(Guid id) => clients.Galleries(UserId, id);

    [Authorize(Roles = "Administrator"), HttpPut("api/admin/sessions/{id:guid}/clients")]
    public async Task<Session> Assign(Guid id, AssignmentInput input)
    {
        await accounts.RequireClients(input.ClientIds);
        return await clients.Assign(id, input.ClientIds, input.ExpectedVersion);
    }

    [Authorize(Roles = "Administrator"), HttpGet("api/admin/clients")]
    public Task<object> Clients() => accounts.Clients();
}
