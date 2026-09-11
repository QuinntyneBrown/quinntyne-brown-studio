using MediatR;
using QuinntyneBrownStudio.Application.Ports.Blog;

namespace QuinntyneBrownStudio.Application.Blog.DigitalAssets.Queries;

public sealed class GetBlogAssetHandler(IAssetStorage assetStorage, IDigitalAssetRepository assetRepository) : IRequestHandler<GetBlogAssetQuery, BlogAssetResult>
{
    // Responsive breakpoints in ascending order (matches ImageVariantGenerator breakpoints).
    private static readonly int[] Breakpoints = [320, 640, 960, 1280, 1920];

    public async Task<BlogAssetResult> Handle(GetBlogAssetQuery request, CancellationToken ct)
    {
        var fileName = request.FileName;
        var w = request.Width;
        if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || fileName.Contains('\\') || fileName.Contains('/'))
            return new BlogAssetResult(400);

        // Validate that the requested filename corresponds to a registered DigitalAsset entity.
        // Design reference: Section 3.6 — GetByStoredFileNameAsync is "used during serve".
        // Only the base stored filename (the original uploaded file) has an entity record;
        // variant files ({assetId}-{width}w.{format}) are derived from that entity.
        var baseStoredFileName = Path.GetFileName(fileName);
        var asset = await assetRepository.GetByStoredFileNameAsync(baseStoredFileName, ct);
        if (asset == null)
            return new BlogAssetResult(404);

        // Determine the best format accepted by the client (AVIF > WebP > original).
        var accept = request.Accept;
        var preferAvif  = accept.Contains("image/avif",  StringComparison.OrdinalIgnoreCase);
        var preferWebp  = accept.Contains("image/webp",  StringComparison.OrdinalIgnoreCase);

        // If a width was requested and a variant-capable format is accepted, try to find
        // a pre-generated variant that matches the request.
        // Variants are derived from the confirmed asset's GUID (entity ID == stored filename base).
        var assetId = asset.DigitalAssetId;
        if (w.HasValue && (preferAvif || preferWebp))
        {
            var nearestWidth = FindNearestBreakpoint(w.Value);

            // Try AVIF first (highest compression), then WebP.
            // Use AssetStorage.GetAsync() per the design (Section 5.2, step 5) so the serving
            // path works regardless of whether the backing store is local filesystem or cloud blob.
            if (preferAvif)
            {
                var avifVariantName = $"{assetId}-{nearestWidth}w.avif";
                var avifStream = await assetStorage.GetAsync(avifVariantName, ct);
                if (avifStream != null)
                    return await ServeStreamAsync(avifStream, avifVariantName, "image/avif", request.IfNoneMatch);
            }

            if (preferWebp)
            {
                var webpVariantName = $"{assetId}-{nearestWidth}w.webp";
                var webpStream = await assetStorage.GetAsync(webpVariantName, ct);
                if (webpStream != null)
                    return await ServeStreamAsync(webpStream, webpVariantName, "image/webp", request.IfNoneMatch);
            }
        }

        // Fall back to serving the exact filename requested via AssetStorage.GetAsync().
        var fallbackStream = await assetStorage.GetAsync(fileName, ct);
        if (fallbackStream == null)
            return new BlogAssetResult(404);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".webp"           => "image/webp",
            ".avif"           => "image/avif",
            ".gif"            => "image/gif",
            ".svg"            => "image/svg+xml",
            _                 => "application/octet-stream"
        };

        return await ServeStreamAsync(fallbackStream, fileName, contentType, request.IfNoneMatch);
    }

    private static Task<BlogAssetResult> ServeStreamAsync(Stream stream, string name, string contentType, string ifNoneMatch)
    {
        // Vary: Accept ensures caches distinguish between format-negotiated variants.

        var etag = $"\"{name}\"";

        if (ifNoneMatch.Split(',').Any(value => value.Trim() == etag))
        {
            stream.Dispose();
            return Task.FromResult(new BlogAssetResult(304, ETag: etag));
        }

        return Task.FromResult(new BlogAssetResult(200, stream, contentType, etag));
    }

    /// <summary>
    /// Returns the nearest breakpoint that is &gt;= the requested width,
    /// or the largest breakpoint if the requested width exceeds all breakpoints.
    /// </summary>
    private static int FindNearestBreakpoint(int requestedWidth)
    {
        foreach (var bp in Breakpoints)
            if (bp >= requestedWidth)
                return bp;
        return Breakpoints[^1];
    }
}
