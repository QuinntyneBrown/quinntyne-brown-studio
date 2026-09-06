using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application.Diagnostics;
namespace Qbs.Api.Controllers;
[ApiController, Route("api/health")]
public sealed class HealthController(ISender sender) : ControllerBase {
  [HttpGet]
  public Task<object> Get(CancellationToken ct) => sender.Send(new GetHealth(), ct);
}
