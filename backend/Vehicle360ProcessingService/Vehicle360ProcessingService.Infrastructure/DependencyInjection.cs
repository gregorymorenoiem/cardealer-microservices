using System.Net;
using System.Net.Http.Headers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;
using Vehicle360ProcessingService.Domain.Interfaces;
using Vehicle360ProcessingService.Infrastructure.HttpClients;
using Vehicle360ProcessingService.Infrastructure.Persistence;

namespace Vehicle360ProcessingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<Vehicle360ProcessingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repository
        services.AddScoped<IVehicle360JobRepository, Vehicle360JobRepository>();

        // Configure HTTP client options
        services.Configure<MediaServiceOptions>(
            configuration.GetSection("Services:MediaService"));
        services.Configure<Video360ServiceOptions>(
            configuration.GetSection("Services:Video360Service"));
        services.Configure<BackgroundRemovalServiceOptions>(
            configuration.GetSection("Services:BackgroundRemovalService"));

        // MediaService client with resilience
        var mediaServiceUrl = configuration["Services:MediaService:BaseUrl"] ?? "http://mediaservice:8080";
        services.AddHttpClient<IMediaServiceClient, MediaServiceHttpClient>(client =>
        {
            client.BaseAddress = new Uri(mediaServiceUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromMinutes(5);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        })
        .AddPolicyHandler((sp, request) => GetRetryPolicy(sp, "MediaService"))
        .AddPolicyHandler((sp, request) => GetCircuitBreakerPolicy(sp, "MediaService"))
        .AddPolicyHandler(GetTimeoutPolicy(120));

        // Video360Service client with resilience
        var video360ServiceUrl = configuration["Services:Video360Service:BaseUrl"] ?? "http://video360service:8080";
        services.AddHttpClient<IVideo360ServiceClient, Video360ServiceHttpClient>(client =>
        {
            client.BaseAddress = new Uri(video360ServiceUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromMinutes(15);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        })
        .AddPolicyHandler((sp, request) => GetRetryPolicy(sp, "Video360Service"))
        .AddPolicyHandler((sp, request) => GetCircuitBreakerPolicy(sp, "Video360Service"))
        .AddPolicyHandler(GetTimeoutPolicy(300));

        // BackgroundRemovalService client with resilience
        var bgRemovalServiceUrl = configuration["Services:BackgroundRemovalService:BaseUrl"] ?? "http://backgroundremovalservice:8080";
        services.AddHttpClient<IBackgroundRemovalClient, BackgroundRemovalHttpClient>(client =>
        {
            client.BaseAddress = new Uri(bgRemovalServiceUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromMinutes(5);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        })
        .AddPolicyHandler((sp, request) => GetRetryPolicy(sp, "BackgroundRemovalService"))
        .AddPolicyHandler((sp, request) => GetCircuitBreakerPolicy(sp, "BackgroundRemovalService"))
        .AddPolicyHandler(GetTimeoutPolicy(180));

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider sp, string serviceName)
    {
        var logger = sp.GetService<ILogger<Vehicle360ProcessingDbContext>>();
        
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    logger?.LogWarning(
                        "Retry {RetryAttempt} for {ServiceName} after {Delay}s. Reason: {Reason}",
                        retryAttempt,
                        serviceName,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(IServiceProvider sp, string serviceName)
    {
        var logger = sp.GetService<ILogger<Vehicle360ProcessingDbContext>>();
        
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    logger?.LogError(
                        "Circuit OPEN for {ServiceName}. Duration: {Duration}s. Reason: {Reason}",
                        serviceName,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                },
                onReset: () =>
                {
                    logger?.LogInformation("Circuit CLOSED for {ServiceName}. Resuming requests.", serviceName);
                },
                onHalfOpen: () =>
                {
                    logger?.LogInformation("Circuit HALF-OPEN for {ServiceName}. Testing service...", serviceName);
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int timeoutSeconds)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(timeoutSeconds),
            TimeoutStrategy.Optimistic);
    }
}
