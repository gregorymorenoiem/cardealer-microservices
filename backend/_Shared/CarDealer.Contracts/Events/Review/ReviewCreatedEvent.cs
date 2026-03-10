using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Review;

/// <summary>
/// Evento emitido cuando un comprador crea una nueva review para un vendedor/dealer.
/// Consumido por VehiclesSaleService para actualizar SellerRating/SellerReviewCount
/// y recalcular el OKLA Platform Score (D4: Seller Reputation).
/// 
/// SWITCHING COST: Las reviews acumuladas están atadas al perfil OKLA del dealer.
/// Si el dealer cancela, pierde este historial de reputación.
/// </summary>
public class ReviewCreatedEvent : EventBase
{
    public override string EventType => "reviews.review.created";

    /// <summary>ID de la review creada</summary>
    public Guid ReviewId { get; set; }

    /// <summary>ID del comprador que dejó la review</summary>
    public Guid BuyerId { get; set; }

    /// <summary>ID del vendedor/dealer que recibe la review</summary>
    public Guid SellerId { get; set; }

    /// <summary>ID del dealer (tenant). Guid.Empty si es vendedor individual.</summary>
    public Guid? DealerId { get; set; }

    /// <summary>ID del vehículo (si la review es sobre un vehículo específico)</summary>
    public Guid? VehicleId { get; set; }

    /// <summary>ID de la transacción de venta que valida la compra verificada</summary>
    public Guid? OrderId { get; set; }

    /// <summary>Rating 1-5 estrellas</summary>
    public int Rating { get; set; }

    /// <summary>Título de la review</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Es compra verificada (tiene OrderId válido)</summary>
    public bool IsVerifiedPurchase { get; set; }

    /// <summary>Nombre del comprador</summary>
    public string BuyerName { get; set; } = string.Empty;

    // ── Datos agregados actualizados del vendedor (post-review) ──

    /// <summary>Nuevo rating promedio del vendedor tras esta review</summary>
    public decimal NewAverageRating { get; set; }

    /// <summary>Nuevo total de reviews del vendedor tras esta review</summary>
    public int NewTotalReviews { get; set; }

    /// <summary>Total de reviews de compra verificada</summary>
    public int VerifiedPurchaseReviewCount { get; set; }
}
