namespace PaymentService.Application.DTOs;

/// <summary>
/// Estado de salud de un proveedor de pagos
/// </summary>
public class ProviderHealthDto
{
    /// <summary>
    /// Identificador del gateway
    /// </summary>
    public string Gateway { get; set; } = string.Empty;

    /// <summary>
    /// Nombre comercial del proveedor
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el proveedor está disponible
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Indica si el proveedor está correctamente configurado
    /// </summary>
    public bool IsConfigured { get; set; }

    /// <summary>
    /// Estado general del proveedor
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje de estado o error
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Latencia en milisegundos del último health check
    /// </summary>
    public long? LatencyMs { get; set; }

    /// <summary>
    /// Fecha/hora de la última verificación
    /// </summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Errores de configuración si los hay
    /// </summary>
    public List<string> ConfigurationErrors { get; set; } = new();
}

/// <summary>
/// Estado de salud de todos los proveedores
/// </summary>
public class AllProvidersHealthDto
{
    /// <summary>
    /// Estado general del servicio de pagos
    /// </summary>
    public string OverallStatus { get; set; } = "healthy";

    /// <summary>
    /// Nombre del servicio
    /// </summary>
    public string Service { get; set; } = "PaymentService";

    /// <summary>
    /// Total de proveedores registrados
    /// </summary>
    public int TotalProviders { get; set; }

    /// <summary>
    /// Proveedores disponibles
    /// </summary>
    public int AvailableProviders { get; set; }

    /// <summary>
    /// Proveedores con problemas
    /// </summary>
    public int UnavailableProviders { get; set; }

    /// <summary>
    /// Gateway por defecto
    /// </summary>
    public string DefaultGateway { get; set; } = string.Empty;

    /// <summary>
    /// Estado de cada proveedor
    /// </summary>
    public List<ProviderHealthDto> Providers { get; set; } = new();

    /// <summary>
    /// Fecha/hora de la verificación
    /// </summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
