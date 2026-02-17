using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.Features.TwoFactor.Commands.VerifySms2FACode;

/// <summary>
/// Request DTO for verifying SMS 2FA code
/// </summary>
public class VerifySms2FACodeRequest
{
    /// <summary>
    /// Temporary token received from login response when 2FA is required
    /// </summary>
    [Required(ErrorMessage = "TempToken is required")]
    public string TempToken { get; set; } = string.Empty;

    /// <summary>
    /// 6-digit verification code received via SMS
    /// </summary>
    [Required(ErrorMessage = "Code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be exactly 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 numeric digits")]
    public string Code { get; set; } = string.Empty;
}
