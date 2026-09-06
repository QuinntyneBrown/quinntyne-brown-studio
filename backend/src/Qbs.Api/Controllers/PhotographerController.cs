using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application.Catalog;
using Qbs.Application.Catalog.Photographer;
using Qbs.Domain.Entities;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/photographers")]
public sealed class PhotographerController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await sender.Send(new ListPhotographer()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        Ok(await sender.Send(new GetPhotographer(id)));

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
