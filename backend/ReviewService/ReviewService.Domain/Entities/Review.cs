using CarDealer.Shared.Domain.Base;

namespace ReviewService.Domain.Entities;

/// &lt;summary&gt;
/// Review de un comprador sobre un vendedor/dealer
/// Sistema estilo Amazon con rating 1-5 estrellas + texto
/// &lt;/summary&gt;
public class Review : BaseEntity&lt;Guid&gt;
{
    /// &lt;summary&gt;
    /// ID del comprador que deja la review
    /// &lt;/summary&gt;
    public Guid BuyerId { get; set; }

    /// &lt;summary&gt;
    /// ID del vendedor que recibe la review
    /// &lt;/summary&gt;
    public Guid SellerId { get; set; }

    /// &lt;summary&gt;
    /// ID del vehículo sobre el que es la review (opcional)
    /// &lt;/summary&gt;
    public Guid? VehicleId { get; set; }

    /// &lt;summary&gt;
    /// ID de la transacción/orden que validó la compra
    /// Para mostrar badge "Compra verificada"
    /// &lt;/summary&gt;
    public Guid? OrderId { get; set; }

    /// &lt;summary&gt;
    /// Rating de 1 a 5 estrellas
    /// &lt;/summary&gt;
    public int Rating { get; set; }

    /// &lt;summary&gt;
    /// Título de la review (ej: "Excelente servicio")
    /// &lt;/summary&gt;
    public string Title { get; set; } = string.Empty;

    /// &lt;summary&gt;
    /// Contenido detallado de la review
    /// &lt;/summary&gt;
    public string Content { get; set; } = string.Empty;

    /// &lt;summary&gt;
    /// Si la review está aprobada por moderación
    /// &lt;/summary&gt;
    public bool IsApproved { get; set; } = true;

    /// &lt;summary&gt;
    /// Si la review es de una compra verificada
    /// &lt;/summary&gt;
    public bool IsVerifiedPurchase { get; set; } = false;

    /// &lt;summary&gt;
    /// Razón de rechazo si IsApproved = false
    /// &lt;/summary&gt;
    public string? RejectionReason { get; set; }

    /// &lt;summary&gt;
    /// ID del admin que moderó
    /// &lt;/summary&gt;
    public Guid? ModeratedById { get; set; }

    /// &lt;summary&gt;
    /// Fecha de moderación
    /// &lt;/summary&gt;
    public DateTime? ModeratedAt { get; set; }

    /// &lt;summary&gt;
    /// Nombre del comprador (cache para performance)
    /// &lt;/summary&gt;
    public string BuyerName { get; set; } = string.Empty;

    /// &lt;summary&gt;
    /// Foto del comprador (cache)
    /// &lt;/summary&gt;
    public string? BuyerPhotoUrl { get; set; }

    /// &lt;summary&gt;
    /// Si la review fue útil para otros usuarios (upvotes)
    /// &lt;/summary&gt;
    public int HelpfulVotes { get; set; } = 0;

    /// &lt;summary&gt;
    /// Total de votos recibidos
    /// &lt;/summary&gt;
    public int TotalVotes { get; set; } = 0;

    // Navigation properties
    
    /// &lt;summary&gt;
    /// Respuesta del vendedor a esta review
    /// &lt;/summary&gt;
    public ReviewResponse? Response { get; set; }
}