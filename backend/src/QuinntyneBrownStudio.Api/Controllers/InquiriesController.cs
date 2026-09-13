using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Api.Models;
using QuinntyneBrownStudio.Application.Inquiries;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/inquiries")]
public sealed class InquiriesController(ISender sender) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> List(string? state = null) =>
        Ok(await sender.Send(new ListInquiries(state)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) => Ok(await sender.Send(new GetInquiry(id)));

    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, VersionInput input) =>
        Ok(await sender.Send(new ReviewInquiry(id, UserId, input.ExpectedVersion)));
}
