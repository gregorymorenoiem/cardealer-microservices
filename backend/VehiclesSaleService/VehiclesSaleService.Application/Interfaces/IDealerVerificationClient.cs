namespace VehiclesSaleService.Application.Interfaces;

/// <summary>
/// Contract for checking dealer KYC verification status.
/// Calls DealerManagementService to verify that a dealer
/// has completed KYC before allowing listing publication.
/// </summary>
public interface IDealerVerificationClient
{
    /// <summary>
    /// Returns true if the dealer with the given userId has a Verified KYC status.
    /// Returns false if not found, pending, or rejected.
    /// </summary>
    Task<bool> IsDealerVerifiedAsync(Guid userId, CancellationToken cancellationToken = default);
}
