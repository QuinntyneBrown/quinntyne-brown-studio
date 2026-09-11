using QuinntyneBrownStudio.Domain.Exceptions.Blog;
using QuinntyneBrownStudio.Application.Blog.Articles.Queries;
using QuinntyneBrownStudio.Application.Blog.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuinntyneBrownStudio.Api.Pages.Articles;

[ResponseCache(CacheProfileName = "HtmlPage")]
public class ArticleDetailModel(IMediator mediator, IETagGenerator eTagGenerator) : PageModel
{
    public ArticleDto? Article { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        try
        {
            var article = await mediator.Send(new GetArticleBySlugQuery(slug));
            if (!article.Published)
            {
                Article = null;
                Response.StatusCode = 404;
                return Page();
            }

            // Compute the weak ETag from stable article metadata (ID + Version).
            // Design reference: docs/detailed-designs/07-web-performance/README.md, Section 3.7.
            var etag = eTagGenerator.Generate(article.ArticleId, article.Version);

            // If the client already holds the current version, short-circuit with 304.
            var ifNoneMatch = Request.Headers.IfNoneMatch.FirstOrDefault();
            if (eTagGenerator.IsMatch(etag, ifNoneMatch))
                return StatusCode(304);

            Article = article;
            Response.Headers.ETag = etag;
            Response.Headers.Append("Cache-Control", "no-cache");
            return Page();
        }
        catch (NotFoundException)
        {
            Article = null;
            Response.StatusCode = 404;
            return Page();
        }
    }
}
