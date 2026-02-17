using MediatR;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces.Repositories;
using NotificationService.Domain.Interfaces.External;
using NotificationService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using ErrorService.Shared.Exceptions;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.UseCases.SendSmsNotification;

public class SendSmsNotificationCommandHandler
    : IRequestHandler<SendSmsNotificationCommand, SendSmsNotificationResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationLogRepository _logRepository;
    private readonly ISmsProvider _smsProvider;
    private readonly IConfigurationServiceClient _configClient;
    private readonly ILogger<SendSmsNotificationCommandHandler> _logger;

    public SendSmsNotificationCommandHandler(
        INotificationRepository notificationRepository,
        INotificationLogRepository logRepository,
        ISmsProvider smsProvider,
        IConfigurationServiceClient configClient,
        ILogger<SendSmsNotificationCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logRepository = logRepository;
        _smsProvider = smsProvider;
        _configClient = configClient;
        _logger = logger;
    }

    public async Task<SendSmsNotificationResponse> Handle(
        SendSmsNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // ✅ Check if SMS channel is enabled in admin configuration
        var smsEnabled = await _configClient.IsEnabledAsync("sms.enabled", cancellationToken);
        if (!smsEnabled)
        {
            _logger.LogInformation("SMS channel is disabled in configuration. Skipping send to {To}", request.To);
            return new SendSmsNotificationResponse(
                Guid.NewGuid(),
                "Skipped",
                "SMS channel is disabled in platform configuration"
            );
        }

        _logger.LogInformation("Creating SMS notification for {To}", request.To);

        try
        {
            var notification = Notification.CreateSmsNotification(
                request.To,
                request.Message,
                metadata: request.Metadata
            );

            await _notificationRepository.AddAsync(notification);

            _logger.LogInformation("SMS notification {NotificationId} created, sending via {Provider}",
                notification.Id, _smsProvider.ProviderName);

            var (success, messageId, error) = await _smsProvider.SendAsync(
                request.To,
                request.Message,
                request.Metadata
            );

            if (success)
            {
                notification.MarkAsSent();
                await _logRepository.AddAsync(NotificationLog.CreateSent(notification.Id, messageId));
                _logger.LogInformation("SMS notification {NotificationId} sent successfully", notification.Id);
            }
            else
            {
                notification.MarkAsFailed(error ?? "Unknown error");
                await _logRepository.AddAsync(NotificationLog.CreateFailed(notification.Id, error ?? "Unknown error"));
                _logger.LogWarning("Failed to send SMS notification {NotificationId}: {Error}",
                    notification.Id, error);
                throw new ServiceUnavailableException($"Failed to send SMS: {error}");
            }

            await _notificationRepository.UpdateAsync(notification);

            return new SendSmsNotificationResponse(
                notification.Id,
                notification.Status.ToString(),
                success ? "SMS sent successfully" : error ?? "Failed to send SMS"
            );
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending SMS notification to {To}", request.To);
            throw new ServiceUnavailableException("An unexpected error occurred while sending the SMS");
        }
    }
}