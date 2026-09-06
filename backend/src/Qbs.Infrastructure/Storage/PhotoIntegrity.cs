using System.Security.Cryptography;
using Qbs.Domain;

namespace Qbs.Infrastructure;

public static class PhotoIntegrity
{
    public static async Task Validate(Stream stream, SessionPhoto photo, CancellationToken ct)
    {
        Rules.Require(stream.Length == photo.Size, "Uploaded length does not match the manifest.");
        var head = new byte[16];
        var n = await stream.ReadAsync(head, ct);
        var ext = Path.GetExtension(photo.Name).ToLowerInvariant();
        var jpeg = n >= 3 && head[0] == 255 && head[1] == 216 && head[2] == 255;
        var tiff =
            n >= 4
            && (
                (head[0] == 73 && head[1] == 73 && head[2] == 42 && head[3] == 0)
                || (head[0] == 77 && head[1] == 77 && head[2] == 0 && head[3] == 42)
            );
        var cr3 =
            n >= 12
            && System.Text.Encoding.ASCII.GetString(head, 4, 4) == "ftyp"
            && System.Text.Encoding.ASCII.GetString(head, 8, 4).StartsWith("crx");
        Rules.Require(
            ext is ".jpg" or ".jpeg" ? jpeg
                : ext == ".cr3" ? cr3
                : tiff,
            "Photo content does not match the selected format."
        );
        stream.Position = 0;
        var digest = Convert.ToHexString(await SHA256.HashDataAsync(stream, ct));
        Rules.Require(
            string.Equals(digest, photo.Sha256, StringComparison.OrdinalIgnoreCase),
            "Uploaded bytes do not match the manifest digest."
        );
        stream.Position = 0;
    }
}
