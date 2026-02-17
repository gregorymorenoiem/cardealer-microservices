using System.Threading;
using System.Threading.Tasks;

namespace StaffService.Application.Clients;

/// <summary>
/// Client for sending notifications via NotificationService.
/// </summary>
public interface INotificationClient
{
    /// <summary>
    /// Sends a staff invitation email.
    /// </summary>
    Task SendInvitationEmailAsync(
        string email,
        string invitationToken,
        string inviterName,
        string role,
        string? personalMessage,
        CancellationToken ct = default);
    
    /// <summary>
    /// Sends welcome email after invitation acceptance.
    /// </summary>
    Task SendWelcomeEmailAsync(
        string email,
        string staffName,
        string role,
        CancellationToken ct = default);
    
    /// <summary>
    /// Sends notification about status change.
    /// </summary>
    Task SendStatusChangeEmailAsync(
        string email,
        string staffName,
        string oldStatus,
        string newStatus,
        string? reason,
        CancellationToken ct = default);
}
