using MediatR;
using AuthService.Domain.Enums;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.Add2FAMethod;

public record Add2FAMethodCommand(
    string UserId,
    string Password,
    TwoFactorAuthType Method,
    string? PhoneNumber = null
) : IRequest<Change2FAMethodResponse>;
