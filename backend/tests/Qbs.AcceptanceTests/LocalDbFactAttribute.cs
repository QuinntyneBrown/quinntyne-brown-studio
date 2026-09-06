namespace Qbs.AcceptanceTests;

public sealed class LocalDbFactAttribute : FactAttribute
{
    public LocalDbFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
            Skip = "Runtime persistence acceptance requires Windows and LocalDB.";
    }
}
