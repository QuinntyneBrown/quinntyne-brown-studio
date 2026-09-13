using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Presentation;
using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController, Route("api/public/launch")]
public sealed class LaunchController(ISender sender) : ControllerBase
{
    [HttpGet]
    public Task<LaunchState> State() => sender.Send(new GetLaunchState());
}
