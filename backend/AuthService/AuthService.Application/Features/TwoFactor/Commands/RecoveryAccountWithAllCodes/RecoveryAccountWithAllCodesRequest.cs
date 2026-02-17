using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.Features.TwoFactor.Commands.RecoveryAccountWithAllCodes;

/// <summary>
/// Request DTO for full account recovery using all 10 recovery codes.
/// 
/// Use Case: User lost access to authenticator AND individual recovery codes expired.
/// Solution: Provide ALL 10 original codes to prove account ownership.
/// </summary>
public class RecoveryAccountWithAllCodesRequest
{
    /// <summary>
    /// Temporary token received from login (when 2FA was required)
    /// </summary>
    [Required]
    public string TempToken { get; set; } = string.Empty;

    /// <summary>
    /// All 10 recovery codes that were generated when 2FA was enabled.
    /// Must provide ALL codes exactly as they were given (order doesn't matter).
    /// 
    /// Format: Array of 8-character alphanumeric codes.
    /// Example: ["H29S41MV", "5LEA3VJY", "ZEW64PCR", ...]
    /// </summary>
    [Required]
    [MinLength(10, ErrorMessage = "Must provide exactly 10 recovery codes")]
    [MaxLength(10, ErrorMessage = "Must provide exactly 10 recovery codes")]
    public List<string> RecoveryCodes { get; set; } = new();
}
