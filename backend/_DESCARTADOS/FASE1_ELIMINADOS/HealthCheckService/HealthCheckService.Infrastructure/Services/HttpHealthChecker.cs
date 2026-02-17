using System.Diagnostics;
using System.Text.Json;
using HealthCheckService.Domain.Entities;
using HealthCheckService.Domain.Enums;
using HealthCheckService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HealthCheckService.Infrastructure.Services;

public class HttpHealthChecker : IHealthChecker
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpHealthChecker> _logger;

    public HttpHealthChecker(HttpClient httpClient, ILogger<HttpHealthChecker> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<ServiceHealth> CheckServiceHealthAsync(string serviceUrl, CancellationToken cancellationToken = default)
    {
        var serviceHealth = new ServiceHealth
        {
            ServiceUrl = serviceUrl,
            CheckedAt = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Try to get health endpoint
            var healthUrl = $"{serviceUrl.TrimEnd('/')}/health";
            var response = await _httpClient.GetAsync(healthUrl, cancellationToken);

            stopwatch.Stop();
            serviceHealth.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                // Try to parse health check response
                try
                {
                    var healthData = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                    serviceHealth.Status = HealthStatus.Healthy;
                    serviceHealth.Description = "Service is healthy";

                    // Extract service name if available
                    if (healthData?.ContainsKey("name") == true)
                    {
                        serviceHealth.ServiceName = healthData["name"].ToString() ?? "Unknown";
                    }
                }
                catch
                {
                    // If we can't parse, but got 200, consider it healthy
                    serviceHealth.Status = HealthStatus.Healthy;
                    serviceHealth.Description = "Service responded successfully";
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                serviceHealth.Status = HealthStatus.Degraded;
                serviceHealth.Description = $"Service returned {response.StatusCode}";
            }
            else
            {
                serviceHealth.Status = HealthStatus.Unhealthy;
                serviceHealth.Description = $"Service returned {response.StatusCode}";
            }
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            serviceHealth.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            serviceHealth.Status = HealthStatus.Unhealthy;
            serviceHealth.Description = "Service health check timed out";
            _logger.LogWarning("Health check timeout for {ServiceUrl}", serviceUrl);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            serviceHealth.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            serviceHealth.Status = HealthStatus.Unhealthy;
            serviceHealth.Description = $"Service unreachable: {ex.Message}";
            _logger.LogError(ex, "Health check failed for {ServiceUrl}", serviceUrl);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            serviceHealth.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            serviceHealth.Status = HealthStatus.Unknown;
            serviceHealth.Description = $"Health check error: {ex.Message}";
            _logger.LogError(ex, "Unexpected error during health check for {ServiceUrl}", serviceUrl);
        }

        return serviceHealth;
    }

    public async Task<bool> IsServiceReachableAsync(string serviceUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthUrl = $"{serviceUrl.TrimEnd('/')}/health";
            var response = await _httpClient.GetAsync(healthUrl, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
