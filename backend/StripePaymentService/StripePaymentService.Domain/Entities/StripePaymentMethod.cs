namespace StripePaymentService.Domain.Entities;

/// <summary>
/// Representa un Payment Method en Stripe
/// </summary>
public class StripePaymentMethod
{
    /// <summary>
    /// ID interno en OKLA
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del Customer
    /// </summary>
    public Guid StripeCustomerId { get; set; }

    /// <summary>
    /// Alias for StripeCustomerId (for EF Core navigation)
    /// </summary>
    public Guid CustomerId
    {
        get => StripeCustomerId;
        set => StripeCustomerId = value;
    }

    /// <summary>
    /// Relación con StripeCustomer
    /// </summary>
    public StripeCustomer? Customer { get; set; }

    /// <summary>
    /// ID del Payment Method en Stripe
    /// </summary>
    public string StripePaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de método (card, bank_account, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Marca de la tarjeta (Visa, Mastercard, etc.)
    /// </summary>
    public string? CardBrand { get; set; }

    /// <summary>
    /// Alias for CardBrand
    /// </summary>
    public string? Brand
    {
        get => CardBrand;
        set => CardBrand = value;
    }

    /// <summary>
    /// Últimos 4 dígitos
    /// </summary>
    public string? Last4 { get; set; }

    /// <summary>
    /// Mes de expiración
    /// </summary>
    public int? ExpMonth { get; set; }

    /// <summary>
    /// Año de expiración
    /// </summary>
    public int? ExpYear { get; set; }

    /// <summary>
    /// Si es el método predeterminado
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Si el método está activo
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Información del Wallet (Apple Pay, Google Pay, etc.)
    /// </summary>
    public string? WalletType { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
