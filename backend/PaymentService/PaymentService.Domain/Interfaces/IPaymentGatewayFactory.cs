using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Interfaces;

/// <summary>
/// Factory para crear instancias de proveedores de pago
/// Implementa el patrón Factory para inyectar dependencias dinámicamente
/// </summary>
public interface IPaymentGatewayFactory
{
    /// <summary>
    /// Obtiene un proveedor de pago específico
    /// </summary>
    /// <param name="gateway">Identificador de la pasarela</param>
    /// <returns>Instancia del proveedor de pago</returns>
    /// <exception cref="KeyNotFoundException">Si la pasarela no está registrada</exception>
    IPaymentGatewayProvider GetProvider(PaymentGateway gateway);

    /// <summary>
    /// Obtiene el proveedor por defecto (generalmente AZUL)
    /// </summary>
    /// <returns>Proveedor por defecto</returns>
    IPaymentGatewayProvider GetDefaultProvider();

    /// <summary>
    /// Obtiene todos los proveedores registrados
    /// </summary>
    /// <returns>Lista de proveedores disponibles</returns>
    IEnumerable<IPaymentGatewayProvider> GetAllProviders();

    /// <summary>
    /// Verifica si un proveedor está disponible y configurado
    /// </summary>
    /// <param name="gateway">Identificador de la pasarela</param>
    /// <returns>True si está disponible y correctamente configurado</returns>
    bool IsProviderAvailable(PaymentGateway gateway);

    /// <summary>
    /// Obtiene estadísticas de las pasarelas (costos, comisiones, etc.)
    /// </summary>
    /// <returns>Dictionary con información de cada pasarela</returns>
    Dictionary<PaymentGateway, GatewayStats> GetGatewayStats();
}

/// <summary>
/// Estadísticas de una pasarela de pago
/// </summary>
public class GatewayStats
{
    public PaymentGateway Gateway { get; set; }
    public string Name { get; set; } = string.Empty;
    public PaymentGatewayType Type { get; set; }
    public string CommissionRange { get; set; } = string.Empty; // Ej: "2.9%-4.5%"
    public string FixedCost { get; set; } = string.Empty; // Ej: "RD$5-10"
    public string MonthlyCost { get; set; } = string.Empty; // Ej: "US$30-50"
    public bool SupportsTokenization { get; set; }
    public string? TokenizationMethod { get; set; }
    public bool IsActive { get; set; }
    public bool IsConfigured { get; set; }
    public List<string> SupportedCurrencies { get; set; } = new();
    public List<PaymentMethod> SupportedPaymentMethods { get; set; } = new();
}
