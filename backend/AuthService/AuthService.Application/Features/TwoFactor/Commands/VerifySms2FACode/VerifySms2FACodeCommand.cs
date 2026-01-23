using MediatR;

namespace AuthService.Application.Features.TwoFactor.Commands.VerifySms2FACode;

/// <summary>
/// Command to verify SMS 2FA code and complete login.
/// User provides the temp token and the 6-digit code received via SMS.
/// </summary>
public record VerifySms2FACodeCommand(
    string TempToken,
    string Code
) : IRequest<VerifySms2FACodeResponse>;

/// <summary>
/// Response after successful SMS 2FA verification
/// </summary>
public record VerifySms2FACodeResponse(
    Guid UserId,
    string AccessToken,
    string RefreshToken,
    string Message
);
