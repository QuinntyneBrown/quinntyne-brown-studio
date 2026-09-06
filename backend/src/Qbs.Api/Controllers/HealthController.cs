using Microsoft.AspNetCore.Mvc;

namespace Qbs.Api.Controllers;

[ApiController, Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public object Get() => new { status = "Ready" };
}
