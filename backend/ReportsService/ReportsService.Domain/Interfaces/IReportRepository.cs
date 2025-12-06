using ReportsService.Domain.Entities;

namespace ReportsService.Domain.Interfaces;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Report>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Report>> GetByTypeAsync(ReportType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Report>> GetByStatusAsync(ReportStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Report>> GetReadyAsync(CancellationToken cancellationToken = default);
    Task<Report> AddAsync(Report report, CancellationToken cancellationToken = default);
    Task UpdateAsync(Report report, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
