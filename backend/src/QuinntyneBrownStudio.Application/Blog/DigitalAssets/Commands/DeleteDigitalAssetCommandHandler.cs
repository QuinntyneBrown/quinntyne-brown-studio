using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Domain.Exceptions.Blog;
using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.DigitalAssets.Commands;

public class DeleteDigitalAssetCommandHandler(IUnitOfWork uow, IAssetStorage assetStorage) : IRequestHandler<DeleteDigitalAssetCommand>
{
    public async Task Handle(DeleteDigitalAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await uow.DigitalAssets.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Digital asset with ID '{request.Id}' was not found.");

        if (await uow.Articles.AnyByFeaturedImageIdAsync(request.Id, cancellationToken))
            throw new ConflictException("Cannot delete this asset because it is referenced by one or more articles.");

        uow.DigitalAssets.Remove(asset);
        await uow.SaveChangesAsync(cancellationToken);

        // Delete the stored file via IAssetStorage (design Section 3.5 — DeleteAsync).
        await assetStorage.DeleteAsync(asset.StoredFileName, cancellationToken);

        foreach (var size in new[] { 320, 640, 960, 1280, 1920 })
            foreach (var format in new[] { "webp", "avif" })
                await assetStorage.DeleteAsync($"{asset.DigitalAssetId}-{size}w.{format}", cancellationToken);

    }
}
