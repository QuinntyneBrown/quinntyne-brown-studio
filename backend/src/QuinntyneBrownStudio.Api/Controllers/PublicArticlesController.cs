using QuinntyneBrownStudio.Application.Blog.Models;
using QuinntyneBrownStudio.Application.Blog.Articles.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace QuinntyneBrownStudio.Api.Controllers;

[Route("blog/api/public/articles")]
[ApiController]
public class PublicArticlesController(IMediator mediator, IConfiguration configuration) : ApiControllerBase(mediator, configuration)
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParameters paging, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPublishedArticlesQuery(paging.Page, paging.PageSize), ct);
        return PagedResult(result);
    }

    [HttpGet("{slug}", Name = "GetPublishedArticleBySlug")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPublishedArticleBySlugQuery(slug), ct);
        return Ok(result);
    }

    // GET /api/public/articles/search?q=azure&page=1
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string q, [FromQuery] int page = 1, [FromQuery] string sort = "relevant", CancellationToken ct = default)
    {
        var result = await Mediator.Send(new SearchArticlesQuery(q, page, 10, sort), ct);
        return PagedResult(result);
    }

    // GET /api/public/articles/suggestions?q=azure
    [HttpGet("suggestions")]
    public async Task<IActionResult> Suggestions(
        [FromQuery] string q, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetSearchSuggestionsQuery(q), ct);
        return Ok(result);
    }
}
