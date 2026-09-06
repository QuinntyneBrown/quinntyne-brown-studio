using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Qbs.Infrastructure.Persistence;

public sealed class StudioDatabase(StudioDbContext db) : IStudioDatabase
{
    public async Task Verify(CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            throw new InvalidOperationException("LocalDB requires Windows. Run the API and worker under the owning Windows account on the same host.");
        if (!await db.Database.CanConnectAsync(cancellationToken))
            throw new InvalidOperationException("The configured LocalDB database is inaccessible. Check LocalDB installation, instance, and Windows account; run Qbs.Api --migrate explicitly if the application database does not exist.");
        try
        {
            if ((await db.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
                throw new InvalidOperationException("LocalDB has unapplied migrations. Back up the database and run Qbs.Api --migrate before starting the API or worker.");
        }
        catch (SqlException)
        {
            throw new InvalidOperationException("Cannot verify the LocalDB schema. Check database access and run Qbs.Api --migrate explicitly before starting.");
        }
    }

    public async Task Migrate(CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            throw new InvalidOperationException("LocalDB migration requires Windows and the instance owner's account.");
        try { await db.Database.MigrateAsync(cancellationToken); }
        catch (SqlException)
        {
            throw new InvalidOperationException("LocalDB migration failed. Check the installed instance, Windows account, database permissions, and existing schema. No database has been replaced.");
        }
    }
}
