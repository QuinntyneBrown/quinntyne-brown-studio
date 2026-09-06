using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application.Diagnostics;
namespace Qbs.Api.Controllers;
[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/development-mail"), ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class DevelopmentMailController(ISender sender) : ControllerBase {
  [HttpGet]
  public Task<object> Get(CancellationToken ct) => sender.Send(new GetDevelopmentMail(), ct);
}
