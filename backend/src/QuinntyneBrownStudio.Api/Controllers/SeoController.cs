using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Blog.Seo;

namespace QuinntyneBrownStudio.Api.Controllers;

[ApiController]
[Route("blog")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SeoController(IMediator mediator) : ControllerBase
{
    [HttpGet("/robots.txt")]
    public async Task<IActionResult> RootRobots(CancellationToken ct)
    {
        var document = await mediator.Send(new GetBlogDocumentQuery("root-robots"), ct);
        return Content(document.Content, document.ContentType);
    }

    [HttpGet("robots.txt")]
    public async Task<IActionResult> Robots(CancellationToken ct)
    {
        var document = await mediator.Send(new GetBlogDocumentQuery("robots"), ct);
        return Content(document.Content, document.ContentType);
    }

    [HttpGet("llms.txt")]
    public async Task<IActionResult> Llms(CancellationToken ct)
    {
        var document = await mediator.Send(new GetBlogDocumentQuery("llms"), ct);
        return Content(document.Content, document.ContentType);
    }

    [HttpGet("sitemap.xml")]
    public async Task<IActionResult> Sitemap(CancellationToken ct)
    {
        var document = await mediator.Send(new GetBlogDocumentQuery("sitemap"), ct);
        return Content(document.Content, document.ContentType);
    }

    [HttpGet("feed.xml")]
    [HttpGet("feed/rss")]
    public async Task<IActionResult> Rss(CancellationToken ct)
    {
        var document = await mediator.Send(new GetBlogDocumentQuery("rss"), ct);
        return Content(document.Content, document.ContentType);
    }

    [HttpGet("atom.xml")]
    [HttpGet("feed/atom")]
    public async Task<IActionResult> Atom(CancellationToken ct)
    {
        var document = await mediator.Send(new GetBlogDocumentQuery("atom"), ct);
        return Content(document.Content, document.ContentType);
    }

    [HttpGet("feed/json")]
    public async Task<IActionResult> JsonFeed(CancellationToken ct)
    {
        var document = await mediator.Send(new GetBlogDocumentQuery("json"), ct);
        return Content(document.Content, document.ContentType);
    }

}
