using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Interfaces;

/// <summary>
/// Determines which payment gateways are available for new payment method selection.
/// Reads the admin-panel billing toggles from ConfigurationService.
/// 
/// KEY BUSINESS RULE:
/// - Disabled gateways are NOT available for new users selecting a payment method.
/// - Existing users who already have a saved payment method with a disabled gateway
///   can STILL be charged (recurring/subscription). This is handled in ChargeCommandHandler.
/// </summary>
public interface IGatewayAvailabilityService
{
    /// <summary>
    /// Check if a gateway is enabled for new payment method selection.
    /// Falls back to local appsettings IsEnabled if ConfigurationService is unavailable.
    /// </summary>
    Task<bool> IsEnabledForNewUsersAsync(PaymentGateway gateway, CancellationToken ct = default);

    /// <summary>
    /// Returns the list of gateways that are currently enabled for new users.
    /// Only includes gateways that are both registered AND enabled in admin panel.
    /// </summary>
    Task<IReadOnlyList<PaymentGateway>> GetEnabledGatewaysAsync(CancellationToken ct = default);
}
