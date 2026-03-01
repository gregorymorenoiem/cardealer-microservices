using MediatR;
using Microsoft.Extensions.Logging;
using AdminService.Domain.Interfaces;
using AdminService.Application.Interfaces;

namespace AdminService.Application.UseCases.Dashboard;

/// <summary>
/// Handler for getting dashboard statistics — aggregates from VehiclesSaleService,
/// DealerManagementService, and UserService concurrently.
/// </summary>
public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsResponse>
{
    private readonly IStatisticsRepository _statisticsRepository;
    private readonly IModerationRepository _moderationRepository;
    private readonly IDealerService _dealerService;
    private readonly IPlatformUserService _userService;
    private readonly ILogger<GetDashboardStatsQueryHandler> _logger;

    public GetDashboardStatsQueryHandler(
        IStatisticsRepository statisticsRepository,
        IModerationRepository moderationRepository,
        IDealerService dealerService,
        IPlatformUserService userService,
        ILogger<GetDashboardStatsQueryHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _moderationRepository = moderationRepository;
        _dealerService = dealerService;
        _userService = userService;
        _logger = logger;
    }

    public async Task<DashboardStatsResponse> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching dashboard statistics");

        // Run all service calls in parallel for performance
        var vehicleStatsTask = _statisticsRepository.GetPlatformStatsAsync();
        var dealerStatsTask = _dealerService.GetDealerStatsAsync(cancellationToken);
        var userStatsTask = _userService.GetUserStatsAsync(cancellationToken);
        var pendingReportsTask = _moderationRepository.GetPendingReportsAsync(100);

        await Task.WhenAll(vehicleStatsTask, dealerStatsTask, userStatsTask, pendingReportsTask);

        var platformStats = vehicleStatsTask.Result;
        var dealerStats = dealerStatsTask.Result;
        var userStats = userStatsTask.Result;
        var pendingReports = pendingReportsTask.Result;

        return new DashboardStatsResponse
        {
            TotalUsers = userStats.Total,
            TotalVehicles = platformStats.TotalListings,
            ActiveVehicles = platformStats.TotalListings - platformStats.PendingListings,
            TotalDealers = dealerStats.Total,
            ActiveDealers = dealerStats.Active,
            PendingApprovals = dealerStats.Pending,
            PendingVerifications = platformStats.PendingListings,
            TotalReports = pendingReports.Count(),
            OpenSupportTickets = platformStats.OpenTickets,
            Mrr = dealerStats.TotalMrr,
            MrrChange = 0,
            UsersChange = 0,
            VehiclesChange = 0,
            DealersChange = 0
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
    private readonly IDealerService _dealerService;
    private readonly ILogger<GetDashboardPendingQueryHandler> _logger;

    public GetDashboardPendingQueryHandler(
        IStatisticsRepository statisticsRepository,
        IModerationRepository moderationRepository,
        IDealerService dealerService,
        ILogger<GetDashboardPendingQueryHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _moderationRepository = moderationRepository;
        _dealerService = dealerService;
        _logger = logger;
    }

    public async Task<List<DashboardPendingActionResponse>> Handle(GetDashboardPendingQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching dashboard pending actions");

        var vehicleStatsTask = _statisticsRepository.GetPlatformStatsAsync();
        var dealerStatsTask = _dealerService.GetDealerStatsAsync(cancellationToken);
        var pendingReportsTask = _moderationRepository.GetPendingReportsAsync(100);

        await Task.WhenAll(vehicleStatsTask, dealerStatsTask, pendingReportsTask);

        var platformStats = vehicleStatsTask.Result;
        var dealerStats = dealerStatsTask.Result;
        var pendingReports = pendingReportsTask.Result;

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
        if (dealerStats.Pending > 0)
        {
            pendingActions.Add(new DashboardPendingActionResponse
            {
                Type = "verification",
                Title = "Dealers pendientes de verificación",
                Count = dealerStats.Pending,
                Priority = dealerStats.Pending > 5 ? "high" : "medium",
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
