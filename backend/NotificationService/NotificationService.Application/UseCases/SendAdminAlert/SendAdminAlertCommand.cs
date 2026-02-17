using MediatR;

namespace NotificationService.Application.UseCases.SendAdminAlert;

/// <summary>
/// Command to send an admin alert through configured channels.
/// </summary>
public record SendAdminAlertCommand(
    /// <summary>
    /// Alert type: new_user_registered, new_listing_pending, new_dealer_registered,
    /// user_report, payment_failed, daily_summary, kyc_pending_review, system_errors
    /// </summary>
    string AlertType,
    string Title,
    string Message,
    string Severity = "Info",
    Dictionary<string, string>? Metadata = null
) : IRequest<SendAdminAlertResponse>;

public record SendAdminAlertResponse(
    bool Success,
    string? Error = null
);
