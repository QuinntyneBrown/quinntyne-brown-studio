using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Application.Catalog.StudioDetails;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/studio-details")]
public sealed class StudioDetailsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await sender.Send(new GetStudioDetails()));

    [HttpPut]
    public async Task<IActionResult> Save(StudioDetails value) =>
        Ok(await sender.Send(new SaveStudioDetails(value, AdminCatalog.ConfigurationId)));
}
