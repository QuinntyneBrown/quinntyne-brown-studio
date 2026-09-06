using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Qbs.Application.Ports;
using Qbs.Domain.Models;

namespace Qbs.Infrastructure.Identity;

public sealed class AccountContext(IHttpContextAccessor accessor, IAntiforgery antiforgery) : IAccountContext
{
    private HttpContext Context => accessor.HttpContext ?? throw new InvalidOperationException("An HTTP request is required.");
    public AccountSession Session() => new(Context.User.Identity?.IsAuthenticated ?? false, Context.User.FindFirstValue(ClaimTypes.NameIdentifier), Context.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray());
    public AntiforgeryToken Antiforgery() => new(antiforgery.GetAndStoreTokens(Context).RequestToken);
}
