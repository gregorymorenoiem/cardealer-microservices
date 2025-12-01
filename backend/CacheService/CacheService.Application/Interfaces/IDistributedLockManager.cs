using CacheService.Domain;

namespace CacheService.Application.Interfaces;

/// <summary>
/// Interface for distributed lock management
/// </summary>
public interface IDistributedLockManager
{
    /// <summary>
    /// Acquires a distributed lock
    /// </summary>
    Task<CacheLock?> AcquireLockAsync(string key, string ownerId, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a distributed lock
    /// </summary>
    Task<bool> ReleaseLockAsync(string key, string ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renews a distributed lock
    /// </summary>
    Task<bool> RenewLockAsync(string key, string ownerId, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a lock exists and is valid
    /// </summary>
    Task<bool> IsLockedAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets lock information
    /// </summary>
    Task<CacheLock?> GetLockAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Forcefully releases a lock (admin operation)
    /// </summary>
    Task<bool> ForceReleaseLockAsync(string key, CancellationToken cancellationToken = default);
}
