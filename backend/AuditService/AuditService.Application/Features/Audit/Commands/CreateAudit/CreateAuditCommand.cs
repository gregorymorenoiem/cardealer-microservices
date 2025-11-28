using AuditService.Shared;
using AuditService.Shared.Enums; // Agregar este using
using MediatR;

namespace AuditService.Application.Features.Audit.Commands.CreateAudit;

public class CreateAuditCommand : IRequest<ApiResponse<string>>
{
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string UserIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public Dictionary<string, object> AdditionalData { get; set; } = new();
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public long? DurationMs { get; set; }
    public string? CorrelationId { get; set; }
    public string ServiceName { get; set; } = "Unknown";
    public AuditSeverity Severity { get; set; } = AuditSeverity.Information; // Ahora usa Shared.Enums.AuditSeverity

    public CreateAuditCommand() { }

    public CreateAuditCommand(
        string userId,
        string action,
        string resource,
        string userIp,
        string userAgent,
        Dictionary<string, object> additionalData,
        bool success = true,
        string? errorMessage = null,
        long? durationMs = null,
        string? correlationId = null,
        string serviceName = "Unknown",
        AuditSeverity severity = AuditSeverity.Information) // Cambiar aquí también
    {
        UserId = userId;
        Action = action;
        Resource = resource;
        UserIp = userIp;
        UserAgent = userAgent;
        AdditionalData = additionalData;
        Success = success;
        ErrorMessage = errorMessage;
        DurationMs = durationMs;
        CorrelationId = correlationId;
        ServiceName = serviceName;
        Severity = severity;
    }
}