using MediatR;
using QuinntyneBrownStudio.Application.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using SchedulingService = QuinntyneBrownStudio.Application.Scheduling.Scheduling;

namespace QuinntyneBrownStudio.Api.Controllers;

[
    ApiController,
    Authorize(Roles = "Administrator"),
    Route("api/admin/photographers/{id:guid}/schedule")
]
public sealed class SchedulesController(ISender sender)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid id) =>
        Ok(await sender.Send(new GetPhotographerSchedule(id)));

    [HttpPut]
    public async Task<IActionResult> Save(Guid id, PhotographerSchedule value) =>
        Ok(await sender.Send(new SavePhotographerSchedule(id, value)));
}
