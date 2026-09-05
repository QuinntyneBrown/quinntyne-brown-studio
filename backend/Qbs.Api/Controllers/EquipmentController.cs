using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/equipment")]
public sealed class EquipmentController(ISender sender, AdminCatalog catalog) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await catalog.List<Equipment>());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        await catalog.Get<Equipment>(id) is { } value ? Ok(value) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(Equipment value)
    {
        var saved = await sender.Send(new SaveEquipment(value, null));
        return Created($"/api/admin/equipment/{saved.Id}", saved);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Save(Guid id, Equipment value) =>
        Ok(await sender.Send(new SaveEquipment(value, id)));
}
