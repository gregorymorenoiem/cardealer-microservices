using AuditService.Application.DTOs;
using AuditService.Domain.Interfaces.Repositories;
using AuditService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuditService.Application.Features.Audit.Queries.GetAuditStats;

public class GetAuditStatsQueryHandler : IRequestHandler<GetAuditStatsQuery, ApiResponse<AuditStatsDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<GetAuditStatsQueryHandler> _logger;

    public GetAuditStatsQueryHandler(
        IAuditLogRepository auditLogRepository,
        ILogger<GetAuditStatsQueryHandler> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<AuditStatsDto>> Handle(GetAuditStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate date range
            if (request.FromDate.HasValue && request.ToDate.HasValue && request.FromDate > request.ToDate)
            {
                return ApiResponse<AuditStatsDto>.Fail("FromDate cannot be greater than ToDate");
            }

            // Get statistics from repository
            var statistics = await _auditLogRepository.GetStatisticsAsync(request.FromDate, request.ToDate, cancellationToken);
            var topActions = await _auditLogRepository.GetTopActionsAsync(10, request.FromDate, request.ToDate, cancellationToken);
            var topUsers = await _auditLogRepository.GetTopUsersAsync(10, request.FromDate, request.ToDate, cancellationToken);

            // Convert to DTO
            var statsDto = new AuditStatsDto
            {
                TotalLogs = statistics.TotalLogs,
                SuccessfulLogs = statistics.SuccessfulLogs,
                FailedLogs = statistics.FailedLogs,
                SystemLogs = statistics.SystemLogs,
                UserLogs = statistics.UserLogs,
                AnonymousLogs = statistics.AnonymousLogs,
                SuccessRate = statistics.SuccessRate,
                FirstLogDate = statistics.FirstLogDate,
                LastLogDate = statistics.LastLogDate,
                LogsBySeverity = statistics.LogsBySeverity,
                LogsByService = statistics.LogsByService,
                LogsByAction = statistics.LogsByAction,
                TopActions = topActions.Select(a => new ActionFrequencyDto
                {
                    Action = a.Action,
                    Count = a.Count,
                    SuccessCount = a.SuccessCount,
                    FailureCount = a.FailureCount
                }).ToList(),
                TopUsers = topUsers.Select(u => new UserActivityDto
                {
                    UserId = u.UserId,
                    UserDisplayName = GetUserDisplayName(u.UserId),
                    TotalActions = u.TotalActions,
                    SuccessfulActions = u.SuccessfulActions,
                    FailedActions = u.FailedActions,
                    FirstActivity = u.FirstActivity,
                    LastActivity = u.LastActivity,
                    MostFrequentActions = u.MostFrequentActions
                }).ToList()
            };

            // Calculate additional metrics
            CalculateAdditionalMetrics(statsDto);

            // Prepare metadata
            var metadata = new Dictionary<string, object>
            {
                ["periodFrom"] = request.FromDate?.ToString("yyyy-MM-dd") ?? "Beginning",
                ["periodTo"] = request.ToDate?.ToString("yyyy-MM-dd") ?? "Now",
                ["generatedAt"] = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(request.ServiceName))
                metadata["filteredByService"] = request.ServiceName;
            if (!string.IsNullOrEmpty(request.UserId))
                metadata["filteredByUserId"] = request.UserId;
            if (!string.IsNullOrEmpty(request.Action))
                metadata["filteredByAction"] = request.Action;

            return ApiResponse<AuditStatsDto>.Ok(statsDto, metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit statistics for period {FromDate} to {ToDate}",
                request.FromDate, request.ToDate);
            return ApiResponse<AuditStatsDto>.Fail("Error retrieving audit statistics");
        }
    }

    private static string GetUserDisplayName(string userId)
    {
        return userId switch
        {
            "system" => "System",
            "anonymous" => "Anonymous",
            _ => userId
        };
    }

    private static void CalculateAdditionalMetrics(AuditStatsDto statsDto)
    {
        // Calculate hourly averages (simplified)
        if (statsDto.FirstLogDate.HasValue && statsDto.LastLogDate.HasValue)
        {
            var totalHours = (statsDto.LastLogDate.Value - statsDto.FirstLogDate.Value).TotalHours;
            if (totalHours > 0)
            {
                statsDto.HourlyAverages["logs_per_hour"] = (int)(statsDto.TotalLogs / totalHours);
            }
        }

        // Calculate error rate trend (simplified)
        statsDto.ErrorRateTrend = statsDto.TotalLogs > 0 ?
            (statsDto.FailedLogs * 100.0) / statsDto.TotalLogs : 0;
    }
}