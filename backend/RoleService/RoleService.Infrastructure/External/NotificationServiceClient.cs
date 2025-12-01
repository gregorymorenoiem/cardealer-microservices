using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RoleService.Application.Interfaces;
using ServiceDiscovery.Application.Interfaces;

namespace RoleService.Infrastructure.External
{
    public class NotificationServiceClient : INotificationServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationServiceClient> _logger;
        private readonly IServiceDiscovery _serviceDiscovery;

        public NotificationServiceClient(HttpClient httpClient, ILogger<NotificationServiceClient> logger, IServiceDiscovery serviceDiscovery)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serviceDiscovery = serviceDiscovery;
        }

        private async Task<string> GetServiceUrlAsync()
        {
            try
            {
                var instance = await _serviceDiscovery.FindServiceInstanceAsync("NotificationService");
                return instance != null ? $"http://{instance.Host}:{instance.Port}" : "http://notificationservice:80";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error resolving NotificationService from Consul, using fallback");
                return "http://notificationservice:80";
            }
        }

        public async Task SendRoleCreatedNotificationAsync(string adminEmail, string roleName)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var notification = new
                {
                    To = adminEmail,
                    Subject = "New Role Created",
                    Body = $"A new role '{roleName}' has been created in the system.",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Role creation notification sent to {Email}", adminEmail);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send role creation notification");
            }
        }

        public async Task SendCriticalRoleChangedNotificationAsync(string adminEmail, string roleName, string action)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var notification = new
                {
                    To = adminEmail,
                    Subject = $"CRITICAL: Role {action}",
                    Body = $"Critical role change: Role '{roleName}' has been {action}. This may affect user permissions.",
                    Type = "Email",
                    Priority = "High"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Critical role change notification sent to {Email}", adminEmail);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send critical role change notification");
            }
        }
    }
}
