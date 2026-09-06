using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using QuinntyneBrownStudio.Application.Ports;
using SkiaSharp;
namespace QuinntyneBrownStudio.Qualification;

public sealed class RawQualificationCheck(IRawPreviewConverter converter) : IQualificationCheck
{
    public async Task<QualificationReport> Run(JsonElement manifest, string directory, string reportPath, CancellationToken ct)
    {
        var fixtures = QualificationInput.Items(manifest, "fixtures");
        foreach (var fixture in fixtures)
        {
            _ = QualificationInput.Text(fixture, "camera");
            if (!fixture.TryGetProperty("width", out var width) || !width.TryGetInt32(out var w) || w < 1 || !fixture.TryGetProperty("height", out var height) || !height.TryGetInt32(out var h) || h < 1) throw new ArgumentException("Supply expected oriented preview width and height for every camera fixture.");
        }
        var observations = new List<QualificationObservation>();
        var output = reportPath + ".previews";
        Directory.CreateDirectory(output);
        foreach (var (fixture, index) in fixtures.Select((value, index) => (value, index)))
        {
            var name = QualificationInput.Text(fixture, "camera");
            var watch = Stopwatch.StartNew();
            try
            {
                var digest = await QualificationInput.VerifyFile(fixture, directory, ct);
                await using var original = File.OpenRead(QualificationInput.FilePath(fixture, directory));
                await using var preview = await converter.Convert(original, original.Name, ct);
                using var bytes = new MemoryStream(); await preview.CopyToAsync(bytes, ct);
                using var decoded = SKBitmap.Decode(bytes.ToArray()) ?? throw new InvalidDataException("The generated preview cannot be decoded.");
                var passed = decoded.Width == fixture.GetProperty("width").GetInt32() && decoded.Height == fixture.GetProperty("height").GetInt32();
                var previewPath = Path.Combine(output, $"{index + 1:D4}.jpg");
                await File.WriteAllBytesAsync(previewPath, bytes.ToArray(), ct);
                await QualificationInput.VerifyFile(fixture, directory, ct);
                observations.Add(new(name, passed, passed ? "Original digest retained; oriented preview dimensions match. Inspect the emitted preview against the annotated reference." : "Oriented preview dimensions differ from the reference.", new() { ["originalSha256"] = digest, ["previewSha256"] = Convert.ToHexString(SHA256.HashData(bytes.ToArray())), ["previewPath"] = previewPath, ["width"] = decoded.Width, ["height"] = decoded.Height, ["milliseconds"] = watch.ElapsedMilliseconds, ["processPeakWorkingSetBytes"] = Process.GetCurrentProcess().PeakWorkingSet64 }));
            }
            catch (Exception error) when (error is not OperationCanceledException) { observations.Add(new(name, false, error.Message, new() { ["milliseconds"] = watch.ElapsedMilliseconds })); }
        }
        return QualificationInput.Report("G-RAW", observations);
    }
}
