using CarDealer.Shared.Configuration;

namespace KYCService.Application.Services;

/// <summary>
/// Service that reads KYC-specific configuration from ConfigurationService (admin panel).
/// Provides typed access to all KYC settings with defaults as fallback.
/// Values are cached for 60 seconds by the underlying ConfigurationServiceClient.
/// Config keys must match the seeded keys in ConfigurationService exactly.
/// </summary>
public interface IKYCConfigurationService
{
    Task<int> GetMaxVerificationAttemptsAsync(CancellationToken ct = default);
    Task<int> GetVerificationTimeoutMinutesAsync(CancellationToken ct = default);
    Task<int> GetDocumentExpirationDaysAsync(CancellationToken ct = default);
    Task<int> GetHighConfidenceThresholdAsync(CancellationToken ct = default);
    Task<int> GetFacialMatchThresholdAsync(CancellationToken ct = default);
    Task<bool> IsLivenessRequiredAsync(CancellationToken ct = default);
    Task<bool> IsAutoApproveHighConfidenceAsync(CancellationToken ct = default);
}

public class KYCConfigurationService : IKYCConfigurationService
{
    private readonly IConfigurationServiceClient _configClient;

    public KYCConfigurationService(IConfigurationServiceClient configClient)
    {
        _configClient = configClient;
    }

    public Task<int> GetMaxVerificationAttemptsAsync(CancellationToken ct = default)
        => _configClient.GetIntAsync("kyc.max_verification_attempts", 3, ct);

    public Task<int> GetVerificationTimeoutMinutesAsync(CancellationToken ct = default)
        => _configClient.GetIntAsync("kyc.verification_timeout_minutes", 30, ct);

    public Task<int> GetDocumentExpirationDaysAsync(CancellationToken ct = default)
        => _configClient.GetIntAsync("kyc.document_expiration_days", 365, ct);

    public Task<int> GetHighConfidenceThresholdAsync(CancellationToken ct = default)
        => _configClient.GetIntAsync("kyc.high_confidence_threshold", 95, ct);

    public Task<int> GetFacialMatchThresholdAsync(CancellationToken ct = default)
        => _configClient.GetIntAsync("kyc.face_match_threshold", 80, ct);

    public Task<bool> IsLivenessRequiredAsync(CancellationToken ct = default)
        => _configClient.IsEnabledAsync("kyc.require_liveness_check", defaultValue: true, ct);

    public Task<bool> IsAutoApproveHighConfidenceAsync(CancellationToken ct = default)
        => _configClient.IsEnabledAsync("kyc.auto_approve_high_confidence", defaultValue: false, ct);
}
