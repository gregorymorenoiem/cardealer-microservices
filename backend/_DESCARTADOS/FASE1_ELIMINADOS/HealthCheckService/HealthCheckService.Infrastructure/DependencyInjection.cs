using HealthCheckService.Domain.Interfaces;
using HealthCheckService.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HealthCheckService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register HttpClient for health checking
        services.AddHttpClient<IHealthChecker, HttpHealthChecker>();

        // Register HealthAggregator as singleton to maintain state
        services.AddSingleton<IHealthAggregator, HealthAggregatorService>();

        return services;
    }

    public static IServiceCollection RegisterServicesForHealthCheck(
        this IServiceCollection services,
        Dictionary<string, string> serviceMappings)
    {
        var serviceProvider = services.BuildServiceProvider();
        var healthAggregator = serviceProvider.GetRequiredService<IHealthAggregator>();

        foreach (var mapping in serviceMappings)
        {
            healthAggregator.RegisterService(mapping.Key, mapping.Value);
        }

        return services;
    }
}
