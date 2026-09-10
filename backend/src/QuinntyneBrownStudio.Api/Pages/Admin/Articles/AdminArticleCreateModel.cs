using QuinntyneBrownStudio.Application.Blog.Articles.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuinntyneBrownStudio.Api.Pages.Admin.Articles;

public class AdminArticleCreateModel(IMediator mediator) : AdminPageModelBase
{
    public string Title { get; private set; } = "";
    public string Body { get; private set; } = "";
    public string Abstract { get; private set; } = "";
    public Guid? FeaturedImageId { get; private set; }

    public void OnGet()
    {
        if (!IsAuthenticated()) Response.Redirect("/admin/login?returnUrl=/blog/admin/articles/create");
    }

    public async Task<IActionResult> OnPostAsync(string title, string body, [FromForm(Name = "abstract")] string articleAbstract, Guid? featuredImageId)
    {
        if (!IsAuthenticated()) return Redirect("/admin/login?returnUrl=" + Uri.EscapeDataString(Request.Path + Request.QueryString));
        Title = title; Body = body; Abstract = articleAbstract; FeaturedImageId = featuredImageId;
        try
        {
            var result = await mediator.Send(new CreateArticleCommand(title, body, articleAbstract, featuredImageId));
            return RedirectToPage("/Admin/Articles/Edit", new { id = result.ArticleId, success = "Article created." });
        }
        catch (Exception ex) when (ex is FluentValidation.ValidationException || ex.GetType().Namespace == "QuinntyneBrownStudio.Domain.Exceptions.Blog")
        {
            ModelState.AddModelError("title", ex.Message);
            return Page();
        }
    }
}
