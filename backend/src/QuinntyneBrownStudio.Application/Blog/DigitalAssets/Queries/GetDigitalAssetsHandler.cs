using QuinntyneBrownStudio.Application.Ports.Blog;

using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.DigitalAssets.Queries;

public class GetDigitalAssetsHandler(IDigitalAssetRepository assets) : IRequestHandler<GetDigitalAssetsQuery, List<DigitalAssetDto>>
{
    public async Task<List<DigitalAssetDto>> Handle(GetDigitalAssetsQuery request, CancellationToken cancellationToken)
    {
        var items = await assets.GetByCreatedByAsync(request.UserId, cancellationToken);
        return items.Select(d => new DigitalAssetDto(
            d.DigitalAssetId, d.OriginalFileName,
            d.ContentType, d.FileSizeBytes, d.Width, d.Height,
            $"/blog/assets/{d.StoredFileName}", d.CreatedAt)).ToList();
    }
}
