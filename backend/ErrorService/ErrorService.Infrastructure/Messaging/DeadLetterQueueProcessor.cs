using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CarDealer.Contracts.Events.Error;

namespace ErrorService.Infrastructure.Messaging;

/// <summary>
/// Background service que procesa la Dead Letter Queue cada minuto
/// Reintenta publicar eventos fallidos cuando RabbitMQ se recupera
/// </summary>
public class DeadLetterQueueProcessor : BackgroundService
{
    private readonly IDeadLetterQueue _deadLetterQueue;
    private readonly RabbitMqEventPublisher _eventPublisher;
    private readonly ILogger<DeadLetterQueueProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromMinutes(1);

    public DeadLetterQueueProcessor(
        IDeadLetterQueue deadLetterQueue,
        RabbitMqEventPublisher eventPublisher,
        ILogger<DeadLetterQueueProcessor> _logger)
    {
        _deadLetterQueue = deadLetterQueue;
        _eventPublisher = eventPublisher;
        this._logger = _logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("⚙️ DeadLetterQueueProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDeadLetterQueueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing dead letter queue");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("⚙️ DeadLetterQueueProcessor stopped");
    }

    private async Task ProcessDeadLetterQueueAsync(CancellationToken ct)
    {
        var eventsToRetry = _deadLetterQueue.GetEventsReadyForRetry().ToList();

        if (!eventsToRetry.Any())
            return;

        _logger.LogInformation("📮 Processing {Count} failed events from DLQ", eventsToRetry.Count);

        foreach (var failedEvent in eventsToRetry)
        {
            if (ct.IsCancellationRequested)
                break;

            try
            {
                // Deserializar evento (solo soportamos ErrorCriticalEvent por ahora)
                if (failedEvent.EventType == nameof(ErrorCriticalEvent))
                {
                    var @event = JsonSerializer.Deserialize<ErrorCriticalEvent>(failedEvent.EventJson);
                    if (@event != null)
                    {
                        // Intentar republicar
                        await _eventPublisher.PublishAsync(@event, ct);

                        // Éxito: remover de DLQ
                        _deadLetterQueue.Remove(failedEvent.Id);
                        _logger.LogInformation("✅ Successfully republished event {EventId} after {Retries} retries",
                            failedEvent.Id, failedEvent.RetryCount);
                    }
                }
            }
            catch (Exception ex)
            {
                // Falló nuevamente: marcar y reagendar
                _deadLetterQueue.MarkAsFailed(failedEvent.Id, ex.Message);
                _logger.LogWarning("⚠️ Failed to republish event {EventId} (retry {Retries}): {Error}",
                    failedEvent.Id, failedEvent.RetryCount, ex.Message);
            }
        }

        // Log stats
        var (total, ready, maxRetries) = _deadLetterQueue.GetStats();
        if (total > 0)
        {
            _logger.LogInformation("📊 DLQ Stats: Total={Total}, Ready={Ready}, MaxRetries={MaxRetries}",
                total, ready, maxRetries);
        }
    }
}
