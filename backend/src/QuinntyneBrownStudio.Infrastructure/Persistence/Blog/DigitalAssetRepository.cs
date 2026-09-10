using QuinntyneBrownStudio.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using QuinntyneBrownStudio.Domain.Entities.Blog;
using QuinntyneBrownStudio.Application.Ports.Blog;
using Microsoft.EntityFrameworkCore;

namespace QuinntyneBrownStudio.Infrastructure.Persistence.Blog;

public class DigitalAssetRepository(StudioDbContext context) : IDigitalAssetRepository
{
    public async Task<DigitalAsset?> GetByIdAsync(Guid digitalAssetId, CancellationToken cancellationToken = default)
        => await context.DigitalAssets.FindAsync([digitalAssetId], cancellationToken);

    public async Task<DigitalAsset?> GetByStoredFileNameAsync(string storedFileName, CancellationToken cancellationToken = default)
        => await context.DigitalAssets.FirstOrDefaultAsync(d => d.StoredFileName == storedFileName, cancellationToken);

    public async Task<IReadOnlyList<DigitalAsset>> GetByCreatedByAsync(Guid userId, CancellationToken cancellationToken = default)
        => await context.DigitalAssets
            .Where(d => d.CreatedBy == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(DigitalAsset digitalAsset, CancellationToken cancellationToken = default)
        => await context.DigitalAssets.AddAsync(digitalAsset, cancellationToken);

    public void Remove(DigitalAsset digitalAsset) => context.DigitalAssets.Remove(digitalAsset);
}
