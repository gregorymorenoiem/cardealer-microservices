using ReviewService.Domain.Base;
using ReviewService.Domain.Entities;

namespace ReviewService.Domain.Interfaces;

/// <summary>
/// Repository para gestión de estadísticas de reviews
/// </summary>
public interface IReviewSummaryRepository : IRepository<ReviewSummary, Guid>
{
    /// <summary>
    /// Obtener o crear summary para un vendedor
    /// </summary>
    Task<ReviewSummary> GetOrCreateBySellerIdAsync(Guid sellerId);

    /// <summary>
    /// Actualizar métricas después de agregar/editar/eliminar review
    /// </summary>
    Task<ReviewSummary> RefreshMetricsAsync(Guid sellerId);

    /// <summary>
    /// Obtener top sellers por rating
    /// </summary>
    Task<IEnumerable<ReviewSummary>> GetTopRatedSellersAsync(int limit = 10);

    /// <summary>
    /// Obtener sellers con métricas desactualizadas (para job automático)
    /// </summary>
    Task<IEnumerable<ReviewSummary>> GetStaleMetricsAsync(DateTime olderThan);
}