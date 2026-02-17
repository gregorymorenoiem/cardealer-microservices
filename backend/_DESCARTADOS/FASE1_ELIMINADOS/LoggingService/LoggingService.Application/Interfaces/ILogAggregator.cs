using LoggingService.Domain;

namespace LoggingService.Application.Interfaces;

public interface ILogAggregator
{
    Task<IEnumerable<LogEntry>> QueryLogsAsync(LogFilter filter, CancellationToken cancellationToken = default);
    Task<LogStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<LogEntry?> GetLogByIdAsync(string id, CancellationToken cancellationToken = default);
}
