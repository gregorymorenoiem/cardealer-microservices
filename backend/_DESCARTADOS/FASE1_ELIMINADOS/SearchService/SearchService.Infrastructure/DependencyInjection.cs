using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using SearchService.Domain.Interfaces;
using SearchService.Infrastructure.Configuration;
using SearchService.Infrastructure.Repositories;
using SearchService.Infrastructure.Services;

namespace SearchService.Infrastructure;

/// <summary>
/// Configuración de servicios de la capa de infraestructura
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Cargar configuración de Elasticsearch
        var elasticsearchOptions = new ElasticsearchOptions();
        configuration.GetSection(ElasticsearchOptions.SectionName).Bind(elasticsearchOptions);
        services.AddSingleton(elasticsearchOptions);

        // Configurar cliente de Elasticsearch
        var settings = new ConnectionSettings(new Uri(elasticsearchOptions.Url))
            .RequestTimeout(TimeSpan.FromSeconds(elasticsearchOptions.TimeoutSeconds))
            .MaximumRetries(elasticsearchOptions.MaxRetries)
            .EnableDebugMode(elasticsearchOptions.EnableDebugMode ? Console.WriteLine : null)
            .PrettyJson();

        // Autenticación si está configurada
        if (!string.IsNullOrWhiteSpace(elasticsearchOptions.Username) &&
            !string.IsNullOrWhiteSpace(elasticsearchOptions.Password))
        {
            settings.BasicAuthentication(elasticsearchOptions.Username, elasticsearchOptions.Password);
        }

        var client = new ElasticClient(settings);
        services.AddSingleton<IElasticClient>(client);

        // Registrar repositorios y servicios
        services.AddScoped<ISearchRepository, ElasticsearchRepository>();
        services.AddScoped<IIndexManager, ElasticsearchIndexManager>();

        return services;
    }
}
