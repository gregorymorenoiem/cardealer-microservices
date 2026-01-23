using MediatR;

namespace AuthService.Application.Features.TwoFactor.Commands.RecoveryAccountWithAllCodes;

/// <summary>
/// Command to recover 2FA access by providing ALL 10 recovery codes.
/// This proves ownership of the account and allows regenerating 2FA.
/// 
/// Industry Pattern: "Full Recovery Code Verification"
/// - User must provide ALL 10 codes (not just one)
/// - If all 10 match, proves account ownership
/// - Resets 2FA and generates new authenticator + new recovery codes
/// </summary>
public record RecoveryAccountWithAllCodesCommand(
    string TempToken,
    List<string> RecoveryCodes // All 10 codes
) : IRequest<RecoveryAccountWithAllCodesResponse>;

public record RecoveryAccountWithAllCodesResponse(
    string UserId,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string NewAuthenticatorSecret,
    string NewQrCodeUri,
    List<string> NewRecoveryCodes,
    string Message
);
