using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator")]
public sealed class AnalysisController(AnalysisWorkflows analysis) : ControllerBase
{
    [HttpPost("api/admin/sessions/{id:guid}/analysis")]
    public async Task<IActionResult> Analyze(Guid id, AnalysisInput input) =>
        Accepted(await analysis.Request(id, input.PhotoIds));

    [HttpGet("api/admin/analysis/{id:guid}")]
    public Task<object> Status(Guid id) => analysis.Status(id);

    [HttpPost("api/admin/analysis/{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id, RetryAnalysisInput input) =>
        Accepted(await analysis.Retry(id, input.FailedPhotoIds));
}
