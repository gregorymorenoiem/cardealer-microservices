namespace PaymentService.Domain.Enums;

/// <summary>
/// Tipo de pasarela (categoría)
/// </summary>
public enum PaymentGatewayType
{
    /// <summary>
    /// Pasarela bancaria tradicional
    /// Ejemplo: AZUL, CardNET
    /// </summary>
    Banking = 0,

    /// <summary>
    /// Fintech moderna
    /// Ejemplo: PixelPay
    /// </summary>
    Fintech = 1,

    /// <summary>
    /// Agregador de pagos
    /// Ejemplo: Fygaro
    /// </summary>
    Aggregator = 2
}
