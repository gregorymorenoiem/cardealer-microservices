using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;

namespace CarDealer.Shared.ApiVersioning.Extensions;

/// <summary>
/// Extensiones para configurar Swagger en el pipeline
/// </summary>
public static class SwaggerEndpointExtensions
{
    /// <summary>
    /// Configura Swagger UI con soporte para múltiples versiones
    /// </summary>
    public static IApplicationBuilder UseVersionedSwagger(
        this IApplicationBuilder app,
        IApiVersionDescriptionProvider provider)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                var url = $"/swagger/{description.GroupName}/swagger.json";
                var name = $"API {description.GroupName.ToUpperInvariant()}";
                
                if (description.IsDeprecated)
                {
                    name += " (Deprecated)";
                }
                
                options.SwaggerEndpoint(url, name);
            }

            // Configuración adicional del UI
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "CarDealer API Documentation";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
        });

        return app;
    }

    /// <summary>
    /// Configura Swagger UI con título personalizado
    /// </summary>
    public static IApplicationBuilder UseVersionedSwagger(
        this IApplicationBuilder app,
        IApiVersionDescriptionProvider provider,
        string documentTitle)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                var url = $"/swagger/{description.GroupName}/swagger.json";
                var name = $"API {description.GroupName.ToUpperInvariant()}";
                
                if (description.IsDeprecated)
                {
                    name += " (Deprecated)";
                }
                
                options.SwaggerEndpoint(url, name);
            }

            options.RoutePrefix = "swagger";
            options.DocumentTitle = documentTitle;
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
        });

        return app;
    }
}
