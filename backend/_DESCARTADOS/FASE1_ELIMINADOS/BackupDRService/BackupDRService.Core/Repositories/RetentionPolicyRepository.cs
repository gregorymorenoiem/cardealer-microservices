using BackupDRService.Core.Data;
using BackupDRService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackupDRService.Core.Repositories;

public class RetentionPolicyRepository : IRetentionPolicyRepository
{
    private readonly BackupDbContext _context;

    public RetentionPolicyRepository(BackupDbContext context)
    {
        _context = context;
    }

    public async Task<RetentionPolicy?> GetByIdAsync(int id)
    {
        return await _context.RetentionPolicies
            .Include(p => p.BackupSchedules)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<RetentionPolicy?> GetByNameAsync(string name)
    {
        return await _context.RetentionPolicies
            .FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<IEnumerable<RetentionPolicy>> GetAllAsync()
    {
        return await _context.RetentionPolicies
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<RetentionPolicy>> GetActiveAsync()
    {
        return await _context.RetentionPolicies
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<RetentionPolicy> CreateAsync(RetentionPolicy policy)
    {
        _context.RetentionPolicies.Add(policy);
        await _context.SaveChangesAsync();
        return policy;
    }

    public async Task<RetentionPolicy> UpdateAsync(RetentionPolicy policy)
    {
        policy.UpdatedAt = DateTime.UtcNow;
        _context.RetentionPolicies.Update(policy);
        await _context.SaveChangesAsync();
        return policy;
    }

    public async Task DeleteAsync(int id)
    {
        var policy = await _context.RetentionPolicies.FindAsync(id);
        if (policy != null)
        {
            _context.RetentionPolicies.Remove(policy);
            await _context.SaveChangesAsync();
        }
    }
}
