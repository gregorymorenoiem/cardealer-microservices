namespace StripePaymentService.Domain.Entities;

/// <summary>
/// Representa un Customer en Stripe
/// </summary>
public class StripeCustomer
{
    /// <summary>
    /// ID interno en OKLA
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del usuario en OKLA
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// ID del Customer en Stripe
    /// </summary>
    public string StripeCustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Email del cliente
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Dirección
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Ciudad
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Estado/Provincia
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Código postal
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// País
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Si el cliente está activo
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Métodos de pago guardados
    /// </summary>
    public List<StripePaymentMethod> PaymentMethods { get; set; } = new();

    /// <summary>
    /// Suscripciones activas
    /// </summary>
    public List<StripeSubscription> Subscriptions { get; set; } = new();
}
