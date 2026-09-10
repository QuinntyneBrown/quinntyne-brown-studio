using QuinntyneBrownStudio.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Infrastructure.Persistence.Blog;
using Microsoft.EntityFrameworkCore.Storage;

namespace QuinntyneBrownStudio.Infrastructure.Persistence.Blog;

public class UnitOfWork(StudioDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IArticleRepository Articles { get; } = new ArticleRepository(context);
    public IDigitalAssetRepository DigitalAssets { get; } = new DigitalAssetRepository(context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => _transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null) await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null) await _transaction.RollbackAsync(cancellationToken);
    }
}
