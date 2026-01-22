using CarDealer.Shared.ApiVersioning.Configuration;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CarDealer.Shared.ApiVersioning.Extensions;

/// <summary>
/// Extensiones para agregar API versioning a los servicios
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Agrega API versioning con configuración desde appsettings
    /// </summary>
    public static IServiceCollection AddStandardApiVersioning(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = new Configuration.ApiVersioningOptions();
        configuration.GetSection(Configuration.ApiVersioningOptions.SectionName).Bind(options);
        
        return services.AddStandardApiVersioning(options);
    }

    /// <summary>
    /// Agrega API versioning con opciones manuales
    /// </summary>
    public static IServiceCollection AddStandardApiVersioning(
        this IServiceCollection services,
        Configuration.ApiVersioningOptions options)
    {
        if (!options.Enabled)
            return services;

        // Parsear versión por defecto
        var versionParts = options.DefaultVersion.Split('.');
        var major = int.Parse(versionParts[0]);
        var minor = versionParts.Length > 1 ? int.Parse(versionParts[1]) : 0;

        services.AddApiVersioning(apiVersioningOptions =>
        {
            apiVersioningOptions.DefaultApiVersion = new ApiVersion(major, minor);
            apiVersioningOptions.AssumeDefaultVersionWhenUnspecified = options.AssumeDefaultVersionWhenUnspecified;
            apiVersioningOptions.ReportApiVersions = options.ReportApiVersions;
            
            // Configurar readers
            var readers = new List<IApiVersionReader>();
            
            if (options.VersionReader.ReadFromQueryString)
                readers.Add(new QueryStringApiVersionReader(options.VersionReader.QueryStringParameter));
            
            if (options.VersionReader.ReadFromHeader)
                readers.Add(new HeaderApiVersionReader(options.VersionReader.HeaderName));
            
            if (options.VersionReader.ReadFromUrl)
                readers.Add(new UrlSegmentApiVersionReader());
            
            if (options.VersionReader.ReadFromMediaType)
                readers.Add(new MediaTypeApiVersionReader(options.VersionReader.MediaTypeParameter));

            apiVersioningOptions.ApiVersionReader = ApiVersionReader.Combine(readers.ToArray());
        })
        .AddApiExplorer(explorerOptions =>
        {
            explorerOptions.GroupNameFormat = "'v'VVV";
            explorerOptions.SubstituteApiVersionInUrl = true;
        });

        // Registrar opciones para uso posterior
        services.AddSingleton(options);

        return services;
    }

    /// <summary>
    /// Agrega API versioning con configuración fluent
    /// </summary>
    public static IServiceCollection AddStandardApiVersioning(
        this IServiceCollection services,
        Action<Configuration.ApiVersioningOptions> configure)
    {
        var options = new Configuration.ApiVersioningOptions();
        configure(options);
        return services.AddStandardApiVersioning(options);
    }

    /// <summary>
    /// Configura Swagger para mostrar múltiples versiones
    /// </summary>
    public static IServiceCollection AddVersionedSwagger(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = new Configuration.ApiVersioningOptions();
        configuration.GetSection(Configuration.ApiVersioningOptions.SectionName).Bind(options);
        
        return services.AddVersionedSwagger(options);
    }

    /// <summary>
    /// Configura Swagger con opciones manuales
    /// </summary>
    public static IServiceCollection AddVersionedSwagger(
        this IServiceCollection services,
        Configuration.ApiVersioningOptions options)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(sp =>
        {
            var provider = sp.GetRequiredService<IApiVersionDescriptionProvider>();
            return new ConfigureSwaggerOptions(provider, options);
        });

        services.AddSwaggerGen(c =>
        {
            // Agregar esquema de seguridad JWT
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header. Example: 'Bearer {token}'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Agregar filtro de operación para versión
            c.OperationFilter<SwaggerDefaultValues>();
        });

        return services;
    }
}

/// <summary>
/// Configura opciones de Swagger para cada versión
/// </summary>
internal class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    private readonly Configuration.ApiVersioningOptions _options;

    public ConfigureSwaggerOptions(
        IApiVersionDescriptionProvider provider,
        Configuration.ApiVersioningOptions options)
    {
        _provider = provider;
        _options = options;
    }

    public void Configure(SwaggerGenOptions swaggerOptions)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            swaggerOptions.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }
    }

    private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = _options.Swagger.Title,
            Version = description.ApiVersion.ToString(),
            Description = _options.Swagger.Description
        };

        if (_options.Swagger.Contact != null)
        {
            info.Contact = new OpenApiContact
            {
                Name = _options.Swagger.Contact.Name,
                Email = _options.Swagger.Contact.Email,
                Url = !string.IsNullOrEmpty(_options.Swagger.Contact.Url) 
                    ? new Uri(_options.Swagger.Contact.Url) 
                    : null
            };
        }

        if (_options.Swagger.License != null)
        {
            info.License = new OpenApiLicense
            {
                Name = _options.Swagger.License.Name,
                Url = !string.IsNullOrEmpty(_options.Swagger.License.Url) 
                    ? new Uri(_options.Swagger.License.Url) 
                    : null
            };
        }

        if (description.IsDeprecated)
        {
            info.Description += " (DEPRECATED)";
        }

        return info;
    }
}

/// <summary>
/// Filtro de Swagger para valores por defecto
/// </summary>
internal class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        operation.Deprecated |= apiDescription.IsDeprecated();

        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            var responseKey = responseType.IsDefaultResponse 
                ? "default" 
                : responseType.StatusCode.ToString();
            
            var response = operation.Responses[responseKey];

            foreach (var contentType in response.Content.Keys)
            {
                if (!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
                {
                    response.Content.Remove(contentType);
                }
            }
        }

        if (operation.Parameters == null)
            return;

        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions
                .First(p => p.Name == parameter.Name);

            parameter.Description ??= description.ModelMetadata?.Description;

            if (parameter.Schema.Default == null && description.DefaultValue != null)
            {
                parameter.Schema.Default = new Microsoft.OpenApi.Any.OpenApiString(
                    description.DefaultValue.ToString());
            }

            parameter.Required |= description.IsRequired;
        }
    }
}
