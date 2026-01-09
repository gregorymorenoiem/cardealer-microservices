using Microsoft.EntityFrameworkCore;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Interfaces;
using ReviewService.Domain.Base;
using ReviewService.Infrastructure.Persistence;
using ReviewService.Infrastructure.Persistence;

namespace ReviewService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio para ReviewSummary
/// </summary>
public class ReviewSummaryRepository : Repository<ReviewSummary, Guid>, IReviewSummaryRepository
{
    private readonly ReviewDbContext _context;

    public ReviewSummaryRepository(ReviewDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtener o crear summary para un vendedor
    /// </summary>
    public async Task<ReviewSummary> GetOrCreateBySellerIdAsync(Guid sellerId)
    {
        var summary = await _context.ReviewSummaries
            .FirstOrDefaultAsync(rs => rs.SellerId == sellerId);

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

    /// <summary>
    /// Actualizar métricas después de agregar/editar/eliminar review
    /// </summary>
    public async Task<ReviewSummary> RefreshMetricsAsync(Guid sellerId)
    {
        var summary = await GetOrCreateBySellerIdAsync(sellerId);

        // Obtener todas las reviews aprobadas del vendedor
        var reviews = await _context.Reviews
            .Where(r => r.SellerId == sellerId && r.IsApproved)
            .ToListAsync();

        // Recalcular métricas
        summary.RecalculateMetrics(reviews);

        // Guardar cambios
        _context.ReviewSummaries.Update(summary);
        await _context.SaveChangesAsync();

        return summary;
    }

    /// <summary>
    /// Obtener top sellers por rating
    /// </summary>
    public async Task<IEnumerable<ReviewSummary>> GetTopRatedSellersAsync(int limit = 10)
    {
        return await _context.ReviewSummaries
            .Where(rs => rs.TotalReviews >= 3) // Mínimo 3 reviews para aparecer
            .OrderByDescending(rs => rs.AverageRating)
            .ThenByDescending(rs => rs.TotalReviews)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Obtener sellers con métricas desactualizadas (para job automático)
    /// </summary>
    public async Task<IEnumerable<ReviewSummary>> GetStaleMetricsAsync(DateTime olderThan)
    {
        return await _context.ReviewSummaries
            .Where(rs => rs.UpdatedAt < olderThan)
            .OrderBy(rs => rs.UpdatedAt)
            .Take(100) // Procesar en batches
            .ToListAsync();
    }
}