using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Infrastructure.Persistence;
namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class LocalDbWorkflowAcceptanceTests
{
    // Given one available photographer, when two HTTP requests concurrently assign
    // overlapping sessions, then LocalDB commits one and rejects the other.
    [SqlFact]
    public Task AC_L2_058_01_LocalDB_serializes_concurrent_session_assignments() => WithDatabase(async factory =>
    {
        using var admin = await factory.Actor();
        var personResponse = await admin.PostAsJsonAsync("/api/admin/photographers", new { name = "Photographer", active = true });
        personResponse.EnsureSuccessStatusCode();
        var person = (await personResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        (await admin.PutAsJsonAsync($"/api/admin/photographers/{person}/schedule", new { workingWindows = new[] { new { startsAt = "2027-06-01T09:00:00-04:00", endsAt = "2027-06-01T17:00:00-04:00" } }, unavailableWindows = Array.Empty<object>(), buffers = new { before = 30, after = 30 }, expectedVersion = 0 })).EnsureSuccessStatusCode();
        var responses = await Task.WhenAll(Enumerable.Range(1, 2).Select(index => admin.PostAsJsonAsync("/api/admin/sessions", new { name = "Competing session " + index, service = "Wedding", startsAt = "2027-06-01T10:00:00-04:00", endsAt = "2027-06-01T12:00:00-04:00", photographerId = person })));
        Assert.Single(responses, response => response.IsSuccessStatusCode);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        Assert.Single((await admin.GetFromJsonAsync<JsonElement[]>("/api/admin/sessions"))!);
    });

    // Given one canonical client request, when identical HTTP submissions race,
    // then one immutable LocalDB snapshot and one stable identifier are returned.
    [SqlFact]
    public Task AC_L2_064_01_LocalDB_deduplicates_simultaneous_print_submissions() => WithDatabase(async factory =>
    {
        var id = Guid.NewGuid(); var photo = await PrivatePhotoFixture.Seed(factory, id);
        using var admin = await factory.Actor(); using var client = await factory.Actor("Client", id.ToString());
        var optionResponse = await admin.PostAsJsonAsync("/api/admin/print-options", new { name = "Archival portrait", dimensions = "8x10", finish = "Matte", unitPrice = "25.55", enabled = true });
        optionResponse.EnsureSuccessStatusCode(); var option = (await optionResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var body = new { idempotencyKey = "same-request", lines = new[] { new { photoId = photo.Id, optionId = option, optionRevision = 1, quantity = 3 } }, notes = "Discuss framing." };
        var responses = await Task.WhenAll(client.PostAsJsonAsync("/api/client/print-requests", body), client.PostAsJsonAsync("/api/client/print-requests", body));
        foreach (var response in responses) response.EnsureSuccessStatusCode();
        var first = await responses[0].Content.ReadFromJsonAsync<JsonElement>(); var second = await responses[1].Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(first.GetProperty("id").GetGuid(), second.GetProperty("id").GetGuid()); Assert.Equal("76.65", first.GetProperty("total").GetString());
        Assert.Single((await admin.GetFromJsonAsync<JsonElement[]>("/api/admin/print-requests"))!);
    });

    private static async Task WithDatabase(Func<StudioFactory, Task> verify)
    {
        var connection = new SqlConnectionStringBuilder("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true") { InitialCatalog = "QbsWorkflowTest_" + Guid.NewGuid().ToString("N") };
        var options = new DbContextOptionsBuilder<StudioDbContext>().UseSqlServer(connection.ConnectionString).Options;
        await using var database = new StudioDbContext(options);
        try
        {
            await database.Database.MigrateAsync();
            using var factory = new StudioFactory { ConfigurePersistence = services => { services.AddScoped(_ => new StudioDbContext(options)); services.AddScoped<IStudioStore, SqlStudioStore>(); } };
            await verify(factory);
        }
        finally
        {
            if (!connection.InitialCatalog.StartsWith("QbsWorkflowTest_", StringComparison.Ordinal)) throw new InvalidOperationException("Refusing to remove a non-test database.");
            await database.Database.EnsureDeletedAsync();
        }
    }
}
