using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using QuinntyneBrownStudio.Api.Models;
using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Application.Inquiries;
using QuinntyneBrownStudio.Application.Presentation;

namespace QuinntyneBrownStudio.Api.Pages;

[ResponseCache(CacheProfileName = "HtmlPage")]
[EnableRateLimiting("contact-inquiries")]
public class ContactModel(IMediator mediator, IETagGenerator eTagGenerator, ILogger<ContactModel> logger) : PageModel
{
    public const string FailureNotice =
        "Your message could not be sent. Your entries are still here; please try again in a moment.";

    public ContactPageView View { get; private set; } = default!;

    [BindProperty]
    public InquiryForm Form { get; set; } = new();

    /// <summary>The reference of the inquiry the visitor just sent, shown as the confirmation.</summary>
    public string? Sent { get; private set; }

    /// <summary>Field errors from the last post, keyed by form field name.</summary>
    public Dictionary<string, string> Errors { get; } = new();

    /// <summary>Set when a valid message could not be stored.</summary>
    public string? Failure { get; private set; }

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

    public async Task<IActionResult> OnPostAsync()
    {
        // Only automated agents fill the hidden field; their post is dropped without storing anything.
        if (!string.IsNullOrWhiteSpace(Form.Website))
            return SeeOther("/contact");
        try
        {
            var inquiry = await mediator.Send(
                new SubmitInquiry(
                    Form.Name ?? "",
                    Form.Email ?? "",
                    Form.Phone,
                    Form.Interest ?? "",
                    Form.Message ?? "",
                    Form.Consent
                )
            );
            return SeeOther("/contact?sent=" + Uri.EscapeDataString(inquiry.Reference));
        }
        catch (ValidationException invalid)
        {
            foreach (var error in invalid.Errors)
                Errors.TryAdd(FieldName(error.PropertyName), error.ErrorMessage);
        }
        catch (Exception failure) when (failure is not OperationCanceledException)
        {
            logger.LogWarning(failure, "A contact inquiry could not be stored.");
            Failure = FailureNotice;
        }
        View = await mediator.Send(new GetContactPage());
        return Page();
    }

    public string? Invalid(string field) => Errors.ContainsKey(field) ? "true" : null;

    public string? Describe(string field) => Errors.ContainsKey(field) ? "error-" + field : null;

    private IActionResult SeeOther(string location)
    {
        Response.Headers.Location = location;
        return StatusCode(303);
    }

    private static string FieldName(string property) =>
        string.IsNullOrEmpty(property) ? "request" : char.ToLowerInvariant(property[0]) + property[1..];
}
