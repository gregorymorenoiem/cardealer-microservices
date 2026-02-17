namespace PaymentService.Domain.Enums;

/// <summary>
/// Métodos de pago soportados por AZUL
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Tarjeta de crédito (Visa, Mastercard, Amex)
    /// </summary>
    CreditCard = 0,

    /// <summary>
    /// Tarjeta de débito
    /// </summary>
    DebitCard = 1,

    /// <summary>
    /// Transferencia bancaria (ACH local RD)
    /// </summary>
    ACH = 2,

    /// <summary>
    /// Pago móvil (Orange Money, Claro Money)
    /// </summary>
    MobilePayment = 3,

    /// <summary>
    /// Billetera electrónica
    /// </summary>
    EWallet = 4,

    /// <summary>
    /// Token de tarjeta (para recurrentes)
    /// </summary>
    TokenizedCard = 5
}
