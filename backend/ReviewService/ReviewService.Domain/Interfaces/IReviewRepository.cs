using ReviewService.Domain.Entities;
using CarDealer.Shared.Persistence;

namespace ReviewService.Domain.Interfaces;

/// &lt;summary&gt;
/// Repository para gestión de reviews
/// &lt;/summary&gt;
public interface IReviewRepository : IRepository&lt;Review, Guid&gt;
{
    /// &lt;summary&gt;
    /// Obtener reviews de un vendedor con paginación
    /// &lt;/summary&gt;
    Task&lt;(IEnumerable&lt;Review&gt; Reviews, int TotalCount)&gt; GetBySellerIdAsync(
        Guid sellerId, 
        int page = 1, 
        int pageSize = 20,
        bool onlyApproved = true);

    /// &lt;summary&gt;
    /// Obtener reviews de un comprador
    /// &lt;/summary&gt;
    Task&lt;IEnumerable&lt;Review&gt;&gt; GetByBuyerIdAsync(Guid buyerId);

    /// &lt;summary&gt;
    /// Verificar si un comprador ya dejó review para un vendedor específico
    /// &lt;/summary&gt;
    Task&lt;bool&gt; HasBuyerReviewedSellerAsync(Guid buyerId, Guid sellerId, Guid? vehicleId = null);

    /// &lt;summary&gt;
    /// Obtener review por OrderId para validar compra verificada
    /// &lt;/summary&gt;
    Task&lt;Review?&gt; GetByOrderIdAsync(Guid orderId);

    /// &lt;summary&gt;
    /// Obtener reviews recientes para moderación
    /// &lt;/summary&gt;
    Task&lt;IEnumerable&lt;Review&gt;&gt; GetPendingModerationAsync(int limit = 50);

    /// &lt;summary&gt;
    /// Buscar reviews por contenido (para moderar)
    /// &lt;/summary&gt;
    Task&lt;IEnumerable&lt;Review&gt;&gt; SearchByContentAsync(string searchTerm);

    /// &lt;summary&gt;
    /// Obtener top reviews (más útiles) para un vendedor
    /// &lt;/summary&gt;
    Task&lt;IEnumerable&lt;Review&gt;&gt; GetTopReviewsAsync(Guid sellerId, int limit = 5);
}