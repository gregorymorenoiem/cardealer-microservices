using CarDealer.Shared.LlmGateway.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ListingAgent.Domain.Interfaces;
using ListingAgent.Infrastructure.Services;
using StackExchange.Redis;

namespace ListingAgent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLlmGateway(configuration);

        // Redis connection — registered as singleton so the multiplexer is shared
        var redisConnection = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrEmpty(redisConnection))
        {
            var multiplexer = ConnectionMultiplexer.Connect(redisConnection);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "ListingAgent:";
            });
        }
        else
        {
            // No Redis configured: register a null object pattern via factory
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false"));
            services.AddDistributedMemoryCache();
        }

        // Cache hit/miss metrics — uses the IConnectionMultiplexer registered above
        services.AddSingleton<IListingCacheMetrics, RedisListingCacheMetrics>();

        services.AddScoped<ILlmListingService, LlmListingService>();
        services.AddMemoryCache();
        return services;
    }
}
