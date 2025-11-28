// NotificationService.Application\UseCases\SendEmailNotification\SendEmailNotificationCommandHandler.cs
using MediatR;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces.Repositories;
using NotificationService.Domain.Interfaces.External;
using Microsoft.Extensions.Logging;
using ErrorService.Shared.Exceptions;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.UseCases.SendEmailNotification;

public class SendEmailNotificationCommandHandler
    : IRequestHandler<SendEmailNotificationCommand, SendEmailNotificationResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationLogRepository _logRepository;
    private readonly IEmailProvider _emailProvider;
    private readonly ILogger<SendEmailNotificationCommandHandler> _logger;

    public SendEmailNotificationCommandHandler(
        INotificationRepository notificationRepository,
        INotificationLogRepository logRepository,
        IEmailProvider emailProvider,
        ILogger<SendEmailNotificationCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logRepository = logRepository;
        _emailProvider = emailProvider;
        _logger = logger;
    }

    public async Task<SendEmailNotificationResponse> Handle(
        SendEmailNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        _logger.LogInformation("Creating email notification for {To}", request.To);

        try
        {
            // Crear la notificación
            var notification = Notification.CreateEmailNotification(
                request.To,
                request.Subject,
                request.Body,
                metadata: request.Metadata
            );

            await _notificationRepository.AddAsync(notification);

            _logger.LogInformation("Notification {NotificationId} created, sending via {Provider}",
                notification.Id, _emailProvider.ProviderName);

            // Enviar el email
            var (success, messageId, error) = await _emailProvider.SendAsync(
                request.To,
                request.Subject,
                request.Body,
                request.IsHtml,
                request.Metadata
            );

            if (success)
            {
                notification.MarkAsSent();
                await _logRepository.AddAsync(NotificationLog.CreateSent(notification.Id, messageId));
                _logger.LogInformation("Email notification {NotificationId} sent successfully", notification.Id);
            }
            else
            {
                notification.MarkAsFailed(error ?? "Unknown error");
                await _logRepository.AddAsync(NotificationLog.CreateFailed(notification.Id, error ?? "Unknown error"));
                _logger.LogWarning("Failed to send email notification {NotificationId}: {Error}",
                    notification.Id, error);

                // ✅ EL MIDDLEWARE CAPTURARÁ AUTOMÁTICAMENTE ESTA EXCEPCIÓN
                throw new ServiceUnavailableException($"Failed to send email: {error}");
            }

            await _notificationRepository.UpdateAsync(notification);

            return new SendEmailNotificationResponse(
                notification.Id,
                notification.Status.ToString(),
                success ? "Email sent successfully" : error ?? "Failed to send email"
            );
        }
        catch (AppException)
        {
            throw; // El middleware capturará estas excepciones
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email notification to {To}", request.To);

            throw new ServiceUnavailableException("An unexpected error occurred while sending the email");
        }
    }
}