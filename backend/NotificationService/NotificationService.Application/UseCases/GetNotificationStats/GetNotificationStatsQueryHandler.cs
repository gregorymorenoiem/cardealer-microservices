using MediatR;
using NotificationService.Domain.Interfaces.Repositories;
using NotificationService.Domain.Enums;
using ErrorService.Shared.Exceptions;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.UseCases.GetNotificationStats;

public class GetNotificationStatsQueryHandler
    : IRequestHandler<GetNotificationStatsQuery, GetNotificationStatsResponse>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationStatsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<GetNotificationStatsResponse> Handle(
        GetNotificationStatsQuery query,
        CancellationToken cancellationToken)
    {
        var request = query.Request;

        try
        {
            // Obtener estadísticas del repositorio
            var totalNotifications = await _notificationRepository.GetTotalCountAsync();

            var last24Hours = await _notificationRepository.GetByDateRangeAsync(
                DateTime.UtcNow.AddHours(-24),
                DateTime.UtcNow
            );
            var sentLast24Hours = last24Hours.Count(n => n.Status == NotificationStatus.Sent);

            var last7Days = await _notificationRepository.GetByDateRangeAsync(
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow
            );
            var sentLast7Days = last7Days.Count(n => n.Status == NotificationStatus.Sent);

            // Obtener distribución por tipo y estado
            var allNotifications = await _notificationRepository.GetRecentNotificationsAsync(1000);

            var byType = allNotifications
                .GroupBy(n => n.Type)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            var byStatus = allNotifications
                .GroupBy(n => n.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            return new GetNotificationStatsResponse(
                totalNotifications,
                sentLast24Hours,
                sentLast7Days,
                byType,
                byStatus
            );
        }
        catch (Exception)
        {
            throw new ServiceUnavailableException("An error occurred while retrieving notification statistics");
        }
    }
}