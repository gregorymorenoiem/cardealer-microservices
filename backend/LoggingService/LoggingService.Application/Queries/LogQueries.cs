using LoggingService.Domain;
using MediatR;

namespace LoggingService.Application.Queries;

public record GetLogsQuery(LogFilter Filter) : IRequest<IEnumerable<LogEntry>>;

public record GetLogByIdQuery(string Id) : IRequest<LogEntry?>;

public record GetLogStatisticsQuery(DateTime? StartDate = null, DateTime? EndDate = null) : IRequest<LogStatistics>;
