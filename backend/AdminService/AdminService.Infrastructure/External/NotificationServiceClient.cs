using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AdminService.Application.Interfaces;
using ServiceDiscovery.Application.Interfaces;

namespace AdminService.Infrastructure.External
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
                _logger.LogWarning(ex, "Failed to discover NotificationService via Consul, using fallback URL");
                return "http://notificationservice:80";
            }
        }

        public async Task SendVehicleApprovedNotificationAsync(string ownerEmail, string vehicleTitle)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var payload = new
                {
                    To = ownerEmail,
                    Subject = "Vehicle Approved",
                    Body = $"Your vehicle '{vehicleTitle}' has been approved and is now live on the platform.",
                    Type = "VehicleApproved"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", payload);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Vehicle approved notification sent to {Email}", ownerEmail);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send vehicle approved notification to {Email}", ownerEmail);
            }
        }

        public async Task SendVehicleRejectedNotificationAsync(string ownerEmail, string vehicleTitle, string reason)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var payload = new
                {
                    To = ownerEmail,
                    Subject = "Vehicle Rejected",
                    Body = $"Your vehicle '{vehicleTitle}' was rejected. Reason: {reason}. Please review and resubmit.",
                    Type = "VehicleRejected"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", payload);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Vehicle rejected notification sent to {Email}", ownerEmail);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send vehicle rejected notification to {Email}", ownerEmail);
            }
        }

        public async Task SendAdminAlertAsync(string adminEmail, string subject, string message)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var payload = new
                {
                    To = adminEmail,
                    Subject = subject,
                    Body = message,
                    Type = "AdminAlert",
                    Priority = "High"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", payload);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Admin alert sent to {Email}: {Subject}", adminEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send admin alert to {Email}", adminEmail);
            }
        }

        public async Task SendReportResolvedNotificationAsync(string reporterEmail, string reportSubject, string resolution)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var payload = new
                {
                    To = reporterEmail,
                    Subject = "Report Resolved",
                    Body = $"Your report '{reportSubject}' has been resolved. Resolution: {resolution}",
                    Type = "ReportResolved"
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/notifications/email", payload);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Report resolved notification sent to {Email}", reporterEmail);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send report resolved notification to {Email}", reporterEmail);
            }
        }
    }
}
