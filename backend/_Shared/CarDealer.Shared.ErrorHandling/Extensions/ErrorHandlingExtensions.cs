using CarDealer.Shared.ErrorHandling.Interfaces;
using CarDealer.Shared.ErrorHandling.Middleware;
using CarDealer.Shared.ErrorHandling.Models;
using CarDealer.Shared.ErrorHandling.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarDealer.Shared.ErrorHandling.Extensions;

/// <summary>
/// Extension methods for adding error handling services
/// </summary>
public static class ErrorHandlingExtensions
{
    /// <summary>
    /// Adds standardized error handling services to the DI container
    /// </summary>
    public static IServiceCollection AddStandardErrorHandling(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        // Bind configuration
        var options = new ErrorHandlingOptions { ServiceName = serviceName };
        configuration.GetSection(ErrorHandlingOptions.SectionName).Bind(options);
        
        services.Configure<ErrorHandlingOptions>(opt =>
        {
            opt.ServiceName = serviceName;
            configuration.GetSection(ErrorHandlingOptions.SectionName).Bind(opt);
        });

        // Register error publisher
        services.AddSingleton<IErrorPublisher, RabbitMQErrorPublisher>();

        return services;
    }

    /// <summary>
    /// Adds error handling with custom configuration
    /// </summary>
    public static IServiceCollection AddStandardErrorHandling(
        this IServiceCollection services,
        Action<ErrorHandlingOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IErrorPublisher, RabbitMQErrorPublisher>();
        
        return services;
    }

    /// <summary>
    /// Uses the global exception handling middleware
    /// </summary>
    public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
