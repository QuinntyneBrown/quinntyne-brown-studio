using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Qbs.Infrastructure.Persistence;

namespace Qbs.Api.Persistence;

public sealed class StudioDesignTimeFactory : IDesignTimeDbContextFactory<StudioDbContext>
{
    public StudioDbContext CreateDbContext(string[] args) =>
        new(
            new DbContextOptionsBuilder<StudioDbContext>()
                .UseSqlServer(
                    LocalDbConnection.Resolve(
                        new ConfigurationBuilder().AddEnvironmentVariables().AddCommandLine(args).Build(),
                        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production")
                )
                .Options
        );
}
