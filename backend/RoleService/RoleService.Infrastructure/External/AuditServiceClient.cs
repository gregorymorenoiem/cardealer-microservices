using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RoleService.Application.Interfaces;
using ServiceDiscovery.Application.Interfaces;

namespace RoleService.Infrastructure.External
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
                return instance != null ? $"http://{instance.Host}:{instance.Port}" : "http://auditservice:80";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error resolving AuditService from Consul, using fallback");
                return "http://auditservice:80";
            }
        }

        public async Task LogRoleCreatedAsync(Guid roleId, string roleName, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var auditLog = new
                {
                    EntityType = "Role",
                    EntityId = roleId.ToString(),
                    Action = "Created",
                    PerformedBy = performedBy,
                    Details = $"Role created: {roleName}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Audit log sent for role creation: {RoleId}", roleId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for role creation: {RoleId}", roleId);
            }
        }

        public async Task LogRoleUpdatedAsync(Guid roleId, string changes, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var auditLog = new
                {
                    EntityType = "Role",
                    EntityId = roleId.ToString(),
                    Action = "Updated",
                    PerformedBy = performedBy,
                    Details = changes,
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Audit log sent for role update: {RoleId}", roleId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for role update: {RoleId}", roleId);
            }
        }

        public async Task LogRoleDeletedAsync(Guid roleId, string roleName, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var auditLog = new
                {
                    EntityType = "Role",
                    EntityId = roleId.ToString(),
                    Action = "Deleted",
                    PerformedBy = performedBy,
                    Details = $"Role deleted: {roleName}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Audit log sent for role deletion: {RoleId}", roleId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for role deletion: {RoleId}", roleId);
            }
        }

        public async Task LogPermissionAssignedAsync(Guid roleId, Guid permissionId, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var auditLog = new
                {
                    EntityType = "RolePermission",
                    EntityId = roleId.ToString(),
                    Action = "PermissionAssigned",
                    PerformedBy = performedBy,
                    Details = $"Permission {permissionId} assigned to role {roleId}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Audit log sent for permission assignment");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for permission assignment");
            }
        }

        public async Task LogPermissionCreatedAsync(Guid permissionId, string permissionName, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var auditLog = new
                {
                    EntityType = "Permission",
                    EntityId = permissionId.ToString(),
                    Action = "Created",
                    PerformedBy = performedBy,
                    Details = $"Permission created: {permissionName}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Audit log sent for permission creation: {PermissionId}", permissionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for permission creation: {PermissionId}", permissionId);
            }
        }

        public async Task LogPermissionRemovedAsync(Guid roleId, Guid permissionId, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var auditLog = new
                {
                    EntityType = "RolePermission",
                    EntityId = roleId.ToString(),
                    Action = "PermissionRemoved",
                    PerformedBy = performedBy,
                    Details = $"Permission {permissionId} removed from role {roleId}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Audit log sent for permission removal");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for permission removal");
            }
        }
    }
}
