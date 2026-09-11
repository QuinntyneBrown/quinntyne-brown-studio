using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Blog.DigitalAssets.Queries;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController]
[Route("blog/assets")]
public sealed class AssetsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{fileName}")]
    public async Task<IActionResult> Serve(string fileName, [FromQuery] int? w, CancellationToken ct)
    {
        var result = await mediator.Send(new GetBlogAssetQuery(fileName, w, Request.Headers.Accept.ToString(), Request.Headers.IfNoneMatch.ToString()), ct);
        Response.Headers.Vary = "Accept";
        Response.Headers.CacheControl = "no-cache";
        if (result.ETag != null) Response.Headers.ETag = result.ETag;
        return result.Content == null ? StatusCode(result.StatusCode) : File(result.Content, result.ContentType!);
    }
}
