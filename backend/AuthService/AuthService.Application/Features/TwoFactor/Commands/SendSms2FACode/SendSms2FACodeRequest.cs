using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.Features.TwoFactor.Commands.SendSms2FACode;

/// <summary>
/// Request DTO for sending SMS 2FA code
/// </summary>
public class SendSms2FACodeRequest
{
    /// <summary>
    /// Temporary token received from login response when 2FA is required
    /// </summary>
    [Required(ErrorMessage = "TempToken is required")]
    public string TempToken { get; set; } = string.Empty;
}
