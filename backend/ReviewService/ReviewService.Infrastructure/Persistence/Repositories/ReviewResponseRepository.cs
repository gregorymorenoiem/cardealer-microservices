using Microsoft.EntityFrameworkCore;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Interfaces;
using ReviewService.Domain.Base;
using ReviewService.Infrastructure.Persistence;
using ReviewService.Infrastructure.Persistence;

namespace ReviewService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio para ReviewResponse
/// </summary>
public class ReviewResponseRepository : Repository<ReviewResponse, Guid>, IReviewResponseRepository
{
    private readonly ReviewDbContext _context;

    public ReviewResponseRepository(ReviewDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtener respuesta por ReviewId
    /// </summary>
    public async Task<ReviewResponse?> GetByReviewIdAsync(Guid reviewId)
    {
        return await _context.ReviewResponses
            .Include(rr => rr.Review)
            .FirstOrDefaultAsync(rr => rr.ReviewId == reviewId);
    }

    /// <summary>
    /// Obtener todas las respuestas de un vendedor
    /// </summary>
    public async Task<IEnumerable<ReviewResponse>> GetBySellerIdAsync(Guid sellerId)
    {
        return await _context.ReviewResponses
            .Include(rr => rr.Review)
            .Where(rr => rr.SellerId == sellerId)
            .OrderByDescending(rr => rr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Verificar si una review ya tiene respuesta
    /// </summary>
    public async Task<bool> HasResponseAsync(Guid reviewId)
    {
        return await _context.ReviewResponses
            .AnyAsync(rr => rr.ReviewId == reviewId);
    }
}