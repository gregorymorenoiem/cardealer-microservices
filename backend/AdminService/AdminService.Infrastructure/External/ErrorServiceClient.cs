using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AdminService.Application.Interfaces;
using ServiceDiscovery.Application.Interfaces;

namespace AdminService.Infrastructure.External
{
    public class ErrorServiceClient : IErrorServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ErrorServiceClient> _logger;
        private readonly IServiceDiscovery _serviceDiscovery;

        public ErrorServiceClient(HttpClient httpClient, ILogger<ErrorServiceClient> logger, IServiceDiscovery serviceDiscovery)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serviceDiscovery = serviceDiscovery;
        }

        private async Task<string> GetServiceUrlAsync()
        {
            try
            {
                var instance = await _serviceDiscovery.FindServiceInstanceAsync("ErrorService");
                return instance != null ? $"http://{instance.Host}:{instance.Port}" : "http://errorservice:80";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to discover ErrorService via Consul, using fallback URL");
                return "http://errorservice:80";
            }
        }

        public async Task LogErrorAsync(string exceptionType, string message, string? stackTrace = null, string? endpoint = null, int? statusCode = null)
        {
            try
            {
                var baseUrl = await GetServiceUrlAsync();
                var payload = new
                {
                    ServiceName = "AdminService",
                    ExceptionType = exceptionType,
                    Message = message,
                    StackTrace = stackTrace,
                    Endpoint = endpoint,
                    StatusCode = statusCode,
                    OccurredAt = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/api/errors", payload);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Error logged to ErrorService: {ExceptionType}", exceptionType);
            }
            catch (Exception ex)
            {
                // Log locally only - avoid infinite loop
                _logger.LogError(ex, "Failed to send error to ErrorService: {ExceptionType}", exceptionType);
            }
        }
    }
}
