using CarDealer.Shared.FeatureFlags.Configuration;
using CarDealer.Shared.FeatureFlags.Interfaces;
using CarDealer.Shared.FeatureFlags.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace CarDealer.Shared.FeatureFlags.Extensions;

/// <summary>
/// Extensiones para registrar el cliente de Feature Flags
/// </summary>
public static class FeatureFlagExtensions
{
    /// <summary>
    /// Agrega el cliente de Feature Flags con configuración automática
    /// </summary>
    public static IServiceCollection AddFeatureFlags(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FeatureFlagOptions>(
            configuration.GetSection(FeatureFlagOptions.SectionName));

        services.AddMemoryCache();

        services.AddHttpClient<IFeatureFlagClient, FeatureFlagClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<FeatureFlagOptions>>().Value;
            client.BaseAddress = new Uri(options.ServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    /// <summary>
    /// Agrega el cliente de Feature Flags con opciones manuales
    /// </summary>
    public static IServiceCollection AddFeatureFlags(
        this IServiceCollection services,
        Action<FeatureFlagOptions> configure)
    {
        services.Configure(configure);

        services.AddMemoryCache();

        services.AddHttpClient<IFeatureFlagClient, FeatureFlagClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<FeatureFlagOptions>>().Value;
            client.BaseAddress = new Uri(options.ServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
