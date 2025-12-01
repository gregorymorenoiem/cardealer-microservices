using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RoleService.Application.Interfaces;

namespace RoleService.Infrastructure.External
{
    public class NotificationServiceClient : INotificationServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationServiceClient> _logger;

        public NotificationServiceClient(HttpClient httpClient, ILogger<NotificationServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendRoleCreatedNotificationAsync(string adminEmail, string roleName)
        {
            try
            {
                var notification = new
                {
                    To = adminEmail,
                    Subject = "New Role Created",
                    Body = $"A new role '{roleName}' has been created in the system.",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync("/api/notifications/email", notification);
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
                var notification = new
                {
                    To = adminEmail,
                    Subject = $"CRITICAL: Role {action}",
                    Body = $"Critical role change: Role '{roleName}' has been {action}. This may affect user permissions.",
                    Type = "Email",
                    Priority = "High"
                };

                var response = await _httpClient.PostAsJsonAsync("/api/notifications/email", notification);
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
