using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator")]
public sealed class RetentionController(RetentionWorkflows retention) : ControllerBase
{
    [HttpGet("api/admin/sessions/{id:guid}/retention")]
    public Task<object> Get(Guid id) => retention.Get(id);

    [HttpPut("api/admin/sessions/{id:guid}/retention")]
    public async Task<object> Extend(Guid id, RetentionInput input) =>
        await retention.Extend(id, input.Months, input.ExpiresAt, input.ExpectedVersion);

    [HttpPost("api/admin/sessions/{id:guid}/photo-deletion")]
    public async Task<IActionResult> Delete(Guid id, DeletionInput input) =>
        Accepted(await retention.Delete(id, input.ImpactRevision, input.Confirm));
}
