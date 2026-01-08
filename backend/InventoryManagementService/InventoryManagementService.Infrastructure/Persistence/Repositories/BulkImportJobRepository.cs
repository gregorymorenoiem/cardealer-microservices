using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InventoryManagementService.Domain.Entities;
using InventoryManagementService.Domain.Interfaces;

namespace InventoryManagementService.Infrastructure.Persistence.Repositories;

public class BulkImportJobRepository : IBulkImportJobRepository
{
    private readonly InventoryDbContext _context;

    public BulkImportJobRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<BulkImportJob?> GetByIdAsync(Guid id)
    {
        return await _context.BulkImportJobs.FindAsync(id);
    }

    public async Task<List<BulkImportJob>> GetByDealerIdAsync(Guid dealerId)
    {
        return await _context.BulkImportJobs
            .Where(j => j.DealerId == dealerId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<(List<BulkImportJob> Jobs, int TotalCount)> GetPagedByDealerIdAsync(
        Guid dealerId,
        int page,
        int pageSize,
        ImportJobStatus? status = null)
    {
        var query = _context.BulkImportJobs.Where(j => j.DealerId == dealerId);

        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        var totalCount = await query.CountAsync();

        var jobs = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (jobs, totalCount);
    }

    public async Task<BulkImportJob> CreateAsync(BulkImportJob job)
    {
        _context.BulkImportJobs.Add(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<BulkImportJob> UpdateAsync(BulkImportJob job)
    {
        _context.BulkImportJobs.Update(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task DeleteAsync(Guid id)
    {
        var job = await GetByIdAsync(id);
        if (job != null)
        {
            _context.BulkImportJobs.Remove(job);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.BulkImportJobs.AnyAsync(j => j.Id == id);
    }
}
