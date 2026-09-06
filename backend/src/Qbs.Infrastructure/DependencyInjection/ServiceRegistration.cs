using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Qbs.Application.Catalog;
using Qbs.Application.Clients;
using Qbs.Application.Photos;
using Qbs.Application.Ports;
using Qbs.Application.Quotations;
using Qbs.Infrastructure.Adapters;
using Qbs.Infrastructure.Identity;
using Qbs.Infrastructure.Persistence;
using Qbs.Infrastructure.Storage;
using PresentationService = Qbs.Application.Presentation.Presentation;
using SchedulingService = Qbs.Application.Scheduling.Scheduling;

namespace Qbs.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddStudio(
        this IServiceCollection services,
        IConfiguration config,
        bool controlled,
        string environment = "Production"
    )
    {
        services.Configure<StudioDatabaseOptions>(o => o.ConnectionString = LocalDbConnection.Resolve(config, environment));
        services.AddDbContext<StudioDbContext>((sp, o) =>
            o.UseSqlServer(sp.GetRequiredService<IOptions<StudioDatabaseOptions>>().Value.ConnectionString));
        services.AddScoped<IStudioStore, SqlStudioStore>();
        services.AddScoped<IStudioDatabase, StudioDatabase>();
        if (controlled)
        {
            services.AddSingleton<IPhotoStorage, FilePhotoStorage>();
            services.AddSingleton<IRouteDistanceService, ControlledRoutes>();
            services.AddSingleton<IPhotoAnalysisService, ControlledAnalysis>();
            services.AddSingleton<IEmailSender, ControlledEmail>();
            services.AddSingleton<IJobQueue, MemoryJobQueue>();
        }
        else
        {
            services.AddSingleton<IPhotoStorage, AzurePhotoStorage>();
            services.Configure<AzureMapsOptions>(config.GetSection("Azure"));
            services.AddSingleton<Azure.Core.TokenCredential, Azure.Identity.DefaultAzureCredential>();
            services.AddHttpClient<IRouteDistanceService, AzureMapsRoutes>(client => client.Timeout = TimeSpan.FromSeconds(20));
            services.AddHttpClient<IPhotoAnalysisService, AzurePhotoAnalysis>();
            services.AddSingleton<IEmailSender, AzureEmailSender>();
            services.AddSingleton<IJobQueue, AzureJobQueue>();
        }
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IRawPreviewConverter, RawPreviewConverter>();
        var keys = services.AddDataProtection().SetApplicationName("QuinntyneBrownStudio");
        if (config["DataProtection:BlobUri"] is { } blob)
            keys.PersistKeysToAzureBlobStorage(
                new Uri(blob),
                new Azure.Identity.DefaultAzureCredential()
            );
        if (config["DataProtection:KeyUri"] is { } key)
            keys.ProtectKeysWithAzureKeyVault(
                new Uri(key),
                new Azure.Identity.DefaultAzureCredential()
            );
        if (config["DataProtection:Directory"] is { } directory)
            keys.PersistKeysToFileSystem(new DirectoryInfo(directory));
        services
            .AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(o =>
            {
                o.Password.RequiredLength = 8;
                o.User.RequireUniqueEmail = true;
                o.SignIn.RequireConfirmedEmail = true;
                o.Lockout.MaxFailedAccessAttempts = 5;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<StudioDbContext>()
            .AddDefaultTokenProviders();
        services.Configure<SecurityStampValidatorOptions>(o =>
            o.ValidationInterval = TimeSpan.Zero
        );
        services.ConfigureApplicationCookie(o =>
        {
            o.Cookie.Name = "__Host-qbs";
            o.Cookie.HttpOnly = true;
            o.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            o.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            o.Cookie.Path = "/";
            o.Events.OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
            o.Events.OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = 403;
                return Task.CompletedTask;
            };
        });
        services.AddScoped<IdentityAccounts>();
        services.AddScoped<IIdentityAccounts>(sp => sp.GetRequiredService<IdentityAccounts>());
        services.AddScoped<AdminCatalog>();
        services.AddScoped<SchedulingService>();
        services.AddScoped<PresentationService>();
        services.AddScoped<ClientWorkflows>();
        services.AddScoped<PhotoWorkflows>();
        services.AddScoped<AnalysisWorkflows>();
        services.AddScoped<RetentionWorkflows>();
        services.AddMediatR(o => o.RegisterServicesFromAssemblyContaining<CalculateQuote>());
        return services;
    }
}
