using AdminService.Domain.Entities;

namespace AdminService.Domain.Interfaces;

public interface IReportRepository
{
    Task<(IReadOnlyList<Report> Items, int Total)> GetReportsAsync(
        string? type = null,
        string? status = null,
        string? priority = null,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ReportStats> GetStatsAsync(CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(Guid id, string status, string? resolution = null, CancellationToken cancellationToken = default);
}

public record ReportStats(
    int Total,
    int Pending,
    int Investigating,
    int Resolved,
    int Dismissed,
    int HighPriority);
