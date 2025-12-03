using Consul;
using Gateway.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gateway.Infrastructure.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly IConsulClient _consulClient;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(IConsulClient consulClient, ILogger<HealthCheckService> logger)
    {
        _consulClient = consulClient;
        _logger = logger;
    }

    public async Task<bool> IsServiceHealthy(string serviceName)
    {
        try
        {
            // Query Consul for service health
            var services = await _consulClient.Health.Service(serviceName, null, true);

            // Check if any healthy instances exist
            return services?.Response?.Any() == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health for service {ServiceName}", serviceName);
            return false;
        }
    }

    public async Task<Dictionary<string, bool>> GetAllServicesHealth()
    {
        try
        {
            // Get all registered services from Consul catalog
            var servicesResult = await _consulClient.Catalog.Services();
            var serviceNames = servicesResult.Response.Keys;

            var healthStatus = new Dictionary<string, bool>();

            foreach (var serviceName in serviceNames)
            {
                // Skip Consul's internal service
                if (serviceName == "consul")
                    continue;

                var isHealthy = await IsServiceHealthy(serviceName);
                healthStatus[serviceName] = isHealthy;
            }

            return healthStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving services health status");
            return new Dictionary<string, bool>();
        }
    }
}
