using ReportsService.Domain.Entities;

namespace ReportsService.Domain.Interfaces;

public interface IContentReportRepository
{
    Task<ContentReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ContentReport> Items, int Total)> GetPaginatedAsync(
        ContentReportType? type = null,
        ContentReportStatus? status = null,
        ContentReportPriority? priority = null,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<ContentReportStats> GetStatsAsync(CancellationToken cancellationToken = default);

    Task<ContentReport> AddAsync(ContentReport report, CancellationToken cancellationToken = default);

    Task UpdateAsync(ContentReport report, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find existing report for the same target by the same user.
    /// Used to increment report count instead of creating duplicates.
    /// </summary>
    Task<ContentReport?> FindByTargetAndReporterAsync(
        string targetId, Guid reportedById,
        CancellationToken cancellationToken = default);
}

public record ContentReportStats(
    int Total,
    int Pending,
    int Investigating,
    int Resolved,
    int Dismissed,
    int HighPriority);
