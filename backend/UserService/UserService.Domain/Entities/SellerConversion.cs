namespace UserService.Domain.Entities;

/// <summary>
/// Tracks account-type conversions (Buyer → Seller).
/// Used for auditing, rollback, and idempotency.
/// </summary>
public class SellerConversion
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SellerProfileId { get; set; }

    /// <summary>
    /// Source of the conversion: "conversion", "direct_registration", "backfill"
    /// </summary>
    public string Source { get; set; } = "conversion";

    /// <summary>
    /// Previous account type before conversion.
    /// </summary>
    public AccountType PreviousAccountType { get; set; }

    /// <summary>
    /// New account type after conversion.
    /// </summary>
    public AccountType NewAccountType { get; set; } = AccountType.Seller;

    /// <summary>
    /// Status of the conversion: Pending, Approved, Rejected, Reverted
    /// </summary>
    public SellerConversionStatus Status { get; set; } = SellerConversionStatus.Pending;

    /// <summary>
    /// KYC profile ID reference (no raw PII stored here).
    /// </summary>
    public Guid? KycProfileId { get; set; }

    /// <summary>
    /// Idempotency key used for the conversion request.
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Correlation ID for distributed tracing.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// IP address of the requester (audit).
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent of the requester (audit).
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Optional notes (e.g., rejection reason or admin comment).
    /// </summary>
    public string? Notes { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? RevertedAt { get; set; }
}

public enum SellerConversionStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Reverted = 3
}
