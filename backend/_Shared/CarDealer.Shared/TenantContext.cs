using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CarDealer.Shared.MultiTenancy;

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CurrentDealerId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var dealerIdClaim = user?.FindFirst("dealerId")?.Value;

            if (string.IsNullOrEmpty(dealerIdClaim))
                return null;

            return Guid.TryParse(dealerIdClaim, out var id) ? id : null;
        }
    }

    public bool HasDealerContext => CurrentDealerId.HasValue;
}
