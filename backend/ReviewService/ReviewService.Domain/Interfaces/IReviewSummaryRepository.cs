using ReviewService.Domain.Entities;
using CarDealer.Shared.Persistence;

namespace ReviewService.Domain.Interfaces;

/// &lt;summary&gt;
/// Repository para gestión de estadísticas de reviews
/// &lt;/summary&gt;
public interface IReviewSummaryRepository : IRepository&lt;ReviewSummary, Guid&gt;
{
    /// &lt;summary&gt;
    /// Obtener o crear summary para un vendedor
    /// &lt;/summary&gt;
    Task&lt;ReviewSummary&gt; GetOrCreateBySellerIdAsync(Guid sellerId);

    /// &lt;summary&gt;
    /// Actualizar métricas después de agregar/editar/eliminar review
    /// &lt;/summary&gt;
    Task&lt;ReviewSummary&gt; RefreshMetricsAsync(Guid sellerId);

    /// &lt;summary&gt;
    /// Obtener top sellers por rating
    /// &lt;/summary&gt;
    Task&lt;IEnumerable&lt;ReviewSummary&gt;&gt; GetTopRatedSellersAsync(int limit = 10);

    /// &lt;summary&gt;
    /// Obtener sellers con métricas desactualizadas (para job automático)
    /// &lt;/summary&gt;
    Task&lt;IEnumerable&lt;ReviewSummary&gt;&gt; GetStaleMetricsAsync(DateTime olderThan);
}