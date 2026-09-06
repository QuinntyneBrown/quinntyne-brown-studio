using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
namespace QuinntyneBrownStudio.Qualification;

public sealed class UploadQualificationCheck(IConfiguration configuration) : IQualificationCheck
{
    public async Task<QualificationReport> Run(JsonElement manifest, string directory, string reportPath, CancellationToken ct)
    {
        _ = QualificationInput.Items(manifest, "fixtures");
        _ = QualificationInput.Text(manifest, "origin");
        _ = QualificationInput.Text(manifest, "sessionId");
        _ = QualificationInput.Text(manifest, "playwrightModule");
        var inputPath = reportPath + ".input.json";
        await File.WriteAllTextAsync(inputPath, manifest.GetRawText(), ct);
        var start = new ProcessStartInfo(configuration["Qualification:NodeExecutable"] ?? "node") { UseShellExecute = false, CreateNoWindow = true };
        start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "upload-qualification.mjs"));
        start.ArgumentList.Add(inputPath); start.ArgumentList.Add(directory); start.ArgumentList.Add(reportPath + ".browser.json");
        using var process = Process.Start(start) ?? throw new InvalidOperationException("The browser qualification runner could not start.");
        try { await process.WaitForExitAsync(ct); }
        catch { if (!process.HasExited) process.Kill(entireProcessTree: true); throw; }
        var path = reportPath + ".browser.json";
        if (!File.Exists(path)) throw new InvalidOperationException("The browser runner did not produce its measurement report. Check the Node and Playwright installation.");
        var result = JsonSerializer.Deserialize<QualificationReport>(await File.ReadAllTextAsync(path, ct), new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new InvalidDataException("Invalid browser report.");
        return result;
    }
}
