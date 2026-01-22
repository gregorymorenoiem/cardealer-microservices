using System.Text.Json.Serialization;

namespace CarDealer.Shared.Audit.Models;

/// <summary>
/// Representa un evento de auditoría para enviar al AuditService
/// </summary>
public class AuditEvent
{
    /// <summary>
    /// ID único del evento
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tipo de evento (ej: "UserLogin", "PaymentCreated", "VehicleUpdated")
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Servicio que origina el evento
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario que realizó la acción
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Email del usuario
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// IP del cliente
    /// </summary>
    public string? ClientIp { get; set; }

    /// <summary>
    /// User Agent del cliente
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Ruta de la solicitud (ej: "/api/payments/123")
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// Método HTTP (GET, POST, PUT, DELETE)
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// Código de respuesta HTTP
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// ID del recurso afectado
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// Tipo del recurso (ej: "Vehicle", "Payment", "User")
    /// </summary>
    public string? ResourceType { get; set; }

    /// <summary>
    /// Acción realizada (ej: "Create", "Update", "Delete", "Read")
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Estado anterior del recurso (para updates)
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// Estado nuevo del recurso (para creates/updates)
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// Metadata adicional como JSON
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Severidad del evento
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AuditSeverity Severity { get; set; } = AuditSeverity.Info;

    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Mensaje de error si la operación falló
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Duración de la operación en milisegundos
    /// </summary>
    public double? DurationMs { get; set; }

    /// <summary>
    /// ID de correlación para tracing
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Trace ID de OpenTelemetry
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Span ID de OpenTelemetry
    /// </summary>
    public string? SpanId { get; set; }

    /// <summary>
    /// Timestamp del evento (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Severidad del evento de auditoría
/// </summary>
public enum AuditSeverity
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}
