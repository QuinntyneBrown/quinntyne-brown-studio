using Microsoft.AspNetCore.Mvc;
using Qbs.Domain.Models;
using SchedulingService = Qbs.Application.Scheduling.Scheduling;

namespace Qbs.Api.Controllers;

[ApiController, Route("api/public/availability")]
public sealed class AvailabilityController(SchedulingService scheduling) : ControllerBase
{
    [HttpPost]
    public Task<AvailabilityResult> Get(QuoteInput input) =>
        scheduling.Check(input.StartsAt, input.EndsAt, input.PhotographerId);
}
