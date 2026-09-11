using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace QuinntyneBrownStudio.Api.Pages.Admin;

public abstract class AdminPageModelBase : PageModel
{
    protected bool IsAuthenticated() => User.Identity?.IsAuthenticated == true && User.IsInRole("Administrator");
    protected Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    protected string? GetCurrentUserEmail() => User.FindFirstValue(ClaimTypes.Email);
    protected string? GetCurrentUserDisplayName() => User.Identity?.Name;
}
