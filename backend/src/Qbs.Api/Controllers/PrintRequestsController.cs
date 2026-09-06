using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Api.Controllers;

[ApiController]
public sealed class PrintRequestsController(ClientWorkflows clients, AdminCatalog catalog)
    : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = "Client"), HttpGet("api/client/print-requests/{id:guid}")]
    public Task<PrintRequest> ClientRequest(Guid id) => clients.PrintRequest(UserId, id);

    [Authorize(Roles = "Client"), HttpPost("api/client/print-requests")]
    public async Task<IActionResult> Submit(PrintRequest input)
    {
        var result = await clients.Submit(UserId, input);
        return Created($"/api/client/print-requests/{result.Id}", result);
    }

    [Authorize(Roles = "Administrator"), HttpGet("api/admin/print-requests")]
    public async Task<object> List(string? state = null) =>
        (await catalog.List<PrintRequest>()).Where(x => state == null || x.State == state);

    [Authorize(Roles = "Administrator"), HttpGet("api/admin/print-requests/{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        await catalog.Get<PrintRequest>(id) is { } value ? Ok(value) : NotFound();

    [Authorize(Roles = "Administrator"), HttpPost("api/admin/print-requests/{id:guid}/review")]
    public Task<PrintRequest> Review(Guid id, VersionInput input) =>
        clients.Review(id, UserId, input.ExpectedVersion);
}
