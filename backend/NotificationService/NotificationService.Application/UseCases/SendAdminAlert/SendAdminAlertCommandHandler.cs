using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.UseCases.SendAdminAlert;

public class SendAdminAlertCommandHandler
    : IRequestHandler<SendAdminAlertCommand, SendAdminAlertResponse>
{
    private readonly IAdminAlertService _adminAlertService;
    private readonly ILogger<SendAdminAlertCommandHandler> _logger;

    public SendAdminAlertCommandHandler(
        IAdminAlertService adminAlertService,
        ILogger<SendAdminAlertCommandHandler> logger)
    {
        _adminAlertService = adminAlertService;
        _logger = logger;
    }

    public async Task<SendAdminAlertResponse> Handle(
        SendAdminAlertCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _adminAlertService.SendAlertAsync(
                request.AlertType,
                request.Title,
                request.Message,
                request.Severity,
                request.Metadata,
                cancellationToken);

            _logger.LogInformation(
                "Admin alert dispatched: Type={AlertType}, Title={Title}, Severity={Severity}",
                request.AlertType, request.Title, request.Severity);

            return new SendAdminAlertResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send admin alert: Type={AlertType}, Title={Title}",
                request.AlertType, request.Title);

            return new SendAdminAlertResponse(false, ex.Message);
        }
    }
}
