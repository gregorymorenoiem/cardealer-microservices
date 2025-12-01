using LoggingService.Application.Interfaces;
using LoggingService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoggingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var seqUrl = configuration["Seq:Url"] ?? "http://localhost:5341";

        services.AddHttpClient();

        services.AddSingleton<ILogAggregator>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new SeqLogAggregator(httpClientFactory, seqUrl);
        });

        return services;
    }
}
