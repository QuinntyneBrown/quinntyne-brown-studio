using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Qbs.Domain.Entities;
using Qbs.Domain.Exceptions;
using Qbs.Infrastructure.Persistence;

namespace Qbs.AcceptanceTests;

[Trait("Layer", "SqlIntegration")]
public sealed class SqlStoreTests
{
    [SqlFact]
    public async Task Transaction_rolls_back_record_and_outbox_together()
    {
        await WithDatabase(async options =>
        {
            await using (var db = new StudioDbContext(options))
            {
                var store = new SqlStudioStore(db);
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    store.Run<bool>(
                        "transaction-check",
                        async tx =>
                        {
                            await tx.Save(new Equipment { Name = "Camera", Quantity = 1 }, 0);
                            await tx.Save(
                                new BackgroundJob { Kind = "Preview", ResourceId = Guid.NewGuid() },
                                0
                            );
                            throw new InvalidOperationException("Simulated pre-commit failure");
                        }
                    )
                );
            }
            await using var verify = new StudioDbContext(options);
            Assert.Empty(await verify.Records.ToArrayAsync());
        });
    }

    [SqlFact]
    public async Task Concurrent_stale_updates_allow_only_one_commit()
    {
        await WithDatabase(async options =>
        {
            var id = Guid.NewGuid();
            await using (var db = new StudioDbContext(options))
                await new SqlStudioStore(db).Run(
                    "equipment",
                    async tx =>
                    {
                        await tx.Save(
                            new Equipment
                            {
                                Id = id,
                                Name = "Camera",
                                Quantity = 1,
                            },
                            0
                        );
                        return true;
                    }
                );
            async Task<bool> Attempt(int quantity)
            {
                await using var db = new StudioDbContext(options);
                try
                {
                    return await new SqlStudioStore(db).Run(
                        "equipment",
                        async tx =>
                        {
                            await tx.Save(
                                new Equipment
                                {
                                    Id = id,
                                    Name = "Camera",
                                    Quantity = quantity,
                                },
                                1
                            );
                            return true;
                        }
                    );
                }
                catch (StudioException e) when (e.Status == 409)
                {
                    return false;
                }
            }
            var outcomes = await Task.WhenAll(Attempt(2), Attempt(3));
            Assert.Single(outcomes, x => x);
        });
    }

    [SqlFact]
    public async Task Database_rejects_duplicate_client_idempotency_keys()
    {
        await WithDatabase(async options =>
        {
            var client = Guid.NewGuid();
            await using (var db = new StudioDbContext(options))
                await new SqlStudioStore(db).Run(
                    "media",
                    async tx =>
                    {
                        await tx.Save(
                            new PrintRequest { ClientId = client, IdempotencyKey = "same-key" },
                            0
                        );
                        return true;
                    }
                );
            await using var other = new StudioDbContext(options);
            var error = await Assert.ThrowsAsync<StudioException>(() =>
                new SqlStudioStore(other).Run(
                    "media",
                    async tx =>
                    {
                        await tx.Save(
                            new PrintRequest { ClientId = client, IdempotencyKey = "same-key" },
                            0
                        );
                        return true;
                    }
                )
            );
            Assert.Equal(409, error.Status);
        });
    }

    private static async Task WithDatabase(Func<DbContextOptions<StudioDbContext>, Task> test)
    {
        var database = "QbsTest_" + Guid.NewGuid().ToString("N");
        var connection = new SqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable("QBS_SQL")
                ?? "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true"
        )
        {
            InitialCatalog = database,
        };
        var options = new DbContextOptionsBuilder<StudioDbContext>()
            .UseSqlServer(connection.ConnectionString)
            .Options;
        await using var setup = new StudioDbContext(options);
        try
        {
            await setup.Database.MigrateAsync();
            await test(options);
        }
        finally
        {
            if (!connection.InitialCatalog.StartsWith("QbsTest_", StringComparison.Ordinal))
                throw new InvalidOperationException("Refusing to remove a non-test database.");
            await setup.Database.EnsureDeletedAsync();
        }
    }
}
