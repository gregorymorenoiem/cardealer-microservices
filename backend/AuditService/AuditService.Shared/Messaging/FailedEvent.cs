namespace AuditService.Shared.Messaging;

/// <summary>
/// Representa un evento que falló al ser publicado a RabbitMQ
/// y será reintentado con exponential backoff
/// </summary>
public class FailedEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tipo de evento que falló (ej: "AuditLogCreated", "AuditLogUpdated")
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// JSON serializado del evento original
    /// </summary>
    public string EventJson { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp cuando ocurrió el fallo inicial
    /// </summary>
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Número de reintentos realizados (0 = primer intento fallido)
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Timestamp cuando debe ejecutarse el próximo reintento
    /// </summary>
    public DateTime NextRetryAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Último mensaje de error registrado
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Calcula el próximo timestamp de reintento usando exponential backoff
    /// Delays: 1min, 2min, 4min, 8min, 16min
    /// </summary>
    public void ScheduleNextRetry()
    {
        RetryCount++;
        var delayMinutes = Math.Pow(2, RetryCount - 1); // 2^0=1, 2^1=2, 2^2=4, 2^3=8, 2^4=16
        NextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);
    }

    /// <summary>
    /// Indica si el evento ha alcanzado el máximo de reintentos
    /// </summary>
    public bool HasExceededMaxRetries(int maxRetries) => RetryCount >= maxRetries;
}
