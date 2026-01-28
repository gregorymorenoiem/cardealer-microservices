namespace PaymentService.Domain.Enums;

/// <summary>
/// Pasarelas de pago soportadas
/// </summary>
public enum PaymentGateway
{
    /// <summary>
    /// AZUL - Banco Popular RD
    /// Comisión: 2.9%-4.5% + RD$5-10 + US$30-50/mes
    /// Tokenización: Incluida (Cybersource)
    /// </summary>
    Azul = 0,

    /// <summary>
    /// CardNET - Bancaria RD
    /// Comisión: 2.5%-4.5% + RD$5-10 + US$30-50/mes
    /// Tokenización: Sí (Solicitar)
    /// </summary>
    CardNET = 1,

    /// <summary>
    /// PixelPay - Fintech
    /// Comisión: 1.0%-3.5% + US$0.15-0.25 + Varía
    /// Tokenización: Nativa (API fácil)
    /// </summary>
    PixelPay = 2,

    /// <summary>
    /// Fygaro - Agregador
    /// Comisión: Varía + Varía + US$15+/mes
    /// Tokenización: Módulo Suscripciones
    /// </summary>
    Fygaro = 3,

    /// <summary>
    /// PayPal - Proveedor Internacional
    /// Comisión: 2.9% + US$0.30 + Gratis (con límites)
    /// Tokenización: Nativa (Vault)
    /// Monedas: USD, EUR, DOP
    /// Alcance: Global - Aceptado en 200+ países
    /// </summary>
    PayPal = 4
}
