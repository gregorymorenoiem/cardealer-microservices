using MediatR;
using TracingService.Application.Interfaces;
using TracingService.Application.Queries;
using TracingService.Domain.Entities;

namespace TracingService.Application.Handlers;

/// <summary>
/// Handler for GetTraceByIdQuery
/// </summary>
public class GetTraceByIdQueryHandler : IRequestHandler<GetTraceByIdQuery, Trace?>
{
    private readonly ITraceQueryService _traceQueryService;

    public GetTraceByIdQueryHandler(ITraceQueryService traceQueryService)
    {
        _traceQueryService = traceQueryService;
    }

    public async Task<Trace?> Handle(GetTraceByIdQuery request, CancellationToken cancellationToken)
    {
        return await _traceQueryService.GetTraceByIdAsync(request.TraceId, cancellationToken);
    }
}

/// <summary>
/// Handler for SearchTracesQuery
/// </summary>
public class SearchTracesQueryHandler : IRequestHandler<SearchTracesQuery, List<Trace>>
{
    private readonly ITraceQueryService _traceQueryService;

    public SearchTracesQueryHandler(ITraceQueryService traceQueryService)
    {
        _traceQueryService = traceQueryService;
    }

    public async Task<List<Trace>> Handle(SearchTracesQuery request, CancellationToken cancellationToken)
    {
        return await _traceQueryService.SearchTracesAsync(
            request.ServiceName,
            request.OperationName,
            request.StartTime,
            request.EndTime,
            request.MinDurationMs,
            request.MaxDurationMs,
            request.HasError,
            request.Limit,
            cancellationToken);
    }
}

/// <summary>
/// Handler for GetSpansByTraceIdQuery
/// </summary>
public class GetSpansByTraceIdQueryHandler : IRequestHandler<GetSpansByTraceIdQuery, List<Span>>
{
    private readonly ITraceQueryService _traceQueryService;

    public GetSpansByTraceIdQueryHandler(ITraceQueryService traceQueryService)
    {
        _traceQueryService = traceQueryService;
    }

    public async Task<List<Span>> Handle(GetSpansByTraceIdQuery request, CancellationToken cancellationToken)
    {
        return await _traceQueryService.GetSpansByTraceIdAsync(request.TraceId, cancellationToken);
    }
}

/// <summary>
/// Handler for GetTraceStatisticsQuery
/// </summary>
public class GetTraceStatisticsQueryHandler : IRequestHandler<GetTraceStatisticsQuery, TraceStatistics>
{
    private readonly ITraceQueryService _traceQueryService;

    public GetTraceStatisticsQueryHandler(ITraceQueryService traceQueryService)
    {
        _traceQueryService = traceQueryService;
    }

    public async Task<TraceStatistics> Handle(GetTraceStatisticsQuery request, CancellationToken cancellationToken)
    {
        return await _traceQueryService.GetTraceStatisticsAsync(
            request.StartTime,
            request.EndTime,
            request.ServiceName,
            cancellationToken);
    }
}

/// <summary>
/// Handler for GetServicesQuery
/// </summary>
public class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, List<string>>
{
    private readonly ITraceQueryService _traceQueryService;

    public GetServicesQueryHandler(ITraceQueryService traceQueryService)
    {
        _traceQueryService = traceQueryService;
    }

    public async Task<List<string>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        return await _traceQueryService.GetServicesAsync(cancellationToken);
    }
}

/// <summary>
/// Handler for GetOperationsQuery
/// </summary>
public class GetOperationsQueryHandler : IRequestHandler<GetOperationsQuery, List<string>>
{
    private readonly ITraceQueryService _traceQueryService;

    public GetOperationsQueryHandler(ITraceQueryService traceQueryService)
    {
        _traceQueryService = traceQueryService;
    }

    public async Task<List<string>> Handle(GetOperationsQuery request, CancellationToken cancellationToken)
    {
        return await _traceQueryService.GetOperationsAsync(request.ServiceName, cancellationToken);
    }
}
