using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Application.Presentation;

namespace QuinntyneBrownStudio.Api.Pages;

[ResponseCache(CacheProfileName = "HtmlPage")]
public class AboutModel(IMediator mediator, IETagGenerator eTagGenerator) : PageModel
{
    public AboutPageView View { get; private set; } = default!;
    /// <summary>The relaunch gate (OD-14) applies to this visitor: the portfolio link stays hidden.</summary>
    public bool ComingSoon { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        View = await mediator.Send(new GetAboutPage());
        ComingSoon = (await mediator.Send(new GetLaunchState())).ComingSoon;
        // The gate changes what the page shows, so it changes what a cached copy may stand in for.
        var etag = PublicPageETag.Compute("about", View.Fingerprint + (ComingSoon ? ":gated" : ""));
        if (eTagGenerator.IsMatch(etag, Request.Headers.IfNoneMatch.FirstOrDefault()))
            return StatusCode(304);
        Response.Headers.ETag = etag;
        Response.Headers.Append("Cache-Control", "no-cache");
        return Page();
    }
}
