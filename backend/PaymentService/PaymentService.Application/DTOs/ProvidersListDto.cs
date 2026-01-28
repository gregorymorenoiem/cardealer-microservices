namespace PaymentService.Application.DTOs;

/// <summary>
/// Lista de proveedores de pago disponibles
/// </summary>
public class ProvidersListDto
{
    /// <summary>
    /// Número total de proveedores registrados
    /// </summary>
    public int TotalProviders { get; set; }

    /// <summary>
    /// Número de proveedores correctamente configurados
    /// </summary>
    public int ConfiguredProviders { get; set; }

    /// <summary>
    /// Gateway por defecto configurado
    /// </summary>
    public string DefaultGateway { get; set; } = string.Empty;

    /// <summary>
    /// Lista de proveedores disponibles
    /// </summary>
    public List<ProviderInfoDto> Providers { get; set; } = new();
}
