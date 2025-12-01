using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs.TwoFactor;

public record Get2FAMethodsResponse(
    string UserId,
    TwoFactorAuthType PrimaryMethod,
    List<TwoFactorAuthType> EnabledMethods,
    bool IsEnabled,
    bool HasPhoneNumber,
    bool HasAuthenticator
);
