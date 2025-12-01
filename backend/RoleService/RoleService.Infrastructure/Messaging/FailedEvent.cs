using CarDealer.Contracts.Events;

namespace RoleService.Infrastructure.Messaging;

/// <summary>
/// Representa un evento que falló al publicarse a RabbitMQ
/// y está en la cola de reintento (Dead Letter Queue local)
/// </summary>
public class FailedEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string EventJson { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; } = 0;
    public DateTime? NextRetryAt { get; set; }
    public string? LastError { get; set; }

    /// <summary>
    /// Calcula el próximo intento usando exponential backoff
    /// </summary>
    public void ScheduleNextRetry()
    {
        RetryCount++;
        // Exponential backoff: 1min, 2min, 4min, 8min, 16min
        var delayMinutes = Math.Min(Math.Pow(2, RetryCount - 1), 16);
        NextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);
    }
}
