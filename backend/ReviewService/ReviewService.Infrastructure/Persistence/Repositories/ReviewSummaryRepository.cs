using Microsoft.EntityFrameworkCore;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Interfaces;
using ReviewService.Infrastructure.Persistence;
using CarDealer.Shared.Persistence;

namespace ReviewService.Infrastructure.Persistence.Repositories;

/// &lt;summary&gt;
/// Implementación del repositorio para ReviewSummary
/// &lt;/summary&gt;
public class ReviewSummaryRepository : Repository&lt;ReviewSummary, Guid&gt;, IReviewSummaryRepository
{
    private readonly ReviewDbContext _context;

    public ReviewSummaryRepository(ReviewDbContext context) : base(context)
    {
        _context = context;
    }

    /// &lt;summary&gt;
    /// Obtener o crear summary para un vendedor
    /// &lt;/summary&gt;
    public async Task&lt;ReviewSummary&gt; GetOrCreateBySellerIdAsync(Guid sellerId)
    {
        var summary = await _context.ReviewSummaries
            .FirstOrDefaultAsync(rs =&gt; rs.SellerId == sellerId);

        if (summary == null)
        {
            summary = new ReviewSummary
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.ReviewSummaries.AddAsync(summary);
            await _context.SaveChangesAsync();
        }

        return summary;
    }

    /// &lt;summary&gt;
    /// Actualizar métricas después de agregar/editar/eliminar review
    /// &lt;/summary&gt;
    public async Task&lt;ReviewSummary&gt; RefreshMetricsAsync(Guid sellerId)
    {
        var summary = await GetOrCreateBySellerIdAsync(sellerId);

        // Obtener todas las reviews aprobadas del vendedor
        var reviews = await _context.Reviews
            .Where(r =&gt; r.SellerId == sellerId && r.IsApproved)
            .ToListAsync();

        // Recalcular métricas
        summary.RecalculateMetrics(reviews);

        // Guardar cambios
        _context.ReviewSummaries.Update(summary);
        await _context.SaveChangesAsync();

        return summary;
    }

    /// &lt;summary&gt;
    /// Obtener top sellers por rating
    /// &lt;/summary&gt;
    public async Task&lt;IEnumerable&lt;ReviewSummary&gt;&gt; GetTopRatedSellersAsync(int limit = 10)
    {
        return await _context.ReviewSummaries
            .Where(rs =&gt; rs.TotalReviews &gt;= 3) // Mínimo 3 reviews para aparecer
            .OrderByDescending(rs =&gt; rs.AverageRating)
            .ThenByDescending(rs =&gt; rs.TotalReviews)
            .Take(limit)
            .ToListAsync();
    }

    /// &lt;summary&gt;
    /// Obtener sellers con métricas desactualizadas (para job automático)
    /// &lt;/summary&gt;
    public async Task&lt;IEnumerable&lt;ReviewSummary&gt;&gt; GetStaleMetricsAsync(DateTime olderThan)
    {
        return await _context.ReviewSummaries
            .Where(rs =&gt; rs.UpdatedAt &lt; olderThan)
            .OrderBy(rs =&gt; rs.UpdatedAt)
            .Take(100) // Procesar en batches
            .ToListAsync();
    }
}