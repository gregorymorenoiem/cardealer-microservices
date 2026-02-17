using System.Threading;
using System.Threading.Tasks;

namespace StaffService.Application.Clients;

/// <summary>
/// Client for logging audit events via AuditService.
/// </summary>
public interface IAuditServiceClient
{
    Task LogActionAsync(
        Guid? userId,
        string action,
        string entityType,
        string? entityId,
        object? details,
        string? ipAddress,
        string? userAgent,
        CancellationToken ct = default);
}
