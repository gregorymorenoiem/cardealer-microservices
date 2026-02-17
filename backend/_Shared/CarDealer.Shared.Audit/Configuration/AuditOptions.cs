namespace CarDealer.Shared.Audit.Configuration;

/// <summary>
/// Opciones de configuración para el cliente de Audit
/// </summary>
public class AuditOptions
{
    public const string SectionName = "Audit";

    /// <summary>
    /// Nombre del servicio que publica los eventos
    /// </summary>
    public string ServiceName { get; set; } = "UnknownService";

    /// <summary>
    /// Habilitar publicación de eventos de auditoría
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Configuración de RabbitMQ
    /// </summary>
    public RabbitMqConfig RabbitMq { get; set; } = new();

    /// <summary>
    /// Nombre del exchange de auditoría
    /// </summary>
    public string ExchangeName { get; set; } = "audit.events";

    /// <summary>
    /// Routing key para eventos de auditoría
    /// </summary>
    public string RoutingKey { get; set; } = "audit.event";

    /// <summary>
    /// Eventos a auditar automáticamente
    /// </summary>
    public AutoAuditConfig AutoAudit { get; set; } = new();
}

public class RabbitMqConfig
{
    public string Host { get; set; } = "rabbitmq";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
}

public class AutoAuditConfig
{
    /// <summary>
    /// Auditar automáticamente requests HTTP
    /// </summary>
    public bool HttpRequests { get; set; } = false;

    /// <summary>
    /// Auditar solo requests que modifican datos (POST, PUT, DELETE)
    /// </summary>
    public bool OnlyMutations { get; set; } = true;

    /// <summary>
    /// Rutas a excluir de la auditoría automática
    /// </summary>
    public List<string> ExcludePaths { get; set; } = new()
    {
        "/health",
        "/metrics",
        "/swagger",
        "/api/health"
    };

    /// <summary>
    /// Eventos de dominio a auditar
    /// </summary>
    public List<string> AuditableEvents { get; set; } = new()
    {
        "Login",
        "Logout",
        "PasswordChange",
        "PaymentCreated",
        "PaymentFailed",
        "RefundIssued",
        "VehicleCreated",
        "VehicleUpdated",
        "VehicleDeleted",
        "UserCreated",
        "UserUpdated",
        "RoleChanged"
    };
}
