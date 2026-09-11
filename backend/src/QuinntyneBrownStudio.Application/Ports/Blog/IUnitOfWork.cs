namespace QuinntyneBrownStudio.Application.Ports.Blog;

public interface IUnitOfWork
{
    IArticleRepository Articles { get; }
    IDigitalAssetRepository DigitalAssets { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
