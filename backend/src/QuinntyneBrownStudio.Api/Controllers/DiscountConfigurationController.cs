using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Application.Catalog.DiscountConfiguration;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/discounts")]
public sealed class DiscountConfigurationController(ISender sender)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await sender.Send(new GetDiscountConfiguration()));

    [HttpPut]
    public async Task<IActionResult> Save(DiscountConfiguration value) =>
        Ok(await sender.Send(new SaveDiscountConfiguration(value, AdminCatalog.ConfigurationId)));
}
