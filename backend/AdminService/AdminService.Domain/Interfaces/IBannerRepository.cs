using AdminService.Domain.Entities;

namespace AdminService.Domain.Interfaces;

public interface IBannerRepository
{
    Task<IEnumerable<BannerEntity>> GetAllAsync();
    Task<IEnumerable<BannerEntity>> GetByPlacementAsync(string placement, bool activeOnly = true);
    Task<BannerEntity?> GetByIdAsync(string id);
    Task<BannerEntity> CreateAsync(BannerEntity banner);
    Task<BannerEntity?> UpdateAsync(string id, BannerEntity banner);
    Task<bool> DeleteAsync(string id);
    Task RecordViewAsync(string id);
    Task RecordClickAsync(string id);
}
