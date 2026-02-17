using MediatR;
using TracingService.Domain.Entities;

namespace TracingService.Application.Queries;

/// <summary>
/// Query to get a trace by its ID
/// </summary>
public class GetTraceByIdQuery : IRequest<Trace?>
{
    public string TraceId { get; set; } = string.Empty;
}

/// <summary>
/// Query to search for traces
/// </summary>
public class SearchTracesQuery : IRequest<List<Trace>>
{
    public string? ServiceName { get; set; }
    public string? OperationName { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? MinDurationMs { get; set; }
    public int? MaxDurationMs { get; set; }
    public bool? HasError { get; set; }
    public int Limit { get; set; } = 100;
}

/// <summary>
/// Query to get spans by trace ID
/// </summary>
public class GetSpansByTraceIdQuery : IRequest<List<Span>>
{
    public string TraceId { get; set; } = string.Empty;
}

/// <summary>
/// Query to get trace statistics
/// </summary>
public class GetTraceStatisticsQuery : IRequest<TraceStatistics>
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? ServiceName { get; set; }
}

/// <summary>
/// Query to get list of services
/// </summary>
public class GetServicesQuery : IRequest<List<string>>
{
}

/// <summary>
/// Query to get list of operations for a service
/// </summary>
public class GetOperationsQuery : IRequest<List<string>>
{
    public string ServiceName { get; set; } = string.Empty;
}
