using AdminService.Domain.Entities;

namespace AdminService.Domain.Interfaces;

/// <summary>
/// Interface para repositorio de moderación
/// </summary>
public interface IModerationRepository
{
    Task<IEnumerable<PendingListingInfo>> GetPendingListingsAsync(int limit);
    Task<IEnumerable<PendingReportInfo>> GetPendingReportsAsync(int limit);
}