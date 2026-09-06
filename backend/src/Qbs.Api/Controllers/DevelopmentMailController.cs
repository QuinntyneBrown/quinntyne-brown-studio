using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application;
using Qbs.Infrastructure;

namespace Qbs.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/development-mail")]
public sealed class DevelopmentMailController(IEmailSender email, IWebHostEnvironment environment)
    : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        if (!environment.IsDevelopment() || email is not ControlledEmail controlled)
            return NotFound();
        Response.Headers.CacheControl = "private, no-store";
        return Ok(controlled.Messages.Select(x => new { id = x.Key, body = x.Value }));
    }
}
