using QuinntyneBrownStudio.Application.Blog.Models;
using QuinntyneBrownStudio.Application.Blog.DigitalAssets.Commands;
using QuinntyneBrownStudio.Application.Blog.DigitalAssets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace QuinntyneBrownStudio.Api.Controllers;

[Route("blog/api/digital-assets")]
public class DigitalAssetsController(IMediator mediator, IConfiguration configuration) : ApiControllerBase(mediator, configuration)
{
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
        var result = await Mediator.Send(new GetDigitalAssetsQuery(userId), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = "GetDigitalAssetById")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetDigitalAssetByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [HttpPost("upload")]
    [Authorize(Roles = "Administrator")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    [EnableRateLimiting("blog-writes")]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
        var result = await Mediator.Send(new UploadDigitalAssetCommand(new QuinntyneBrownStudio.Application.Blog.Services.BlogUploadFile(file.Length, file.FileName, file.OpenReadStream), userId), ct);
        return CreatedResource(result, "GetDigitalAssetById", new { id = result.DigitalAssetId });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    [EnableRateLimiting("blog-writes")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteDigitalAssetCommand(id), ct);
        return NoContent();
    }
}
