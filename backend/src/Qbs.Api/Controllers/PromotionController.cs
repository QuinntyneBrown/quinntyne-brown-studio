using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application.Catalog;
using Qbs.Application.Catalog.Promotion;
using Qbs.Domain.Entities;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/promotions")]
public sealed class PromotionController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await sender.Send(new ListPromotion()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        Ok(await sender.Send(new GetPromotion(id)));

    [HttpPost]
    public async Task<IActionResult> Create(Promotion value)
    {
        var saved = await sender.Send(new SavePromotion(value, null));
        return Created($"/api/admin/promotions/{saved.Id}", saved);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Save(Guid id, Promotion value) =>
        Ok(await sender.Send(new SavePromotion(value, id)));
}
