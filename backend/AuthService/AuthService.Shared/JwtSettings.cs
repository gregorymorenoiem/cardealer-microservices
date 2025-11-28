namespace AuthService.Shared;

/// <summary>
/// JWT configuration settings read from appsettings.json
/// </summary>
public class JwtSettings
{
    /// <summary>Secret key used to sign the token</summary>
    public string Key { get; set; } = null!;

    /// <summary>Token issuer (iss)</summary>
    public string Issuer { get; set; } = null!;

    /// <summary>Token audience (aud)</summary>
    public string Audience { get; set; } = null!;

    /// <summary>Expiration time in minutes</summary>
    public int ExpiresMinutes { get; set; } = 60;

    /// <summary>Refresh token expiration in days</summary>
    public int RefreshTokenExpiresDays { get; set; } = 7;

    /// <summary>Clock skew allowed in token validation</summary>
    public int ClockSkewMinutes { get; set; } = 5;
}
