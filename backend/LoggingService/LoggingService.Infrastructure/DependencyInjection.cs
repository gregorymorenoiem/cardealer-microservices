using LoggingService.Application.Interfaces;
using LoggingService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LoggingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var seqUrl = configuration["Seq:Url"] ?? "http://localhost:5341";

        services.AddHttpClient();

        // Log aggregation
        services.AddSingleton<ILogAggregator>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new SeqLogAggregator(httpClientFactory, seqUrl);
        });

        // Log analysis
        services.AddScoped<ILogAnalyzer, LogAnalyzer>();

        // Alerting
        services.AddSingleton<IAlertingService, InMemoryAlertingService>();

        return services;
    }
}
