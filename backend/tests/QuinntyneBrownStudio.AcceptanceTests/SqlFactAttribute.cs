namespace QuinntyneBrownStudio.AcceptanceTests;

public sealed class SqlFactAttribute : FactAttribute
{
    public SqlFactAttribute()
    {
        if (
            !OperatingSystem.IsWindows()
            && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("QBS_SQL"))
        )
            Skip = "Set QBS_SQL to run SQL boundary checks outside Windows LocalDB.";
    }
}
