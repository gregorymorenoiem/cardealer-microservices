// AuthService.Application/DTOs/ForgotPasswordResponse.cs
namespace AuthService.Application.DTOs.Auth;

public record ForgotPasswordResponse(bool Success, string Message);