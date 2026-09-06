using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Application.Catalog.Equipment;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/equipment")]
public sealed class EquipmentController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await sender.Send(new ListEquipment()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        Ok(await sender.Send(new GetEquipment(id)));

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
