using ReviewService.Domain.Base;

namespace ReviewService.Domain.Entities;

/// <summary>
/// Respuesta del vendedor a una review recibida
/// </summary>
public class ReviewResponse : BaseEntity<Guid>
{
    /// <summary>
    /// ID de la review a la que responde
    /// </summary>
    public Guid ReviewId { get; set; }

    /// <summary>
    /// ID del vendedor que responde
    /// </summary>
    public Guid SellerId { get; set; }

    /// <summary>
    /// Contenido de la respuesta
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Si la respuesta está aprobada
    /// </summary>
    public bool IsApproved { get; set; } = true;

    /// <summary>
    /// Nombre del vendedor (cache)
    /// </summary>
    public string SellerName { get; set; } = string.Empty;

    // Navigation properties
    public Review Review { get; set; } = null!;
}