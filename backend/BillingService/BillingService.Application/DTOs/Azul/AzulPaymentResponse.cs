namespace BillingService.Application.DTOs.Azul;

public record AzulPaymentResponse
{
    public string OrderNumber { get; init; } = string.Empty;
    public string Amount { get; init; } = string.Empty;
    public string ITBIS { get; init; } = string.Empty;
    public string AuthorizationCode { get; init; } = string.Empty;
    public string DateTime { get; init; } = string.Empty;
    public string ResponseCode { get; init; } = string.Empty;
    public string IsoCode { get; init; } = string.Empty;
    public string ResponseMessage { get; init; } = string.Empty;
    public string ErrorDescription { get; init; } = string.Empty;
    public string RRN { get; init; } = string.Empty;
    public string AzulOrderId { get; init; } = string.Empty;
    public string DataVaultToken { get; init; } = string.Empty;
    public string DataVaultExpiration { get; init; } = string.Empty;
    public string DataVaultBrand { get; init; } = string.Empty;
    public string AuthHash { get; init; } = string.Empty;

    /// <summary>
    /// Azul returns IsoCode "00" for approved transactions.
    /// ResponseCode "ISO8583" indicates the response follows ISO 8583 standard (normal flow).
    /// We check IsoCode == "00" as the primary approval indicator.
    /// </summary>
    public bool IsApproved => IsoCode == "00";
    public bool IsDeclined => IsoCode != "00" && !HasError;
    public bool HasError => string.Equals(ResponseCode, "Error", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns a user-friendly decline reason in Spanish for Dominican card issuers.
    /// Maps ISO 8583 response codes used by BHD, Popular, BanReservas, and other DR banks.
    /// </summary>
    public string GetDeclineReasonSpanish() => IsoCode switch
    {
        "05" => "Tu banco ha declinado la transacción. Contacta a tu banco para autorizar el pago.",
        "14" => "Número de tarjeta inválido. Verifica los datos e intenta de nuevo.",
        "51" => "Fondos insuficientes. Verifica tu balance disponible.",
        "54" => "Tarjeta expirada. Usa una tarjeta vigente.",
        "57" => "Transacción no permitida para este tipo de tarjeta.",
        "61" => "Monto excede el límite de transacción. Intenta con un monto menor.",
        "65" => "Has excedido el límite de transacciones permitidas. Intenta más tarde.",
        "75" => "Has excedido el número de intentos de PIN. Contacta a tu banco.",
        "91" => "El banco emisor no está disponible. Intenta de nuevo en unos minutos.",
        "96" => "Error del sistema. Intenta de nuevo en unos minutos.",
        "N7" => "Tu banco requiere verificación adicional. Contacta a tu banco.",
        _ => !string.IsNullOrEmpty(ResponseMessage) ? ResponseMessage : "Transacción declinada. Intenta con otro método de pago."
    };
}
