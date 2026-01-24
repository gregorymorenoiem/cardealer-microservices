using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for dealer onboarding processes
/// </summary>
public class DealerOnboardingRepository : IDealerOnboardingRepository
{
    private readonly ApplicationDbContext _context;

    public DealerOnboardingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DealerOnboardingProcess?> GetByDealerIdAsync(Guid dealerId)
    {
        return await _context.DealerOnboardingProcesses
            .FirstOrDefaultAsync(o => o.DealerId == dealerId);
    }

    public async Task<DealerOnboardingProcess> CreateAsync(DealerOnboardingProcess process)
    {
        await _context.DealerOnboardingProcesses.AddAsync(process);
        await _context.SaveChangesAsync();
        return process;
    }

    public async Task UpdateAsync(DealerOnboardingProcess process)
    {
        _context.DealerOnboardingProcesses.Update(process);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid dealerId)
    {
        return await _context.DealerOnboardingProcesses
            .AnyAsync(o => o.DealerId == dealerId);
    }
}
