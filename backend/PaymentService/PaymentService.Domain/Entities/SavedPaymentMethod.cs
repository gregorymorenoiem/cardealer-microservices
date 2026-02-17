using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

/// <summary>
/// Representa un método de pago guardado (tarjeta tokenizada) de un usuario
/// Permite realizar pagos recurrentes sin solicitar datos de tarjeta cada vez
/// </summary>
public class SavedPaymentMethod
{
    /// <summary>
    /// ID único del método de pago guardado
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del usuario propietario del método de pago
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Pasarela de pago donde está tokenizada la tarjeta
    /// </summary>
    public PaymentGateway PaymentGateway { get; set; }

    /// <summary>
    /// Token de la tarjeta en la pasarela (encriptado)
    /// AZUL: DataVault token
    /// PayPal: Vault token
    /// PixelPay: Native token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de método de pago
    /// </summary>
    public SavedPaymentMethodType Type { get; set; }

    /// <summary>
    /// Nombre personalizado del método de pago
    /// Ej: "Mi tarjeta personal", "Tarjeta del negocio"
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// Indica si es el método de pago por defecto del usuario
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Indica si el método está activo
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ========== Datos de la tarjeta (enmascarados) ==========

    /// <summary>
    /// Marca de la tarjeta (Visa, Mastercard, Amex, etc.)
    /// </summary>
    public string CardBrand { get; set; } = string.Empty;

    /// <summary>
    /// Últimos 4 dígitos de la tarjeta
    /// </summary>
    public string CardLast4 { get; set; } = string.Empty;

    /// <summary>
    /// Mes de expiración (1-12)
    /// </summary>
    public int ExpirationMonth { get; set; }

    /// <summary>
    /// Año de expiración (4 dígitos)
    /// </summary>
    public int ExpirationYear { get; set; }

    /// <summary>
    /// Nombre del titular de la tarjeta (si está disponible)
    /// </summary>
    public string? CardHolderName { get; set; }

    /// <summary>
    /// País del banco emisor (ISO 3166-1 alpha-2)
    /// </summary>
    public string? BankCountry { get; set; }

    /// <summary>
    /// Nombre del banco emisor (si está disponible)
    /// </summary>
    public string? BankName { get; set; }

    // ========== Datos de cuenta bancaria (si aplica) ==========

    /// <summary>
    /// Últimos 4 dígitos de la cuenta bancaria
    /// </summary>
    public string? AccountLast4 { get; set; }

    /// <summary>
    /// Tipo de cuenta (checking, savings)
    /// </summary>
    public string? AccountType { get; set; }

    /// <summary>
    /// Nombre del banco
    /// </summary>
    public string? AccountBankName { get; set; }

    // ========== Metadatos ==========

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Última fecha de uso
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Número de veces que se ha usado
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Dirección de facturación (JSON serializado)
    /// </summary>
    public string? BillingAddressJson { get; set; }

    /// <summary>
    /// Referencia externa en la pasarela (para reconciliación)
    /// </summary>
    public string? ExternalReference { get; set; }

    // ========== Métodos de ayuda ==========

    /// <summary>
    /// Verifica si la tarjeta está expirada
    /// </summary>
    public bool IsExpired()
    {
        var now = DateTime.UtcNow;
        var expiryDate = new DateTime(ExpirationYear, ExpirationMonth, 1).AddMonths(1).AddDays(-1);
        return now > expiryDate;
    }

    /// <summary>
    /// Verifica si la tarjeta expira dentro de los próximos meses
    /// </summary>
    public bool ExpiresWithinMonths(int months)
    {
        var futureDate = DateTime.UtcNow.AddMonths(months);
        var expiryDate = new DateTime(ExpirationYear, ExpirationMonth, 1).AddMonths(1).AddDays(-1);
        return futureDate > expiryDate && !IsExpired();
    }

    /// <summary>
    /// Retorna el número de tarjeta enmascarado
    /// </summary>
    public string GetMaskedNumber()
    {
        return $"•••• •••• •••• {CardLast4}";
    }

    /// <summary>
    /// Retorna la fecha de expiración formateada
    /// </summary>
    public string GetFormattedExpiry()
    {
        return $"{ExpirationMonth:D2}/{ExpirationYear}";
    }

    /// <summary>
    /// Retorna una descripción amigable del método de pago
    /// </summary>
    public string GetDisplayName()
    {
        if (!string.IsNullOrEmpty(NickName))
            return NickName;

        if (Type == SavedPaymentMethodType.Card)
            return $"{CardBrand} •••• {CardLast4}";

        if (Type == SavedPaymentMethodType.BankAccount)
            return $"{AccountBankName ?? "Cuenta"} •••• {AccountLast4}";

        return "Método de pago";
    }

    /// <summary>
    /// Registra un uso del método de pago
    /// </summary>
    public void RecordUsage()
    {
        LastUsedAt = DateTime.UtcNow;
        UsageCount++;
    }
}

/// <summary>
/// Tipo de método de pago guardado
/// </summary>
public enum SavedPaymentMethodType
{
    /// <summary>
    /// Tarjeta de crédito o débito
    /// </summary>
    Card = 1,

    /// <summary>
    /// Cuenta bancaria (ACH)
    /// </summary>
    BankAccount = 2,

    /// <summary>
    /// PayPal
    /// </summary>
    PayPal = 3,

    /// <summary>
    /// Otro tipo de método
    /// </summary>
    Other = 99
}
