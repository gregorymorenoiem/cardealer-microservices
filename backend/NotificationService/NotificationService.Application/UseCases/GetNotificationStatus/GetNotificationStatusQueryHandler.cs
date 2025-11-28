using MediatR;
using NotificationService.Domain.Interfaces.Repositories;
using ErrorService.Shared.Exceptions;
using NotificationService.Application.DTOs; // ✅ Agregar este using

namespace NotificationService.Application.UseCases.GetNotificationStatus;

public class GetNotificationStatusQueryHandler
    : IRequestHandler<GetNotificationStatusQuery, GetNotificationStatusResponse>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationStatusQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<GetNotificationStatusResponse> Handle(
        GetNotificationStatusQuery query,
        CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(query.NotificationId);

        if (notification == null)
            throw new NotFoundException($"Notification with ID {query.NotificationId} not found");

        return new GetNotificationStatusResponse(
            notification.Id,
            notification.Status.ToString(),
            notification.SentAt,
            notification.ErrorMessage
        );
    }
}