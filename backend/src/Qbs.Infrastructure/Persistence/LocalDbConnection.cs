using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Qbs.Infrastructure.Persistence;

public static class LocalDbConnection
{
    public const string DevelopmentConnection = "Server=(localdb)\\MSSQLLocalDB;Database=QbsDevelopment;Integrated Security=true;Encrypt=true;TrustServerCertificate=true";

    public static string Resolve(IConfiguration configuration, string environment)
    {
        var value = configuration.GetConnectionString("Studio");
        if (string.IsNullOrWhiteSpace(value) && environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            value = DevelopmentConnection;
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("ConnectionStrings:Studio is required. Configure a named LocalDB database with Windows integrated authentication.");
        SqlConnectionStringBuilder connection;
        try { connection = new(value); }
        catch (ArgumentException)
        {
            throw new InvalidOperationException("ConnectionStrings:Studio is not a valid LocalDB connection string.");
        }
        const string prefix = "(localdb)\\";
        if (!connection.DataSource.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(connection.DataSource[prefix.Length..])
            || !connection.IntegratedSecurity
            || string.IsNullOrWhiteSpace(connection.InitialCatalog)
            || connection.InitialCatalog.Equals("master", StringComparison.OrdinalIgnoreCase)
            || connection.InitialCatalog.Equals("model", StringComparison.OrdinalIgnoreCase)
            || connection.InitialCatalog.Equals("msdb", StringComparison.OrdinalIgnoreCase)
            || connection.InitialCatalog.Equals("tempdb", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrEmpty(connection.UserID)
            || !string.IsNullOrEmpty(connection.Password)
            || !string.IsNullOrEmpty(connection.AttachDBFilename)
            || connection.UserInstance
            || connection.Authentication != SqlAuthenticationMethod.NotSpecified)
            throw new InvalidOperationException("ConnectionStrings:Studio must select a named LocalDB instance, an explicit application database, and Windows integrated authentication. SQL credentials, attached files, and remote SQL servers are unsupported.");
        return connection.ConnectionString;
    }
}
