using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AdminService.Application.Interfaces;
using ServiceDiscovery.Application.Interfaces;

namespace AdminService.Infrastructure.External
{
    public class AuditServiceClient : IAuditServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuditServiceClient> _logger;
        private readonly IServiceDiscovery _serviceDiscovery;

        public AuditServiceClient(HttpClient httpClient, ILogger<AuditServiceClient> logger, IServiceDiscovery serviceDiscovery)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serviceDiscovery = serviceDiscovery;
        }

        private async Task<string> GetServiceUrlAsync()
        {
            try
            {
                var instance = await _serviceDiscovery.FindServiceInstanceAsync("AuditService");
                return $"http://{instance.Host}:{instance.Port}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to discover AuditService via Consul, using fallback URL");
                return "http://auditservice:80";
            }
        }

        public async Task LogVehicleApprovedAsync(Guid vehicleId, string approvedBy, string reason)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var payload = new
                {
                    EntityType = "Vehicle",
                    EntityId = vehicleId,
                    Action = "Approved",
                    PerformedBy = approvedBy,
                    Details = $"Vehicle approved. Reason: {reason}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", payload);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Audit log sent for vehicle approval: {VehicleId}", vehicleId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for vehicle approval: {VehicleId}", vehicleId);
            }
        }

        public async Task LogVehicleRejectedAsync(Guid vehicleId, string rejectedBy, string reason)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var payload = new
                {
                    EntityType = "Vehicle",
                    EntityId = vehicleId,
                    Action = "Rejected",
                    PerformedBy = rejectedBy,
                    Details = $"Vehicle rejected. Reason: {reason}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", payload);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Audit log sent for vehicle rejection: {VehicleId}", vehicleId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for vehicle rejection: {VehicleId}", vehicleId);
            }
        }

        public async Task LogReportResolvedAsync(Guid reportId, string resolvedBy, string resolution)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var payload = new
                {
                    EntityType = "Report",
                    EntityId = reportId,
                    Action = "Resolved",
                    PerformedBy = resolvedBy,
                    Details = $"Report resolved. Resolution: {resolution}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", payload);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Audit log sent for report resolution: {ReportId}", reportId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for report resolution: {ReportId}", reportId);
            }
        }

        public async Task LogUserActionAsync(Guid userId, string action, string performedBy, string details)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var payload = new
                {
                    EntityType = "User",
                    EntityId = userId,
                    Action = action,
                    PerformedBy = performedBy,
                    Details = details,
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", payload);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Audit log sent for user action: {Action} on {UserId}", action, userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for user action: {Action} on {UserId}", action, userId);
            }
        }

        public async Task LogSystemConfigChangedAsync(string configKey, string oldValue, string newValue, string changedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var payload = new
                {
                    EntityType = "SystemConfig",
                    EntityId = Guid.NewGuid(),
                    Action = "ConfigChanged",
                    PerformedBy = changedBy,
                    Details = $"Config '{configKey}' changed from '{oldValue}' to '{newValue}'",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", payload);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Audit log sent for config change: {ConfigKey}", configKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for config change: {ConfigKey}", configKey);
            }
        }
    }
}
