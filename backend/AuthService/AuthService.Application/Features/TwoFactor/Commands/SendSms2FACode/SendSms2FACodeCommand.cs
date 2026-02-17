using MediatR;

namespace AuthService.Application.Features.TwoFactor.Commands.SendSms2FACode;

/// <summary>
/// Command to send a 2FA verification code via SMS after login.
/// User provides the temp token received after successful password login.
/// </summary>
public record SendSms2FACodeCommand(string TempToken) : IRequest<SendSms2FACodeResponse>;

/// <summary>
/// Response after sending SMS 2FA code
/// </summary>
public record SendSms2FACodeResponse(
    Guid UserId,
    string MaskedPhoneNumber,
    int CodeExpirationMinutes,
    string Message
);
