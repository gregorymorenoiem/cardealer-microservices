using LeadScoringService.Domain.Entities;
using LeadScoringService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LeadScoringService.Infrastructure.Persistence.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly LeadScoringDbContext _context;

    public LeadRepository(LeadScoringDbContext context)
    {
        _context = context;
    }

    public async Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Include(l => l.Actions.OrderByDescending(a => a.OccurredAt).Take(10))
            .Include(l => l.ScoreHistory.OrderByDescending(h => h.ChangedAt).Take(10))
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<Lead?> GetByUserAndVehicleAsync(Guid userId, Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .FirstOrDefaultAsync(l => l.UserId == userId && l.VehicleId == vehicleId, cancellationToken);
    }

    public async Task<List<Lead>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .OrderByDescending(l => l.Score)
            .ToListAsync(cancellationToken);
    }

    public async Task<Lead> CreateAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        _context.Leads.Add(lead);
        await _context.SaveChangesAsync(cancellationToken);
        return lead;
    }

    public async Task<Lead> UpdateAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        _context.Leads.Update(lead);
        await _context.SaveChangesAsync(cancellationToken);
        return lead;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await GetByIdAsync(id, cancellationToken);
        if (lead != null)
        {
            _context.Leads.Remove(lead);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<Lead>> GetLeadsByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Where(l => l.DealerId == dealerId)
            .OrderByDescending(l => l.Score)
            .ThenByDescending(l => l.LastInteractionAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Lead>> GetHotLeadsByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Where(l => l.DealerId == dealerId && l.Temperature == LeadTemperature.Hot)
            .OrderByDescending(l => l.Score)
            .ThenByDescending(l => l.LastInteractionAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Lead>> GetLeadsByDealerAndTemperatureAsync(Guid dealerId, LeadTemperature temperature, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Where(l => l.DealerId == dealerId && l.Temperature == temperature)
            .OrderByDescending(l => l.Score)
            .ThenByDescending(l => l.LastInteractionAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Lead>> GetLeadsByDealerAndStatusAsync(Guid dealerId, LeadStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Where(l => l.DealerId == dealerId && l.Status == status)
            .OrderByDescending(l => l.Score)
            .ThenByDescending(l => l.LastInteractionAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Lead>> GetLeadsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.LastInteractionAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Lead>> GetLeadsByScoreRangeAsync(int minScore, int maxScore, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Where(l => l.Score >= minScore && l.Score <= maxScore)
            .OrderByDescending(l => l.Score)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Lead>> GetStaleLeadsAsync(int daysSinceLastInteraction, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysSinceLastInteraction);
        return await _context.Leads
            .Where(l => l.LastInteractionAt < cutoffDate && l.Status != LeadStatus.Converted && l.Status != LeadStatus.Lost)
            .OrderBy(l => l.LastInteractionAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Lead>> GetConvertedLeadsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Where(l => l.Status == LeadStatus.Converted && l.ConvertedAt >= fromDate && l.ConvertedAt <= toDate)
            .OrderByDescending(l => l.ConvertedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Lead> Leads, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Guid? dealerId = null,
        LeadTemperature? temperature = null,
        LeadStatus? status = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Leads.AsQueryable();

        if (dealerId.HasValue)
        {
            query = query.Where(l => l.DealerId == dealerId.Value);
        }

        if (temperature.HasValue)
        {
            query = query.Where(l => l.Temperature == temperature.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(l =>
                l.UserEmail.Contains(searchTerm) ||
                l.UserFullName.Contains(searchTerm) ||
                l.VehicleTitle.Contains(searchTerm) ||
                (l.UserPhone != null && l.UserPhone.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var leads = await query
            .OrderByDescending(l => l.Score)
            .ThenByDescending(l => l.LastInteractionAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (leads, totalCount);
    }

    public async Task<int> GetTotalCountByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .CountAsync(l => l.DealerId == dealerId, cancellationToken);
    }

    public async Task<int> GetHotLeadsCountByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .CountAsync(l => l.DealerId == dealerId && l.Temperature == LeadTemperature.Hot, cancellationToken);
    }

    public async Task<decimal> GetAverageScoreByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        var leads = await _context.Leads
            .Where(l => l.DealerId == dealerId)
            .ToListAsync(cancellationToken);

        return leads.Any() ? (decimal)leads.Average(l => l.Score) : 0;
    }

    public async Task<decimal> GetConversionRateByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        var totalLeads = await _context.Leads
            .CountAsync(l => l.DealerId == dealerId, cancellationToken);

        if (totalLeads == 0) return 0;

        var convertedLeads = await _context.Leads
            .CountAsync(l => l.DealerId == dealerId && l.Status == LeadStatus.Converted, cancellationToken);

        return (decimal)convertedLeads / totalLeads * 100;
    }
}
