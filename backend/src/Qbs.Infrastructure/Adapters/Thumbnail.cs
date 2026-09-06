using Qbs.Domain.Exceptions;
using SkiaSharp;

namespace Qbs.Infrastructure.Adapters;

public static class Thumbnail
{
    public static Stream Create(Stream preview)
    {
        using var bitmap =
            SKBitmap.Decode(preview)
            ?? throw new StudioException(400, "Preview cannot be decoded.");
        var scale = Math.Min(1d, 480d / Math.Max(bitmap.Width, bitmap.Height));
        using var resized = new SKBitmap(
            Math.Max(1, (int)(bitmap.Width * scale)),
            Math.Max(1, (int)(bitmap.Height * scale))
        );
        using (var canvas = new SKCanvas(resized))
            canvas.DrawBitmap(
                bitmap,
                new SKRect(0, 0, resized.Width, resized.Height),
                new SKSamplingOptions(SKFilterMode.Linear)
            );
        using var encoded = resized.Encode(SKEncodedImageFormat.Jpeg, 82);
        return new MemoryStream(encoded.ToArray());
    }
}
