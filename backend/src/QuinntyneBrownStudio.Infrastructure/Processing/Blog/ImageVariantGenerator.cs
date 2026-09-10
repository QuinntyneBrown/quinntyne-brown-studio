using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NeoSolve.ImageSharp.AVIF;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace QuinntyneBrownStudio.Infrastructure.Processing.Blog;

public class ImageVariantGenerator(ILogger<ImageVariantGenerator> logger) : IImageVariantGenerator
{
    private static readonly int[] Breakpoints = [320, 640, 960, 1280, 1920];

    public async Task GenerateVariantsAsync(
        string sourceFilePath,
        Guid assetId,
        int originalWidth,
        CancellationToken cancellationToken = default)
    {
        var assetsDir = Path.GetDirectoryName(sourceFilePath)!;

        // Load the source image once; ImageSharp keeps it in memory for repeated resizes.
        using var source = await Image.LoadAsync(sourceFilePath, cancellationToken);

        foreach (var breakpointWidth in Breakpoints)
        {
            // Skip breakpoints that are wider than (or equal to) the original — no upscaling.
            if (breakpointWidth >= originalWidth)
                continue;

            // Clone so the resize for one breakpoint does not affect subsequent iterations.
            using var clone = source.Clone(ctx =>
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(breakpointWidth, 0), // height = 0 → preserve aspect ratio
                    Mode = ResizeMode.Max,
                }));

            // Generate WebP variant.
            await SaveVariantAsync(clone, assetsDir, assetId, breakpointWidth, "webp",
                new WebpEncoder { Quality = 80 }, cancellationToken);

            // Generate AVIF variant (design Section 3.4 requires both formats).
            await SaveVariantAsync(clone, assetsDir, assetId, breakpointWidth, "avif",
                new AVIFEncoder(), cancellationToken);
        }
    }

    private async Task SaveVariantAsync(
        Image clone,
        string assetsDir,
        Guid assetId,
        int breakpointWidth,
        string format,
        SixLabors.ImageSharp.Formats.IImageEncoder encoder,
        CancellationToken cancellationToken)
    {
        var variantFileName = $"{assetId}-{breakpointWidth}w.{format}";
        var variantPath = Path.Combine(assetsDir, variantFileName);

        try
        {
            await clone.SaveAsync(variantPath, encoder, cancellationToken);

            logger.LogDebug(
                "Generated {Format} variant at {Width}px for asset {AssetId}: {FileName}",
                format.ToUpperInvariant(), breakpointWidth, assetId, variantFileName);
        }
        catch (Exception ex)
        {
            // ImageSharp creates the destination before invoking the optional encoder.
            // A failed AVIF encode must not leave an empty file that masks WebP fallback.
            if (File.Exists(variantPath)) File.Delete(variantPath);
            if (ex is OperationCanceledException) throw;
            // A failure to generate one variant must not abort the upload.
            // Log and continue so other variants and the original asset are unaffected.
            logger.LogWarning(ex,
                "Failed to generate {Format} variant at {Width}px for asset {AssetId}",
                format.ToUpperInvariant(), breakpointWidth, assetId);
        }
    }
}
