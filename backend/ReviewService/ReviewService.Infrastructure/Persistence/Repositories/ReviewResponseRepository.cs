using Microsoft.EntityFrameworkCore;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Interfaces;
using ReviewService.Infrastructure.Persistence;
using CarDealer.Shared.Persistence;

namespace ReviewService.Infrastructure.Persistence.Repositories;

/// &lt;summary&gt;
/// Implementación del repositorio para ReviewResponse
/// &lt;/summary&gt;
public class ReviewResponseRepository : Repository&lt;ReviewResponse, Guid&gt;, IReviewResponseRepository
{
    private readonly ReviewDbContext _context;

    public ReviewResponseRepository(ReviewDbContext context) : base(context)
    {
        _context = context;
    }

    /// &lt;summary&gt;
    /// Obtener respuesta por ReviewId
    /// &lt;/summary&gt;
    public async Task&lt;ReviewResponse?&gt; GetByReviewIdAsync(Guid reviewId)
    {
        return await _context.ReviewResponses
            .Include(rr =&gt; rr.Review)
            .FirstOrDefaultAsync(rr =&gt; rr.ReviewId == reviewId);
    }

    /// &lt;summary&gt;
    /// Obtener todas las respuestas de un vendedor
    /// &lt;/summary&gt;
    public async Task&lt;IEnumerable&lt;ReviewResponse&gt;&gt; GetBySellerIdAsync(Guid sellerId)
    {
        return await _context.ReviewResponses
            .Include(rr =&gt; rr.Review)
            .Where(rr =&gt; rr.SellerId == sellerId)
            .OrderByDescending(rr =&gt; rr.CreatedAt)
            .ToListAsync();
    }

    /// &lt;summary&gt;
    /// Verificar si una review ya tiene respuesta
    /// &lt;/summary&gt;
    public async Task&lt;bool&gt; HasResponseAsync(Guid reviewId)
    {
        return await _context.ReviewResponses
            .AnyAsync(rr =&gt; rr.ReviewId == reviewId);
    }
}