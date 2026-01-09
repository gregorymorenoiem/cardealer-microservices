using ReviewService.Domain.Base;
using ReviewService.Domain.Entities;

namespace ReviewService.Domain.Interfaces;

/// <summary>
/// Repository para respuestas de vendedores a reviews
/// </summary>
public interface IReviewResponseRepository : IRepository<ReviewResponse, Guid>
{
    /// <summary>
    /// Obtener respuesta por ReviewId
    /// </summary>
    Task<ReviewResponse?> GetByReviewIdAsync(Guid reviewId);

    /// <summary>
    /// Obtener todas las respuestas de un vendedor
    /// </summary>
    Task<IEnumerable<ReviewResponse>> GetBySellerIdAsync(Guid sellerId);

    /// <summary>
    /// Verificar si una review ya tiene respuesta
    /// </summary>
    Task<bool> HasResponseAsync(Guid reviewId);
}