using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuinntyneBrownStudio.Infrastructure.Persistence;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class LocalDbTestDatabase : IAsyncDisposable
{
    private readonly string name = "QbsPersistenceTest_" + Guid.NewGuid().ToString("N");
    public string ConnectionString => $"Server=(localdb)\\MSSQLLocalDB;Database={name};Integrated Security=true;TrustServerCertificate=true;Connect Timeout=5";
    public StudioDbContext Open() => new(new DbContextOptionsBuilder<StudioDbContext>().UseSqlServer(ConnectionString).Options);

    public async Task Migrate()
    {
        await using var db = Open();
        await db.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (!name.StartsWith("QbsPersistenceTest_", StringComparison.Ordinal))
            throw new InvalidOperationException("Refusing to delete a non-test database.");
        await using var db = Open();
        await db.Database.EnsureDeletedAsync();
    }
}
