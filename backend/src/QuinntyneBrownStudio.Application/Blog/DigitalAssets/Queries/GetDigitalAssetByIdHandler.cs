using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Domain.Exceptions.Blog;

using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.DigitalAssets.Queries;

public class GetDigitalAssetByIdHandler(IDigitalAssetRepository assets) : IRequestHandler<GetDigitalAssetByIdQuery, DigitalAssetDto>
{
    public async Task<DigitalAssetDto> Handle(GetDigitalAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var asset = await assets.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Digital asset with ID '{request.Id}' was not found.");

        return new DigitalAssetDto(
            asset.DigitalAssetId, asset.OriginalFileName,
            asset.ContentType, asset.FileSizeBytes, asset.Width, asset.Height,
            $"/blog/assets/{asset.StoredFileName}", asset.CreatedAt);
    }
}
