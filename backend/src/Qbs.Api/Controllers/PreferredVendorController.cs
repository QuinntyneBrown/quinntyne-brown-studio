using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application.Catalog;
using Qbs.Application.Catalog.PreferredVendor;
using Qbs.Domain.Entities;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/vendors")]
public sealed class PreferredVendorController(ISender sender, AdminCatalog catalog) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await catalog.List<PreferredVendor>());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        await catalog.Get<PreferredVendor>(id) is { } value ? Ok(value) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(PreferredVendor value)
    {
        var saved = await sender.Send(new SavePreferredVendor(value, null));
        return Created($"/api/admin/vendors/{saved.Id}", saved);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Save(Guid id, PreferredVendor value) =>
        Ok(await sender.Send(new SavePreferredVendor(value, id)));
}
