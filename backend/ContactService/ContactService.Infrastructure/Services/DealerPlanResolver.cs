using CarDealer.Contracts.Enums;
using ContactService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ContactService.Infrastructure.Services;

/// <summary>
/// Redis-backed dealer plan resolver.
/// Reads the dealer's current plan from Redis cache (set by BillingService/AdminService).
///
/// Redis key: okla:dealer:plan:{dealerId} → plan name string
///
/// Fallback: Returns "libre" if Redis is unavailable or key not found.
/// This is the safest default (most restrictive plan = no ChatAgent access).
/// </summary>
public sealed class DealerPlanResolver : IDealerPlanResolver
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<DealerPlanResolver> _logger;

    private const string KeyPrefix = "okla:dealer:plan";

    public DealerPlanResolver(
        ILogger<DealerPlanResolver> logger,
        IConnectionMultiplexer? redis = null)
    {
        _logger = logger;
        _redis = redis;
    }

    public async Task<string> GetDealerPlanAsync(Guid dealerId, CancellationToken ct = default)
    {
        if (_redis == null)
        {
            _logger.LogWarning("[DealerPlanResolver] Redis not available, defaulting to 'libre' for dealer {DealerId}", dealerId);
            return "libre";
        }

        try
        {
            var db = _redis.GetDatabase();
            var key = $"{KeyPrefix}:{dealerId:N}";
            var plan = await db.StringGetAsync(key);

            if (plan.HasValue)
            {
                var planValue = plan.ToString();
                // Normalize to frontend key for consistency
                return PlanConfiguration.GetFrontendKey(planValue);
            }

            _logger.LogWarning(
                "[DealerPlanResolver] No plan cached for dealer {DealerId}, defaulting to 'libre'",
                dealerId);
            return "libre";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "[DealerPlanResolver] Redis read failed for dealer {DealerId}, defaulting to 'libre'",
                dealerId);
            return "libre";
        }
    }
}
