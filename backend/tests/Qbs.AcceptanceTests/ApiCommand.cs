using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Qbs.AcceptanceTests;

public static class ApiCommand
{
    public static async Task<BackendCommandResult> Run(string connection, string command)
    {
        var dotnet = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH")
            ?? Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", "..", "dotnet.exe"));
        var start = new ProcessStartInfo(dotnet)
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true,
            RedirectStandardError = true, WorkingDirectory = AppContext.BaseDirectory,
        };
        start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "Qbs.Api.dll"));
        start.ArgumentList.Add(command);
        start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        start.Environment["DOTNET_ENVIRONMENT"] = "Production";
        start.Environment["ConnectionStrings__Studio"] = connection;
        start.Environment["Bootstrap__Email"] = "localdb-acceptance@example.test";
        start.Environment["Bootstrap__Password"] = "Acceptance-only!1945";
        start.Environment["Development__Controlled"] = "false";
        start.Environment["ASPNETCORE_URLS"] = "http://127.0.0.1:0";
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Cannot start migration acceptance process.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(60)); }
        finally { if (!process.HasExited) { process.Kill(true); await process.WaitForExitAsync(); } }
        return new(process.ExitCode, await stdout + await stderr);
    }
}
