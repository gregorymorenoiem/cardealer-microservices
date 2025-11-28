using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Shared;

/// <summary>
/// Extension methods for configuring services
/// </summary>
public static class ServiceConfigurations
{
    /// <summary>
    /// Registers all configuration settings from appsettings.json
    /// </summary>
    public static IServiceCollection AddSharedConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Register JWT settings
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        // Register email settings
        services.Configure<EmailSettings>(configuration.GetSection("Email"));

        // Register error service settings
        services.Configure<ErrorServiceSettings>(configuration.GetSection("ErrorService"));

        // Register cache settings
        services.Configure<CacheSettings>(configuration.GetSection("Cache"));

        // Register database settings
        services.Configure<DatabaseSettings>(configuration.GetSection("Database"));

        // Register security settings
        services.Configure<SecuritySettings>(configuration.GetSection("Security"));

        return services;
    }

    /// <summary>
    /// Validates that all required configuration settings are present
    /// </summary>
    public static void ValidateConfiguration(this IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        if (string.IsNullOrEmpty(jwtSettings?.Key) || jwtSettings.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT Key is missing or too short. It must be at least 32 characters long.");
        }

        var dbSettings = configuration.GetSection("Database").Get<DatabaseSettings>();
        if (string.IsNullOrEmpty(dbSettings?.ConnectionString))
        {
            throw new InvalidOperationException("Database connection string is required.");
        }
    }
}
