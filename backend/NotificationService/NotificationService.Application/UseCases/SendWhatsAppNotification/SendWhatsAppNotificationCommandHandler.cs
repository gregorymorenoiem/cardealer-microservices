using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Interfaces.External;
using NotificationService.Domain.Interfaces.Repositories;

namespace NotificationService.Application.UseCases.SendWhatsAppNotification;

public class SendWhatsAppNotificationCommandHandler
    : IRequestHandler<SendWhatsAppNotificationCommand, SendWhatsAppNotificationResponse>
{
    private readonly IConfigurationServiceClient _configClient;
    private readonly IWhatsAppProvider _whatsAppProvider;
    private readonly INotificationRepository _notificationRepo;
    private readonly INotificationLogRepository _logRepo;
    private readonly ILogger<SendWhatsAppNotificationCommandHandler> _logger;

    public SendWhatsAppNotificationCommandHandler(
        IConfigurationServiceClient configClient,
        IWhatsAppProvider whatsAppProvider,
        INotificationRepository notificationRepo,
        INotificationLogRepository logRepo,
        ILogger<SendWhatsAppNotificationCommandHandler> logger)
    {
        _configClient = configClient;
        _whatsAppProvider = whatsAppProvider;
        _notificationRepo = notificationRepo;
        _logRepo = logRepo;
        _logger = logger;
    }

    public async Task<SendWhatsAppNotificationResponse> Handle(
        SendWhatsAppNotificationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check if WhatsApp is enabled
        var isEnabled = await _configClient.IsEnabledAsync("whatsapp.enabled", cancellationToken);
        if (!isEnabled)
        {
            _logger.LogInformation("WhatsApp is disabled in configuration. Skipping message to {To}", request.To);
            return new SendWhatsAppNotificationResponse(false, Error: "WhatsApp is disabled in configuration");
        }

        // 2. Create notification record
        var notification = new Notification
        {
            Type = NotificationType.WhatsApp,
            Recipient = request.To,
            Subject = request.TemplateName ?? "WhatsApp Message",
            Content = request.Message ?? $"[Template: {request.TemplateName}]",
            Provider = NotificationProvider.TwilioWhatsApp, // Will be updated by actual provider
            Priority = PriorityLevel.Medium,
            Metadata = request.Metadata ?? new Dictionary<string, object>()
        };

        await _notificationRepo.AddAsync(notification);

        // 3. Send via WhatsApp provider
        (bool success, string? messageId, string? error) result;

        if (!string.IsNullOrWhiteSpace(request.TemplateName))
        {
            // Template message
            result = await _whatsAppProvider.SendTemplateAsync(
                request.To,
                request.TemplateName,
                request.TemplateParameters,
                request.LanguageCode,
                request.Metadata);
        }
        else
        {
            // Free-form message
            result = await _whatsAppProvider.SendMessageAsync(
                request.To,
                request.Message!,
                request.Metadata);
        }

        // 4. Update notification status and log
        if (result.success)
        {
            notification.MarkAsSent();
            notification.UpdateMetadata("provider_message_id", result.messageId ?? "");
            notification.UpdateMetadata("provider", _whatsAppProvider.ProviderName);

            var log = new NotificationLog
            {
                NotificationId = notification.Id,
                Action = "SENT",
                ProviderMessageId = result.messageId,
                ProviderResponse = "Success",
                Details = $"WhatsApp message sent via {_whatsAppProvider.ProviderName}"
            };
            await _logRepo.AddAsync(log);
        }
        else
        {
            notification.MarkAsFailed(result.error ?? "Unknown error");

            var log = new NotificationLog
            {
                NotificationId = notification.Id,
                Action = "FAILED",
                ErrorMessage = result.error,
                Details = $"WhatsApp send failed via {_whatsAppProvider.ProviderName}"
            };
            await _logRepo.AddAsync(log);
        }

        await _notificationRepo.UpdateAsync(notification);

        _logger.LogInformation(
            "WhatsApp notification {Status} to {To} via {Provider}. MessageId: {MessageId}",
            result.success ? "sent" : "failed",
            request.To,
            _whatsAppProvider.ProviderName,
            result.messageId);

        return new SendWhatsAppNotificationResponse(
            result.success,
            result.messageId,
            result.error,
            _whatsAppProvider.ProviderName);
    }
}
