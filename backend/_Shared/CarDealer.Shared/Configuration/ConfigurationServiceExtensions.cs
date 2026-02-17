using Microsoft.Extensions.DependencyInjection;

namespace CarDealer.Shared.Configuration;

/// <summary>
/// Extension methods to register the ConfigurationServiceClient in any microservice.
/// Usage in Program.cs:
///   builder.Services.AddConfigurationServiceClient(builder.Configuration);
/// </summary>
public static class ConfigurationServiceExtensions
{
    /// <summary>
    /// Registers IConfigurationServiceClient backed by HTTP calls to ConfigurationService.
    /// Reads base URL from configuration key "Services:ConfigurationService" 
    /// (defaults to http://localhost:15124 for development).
    /// </summary>
    public static IServiceCollection AddConfigurationServiceClient(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        string? serviceName = null)
    {
        var configServiceUrl = configuration["Services:ConfigurationService"]
            ?? "http://localhost:15124";

        services.AddMemoryCache();

        services.AddHttpClient<ConfigurationServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configServiceUrl);
            client.DefaultRequestHeaders.Add("User-Agent", serviceName ?? "Microservice");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddSingleton<IConfigurationServiceClient>(sp =>
            sp.GetRequiredService<ConfigurationServiceClient>());

        return services;
    }
}
