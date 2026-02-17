namespace MediaService.Application.Interfaces;

/// <summary>
/// Client for centralized audit logging via AuditService.
/// </summary>
public interface IAuditServiceClient
{
    /// <summary>
    /// Logs an action to the centralized audit service.
    /// </summary>
    Task LogActionAsync(string entityType, string entityId, string action, string performedBy, string? details = null);
}
