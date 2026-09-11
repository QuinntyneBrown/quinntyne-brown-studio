using QuinntyneBrownStudio.Api.Blog.Services;

namespace QuinntyneBrownStudio.Api.Blog.Middleware;

public class ContentHashRewriteMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IContentHashService _hashService;

    public ContentHashRewriteMiddleware(RequestDelegate next, IContentHashService hashService)
    {
        _next = next;
        _hashService = hashService;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (path is not null)
        {
            var resolved = _hashService.ResolveHashedPath(path);
            if (resolved is not null)
            {
                context.Request.Path = resolved;
            }
        }

        return _next(context);
    }
}
