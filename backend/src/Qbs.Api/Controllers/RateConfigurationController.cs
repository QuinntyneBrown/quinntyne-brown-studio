using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application.Catalog;
using Qbs.Application.Catalog.RateConfiguration;
using Qbs.Domain.Entities;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/rates")]
public sealed class RateConfigurationController(ISender sender, AdminCatalog catalog)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(
            await catalog.Get<RateConfiguration>(AdminCatalog.ConfigurationId)
                ?? new RateConfiguration() { Id = AdminCatalog.ConfigurationId }
        );

    [HttpPut]
    public async Task<IActionResult> Save(RateConfiguration value) =>
        Ok(await sender.Send(new SaveRateConfiguration(value, AdminCatalog.ConfigurationId)));
}
