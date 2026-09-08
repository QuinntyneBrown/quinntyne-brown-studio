using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace QuinntyneBrownStudio.Infrastructure.Persistence;

public static class LocalDbConnection
{
    public const string DevelopmentConnection = "Server=(localdb)\\MSSQLLocalDB;Database=QbsDevelopment;Integrated Security=true;Encrypt=true;TrustServerCertificate=true";

    public static string Resolve(IConfiguration configuration, string environment)
    {
        var value = configuration.GetConnectionString("Studio");
        if (string.IsNullOrWhiteSpace(value) && environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            value = DevelopmentConnection;
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("ConnectionStrings:Studio is required. Configure LocalDB with integrated authentication or Azure SQL with Entra authentication.");
        SqlConnectionStringBuilder connection;
        try { connection = new(value); }
        catch (ArgumentException)
        {
            throw new InvalidOperationException("ConnectionStrings:Studio is not a valid LocalDB or Azure SQL connection string.");
        }
        const string prefix = "(localdb)\\";
        var local = connection.DataSource.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(connection.DataSource[prefix.Length..])
            && connection.IntegratedSecurity
            && string.IsNullOrEmpty(connection.UserID)
            && connection.Authentication == SqlAuthenticationMethod.NotSpecified;
        var azure = Regex.IsMatch(connection.DataSource,
                @"\A(tcp:)?[a-z0-9][a-z0-9-]*\.database\.windows\.net(,1433)?\z", RegexOptions.IgnoreCase)
            && !connection.IntegratedSecurity
            && !connection.TrustServerCertificate
            && connection.Encrypt != SqlConnectionEncryptOption.Optional
            && string.IsNullOrEmpty(connection.HostNameInCertificate)
            && (connection.Authentication is SqlAuthenticationMethod.ActiveDirectoryManagedIdentity or SqlAuthenticationMethod.ActiveDirectoryDefault)
            && (string.IsNullOrEmpty(connection.UserID) || Guid.TryParse(connection.UserID, out _));
        if ((!local && !azure)
            || string.IsNullOrWhiteSpace(connection.InitialCatalog)
            || connection.InitialCatalog.Equals("master", StringComparison.OrdinalIgnoreCase)
            || connection.InitialCatalog.Equals("model", StringComparison.OrdinalIgnoreCase)
            || connection.InitialCatalog.Equals("msdb", StringComparison.OrdinalIgnoreCase)
            || connection.InitialCatalog.Equals("tempdb", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrEmpty(connection.Password)
            || !string.IsNullOrEmpty(connection.AttachDBFilename)
            || connection.UserInstance
            || !string.IsNullOrEmpty(connection.FailoverPartner))
            throw new InvalidOperationException("ConnectionStrings:Studio requires a named LocalDB instance with integrated authentication or encrypted Azure SQL with Entra authentication, and an explicit application database. SQL passwords, attached files and other servers are unsupported.");
        return connection.ConnectionString;
    }
}
