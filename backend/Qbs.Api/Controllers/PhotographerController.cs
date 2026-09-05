using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/photographers")]
public sealed class PhotographerController(ISender sender, AdminCatalog catalog) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await catalog.List<Photographer>());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        await catalog.Get<Photographer>(id) is { } value ? Ok(value) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(Photographer value)
    {
        var saved = await sender.Send(new SavePhotographer(value, null));
        return Created($"/api/admin/photographers/{saved.Id}", saved);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Save(Guid id, Photographer value) =>
        Ok(await sender.Send(new SavePhotographer(value, id)));
}
