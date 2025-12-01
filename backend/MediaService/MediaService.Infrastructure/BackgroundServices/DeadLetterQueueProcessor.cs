using MediaService.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MediaService.Infrastructure.BackgroundServices;

public class DeadLetterQueueProcessor : BackgroundService
{
    private readonly IDeadLetterQueue _deadLetterQueue;
    private readonly ILogger<DeadLetterQueueProcessor> _logger;

    public DeadLetterQueueProcessor(
        IDeadLetterQueue deadLetterQueue,
        ILogger<DeadLetterQueueProcessor> logger)
    {
        _deadLetterQueue = deadLetterQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 MediaService DLQ Processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessFailedEvents();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "❌ MediaService DLQ Processor error");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task ProcessFailedEvents()
    {
        var stats = await _deadLetterQueue.GetStats();
        if (stats.Total > 0)
        {
            _logger.LogInformation("📊 MediaService DLQ Stats: Total={Total}, Ready={Ready}, Exhausted={Exhausted}",
                stats.Total, stats.ReadyForRetry, stats.Exhausted);
        }

        var eventsToRetry = await _deadLetterQueue.GetEventsReadyForRetry();
        foreach (var failedEvent in eventsToRetry)
        {
            try
            {
                _logger.LogInformation("🔄 MediaService DLQ retry: {EventType} (Attempt {RetryCount})",
                    failedEvent.EventType, failedEvent.RetryCount + 1);

                await _deadLetterQueue.Remove(failedEvent.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ MediaService DLQ retry failed: {EventType}", failedEvent.EventType);
                await _deadLetterQueue.MarkAsFailed(failedEvent.Id, ex.Message);
            }
        }
    }
}
