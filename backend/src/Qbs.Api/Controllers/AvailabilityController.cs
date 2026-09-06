using Microsoft.AspNetCore.Mvc;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Api.Controllers;

[ApiController, Route("api/public/availability")]
public sealed class AvailabilityController(Scheduling scheduling) : ControllerBase
{
    [HttpPost]
    public Task<AvailabilityResult> Get(QuoteInput input) =>
        scheduling.Check(input.StartsAt, input.EndsAt, input.PhotographerId);
}
