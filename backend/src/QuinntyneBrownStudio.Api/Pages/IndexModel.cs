using QuinntyneBrownStudio.Application.Blog.Models;
using QuinntyneBrownStudio.Application.Blog.Articles.Queries;
using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Application.Presentation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuinntyneBrownStudio.Api.Pages;

[ResponseCache(CacheProfileName = "HtmlPage")]
public class IndexModel(IMediator mediator, IETagGenerator eTagGenerator) : PageModel
{
    public PagedResponse<ArticleListDto> Articles { get; private set; } = new();
    public int CurrentPage { get; private set; } = 1;

    public async Task OnGetAsync(int page = 1)
    {
        CurrentPage = page;
        Articles = await mediator.Send(new GetPublishedArticlesQuery(page, 9));

        // Generate ETag from the page content hash so reverse proxies and browsers
        // can use conditional GET requests. The relaunch gate (OD-14) changes the shell around
        // the listing, so a cached copy from the other side of the gate does not match.
        var gated = (await mediator.Send(new GetLaunchState())).ComingSoon ? "-gated" : "";
        var hashInput = string.Join(",", Articles.Items.Select(a => $"{a.ArticleId}:{a.Version}"));
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"index-p{page}-{hashInput}{gated}"));
        var etag = $"W/\"{Convert.ToHexString(hash[..8]).ToLowerInvariant()}\"";
        var ifNoneMatch = Request.Headers.IfNoneMatch.FirstOrDefault();
        if (eTagGenerator.IsMatch(etag, ifNoneMatch))
        {
            Response.StatusCode = 304;
            return;
        }
        Response.Headers.ETag = etag;
        Response.Headers.Append("Cache-Control", "no-cache");
    }
}
