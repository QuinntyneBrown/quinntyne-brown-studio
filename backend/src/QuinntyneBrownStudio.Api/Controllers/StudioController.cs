using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Application.Catalog.Studio;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/studios")]
public sealed class StudioController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await sender.Send(new ListStudio()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        Ok(await sender.Send(new GetStudio(id)));

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
