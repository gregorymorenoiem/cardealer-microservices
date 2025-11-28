using MediatR;

namespace NotificationService.Application.UseCases.ProcessNotificationQueue;

public record ProcessNotificationQueueCommand : IRequest;