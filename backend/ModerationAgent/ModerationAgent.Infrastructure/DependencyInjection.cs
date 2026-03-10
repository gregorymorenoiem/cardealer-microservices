using CarDealer.Shared.LlmGateway.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModerationAgent.Domain.Interfaces;
using ModerationAgent.Infrastructure.Services;

namespace ModerationAgent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLlmGateway(configuration);
        services.AddScoped<ILlmModerationService, LlmModerationService>();

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
            services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnection; options.InstanceName = "ModerationAgent:"; });
        else
            services.AddDistributedMemoryCache();

        services.AddMemoryCache();
        return services;
    }
}
