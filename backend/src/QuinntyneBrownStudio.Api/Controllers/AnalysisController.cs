using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Api.Models;
using QuinntyneBrownStudio.Application.Photos;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator")]
public sealed class AnalysisController(ISender sender) : ControllerBase
{
    [HttpPost("api/admin/sessions/{id:guid}/analysis")]
    public async Task<IActionResult> Analyze(Guid id, AnalysisInput input) =>
        Accepted(await sender.Send(new StartPhotoAnalysis(id, input.PhotoIds)));

    [HttpGet("api/admin/analysis/{id:guid}")]
    public Task<object> Status(Guid id) => sender.Send(new GetPhotoAnalysis(id));

    [HttpPost("api/admin/analysis/{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id, RetryAnalysisInput input) =>
        Accepted(await sender.Send(new RetryPhotoAnalysis(id, input.FailedPhotoIds)));
}
