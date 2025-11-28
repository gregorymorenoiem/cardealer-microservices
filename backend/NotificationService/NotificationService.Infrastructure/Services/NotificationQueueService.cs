using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Interfaces.Repositories;
using NotificationService.Domain.Interfaces.External;

namespace NotificationService.Infrastructure.Services;

public class NotificationQueueService
{
    private readonly INotificationQueueRepository _queueRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationLogRepository _logRepository;
    private readonly IEmailProvider _emailProvider;
    private readonly ISmsProvider _smsProvider;
    private readonly IPushNotificationProvider _pushProvider;
    private readonly ILogger<NotificationQueueService> _logger;

    public NotificationQueueService(
        INotificationQueueRepository queueRepository,
        INotificationRepository notificationRepository,
        INotificationLogRepository logRepository,
        IEmailProvider emailProvider,
        ISmsProvider smsProvider,
        IPushNotificationProvider pushProvider,
        ILogger<NotificationQueueService> logger)
    {
        _queueRepository = queueRepository;
        _notificationRepository = notificationRepository;
        _logRepository = logRepository;
        _emailProvider = emailProvider;
        _smsProvider = smsProvider;
        _pushProvider = pushProvider;
        _logger = logger;
    }

    public async Task ProcessPendingQueueAsync()
    {
        _logger.LogInformation("Starting queue processing");

        try
        {
            var pendingQueues = await _queueRepository.GetPendingAsync();

            foreach (var queue in pendingQueues)
            {
                await ProcessQueueItemAsync(queue);
            }

            _logger.LogInformation("Queue processing completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during queue processing");
        }
    }

    private async Task ProcessQueueItemAsync(NotificationQueue queue)
    {
        try
        {
            _logger.LogInformation("Processing queue item {QueueId} for notification {NotificationId}",
                queue.Id, queue.NotificationId);

            queue.MarkAsProcessing();
            await _queueRepository.UpdateAsync(queue);

            var notification = await _notificationRepository.GetByIdAsync(queue.NotificationId);
            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found for queue item {QueueId}",
                    queue.NotificationId, queue.Id);
                return;
            }

            var (success, messageId, error) = await ProcessNotificationAsync(notification);

            if (success)
            {
                notification.MarkAsSent();
                await _logRepository.AddAsync(NotificationLog.CreateSent(notification.Id, messageId));
                queue.MarkAsCompleted();
                _logger.LogInformation("Successfully processed notification {NotificationId}", notification.Id);
            }
            else
            {
                if (notification.CanRetry())
                {
                    notification.MarkAsFailed(error ?? "Unknown error");
                    queue.MarkAsFailed(error ?? "Unknown error");
                    _logger.LogWarning("Failed to process notification {NotificationId}, will retry: {Error}",
                        notification.Id, error);
                }
                else
                {
                    notification.MarkAsFailed(error ?? "Unknown error");
                    queue.MarkAsFailed(error ?? "Unknown error");
                    _logger.LogError("Failed to process notification {NotificationId} after retries: {Error}",
                        notification.Id, error);
                }

                await _logRepository.AddAsync(NotificationLog.CreateFailed(notification.Id, error ?? "Unknown error"));
            }

            await _notificationRepository.UpdateAsync(notification);
            await _queueRepository.UpdateAsync(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing queue item {QueueId}", queue.Id);
            queue.MarkAsFailed(ex.Message);
            await _queueRepository.UpdateAsync(queue);
        }
    }

    private async Task<(bool success, string? messageId, string? error)> ProcessNotificationAsync(Notification notification)
    {
        try
        {
            return notification.Type switch
            {
                NotificationType.Email => await _emailProvider.SendAsync(
                    notification.Recipient,
                    notification.Subject,
                    notification.Content,
                    true,
                    notification.Metadata
                ),
                NotificationType.Sms => await _smsProvider.SendAsync(
                    notification.Recipient,
                    notification.Content,
                    notification.Metadata
                ),
                NotificationType.Push => await _pushProvider.SendAsync(
                    notification.Recipient,
                    notification.Subject,
                    notification.Content,
                    null,
                    notification.Metadata
                ),
                _ => (false, null, $"Unsupported notification type: {notification.Type}")
            };
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }
}