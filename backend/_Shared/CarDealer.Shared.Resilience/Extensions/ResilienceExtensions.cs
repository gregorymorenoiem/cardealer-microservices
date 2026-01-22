using CarDealer.Shared.Resilience.Configuration;
using CarDealer.Shared.Resilience.Policies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Polly;

namespace CarDealer.Shared.Resilience.Extensions;

/// <summary>
/// Extensiones para agregar resiliencia a HttpClient
/// </summary>
public static class ResilienceExtensions
{
    /// <summary>
    /// Agrega un HttpClient con políticas de resiliencia estándar
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? clientName = null,
        Action<HttpClient>? configureClient = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        var options = new ResilienceOptions();
        configuration.GetSection(ResilienceOptions.SectionName).Bind(options);
        
        return services.AddResilientHttpClient<TClient, TImplementation>(
            clientName ?? typeof(TClient).Name,
            options,
            configureClient);
    }

    /// <summary>
    /// Agrega un HttpClient con políticas de resiliencia con opciones manuales
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        string clientName,
        ResilienceOptions options,
        Action<HttpClient>? configureClient = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        var builder = services.AddHttpClient<TClient, TImplementation>(clientName, client =>
        {
            configureClient?.Invoke(client);
        });

        if (options.Enabled)
        {
            builder.AddResilienceHandler($"{clientName}-resilience", (resilienceBuilder, context) =>
            {
                var logger = context.ServiceProvider.GetService<ILogger<TClient>>();
                
                // Timeout total (wrapping)
                resilienceBuilder.AddTimeout(new Polly.Timeout.TimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(options.Timeout.TotalTimeoutSeconds)
                });

                // Retry con backoff exponencial
                resilienceBuilder.AddRetry(new Polly.Retry.RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = options.Retry.MaxRetries,
                    Delay = TimeSpan.FromSeconds(options.Retry.DelaySeconds),
                    BackoffType = options.Retry.UseExponentialBackoff 
                        ? DelayBackoffType.Exponential 
                        : DelayBackoffType.Constant,
                    UseJitter = options.Retry.UseJitter,
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<Polly.Timeout.TimeoutRejectedException>()
                        .HandleResult(response => options.Retry.RetryStatusCodes.Contains((int)response.StatusCode)),
                    OnRetry = args =>
                    {
                        logger?.LogWarning(
                            "Retry {Attempt}/{Max} for {Client} after {Delay}ms",
                            args.AttemptNumber,
                            options.Retry.MaxRetries,
                            clientName,
                            args.RetryDelay.TotalMilliseconds);
                        return ValueTask.CompletedTask;
                    }
                });

                // Circuit breaker
                resilienceBuilder.AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions<HttpResponseMessage>
                {
                    FailureRatio = options.CircuitBreaker.FailureRatio,
                    MinimumThroughput = options.CircuitBreaker.MinimumThroughput,
                    SamplingDuration = TimeSpan.FromSeconds(options.CircuitBreaker.SamplingDurationSeconds),
                    BreakDuration = TimeSpan.FromSeconds(options.CircuitBreaker.BreakDurationSeconds),
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(response => !response.IsSuccessStatusCode),
                    OnOpened = args =>
                    {
                        logger?.LogWarning(
                            "Circuit OPENED for {Client}. Break duration: {Duration}s",
                            clientName,
                            options.CircuitBreaker.BreakDurationSeconds);
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = args =>
                    {
                        logger?.LogInformation("Circuit CLOSED for {Client}", clientName);
                        return ValueTask.CompletedTask;
                    }
                });

                // Timeout por request
                resilienceBuilder.AddTimeout(new Polly.Timeout.TimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(options.Timeout.TimeoutSeconds)
                });
            });
        }

        return builder;
    }

    /// <summary>
    /// Agrega resiliencia a un HttpClientBuilder existente
    /// </summary>
    public static IHttpClientBuilder AddStandardResilience(
        this IHttpClientBuilder builder,
        IConfiguration configuration)
    {
        var options = new ResilienceOptions();
        configuration.GetSection(ResilienceOptions.SectionName).Bind(options);
        
        return builder.AddStandardResilience(options);
    }

    /// <summary>
    /// Agrega resiliencia con opciones manuales
    /// </summary>
    public static IHttpClientBuilder AddStandardResilience(
        this IHttpClientBuilder builder,
        ResilienceOptions options)
    {
        if (!options.Enabled)
            return builder;

        builder.AddResilienceHandler("standard-resilience", resilienceBuilder =>
        {
            // Timeout total
            resilienceBuilder.AddTimeout(new Polly.Timeout.TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(options.Timeout.TotalTimeoutSeconds)
            });

            // Retry
            resilienceBuilder.AddRetry(new Polly.Retry.RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = options.Retry.MaxRetries,
                Delay = TimeSpan.FromSeconds(options.Retry.DelaySeconds),
                BackoffType = options.Retry.UseExponentialBackoff 
                    ? DelayBackoffType.Exponential 
                    : DelayBackoffType.Constant,
                UseJitter = options.Retry.UseJitter,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(response => options.Retry.RetryStatusCodes.Contains((int)response.StatusCode))
            });

            // Circuit Breaker
            resilienceBuilder.AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = options.CircuitBreaker.FailureRatio,
                MinimumThroughput = options.CircuitBreaker.MinimumThroughput,
                SamplingDuration = TimeSpan.FromSeconds(options.CircuitBreaker.SamplingDurationSeconds),
                BreakDuration = TimeSpan.FromSeconds(options.CircuitBreaker.BreakDurationSeconds)
            });

            // Timeout individual
            resilienceBuilder.AddTimeout(new Polly.Timeout.TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(options.Timeout.TimeoutSeconds)
            });
        });
        
        return builder;
    }

    /// <summary>
    /// Agrega resiliencia con configuración fluent
    /// </summary>
    public static IHttpClientBuilder AddStandardResilience(
        this IHttpClientBuilder builder,
        Action<ResilienceOptions> configure)
    {
        var options = new ResilienceOptions();
        configure(options);
        return builder.AddStandardResilience(options);
    }
}
