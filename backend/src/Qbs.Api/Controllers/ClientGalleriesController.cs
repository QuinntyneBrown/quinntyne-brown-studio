using MediatR;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Api.Models;
using Qbs.Application.Clients;
using Qbs.Application.Ports;
using Qbs.Domain.Entities;

namespace Qbs.Api.Controllers;

[ApiController]
public sealed class ClientGalleriesController(ISender sender)
    : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = "Client"), HttpGet("api/client/galleries")]
    public Task<object> Galleries() => sender.Send(new GetClientGalleries(UserId));

    [Authorize(Roles = "Client"), HttpGet("api/client/galleries/{id:guid}")]
    public Task<object> Gallery(Guid id) => sender.Send(new GetClientGalleries(UserId, id));

    [Authorize(Roles = "Administrator"), HttpPut("api/admin/sessions/{id:guid}/clients")]
    public async Task<Session> Assign(Guid id, AssignmentInput input)
    {
        return await sender.Send(new AssignClientGallery(id, input.ClientIds, input.ExpectedVersion));
    }

    [Authorize(Roles = "Administrator"), HttpGet("api/admin/clients")]
    public Task<object> Clients() => sender.Send(new ListClientAccounts());
}
