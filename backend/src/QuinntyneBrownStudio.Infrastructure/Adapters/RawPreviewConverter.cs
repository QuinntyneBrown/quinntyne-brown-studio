using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Exceptions;
using QuinntyneBrownStudio.Domain.Policies;
using SkiaSharp;

namespace QuinntyneBrownStudio.Infrastructure.Adapters;

public sealed class RawPreviewConverter(IConfiguration config) : IRawPreviewConverter
{
    public async Task<Stream> Convert(Stream original, string name, CancellationToken ct)
    {
        Stream imageSource = original;
        string? temp = null;
        try
        {
            if (Path.GetExtension(name).ToLowerInvariant() is not ".jpg" and not ".jpeg")
            {
                temp = Path.Combine(Path.GetTempPath(), "qbs-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(temp);
                var input = Path.Combine(temp, "input" + Path.GetExtension(name));
                await using (var file = File.Create(input))
                    await original.CopyToAsync(file, ct);
                var start = new ProcessStartInfo(config["Raw:Executable"] ?? "dcraw_emu")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = temp,
                };
                foreach (var arg in new[] { "-w", "-O", Path.Combine(temp, "preview.ppm"), input })
                    start.ArgumentList.Add(arg);
                using var process =
                    Process.Start(start)
                    ?? throw new StudioException(503, "RAW converter could not start.");
                var error = process.StandardError.ReadToEndAsync(ct);
                var output = process.StandardOutput.ReadToEndAsync(ct);
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(TimeSpan.FromMinutes(2));
                try
                {
                    await process.WaitForExitAsync(timeout.Token);
                }
                catch
                {
                    process.Kill(true);
                    throw;
                }
                await Task.WhenAll(error, output);
                if (process.ExitCode != 0)
                    throw new StudioException(
                        400,
                        "RAW conversion failed for this camera encoding."
                    );
                using (var ppm = File.OpenRead(Path.Combine(temp, "preview.ppm")))
                    imageSource = PpmDecoder.Decode(ppm);
            }
            using var codec =
                SKCodec.Create(imageSource)
                ?? throw new StudioException(400, "The photo cannot be decoded.");
            Rules.Require(
                codec.Info.Width > 0
                    && codec.Info.Height > 0
                    && (long)codec.Info.Width * codec.Info.Height <= 200000000,
                "Image dimensions exceed the decoder limit."
            );
            using var bitmap =
                SKBitmap.Decode(codec)
                ?? throw new StudioException(400, "The photo cannot be decoded.");
            var rotate =
                codec.EncodedOrigin
                is SKEncodedOrigin.LeftTop
                    or SKEncodedOrigin.RightTop
                    or SKEncodedOrigin.RightBottom
                    or SKEncodedOrigin.LeftBottom;
            using var oriented = new SKBitmap(
                rotate ? bitmap.Height : bitmap.Width,
                rotate ? bitmap.Width : bitmap.Height
            );
            using (var canvas = new SKCanvas(oriented))
            {
                switch (codec.EncodedOrigin)
                {
                    case SKEncodedOrigin.TopRight:
                        canvas.Translate(bitmap.Width, 0);
                        canvas.Scale(-1, 1);
                        break;
                    case SKEncodedOrigin.BottomRight:
                        canvas.Translate(bitmap.Width, bitmap.Height);
                        canvas.RotateDegrees(180);
                        break;
                    case SKEncodedOrigin.BottomLeft:
                        canvas.Translate(0, bitmap.Height);
                        canvas.Scale(1, -1);
                        break;
                    case SKEncodedOrigin.LeftTop:
                        canvas.RotateDegrees(90);
                        canvas.Scale(1, -1);
                        break;
                    case SKEncodedOrigin.RightTop:
                        canvas.Translate(bitmap.Height, 0);
                        canvas.RotateDegrees(90);
                        break;
                    case SKEncodedOrigin.RightBottom:
                        canvas.Translate(bitmap.Height, bitmap.Width);
                        canvas.RotateDegrees(90);
                        canvas.Scale(-1, 1);
                        break;
                    case SKEncodedOrigin.LeftBottom:
                        canvas.Translate(0, bitmap.Width);
                        canvas.RotateDegrees(-90);
                        break;
                }
                canvas.DrawBitmap(bitmap, 0, 0, new SKSamplingOptions(SKFilterMode.Linear));
            }
            var scale = Math.Min(1d, 2400d / Math.Max(oriented.Width, oriented.Height));
            using var resized = new SKBitmap(
                Math.Max(1, (int)(oriented.Width * scale)),
                Math.Max(1, (int)(oriented.Height * scale))
            );
            using (var canvas = new SKCanvas(resized))
                canvas.DrawBitmap(
                    oriented,
                    new SKRect(0, 0, resized.Width, resized.Height),
                    new SKSamplingOptions(SKFilterMode.Linear)
                );
            using var encoded = resized.Encode(SKEncodedImageFormat.Jpeg, 88);
            return new MemoryStream(encoded.ToArray());
        }
        finally
        {
            if (imageSource != original)
                await imageSource.DisposeAsync();
            if (temp != null)
                Directory.Delete(temp, true);
        }
    }
}
