using ReviewService.Domain.Base;
using ReviewService.Domain.Entities;

namespace ReviewService.Domain.Interfaces;

/// <summary>
/// Repository para gestión de reviews
/// </summary>
public interface IReviewRepository : IRepository<Review, Guid>
{
    /// <summary>
    /// Obtener reviews de un vendedor con paginación
    /// </summary>
    Task<(IEnumerable<Review> Reviews, int TotalCount)> GetBySellerIdAsync(
        Guid sellerId, 
        int page = 1, 
        int pageSize = 20,
        bool onlyApproved = true);

    /// <summary>
    /// Obtener reviews de un comprador
    /// </summary>
    Task<IEnumerable<Review>> GetByBuyerIdAsync(Guid buyerId);

    /// <summary>
    /// Verificar si un comprador ya dejó review para un vendedor específico
    /// </summary>
    Task<bool> HasBuyerReviewedSellerAsync(Guid buyerId, Guid sellerId, Guid? vehicleId = null);

    /// <summary>
    /// Obtener review por OrderId para validar compra verificada
    /// </summary>
    Task<Review?> GetByOrderIdAsync(Guid orderId);

    /// <summary>
    /// Obtener reviews recientes para moderación
    /// </summary>
    Task<IEnumerable<Review>> GetPendingModerationAsync(int limit = 50);

    /// <summary>
    /// Buscar reviews por contenido (para moderar)
    /// </summary>
    Task<IEnumerable<Review>> SearchByContentAsync(string searchTerm);

    /// <summary>
    /// Obtener top reviews (más útiles) para un vendedor
    /// </summary>
    Task<IEnumerable<Review>> GetTopReviewsAsync(Guid sellerId, int limit = 5);
}