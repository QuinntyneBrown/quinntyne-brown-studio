using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Qbs.Infrastructure.Persistence;

namespace Qbs.Api.Persistence;

public sealed class StudioDesignTimeFactory : IDesignTimeDbContextFactory<StudioDbContext>
{
    public StudioDbContext CreateDbContext(string[] args) =>
        new(
            new DbContextOptionsBuilder<StudioDbContext>()
                .UseSqlServer(
                    Environment.GetEnvironmentVariable("QBS_SQL")
                        ?? "Server=(localdb)\\MSSQLLocalDB;Database=QbsDesign;Integrated Security=true;TrustServerCertificate=true"
                )
                .Options
        );
}
