using UserService.Domain.Entities;

namespace UserService.Domain.Interfaces;

/// <summary>
/// Repository for seller conversion tracking records.
/// </summary>
public interface ISellerConversionRepository
{
    Task<SellerConversion?> GetByIdAsync(Guid id);
    Task<SellerConversion?> GetByUserIdAsync(Guid userId);
    Task<SellerConversion?> GetByIdempotencyKeyAsync(string idempotencyKey);
    Task<SellerConversion> CreateAsync(SellerConversion conversion);
    Task<SellerConversion> UpdateAsync(SellerConversion conversion);
    Task<List<SellerConversion>> GetByStatusAsync(SellerConversionStatus status, int page = 1, int pageSize = 20);
    Task<bool> ExistsForUserAsync(Guid userId);
}
