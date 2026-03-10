using CarDealer.Shared.LlmGateway.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AnalyticsAgent.Domain.Interfaces;
using AnalyticsAgent.Infrastructure.Services;

namespace AnalyticsAgent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLlmGateway(configuration);
        services.AddScoped<ILlmAnalyticsService, LlmAnalyticsService>();

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
            services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnection; options.InstanceName = "AnalyticsAgent:"; });
        else
            services.AddDistributedMemoryCache();

        services.AddMemoryCache();
        return services;
    }
}
