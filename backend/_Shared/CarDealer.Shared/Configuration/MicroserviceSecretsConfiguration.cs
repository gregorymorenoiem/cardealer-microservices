using CarDealer.Shared.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Configuration;

/// <summary>
/// Configuración de secretos genérica para microservicios.
/// Puede ser usada por cualquier servicio con configuración estándar.
/// </summary>
public static class MicroserviceSecretsConfiguration
{
    /// <summary>
    /// Obtiene la connection string de la base de datos desde secretos.
    /// Prioridad: ENV > Docker Secrets > appsettings.json
    /// </summary>
    public static string GetDatabaseConnectionString(
        IConfiguration configuration,
        string serviceName = "")
    {
        var secretProvider = CompositeSecretProvider.CreateDefault();
        var prefix = string.IsNullOrEmpty(serviceName) ? "" : $"{serviceName.ToUpperInvariant()}_";

        // 1. Intentar connection string completa específica del servicio
        var connectionString = secretProvider.GetSecret($"{prefix}DATABASE_CONNECTION_STRING")
            ?? secretProvider.GetSecret(SecretKeys.DatabaseConnectionString);

        if (!string.IsNullOrEmpty(connectionString))
            return connectionString;

        // 2. Intentar construir desde variables individuales
        var host = secretProvider.GetSecret(SecretKeys.DatabaseHost);
        var password = secretProvider.GetSecret(SecretKeys.DatabasePassword);

        if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(password))
        {
            var port = secretProvider.GetSecret(SecretKeys.DatabasePort) ?? "5432";
            var database = secretProvider.GetSecret(SecretKeys.DatabaseName)
                ?? serviceName.ToLowerInvariant().Replace("service", "");
            var user = secretProvider.GetSecret(SecretKeys.DatabaseUser) ?? "postgres";

            return $"Host={host};Port={port};Database={database};Username={user};Password={password};" +
                   "Pooling=true;Minimum Pool Size=5;Maximum Pool Size=20";
        }

        // 3. Fallback a configuración tradicional
        return configuration.GetConnectionString("DefaultConnection")
            ?? configuration["Database:ConnectionStrings:PostgreSQL"]
            ?? throw new InvalidOperationException($"No database connection string configured for {serviceName}");
    }

    /// <summary>
    /// Obtiene la connection string de Redis desde secretos.
    /// </summary>
    public static string? GetRedisConnectionString(IConfiguration configuration)
    {
        var secretProvider = CompositeSecretProvider.CreateDefault();

        var connectionString = secretProvider.GetSecret(SecretKeys.RedisConnectionString);
        if (!string.IsNullOrEmpty(connectionString))
            return connectionString;

        var host = secretProvider.GetSecret(SecretKeys.RedisHost);
        if (!string.IsNullOrEmpty(host))
        {
            var port = secretProvider.GetSecret(SecretKeys.RedisPort) ?? "6379";
            var password = secretProvider.GetSecret(SecretKeys.RedisPassword);

            var redis = $"{host}:{port}";
            if (!string.IsNullOrEmpty(password))
                redis += $",password={password}";
            return redis;
        }

        return configuration.GetConnectionString("Redis")
            ?? configuration["Cache:RedisConnectionString"];
    }

    /// <summary>
    /// Obtiene configuración de RabbitMQ desde secretos.
    /// </summary>
    public static (string Host, int Port, string User, string Password, string VHost) GetRabbitMqConfig(
        IConfiguration configuration)
    {
        var secretProvider = CompositeSecretProvider.CreateDefault();

        var host = secretProvider.GetSecret(SecretKeys.RabbitMqHost)
            ?? configuration["RabbitMQ:HostName"] ?? "localhost";
        var port = int.Parse(secretProvider.GetSecret(SecretKeys.RabbitMqPort)
            ?? configuration["RabbitMQ:Port"] ?? "5672");
        var user = secretProvider.GetSecret(SecretKeys.RabbitMqUser)
            ?? configuration["RabbitMQ:UserName"] ?? throw new InvalidOperationException("RabbitMQ:UserName is not configured");
        var password = secretProvider.GetSecret(SecretKeys.RabbitMqPassword)
            ?? configuration["RabbitMQ:Password"] ?? throw new InvalidOperationException("RabbitMQ:Password is not configured");
        var vhost = secretProvider.GetSecret(SecretKeys.RabbitMqVirtualHost)
            ?? configuration["RabbitMQ:VirtualHost"] ?? "/";

        return (host, port, user, password, vhost);
    }

    /// <summary>
    /// Obtiene configuración JWT desde secretos.
    /// </summary>
    public static (string Key, string Issuer, string Audience) GetJwtConfig(
        IConfiguration configuration)
    {
        var secretProvider = CompositeSecretProvider.CreateDefault();

        var key = secretProvider.GetSecret(SecretKeys.JwtSecretKey)
            ?? configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key not configured");

        var issuer = secretProvider.GetSecret(SecretKeys.JwtIssuer)
            ?? configuration["Jwt:Issuer"]
            ?? "CarDealer";

        var audience = secretProvider.GetSecret(SecretKeys.JwtAudience)
            ?? configuration["Jwt:Audience"]
            ?? "CarDealerServices";

        return (key, issuer, audience);
    }

    /// <summary>
    /// Obtiene URL de Elasticsearch desde secretos.
    /// </summary>
    public static string? GetElasticsearchUrl(IConfiguration configuration)
    {
        var secretProvider = CompositeSecretProvider.CreateDefault();

        return secretProvider.GetSecret(SecretKeys.ElasticsearchUrl)
            ?? configuration["ElasticSearch:ConnectionString"]
            ?? configuration["Elasticsearch:Url"];
    }

    /// <summary>
    /// Valida y loguea advertencias para secretos faltantes.
    /// </summary>
    public static void ValidateSecrets(ILogger logger, string serviceName)
    {
        var secretProvider = CompositeSecretProvider.CreateDefault();

        if (!secretProvider.HasSecret(SecretKeys.DatabaseConnectionString) &&
            !secretProvider.HasSecret(SecretKeys.DatabasePassword))
        {
            logger.LogWarning(
                "[{Service}] Database credentials not found in secrets. Using appsettings.json fallback. " +
                "In production, set DATABASE_CONNECTION_STRING environment variable.",
                serviceName);
        }
    }
}
