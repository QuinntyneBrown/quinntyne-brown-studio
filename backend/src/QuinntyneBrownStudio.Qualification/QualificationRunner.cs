using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Infrastructure.Adapters;
using QuinntyneBrownStudio.Infrastructure.Serialization;
namespace QuinntyneBrownStudio.Qualification;

public sealed class QualificationRunner(IConfiguration configuration)
{
    public async Task<int> Run(string[] args, CancellationToken ct)
    {
        const string usage = "QuinntyneBrownStudio.Qualification raw|ai|upload|environment --manifest <json> --report <json>. Set adapter configuration through environment variables. Reports never close evidence gates.";
        if (args.Length == 1 && args[0] is "--help" or "-h") { Console.WriteLine(usage); return 0; }
        if (args.Length != 5 || args[1] != "--manifest" || args[3] != "--report") { Console.Error.WriteLine(usage); return 2; }
        var reportPath = Path.GetFullPath(args[4]);
        var gate = args[0] switch { "raw" => "G-RAW", "ai" => "G-AI", "upload" => "G-UPLOAD", "environment" => "G-ENV", _ => "Unknown" };
        QualificationReport report;
        try
        {
            var input = Path.GetFullPath(args[2]);
            if (input.Equals(reportPath, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Manifest and report must be different files.");
            var manifest = JsonSerializer.Deserialize<JsonElement>(await File.ReadAllTextAsync(input, ct));
            var registrations = new ServiceCollection();
            registrations.AddSingleton(configuration);
            registrations.AddSingleton<Azure.Core.TokenCredential, Azure.Identity.DefaultAzureCredential>();
            registrations.AddSingleton<IRawPreviewConverter, RawPreviewConverter>();
            registrations.AddHttpClient<IPhotoAnalysisService, AzurePhotoAnalysis>();
            registrations.AddHttpClient<EnvironmentQualificationCheck>();
            registrations.AddTransient<RawQualificationCheck>();
            registrations.AddTransient<AiQualificationCheck>();
            registrations.AddTransient<UploadQualificationCheck>();
            await using var services = registrations.BuildServiceProvider();
            IQualificationCheck check = args[0] switch
            {
                "raw" => services.GetRequiredService<RawQualificationCheck>(),
                "ai" => services.GetRequiredService<AiQualificationCheck>(),
                "upload" => services.GetRequiredService<UploadQualificationCheck>(),
                "environment" => services.GetRequiredService<EnvironmentQualificationCheck>(),
                _ => throw new ArgumentException(usage),
            };
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
            report = await check.Run(manifest, Path.GetDirectoryName(input)!, reportPath, ct);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            report = new(gate, "Blocked", error.Message, []);
        }
        catch (OperationCanceledException) { report = new(gate, "Blocked", "The run was cancelled. No qualification was recorded.", []); }
        // Never overwrite the supplied evidence manifest, including on an invalid invocation.
        if (Path.GetFullPath(args[2]).Equals(reportPath, StringComparison.OrdinalIgnoreCase)) { Console.Error.WriteLine(report.Message); return 2; }
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, new JsonSerializerOptions(StudioJson.Options) { WriteIndented = true }), CancellationToken.None);
        Console.WriteLine($"{report.Gate}: {report.Status}. Report: {reportPath}");
        return report.Status == "Measured" ? 0 : report.Status == "Failed" ? 1 : 2;
    }
}
