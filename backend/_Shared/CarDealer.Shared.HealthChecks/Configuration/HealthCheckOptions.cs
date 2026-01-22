namespace CarDealer.Shared.HealthChecks.Configuration;

/// <summary>
/// Opciones de configuración para Health Checks
/// </summary>
public class StandardHealthCheckOptions
{
    public const string SectionName = "HealthChecks";
    
    /// <summary>
    /// Habilitar Health Checks
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Configuración de PostgreSQL
    /// </summary>
    public DatabaseHealthCheckOptions PostgreSQL { get; set; } = new();
    
    /// <summary>
    /// Configuración de Redis
    /// </summary>
    public RedisHealthCheckOptions Redis { get; set; } = new();
    
    /// <summary>
    /// Configuración de RabbitMQ
    /// </summary>
    public RabbitMQHealthCheckOptions RabbitMQ { get; set; } = new();
    
    /// <summary>
    /// Configuración de servicios externos (URLs)
    /// </summary>
    public ExternalServicesHealthCheckOptions ExternalServices { get; set; } = new();
    
    /// <summary>
    /// Endpoints para exponer health checks
    /// </summary>
    public HealthCheckEndpointsOptions Endpoints { get; set; } = new();
}

public class DatabaseHealthCheckOptions
{
    public bool Enabled { get; set; } = true;
    public string? ConnectionString { get; set; }
    public string Name { get; set; } = "postgresql";
    public string[] Tags { get; set; } = ["ready"];
    public int TimeoutSeconds { get; set; } = 10;
}

public class RedisHealthCheckOptions
{
    public bool Enabled { get; set; } = false;
    public string ConnectionString { get; set; } = "localhost:6379";
    public string Name { get; set; } = "redis";
    public string[] Tags { get; set; } = ["ready", "cache"];
    public int TimeoutSeconds { get; set; } = 5;
}

public class RabbitMQHealthCheckOptions
{
    public bool Enabled { get; set; } = false;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string Name { get; set; } = "rabbitmq";
    public string[] Tags { get; set; } = ["ready", "messaging"];
    public int TimeoutSeconds { get; set; } = 5;
}

public class ExternalServicesHealthCheckOptions
{
    public bool Enabled { get; set; } = false;
    public Dictionary<string, ExternalServiceConfig> Services { get; set; } = new();
}

public class ExternalServiceConfig
{
    public string Url { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string[] Tags { get; set; } = ["ready"];
    public int TimeoutSeconds { get; set; } = 10;
}

public class HealthCheckEndpointsOptions
{
    /// <summary>
    /// Ruta para liveness check (está vivo el servicio?)
    /// </summary>
    public string LivePath { get; set; } = "/health/live";
    
    /// <summary>
    /// Ruta para readiness check (puede recibir tráfico?)
    /// </summary>
    public string ReadyPath { get; set; } = "/health/ready";
    
    /// <summary>
    /// Ruta para health check general
    /// </summary>
    public string HealthPath { get; set; } = "/health";
    
    /// <summary>
    /// Incluir detalles en respuesta (solo para /health)
    /// </summary>
    public bool IncludeDetails { get; set; } = true;
}
