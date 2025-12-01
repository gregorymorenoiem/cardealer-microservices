namespace CacheService.Domain;

/// <summary>
/// Represents a distributed lock for coordinating access to shared resources
/// </summary>
public class CacheLock
{
    public string Key { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime AcquiredAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public TimeSpan Ttl { get; set; }
    public bool IsAutoRenew { get; set; }
    public int RenewCount { get; set; }

    /// <summary>
    /// Checks if the lock has expired
    /// </summary>
    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }

    /// <summary>
    /// Checks if the lock is still valid
    /// </summary>
    public bool IsValid()
    {
        return !IsExpired();
    }

    /// <summary>
    /// Gets the remaining time before lock expiration
    /// </summary>
    public TimeSpan GetRemainingTime()
    {
        var remaining = ExpiresAt - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Renews the lock for another TTL period
    /// </summary>
    public void Renew()
    {
        ExpiresAt = DateTime.UtcNow.Add(Ttl);
        RenewCount++;
    }
}
