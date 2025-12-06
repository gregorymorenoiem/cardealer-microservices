using ReportsService.Domain.Entities;

namespace ReportsService.Domain.Interfaces;

public interface IReportScheduleRepository
{
    Task<ReportSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReportSchedule>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ReportSchedule>> GetByReportIdAsync(Guid reportId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReportSchedule>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ReportSchedule>> GetDueAsync(CancellationToken cancellationToken = default);
    Task<ReportSchedule> AddAsync(ReportSchedule schedule, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReportSchedule schedule, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
