using QuinntyneBrownStudio.Application.Blog.Models;
using QuinntyneBrownStudio.Application.Blog.Articles.Commands;
using QuinntyneBrownStudio.Application.Blog.Articles.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuinntyneBrownStudio.Api.Pages.Admin.Articles;

public class AdminArticlesIndexModel(IMediator mediator) : AdminPageModelBase
{
    public PagedResponse<ArticleListDto> Articles { get; private set; } = new();
    public int CurrentPage { get; private set; } = 1;

    public async Task<IActionResult> OnGetAsync(int page = 1)
    {
        if (!IsAuthenticated()) return Redirect("/admin/login?returnUrl=" + Uri.EscapeDataString(Request.Path + Request.QueryString));
        CurrentPage = page;
        Articles = await mediator.Send(new GetArticlesQuery(page, 20));
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, int version)
    {
        if (!IsAuthenticated()) return Redirect("/admin/login?returnUrl=" + Uri.EscapeDataString(Request.Path + Request.QueryString));
        try
        {
            var ifMatch = $"W/\"article-{id}-v{version}\"";
            await mediator.Send(new DeleteArticleCommand(id, ifMatch));
        }
        catch (Exception ex) when (ex.GetType().Namespace == "QuinntyneBrownStudio.Domain.Exceptions.Blog")
        {
            return RedirectToPage("/Admin/Articles/Index", new { error = ex.Message });
        }
        return RedirectToPage("/Admin/Articles/Index");
    }
}
