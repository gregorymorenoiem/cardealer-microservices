namespace UserService.Application.Interfaces;

/// <summary>
/// Evaluates the 4 criteria for the "Dealer Verificado OKLA" badge.
/// Called by admin actions, background workers, and the dealer profile API.
///
/// Criteria:
///   1. RNC activo verificado en DGII
///   2. WhatsApp verificado por OTP
///   3. ≥10 listings con fotos reales procesadas por ModerationAgent
///   4. Sin reportes de fraude en los últimos 90 días
/// </summary>
public interface IDealerBadgeEvaluator
{
    /// <summary>
    /// Evaluate all 4 badge criteria for a dealer and return detailed results.
    /// Does NOT persist changes — the caller decides whether to update the entity.
    /// </summary>
    Task<DealerBadgeEvaluation> EvaluateAsync(Guid dealerId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of evaluating the 4 "Dealer Verificado OKLA" criteria.
/// </summary>
public record DealerBadgeEvaluation(
    Guid DealerId,
    bool IsRncVerified,
    string? DgiiTaxpayerStatus,
    bool IsWhatsAppVerified,
    int ModeratedListingsCount,
    bool HasRecentFraudReports,
    int FraudReportCount90Days,
    bool IsOklaVerified,
    string Summary)
{
    /// <summary>How many of the 4 criteria are currently met</summary>
    public int CriteriaMet =>
        (IsRncVerified ? 1 : 0) +
        (IsWhatsAppVerified ? 1 : 0) +
        (ModeratedListingsCount >= 10 ? 1 : 0) +
        (!HasRecentFraudReports ? 1 : 0);
}
