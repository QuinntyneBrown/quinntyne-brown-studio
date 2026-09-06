using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application.Ports;
using Qbs.Infrastructure.Storage;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/development-storage/{*key}")]
public sealed class DevelopmentStorageController(
    IPhotoStorage storage,
    IWebHostEnvironment environment
) : ControllerBase
{
    [HttpPut, RequestSizeLimit(9000000)]
    public async Task<IActionResult> Put(
        string key,
        string comp,
        string? blockid,
        CancellationToken ct
    )
    {
        if (
            !environment.IsDevelopment() && !environment.IsEnvironment("Testing")
            || storage is not FilePhotoStorage files
        )
            return NotFound();
        if (comp == "block" && blockid != null)
            await files.Block(key, blockid, Request.Body, ct);
        else if (comp == "blocklist")
        {
            var xml = await XDocument.LoadAsync(Request.Body, LoadOptions.None, ct);
            await files.Commit(key, xml.Descendants("Latest").Select(x => x.Value).ToArray(), ct);
        }
        else
            return BadRequest();
        return StatusCode(201);
    }
}
