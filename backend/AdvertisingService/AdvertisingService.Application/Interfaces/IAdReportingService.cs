using AdvertisingService.Application.DTOs;

namespace AdvertisingService.Application.Interfaces;

/// <summary>
/// Abstraction for advertising reporting service.
/// Implemented in Infrastructure layer by AdReportingService.
/// </summary>
public interface IAdReportingService
{
    Task<CampaignReportData> GetCampaignReportAsync(Guid campaignId, DateTime since, CancellationToken ct = default);
    Task<OwnerReportData> GetOwnerReportAsync(Guid ownerId, string ownerType, DateTime since, CancellationToken ct = default);
    Task<PlatformReportData> GetPlatformReportAsync(DateTime since, CancellationToken ct = default);
}
