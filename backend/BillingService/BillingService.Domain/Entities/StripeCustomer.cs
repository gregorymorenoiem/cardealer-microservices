using CarDealer.Shared.MultiTenancy;

namespace BillingService.Domain.Entities;

/// <summary>
/// Representa un cliente de Stripe asociado a un dealer
/// Se crea automáticamente cuando un dealer se registra
/// </summary>
public class StripeCustomer : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }

    /// <summary>
    /// ID del customer en Stripe (cus_xxx)
    /// </summary>
    public string StripeCustomerId { get; private set; } = string.Empty;

    /// <summary>
    /// Email asociado al customer en Stripe
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Nombre del dealer/empresa
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Teléfono de contacto
    /// </summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// Método de pago por defecto (pm_xxx)
    /// </summary>
    public string? DefaultPaymentMethodId { get; private set; }

    /// <summary>
    /// Balance del customer en Stripe (en centavos)
    /// </summary>
    public long Balance { get; private set; }

    /// <summary>
    /// Moneda preferida
    /// </summary>
    public string Currency { get; private set; } = "usd";

    /// <summary>
    /// Metadatos adicionales en JSON
    /// </summary>
    public string? Metadata { get; private set; }

    /// <summary>
    /// Indica si el customer está activo en Stripe
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Indica si el customer está en modo de prueba
    /// </summary>
    public bool IsTestMode { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private StripeCustomer() { }

    public StripeCustomer(
        Guid dealerId,
        string stripeCustomerId,
        string email,
        string name,
        string? phone = null,
        bool isTestMode = false)
    {
        if (string.IsNullOrWhiteSpace(stripeCustomerId))
            throw new ArgumentException("Stripe customer ID is required", nameof(stripeCustomerId));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        StripeCustomerId = stripeCustomerId;
        Email = email;
        Name = name;
        Phone = phone;
        IsTestMode = isTestMode;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateFromStripe(string email, string name, string? phone, long balance)
    {
        Email = email;
        Name = name;
        Phone = phone;
        Balance = balance;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefaultPaymentMethod(string paymentMethodId)
    {
        DefaultPaymentMethodId = paymentMethodId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMetadata(string metadata)
    {
        Metadata = metadata;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
