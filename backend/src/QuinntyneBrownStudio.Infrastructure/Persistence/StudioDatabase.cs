using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace QuinntyneBrownStudio.Infrastructure.Persistence;

public sealed class StudioDatabase(StudioDbContext db) : IStudioDatabase
{
    // Only a LocalDB user instance is tied to Windows and its owning account. An Azure SQL
    // server is reachable from any supported host, including the Linux production VM.
    private bool LocalDb => LocalDbConnection.IsLocalDb(db.Database.GetConnectionString());

    private string Target => LocalDb ? "LocalDB" : "Azure SQL";

    public async Task Verify(CancellationToken cancellationToken = default)
    {
        if (LocalDb && !OperatingSystem.IsWindows())
            throw new InvalidOperationException("LocalDB requires Windows. Run the API and worker under the owning Windows account on the same host.");
        if (!await db.Database.CanConnectAsync(cancellationToken))
            throw new InvalidOperationException($"The configured {Target} database is inaccessible. Check the instance or server, the account it authenticates as, and network access; run QuinntyneBrownStudio.Api --migrate explicitly if the application database does not exist.");
        try
        {
            if ((await db.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
                throw new InvalidOperationException($"{Target} has unapplied migrations. Back up the database and run QuinntyneBrownStudio.Api --migrate before starting the API or worker.");
        }
        catch (SqlException)
        {
            throw new InvalidOperationException($"Cannot verify the {Target} schema. Check database access and run QuinntyneBrownStudio.Api --migrate explicitly before starting.");
        }
    }

    public async Task Migrate(CancellationToken cancellationToken = default)
    {
        if (LocalDb && !OperatingSystem.IsWindows())
            throw new InvalidOperationException("LocalDB migration requires Windows and the instance owner's account.");
        try { await db.Database.MigrateAsync(cancellationToken); }
        catch (SqlException)
        {
            throw new InvalidOperationException($"{Target} migration failed. Check the instance or server, the account it authenticates as, database permissions, and existing schema. No database has been replaced.");
        }
    }
}
