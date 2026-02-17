namespace AuthService.Domain.Entities;

/// <summary>
/// Representa una sesión activa del usuario
/// </summary>
public class UserSession
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string RefreshTokenId { get; private set; } = string.Empty;
    public string DeviceInfo { get; private set; } = string.Empty;
    public string Browser { get; private set; } = string.Empty;
    public string OperatingSystem { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public string? Country { get; private set; }
    public string? City { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastActiveAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedReason { get; private set; }

    // Calculado
    public bool IsActive => !IsRevoked && (!ExpiresAt.HasValue || ExpiresAt > DateTime.UtcNow);

    // Navigation property
    public virtual ApplicationUser User { get; private set; } = null!;

    // EF Core constructor
    private UserSession() { }

    public UserSession(
        string userId,
        string refreshTokenId,
        string deviceInfo,
        string browser,
        string operatingSystem,
        string ipAddress,
        DateTime? expiresAt = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        RefreshTokenId = refreshTokenId;
        DeviceInfo = deviceInfo;
        Browser = browser;
        OperatingSystem = operatingSystem;
        IpAddress = ipAddress;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        LastActiveAt = DateTime.UtcNow;
        IsRevoked = false;
    }

    public void UpdateLastActive()
    {
        LastActiveAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the session for a new login (renews the session instead of creating a duplicate).
    /// Updates the refresh token, expiration, and last active time.
    /// </summary>
    public void RenewSession(string newRefreshTokenId, DateTime? newExpiresAt = null)
    {
        RefreshTokenId = newRefreshTokenId;
        ExpiresAt = newExpiresAt;
        LastActiveAt = DateTime.UtcNow;
    }

    public void UpdateLocation(string? location, string? country, string? city)
    {
        Location = location;
        Country = country;
        City = city;
    }

    public void Revoke(string reason = "User requested")
    {
        if (IsRevoked) return;
        
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
    }
}
