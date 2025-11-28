using AuditService.Shared.Enums;

namespace AuditService.Application.DTOs;

public class AuditLogDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string UserIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public Dictionary<string, object> AdditionalData { get; set; } = new();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long? DurationMs { get; set; }
    public string? CorrelationId { get; set; }
    public string ServiceName { get; set; } = "Unknown";
    public AuditSeverity Severity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Display properties
    public string UserDisplayName => GetUserDisplayName();
    public string SeverityDisplayName => Severity.GetDisplayName();
    public string SeverityCssClass => Severity.GetCssClass();
    public string ActionCategory => AuditEnumExtensions.GetActionCategory(Action);
    public string ActionIcon => AuditEnumExtensions.GetActionIcon(Action);
    public string Summary => GetSummary();

    public static AuditLogDto FromEntity(Domain.Entities.AuditLog entity)
    {
        return new AuditLogDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Action = entity.Action,
            Resource = entity.Resource,
            UserIp = entity.UserIp,
            UserAgent = entity.UserAgent,
            AdditionalData = entity.AdditionalData,
            Success = entity.Success,
            ErrorMessage = entity.ErrorMessage,
            DurationMs = entity.DurationMs,
            CorrelationId = entity.CorrelationId,
            ServiceName = entity.ServiceName,
            Severity = entity.Severity,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private string GetUserDisplayName()
    {
        return UserId switch
        {
            "system" => "System",
            "anonymous" => "Anonymous",
            _ => UserId
        };
    }

    private string GetSummary()
    {
        var status = Success ? "SUCCESS" : "FAILED";
        var user = GetUserDisplayName();
        return $"{Action} on {Resource} by {user} - {status}";
    }
}