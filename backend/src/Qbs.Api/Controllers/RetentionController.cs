using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Api.Models;
using Qbs.Application.Photos;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator")]
public sealed class RetentionController(ISender sender) : ControllerBase
{
    [HttpGet("api/admin/sessions/{id:guid}/retention")]
    public Task<object> Get(Guid id) => sender.Send(new GetPhotoRetention(id));

    [HttpPut("api/admin/sessions/{id:guid}/retention")]
    public async Task<object> Extend(Guid id, RetentionInput input) =>
        await sender.Send(new ExtendPhotoRetention(id, input.Months, input.ExpiresAt, input.ExpectedVersion));

    [HttpPost("api/admin/sessions/{id:guid}/photo-deletion")]
    public async Task<IActionResult> Delete(Guid id, DeletionInput input) =>
        Accepted(await sender.Send(new DeleteSessionPhotos(id, input.ImpactRevision, input.Confirm)));
}
