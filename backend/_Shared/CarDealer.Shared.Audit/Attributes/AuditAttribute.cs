using CarDealer.Shared.Audit.Interfaces;
using CarDealer.Shared.Audit.Models;

namespace CarDealer.Shared.Audit.Attributes;

/// <summary>
/// Atributo para marcar métodos que deben ser auditados
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class AuditAttribute : Attribute
{
    /// <summary>
    /// Tipo de evento de auditoría
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Acción realizada
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de recurso
    /// </summary>
    public string? ResourceType { get; set; }

    /// <summary>
    /// Severidad del evento
    /// </summary>
    public AuditSeverity Severity { get; set; } = AuditSeverity.Info;

    /// <summary>
    /// Incluir request body en el evento
    /// </summary>
    public bool IncludeRequestBody { get; set; } = false;

    /// <summary>
    /// Incluir response body en el evento
    /// </summary>
    public bool IncludeResponseBody { get; set; } = false;

    public AuditAttribute() { }

    public AuditAttribute(string eventType)
    {
        EventType = eventType;
    }

    public AuditAttribute(string eventType, string action)
    {
        EventType = eventType;
        Action = action;
    }
}
