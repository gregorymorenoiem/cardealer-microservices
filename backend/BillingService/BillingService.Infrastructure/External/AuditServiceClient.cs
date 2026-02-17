using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using BillingService.Application.Interfaces;

namespace BillingService.Infrastructure.External;

/// <summary>
/// HTTP client for centralized audit logging via AuditService.
/// Uses typed HttpClient with base address configured via DI.
/// </summary>
public class AuditServiceClient : IAuditServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuditServiceClient> _logger;

    public AuditServiceClient(HttpClient httpClient, ILogger<AuditServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task LogActionAsync(string entityType, string entityId, string action, string performedBy, string? details = null)
    {
        try
        {
            var payload = new
            {
                ServiceName = "BillingService",
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                PerformedBy = performedBy,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            await _httpClient.PostAsJsonAsync("/api/audit", payload);
            _logger.LogDebug("Audit log sent: {Action} on {EntityType}/{EntityId}", action, entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send audit log: {Action} on {EntityType}/{EntityId}", action, entityType, entityId);
        }
    }
}
