using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Application.Catalog.RateConfiguration;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/rates")]
public sealed class RateConfigurationController(ISender sender)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await sender.Send(new GetRateConfiguration()));

    [HttpPut]
    public async Task<IActionResult> Save(RateConfiguration value) =>
        Ok(await sender.Send(new SaveRateConfiguration(value, AdminCatalog.ConfigurationId)));
}
