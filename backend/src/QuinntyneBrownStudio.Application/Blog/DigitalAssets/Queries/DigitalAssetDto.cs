using QuinntyneBrownStudio.Application.Ports.Blog;

using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.DigitalAssets.Queries;

public record DigitalAssetDto(
    Guid DigitalAssetId, string OriginalFileName,
    string ContentType, long FileSizeBytes, int Width, int Height,
    string Url, DateTime CreatedAt);
