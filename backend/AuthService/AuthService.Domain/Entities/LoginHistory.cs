namespace AuthService.Domain.Entities;

/// <summary>
/// Representa un registro histórico de login
/// </summary>
public class LoginHistory
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string DeviceInfo { get; private set; } = string.Empty;
    public string Browser { get; private set; } = string.Empty;
    public string OperatingSystem { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public string? Country { get; private set; }
    public string? City { get; private set; }
    public DateTime LoginTime { get; private set; }
    public bool Success { get; private set; }
    public string? FailureReason { get; private set; }
    public LoginMethod Method { get; private set; }
    public bool TwoFactorUsed { get; private set; }
    public TwoFactorMethod? TwoFactorMethod { get; private set; }

    // Navigation property
    public virtual ApplicationUser User { get; private set; } = null!;

    // EF Core constructor
    private LoginHistory() { }

    public LoginHistory(
        string userId,
        string deviceInfo,
        string browser,
        string operatingSystem,
        string ipAddress,
        bool success,
        LoginMethod method = LoginMethod.Password,
        string? failureReason = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        DeviceInfo = deviceInfo;
        Browser = browser;
        OperatingSystem = operatingSystem;
        IpAddress = ipAddress;
        Success = success;
        FailureReason = failureReason;
        Method = method;
        LoginTime = DateTime.UtcNow;
        TwoFactorUsed = false;
    }

    public void SetTwoFactor(TwoFactorMethod method)
    {
        TwoFactorUsed = true;
        TwoFactorMethod = method;
    }

    public void SetLocation(string? location, string? country, string? city)
    {
        Location = location;
        Country = country;
        City = city;
    }
}

public enum LoginMethod
{
    Password = 0,
    Google = 1,
    Facebook = 2,
    Apple = 3,
    Microsoft = 4,
    RefreshToken = 5
}

public enum TwoFactorMethod
{
    None = 0,
    Totp = 1,
    Sms = 2,
    Email = 3,
    RecoveryCode = 4
}
