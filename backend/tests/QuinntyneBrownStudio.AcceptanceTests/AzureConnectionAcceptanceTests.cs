using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuinntyneBrownStudio.Infrastructure.DependencyInjection;
using QuinntyneBrownStudio.Infrastructure.Persistence;

namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class AzureConnectionAcceptanceTests
{
    // AC-AZ-01: Given an encrypted Azure SQL target and Entra authentication,
    // when the production host composes persistence, then it retains that target.
    [Theory]
    [InlineData("Active Directory Managed Identity")]
    [InlineData("Active Directory Default")]
    public void AC_AZ_01_Production_composition_accepts_Azure_SQL(string authentication)
    {
        var connection = $"Server=tcp:qbs.database.windows.net,1433;Database=studio;Authentication={authentication};Encrypt=True";
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStudio(Configuration(connection), false, "Production");
        using var provider = services.BuildServiceProvider();
        var actual = new SqlConnectionStringBuilder(provider.GetRequiredService<IOptions<StudioDatabaseOptions>>().Value.ConnectionString);
        Assert.Equal("studio", actual.InitialCatalog);
        Assert.Equal("tcp:qbs.database.windows.net,1433", actual.DataSource);
    }

    // AC-AZ-02: Given unsafe production settings, when persistence is configured,
    // then it fails without returning credentials or substituting a database.
    [Theory]
    [InlineData("Encrypt=False")]
    [InlineData("TrustServerCertificate=True")]
    [InlineData("Password=do-not-print-this")]
    [InlineData("Database=master")]
    [InlineData("AttachDBFilename=C:\\data.mdf")]
    [InlineData("Server=attacker.example")]
    [InlineData("Server=qbs.database.windows.net.attacker.example")]
    [InlineData("Authentication=Sql Password;User ID=sa")]
    [InlineData("Integrated Security=True")]
    public void AC_AZ_02_Unsafe_Azure_configuration_is_rejected(string overrideValue)
    {
        var connection = "Server=tcp:qbs.database.windows.net,1433;Database=studio;Authentication=Active Directory Managed Identity;Encrypt=True;" + overrideValue;
        var error = Assert.Throws<InvalidOperationException>(() => LocalDbConnection.Resolve(Configuration(connection), "Production"));
        Assert.DoesNotContain("do-not-print-this", error.ToString());
    }

    // AC-AZ-01: Given an Azure SQL target and a host that owns no LocalDB instance,
    // when migration runs, then it reaches the server instead of refusing the host.
    [Fact]
    public async Task AC_AZ_01_Azure_SQL_migration_is_never_refused_for_the_host_operating_system()
    {
        // A server name that resolves nowhere fails at the network, never at a host check.
        var connection = $"Server=tcp:qbs-{Guid.NewGuid():N}.database.windows.net,1433;Database=studio;"
            + "Authentication=Active Directory Managed Identity;Encrypt=True;Connect Timeout=1";
        await using var db = new StudioDbContext(
            new DbContextOptionsBuilder<StudioDbContext>().UseSqlServer(connection).Options);
        var error = await Record.ExceptionAsync(() => new StudioDatabase(db).Migrate(CancellationToken.None));
        Assert.NotNull(error);
        Assert.DoesNotContain("requires Windows", error.ToString());
        Assert.DoesNotContain("LocalDB", error.ToString());
    }

    private static IConfiguration Configuration(string connection) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Studio"] = connection }).Build();
}
