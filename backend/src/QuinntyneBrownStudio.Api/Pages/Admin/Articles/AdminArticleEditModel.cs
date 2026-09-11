using QuinntyneBrownStudio.Domain.Exceptions.Blog;
using QuinntyneBrownStudio.Application.Blog.Articles.Commands;
using QuinntyneBrownStudio.Application.Blog.Articles.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuinntyneBrownStudio.Api.Pages.Admin.Articles;

public class AdminArticleEditModel(IMediator mediator) : AdminPageModelBase
{
    public ArticleDto? Article { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        if (!IsAuthenticated()) return Redirect("/admin/login?returnUrl=" + Uri.EscapeDataString(Request.Path + Request.QueryString));
        try { Article = await mediator.Send(new GetArticleByIdQuery(id)); }
        catch (NotFoundException) { return NotFound(); }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, string title, string body, [FromForm(Name = "abstract")] string articleAbstract, string action, int version, Guid? featuredImageId, IFormFile? featuredImage)
    {
        if (!IsAuthenticated()) return Redirect("/admin/login?returnUrl=" + Uri.EscapeDataString(Request.Path + Request.QueryString));
        var ifMatch = $"W/\"article-{id}-v{version}\"";
        try
        {
            var updated = await mediator.Send(new UpdateArticleCommand(id, title, body, articleAbstract, featuredImageId, ifMatch));
            if (action == "publish")
            {
                var newIfMatch = $"W/\"article-{updated.ArticleId}-v{updated.Version}\"";
                await mediator.Send(new PublishArticleCommand(id, !updated.Published, newIfMatch));
            }
            return RedirectToPage("/Admin/Articles/Edit", new { id, success = "Article saved." });
        }
        catch (Exception ex) when (ex is FluentValidation.ValidationException || ex.GetType().Namespace == "QuinntyneBrownStudio.Domain.Exceptions.Blog")
        {
            TempData["Error"] = ex.Message;
            var current = await mediator.Send(new GetArticleByIdQuery(id));
            Article = current with { Title = title, Body = body, Abstract = articleAbstract, FeaturedImageId = featuredImageId, Version = version };
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, int version)
    {
        if (!IsAuthenticated()) return Redirect("/admin/login?returnUrl=" + Uri.EscapeDataString(Request.Path + Request.QueryString));
        try
        {
            var ifMatch = $"W/\"article-{id}-v{version}\"";
            await mediator.Send(new DeleteArticleCommand(id, ifMatch));
            return RedirectToPage("/Admin/Articles/Index");
        }
        catch (Exception ex) when (ex is FluentValidation.ValidationException || ex.GetType().Namespace == "QuinntyneBrownStudio.Domain.Exceptions.Blog")
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("/Admin/Articles/Edit", new { id });
        }
    }
}
