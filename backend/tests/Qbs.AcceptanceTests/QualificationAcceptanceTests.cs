using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Qbs.Qualification;
using SkiaSharp;

namespace Qbs.AcceptanceTests;

public sealed class QualificationAcceptanceTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "qbs-qualification-" + Guid.NewGuid().ToString("N"));
    public QualificationAcceptanceTests() => Directory.CreateDirectory(directory);
    public void Dispose() => Directory.Delete(directory, true);

    // Given a pinned JPEG fixture and expected preview dimensions, when the actual
    // converter runs, then original integrity and measured derivative data are reported.
    [Fact]
    public async Task AC_L2_060_01_Qualification_measures_the_real_converter_without_claiming_camera_approval()
    {
        using var bitmap = new SKBitmap(32, 24);
        using var encoded = bitmap.Encode(SKEncodedImageFormat.Jpeg, 90);
        var bytes = encoded.ToArray();
        await File.WriteAllBytesAsync(Path.Combine(directory, "photo.jpg"), bytes);
        var manifest = new { fixtures = new[] { new { path = "photo.jpg", camera = "Synthetic JPEG regression", sha256 = Convert.ToHexString(SHA256.HashData(bytes)), width = 32, height = 24 } } };
        var report = await Run("raw", manifest, 0);
        Assert.Equal("Measured", report.GetProperty("status").GetString());
        Assert.False(report.GetProperty("gateClosed").GetBoolean());
        Assert.Equal(32, report.GetProperty("observations")[0].GetProperty("metrics").GetProperty("width").GetInt32());
        Assert.Equal(bytes, await File.ReadAllBytesAsync(Path.Combine(directory, "photo.jpg")));
    }

    // Given missing evidence or a changed fixture, when qualification is invoked,
    // then an actionable blocked/failed report replaces any claim of a measured pass.
    [Theory]
    [InlineData("raw")]
    [InlineData("ai")]
    [InlineData("upload")]
    [InlineData("environment")]
    public async Task AC_L2_060_01_AC_L2_067_01_AC_L2_068_01_Missing_inputs_produce_blocked_evidence(string command)
    {
        var report = await Run(command, new { }, 2);
        Assert.Equal("Blocked", report.GetProperty("status").GetString());
        Assert.False(report.GetProperty("gateClosed").GetBoolean());
        Assert.NotEmpty(report.GetProperty("message").GetString()!);
    }

    [Fact]
    public async Task AC_L2_060_01_Changed_fixture_does_not_receive_a_measurement_pass()
    {
        await File.WriteAllBytesAsync(Path.Combine(directory, "changed.jpg"), [1, 2, 3]);
        var report = await Run("raw", new { fixtures = new[] { new { path = "changed.jpg", camera = "Pinned camera", sha256 = new string('0', 64), width = 32, height = 24 } } }, 1);
        Assert.Equal("Failed", report.GetProperty("status").GetString());
        Assert.Contains("digest", report.GetProperty("observations")[0].GetProperty("message").GetString());
    }

    private async Task<JsonElement> Run(string command, object manifest, int expectedExit)
    {
        var input = Path.Combine(directory, "manifest.json");
        var output = Path.Combine(directory, "report.json");
        await File.WriteAllTextAsync(input, JsonSerializer.Serialize(manifest));
        var runner = new QualificationRunner(new ConfigurationBuilder().Build());
        Assert.Equal(expectedExit, await runner.Run([command, "--manifest", input, "--report", output], CancellationToken.None));
        return JsonSerializer.Deserialize<JsonElement>(await File.ReadAllTextAsync(output));
    }
}
