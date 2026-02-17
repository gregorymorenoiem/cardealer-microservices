using TracingService.Domain.Entities;

namespace TracingService.Application.Interfaces;

/// <summary>
/// Service for querying traces from Jaeger backend
/// </summary>
public interface ITraceQueryService
{
    /// <summary>
    /// Get a specific trace by ID
    /// </summary>
    Task<Trace?> GetTraceByIdAsync(string traceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for traces matching the given criteria
    /// </summary>
    Task<List<Trace>> SearchTracesAsync(
        string? serviceName = null,
        string? operationName = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int? minDurationMs = null,
        int? maxDurationMs = null,
        bool? hasError = null,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all spans for a specific trace
    /// </summary>
    Task<List<Span>> GetSpansByTraceIdAsync(string traceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get statistics about traces
    /// </summary>
    Task<TraceStatistics> GetTraceStatisticsAsync(
        DateTime? startTime = null,
        DateTime? endTime = null,
        string? serviceName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of services that have reported traces
    /// </summary>
    Task<List<string>> GetServicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of operations for a specific service
    /// </summary>
    Task<List<string>> GetOperationsAsync(string serviceName, CancellationToken cancellationToken = default);
}
