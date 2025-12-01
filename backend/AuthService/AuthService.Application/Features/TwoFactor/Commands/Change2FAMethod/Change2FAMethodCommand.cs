using MediatR;
using AuthService.Domain.Enums;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.Change2FAMethod;

public record Change2FAMethodCommand(
    string UserId,
    string Password,
    TwoFactorAuthType NewMethod,
    string? PhoneNumber = null
) : IRequest<Change2FAMethodResponse>;
