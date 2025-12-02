using ApiDocsService.Core.Interfaces;
using ApiDocsService.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace ApiDocsService.Core.Services;

/// <summary>
/// Configuration for registered services
/// </summary>
public class ServicesConfiguration
{
    public List<ServiceInfo> Services { get; set; } = new();
}

/// <summary>
/// Implementation of API documentation aggregator
/// </summary>
public class ApiAggregatorService : IApiAggregatorService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiAggregatorService> _logger;
    private readonly List<ServiceInfo> _services;
    private readonly Dictionary<string, AggregatedApiDoc> _cachedDocs = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private DateTime _lastRefresh = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public ApiAggregatorService(
        HttpClient httpClient,
        IOptions<ServicesConfiguration> options,
        ILogger<ApiAggregatorService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _services = options.Value.Services;
    }

    public Task<List<ServiceInfo>> GetAllServicesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_services.OrderBy(s => s.Order).ThenBy(s => s.Name).ToList());
    }

    public Task<ServiceInfo?> GetServiceByNameAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        var service = _services.FirstOrDefault(s =>
            s.Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(service);
    }

    public Task<List<ServiceInfo>> GetServicesByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var services = _services
            .Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.Order)
            .ThenBy(s => s.Name)
            .ToList();
        return Task.FromResult(services);
    }

    public async Task<List<ServiceStatus>> CheckAllServicesHealthAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _services.Select(s => CheckServiceHealthAsync(s.Name, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }

    public async Task<ServiceStatus> CheckServiceHealthAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        var service = _services.FirstOrDefault(s =>
            s.Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

        if (service == null)
        {
            return new ServiceStatus
            {
                ServiceName = serviceName,
                IsAvailable = false,
                ErrorMessage = "Service not registered"
            };
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var healthUrl = $"{service.BaseUrl.TrimEnd('/')}/health";
            var response = await _httpClient.GetAsync(healthUrl, cancellationToken);
            stopwatch.Stop();

            var status = new ServiceStatus
            {
                ServiceName = serviceName,
                IsAvailable = response.IsSuccessStatusCode,
                HealthStatus = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                CheckedAt = DateTime.UtcNow
            };

            // Update service health status
            service.IsHealthy = status.IsAvailable;
            service.LastChecked = status.CheckedAt;

            return status;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex, "Health check failed for {ServiceName}", serviceName);

            service.IsHealthy = false;
            service.LastChecked = DateTime.UtcNow;

            return new ServiceStatus
            {
                ServiceName = serviceName,
                IsAvailable = false,
                HealthStatus = "Unreachable",
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ErrorMessage = ex.Message,
                CheckedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<string?> GetOpenApiSpecAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        var service = _services.FirstOrDefault(s =>
            s.Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

        if (service == null)
        {
            _logger.LogWarning("Service {ServiceName} not found", serviceName);
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(service.SwaggerUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }

            _logger.LogWarning("Failed to get OpenAPI spec for {ServiceName}: {StatusCode}",
                serviceName, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching OpenAPI spec for {ServiceName}", serviceName);
            return null;
        }
    }

    public async Task<List<AggregatedApiDoc>> GetAllApiDocsAsync(CancellationToken cancellationToken = default)
    {
        // Check if cache needs refresh
        if (DateTime.UtcNow - _lastRefresh > _cacheExpiration || !_cachedDocs.Any())
        {
            await RefreshAllDocsAsync(cancellationToken);
        }

        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            return _cachedDocs.Values.ToList();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<ApiDocsDashboard> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var healthStatuses = await CheckAllServicesHealthAsync(cancellationToken);
        var docs = await GetAllApiDocsAsync(cancellationToken);

        var dashboard = new ApiDocsDashboard
        {
            TotalServices = _services.Count,
            HealthyServices = healthStatuses.Count(s => s.IsAvailable),
            UnhealthyServices = healthStatuses.Count(s => !s.IsAvailable),
            TotalEndpoints = docs.Sum(d => d.Endpoints.Count),
            GeneratedAt = DateTime.UtcNow
        };

        // Group by category
        dashboard.ByCategory = _services
            .GroupBy(s => s.Category)
            .Select(g => new CategoryStats
            {
                Category = g.Key,
                ServiceCount = g.Count(),
                EndpointCount = docs
                    .Where(d => g.Any(s => s.Name == d.ServiceName))
                    .Sum(d => d.Endpoints.Count)
            })
            .OrderBy(c => c.Category)
            .ToList();

        return dashboard;
    }

    public async Task<List<ApiEndpointInfo>> SearchEndpointsAsync(string query, CancellationToken cancellationToken = default)
    {
        var docs = await GetAllApiDocsAsync(cancellationToken);
        var queryLower = query.ToLowerInvariant();

        var results = docs
            .SelectMany(d => d.Endpoints.Select(e => new { ServiceName = d.ServiceName, Endpoint = e }))
            .Where(x =>
                x.Endpoint.Path.ToLowerInvariant().Contains(queryLower) ||
                (x.Endpoint.Summary?.ToLowerInvariant().Contains(queryLower) ?? false) ||
                (x.Endpoint.Description?.ToLowerInvariant().Contains(queryLower) ?? false) ||
                x.Endpoint.Tags.Any(t => t.ToLowerInvariant().Contains(queryLower)))
            .Select(x => x.Endpoint)
            .ToList();

        return results;
    }

    public async Task RefreshAllDocsAsync(CancellationToken cancellationToken = default)
    {
        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            _cachedDocs.Clear();

            var tasks = _services.Select(async service =>
            {
                var doc = new AggregatedApiDoc
                {
                    ServiceName = service.Name,
                    DisplayName = service.DisplayName,
                    Version = service.Version,
                    RetrievedAt = DateTime.UtcNow
                };

                try
                {
                    var spec = await GetOpenApiSpecAsync(service.Name, cancellationToken);

                    if (!string.IsNullOrEmpty(spec))
                    {
                        doc.OpenApiSpec = spec;
                        doc.IsAvailable = true;
                        doc.Endpoints = ParseEndpointsFromOpenApi(spec);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to refresh docs for {ServiceName}", service.Name);
                    doc.IsAvailable = false;
                }

                return doc;
            });

            var results = await Task.WhenAll(tasks);

            foreach (var doc in results)
            {
                _cachedDocs[doc.ServiceName] = doc;
            }

            _lastRefresh = DateTime.UtcNow;
            _logger.LogInformation("Refreshed API documentation for {Count} services", results.Length);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private List<ApiEndpointInfo> ParseEndpointsFromOpenApi(string openApiSpec)
    {
        var endpoints = new List<ApiEndpointInfo>();

        try
        {
            using var doc = JsonDocument.Parse(openApiSpec);
            var root = doc.RootElement;

            if (root.TryGetProperty("paths", out var paths))
            {
                foreach (var path in paths.EnumerateObject())
                {
                    foreach (var method in path.Value.EnumerateObject())
                    {
                        var endpoint = new ApiEndpointInfo
                        {
                            Path = path.Name,
                            Method = method.Name.ToUpper()
                        };

                        if (method.Value.TryGetProperty("summary", out var summary))
                            endpoint.Summary = summary.GetString();

                        if (method.Value.TryGetProperty("description", out var description))
                            endpoint.Description = description.GetString();

                        if (method.Value.TryGetProperty("operationId", out var operationId))
                            endpoint.OperationId = operationId.GetString();

                        if (method.Value.TryGetProperty("tags", out var tags))
                        {
                            foreach (var tag in tags.EnumerateArray())
                            {
                                var tagValue = tag.GetString();
                                if (!string.IsNullOrEmpty(tagValue))
                                    endpoint.Tags.Add(tagValue);
                            }
                        }

                        if (method.Value.TryGetProperty("security", out var security))
                        {
                            endpoint.RequiresAuth = security.EnumerateArray().Any();
                        }

                        endpoints.Add(endpoint);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse OpenAPI spec");
        }

        return endpoints;
    }
}
