using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using QuinntyneBrownStudio.Application.Ports;
namespace QuinntyneBrownStudio.Qualification;

public sealed class AiQualificationCheck(IPhotoAnalysisService analysis, IRawPreviewConverter converter, IConfiguration configuration) : IQualificationCheck
{
    public async Task<QualificationReport> Run(JsonElement manifest, string directory, string reportPath, CancellationToken ct)
    {
        var fixtures = QualificationInput.Items(manifest, "fixtures");
        foreach (var key in new[] { "Azure:AiEndpoint", "Azure:AiDeployment", "Azure:AiModelVersion" }) if (string.IsNullOrWhiteSpace(configuration[key])) throw new ArgumentException($"Configure {key} before running a real Azure evaluation.");
        _ = QualificationInput.Text(manifest, "approvedBy");
        if (!manifest.TryGetProperty("minimumAgreement", out var threshold) || !threshold.TryGetDecimal(out var minimum) || minimum < 0 || minimum > 1) throw new ArgumentException("Supply the studio-approved minimumAgreement between zero and one.");
        var services = fixtures.Select(fixture => QualificationInput.Text(fixture, "service")).ToHashSet();
        if (new[] { "Wedding", "Event", "Headshot", "FamilyPortrait" }.Any(service => !services.Contains(service))) throw new ArgumentException("Supply annotated fixtures for all four photography services.");
        foreach (var fixture in fixtures)
            foreach (var criterion in new[] { "sharpness", "exposure", "closed-eyes" }) _ = QualificationInput.Text(fixture.GetProperty("expectedOutcomes"), criterion);
        var observations = new List<QualificationObservation>();
        var matched = 0; var count = 0;
        foreach (var fixture in fixtures)
        {
            var name = QualificationInput.Text(fixture, "path"); var watch = Stopwatch.StartNew();
            try
            {
                var digest = await QualificationInput.VerifyFile(fixture, directory, ct);
                await using var original = File.OpenRead(QualificationInput.FilePath(fixture, directory));
                await using var preview = await converter.Convert(original, original.Name, ct);
                var id = Guid.NewGuid(); var result = await analysis.Analyze(id, preview, ct);
                var agreement = result.Findings.Count(finding => fixture.GetProperty("expectedOutcomes").GetProperty(finding.Criterion).GetString() == finding.Outcome.ToString());
                matched += agreement; count += 3;
                observations.Add(new(name, result.PhotoId == id, "Advisory result measured against studio annotations; subjective usefulness still requires studio review.", new() { ["sha256"] = digest, ["service"] = QualificationInput.Text(fixture, "service"), ["result"] = result, ["agreementCount"] = agreement, ["milliseconds"] = watch.ElapsedMilliseconds }));
            }
            catch (Exception error) when (error is not OperationCanceledException) { count += 3; observations.Add(new(name, false, error.Message, new())); }
        }
        var ratio = count == 0 ? 0m : (decimal)matched / count;
        observations.Add(new("Annotation agreement", ratio >= minimum, "Outcome agreement is separate from usefulness approval.", new() { ["agreement"] = ratio, ["minimumAgreement"] = minimum, ["approvedBy"] = QualificationInput.Text(manifest, "approvedBy") }));
        return QualificationInput.Report("G-AI", observations);
    }
}
