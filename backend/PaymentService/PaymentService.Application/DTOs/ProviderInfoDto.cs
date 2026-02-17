namespace PaymentService.Application.DTOs;

/// <summary>
/// Información de un proveedor de pagos
/// </summary>
public class ProviderInfoDto
{
    /// <summary>
    /// Identificador del gateway (AZUL, CardNET, PixelPay, Fygaro, PayPal)
    /// </summary>
    public string Gateway { get; set; } = string.Empty;

    /// <summary>
    /// Nombre comercial del proveedor
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de proveedor (Banking, Fintech, Aggregator)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Porcentaje de comisión del proveedor
    /// </summary>
    public decimal CommissionPercentage { get; set; }

    /// <summary>
    /// Tarifa fija por transacción
    /// </summary>
    public decimal FixedFee { get; set; }

    /// <summary>
    /// Monedas soportadas
    /// </summary>
    public List<string> SupportedCurrencies { get; set; } = new();

    /// <summary>
    /// Indica si el proveedor está correctamente configurado
    /// </summary>
    public bool IsConfigured { get; set; }

    /// <summary>
    /// Lista de errores de configuración (vacío si IsConfigured = true)
    /// </summary>
    public List<string> ConfigurationErrors { get; set; } = new();

    /// <summary>
    /// Indica si el proveedor soporta tokenización
    /// </summary>
    public bool SupportsTokenization { get; set; }

    /// <summary>
    /// Indica si el proveedor soporta 3D Secure
    /// </summary>
    public bool Supports3DSecure { get; set; }

    /// <summary>
    /// Indica si el proveedor soporta reembolsos
    /// </summary>
    public bool SupportsRefunds { get; set; }

    /// <summary>
    /// Indica si el proveedor soporta pagos recurrentes
    /// </summary>
    public bool SupportsRecurring { get; set; }

    /// <summary>
    /// Cobertura geográfica del proveedor
    /// </summary>
    public string Coverage { get; set; } = string.Empty;
}
