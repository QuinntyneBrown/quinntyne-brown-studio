using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/discounts")]
public sealed class DiscountConfigurationController(ISender sender, AdminCatalog catalog)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(
            await catalog.Get<DiscountConfiguration>(AdminCatalog.ConfigurationId)
                ?? new DiscountConfiguration() { Id = AdminCatalog.ConfigurationId }
        );

    [HttpPut]
    public async Task<IActionResult> Save(DiscountConfiguration value) =>
        Ok(await sender.Send(new SaveDiscountConfiguration(value, AdminCatalog.ConfigurationId)));
}
