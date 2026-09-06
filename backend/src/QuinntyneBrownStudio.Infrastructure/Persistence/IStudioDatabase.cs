namespace QuinntyneBrownStudio.Infrastructure.Persistence;

public interface IStudioDatabase
{
    Task Verify(CancellationToken cancellationToken = default);
    Task Migrate(CancellationToken cancellationToken = default);
}
