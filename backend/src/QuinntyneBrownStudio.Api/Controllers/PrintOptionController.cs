using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Application.Catalog.PrintOption;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/print-options")]
public sealed class PrintOptionController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await sender.Send(new ListPrintOption()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        Ok(await sender.Send(new GetPrintOption(id)));

    [HttpPost]
    public async Task<IActionResult> Create(PrintOption value)
    {
        var saved = await sender.Send(new SavePrintOption(value, null));
        return Created($"/api/admin/print-options/{saved.Id}", saved);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Save(Guid id, PrintOption value) =>
        Ok(await sender.Send(new SavePrintOption(value, id)));
}
