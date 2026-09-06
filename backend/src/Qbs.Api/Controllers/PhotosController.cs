using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application.Photos;

namespace Qbs.Api.Controllers;

[ApiController]
public sealed class PhotosController(PhotoWorkflows photos) : ControllerBase
{
    [Authorize(Roles = "Administrator"), HttpGet("api/admin/sessions/{id:guid}/photos")]
    public Task<object> List(Guid id, string? cursor) => photos.Photos(id, cursor);

    [Authorize(Roles = "Administrator"), HttpGet("api/admin/photos/{id:guid}/preview")]
    public Task<IActionResult> Admin(Guid id, bool thumbnail, CancellationToken ct) =>
        Image(id, null, null, ct, thumbnail);

    [Authorize(Roles = "Client"), HttpGet("api/client/photos/{id:guid}/preview")]
    public Task<IActionResult> Client(Guid id, bool thumbnail, CancellationToken ct) =>
        Image(id, Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), null, ct, thumbnail);

    [HttpGet("api/public/galleries/{slug}/photos/{id:guid}")]
    public Task<IActionResult> Public(string slug, Guid id, bool thumbnail, CancellationToken ct) =>
        Image(id, null, slug, ct, thumbnail);

    private async Task<IActionResult> Image(
        Guid id,
        Guid? client,
        string? slug,
        CancellationToken ct,
        bool thumbnail
    )
    {
        var file = await photos.Preview(id, client, slug, ct, thumbnail);
        Response.Headers.CacheControl = "private, no-store";
        return File(file.Content, file.ContentType);
    }
}
