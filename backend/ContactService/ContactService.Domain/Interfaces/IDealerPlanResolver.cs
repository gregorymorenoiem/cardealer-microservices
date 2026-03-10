namespace ContactService.Domain.Interfaces;

/// <summary>
/// Resolves a dealer's current subscription plan.
/// In a microservices architecture, the plan is cached in Redis by BillingService/AdminService
/// and read by ContactService for limit enforcement.
///
/// Redis key: okla:dealer:plan:{dealerId} → plan name string (e.g., "elite", "pro")
///
/// CONTRA #5 FIX: Required for per-dealer conversation limit enforcement.
/// </summary>
public interface IDealerPlanResolver
{
    /// <summary>
    /// Get the current plan for a dealer.
    /// Returns the plan key (e.g., "libre", "visible", "pro", "elite").
    /// Defaults to "libre" if the plan cannot be resolved.
    /// </summary>
    Task<string> GetDealerPlanAsync(Guid dealerId, CancellationToken ct = default);
}
