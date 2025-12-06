using CRMService.Domain.Entities;
using CRMService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.Persistence.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly CRMDbContext _context;

    public LeadRepository(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Include(l => l.Activities.OrderByDescending(a => a.CreatedAt).Take(5))
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Lead>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lead>> GetByStatusAsync(LeadStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Where(l => l.Status == status)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lead>> GetByAssignedUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Where(l => l.AssignedToUserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lead>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await _context.Leads
            .Where(l =>
                l.FirstName.ToLower().Contains(term) ||
                l.LastName.ToLower().Contains(term) ||
                l.Email.ToLower().Contains(term) ||
                (l.Company != null && l.Company.ToLower().Contains(term)) ||
                (l.Phone != null && l.Phone.Contains(term)))
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        await _context.Leads.AddAsync(lead, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return lead;
    }

    public async Task UpdateAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        _context.Leads.Update(lead);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await _context.Leads.FindAsync(new object[] { id }, cancellationToken);
        if (lead != null)
        {
            _context.Leads.Remove(lead);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetCountByStatusAsync(LeadStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .CountAsync(l => l.Status == status, cancellationToken);
    }

    public async Task<IEnumerable<Lead>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .OrderByDescending(l => l.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
