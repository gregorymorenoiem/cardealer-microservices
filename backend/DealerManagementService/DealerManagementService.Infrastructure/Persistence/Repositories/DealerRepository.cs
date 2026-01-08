using DealerManagementService.Domain.Entities;
using DealerManagementService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DealerManagementService.Infrastructure.Persistence.Repositories;

public class DealerRepository : IDealerRepository
{
    private readonly DealerDbContext _context;

    public DealerRepository(DealerDbContext context)
    {
        _context = context;
    }

    public async Task<Dealer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Dealers
            .Include(d => d.Documents)
            .Include(d => d.Locations)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Dealer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Dealers
            .Include(d => d.Documents)
            .Include(d => d.Locations)
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
    }

    public async Task<Dealer?> GetByRNCAsync(string rnc, CancellationToken cancellationToken = default)
    {
        return await _context.Dealers
            .FirstOrDefaultAsync(d => d.RNC == rnc, cancellationToken);
    }

    public async Task<IEnumerable<Dealer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Dealers
            .Include(d => d.Documents)
            .Include(d => d.Locations)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Dealer> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        DealerStatus? status = null,
        VerificationStatus? verificationStatus = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Dealers.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        if (verificationStatus.HasValue)
        {
            query = query.Where(d => d.VerificationStatus == verificationStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(d =>
                d.BusinessName.ToLower().Contains(search) ||
                d.RNC.Contains(search) ||
                d.Email.ToLower().Contains(search) ||
                d.City.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Dealer> AddAsync(Dealer dealer, CancellationToken cancellationToken = default)
    {
        await _context.Dealers.AddAsync(dealer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return dealer;
    }

    public async Task UpdateAsync(Dealer dealer, CancellationToken cancellationToken = default)
    {
        _context.Dealers.Update(dealer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dealer = await _context.Dealers.FindAsync(new object[] { id }, cancellationToken);
        if (dealer != null)
        {
            dealer.IsDeleted = true;
            dealer.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Dealers.AnyAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<bool> RNCExistsAsync(string rnc, CancellationToken cancellationToken = default)
    {
        return await _context.Dealers.AnyAsync(d => d.RNC == rnc, cancellationToken);
    }

    public async Task<IEnumerable<Dealer>> GetByStatusAsync(DealerStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Dealers
            .Where(d => d.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Dealer>> GetPendingVerificationAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Dealers
            .Where(d => d.VerificationStatus == VerificationStatus.DocumentsUploaded ||
                       d.VerificationStatus == VerificationStatus.UnderReview)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Dealer>> GetByPlanAsync(DealerPlan plan, CancellationToken cancellationToken = default)
    {
        return await _context.Dealers
            .Where(d => d.CurrentPlan == plan && d.IsSubscriptionActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetActiveDealersCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Dealers
            .CountAsync(d => d.Status == DealerStatus.Active, cancellationToken);
    }

    public async Task UpdateSubscriptionAsync(
        Guid dealerId,
        Guid subscriptionId,
        DealerPlan plan,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var dealer = await _context.Dealers.FindAsync(new object[] { dealerId }, cancellationToken);
        if (dealer != null)
        {
            dealer.SubscriptionId = subscriptionId;
            dealer.CurrentPlan = plan;
            dealer.SubscriptionStartDate = startDate;
            dealer.SubscriptionEndDate = endDate;
            dealer.IsSubscriptionActive = true;
            
            // Set limits based on plan
            dealer.MaxActiveListings = plan switch
            {
                DealerPlan.Starter => 15,
                DealerPlan.Pro => 50,
                DealerPlan.Enterprise => int.MaxValue,
                _ => 0
            };
            
            dealer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeactivateSubscriptionAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        var dealer = await _context.Dealers.FindAsync(new object[] { dealerId }, cancellationToken);
        if (dealer != null)
        {
            dealer.IsSubscriptionActive = false;
            dealer.MaxActiveListings = 0;
            dealer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task IncrementActiveListingsAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        var dealer = await _context.Dealers.FindAsync(new object[] { dealerId }, cancellationToken);
        if (dealer != null)
        {
            dealer.CurrentActiveListings++;
            dealer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DecrementActiveListingsAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        var dealer = await _context.Dealers.FindAsync(new object[] { dealerId }, cancellationToken);
        if (dealer != null && dealer.CurrentActiveListings > 0)
        {
            dealer.CurrentActiveListings--;
            dealer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // Sprint 7: Public Profile Methods
    public async Task<Dealer?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Dealers
            .Include(d => d.Locations)
                .ThenInclude(l => l.BusinessHours)
            .FirstOrDefaultAsync(d => d.Slug == slug && d.Status == DealerStatus.Active, cancellationToken);
    }

    public async Task<IEnumerable<Dealer>> GetTrustedDealersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Dealers
            .Where(d => d.IsTrustedDealer && d.Status == DealerStatus.Active)
            .OrderByDescending(d => d.TotalSales)
            .ThenByDescending(d => d.AverageRating)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Dealer>> GetFoundingMembersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Dealers
            .Where(d => d.IsFoundingMember && d.Status == DealerStatus.Active)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Dealer>> GetTopRatedDealersAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Dealers
            .Where(d => d.Status == DealerStatus.Active && d.TotalReviews > 0)
            .OrderByDescending(d => d.AverageRating)
            .ThenByDescending(d => d.TotalReviews)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateProfileAsync(Dealer dealer, CancellationToken cancellationToken = default)
    {
        dealer.UpdatedAt = DateTime.UtcNow;
        _context.Dealers.Update(dealer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeDealerId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Dealers.Where(d => d.Slug == slug);
        
        if (excludeDealerId.HasValue)
        {
            query = query.Where(d => d.Id != excludeDealerId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
}
