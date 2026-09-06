using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Infrastructure.Persistence;

namespace QuinntyneBrownStudio.AcceptanceTests;

public static class FakePersistenceRegistration
{
    public static void AddFakePersistence(this IServiceCollection services)
    {
        services.RemoveAll<StudioDbContext>();
        services.RemoveAll<DbContextOptions<StudioDbContext>>();
        services.RemoveAll<IDbContextOptionsConfiguration<StudioDbContext>>();
        services.RemoveAll<IStudioStore>();
        services.RemoveAll<IStudioDatabase>();
        var database = "qbs-acceptance-" + Guid.NewGuid();
        services.AddDbContext<StudioDbContext>(o => o.UseInMemoryDatabase(database));
        services.AddSingleton<IStudioStore, MemoryStudioStore>();
        services.AddScoped<IStudioDatabase, FakeStudioDatabase>();
    }
}
