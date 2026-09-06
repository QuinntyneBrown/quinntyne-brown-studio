using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuinntyneBrownStudio.Application.Ports;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class RuntimeStudioFactory(string environment, string? connection) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment);
        builder.UseSetting("Development:Controlled", "true");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Studio"] = connection,
            ["Development:Controlled"] = "true",
            ["Bootstrap:Email"] = null,
            ["Bootstrap:Password"] = null,
        }));
        // Only external boundaries are controlled. Persistence and identity use runtime composition.
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IRouteDistanceService>(new QuoteRoutes { Distance = 12000 });
            services.AddSingleton<IClock>(new QuoteClock());
        });
    }
}
