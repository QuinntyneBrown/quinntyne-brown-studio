using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Application.Presentation;

namespace QuinntyneBrownStudio.Api.Pages;

[ResponseCache(CacheProfileName = "HtmlPage")]
public class ContactModel(IMediator mediator, IETagGenerator eTagGenerator) : PageModel
{
    public ContactPageView View { get; private set; } = default!;

    /// <summary>The reference of the inquiry the visitor just sent, shown as the confirmation.</summary>
    public string? Sent { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? sent)
    {
        View = await mediator.Send(new GetContactPage());
        Sent = string.IsNullOrWhiteSpace(sent) ? null : sent.Trim();
        var etag = PublicPageETag.Compute("contact", View.Fingerprint + "|" + Sent);
        if (eTagGenerator.IsMatch(etag, Request.Headers.IfNoneMatch.FirstOrDefault()))
            return StatusCode(304);
        Response.Headers.ETag = etag;
        Response.Headers.Append("Cache-Control", "no-cache");
        return Page();
    }
}
