using CarDealer.Shared.Audit.Models;

namespace CarDealer.Shared.Audit.Interfaces;

/// <summary>
/// Interfaz para publicar eventos de auditoría
/// </summary>
public interface IAuditPublisher
{
    /// <summary>
    /// Publica un evento de auditoría al AuditService
    /// </summary>
    Task PublishAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publica un evento de auditoría de forma síncrona (fire and forget)
    /// </summary>
    void Publish(AuditEvent auditEvent);

    /// <summary>
    /// Crea y publica un evento de auditoría con parámetros básicos
    /// </summary>
    Task PublishAsync(
        string eventType,
        string action,
        string? resourceType = null,
        string? resourceId = null,
        object? metadata = null,
        CancellationToken cancellationToken = default);
}
