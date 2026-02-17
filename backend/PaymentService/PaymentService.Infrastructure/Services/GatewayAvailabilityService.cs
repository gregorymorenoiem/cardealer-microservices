using CarDealer.Shared.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Services.Settings;

namespace PaymentService.Infrastructure.Services;

/// <summary>
/// Reads billing.{provider}_enabled flags from ConfigurationService (admin panel).
/// Falls back to local appsettings IsEnabled if ConfigurationService is unavailable.
/// Uses 60s in-memory cache via the shared ConfigurationServiceClient.
/// </summary>
public class GatewayAvailabilityService : IGatewayAvailabilityService
{
    private readonly IConfigurationServiceClient _configClient;
    private readonly IPaymentGatewayRegistry _registry;
    private readonly ILogger<GatewayAvailabilityService> _logger;

    // Local fallback settings (from appsettings.json)
    private readonly AzulSettings _azulSettings;
    private readonly CardNETSettings _cardnetSettings;
    private readonly PixelPaySettings _pixelpaySettings;
    private readonly FygaroSettings _fygaroSettings;
    private readonly PayPalSettings _paypalSettings;

    public GatewayAvailabilityService(
        IConfigurationServiceClient configClient,
        IPaymentGatewayRegistry registry,
        IOptions<AzulSettings> azulSettings,
        IOptions<CardNETSettings> cardnetSettings,
        IOptions<PixelPaySettings> pixelpaySettings,
        IOptions<FygaroSettings> fygaroSettings,
        IOptions<PayPalSettings> paypalSettings,
        ILogger<GatewayAvailabilityService> logger)
    {
        _configClient = configClient;
        _registry = registry;
        _azulSettings = azulSettings.Value;
        _cardnetSettings = cardnetSettings.Value;
        _pixelpaySettings = pixelpaySettings.Value;
        _fygaroSettings = fygaroSettings.Value;
        _paypalSettings = paypalSettings.Value;
        _logger = logger;
    }

    public async Task<bool> IsEnabledForNewUsersAsync(PaymentGateway gateway, CancellationToken ct = default)
    {
        // Must be registered in the provider registry
        if (!_registry.Contains(gateway))
            return false;

        var configKey = $"billing.{GatewayToPrefix(gateway)}_enabled";
        var localDefault = GetLocalDefault(gateway);

        // Read from ConfigurationService (admin panel). Falls open to local default.
        var isEnabled = await _configClient.IsEnabledAsync(configKey, localDefault, ct);

        _logger.LogDebug(
            "Gateway {Gateway} enabled for new users: {IsEnabled} (config key: {Key})",
            gateway, isEnabled, configKey);

        return isEnabled;
    }

    public async Task<IReadOnlyList<PaymentGateway>> GetEnabledGatewaysAsync(CancellationToken ct = default)
    {
        var enabled = new List<PaymentGateway>();

        foreach (PaymentGateway gateway in Enum.GetValues<PaymentGateway>())
        {
            if (await IsEnabledForNewUsersAsync(gateway, ct))
                enabled.Add(gateway);
        }

        return enabled.AsReadOnly();
    }

    /// <summary>
    /// Maps PaymentGateway enum to the billing config prefix used in ConfigurationService.
    /// </summary>
    private static string GatewayToPrefix(PaymentGateway gateway) => gateway switch
    {
        PaymentGateway.Azul => "azul",
        PaymentGateway.CardNET => "cardnet",
        PaymentGateway.PixelPay => "pixelpay",
        PaymentGateway.Fygaro => "fygaro",
        PaymentGateway.PayPal => "paypal",
        _ => gateway.ToString().ToLowerInvariant()
    };

    /// <summary>
    /// Gets the local appsettings fallback for a gateway's IsEnabled.
    /// </summary>
    private bool GetLocalDefault(PaymentGateway gateway) => gateway switch
    {
        PaymentGateway.Azul => _azulSettings.IsEnabled,
        PaymentGateway.CardNET => _cardnetSettings.IsEnabled,
        PaymentGateway.PixelPay => _pixelpaySettings.IsEnabled,
        PaymentGateway.Fygaro => _fygaroSettings.IsEnabled,
        PaymentGateway.PayPal => _paypalSettings.IsEnabled,
        _ => false
    };
}
