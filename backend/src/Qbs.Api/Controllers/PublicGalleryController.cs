using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application.Catalog;
using Qbs.Application.Catalog.PublicGallery;
using Qbs.Domain.Entities;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/public-galleries")]
public sealed class PublicGalleryController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await sender.Send(new ListPublicGallery()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        Ok(await sender.Send(new GetPublicGallery(id)));

    [HttpPost]
    public async Task<IActionResult> Create(PublicGallery value)
    {
        var saved = await sender.Send(new SavePublicGallery(value, null));
        return Created($"/api/admin/public-galleries/{saved.Id}", saved);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Save(Guid id, PublicGallery value) =>
        Ok(await sender.Send(new SavePublicGallery(value, id)));
}
