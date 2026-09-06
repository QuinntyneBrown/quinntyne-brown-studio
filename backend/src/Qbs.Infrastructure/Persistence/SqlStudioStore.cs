using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Infrastructure;

public sealed class SqlStudioStore(StudioDbContext db) : IStudioStore
{
    public async Task<T> Run<T>(
        string lockKey,
        Func<IStudioTransaction, Task<T>> action,
        CancellationToken ct = default
    )
    {
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            ct
        );
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"DECLARE @r int; EXEC @r=sp_getapplock @Resource={lockKey}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000; IF @r<0 THROW 51000, 'Resource is busy.',1;",
                ct
            );
            var result = await action(new SqlStudioTransaction(db));
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return result;
        }
        catch (DbUpdateException e)
            when (e.InnerException is Microsoft.Data.SqlClient.SqlException sql
                && sql.Number is 2601 or 2627
            )
        {
            throw new StudioException(409, "A record with this identifier already exists.");
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new StudioException(409, "This record changed. Reload before saving.");
        }
    }
}
