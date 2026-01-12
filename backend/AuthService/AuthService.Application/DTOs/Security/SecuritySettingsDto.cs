namespace AuthService.Application.DTOs.Security;

/// <summary>
/// Security settings for a user account
/// </summary>
public record SecuritySettingsDto(
    bool TwoFactorEnabled,
    string? TwoFactorType,
    string? LastPasswordChange,
    List<ActiveSessionDto> ActiveSessions,
    List<LoginHistoryDto> RecentLogins
);

/// <summary>
/// Active user session information
/// </summary>
public record ActiveSessionDto(
    string Id,
    string Device,
    string Browser,
    string Location,
    string IpAddress,
    string LastActive,
    bool IsCurrent
);

/// <summary>
/// Login history entry
/// </summary>
public record LoginHistoryDto(
    string Id,
    string Device,
    string Browser,
    string Location,
    string IpAddress,
    string LoginTime,
    bool Success
);

/// <summary>
/// Request to change password
/// </summary>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);

/// <summary>
/// Response for password change
/// </summary>
public record ChangePasswordResponse(
    bool Success,
    string Message
);
