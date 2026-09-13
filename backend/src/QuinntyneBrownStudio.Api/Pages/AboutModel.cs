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

    public async Task<IActionResult> OnGetAsync()
    {
        View = await mediator.Send(new GetAboutPage());
        var etag = PublicPageETag.Compute("about", View.Fingerprint);
        if (eTagGenerator.IsMatch(etag, Request.Headers.IfNoneMatch.FirstOrDefault()))
            return StatusCode(304);
        Response.Headers.ETag = etag;
        Response.Headers.Append("Cache-Control", "no-cache");
        return Page();
    }
}
