using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Qbs.Infrastructure.Processing;

public sealed class ProcessingService(
    IServiceScopeFactory scopes,
    ILogger<ProcessingService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DateOnly? lastRetention = null;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<JobProcessor>();
                await processor.Cycle(stoppingToken);
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                if (lastRetention != today)
                {
                    await processor.Retention(stoppingToken);
                    lastRetention = today;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    "Background processing cycle failed: {FailureType}",
                    ex.GetType().Name
                );
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
