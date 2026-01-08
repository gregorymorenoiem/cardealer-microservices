using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryManagementService.Domain.Entities;

namespace InventoryManagementService.Domain.Interfaces;

public interface IInventoryItemRepository
{
    Task<InventoryItem?> GetByIdAsync(Guid id);
    Task<InventoryItem?> GetByVehicleIdAsync(Guid vehicleId);
    Task<List<InventoryItem>> GetByDealerIdAsync(Guid dealerId);
    Task<List<InventoryItem>> GetByDealerIdAndStatusAsync(Guid dealerId, InventoryStatus status);
    Task<(List<InventoryItem> Items, int TotalCount)> GetPagedByDealerIdAsync(
        Guid dealerId,
        int page,
        int pageSize,
        InventoryStatus? status = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDescending = false);
    Task<InventoryItem> CreateAsync(InventoryItem item);
    Task<InventoryItem> UpdateAsync(InventoryItem item);
    Task DeleteAsync(Guid id);
    Task<int> GetActiveCountByDealerIdAsync(Guid dealerId);
    Task<int> CountByDealerIdAndStatusAsync(Guid dealerId, InventoryStatus status);
    Task<List<InventoryItem>> GetFeaturedByDealerIdAsync(Guid dealerId);
    Task<List<InventoryItem>> GetHotItemsByDealerIdAsync(Guid dealerId);
    Task<List<InventoryItem>> GetOverdueByDealerIdAsync(Guid dealerId);
    Task BulkUpdateStatusAsync(List<Guid> ids, InventoryStatus status);
    Task BulkDeleteAsync(List<Guid> ids);
    Task<bool> ExistsAsync(Guid id);
}
