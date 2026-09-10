using QuinntyneBrownStudio.Domain.Entities.Blog;

namespace QuinntyneBrownStudio.Application.Ports.Blog;

public interface IDigitalAssetRepository
{
    Task<DigitalAsset?> GetByIdAsync(Guid digitalAssetId, CancellationToken cancellationToken = default);
    Task<DigitalAsset?> GetByStoredFileNameAsync(string storedFileName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DigitalAsset>> GetByCreatedByAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(DigitalAsset digitalAsset, CancellationToken cancellationToken = default);
    void Remove(DigitalAsset digitalAsset);
}
