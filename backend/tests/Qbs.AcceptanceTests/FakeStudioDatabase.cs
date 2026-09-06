using Microsoft.EntityFrameworkCore;
using Qbs.Infrastructure.Persistence;

namespace Qbs.AcceptanceTests;

public sealed class FakeStudioDatabase(StudioDbContext db) : IStudioDatabase
{
    public async Task Verify(CancellationToken cancellationToken = default) =>
        await db.Database.EnsureCreatedAsync(cancellationToken);

    public Task Migrate(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Test fakes cannot run migrations. Use the LocalDB acceptance fixture.");
}
