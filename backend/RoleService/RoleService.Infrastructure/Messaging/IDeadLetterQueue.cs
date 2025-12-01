namespace RoleService.Infrastructure.Messaging;

/// <summary>
/// Dead Letter Queue para almacenar eventos que fallaron al publicarse a RabbitMQ
/// </summary>
public interface IDeadLetterQueue
{
    /// <summary>
    /// Agrega un evento fallido a la cola de reintento
    /// </summary>
    void Enqueue(FailedEvent failedEvent);

    /// <summary>
    /// Obtiene eventos listos para reintentar
    /// </summary>
    IEnumerable<FailedEvent> GetEventsReadyForRetry();

    /// <summary>
    /// Remueve un evento de la cola (después de publicarse exitosamente)
    /// </summary>
    void Remove(Guid eventId);

    /// <summary>
    /// Marca un evento como fallido nuevamente y reagenda retry
    /// </summary>
    void MarkAsFailed(Guid eventId, string error);

    /// <summary>
    /// Obtiene estadísticas de la cola
    /// </summary>
    (int TotalEvents, int ReadyForRetry, int MaxRetries) GetStats();
}
