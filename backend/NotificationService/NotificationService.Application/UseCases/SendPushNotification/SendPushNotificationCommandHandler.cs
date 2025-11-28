using MediatR;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces.Repositories;
using NotificationService.Domain.Interfaces.External;
using Microsoft.Extensions.Logging;
using ErrorService.Shared.Exceptions;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.UseCases.SendPushNotification;

public class SendPushNotificationCommandHandler
    : IRequestHandler<SendPushNotificationCommand, SendPushNotificationResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationLogRepository _logRepository;
    private readonly IPushNotificationProvider _pushProvider;
    private readonly ILogger<SendPushNotificationCommandHandler> _logger;

    public SendPushNotificationCommandHandler(
        INotificationRepository notificationRepository,
        INotificationLogRepository logRepository,
        IPushNotificationProvider pushProvider,
        ILogger<SendPushNotificationCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logRepository = logRepository;
        _pushProvider = pushProvider;
        _logger = logger;
    }

    public async Task<SendPushNotificationResponse> Handle(
        SendPushNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        _logger.LogInformation("Creating push notification for device {DeviceToken}", request.DeviceToken);

        try
        {
            var notification = Notification.CreatePushNotification(
                request.DeviceToken,
                request.Title,
                request.Body,
                metadata: request.Metadata
            );

            await _notificationRepository.AddAsync(notification);

            _logger.LogInformation("Push notification {NotificationId} created, sending via {Provider}",
                notification.Id, _pushProvider.ProviderName);

            var (success, messageId, error) = await _pushProvider.SendAsync(
                request.DeviceToken,
                request.Title,
                request.Body,
                request.Data,
                request.Metadata
            );

            if (success)
            {
                notification.MarkAsSent();
                await _logRepository.AddAsync(NotificationLog.CreateSent(notification.Id, messageId));
                _logger.LogInformation("Push notification {NotificationId} sent successfully", notification.Id);
            }
            else
            {
                notification.MarkAsFailed(error ?? "Unknown error");
                await _logRepository.AddAsync(NotificationLog.CreateFailed(notification.Id, error ?? "Unknown error"));
                _logger.LogWarning("Failed to send push notification {NotificationId}: {Error}",
                    notification.Id, error);
                throw new ServiceUnavailableException($"Failed to send push notification: {error}");
            }

            await _notificationRepository.UpdateAsync(notification);

            return new SendPushNotificationResponse(
                notification.Id,
                notification.Status.ToString(),
                success ? "Push notification sent successfully" : error ?? "Failed to send push notification"
            );
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending push notification to device {DeviceToken}", request.DeviceToken);
            throw new ServiceUnavailableException("An unexpected error occurred while sending the push notification");
        }
    }
}