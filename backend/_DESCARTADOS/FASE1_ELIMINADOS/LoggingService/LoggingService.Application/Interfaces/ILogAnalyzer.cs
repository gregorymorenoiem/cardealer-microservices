using LoggingService.Domain;

namespace LoggingService.Application.Interfaces;

/// <summary>
/// Service for analyzing log data and detecting patterns/anomalies
/// </summary>
public interface ILogAnalyzer
{
    /// <summary>
    /// Analyze logs within a time range
    /// </summary>
    Task<LogAnalysis> AnalyzeLogsAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Detect patterns in log entries
    /// </summary>
    Task<List<LogPattern>> DetectPatternsAsync(IEnumerable<LogEntry> logs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Detect anomalies in log data
    /// </summary>
    Task<List<LogAnomaly>> DetectAnomaliesAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get service health metrics
    /// </summary>
    Task<Dictionary<string, ServiceHealthMetrics>> GetServiceHealthAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate recommendations based on analysis
    /// </summary>
    Task<List<Recommendation>> GenerateRecommendationsAsync(LogAnalysis analysis, CancellationToken cancellationToken = default);
}
