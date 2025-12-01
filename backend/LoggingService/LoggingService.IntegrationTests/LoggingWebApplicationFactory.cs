using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using LoggingService.Application.Interfaces;
using LoggingService.Infrastructure.Services;

namespace LoggingService.IntegrationTests;

public class LoggingWebApplicationFactory : WebApplicationFactory<Program>
{
    public string SeqUrl { get; set; } = "http://localhost:5341";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seq:Url"] = SeqUrl
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remover el agregador existente si es necesario
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ILogAggregator));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Registrar el agregador con la URL configurada
            services.AddSingleton<ILogAggregator>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new SeqLogAggregator(httpClientFactory, SeqUrl);
            });
        });
    }
}
