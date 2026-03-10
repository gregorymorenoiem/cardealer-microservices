using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Review;

/// <summary>
/// Evento emitido cuando se actualiza una review (rating cambia, contenido editado, moderación).
/// Consumido por VehiclesSaleService para mantener sincronizado el SellerRating.
/// </summary>
public class ReviewUpdatedEvent : EventBase
{
    public override string EventType => "reviews.review.updated";

    /// <summary>ID de la review actualizada</summary>
    public Guid ReviewId { get; set; }

    /// <summary>ID del vendedor/dealer</summary>
    public Guid SellerId { get; set; }

    /// <summary>ID del dealer (tenant)</summary>
    public Guid? DealerId { get; set; }

    /// <summary>ID del vehículo (si aplica)</summary>
    public Guid? VehicleId { get; set; }

    /// <summary>Nuevo rating (si cambió)</summary>
    public int? NewRating { get; set; }

    /// <summary>Rating anterior</summary>
    public int? OldRating { get; set; }

    /// <summary>Si la review fue aprobada/rechazada por moderación</summary>
    public bool IsApproved { get; set; }

    /// <summary>Tipo de actualización</summary>
    public ReviewUpdateType UpdateType { get; set; }

    // ── Datos agregados actualizados ──
    /// <summary>Rating promedio actualizado del vendedor</summary>
    public decimal NewAverageRating { get; set; }

    /// <summary>Total de reviews actualizadas</summary>
    public int NewTotalReviews { get; set; }
}

/// <summary>
/// Tipo de actualización de review
/// </summary>
public enum ReviewUpdateType
{
    /// <summary>Comprador editó contenido/rating</summary>
    ContentEdit = 0,

    /// <summary>Admin aprobó la review</summary>
    Approved = 1,

    /// <summary>Admin rechazó la review</summary>
    Rejected = 2,

    /// <summary>Review fue eliminada</summary>
    Deleted = 3,

    /// <summary>Review fue reportada y flagged</summary>
    Flagged = 4
}
