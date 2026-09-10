using QuinntyneBrownStudio.Application.Blog.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController]
[Route("blog/api/[controller]")]
public abstract class ApiControllerBase(IMediator mediator, IConfiguration configuration) : ControllerBase
{
    protected IMediator Mediator { get; } = mediator;

    /// <summary>
    /// Returns a 200 OK result with pagination navigation URLs populated on the response.
    /// URLs are built from the configured <c>Site:SiteUrl</c> base to avoid host-header injection
    /// (Feature 06, Section 3.3 â€” PaginationHelper).
    /// </summary>
    protected IActionResult PagedResult<T>(PagedResponse<T> response)
    {
        var siteUrl = configuration["PublicOrigin"] ?? string.Empty;
        PaginationHelper.SetNavigationUrls(response, siteUrl, Request.Path.Value ?? string.Empty, Request.Query.ToDictionary(p => p.Key, p => p.Value.ToString()));
        return Ok(response);
    }

    protected IActionResult CreatedResource<T>(T resource, string routeName, object routeValues)
        => CreatedAtRoute(routeName, routeValues, resource);
}
