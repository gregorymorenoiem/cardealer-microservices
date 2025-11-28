namespace AuthService.Domain.Enums;

public enum TwoFactorAuthType
{
    Authenticator = 1,
    SMS = 2,
    Email = 3
}

public enum TwoFactorAuthStatus
{
    Disabled = 0,
    Enabled = 1,
    PendingVerification = 2
}