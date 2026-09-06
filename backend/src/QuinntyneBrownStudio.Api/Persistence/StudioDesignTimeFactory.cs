using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using QuinntyneBrownStudio.Infrastructure.Persistence;

namespace QuinntyneBrownStudio.Api.Persistence;

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
