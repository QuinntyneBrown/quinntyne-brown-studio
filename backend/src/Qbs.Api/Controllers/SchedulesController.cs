using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using SchedulingService = Qbs.Application.Scheduling.Scheduling;

namespace Qbs.Api.Controllers;

[
    ApiController,
    Authorize(Roles = "Administrator"),
    Route("api/admin/photographers/{id:guid}/schedule")
]
public sealed class SchedulesController(SchedulingService scheduling, AdminCatalog catalog)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid id) =>
        Ok(await catalog.Get<PhotographerSchedule>(id) ?? new() { Id = id, PhotographerId = id });

    [HttpPut]
    public async Task<IActionResult> Save(Guid id, PhotographerSchedule value) =>
        Ok(await scheduling.Save(id, value));
}
