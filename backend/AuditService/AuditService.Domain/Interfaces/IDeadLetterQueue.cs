using AuditService.Shared.Messaging;

namespace AuditService.Domain.Interfaces;

/// <summary>
/// Dead Letter Queue para eventos fallidos de RabbitMQ
/// Almacena eventos que no pudieron ser publicados y los reintenta con exponential backoff
/// </summary>
public interface IDeadLetterQueue
{
    /// <summary>
    /// Encola un evento fallido para reintento posterior
    /// </summary>
    Task Enqueue(string eventType, string eventJson, string error);

    /// <summary>
    /// Obtiene eventos que están listos para ser reintentados
    /// </summary>
    Task<List<FailedEvent>> GetEventsReadyForRetry();

    /// <summary>
    /// Remueve un evento de la cola (después de éxito)
    /// </summary>
    Task Remove(Guid eventId);

    /// <summary>
    /// Marca un evento como fallido nuevamente y programa próximo reintento
    /// </summary>
    Task MarkAsFailed(Guid eventId, string error);

    /// <summary>
    /// Obtiene estadísticas de la DLQ para monitoreo
    /// </summary>
    Task<(int Total, int ReadyForRetry, int Exhausted)> GetStats();
}
