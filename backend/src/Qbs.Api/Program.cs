using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qbs.Api.Filters;
using Qbs.Domain.Exceptions;
using Qbs.Infrastructure.DependencyInjection;
using Qbs.Infrastructure.Persistence;
using Qbs.Infrastructure.Processing;
using Qbs.Infrastructure.Serialization;

var builder = WebApplication.CreateBuilder(args);
var controlled =
    builder.Environment.IsEnvironment("Testing")
    || (
        builder.Environment.IsDevelopment()
        && builder.Configuration.GetValue<bool>("Development:Controlled")
    );
builder.Services.AddStudio(builder.Configuration, controlled, builder.Environment.EnvironmentName);
if (
    !controlled
    && !string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"])
)
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
builder.Services.AddScoped<JobProcessor>();
if (controlled && !builder.Environment.IsEnvironment("Testing"))
    builder.Services.AddHostedService<ProcessingService>();
builder
    .Services.AddControllers(o => o.Filters.Add<Qbs.Api.Filters.AntiforgeryFilter>())
    .AddJsonOptions(o =>
    {
        foreach (var c in StudioJson.Options.Converters)
            o.JsonSerializerOptions.Converters.Add(c);
    });
builder.Services.AddAntiforgery(o =>
{
    o.HeaderName = "X-XSRF-TOKEN";
    o.Cookie.Name = "__Host-qbs-xsrf";
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.Path = "/";
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();
var app = builder.Build();
app.Use(
    async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (Exception ex)
            when (ex
                    is StudioException
                        or OverflowException
                        or FormatException
                        or HttpRequestException
                        or Azure.RequestFailedException
                        or Azure.Identity.AuthenticationFailedException
                        or Microsoft.Data.SqlClient.SqlException
            )
        {
            var error = ex as StudioException;
            var status =
                error?.Status
                ?? (
                    ex
                        is HttpRequestException
                            or Azure.RequestFailedException
                            or Azure.Identity.AuthenticationFailedException
                            or Microsoft.Data.SqlClient.SqlException
                        ? 503
                        : 400
                );
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = status,
                    Title =
                        error?.Message
                        ?? (
                            status == 503
                                ? "A required service is temporarily unavailable."
                                : "A numeric or date input is invalid."
                        ),
                    Extensions =
                    {
                        ["errors"] = new Dictionary<string, string[]>
                        {
                            { error?.Field ?? "request", [error?.Message ?? "Invalid value."] },
                        },
                    },
                }
            );
        }
    }
);
var forwarded = new Microsoft.AspNetCore.Builder.ForwardedHeadersOptions
{
    ForwardedHeaders =
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
        | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto,
};
if (builder.Configuration.GetValue<bool>("Gateway:TrustForwardedHeaders"))
{
    forwarded.KnownIPNetworks.Clear();
    forwarded.KnownProxies.Clear();
}
app.UseForwardedHeaders(forwarded);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
await using (var scope = app.Services.CreateAsyncScope())
{
    var database = scope.ServiceProvider.GetRequiredService<IStudioDatabase>();
    if (args.Contains("--migrate"))
    {
        await database.Migrate();
        return;
    }
    await database.Verify();
    if (controlled || args.Contains("--provision-admin"))
    {
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var role in new[] { "Administrator", "Client" })
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new(role));
        if (
            args.Contains("--provision-admin")
            || controlled && builder.Configuration["Bootstrap:Email"] != null
        )
        {
            var email =
                builder.Configuration["Bootstrap:Email"]
                ?? throw new InvalidOperationException("Bootstrap:Email is required.");
            var password =
                builder.Configuration["Bootstrap:Password"]
                ?? throw new InvalidOperationException("Bootstrap:Password is required.");
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();
            if (await users.FindByEmailAsync(email) == null)
            {
                var user = new IdentityUser<Guid>
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                };
                var result = await users.CreateAsync(user, password);
                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        string.Join(" ", result.Errors.Select(x => x.Description))
                    );
                await users.AddToRoleAsync(user, "Administrator");
            }
        }
        if (args.Contains("--provision-admin"))
            return;
    }
}
app.Run();

public partial class Program { }
