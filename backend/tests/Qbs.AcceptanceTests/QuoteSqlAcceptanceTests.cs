using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qbs.Application.Ports;
using Qbs.Infrastructure.Persistence;

namespace Qbs.AcceptanceTests;

public sealed class QuoteSqlAcceptanceTests
{
    [SqlFact]
    public async Task Q08_AC_L2_055_01_Quote_uses_configuration_after_reopening_the_SQL_backed_API()
    {
        var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("QBS_SQL")
            ?? "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true")
        { InitialCatalog = "QbsQuoteTest_" + Guid.NewGuid().ToString("N") };
        var options = new DbContextOptionsBuilder<StudioDbContext>().UseSqlServer(connection.ConnectionString).Options;
        await using var setup = new StudioDbContext(options);
        try
        {
            await setup.Database.MigrateAsync();
            using var root = new StudioFactory();
            void Configure(IWebHostBuilder builder) => builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => new StudioDbContext(options));
                services.AddScoped<IStudioStore, SqlStudioStore>();
                services.AddSingleton<IClock>(new QuoteClock());
            });
            using (var first = root.WithWebHostBuilder(Configure))
            using (var admin = await LiveQuoteAcceptanceTests.Actor(first))
            {
                (await admin.PostAsJsonAsync("/api/admin/studios", new { name = "Persistent base", resolvedAddress = new { label = "Base", latitude = 43.6, longitude = -79.4 }, hourlyFee = "20", enabled = true, isBase = true })).EnsureSuccessStatusCode();
                (await admin.PutAsJsonAsync("/api/admin/rates", new { serviceRates = new { Wedding = "120" }, costRates = new { travel = "2", equipment = "0", assistant = "0", lunch = "0" }, expectedVersion = 0 })).EnsureSuccessStatusCode();
            }
            using (var reopened = root.WithWebHostBuilder(Configure))
            using (var visitor = await LiveQuoteAcceptanceTests.Actor(reopened))
            {
                var studios = await visitor.GetFromJsonAsync<JsonElement[]>("/api/public/studios");
                Assert.Equal("Persistent base", Assert.Single(studios!).GetProperty("name").GetString());
                var response = await visitor.PostAsJsonAsync("/api/public/quotes/calculate", new
                {
                    service = "Wedding",
                    startsAt = "2027-06-01T10:00:00-04:00",
                    endsAt = "2027-06-01T11:00:00-04:00",
                    locations = new[] { new { location = new { label = "Venue", latitude = 43.7, longitude = -79.3 }, parkingAmount = "0" } },
                    inputRevision = 4,
                });
                response.EnsureSuccessStatusCode();
                Assert.Equal("144.00", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("total").GetProperty("amount").GetString());
            }
        }
        finally
        {
            if (!connection.InitialCatalog.StartsWith("QbsQuoteTest_", StringComparison.Ordinal)) throw new InvalidOperationException("Refusing to remove a non-test database.");
            await setup.Database.EnsureDeletedAsync();
        }
    }
}
