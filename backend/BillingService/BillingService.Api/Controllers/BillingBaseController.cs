using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

/// <summary>
/// Base controller for BillingService that provides JWT-based DealerId extraction.
/// All billing controllers should inherit from this to enforce ownership validation
/// instead of trusting the X-Dealer-Id header (IDOR prevention — OWASP A01:2021).
/// </summary>
public abstract class BillingBaseController : ControllerBase
{
    /// <summary>
    /// Extracts the DealerId from the authenticated user's JWT claims.
    /// The "dealerId" claim is set by AuthService during token generation.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the dealerId claim is missing or not a valid GUID.
    /// </exception>
    protected Guid GetDealerIdFromJwt()
    {
        var claim = User.FindFirst("dealerId")?.Value;
        if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var dealerId))
            throw new UnauthorizedAccessException(
                "dealerId claim missing or invalid in JWT token. Ensure you are authenticated as a dealer.");
        return dealerId;
    }

    /// <summary>
    /// Returns true if the current user has the Admin role.
    /// </summary>
    protected bool IsAdmin()
    {
        return User.IsInRole("Admin") || User.IsInRole("admin");
    }

    /// <summary>
    /// Returns the DealerId from JWT. If a different dealerId is requested (e.g., via route param),
    /// validates that the user is an Admin. Non-admin users can only access their own data.
    /// Use this for endpoints that accept dealerId as a route parameter for admin access.
    /// </summary>
    protected Guid GetDealerIdOrOverride(Guid? requestedDealerId)
    {
        var jwtDealerId = GetDealerIdFromJwt();

        if (requestedDealerId.HasValue && requestedDealerId.Value != jwtDealerId)
        {
            if (!IsAdmin())
                throw new UnauthorizedAccessException(
                    "Access denied: you can only access your own dealer data.");
            return requestedDealerId.Value;
        }

        return jwtDealerId;
    }
}
