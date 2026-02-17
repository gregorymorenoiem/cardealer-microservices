using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdminService.Infrastructure.Persistence;

/// <summary>
/// Implementation of IReportRepository
/// Returns empty data - will be connected to real data sources later
/// Reports will come from user submissions via the platform
/// </summary>
public class EfReportRepository : IReportRepository
{
    private readonly ILogger<EfReportRepository> _logger;

    public EfReportRepository(ILogger<EfReportRepository> logger)
    {
        _logger = logger;
    }

    public async Task<(IReadOnlyList<Report> Items, int Total)> GetReportsAsync(
        string? type = null,
        string? status = null,
        string? priority = null,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting reports with filters: Type={Type}, Status={Status}, Priority={Priority}, Page={Page}",
            type, status, priority, page);

        // TODO: Connect to real database when reports feature is fully implemented
        await Task.CompletedTask;

        return (new List<Report>().AsReadOnly(), 0);
    }

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting report by id: {Id}", id);

        // TODO: Connect to real database
        await Task.CompletedTask;

        return null;
    }

    public async Task<ReportStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting report stats");

        // TODO: Connect to real database for aggregated stats
        await Task.CompletedTask;

        return new ReportStats(
            Total: 0,
            Pending: 0,
            Investigating: 0,
            Resolved: 0,
            Dismissed: 0,
            HighPriority: 0);
    }

    public async Task UpdateStatusAsync(Guid id, string status, string? resolution = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating report {Id} status to {Status}", id, status);

        // TODO: Connect to real database
        await Task.CompletedTask;
    }
}
