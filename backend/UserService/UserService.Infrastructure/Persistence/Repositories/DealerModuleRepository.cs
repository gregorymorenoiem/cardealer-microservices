using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for dealer module subscriptions
/// </summary>
public class DealerModuleRepository : IDealerModuleRepository
{
    private readonly ApplicationDbContext _context;

    public DealerModuleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DealerModule>> GetActiveByDealerIdAsync(Guid dealerId)
    {
        return await _context.DealerModules
            .Where(dm => dm.DealerId == dealerId && dm.IsActive)
            .Include(dm => dm.Module)
            .OrderBy(dm => dm.Module.Name)
            .ToListAsync();
    }

    public async Task<DealerModule?> GetByIdAsync(Guid dealerId, Guid moduleId)
    {
        return await _context.DealerModules
            .Include(dm => dm.Module)
            .FirstOrDefaultAsync(dm => dm.DealerId == dealerId && dm.ModuleId == moduleId);
    }

    public async Task<DealerModule> AddAsync(DealerModule dealerModule)
    {
        await _context.DealerModules.AddAsync(dealerModule);
        await _context.SaveChangesAsync();
        return dealerModule;
    }

    public async Task UpdateAsync(DealerModule dealerModule)
    {
        _context.DealerModules.Update(dealerModule);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsSubscribedAsync(Guid dealerId, Guid moduleId)
    {
        return await _context.DealerModules
            .AnyAsync(dm => dm.DealerId == dealerId && dm.ModuleId == moduleId && dm.IsActive);
    }
}
