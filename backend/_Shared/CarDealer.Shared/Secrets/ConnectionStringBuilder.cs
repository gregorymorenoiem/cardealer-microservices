using System.Text;

namespace CarDealer.Shared.Secrets;

/// <summary>
/// Utilidades para construir connection strings desde secretos individuales.
/// Útil cuando los secretos se pasan como variables separadas.
/// </summary>
public static class ConnectionStringBuilder
{
    /// <summary>
    /// Construye un connection string de PostgreSQL desde secretos.
    /// </summary>
    public static string BuildPostgreSqlConnectionString(ISecretProvider secrets, string prefix = "")
    {
        var keyPrefix = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}_";

        // Primero intentar obtener connection string completo
        var fullConnectionString = secrets.GetSecret($"{keyPrefix}DATABASE_CONNECTION_STRING")
                                   ?? secrets.GetSecret("DATABASE_CONNECTION_STRING");

        if (!string.IsNullOrEmpty(fullConnectionString))
        {
            return fullConnectionString;
        }

        // Si no, construir desde partes
        var host = secrets.GetSecretOrDefault($"{keyPrefix}DATABASE_HOST", "localhost");
        var port = secrets.GetSecretOrDefault($"{keyPrefix}DATABASE_PORT", "5432");
        var database = secrets.GetSecret($"{keyPrefix}DATABASE_NAME")
                       ?? throw new SecretNotFoundException($"{keyPrefix}DATABASE_NAME");
        var username = secrets.GetSecretOrDefault($"{keyPrefix}DATABASE_USER", "postgres");
        var password = secrets.GetSecret($"{keyPrefix}DATABASE_PASSWORD")
                       ?? throw new SecretNotFoundException($"{keyPrefix}DATABASE_PASSWORD");

        var pooling = secrets.GetSecretOrDefault($"{keyPrefix}DATABASE_POOLING", "true");
        var minPoolSize = secrets.GetSecretOrDefault($"{keyPrefix}DATABASE_MIN_POOL_SIZE", "5");
        var maxPoolSize = secrets.GetSecretOrDefault($"{keyPrefix}DATABASE_MAX_POOL_SIZE", "20");

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};" +
               $"Pooling={pooling};Minimum Pool Size={minPoolSize};Maximum Pool Size={maxPoolSize}";
    }

    /// <summary>
    /// Construye un connection string de SQL Server desde secretos.
    /// </summary>
    public static string BuildSqlServerConnectionString(ISecretProvider secrets, string prefix = "")
    {
        var keyPrefix = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}_";

        var fullConnectionString = secrets.GetSecret($"{keyPrefix}DATABASE_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(fullConnectionString))
        {
            return fullConnectionString;
        }

        var host = secrets.GetSecretOrDefault($"{keyPrefix}DATABASE_HOST", "localhost");
        var port = secrets.GetSecretOrDefault($"{keyPrefix}DATABASE_PORT", "1433");
        var database = secrets.GetSecret($"{keyPrefix}DATABASE_NAME")
                       ?? throw new SecretNotFoundException($"{keyPrefix}DATABASE_NAME");
        var username = secrets.GetSecretOrDefault($"{keyPrefix}DATABASE_USER", "sa");
        var password = secrets.GetSecret($"{keyPrefix}DATABASE_PASSWORD")
                       ?? throw new SecretNotFoundException($"{keyPrefix}DATABASE_PASSWORD");

        return $"Server={host},{port};Database={database};User Id={username};Password={password};" +
               $"TrustServerCertificate=True;MultipleActiveResultSets=true";
    }

    /// <summary>
    /// Construye un connection string de Redis desde secretos.
    /// </summary>
    public static string BuildRedisConnectionString(ISecretProvider secrets, string prefix = "")
    {
        var keyPrefix = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}_";

        var fullConnectionString = secrets.GetSecret($"{keyPrefix}REDIS_CONNECTION_STRING")
                                   ?? secrets.GetSecret("REDIS_CONNECTION_STRING");

        if (!string.IsNullOrEmpty(fullConnectionString))
        {
            return fullConnectionString;
        }

        var host = secrets.GetSecretOrDefault($"{keyPrefix}REDIS_HOST", "localhost");
        var port = secrets.GetSecretOrDefault($"{keyPrefix}REDIS_PORT", "6379");
        var password = secrets.GetSecret($"{keyPrefix}REDIS_PASSWORD");

        var sb = new StringBuilder($"{host}:{port}");

        if (!string.IsNullOrEmpty(password))
        {
            sb.Append($",password={password}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Construye un connection string de RabbitMQ desde secretos.
    /// </summary>
    public static string BuildRabbitMqConnectionString(ISecretProvider secrets, string prefix = "")
    {
        var keyPrefix = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}_";

        var fullConnectionString = secrets.GetSecret($"{keyPrefix}RABBITMQ_CONNECTION_STRING")
                                   ?? secrets.GetSecret("RABBITMQ_CONNECTION_STRING");

        if (!string.IsNullOrEmpty(fullConnectionString))
        {
            return fullConnectionString;
        }

        var host = secrets.GetSecretOrDefault($"{keyPrefix}RABBITMQ_HOST", "localhost");
        var port = secrets.GetSecretOrDefault($"{keyPrefix}RABBITMQ_PORT", "5672");
        var username = secrets.GetSecretOrDefault($"{keyPrefix}RABBITMQ_USER", "guest");
        var password = secrets.GetSecretOrDefault($"{keyPrefix}RABBITMQ_PASSWORD", "guest");
        var vhost = secrets.GetSecretOrDefault($"{keyPrefix}RABBITMQ_VIRTUAL_HOST", "/");

        return $"amqp://{username}:{password}@{host}:{port}/{Uri.EscapeDataString(vhost)}";
    }

    /// <summary>
    /// Construye un connection string de Elasticsearch desde secretos.
    /// </summary>
    public static string BuildElasticsearchConnectionString(ISecretProvider secrets, string prefix = "")
    {
        var keyPrefix = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}_";

        var url = secrets.GetSecret($"{keyPrefix}ELASTICSEARCH_URL")
                  ?? secrets.GetSecretOrDefault("ELASTICSEARCH_URL", "http://localhost:9200");

        var username = secrets.GetSecret($"{keyPrefix}ELASTICSEARCH_USERNAME");
        var password = secrets.GetSecret($"{keyPrefix}ELASTICSEARCH_PASSWORD");

        // Si hay credenciales, agregarlas a la URL
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            var uri = new UriBuilder(url)
            {
                UserName = Uri.EscapeDataString(username),
                Password = Uri.EscapeDataString(password)
            };
            return uri.Uri.ToString();
        }

        return url;
    }

    /// <summary>
    /// Construye la configuración JWT desde secretos.
    /// </summary>
    public static JwtConfiguration BuildJwtConfiguration(ISecretProvider secrets)
    {
        return new JwtConfiguration
        {
            SecretKey = secrets.GetRequiredSecret(SecretKeys.JwtSecretKey),
            Issuer = secrets.GetSecretOrDefault(SecretKeys.JwtIssuer, "CarDealer"),
            Audience = secrets.GetSecretOrDefault(SecretKeys.JwtAudience, "CarDealerClients"),
            ExpiresMinutes = int.TryParse(
                secrets.GetSecretOrDefault(SecretKeys.JwtExpiresMinutes, "60"),
                out var minutes) ? minutes : 60,
            RefreshTokenExpiresDays = int.TryParse(
                secrets.GetSecretOrDefault(SecretKeys.JwtRefreshTokenExpiresDays, "7"),
                out var days) ? days : 7
        };
    }
}

/// <summary>
/// Configuración de JWT construida desde secretos.
/// </summary>
public class JwtConfiguration
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "CarDealer";
    public string Audience { get; set; } = "CarDealerClients";
    public int ExpiresMinutes { get; set; } = 60;
    public int RefreshTokenExpiresDays { get; set; } = 7;
}
