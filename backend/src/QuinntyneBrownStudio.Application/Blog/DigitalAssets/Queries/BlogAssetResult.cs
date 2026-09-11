namespace QuinntyneBrownStudio.Application.Blog.DigitalAssets.Queries;

public sealed record BlogAssetResult(int StatusCode, Stream? Content = null, string? ContentType = null, string? ETag = null);
