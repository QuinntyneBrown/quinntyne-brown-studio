using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Api.Models;
using Qbs.Application.Photos;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator")]
public sealed class UploadsController(ISender sender) : ControllerBase
{
    [HttpPost("api/admin/sessions/{id:guid}/uploads")]
    public async Task<IActionResult> Create(Guid id, UploadInput input, CancellationToken ct) =>
        Created("/api/admin/uploads", await sender.Send(new CreatePhotoUpload(id, input.Files), ct));

    [HttpGet("api/admin/uploads/{id:guid}")]
    public Task<object> Status(Guid id) => sender.Send(new GetPhotoUpload(id));

    [HttpPost("api/admin/uploads/{batch:guid}/files/{id:guid}/renew")]
    public async Task<object> Renew(Guid batch, Guid id, CancellationToken ct) =>
        await sender.Send(new RenewPhotoUpload(batch, id), ct);

    [HttpPost("api/admin/uploads/{batch:guid}/files/{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid batch, Guid id, CancellationToken ct) =>
        Accepted(await sender.Send(new CompletePhotoUpload(batch, id), ct));

    [HttpPost("api/admin/photos/{id:guid}/retry-preview")]
    public async Task<IActionResult> Retry(Guid id) => Accepted(await sender.Send(new RetryPhotoPreview(id)));
}
