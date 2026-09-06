using MediatR;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Api.Models;
using Qbs.Application.Catalog;
using Qbs.Application.Clients;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;

namespace Qbs.Api.Controllers;

[ApiController]
public sealed class PrintRequestsController(ISender sender)
    : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = "Client"), HttpPost("api/client/print-requests/preview")]
    public Task<PrintPreview> Preview(PrintPreviewInput input) => sender.Send(new PreviewPrintRequest(UserId, input));

    [Authorize(Roles = "Client"), HttpGet("api/client/print-requests/{id:guid}")]
    public Task<PrintRequest> ClientRequest(Guid id) => sender.Send(new GetClientPrintRequest(UserId, id));

    [Authorize(Roles = "Client"), HttpPost("api/client/print-requests")]
    public async Task<IActionResult> Submit(PrintRequest input)
    {
        var result = await sender.Send(new SubmitPrintRequest(UserId, input));
        return Created($"/api/client/print-requests/{result.Id}", result);
    }

    [Authorize(Roles = "Administrator"), HttpGet("api/admin/print-requests")]
    public async Task<object> List(string? state = null) =>
        await sender.Send(new ListPrintRequests(state));

    [Authorize(Roles = "Administrator"), HttpGet("api/admin/print-requests/{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        Ok(await sender.Send(new GetPrintRequest(id)));

    [Authorize(Roles = "Administrator"), HttpPost("api/admin/print-requests/{id:guid}/review")]
    public Task<PrintRequest> Review(Guid id, VersionInput input) =>
        sender.Send(new ReviewPrintRequest(id, UserId, input.ExpectedVersion));
}
