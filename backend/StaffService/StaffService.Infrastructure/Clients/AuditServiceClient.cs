using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using StaffService.Application.Clients;

namespace StaffService.Infrastructure.Clients;

public class AuditServiceClient : IAuditServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuditServiceClient> _logger;

    public AuditServiceClient(HttpClient httpClient, ILogger<AuditServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task LogActionAsync(
        Guid? userId,
        string action,
        string entityType,
        string? entityId,
        object? details,
        string? ipAddress,
        string? userAgent,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/audit/logs", new
            {
                userId = userId,
                action = action,
                entityType = entityType,
                entityId = entityId,
                details = details != null ? System.Text.Json.JsonSerializer.Serialize(details) : null,
                ipAddress = ipAddress,
                userAgent = userAgent,
                timestamp = DateTime.UtcNow
            }, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to log audit action {Action} for entity {EntityType}/{EntityId}",
                    action, entityType, entityId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit action {Action}", action);
            // Don't throw - audit logging should not break main flow
        }
    }
}
