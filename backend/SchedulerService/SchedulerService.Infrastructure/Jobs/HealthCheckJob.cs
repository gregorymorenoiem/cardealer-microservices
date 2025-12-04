using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SchedulerService.Infrastructure.Jobs;

/// <summary>
/// Job that performs health checks on all configured services
/// </summary>
public class HealthCheckJob : IScheduledJob
{
    private readonly ILogger<HealthCheckJob> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private const int DefaultTimeoutSeconds = 10;

    // Default services to check if none are specified
    private static readonly string[] DefaultServices =
    {
        "http://authservice:5001/health",
        "http://userservice:5002/health",
        "http://vehicleservice:5003/health",
        "http://notificationservice:5010/health"
    };

    public HealthCheckJob(
        ILogger<HealthCheckJob> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task ExecuteAsync(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("Starting health check for all services...");

        try
        {
            var serviceUrls = parameters.TryGetValue("Services", out var servicesParam)
                ? servicesParam.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : DefaultServices;

            var timeout = parameters.TryGetValue("TimeoutSeconds", out var timeoutStr)
                ? TimeSpan.FromSeconds(int.Parse(timeoutStr))
                : TimeSpan.FromSeconds(DefaultTimeoutSeconds);

            var results = new List<HealthCheckResult>();
            var client = _httpClientFactory.CreateClient();
            client.Timeout = timeout;

            foreach (var serviceUrl in serviceUrls)
            {
                var result = await CheckServiceHealthAsync(client, serviceUrl);
                results.Add(result);

                var statusLog = result.IsHealthy ? "healthy" : "unhealthy";
                _logger.LogInformation(
                    "Service {Service} is {Status} (ResponseTime: {ResponseTime}ms)",
                    result.ServiceName,
                    statusLog,
                    result.ResponseTimeMs);
            }

            // Log summary
            var healthyCount = results.Count(r => r.IsHealthy);
            var unhealthyCount = results.Count(r => !r.IsHealthy);

            _logger.LogInformation(
                "Health check completed. Total: {Total}, Healthy: {Healthy}, Unhealthy: {Unhealthy}",
                results.Count,
                healthyCount,
                unhealthyCount);

            // Alert on unhealthy services
            if (unhealthyCount > 0)
            {
                var unhealthyServices = results.Where(r => !r.IsHealthy).Select(r => r.ServiceName);
                _logger.LogWarning(
                    "Unhealthy services detected: {UnhealthyServices}",
                    string.Join(", ", unhealthyServices));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check job execution");
            throw;
        }
    }

    private async Task<HealthCheckResult> CheckServiceHealthAsync(HttpClient client, string url)
    {
        var serviceName = ExtractServiceName(url);
        var startTime = DateTime.UtcNow;

        try
        {
            var response = await client.GetAsync(url);
            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            return new HealthCheckResult
            {
                ServiceName = serviceName,
                Url = url,
                IsHealthy = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                ResponseTimeMs = (long)responseTime,
                CheckedAt = DateTime.UtcNow
            };
        }
        catch (HttpRequestException ex)
        {
            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogWarning("Health check failed for {Service}: {Error}", serviceName, ex.Message);

            return new HealthCheckResult
            {
                ServiceName = serviceName,
                Url = url,
                IsHealthy = false,
                StatusCode = 0,
                ResponseTimeMs = (long)responseTime,
                ErrorMessage = ex.Message,
                CheckedAt = DateTime.UtcNow
            };
        }
        catch (TaskCanceledException)
        {
            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogWarning("Health check timed out for {Service}", serviceName);

            return new HealthCheckResult
            {
                ServiceName = serviceName,
                Url = url,
                IsHealthy = false,
                StatusCode = 0,
                ResponseTimeMs = (long)responseTime,
                ErrorMessage = "Request timed out",
                CheckedAt = DateTime.UtcNow
            };
        }
    }

    private static string ExtractServiceName(string url)
    {
        try
        {
            var uri = new Uri(url);
            return uri.Host;
        }
        catch
        {
            return url;
        }
    }
}

/// <summary>
/// Result of a single service health check
/// </summary>
public record HealthCheckResult
{
    public string ServiceName { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public bool IsHealthy { get; init; }
    public int StatusCode { get; init; }
    public long ResponseTimeMs { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime CheckedAt { get; init; }
}
