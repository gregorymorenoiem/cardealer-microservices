using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryManagementService.Domain.Entities;

namespace InventoryManagementService.Domain.Interfaces;

public interface IBulkImportJobRepository
{
    Task<BulkImportJob?> GetByIdAsync(Guid id);
    Task<List<BulkImportJob>> GetByDealerIdAsync(Guid dealerId);
    Task<(List<BulkImportJob> Jobs, int TotalCount)> GetPagedByDealerIdAsync(
        Guid dealerId,
        int page,
        int pageSize,
        ImportJobStatus? status = null);
    Task<BulkImportJob> CreateAsync(BulkImportJob job);
    Task<BulkImportJob> UpdateAsync(BulkImportJob job);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
