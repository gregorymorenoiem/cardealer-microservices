using BillingService.Domain.Entities;

namespace BillingService.Domain.Interfaces;

public interface IAzulTransactionRepository
{
    Task<AzulTransaction?> GetByIdAsync(Guid id);
    Task<AzulTransaction?> GetByOrderNumberAsync(string orderNumber);
    Task<AzulTransaction?> GetByAzulOrderIdAsync(string azulOrderId);
    Task<List<AzulTransaction>> GetByUserIdAsync(Guid userId);
    Task<List<AzulTransaction>> GetApprovedTransactionsAsync(DateTime? from = null, DateTime? to = null);
    Task<AzulTransaction> CreateAsync(AzulTransaction transaction);
    Task<AzulTransaction> UpdateAsync(AzulTransaction transaction);
    Task<bool> ExistsAsync(string orderNumber);
}
