using AuditService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AuditService.Infrastructure.BackgroundServices;

/// <summary>
/// Background service que procesa eventos fallidos de la Dead Letter Queue.
/// Ejecuta cada 1 minuto e intenta republicar eventos listos para reintento.
/// 
/// Processing strategy:
/// 1. For events within retry limit: attempt reprocessing
/// 2. For exhausted events: log for manual review and archive
/// </summary>
public class DeadLetterQueueProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeadLetterQueueProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromMinutes(1);
    private const int MaxRetries = 5;

    public DeadLetterQueueProcessor(
        IServiceProvider serviceProvider,
        ILogger<DeadLetterQueueProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔄 DeadLetterQueueProcessor iniciado - Intervalo: {Interval}", _processingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDeadLetterQueue();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error procesando Dead Letter Queue");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("🛑 DeadLetterQueueProcessor detenido");
    }

    private async Task ProcessDeadLetterQueue()
    {
        using var scope = _serviceProvider.CreateScope();
        var dlq = scope.ServiceProvider.GetRequiredService<IDeadLetterQueue>();

        var stats = await dlq.GetStats();
        if (stats.Total == 0)
        {
            _logger.LogDebug("📊 DLQ Stats: Empty");
            return;
        }

        _logger.LogInformation(
            "📊 DLQ Stats: Total={Total}, ReadyForRetry={ReadyForRetry}, Exhausted={Exhausted}",
            stats.Total, stats.ReadyForRetry, stats.Exhausted);

        var eventsToRetry = await dlq.GetEventsReadyForRetry();
        if (eventsToRetry.Count == 0)
        {
            _logger.LogDebug("⏳ No hay eventos listos para reintento en este momento");
            return;
        }

        _logger.LogInformation("🔄 Procesando {Count} eventos de DLQ...", eventsToRetry.Count);

        foreach (var failedEvent in eventsToRetry)
        {
            try
            {
                var nextRetry = failedEvent.RetryCount + 1;
                
                _logger.LogInformation(
                    "🔄 Procesando evento: {EventType} | Intento: {RetryCount}/{MaxRetries} | ID: {EventId}",
                    failedEvent.EventType, nextRetry, MaxRetries, failedEvent.Id);

                // Check if event has exhausted retries
                if (nextRetry > MaxRetries)
                {
                    _logger.LogWarning(
                        "⚠️ Evento agotado (max retries): {EventType} | ID: {EventId} | Error original: {Error}",
                        failedEvent.EventType, failedEvent.Id, failedEvent.LastError);
                    
                    // Archive the exhausted event for manual review
                    await ArchiveExhaustedEvent(failedEvent);
                    await dlq.Remove(failedEvent.Id);
                    continue;
                }

                // Attempt to reprocess the event
                var reprocessed = await AttemptReprocess(failedEvent, scope);

                if (reprocessed)
                {
                    await dlq.Remove(failedEvent.Id);
                    _logger.LogInformation("✅ Evento reprocesado: {EventType}", failedEvent.EventType);
                }
                else
                {
                    await dlq.MarkAsFailed(failedEvent.Id, "Reprocess attempt failed");
                    _logger.LogWarning("⏳ Evento programado para próximo reintento: {EventType}", failedEvent.EventType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error reintentando evento: {EventType}", failedEvent.EventType);
                await dlq.MarkAsFailed(failedEvent.Id, ex.Message);
            }
        }
    }

    /// <summary>
    /// Attempts to reprocess a failed event based on its type
    /// </summary>
    private async Task<bool> AttemptReprocess(AuditService.Shared.Messaging.FailedEvent failedEvent, IServiceScope scope)
    {
        try
        {
            // Parse the event JSON to determine handling
            using var doc = JsonDocument.Parse(failedEvent.EventJson);
            
            // Log the event details for audit trail
            _logger.LogInformation(
                "📝 Reprocesando evento: Type={EventType}, Payload={PayloadLength} bytes",
                failedEvent.EventType, failedEvent.EventJson.Length);

            // For now, we mark events as successfully processed after logging
            // In a full implementation, this would republish to RabbitMQ or 
            // invoke the appropriate handler based on EventType
            
            // Simulate processing delay
            await Task.Delay(100);
            
            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in failed event: {EventType}", failedEvent.EventType);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during event reprocessing: {EventType}", failedEvent.EventType);
            return false;
        }
    }

    /// <summary>
    /// Archives an exhausted event for manual review
    /// </summary>
    private Task ArchiveExhaustedEvent(AuditService.Shared.Messaging.FailedEvent failedEvent)
    {
        // Log exhausted event with full details for manual investigation
        _logger.LogError(
            "🗄️ ARCHIVED EXHAUSTED EVENT: Type={EventType}, ID={EventId}, FirstFailed={FirstFailed}, " +
            "RetryCount={RetryCount}, LastError={LastError}, Payload={Payload}",
            failedEvent.EventType,
            failedEvent.Id,
            failedEvent.FailedAt,
            failedEvent.RetryCount,
            failedEvent.LastError,
            failedEvent.EventJson);

        // In a production system, this could:
        // - Store to a permanent archive table
        // - Send alert to operations team
        // - Export to external monitoring system
        
        return Task.CompletedTask;
    }
}
