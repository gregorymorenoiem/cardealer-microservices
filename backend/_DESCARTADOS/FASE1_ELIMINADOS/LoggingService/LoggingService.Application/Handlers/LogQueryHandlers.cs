using LoggingService.Application.Interfaces;
using LoggingService.Application.Queries;
using LoggingService.Domain;
using MediatR;

namespace LoggingService.Application.Handlers;

public class GetLogsQueryHandler : IRequestHandler<GetLogsQuery, IEnumerable<LogEntry>>
{
    private readonly ILogAggregator _logAggregator;

    public GetLogsQueryHandler(ILogAggregator logAggregator)
    {
        _logAggregator = logAggregator;
    }

    public async Task<IEnumerable<LogEntry>> Handle(GetLogsQuery request, CancellationToken cancellationToken)
    {
        if (!request.Filter.IsValid())
        {
            throw new ArgumentException("Invalid filter parameters");
        }

        return await _logAggregator.QueryLogsAsync(request.Filter, cancellationToken);
    }
}

public class GetLogByIdQueryHandler : IRequestHandler<GetLogByIdQuery, LogEntry?>
{
    private readonly ILogAggregator _logAggregator;

    public GetLogByIdQueryHandler(ILogAggregator logAggregator)
    {
        _logAggregator = logAggregator;
    }

    public async Task<LogEntry?> Handle(GetLogByIdQuery request, CancellationToken cancellationToken)
    {
        return await _logAggregator.GetLogByIdAsync(request.Id, cancellationToken);
    }
}

public class GetLogStatisticsQueryHandler : IRequestHandler<GetLogStatisticsQuery, LogStatistics>
{
    private readonly ILogAggregator _logAggregator;

    public GetLogStatisticsQueryHandler(ILogAggregator logAggregator)
    {
        _logAggregator = logAggregator;
    }

    public async Task<LogStatistics> Handle(GetLogStatisticsQuery request, CancellationToken cancellationToken)
    {
        return await _logAggregator.GetStatisticsAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}
