using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.ReckonerAccess;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/admin/reckoner/token")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ReckonerController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Mint(CancellationToken cancellationToken) => StatusCode(201, await sender.Send(new MintReckonerAdminToken(), cancellationToken));
}
