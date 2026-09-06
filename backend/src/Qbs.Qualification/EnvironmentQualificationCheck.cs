using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Qbs.Infrastructure.Persistence;
namespace Qbs.Qualification;

public sealed class EnvironmentQualificationCheck(IConfiguration configuration, HttpClient http) : IQualificationCheck
{
    public async Task<QualificationReport> Run(JsonElement manifest, string directory, string reportPath, CancellationToken ct)
    {
        var origin = new Uri(QualificationInput.Text(manifest, "origin"));
        var expectedDatabase = QualificationInput.Text(manifest, "expectedDatabase");
        var evidence = QualificationInput.Items(manifest, "evidence");
        foreach (var required in new[] { "backup-restore", "identity-storage-isolation", "tls", "monitoring", "azure-roles", "email-sender" })
            if (!evidence.Any(item => QualificationInput.Text(item, "kind") == required)) throw new ArgumentException($"Supply the '{required}' operator evidence file and SHA-256 digest.");
        if (origin.Scheme != "https" || !string.IsNullOrEmpty(origin.UserInfo)) throw new ArgumentException("Supply a credential-free HTTPS product origin.");
        var connection = LocalDbConnection.Resolve(configuration, "Production");
        if (new SqlConnectionStringBuilder(connection).InitialCatalog != expectedDatabase) throw new ArgumentException("The configured LocalDB database differs from expectedDatabase.");
        var observations = new List<QualificationObservation>();
        await using var database = new StudioDbContext(new DbContextOptionsBuilder<StudioDbContext>().UseSqlServer(connection).Options);
        await new StudioDatabase(database).Verify(ct);
        observations.Add(new("LocalDB schema", true, "Existing database is accessible with no pending migrations. No migration or write was performed.", new() { ["database"] = expectedDatabase, ["migrations"] = (await database.Database.GetAppliedMigrationsAsync(ct)).ToArray() }));
        using var response = await http.GetAsync(new Uri(origin, "/api/health"), ct);
        observations.Add(new("HTTPS API", response.IsSuccessStatusCode, "TLS validation remained enabled.", new() { ["origin"] = origin.GetLeftPart(UriPartial.Authority), ["status"] = (int)response.StatusCode }));
        foreach (var item in evidence)
        {
            var digest = await QualificationInput.VerifyFile(item, directory, ct);
            observations.Add(new(QualificationInput.Text(item, "kind"), true, "Operator evidence digest verified; review its contents before approving the environment.", new() { ["sha256"] = digest, ["path"] = QualificationInput.Text(item, "path") }));
        }
        return QualificationInput.Report("G-ENV", observations);
    }
}
