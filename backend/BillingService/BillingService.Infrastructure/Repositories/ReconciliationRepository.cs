using Microsoft.EntityFrameworkCore;
using BillingService.Domain.Entities;
using BillingService.Domain.Interfaces;
using BillingService.Infrastructure.Persistence;

namespace BillingService.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for reconciliation reports and discrepancies.
/// CONTRA #6 FIX: Persists reconciliation results for audit trail.
/// </summary>
public class ReconciliationRepository : IReconciliationRepository
{
    private readonly BillingDbContext _context;

    public ReconciliationRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<ReconciliationReport?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReconciliationReports
            .Include(r => r.Discrepancies)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<ReconciliationReport?> GetLatestByPeriodAsync(string period, CancellationToken ct = default)
    {
        return await _context.ReconciliationReports
            .Include(r => r.Discrepancies)
            .Where(r => r.Period == period)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<ReconciliationReport>> GetAllAsync(int limit = 30, CancellationToken ct = default)
    {
        return await _context.ReconciliationReports
            .OrderByDescending(r => r.StartedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ReconciliationReport>> GetByStatusAsync(
        ReconciliationStatus status, CancellationToken ct = default)
    {
        return await _context.ReconciliationReports
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync(ct);
    }

    public async Task<ReconciliationReport> AddAsync(ReconciliationReport report, CancellationToken ct = default)
    {
        _context.ReconciliationReports.Add(report);
        await _context.SaveChangesAsync(ct);
        return report;
    }

    public async Task UpdateAsync(ReconciliationReport report, CancellationToken ct = default)
    {
        _context.ReconciliationReports.Update(report);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ReconciliationDiscrepancy?> GetDiscrepancyByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReconciliationDiscrepancies
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<IEnumerable<ReconciliationDiscrepancy>> GetUnresolvedDiscrepanciesAsync(
        CancellationToken ct = default)
    {
        return await _context.ReconciliationDiscrepancies
            .Where(d => !d.IsAutoResolved && d.ResolvedAt == null)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ReconciliationDiscrepancy>> GetDiscrepanciesByDealerAsync(
        Guid dealerId, CancellationToken ct = default)
    {
        return await _context.ReconciliationDiscrepancies
            .Where(d => d.DealerId == dealerId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task UpdateDiscrepancyAsync(ReconciliationDiscrepancy discrepancy, CancellationToken ct = default)
    {
        _context.ReconciliationDiscrepancies.Update(discrepancy);
        await _context.SaveChangesAsync(ct);
    }
}
