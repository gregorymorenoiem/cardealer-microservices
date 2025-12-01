using StackExchange.Redis;
using CacheService.Application.Interfaces;
using CacheService.Domain;
using System.Text.Json;

namespace CacheService.Infrastructure;

public class RedisLockManager : IDistributedLockManager
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private const string LockPrefix = "lock:";

    public RedisLockManager(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task<CacheLock?> AcquireLockAsync(string key, string ownerId, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var lockKey = GetLockKey(key);
        var lockValue = JsonSerializer.Serialize(new
        {
            OwnerId = ownerId,
            AcquiredAt = DateTime.UtcNow
        });

        // Try to acquire lock using SET NX (set if not exists)
        var acquired = await _db.StringSetAsync(lockKey, lockValue, ttl, When.NotExists);

        if (!acquired)
            return null;

        return new CacheLock
        {
            Key = key,
            OwnerId = ownerId,
            AcquiredAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(ttl),
            Ttl = ttl,
            IsAutoRenew = false,
            RenewCount = 0
        };
    }

    public async Task<bool> ReleaseLockAsync(string key, string ownerId, CancellationToken cancellationToken = default)
    {
        var lockKey = GetLockKey(key);
        var currentValue = await _db.StringGetAsync(lockKey);

        if (!currentValue.HasValue)
            return false;

        // Verify ownership before releasing
        var lockData = JsonSerializer.Deserialize<Dictionary<string, object>>(currentValue.ToString());
        if (lockData == null || !lockData.ContainsKey("OwnerId"))
            return false;

        var currentOwnerId = lockData["OwnerId"].ToString();
        if (currentOwnerId != ownerId)
            return false; // Cannot release someone else's lock

        return await _db.KeyDeleteAsync(lockKey);
    }

    public async Task<bool> RenewLockAsync(string key, string ownerId, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var lockKey = GetLockKey(key);
        var currentValue = await _db.StringGetAsync(lockKey);

        if (!currentValue.HasValue)
            return false;

        // Verify ownership before renewing
        var lockData = JsonSerializer.Deserialize<Dictionary<string, object>>(currentValue.ToString());
        if (lockData == null || !lockData.ContainsKey("OwnerId"))
            return false;

        var currentOwnerId = lockData["OwnerId"].ToString();
        if (currentOwnerId != ownerId)
            return false; // Cannot renew someone else's lock

        return await _db.KeyExpireAsync(lockKey, ttl);
    }

    public async Task<bool> IsLockedAsync(string key, CancellationToken cancellationToken = default)
    {
        var lockKey = GetLockKey(key);
        return await _db.KeyExistsAsync(lockKey);
    }

    public async Task<CacheLock?> GetLockAsync(string key, CancellationToken cancellationToken = default)
    {
        var lockKey = GetLockKey(key);
        var value = await _db.StringGetAsync(lockKey);

        if (!value.HasValue)
            return null;

        var ttl = await _db.KeyTimeToLiveAsync(lockKey);
        var lockData = JsonSerializer.Deserialize<Dictionary<string, object>>(value.ToString());

        if (lockData == null)
            return null;

        var ownerId = lockData.ContainsKey("OwnerId") ? lockData["OwnerId"].ToString() : string.Empty;
        var acquiredAt = lockData.ContainsKey("AcquiredAt") 
            ? DateTime.Parse(lockData["AcquiredAt"].ToString()!) 
            : DateTime.UtcNow;

        return new CacheLock
        {
            Key = key,
            OwnerId = ownerId ?? string.Empty,
            AcquiredAt = acquiredAt,
            ExpiresAt = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : DateTime.UtcNow,
            Ttl = ttl ?? TimeSpan.Zero,
            IsAutoRenew = false,
            RenewCount = 0
        };
    }

    public async Task<bool> ForceReleaseLockAsync(string key, CancellationToken cancellationToken = default)
    {
        var lockKey = GetLockKey(key);
        return await _db.KeyDeleteAsync(lockKey);
    }

    private string GetLockKey(string key)
    {
        return $"{LockPrefix}{key}";
    }
}
