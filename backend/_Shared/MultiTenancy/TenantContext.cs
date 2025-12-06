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
            var dealerIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst("dealerId")?.Value;
            if (string.IsNullOrEmpty(dealerIdClaim))
                return null;
            return Guid.TryParse(dealerIdClaim, out var dealerId) ? dealerId : null;
        }
    }

    public bool HasDealerContext => CurrentDealerId.HasValue;
}
