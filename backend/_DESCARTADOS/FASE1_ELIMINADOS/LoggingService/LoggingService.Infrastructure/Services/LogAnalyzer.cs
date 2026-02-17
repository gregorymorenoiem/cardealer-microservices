using LoggingService.Application.Interfaces;
using LoggingService.Domain;
using Microsoft.Extensions.Logging;

namespace LoggingService.Infrastructure.Services;

/// <summary>
/// Implementation of log analysis service
/// </summary>
public class LogAnalyzer : ILogAnalyzer
{
    private readonly ILogAggregator _logAggregator;
    private readonly ILogger<LogAnalyzer> _logger;

    public LogAnalyzer(ILogAggregator logAggregator, ILogger<LogAnalyzer> logger)
    {
        _logAggregator = logAggregator;
        _logger = logger;
    }

    public async Task<LogAnalysis> AnalyzeLogsAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting log analysis from {StartTime} to {EndTime}", startTime, endTime);

        var filter = new LogFilter
        {
            StartDate = startTime,
            EndDate = endTime
        };

        var logs = (await _logAggregator.QueryLogsAsync(filter, cancellationToken)).ToList();
        var statistics = await _logAggregator.GetStatisticsAsync(startTime, endTime, cancellationToken);

        var analysis = new LogAnalysis
        {
            StartTime = startTime,
            EndTime = endTime,
            Statistics = statistics
        };

        // Detect patterns
        analysis.DetectedPatterns = await DetectPatternsAsync(logs, cancellationToken);

        // Detect anomalies
        analysis.DetectedAnomalies = await DetectAnomaliesAsync(startTime, endTime, cancellationToken);

        // Get service health
        analysis.ServiceHealth = await GetServiceHealthAsync(startTime, endTime, cancellationToken);

        // Generate recommendations
        analysis.Recommendations = await GenerateRecommendationsAsync(analysis, cancellationToken);

        // Build summary
        analysis.Summary = BuildAnalysisSummary(analysis);

        _logger.LogInformation("Log analysis completed. Found {PatternCount} patterns and {AnomalyCount} anomalies",
            analysis.DetectedPatterns.Count, analysis.DetectedAnomalies.Count);

        return analysis;
    }

    public async Task<List<LogPattern>> DetectPatternsAsync(IEnumerable<LogEntry> logs, CancellationToken cancellationToken = default)
    {
        var patterns = new List<LogPattern>();
        var logList = logs.ToList();

        // 1. Detect recurring errors
        patterns.AddRange(DetectRecurringErrors(logList));

        // 2. Detect error spikes
        patterns.AddRange(DetectErrorSpikes(logList));

        // 3. Detect dependency failures
        patterns.AddRange(DetectDependencyFailures(logList));

        // 4. Detect configuration issues
        patterns.AddRange(DetectConfigurationIssues(logList));

        await Task.CompletedTask;
        return patterns;
    }

    public async Task<List<LogAnomaly>> DetectAnomaliesAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        var anomalies = new List<LogAnomaly>();

        var filter = new LogFilter
        {
            StartDate = startTime,
            EndDate = endTime
        };

        var logs = (await _logAggregator.QueryLogsAsync(filter, cancellationToken)).ToList();
        var statistics = await _logAggregator.GetStatisticsAsync(startTime, endTime, cancellationToken);

        // 1. Error rate spike detection
        if (statistics.GetErrorRate() > 10) // More than 10% errors
        {
            anomalies.Add(new LogAnomaly
            {
                Title = "High Error Rate Detected",
                Description = $"Error rate is {statistics.GetErrorRate():F2}%, which exceeds the normal threshold of 10%",
                Type = AnomalyType.ErrorRateSpike,
                Severity = statistics.GetErrorRate() > 25 ? AnomalySeverity.Critical : AnomalySeverity.High,
                ServiceName = "System",
                Confidence = 95,
                Metrics = new Dictionary<string, object>
                {
                    ["ErrorRate"] = statistics.GetErrorRate(),
                    ["ErrorCount"] = statistics.ErrorCount,
                    ["TotalLogs"] = statistics.TotalLogs
                }
            });
        }

        // 2. Service-specific anomalies
        foreach (var service in statistics.LogsByService)
        {
            var serviceFilter = new LogFilter
            {
                StartDate = startTime,
                EndDate = endTime,
                ServiceName = service.Key
            };

            var serviceLogs = (await _logAggregator.QueryLogsAsync(serviceFilter, cancellationToken)).ToList();
            var errorLogs = serviceLogs.Where(l => l.IsError()).ToList();

            if (errorLogs.Count > 100) // Service has many errors
            {
                anomalies.Add(new LogAnomaly
                {
                    Title = $"High Error Count in {service.Key}",
                    Description = $"Service {service.Key} has generated {errorLogs.Count} errors in the analysis period",
                    Type = AnomalyType.ErrorRateSpike,
                    Severity = errorLogs.Count > 500 ? AnomalySeverity.Critical : AnomalySeverity.High,
                    ServiceName = service.Key,
                    Confidence = 90,
                    RelatedLogIds = errorLogs.Take(10).Select(l => l.Id).ToList(),
                    Metrics = new Dictionary<string, object>
                    {
                        ["ErrorCount"] = errorLogs.Count,
                        ["ServiceLogCount"] = serviceLogs.Count
                    }
                });
            }
        }

        // 3. Unusual error patterns
        var criticalLogs = logs.Where(l => l.IsCritical()).ToList();
        if (criticalLogs.Any())
        {
            anomalies.Add(new LogAnomaly
            {
                Title = "Critical Errors Detected",
                Description = $"Found {criticalLogs.Count} critical errors that require immediate attention",
                Type = AnomalyType.UnusualErrorPattern,
                Severity = AnomalySeverity.Critical,
                ServiceName = criticalLogs.First().ServiceName,
                Confidence = 100,
                RelatedLogIds = criticalLogs.Select(l => l.Id).ToList(),
                Metrics = new Dictionary<string, object>
                {
                    ["CriticalCount"] = criticalLogs.Count
                }
            });
        }

        return anomalies;
    }

    public async Task<Dictionary<string, ServiceHealthMetrics>> GetServiceHealthAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        var statistics = await _logAggregator.GetStatisticsAsync(startTime, endTime, cancellationToken);
        var healthMetrics = new Dictionary<string, ServiceHealthMetrics>();

        foreach (var service in statistics.LogsByService)
        {
            var serviceFilter = new LogFilter
            {
                StartDate = startTime,
                EndDate = endTime,
                ServiceName = service.Key
            };

            var serviceLogs = (await _logAggregator.QueryLogsAsync(serviceFilter, cancellationToken)).ToList();
            var errorLogs = serviceLogs.Where(l => l.IsError()).ToList();
            var errorRate = serviceLogs.Count > 0 ? (double)errorLogs.Count / serviceLogs.Count * 100 : 0;

            var health = new ServiceHealthMetrics
            {
                ServiceName = service.Key,
                RequestCount = serviceLogs.Count,
                ErrorCount = errorLogs.Count,
                ErrorRate = errorRate,
                Status = DetermineHealthStatus(errorRate, errorLogs.Count),
                LastHealthy = serviceLogs.Any(l => !l.IsError()) ? serviceLogs.Where(l => !l.IsError()).Max(l => l.Timestamp) : DateTime.MinValue,
                AvailabilityPercentage = 100 - errorRate
            };

            if (errorRate > 10)
            {
                health.CurrentIssues.Add($"High error rate: {errorRate:F2}%");
            }

            if (errorLogs.Count > 100)
            {
                health.CurrentIssues.Add($"High error count: {errorLogs.Count}");
            }

            healthMetrics[service.Key] = health;
        }

        return healthMetrics;
    }

    public async Task<List<Recommendation>> GenerateRecommendationsAsync(LogAnalysis analysis, CancellationToken cancellationToken = default)
    {
        var recommendations = new List<Recommendation>();

        // 1. High error rate recommendations
        if (analysis.Statistics.GetErrorRate() > 10)
        {
            recommendations.Add(new Recommendation
            {
                Title = "Investigate High Error Rate",
                Description = $"The system error rate is {analysis.Statistics.GetErrorRate():F2}%. Consider investigating the root cause.",
                Type = RecommendationType.Reliability,
                Priority = RecommendationPriority.High,
                ActionItems = new List<string>
                {
                    "Review error logs for common patterns",
                    "Check service dependencies",
                    "Verify configuration settings",
                    "Consider implementing circuit breakers"
                }
            });
        }

        // 2. Service-specific recommendations
        foreach (var service in analysis.ServiceHealth)
        {
            if (service.Value.Status == HealthStatus.Unhealthy || service.Value.Status == HealthStatus.Critical)
            {
                recommendations.Add(new Recommendation
                {
                    Title = $"Address Issues in {service.Key}",
                    Description = $"Service {service.Key} is reporting {service.Value.Status} status with {service.Value.ErrorRate:F2}% error rate",
                    Type = RecommendationType.Reliability,
                    Priority = service.Value.Status == HealthStatus.Critical ? RecommendationPriority.Critical : RecommendationPriority.High,
                    AffectedService = service.Key,
                    ActionItems = new List<string>
                    {
                        $"Review {service.Key} logs",
                        "Check service resources (CPU, Memory, Disk)",
                        "Verify external dependencies",
                        "Consider scaling the service"
                    }
                });
            }
        }

        // 3. Pattern-based recommendations
        foreach (var pattern in analysis.DetectedPatterns.Where(p => p.Type == PatternType.RecurringError))
        {
            recommendations.Add(new Recommendation
            {
                Title = $"Fix Recurring Error: {pattern.Name}",
                Description = $"This error has occurred {pattern.OccurrenceCount} times",
                Type = RecommendationType.Reliability,
                Priority = pattern.OccurrenceCount > 50 ? RecommendationPriority.High : RecommendationPriority.Medium,
                ActionItems = new List<string>
                {
                    "Analyze the error pattern",
                    "Implement proper error handling",
                    "Add monitoring for this specific error",
                    "Consider adding retry logic"
                }
            });
        }

        // 4. Performance recommendations
        if (analysis.Statistics.TotalLogs > 100000)
        {
            recommendations.Add(new Recommendation
            {
                Title = "High Log Volume Detected",
                Description = $"System generated {analysis.Statistics.TotalLogs} logs in the analysis period",
                Type = RecommendationType.Performance,
                Priority = RecommendationPriority.Medium,
                ActionItems = new List<string>
                {
                    "Review log levels (reduce Debug/Trace in production)",
                    "Implement log sampling for high-volume operations",
                    "Consider log aggregation strategies",
                    "Set up log retention policies"
                }
            });
        }

        await Task.CompletedTask;
        return recommendations;
    }

    // Helper methods

    private List<LogPattern> DetectRecurringErrors(List<LogEntry> logs)
    {
        var patterns = new List<LogPattern>();
        var errorGroups = logs.Where(l => l.IsError())
            .GroupBy(l => l.Message)
            .Where(g => g.Count() > 5) // At least 5 occurrences
            .OrderByDescending(g => g.Count());

        foreach (var group in errorGroups)
        {
            var groupLogs = group.ToList();
            patterns.Add(new LogPattern
            {
                Name = $"Recurring Error: {TruncateMessage(group.Key)}",
                Pattern = group.Key,
                Description = $"This error has occurred {group.Count()} times",
                Type = PatternType.RecurringError,
                OccurrenceCount = group.Count(),
                FirstSeen = groupLogs.Min(l => l.Timestamp),
                LastSeen = groupLogs.Max(l => l.Timestamp),
                AffectedServices = groupLogs.Select(l => l.ServiceName).Distinct().ToList()
            });
        }

        return patterns;
    }

    private List<LogPattern> DetectErrorSpikes(List<LogEntry> logs)
    {
        var patterns = new List<LogPattern>();

        // Group by hour and detect spikes
        var hourlyErrors = logs.Where(l => l.IsError())
            .GroupBy(l => new DateTime(l.Timestamp.Year, l.Timestamp.Month, l.Timestamp.Day, l.Timestamp.Hour, 0, 0))
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        if (hourlyErrors.Any())
        {
            var avgErrorsPerHour = hourlyErrors.Average(x => x.Count);
            var spikes = hourlyErrors.Where(x => x.Count > avgErrorsPerHour * 2).ToList();

            foreach (var spike in spikes)
            {
                patterns.Add(new LogPattern
                {
                    Name = $"Error Spike at {spike.Hour:yyyy-MM-dd HH:mm}",
                    Pattern = "Error spike detected",
                    Description = $"Error count spiked to {spike.Count} (avg: {avgErrorsPerHour:F0})",
                    Type = PatternType.ErrorSpike,
                    OccurrenceCount = spike.Count,
                    FirstSeen = spike.Hour,
                    LastSeen = spike.Hour.AddHours(1),
                    Metadata = new Dictionary<string, object>
                    {
                        ["AverageErrorsPerHour"] = avgErrorsPerHour,
                        ["SpikeMultiplier"] = spike.Count / avgErrorsPerHour
                    }
                });
            }
        }

        return patterns;
    }

    private List<LogPattern> DetectDependencyFailures(List<LogEntry> logs)
    {
        var patterns = new List<LogPattern>();
        var dependencyKeywords = new[] { "connection", "timeout", "unavailable", "refused", "unreachable" };

        var dependencyErrors = logs.Where(l => l.IsError() &&
            dependencyKeywords.Any(k => l.Message.Contains(k, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (dependencyErrors.Count > 10)
        {
            patterns.Add(new LogPattern
            {
                Name = "Dependency Failure Pattern",
                Pattern = "Connection/Timeout errors",
                Description = $"Detected {dependencyErrors.Count} dependency-related errors",
                Type = PatternType.DependencyFailure,
                OccurrenceCount = dependencyErrors.Count,
                FirstSeen = dependencyErrors.Min(l => l.Timestamp),
                LastSeen = dependencyErrors.Max(l => l.Timestamp),
                AffectedServices = dependencyErrors.Select(l => l.ServiceName).Distinct().ToList()
            });
        }

        return patterns;
    }

    private List<LogPattern> DetectConfigurationIssues(List<LogEntry> logs)
    {
        var patterns = new List<LogPattern>();
        var configKeywords = new[] { "configuration", "missing", "invalid", "not found", "settings" };

        var configErrors = logs.Where(l => l.IsError() &&
            configKeywords.Any(k => l.Message.Contains(k, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (configErrors.Count > 5)
        {
            patterns.Add(new LogPattern
            {
                Name = "Configuration Issue Pattern",
                Pattern = "Configuration-related errors",
                Description = $"Detected {configErrors.Count} configuration-related errors",
                Type = PatternType.ConfigurationIssue,
                OccurrenceCount = configErrors.Count,
                FirstSeen = configErrors.Min(l => l.Timestamp),
                LastSeen = configErrors.Max(l => l.Timestamp),
                AffectedServices = configErrors.Select(l => l.ServiceName).Distinct().ToList()
            });
        }

        return patterns;
    }

    private AnalysisSummary BuildAnalysisSummary(LogAnalysis analysis)
    {
        return new AnalysisSummary
        {
            TotalLogsAnalyzed = analysis.Statistics.TotalLogs,
            CriticalIssuesFound = analysis.DetectedAnomalies.Count(a => a.Severity == AnomalySeverity.Critical),
            WarningsFound = analysis.Statistics.WarningCount,
            PatternsDetected = analysis.DetectedPatterns.Count,
            AnomaliesDetected = analysis.DetectedAnomalies.Count,
            OverallSystemHealth = CalculateOverallHealth(analysis),
            TopIssues = analysis.DetectedAnomalies
                .OrderByDescending(a => a.Severity)
                .Take(5)
                .Select(a => a.Title)
                .ToList(),
            IssuesByCategory = analysis.DetectedPatterns
                .GroupBy(p => p.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    private double CalculateOverallHealth(LogAnalysis analysis)
    {
        var baseHealth = 100.0;

        // Deduct for error rate
        baseHealth -= analysis.Statistics.GetErrorRate();

        // Deduct for anomalies
        baseHealth -= analysis.DetectedAnomalies.Count(a => a.Severity == AnomalySeverity.Critical) * 10;
        baseHealth -= analysis.DetectedAnomalies.Count(a => a.Severity == AnomalySeverity.High) * 5;

        // Deduct for unhealthy services
        var unhealthyServices = analysis.ServiceHealth.Values.Count(s => s.Status >= HealthStatus.Unhealthy);
        baseHealth -= unhealthyServices * 5;

        return Math.Max(0, Math.Min(100, baseHealth));
    }

    private HealthStatus DetermineHealthStatus(double errorRate, int errorCount)
    {
        if (errorRate > 25 || errorCount > 500) return HealthStatus.Critical;
        if (errorRate > 10 || errorCount > 200) return HealthStatus.Unhealthy;
        if (errorRate > 5 || errorCount > 50) return HealthStatus.Degraded;
        return HealthStatus.Healthy;
    }

    private string TruncateMessage(string message, int maxLength = 80)
    {
        return message.Length <= maxLength ? message : message.Substring(0, maxLength) + "...";
    }
}
