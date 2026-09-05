using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Api.Controllers;

[ApiController]
public sealed class PresentationController(Presentation presentation, AdminCatalog catalog)
    : ControllerBase
{
    [HttpGet("api/public/promotions")]
    public Task<object> Promotions() => presentation.Public("promotions");

    [HttpGet("api/public/studios")]
    public Task<object> Studios() => presentation.Public("studios");

    [HttpGet("api/public/print-options")]
    public Task<object> Prints() => presentation.Public("print-options");

    [HttpGet("api/public/galleries")]
    public Task<object> Galleries() => presentation.Public("galleries");

    [HttpGet("api/public/galleries/{slug}")]
    public Task<object> Gallery(string slug) => presentation.Public("galleries", slug);

    [HttpGet("api/public/content/{key}")]
    public Task<object> GetContent(string key) => presentation.Public("content", key);

    [Authorize(Roles = "Administrator"), HttpGet("api/admin/content")]
    public Task<MarketingContent[]> Contents() => catalog.List<MarketingContent>();

    [Authorize(Roles = "Administrator"), HttpPut("api/admin/content/{key}")]
    public Task<MarketingContent> Save(string key, MarketingContent value) =>
        presentation.Save(key, value);
}
