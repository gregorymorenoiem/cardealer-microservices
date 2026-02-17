using BackgroundRemovalService.Domain.Entities;
using BackgroundRemovalService.Domain.Enums;

namespace BackgroundRemovalService.Domain.Interfaces;

/// <summary>
/// Repository for provider usage statistics
/// </summary>
public interface IProviderUsageStatsRepository
{
    Task<ProviderUsageStats?> GetByProviderAndDateAsync(
        BackgroundRemovalProvider provider, 
        DateOnly date, 
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<ProviderUsageStats>> GetByProviderAsync(
        BackgroundRemovalProvider provider, 
        DateOnly fromDate, 
        DateOnly toDate, 
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<ProviderUsageStats>> GetAllByDateRangeAsync(
        DateOnly fromDate, 
        DateOnly toDate, 
        CancellationToken cancellationToken = default);
    
    Task<ProviderUsageStats> CreateAsync(ProviderUsageStats stats, CancellationToken cancellationToken = default);
    
    Task<ProviderUsageStats> UpdateAsync(ProviderUsageStats stats, CancellationToken cancellationToken = default);
    
    Task IncrementStatsAsync(
        BackgroundRemovalProvider provider, 
        DateOnly date, 
        bool success, 
        long processingTimeMs, 
        long bytesProcessed,
        decimal estimatedCost,
        CancellationToken cancellationToken = default);
}
