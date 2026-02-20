using System.Text.Json;
using AdvertisingService.Application.Interfaces;
using AdvertisingService.Domain.Enums;
using AdvertisingService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AdvertisingService.Infrastructure.Services;

public class HomepageRotationCacheService : IHomepageRotationCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IAdRotationEngine _rotationEngine;
    private readonly IRotationConfigRepository _rotationConfigRepo;
    private readonly ILogger<HomepageRotationCacheService> _logger;
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(30);

    public HomepageRotationCacheService(
        IConnectionMultiplexer redis,
        IAdRotationEngine rotationEngine,
        IRotationConfigRepository rotationConfigRepo,
        ILogger<HomepageRotationCacheService> logger)
    {
        _redis = redis;
        _rotationEngine = rotationEngine;
        _rotationConfigRepo = rotationConfigRepo;
        _logger = logger;
    }

    public async Task<HomepageRotationResult?> GetRotationAsync(AdPlacementType section, CancellationToken ct = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = GetCacheKey(section);
            var cached = await db.StringGetAsync(key);

            if (cached.HasValue)
            {
                _logger.LogDebug("Cache hit for rotation {Section}", section);
                return JsonSerializer.Deserialize<HomepageRotationResult>(cached!);
            }

            _logger.LogDebug("Cache miss for rotation {Section}, computing...", section);
            return await RefreshRotationAsync(section, ct);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis unavailable for rotation cache, computing directly");
            return await ComputeRotationDirectlyAsync(section, ct);
        }
    }

    public async Task<HomepageRotationResult?> RefreshRotationAsync(AdPlacementType section, CancellationToken ct = default)
    {
        var config = await _rotationConfigRepo.GetBySectionAsync(section, ct);
        if (config == null || !config.IsActive)
        {
            _logger.LogWarning("No active rotation config for {Section}", section);
            return null;
        }

        var result = await _rotationEngine.ComputeRotationAsync(section, config, ct);

        try
        {
            var db = _redis.GetDatabase();
            var key = GetCacheKey(section);
            var expiry = TimeSpan.FromMinutes(config.RefreshIntervalMinutes);
            await db.StringSetAsync(key, JsonSerializer.Serialize(result), expiry);
            _logger.LogInformation("Rotation cached for {Section}, expiry {Minutes}m", section, config.RefreshIntervalMinutes);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to cache rotation for {Section}", section);
        }

        return result;
    }

    public async Task InvalidateAsync(AdPlacementType section)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(GetCacheKey(section));
            _logger.LogInformation("Rotation cache invalidated for {Section}", section);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate rotation cache for {Section}", section);
        }
    }

    public async Task InvalidateAllAsync()
    {
        foreach (var section in Enum.GetValues<AdPlacementType>())
        {
            await InvalidateAsync(section);
        }
    }

    private async Task<HomepageRotationResult?> ComputeRotationDirectlyAsync(AdPlacementType section, CancellationToken ct)
    {
        var config = await _rotationConfigRepo.GetBySectionAsync(section, ct);
        if (config == null || !config.IsActive) return null;
        return await _rotationEngine.ComputeRotationAsync(section, config, ct);
    }

    private static string GetCacheKey(AdPlacementType section)
        => $"advertising:rotation:{section.ToString().ToLowerInvariant()}";
}
