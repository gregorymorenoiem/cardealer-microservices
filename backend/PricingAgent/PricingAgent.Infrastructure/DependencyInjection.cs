using CarDealer.Shared.LlmGateway.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PricingAgent.Domain.Interfaces;
using PricingAgent.Infrastructure.Services;

namespace PricingAgent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // LLM Gateway (Claude → Gemini → Llama cascade)
        services.AddLlmGateway(configuration);

        // LLM Pricing Service
        services.AddScoped<ILlmPricingService, LlmPricingService>();

        // Redis Cache
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "PricingAgent:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddMemoryCache();

        return services;
    }
}
