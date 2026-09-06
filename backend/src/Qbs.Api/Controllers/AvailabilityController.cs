using MediatR;
using Qbs.Application.Scheduling;
using Microsoft.AspNetCore.Mvc;
using Qbs.Domain.Models;
using SchedulingService = Qbs.Application.Scheduling.Scheduling;

namespace Qbs.Api.Controllers;

[ApiController, Route("api/public/availability")]
public sealed class AvailabilityController(ISender sender) : ControllerBase
{
    [HttpPost]
    public Task<AvailabilityResult> Get(QuoteInput input) =>
        sender.Send(new CheckSessionAvailability(input));
}
