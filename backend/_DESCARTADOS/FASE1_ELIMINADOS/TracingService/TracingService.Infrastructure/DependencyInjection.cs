using Microsoft.Extensions.DependencyInjection;
using TracingService.Application.Interfaces;
using TracingService.Infrastructure.Services;

namespace TracingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string jaegerQueryUrl)
    {
        // Register HttpClient for Jaeger
        services.AddHttpClient<ITraceQueryService, JaegerTraceQueryService>(client =>
        {
            client.BaseAddress = new Uri(jaegerQueryUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register JaegerTraceQueryService
        services.AddScoped<ITraceQueryService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(JaegerTraceQueryService));
            return new JaegerTraceQueryService(httpClient, jaegerQueryUrl);
        });

        return services;
    }
}
