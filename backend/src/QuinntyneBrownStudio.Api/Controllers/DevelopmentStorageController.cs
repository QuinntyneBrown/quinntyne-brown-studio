using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Diagnostics;
namespace QuinntyneBrownStudio.Api.Controllers;
[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/development-storage/{*key}")]
public sealed class DevelopmentStorageController(ISender sender) : ControllerBase {
  [HttpPut, RequestSizeLimit(9000000)]
  public async Task<IActionResult> Put(string key, string comp, string? blockid, CancellationToken ct) {
    await sender.Send(new ReceiveDevelopmentUpload(key, comp, blockid, Request.Body), ct);
    return StatusCode(201);
  }
}
