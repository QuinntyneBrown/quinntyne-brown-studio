using System.Security.Cryptography;
using System.Text.Json;
namespace Qbs.Qualification;

public static class QualificationInput
{
    public static string Text(JsonElement value, string key) => value.TryGetProperty(key, out var field) && field.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(field.GetString()) ? field.GetString()! : throw new ArgumentException($"Supply '{key}' in the qualification manifest.");
    public static JsonElement[] Items(JsonElement value, string key) => value.TryGetProperty(key, out var field) && field.ValueKind == JsonValueKind.Array && field.GetArrayLength() > 0 ? field.EnumerateArray().ToArray() : throw new ArgumentException($"Supply a nonempty '{key}' array in the qualification manifest.");
    public static string FilePath(JsonElement value, string directory) => Path.GetFullPath(Text(value, "path"), directory);
    public static async Task<string> VerifyFile(JsonElement value, string directory, CancellationToken ct)
    {
        var path = FilePath(value, directory);
        var expected = Text(value, "sha256");
        if (expected.Length != 64 || expected.Any(character => !Uri.IsHexDigit(character))) throw new ArgumentException("Supply a 64-character SHA-256 digest for each fixture.");
        await using var input = File.OpenRead(path);
        var digest = Convert.ToHexString(await SHA256.HashDataAsync(input, ct));
        if (!digest.Equals(expected, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Fixture digest differs from the approved manifest.");
        return digest;
    }
    public static QualificationReport Report(string gate, List<QualificationObservation> observations) => new(gate, observations.All(item => item.Passed) ? "Measured" : "Failed", "Measurements are recorded; studio review and the evidence register determine qualification. This command never closes a gate.", observations.ToArray());
}
