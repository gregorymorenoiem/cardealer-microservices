namespace SearchService.Infrastructure.Configuration;

/// <summary>
/// Opciones de configuración de Elasticsearch
/// </summary>
public class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    /// <summary>
    /// URL del cluster de Elasticsearch
    /// </summary>
    public string Url { get; set; } = "http://localhost:9200";

    /// <summary>
    /// Nombre de usuario para autenticación
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Contraseña para autenticación
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Prefijo para nombres de índices
    /// </summary>
    public string IndexPrefix { get; set; } = "cardealer";

    /// <summary>
    /// Timeout de peticiones en segundos
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Número máximo de intentos de retry
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Habilitar logging de debugging
    /// </summary>
    public bool EnableDebugMode { get; set; } = false;

    /// <summary>
    /// Número de shards por defecto
    /// </summary>
    public int DefaultShards { get; set; } = 1;

    /// <summary>
    /// Número de réplicas por defecto
    /// </summary>
    public int DefaultReplicas { get; set; } = 1;
}
