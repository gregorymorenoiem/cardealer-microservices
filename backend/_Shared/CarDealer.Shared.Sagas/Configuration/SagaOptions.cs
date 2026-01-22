namespace CarDealer.Shared.Sagas.Configuration;

/// <summary>
/// Opciones de configuración para MassTransit y Sagas
/// </summary>
public class SagaOptions
{
    public const string SectionName = "Sagas";

    /// <summary>
    /// Habilitar el uso de Sagas
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Configuración de RabbitMQ
    /// </summary>
    public RabbitMqOptions RabbitMq { get; set; } = new();

    /// <summary>
    /// Configuración del Saga State Machine Repository
    /// </summary>
    public SagaRepositoryOptions Repository { get; set; } = new();

    /// <summary>
    /// Configuración de reintentos
    /// </summary>
    public SagaRetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Configuración de outbox
    /// </summary>
    public OutboxOptions Outbox { get; set; } = new();
}

/// <summary>
/// Opciones de RabbitMQ
/// </summary>
public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    
    /// <summary>
    /// Prefijo para las colas
    /// </summary>
    public string QueuePrefix { get; set; } = "cardealer";

    /// <summary>
    /// Obtiene la URI de conexión a RabbitMQ
    /// </summary>
    public string ConnectionString => $"amqp://{Username}:{Password}@{Host}:{Port}{VirtualHost}";
}

/// <summary>
/// Opciones del repositorio de Sagas
/// </summary>
public class SagaRepositoryOptions
{
    /// <summary>
    /// Tipo de repositorio: EntityFramework, Redis, InMemory
    /// </summary>
    public SagaRepositoryType Type { get; set; } = SagaRepositoryType.EntityFramework;

    /// <summary>
    /// Connection string para Redis (si Type = Redis)
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Prefijo de keys en Redis
    /// </summary>
    public string RedisKeyPrefix { get; set; } = "saga:";

    /// <summary>
    /// Tiempo de expiración de estados en Redis (minutos)
    /// </summary>
    public int RedisExpirationMinutes { get; set; } = 60 * 24; // 24 horas
}

public enum SagaRepositoryType
{
    InMemory,
    EntityFramework,
    Redis
}

/// <summary>
/// Opciones de reintentos
/// </summary>
public class SagaRetryOptions
{
    /// <summary>
    /// Número máximo de reintentos
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Intervalo base entre reintentos (segundos)
    /// </summary>
    public int IntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Usar backoff exponencial
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
}

/// <summary>
/// Opciones del Outbox Pattern
/// </summary>
public class OutboxOptions
{
    /// <summary>
    /// Habilitar outbox para entrega garantizada
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Intervalo de limpieza del outbox (segundos)
    /// </summary>
    public int CleanupIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Tiempo de vida de mensajes entregados (horas)
    /// </summary>
    public int DeliveredMessageTtlHours { get; set; } = 24;

    /// <summary>
    /// Número de mensajes a procesar por batch
    /// </summary>
    public int BatchSize { get; set; } = 100;
}
