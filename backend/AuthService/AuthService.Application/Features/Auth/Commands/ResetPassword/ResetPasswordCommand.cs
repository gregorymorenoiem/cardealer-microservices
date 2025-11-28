using MediatR;
using AuthService.Application.DTOs.Auth;

namespace AuthService.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Token, string NewPassword, string ConfirmPassword)
    : IRequest<ResetPasswordResponse>;