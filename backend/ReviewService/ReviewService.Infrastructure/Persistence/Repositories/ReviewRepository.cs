using Microsoft.EntityFrameworkCore;
using ReviewService.Domain.Entities;
using ReviewService.Domain.Interfaces;
using ReviewService.Infrastructure.Persistence;
using CarDealer.Shared.Persistence;

namespace ReviewService.Infrastructure.Persistence.Repositories;

/// &lt;summary&gt;
/// Implementación del repositorio para Reviews
/// &lt;/summary&gt;
public class ReviewRepository : Repository&lt;Review, Guid&gt;, IReviewRepository
{
    private readonly ReviewDbContext _context;

    public ReviewRepository(ReviewDbContext context) : base(context)
    {
        _context = context;
    }

    /// &lt;summary&gt;
    /// Obtener reviews de un vendedor con paginación
    /// &lt;/summary&gt;
    public async Task&lt;(IEnumerable&lt;Review&gt; Reviews, int TotalCount)&gt; GetBySellerIdAsync(
        Guid sellerId, 
        int page = 1, 
        int pageSize = 20,
        bool onlyApproved = true)
    {
        var query = _context.Reviews
            .Include(r =&gt; r.Response)
            .Where(r =&gt; r.SellerId == sellerId);

        if (onlyApproved)
        {
            query = query.Where(r =&gt; r.IsApproved);
        }

        var totalCount = await query.CountAsync();

        var reviews = await query
            .OrderByDescending(r =&gt; r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (reviews, totalCount);
    }

    /// &lt;summary&gt;
    /// Obtener reviews de un comprador
    /// &lt;/summary&gt;
    public async Task&lt;IEnumerable&lt;Review&gt;&gt; GetByBuyerIdAsync(Guid buyerId)
    {
        return await _context.Reviews
            .Include(r =&gt; r.Response)
            .Where(r =&gt; r.BuyerId == buyerId)
            .OrderByDescending(r =&gt; r.CreatedAt)
            .ToListAsync();
    }

    /// &lt;summary&gt;
    /// Verificar si un comprador ya dejó review para un vendedor específico
    /// &lt;/summary&gt;
    public async Task&lt;bool&gt; HasBuyerReviewedSellerAsync(Guid buyerId, Guid sellerId, Guid? vehicleId = null)
    {
        var query = _context.Reviews
            .Where(r =&gt; r.BuyerId == buyerId && r.SellerId == sellerId);

        if (vehicleId.HasValue)
        {
            query = query.Where(r =&gt; r.VehicleId == vehicleId.Value);
        }

        return await query.AnyAsync();
    }

    /// &lt;summary&gt;
    /// Obtener review por OrderId para validar compra verificada
    /// &lt;/summary&gt;
    public async Task&lt;Review?&gt; GetByOrderIdAsync(Guid orderId)
    {
        return await _context.Reviews
            .Include(r =&gt; r.Response)
            .FirstOrDefaultAsync(r =&gt; r.OrderId == orderId);
    }

    /// &lt;summary&gt;
    /// Obtener reviews recientes para moderación
    /// &lt;/summary&gt;
    public async Task&lt;IEnumerable&lt;Review&gt;&gt; GetPendingModerationAsync(int limit = 50)
    {
        return await _context.Reviews
            .Where(r =&gt; !r.IsApproved && r.ModeratedById == null)
            .OrderBy(r =&gt; r.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// &lt;summary&gt;
    /// Buscar reviews por contenido (para moderar)
    /// &lt;/summary&gt;
    public async Task&lt;IEnumerable&lt;Review&gt;&gt; SearchByContentAsync(string searchTerm)
    {
        return await _context.Reviews
            .Where(r =&gt; EF.Functions.ILike(r.Title, $"%{searchTerm}%") ||
                        EF.Functions.ILike(r.Content, $"%{searchTerm}%"))
            .OrderByDescending(r =&gt; r.CreatedAt)
            .Take(100)
            .ToListAsync();
    }

    /// &lt;summary&gt;
    /// Obtener top reviews (más útiles) para un vendedor
    /// &lt;/summary&gt;
    public async Task&lt;IEnumerable&lt;Review&gt;&gt; GetTopReviewsAsync(Guid sellerId, int limit = 5)
    {
        return await _context.Reviews
            .Where(r =&gt; r.SellerId == sellerId && r.IsApproved)
            .OrderByDescending(r =&gt; r.HelpfulVotes)
            .ThenByDescending(r =&gt; r.Rating)
            .ThenByDescending(r =&gt; r.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// &lt;summary&gt;
    /// Override GetByIdAsync para incluir Response
    /// &lt;/summary&gt;
    public override async Task&lt;Review?&gt; GetByIdAsync(Guid id)
    {
        return await _context.Reviews
            .Include(r =&gt; r.Response)
            .FirstOrDefaultAsync(r =&gt; r.Id == id);
    }
}