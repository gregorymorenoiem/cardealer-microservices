using CarDealer.Shared.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarDealer.Shared.Extensions
{
    /// <summary>
    /// Extension methods para configurar el sistema de acceso a módulos en microservicios
    /// </summary>
    public static class ModuleAccessExtensions
    {
        /// <summary>
        /// Registra los servicios necesarios para el sistema de módulos
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="userServiceUrl">URL base del UserService (opcional, default from config)</param>
        public static IServiceCollection AddModuleAccessServices(
            this IServiceCollection services,
            IConfiguration configuration,
            string? userServiceUrl = null)
        {
            // Obtener URL del UserService
            var baseUrl = userServiceUrl
                ?? configuration["ServiceUrls:UserService"]
                ?? "http://localhost:5006";

            // Registrar HttpClient para UserService
            services.AddHttpClient("UserService", client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // Registrar Redis como cache distribuido
            var redisConnection = configuration.GetConnectionString("Redis")
                ?? configuration["Redis:ConnectionString"];

            if (!string.IsNullOrEmpty(redisConnection))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "CarDealer:";
                });
            }
            else
            {
                // Fallback a cache en memoria para desarrollo
                services.AddDistributedMemoryCache();
            }

            // Registrar el servicio de acceso a módulos
            services.AddScoped<IModuleAccessService, ModuleAccessService>();

            return services;
        }

        /// <summary>
        /// Configura el middleware de verificación de módulos para un servicio completo
        /// Usar después de UseAuthentication() y UseAuthorization()
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="moduleCode">Código del módulo requerido</param>
        public static IApplicationBuilder UseModuleAccessVerification(
            this IApplicationBuilder app,
            string moduleCode)
        {
            return app.UseMiddleware<CarDealer.Shared.Middleware.ModuleAccessMiddleware>(moduleCode);
        }
    }
}
