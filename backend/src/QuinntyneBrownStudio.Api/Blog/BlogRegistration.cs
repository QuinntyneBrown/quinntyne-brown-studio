using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuinntyneBrownStudio.Application.Blog.Behaviors;
using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Infrastructure.Persistence.Blog;
using QuinntyneBrownStudio.Infrastructure.Processing.Blog;
using QuinntyneBrownStudio.Infrastructure.Storage.Blog;
using QuinntyneBrownStudio.Api.Blog.Services;
using System.Threading.RateLimiting;

namespace QuinntyneBrownStudio.Api.Blog;

public static class BlogRegistration
{
    public static IServiceCollection AddBlog(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddOptions<BlogStorageOptions>().Configure(o =>
        {
            o.Path = configuration["Blog:StoragePath"] ??
                (environment.IsDevelopment() || environment.IsEnvironment("Testing")
                    ? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuinntyneBrownStudio", "BlogMedia")
                    : "");
        }).Validate(o => System.IO.Path.IsPathFullyQualified(o.Path), "Blog:StoragePath must be an absolute persistent directory.").ValidateOnStart();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IDigitalAssetRepository, DigitalAssetRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IAssetStorage, LocalFileAssetStorage>();
        services.AddSingleton<IImageVariantGenerator, ImageVariantGenerator>();
        services.AddSingleton<ISlugGenerator, SlugGenerator>();
        services.AddSingleton<IMarkdownConverter, MarkdownConverter>();
        services.AddSingleton<IReadingTimeCalculator, ReadingTimeCalculator>();
        services.AddSingleton<ISearchHighlighter, SearchHighlighter>();
        services.AddSingleton<IETagGenerator, ETagGenerator>();
        services.AddSingleton<IContentHashService, ContentHashService>();
        services.AddMemoryCache();
        services.AddValidatorsFromAssemblyContaining<ValidationBehavior<object, object>>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddAuthorization(o => o.AddPolicy("BlogAdministrator", policy => policy.RequireRole("Administrator")));
        services.ConfigureApplicationCookie(o =>
        {
            o.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/blog/admin"))
                    context.Response.Redirect("/admin/login?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
                else context.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
        });
        services.AddRazorPages(o =>
        {
            o.Conventions.AuthorizeFolder("/Admin", "BlogAdministrator");
        });
        services.Configure<MvcOptions>(o => o.CacheProfiles.Add("HtmlPage", new CacheProfile { NoStore = true, Location = ResponseCacheLocation.None }));
        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = 429;
            o.AddPolicy("blog-writes", context => RateLimitPartition.GetFixedWindowLimiter(
                context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 60, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });
        services.AddResponseCompression();
        return services;
    }
}
