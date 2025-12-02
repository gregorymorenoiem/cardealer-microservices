using System.Collections.Concurrent;
using HealthCheckService.Domain.Entities;
using HealthCheckService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HealthCheckService.Infrastructure.Services;

public class HealthAggregatorService : IHealthAggregator
{
    private readonly ConcurrentDictionary<string, string> _registeredServices = new();
    private readonly IHealthChecker _healthChecker;
    private readonly ILogger<HealthAggregatorService> _logger;

    public HealthAggregatorService(IHealthChecker healthChecker, ILogger<HealthAggregatorService> logger)
    {
        _healthChecker = healthChecker;
        _logger = logger;
    }

    public async Task<SystemHealth> GetSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Aggregating system health from {ServiceCount} services", _registeredServices.Count);

        var systemHealth = new SystemHealth();

        // Check all registered services in parallel
        var healthCheckTasks = _registeredServices.Select(async kvp =>
        {
            try
            {
                var serviceHealth = await _healthChecker.CheckServiceHealthAsync(kvp.Value, cancellationToken);
                serviceHealth.ServiceName = kvp.Key;
                return serviceHealth;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check health for service {ServiceName}", kvp.Key);
                return new ServiceHealth
                {
                    ServiceName = kvp.Key,
                    ServiceUrl = kvp.Value,
                    Status = Domain.Enums.HealthStatus.Unknown,
                    Description = $"Health check failed: {ex.Message}",
                    CheckedAt = DateTime.UtcNow
                };
            }
        });

        systemHealth.Services = (await Task.WhenAll(healthCheckTasks)).ToList();
        systemHealth.CalculateOverallStatus();

        _logger.LogInformation(
            "System health check completed: {OverallStatus}, {HealthyCount}/{TotalCount} services healthy",
            systemHealth.OverallStatus,
            systemHealth.HealthyServices,
            systemHealth.TotalServices);

        return systemHealth;
    }

    public async Task<ServiceHealth?> GetServiceHealthAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        if (!_registeredServices.TryGetValue(serviceName, out var serviceUrl))
        {
            _logger.LogWarning("Service {ServiceName} is not registered", serviceName);
            return null;
        }

        _logger.LogDebug("Checking health for service {ServiceName}", serviceName);

        var serviceHealth = await _healthChecker.CheckServiceHealthAsync(serviceUrl, cancellationToken);
        serviceHealth.ServiceName = serviceName;

        return serviceHealth;
    }

    public void RegisterService(string serviceName, string serviceUrl)
    {
        if (_registeredServices.TryAdd(serviceName, serviceUrl))
        {
            _logger.LogInformation("Registered service {ServiceName} at {ServiceUrl}", serviceName, serviceUrl);
        }
        else
        {
            _logger.LogWarning("Service {ServiceName} is already registered", serviceName);
        }
    }

    public void UnregisterService(string serviceName)
    {
        if (_registeredServices.TryRemove(serviceName, out _))
        {
            _logger.LogInformation("Unregistered service {ServiceName}", serviceName);
        }
        else
        {
            _logger.LogWarning("Service {ServiceName} was not found for unregistration", serviceName);
        }
    }

    public IEnumerable<string> GetRegisteredServices()
    {
        return _registeredServices.Keys.ToList();
    }
}
