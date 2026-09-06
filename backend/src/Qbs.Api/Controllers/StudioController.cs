using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application.Catalog;
using Qbs.Application.Catalog.Studio;
using Qbs.Domain.Entities;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/studios")]
public sealed class StudioController(ISender sender, AdminCatalog catalog) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await catalog.List<Studio>());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        await catalog.Get<Studio>(id) is { } value ? Ok(value) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(Studio value)
    {
        var saved = await sender.Send(new SaveStudio(value, null));
        return Created($"/api/admin/studios/{saved.Id}", saved);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Save(Guid id, Studio value) =>
        Ok(await sender.Send(new SaveStudio(value, id)));
}
