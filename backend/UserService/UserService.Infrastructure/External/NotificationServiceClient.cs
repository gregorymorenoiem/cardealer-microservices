using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces;
using ServiceDiscovery.Application.Interfaces;

namespace UserService.Infrastructure.External
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

        public async Task SendWelcomeEmailAsync(string email, string firstName, string lastName)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var notification = new
                {
                    To = email,
                    Subject = "Welcome to CarDealer",
                    Body = $"Hello {firstName} {lastName},\n\nWelcome to CarDealer! Your account has been successfully created.",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Welcome email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send welcome email to {Email}", email);
            }
        }

        public async Task SendRoleAssignedNotificationAsync(string email, string roleName)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var notification = new
                {
                    To = email,
                    Subject = "Role Assigned",
                    Body = $"A new role '{roleName}' has been assigned to your account.",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Role assignment notification sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send role assignment notification to {Email}", email);
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var notification = new
                {
                    To = email,
                    Subject = "Password Reset Request",
                    Body = $"Click the following link to reset your password: https://cardealer.com/reset-password?token={resetToken}",
                    Type = "Email"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", notification);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Password reset email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send password reset email to {Email}", email);
            }
        }
    }
}
