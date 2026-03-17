namespace BillingService.Domain.Entities;

/// <summary>
/// Stores which payment gateways a dealer has enabled for their subscription payments.
/// Defaults: all gateways enabled, stored as comma-separated string for simplicity.
/// </summary>
public class DealerGatewayPreferences
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; private set; }

    /// <summary>
    /// Comma-separated list of enabled gateway IDs.
    /// Example: "Azul,CardNET,PayPal"
    /// Null / empty = all gateways enabled (default behaviour).
    /// </summary>
    public string? EnabledGateways { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    // For EF Core
    private DealerGatewayPreferences() { }

    public DealerGatewayPreferences(Guid dealerId, IEnumerable<string> enabledGateways)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        EnabledGateways = string.Join(",", enabledGateways);
        UpdatedAt = DateTime.UtcNow;
    }

    public IReadOnlyList<string> GetEnabledGateways()
    {
        if (string.IsNullOrWhiteSpace(EnabledGateways))
            return [];

        return EnabledGateways.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }

    public void Update(IEnumerable<string> enabledGateways)
    {
        EnabledGateways = string.Join(",", enabledGateways);
        UpdatedAt = DateTime.UtcNow;
    }
}
