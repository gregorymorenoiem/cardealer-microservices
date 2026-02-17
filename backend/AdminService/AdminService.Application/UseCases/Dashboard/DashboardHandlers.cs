using MediatR;
using Microsoft.Extensions.Logging;
using AdminService.Domain.Interfaces;

namespace AdminService.Application.UseCases.Dashboard;

/// <summary>
/// Handler for getting dashboard statistics
/// </summary>
public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsResponse>
{
    private readonly IStatisticsRepository _statisticsRepository;
    private readonly IModerationRepository _moderationRepository;
    private readonly ILogger<GetDashboardStatsQueryHandler> _logger;

    public GetDashboardStatsQueryHandler(
        IStatisticsRepository statisticsRepository,
        IModerationRepository moderationRepository,
        ILogger<GetDashboardStatsQueryHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _moderationRepository = moderationRepository;
        _logger = logger;
    }

    public async Task<DashboardStatsResponse> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching dashboard statistics");

        var platformStats = await _statisticsRepository.GetPlatformStatsAsync();
        var pendingReports = await _moderationRepository.GetPendingReportsAsync(100);

        // Map to frontend-compatible response
        return new DashboardStatsResponse
        {
            TotalUsers = platformStats.TotalUsers,
            TotalVehicles = platformStats.TotalListings,
            ActiveVehicles = platformStats.TotalListings - platformStats.PendingListings,
            TotalDealers = platformStats.TotalDealers,
            ActiveDealers = platformStats.ActiveDealers,
            PendingApprovals = platformStats.PendingDealers,
            PendingVerifications = platformStats.PendingListings,
            TotalReports = pendingReports.Count(),
            OpenSupportTickets = platformStats.OpenTickets,
            Mrr = platformStats.MonthlyRevenue,
            MrrChange = 0, // TODO: Calculate from historical data
            UsersChange = 0, // TODO: Calculate from historical data
            VehiclesChange = 0, // TODO: Calculate from historical data
            DealersChange = 0 // TODO: Calculate from historical data
        };
    }
}

/// <summary>
/// Handler for getting recent dashboard activity
/// </summary>
public class GetDashboardActivityQueryHandler : IRequestHandler<GetDashboardActivityQuery, List<DashboardActivityResponse>>
{
    private readonly IAdminActionLogRepository _actionLogRepository;
    private readonly ILogger<GetDashboardActivityQueryHandler> _logger;

    public GetDashboardActivityQueryHandler(
        IAdminActionLogRepository actionLogRepository,
        ILogger<GetDashboardActivityQueryHandler> logger)
    {
        _actionLogRepository = actionLogRepository;
        _logger = logger;
    }

    public async Task<List<DashboardActivityResponse>> Handle(GetDashboardActivityQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching dashboard activity with limit: {Limit}", request.Limit);

        var recentActions = await _actionLogRepository.GetRecentAsync(request.Limit);

        return recentActions.Select(a => new DashboardActivityResponse
        {
            Id = a.Id.ToString(),
            Action = a.Action,
            Subject = a.Description ?? a.Action,
            SubjectType = MapTargetTypeToSubjectType(a.TargetType),
            SubjectId = a.TargetId ?? string.Empty,
            Timestamp = a.Timestamp,
            Metadata = string.IsNullOrEmpty(a.Metadata) 
                ? null 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(a.Metadata)
        }).ToList();
    }

    private static string MapTargetTypeToSubjectType(string? targetType)
    {
        return targetType?.ToLowerInvariant() switch
        {
            "user" => "user",
            "dealer" => "dealer",
            "vehicle" or "listing" => "vehicle",
            "payment" or "transaction" => "payment",
            "report" => "report",
            _ => "user"
        };
    }
}

/// <summary>
/// Handler for getting pending actions for dashboard
/// </summary>
public class GetDashboardPendingQueryHandler : IRequestHandler<GetDashboardPendingQuery, List<DashboardPendingActionResponse>>
{
    private readonly IStatisticsRepository _statisticsRepository;
    private readonly IModerationRepository _moderationRepository;
    private readonly ILogger<GetDashboardPendingQueryHandler> _logger;

    public GetDashboardPendingQueryHandler(
        IStatisticsRepository statisticsRepository,
        IModerationRepository moderationRepository,
        ILogger<GetDashboardPendingQueryHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _moderationRepository = moderationRepository;
        _logger = logger;
    }

    public async Task<List<DashboardPendingActionResponse>> Handle(GetDashboardPendingQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching dashboard pending actions");

        var platformStats = await _statisticsRepository.GetPlatformStatsAsync();
        var pendingReports = await _moderationRepository.GetPendingReportsAsync(100);

        var pendingActions = new List<DashboardPendingActionResponse>();

        // Pending vehicle moderation
        if (platformStats.PendingListings > 0)
        {
            pendingActions.Add(new DashboardPendingActionResponse
            {
                Type = "moderation",
                Title = "Vehículos pendientes de aprobación",
                Count = platformStats.PendingListings,
                Priority = platformStats.PendingListings > 10 ? "high" : "medium",
                Href = "/admin/moderation/vehicles"
            });
        }

        // Pending dealer verification
        if (platformStats.PendingDealers > 0)
        {
            pendingActions.Add(new DashboardPendingActionResponse
            {
                Type = "verification",
                Title = "Dealers pendientes de verificación",
                Count = platformStats.PendingDealers,
                Priority = platformStats.PendingDealers > 5 ? "high" : "medium",
                Href = "/admin/dealers/pending"
            });
        }

        // Pending reports
        var reportCount = pendingReports.Count();
        if (reportCount > 0)
        {
            pendingActions.Add(new DashboardPendingActionResponse
            {
                Type = "report",
                Title = "Reportes sin revisar",
                Count = reportCount,
                Priority = reportCount > 10 ? "high" : "medium",
                Href = "/admin/reports"
            });
        }

        // Open support tickets
        if (platformStats.OpenTickets > 0)
        {
            pendingActions.Add(new DashboardPendingActionResponse
            {
                Type = "support",
                Title = "Tickets de soporte abiertos",
                Count = platformStats.OpenTickets,
                Priority = platformStats.OpenTickets > 20 ? "high" : "low",
                Href = "/admin/support"
            });
        }

        return pendingActions;
    }
}
