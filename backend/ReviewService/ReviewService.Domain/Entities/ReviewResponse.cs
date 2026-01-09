using CarDealer.Shared.Domain.Base;

namespace ReviewService.Domain.Entities;

/// &lt;summary&gt;
/// Respuesta del vendedor a una review recibida
/// &lt;/summary&gt;
public class ReviewResponse : BaseEntity&lt;Guid&gt;
{
    /// &lt;summary&gt;
    /// ID de la review a la que responde
    /// &lt;/summary&gt;
    public Guid ReviewId { get; set; }

    /// &lt;summary&gt;
    /// ID del vendedor que responde
    /// &lt;/summary&gt;
    public Guid SellerId { get; set; }

    /// &lt;summary&gt;
    /// Contenido de la respuesta
    /// &lt;/summary&gt;
    public string Content { get; set; } = string.Empty;

    /// &lt;summary&gt;
    /// Si la respuesta está aprobada
    /// &lt;/summary&gt;
    public bool IsApproved { get; set; } = true;

    /// &lt;summary&gt;
    /// Nombre del vendedor (cache)
    /// &lt;/summary&gt;
    public string SellerName { get; set; } = string.Empty;

    // Navigation properties
    public Review Review { get; set; } = null!;
}