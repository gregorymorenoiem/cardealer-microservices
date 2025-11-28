using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Services;

public class NotificationQueueBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationQueueBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30); // Procesar cada 30 segundos

    public NotificationQueueBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<NotificationQueueBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔄 Notification Queue Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var queueService = scope.ServiceProvider.GetRequiredService<NotificationQueueService>();

                await queueService.ProcessPendingQueueAsync();
                _logger.LogDebug("✅ Processed notification queue cycle");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing notification queue");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("🛑 Notification Queue Background Service stopped");
    }
}