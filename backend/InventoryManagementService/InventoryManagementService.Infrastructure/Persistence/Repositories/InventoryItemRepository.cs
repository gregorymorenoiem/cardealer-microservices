using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InventoryManagementService.Domain.Entities;
using InventoryManagementService.Domain.Interfaces;

namespace InventoryManagementService.Infrastructure.Persistence.Repositories;

public class InventoryItemRepository : IInventoryItemRepository
{
    private readonly InventoryDbContext _context;

    public InventoryItemRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItem?> GetByIdAsync(Guid id)
    {
        return await _context.InventoryItems.FindAsync(id);
    }

    public async Task<InventoryItem?> GetByVehicleIdAsync(Guid vehicleId)
    {
        return await _context.InventoryItems.FirstOrDefaultAsync(i => i.VehicleId == vehicleId);
    }

    public async Task<List<InventoryItem>> GetByDealerIdAsync(Guid dealerId)
    {
        return await _context.InventoryItems
            .Where(i => i.DealerId == dealerId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<InventoryItem>> GetByDealerIdAndStatusAsync(Guid dealerId, InventoryStatus status)
    {
        return await _context.InventoryItems
            .Where(i => i.DealerId == dealerId && i.Status == status)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<(List<InventoryItem> Items, int TotalCount)> GetPagedByDealerIdAsync(
        Guid dealerId,
        int page,
        int pageSize,
        InventoryStatus? status = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDescending = false)
    {
        var query = _context.InventoryItems.Where(i => i.DealerId == dealerId);

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(i =>
                (i.VIN != null && i.VIN.Contains(searchTerm)) ||
                (i.Location != null && i.Location.Contains(searchTerm)) ||
                (i.InternalNotes != null && i.InternalNotes.Contains(searchTerm)) ||
                i.StockNumber.ToString()!.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "price" => sortDescending ? query.OrderByDescending(i => i.ListPrice) : query.OrderBy(i => i.ListPrice),
            "days" => sortDescending ? query.OrderByDescending(i => i.DaysOnMarket) : query.OrderBy(i => i.DaysOnMarket),
            "views" => sortDescending ? query.OrderByDescending(i => i.ViewCount) : query.OrderBy(i => i.ViewCount),
            _ => sortDescending ? query.OrderByDescending(i => i.CreatedAt) : query.OrderBy(i => i.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<InventoryItem> CreateAsync(InventoryItem item)
    {
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<InventoryItem> UpdateAsync(InventoryItem item)
    {
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await GetByIdAsync(id);
        if (item != null)
        {
            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetActiveCountByDealerIdAsync(Guid dealerId)
    {
        return await _context.InventoryItems
            .CountAsync(i => i.DealerId == dealerId && i.Status == InventoryStatus.Active);
    }

    public async Task<int> CountByDealerIdAndStatusAsync(Guid dealerId, InventoryStatus status)
    {
        return await _context.InventoryItems
            .CountAsync(i => i.DealerId == dealerId && i.Status == status);
    }

    public async Task<List<InventoryItem>> GetFeaturedByDealerIdAsync(Guid dealerId)
    {
        return await _context.InventoryItems
            .Where(i => i.DealerId == dealerId && i.IsFeatured)
            .OrderByDescending(i => i.Priority)
            .ThenByDescending(i => i.FeaturedUntil)
            .ToListAsync();
    }

    public async Task<List<InventoryItem>> GetHotItemsByDealerIdAsync(Guid dealerId)
    {
        var items = await _context.InventoryItems
            .Where(i => i.DealerId == dealerId && i.Status == InventoryStatus.Active)
            .ToListAsync();
        
        return items.Where(i => i.IsHot).ToList();
    }

    public async Task<List<InventoryItem>> GetOverdueByDealerIdAsync(Guid dealerId)
    {
        var items = await _context.InventoryItems
            .Where(i => i.DealerId == dealerId && i.Status == InventoryStatus.Active)
            .ToListAsync();
        
        return items.Where(i => i.IsOverdue).ToList();
    }

    public async Task BulkUpdateStatusAsync(List<Guid> ids, InventoryStatus status)
    {
        await _context.InventoryItems
            .Where(i => ids.Contains(i.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.Status, status));
    }

    public async Task BulkDeleteAsync(List<Guid> ids)
    {
        await _context.InventoryItems
            .Where(i => ids.Contains(i.Id))
            .ExecuteDeleteAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.InventoryItems.AnyAsync(i => i.Id == id);
    }
}
