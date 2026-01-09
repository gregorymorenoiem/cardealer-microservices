using ReviewService.Domain.Entities;
using CarDealer.Shared.Persistence;

namespace ReviewService.Domain.Interfaces;

/// &lt;summary&gt;
/// Repository para respuestas de vendedores a reviews
/// &lt;/summary&gt;
public interface IReviewResponseRepository : IRepository&lt;ReviewResponse, Guid&gt;
{
    /// &lt;summary&gt;
    /// Obtener respuesta por ReviewId
    /// &lt;/summary&gt;
    Task&lt;ReviewResponse?&gt; GetByReviewIdAsync(Guid reviewId);

    /// &lt;summary&gt;
    /// Obtener todas las respuestas de un vendedor
    /// &lt;/summary&gt;
    Task&lt;IEnumerable&lt;ReviewResponse&gt;&gt; GetBySellerIdAsync(Guid sellerId);

    /// &lt;summary&gt;
    /// Verificar si una review ya tiene respuesta
    /// &lt;/summary&gt;
    Task&lt;bool&gt; HasResponseAsync(Guid reviewId);
}