using MediatR;
using QuinntyneBrownStudio.Application.Scheduling;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Domain.Models;
using SchedulingService = QuinntyneBrownStudio.Application.Scheduling.Scheduling;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController, Route("api/public/availability")]
public sealed class AvailabilityController(ISender sender) : ControllerBase
{
    [HttpPost]
    public Task<AvailabilityResult> Get(QuoteInput input) =>
        sender.Send(new CheckSessionAvailability(input));
}
