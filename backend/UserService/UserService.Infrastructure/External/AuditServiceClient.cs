using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces;
using ServiceDiscovery.Application.Interfaces;

namespace UserService.Infrastructure.External
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

        public async Task LogUserCreatedAsync(Guid userId, string email, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var auditLog = new
                {
                    EntityType = "User",
                    EntityId = userId.ToString(),
                    Action = "Created",
                    PerformedBy = performedBy,
                    Details = $"User created: {email}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Audit log sent for user creation: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for user creation: {UserId}", userId);
                // No lanzamos la excepción para no afectar el flujo principal
            }
        }

        public async Task LogUserUpdatedAsync(Guid userId, string changes, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var auditLog = new
                {
                    EntityType = "User",
                    EntityId = userId.ToString(),
                    Action = "Updated",
                    PerformedBy = performedBy,
                    Details = changes,
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Audit log sent for user update: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for user update: {UserId}", userId);
            }
        }

        public async Task LogUserDeletedAsync(Guid userId, string email, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var auditLog = new
                {
                    EntityType = "User",
                    EntityId = userId.ToString(),
                    Action = "Deleted",
                    PerformedBy = performedBy,
                    Details = $"User deleted: {email}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Audit log sent for user deletion: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for user deletion: {UserId}", userId);
            }
        }

        public async Task LogRoleAssignedAsync(Guid userId, Guid roleId, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var auditLog = new
                {
                    EntityType = "UserRole",
                    EntityId = userId.ToString(),
                    Action = "RoleAssigned",
                    PerformedBy = performedBy,
                    Details = $"Role {roleId} assigned to user {userId}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Audit log sent for role assignment: User {UserId}, Role {RoleId}", userId, roleId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for role assignment");
            }
        }

        public async Task LogRoleRevokedAsync(Guid userId, Guid roleId, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var auditLog = new
                {
                    EntityType = "UserRole",
                    EntityId = userId.ToString(),
                    Action = "RoleRevoked",
                    PerformedBy = performedBy,
                    Details = $"Role {roleId} revoked from user {userId}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Audit log sent for role revocation: User {UserId}, Role {RoleId}", userId, roleId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for role revocation");
            }
        }

        public async Task LogSellerConversionAsync(Guid userId, Guid sellerProfileId, string previousAccountType, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var auditLog = new
                {
                    EntityType = "SellerConversion",
                    EntityId = sellerProfileId.ToString(),
                    Action = "USER_CONVERTED_TO_SELLER",
                    PerformedBy = performedBy,
                    Details = $"User {userId} converted from {previousAccountType} to Seller. SellerProfileId: {sellerProfileId}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation(
                    "Audit log sent for seller conversion: User {UserId}, SellerProfile {SellerProfileId}",
                    userId, sellerProfileId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for seller conversion: User {UserId}", userId);
            }
        }

        public async Task LogDealerRegistrationAsync(Guid dealerId, Guid ownerUserId, string businessName, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var auditLog = new
                {
                    EntityType = "Dealer",
                    EntityId = dealerId.ToString(),
                    Action = "DEALER_REGISTRATION_REQUESTED",
                    PerformedBy = performedBy,
                    Details = $"Dealer registration requested: {businessName} (DealerId: {dealerId}) by user {ownerUserId}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation(
                    "Audit log sent for dealer registration: Dealer {DealerId}, Owner {OwnerId}",
                    dealerId, ownerUserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for dealer registration: Dealer {DealerId}", dealerId);
            }
        }

        public async Task LogDealerVerificationAsync(Guid dealerId, bool isApproved, string performedBy)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var action = isApproved ? "DEALER_APPROVED" : "DEALER_REJECTED";
                var auditLog = new
                {
                    EntityType = "Dealer",
                    EntityId = dealerId.ToString(),
                    Action = action,
                    PerformedBy = performedBy,
                    Details = $"Dealer {dealerId} {(isApproved ? "approved" : "rejected")} by admin {performedBy}",
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/audit", auditLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation(
                    "Audit log sent for dealer verification: Dealer {DealerId}, Approved={IsApproved}",
                    dealerId, isApproved);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for dealer verification: Dealer {DealerId}", dealerId);
            }
        }
    }
}
