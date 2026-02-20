using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Clients;

public class AuditServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuditServiceClient> _logger;

    public AuditServiceClient(HttpClient httpClient, ILogger<AuditServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task LogActionAsync(AuditLogRequest request, CancellationToken ct = default)
    {
        try
        {
            await _httpClient.PostAsJsonAsync("/api/audit/logs", request, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send audit log for action {Action}", request.Action);
        }
    }
}

public class AuditLogRequest
{
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
