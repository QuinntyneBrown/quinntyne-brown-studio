using QuinntyneBrownStudio.Domain.Exceptions.Blog;
using QuinntyneBrownStudio.Application.Blog.DigitalAssets.Commands;
using QuinntyneBrownStudio.Application.Blog.DigitalAssets.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace QuinntyneBrownStudio.Api.Pages.Admin.DigitalAssets;

public class AdminDigitalAssetsIndexModel(IMediator mediator) : AdminPageModelBase
{
    public List<DigitalAssetDto> Assets { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!IsAuthenticated()) return Redirect("/admin/login?returnUrl=" + Uri.EscapeDataString(Request.Path + Request.QueryString));
        var userId = GetCurrentUserId();
        Assets = await mediator.Send(new GetDigitalAssetsQuery(userId));
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(IFormFile file)
    {
        if (!IsAuthenticated()) return Redirect("/admin/login?returnUrl=" + Uri.EscapeDataString(Request.Path + Request.QueryString));
        var userId = GetCurrentUserId();
        try
        {
            await mediator.Send(new UploadDigitalAssetCommand(new QuinntyneBrownStudio.Application.Blog.Services.BlogUploadFile(file.Length, file.FileName, file.OpenReadStream), userId));
        }
        catch (Exception ex) when (ex is FluentValidation.ValidationException || ex.GetType().Namespace == "QuinntyneBrownStudio.Domain.Exceptions.Blog")
        {
            return RedirectToPage(new { error = ex.Message });
        }
        return RedirectToPage(new { success = "Asset uploaded." });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (!IsAuthenticated()) return Redirect("/admin/login?returnUrl=" + Uri.EscapeDataString(Request.Path + Request.QueryString));
        try
        {
            await mediator.Send(new DeleteDigitalAssetCommand(id));
        }
        catch (Exception ex) when (ex is ConflictException or NotFoundException)
        {
            return RedirectToPage(new { error = ex.Message });
        }
        catch
        {
            return RedirectToPage(new { error = "An error occurred while deleting the asset." });
        }
        return RedirectToPage(new { success = "Asset deleted." });
    }


}
