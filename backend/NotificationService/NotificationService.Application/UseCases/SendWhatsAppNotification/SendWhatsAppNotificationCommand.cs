using MediatR;

namespace NotificationService.Application.UseCases.SendWhatsAppNotification;

/// <summary>
/// Command to send a WhatsApp message (free-form or template-based).
/// </summary>
public record SendWhatsAppNotificationCommand(
    string To,
    string? Message = null,
    string? TemplateName = null,
    Dictionary<string, string>? TemplateParameters = null,
    string? LanguageCode = "es",
    Dictionary<string, object>? Metadata = null
) : IRequest<SendWhatsAppNotificationResponse>;

public record SendWhatsAppNotificationResponse(
    bool Success,
    string? MessageId = null,
    string? Error = null,
    string? Provider = null
);
