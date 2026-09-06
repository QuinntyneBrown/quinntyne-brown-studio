using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application.Catalog;
using Qbs.Application.Catalog.Session;
using Qbs.Domain.Entities;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/sessions")]
public sealed class SessionController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await sender.Send(new ListSession()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        Ok(await sender.Send(new GetSession(id)));

    [HttpPost]
    public async Task<IActionResult> Create(Session value)
    {
        var saved = await sender.Send(new SaveSession(value, null));
        return Created($"/api/admin/sessions/{saved.Id}", saved);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Save(Guid id, Session value) =>
        Ok(await sender.Send(new SaveSession(value, id)));
}
