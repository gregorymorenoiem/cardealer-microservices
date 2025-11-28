
namespace AuthService.Application.DTOs.PhoneVerification;

public record VerifyPhoneRequest(string PhoneNumber, string VerificationCode);