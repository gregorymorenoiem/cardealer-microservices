using Microsoft.Extensions.Logging;
using SchedulerService.Application.Interfaces;
using System.Text.Json;

namespace SchedulerService.Infrastructure.Jobs;

/// <summary>
/// Job that generates daily statistics report for all job executions
/// </summary>
public class DailyStatsReportJob : IScheduledJob
{
    private readonly ILogger<DailyStatsReportJob> _logger;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly IJobRepository _jobRepository;

    public DailyStatsReportJob(
        ILogger<DailyStatsReportJob> logger,
        IJobExecutionRepository executionRepository,
        IJobRepository jobRepository)
    {
        _logger = logger;
        _executionRepository = executionRepository;
        _jobRepository = jobRepository;
    }

    public async Task ExecuteAsync(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("Starting daily statistics report generation...");

        try
        {
            var reportDate = parameters.TryGetValue("Date", out var dateStr)
                ? DateTime.Parse(dateStr)
                : DateTime.UtcNow.AddDays(-1);

            var startDate = reportDate.Date;
            var endDate = startDate.AddDays(1).AddTicks(-1);

            _logger.LogInformation("Generating report for date: {ReportDate}", reportDate.Date);

            // Get execution statistics from repository
            var stats = await _executionRepository.GetStatisticsAsync(startDate, endDate);

            // Get active jobs count
            var activeJobs = await _jobRepository.GetActiveJobsAsync();

            // Calculate success rate
            var successRate = stats.TotalExecutions > 0
                ? (double)stats.SuccessCount / stats.TotalExecutions * 100
                : 0;

            // Build report
            var report = new DailyStatsReport
            {
                ReportDate = reportDate.Date,
                GeneratedAt = DateTime.UtcNow,
                TotalJobs = activeJobs.Count,
                TotalExecutions = stats.TotalExecutions,
                SuccessfulExecutions = stats.SuccessCount,
                FailedExecutions = stats.FailedCount,
                CancelledExecutions = stats.CancelledCount,
                SuccessRate = Math.Round(successRate, 2),
                AverageDurationMs = Math.Round(stats.AverageDurationMs, 2),
                ExecutionsByJob = stats.ExecutionsByJob
            };

            // Log the report (in production, this could be sent via email or stored in a report table)
            var reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });

            _logger.LogInformation(
                "Daily Report for {Date}: Total={Total}, Success={Success} ({SuccessRate}%), Failed={Failed}, AvgDuration={AvgMs}ms",
                reportDate.Date.ToString("yyyy-MM-dd"),
                stats.TotalExecutions,
                stats.SuccessCount,
                report.SuccessRate,
                stats.FailedCount,
                report.AverageDurationMs);

            _logger.LogDebug("Full report: {Report}", reportJson);

            _logger.LogInformation("Daily report generated successfully for {ReportDate}", reportDate.Date);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during daily report job execution");
            throw;
        }
    }
}

/// <summary>
/// Daily statistics report data structure
/// </summary>
public record DailyStatsReport
{
    public DateTime ReportDate { get; init; }
    public DateTime GeneratedAt { get; init; }
    public int TotalJobs { get; init; }
    public int TotalExecutions { get; init; }
    public int SuccessfulExecutions { get; init; }
    public int FailedExecutions { get; init; }
    public int CancelledExecutions { get; init; }
    public double SuccessRate { get; init; }
    public double AverageDurationMs { get; init; }
    public Dictionary<string, int> ExecutionsByJob { get; init; } = new();
}
