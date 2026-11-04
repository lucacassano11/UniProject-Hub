using Microsoft.Extensions.DependencyInjection;

namespace Sessioni.Services;

public class InactiveSessionsWorker(IServiceProvider services, IConfiguration config, ILogger<InactiveSessionsWorker> logger)
    : BackgroundService
{
    // Task.Delay accepts at most ~24.8 days; cap large test/config values.
    private static readonly TimeSpan MaxDelay = TimeSpan.FromMilliseconds(int.MaxValue);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var hours = config.GetValue("Cleanup:IntervalHours", 24);
        var interval = TimeSpan.FromHours(hours);
        if (interval > MaxDelay) interval = MaxDelay;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = services.CreateScope();
                var cleanup = scope.ServiceProvider.GetRequiredService<ISessionCleanup>();
                var n = await cleanup.RunOnceAsync(DateOnly.FromDateTime(DateTime.UtcNow));
                if (n > 0) logger.LogInformation("EliminaSessioniInattive: rimosse {Count} sessioni", n);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore nel job EliminaSessioniInattive");
            }
            await Task.Delay(interval, stoppingToken);
        }
    }
}
