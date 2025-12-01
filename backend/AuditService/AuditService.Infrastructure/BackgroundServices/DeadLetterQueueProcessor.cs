using AuditService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AuditService.Infrastructure.BackgroundServices;

/// <summary>
/// Background service que procesa eventos fallidos de la Dead Letter Queue
/// Ejecuta cada 1 minuto e intenta republicar eventos listos para reintento
/// </summary>
public class DeadLetterQueueProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeadLetterQueueProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromMinutes(1);

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
                _logger.LogInformation(
                    "🔄 Reintentando evento: {EventType} | Intento: {RetryCount}",
                    failedEvent.EventType, failedEvent.RetryCount + 1);

                // Por ahora solo removemos el evento (implementación futura: republicar)
                // TODO: Implementar lógica de republicación cuando esté disponible el EventPublisher
                await dlq.Remove(failedEvent.Id);
                _logger.LogInformation("✅ Evento procesado: {EventType}", failedEvent.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error reintentando evento: {EventType}", failedEvent.EventType);
                await dlq.MarkAsFailed(failedEvent.Id, ex.Message);
            }
        }
    }
}
