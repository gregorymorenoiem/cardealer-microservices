namespace CacheService.Domain;

/// <summary>
/// Represents a cached entry with value, expiration, and metadata
/// </summary>
public class CacheEntry
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public TimeSpan? Ttl { get; set; }
    public string? TenantId { get; set; }
    public string? Tags { get; set; } // Comma-separated tags for cache invalidation
    public long SizeInBytes { get; set; }
    public int AccessCount { get; set; }
    public DateTime? LastAccessedAt { get; set; }

    /// <summary>
    /// Checks if the cache entry has expired
    /// </summary>
    public bool IsExpired()
    {
        if (!ExpiresAt.HasValue)
            return false;

        return DateTime.UtcNow >= ExpiresAt.Value;
    }

    /// <summary>
    /// Calculates the remaining TTL
    /// </summary>
    public TimeSpan? GetRemainingTtl()
    {
        if (!ExpiresAt.HasValue)
            return null;

        var remaining = ExpiresAt.Value - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Records an access to this cache entry
    /// </summary>
    public void RecordAccess()
    {
        AccessCount++;
        LastAccessedAt = DateTime.UtcNow;
    }
}
