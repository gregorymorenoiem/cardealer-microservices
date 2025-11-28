using MediatR;
using NotificationService.Domain.Interfaces.Repositories;
using NotificationService.Domain.Enums;
using ErrorService.Shared.Exceptions;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.UseCases.GetNotifications;

public class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, GetNotificationsResponse>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<GetNotificationsResponse> Handle(
        GetNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        var request = query.Request;

        try
        {
            // Obtener notificaciones paginadas
            var notifications = await _notificationRepository.GetNotificationsWithPaginationAsync(
                request.Page,
                request.PageSize
            );

            // Aplicar filtros
            var filteredNotifications = notifications
                .Where(n => string.IsNullOrEmpty(request.Recipient) || n.Recipient.Contains(request.Recipient))
                .Where(n => string.IsNullOrEmpty(request.Type) || n.Type.ToString() == request.Type)
                .Where(n => string.IsNullOrEmpty(request.Status) || n.Status.ToString() == request.Status)
                .Where(n => !request.From.HasValue || n.CreatedAt >= request.From.Value)
                .Where(n => !request.To.HasValue || n.CreatedAt <= request.To.Value)
                .ToList();

            var notificationDtos = filteredNotifications.Select(n => new NotificationItemDto(
                n.Id,
                n.Type.ToString(),
                n.Recipient,
                n.Subject,
                n.Status.ToString(),
                n.CreatedAt,
                n.SentAt,
                n.ErrorMessage
            )).ToList();

            return new GetNotificationsResponse(
                notificationDtos,
                notificationDtos.Count,
                request.Page,
                request.PageSize
            );
        }
        catch (Exception)
        {
            throw new ServiceUnavailableException("An error occurred while retrieving notifications");
        }
    }
}