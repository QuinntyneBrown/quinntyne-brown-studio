using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Diagnostics;
namespace QuinntyneBrownStudio.Api.Controllers;
[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/development-mail"), ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class DevelopmentMailController(ISender sender) : ControllerBase {
  [HttpGet]
  public Task<object> Get(CancellationToken ct) => sender.Send(new GetDevelopmentMail(), ct);
}
