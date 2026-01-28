using BankReconciliationService.Domain.Interfaces;
using BankReconciliationService.Infrastructure.Services.Banks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankReconciliationService.Infrastructure.Services.Banks;

/// <summary>
/// Extensiones para registrar los servicios bancarios en el contenedor de DI
/// </summary>
public static class BankServicesExtensions
{
    /// <summary>
    /// Registra todos los servicios bancarios en el contenedor
    /// </summary>
    public static IServiceCollection AddBankApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar configuraciones
        services.Configure<BancoPopularApiSettings>(
            configuration.GetSection(BancoPopularApiSettings.SectionName));
        
        services.Configure<BanreservasApiSettings>(
            configuration.GetSection(BanreservasApiSettings.SectionName));
        
        services.Configure<BHDLeonApiSettings>(
            configuration.GetSection(BHDLeonApiSettings.SectionName));
        
        services.Configure<ScotiabankApiSettings>(
            configuration.GetSection(ScotiabankApiSettings.SectionName));

        // Registrar HttpClient para cada banco
        services.AddHttpClient<BancoPopularApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<BanreservasApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<BHDLeonApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<ScotiabankApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .ConfigurePrimaryHttpMessageHandler(sp =>
        {
            var handler = new HttpClientHandler();
            
            // Configurar certificado cliente para Scotiabank si está disponible
            var config = configuration.GetSection(ScotiabankApiSettings.SectionName).Get<ScotiabankApiSettings>();
            if (config != null && !string.IsNullOrEmpty(config.CertificatePath))
            {
                // En producción, cargar certificado aquí
                // handler.ClientCertificates.Add(new X509Certificate2(config.CertificatePath, config.CertificatePassword));
            }
            
            return handler;
        });

        // Registrar servicios
        services.AddScoped<BancoPopularApiService>();
        services.AddScoped<BanreservasApiService>();
        services.AddScoped<BHDLeonApiService>();
        services.AddScoped<ScotiabankApiService>();

        // Registrar factory
        services.AddScoped<IBankApiServiceFactory, BankApiServiceFactory>();

        return services;
    }
}
