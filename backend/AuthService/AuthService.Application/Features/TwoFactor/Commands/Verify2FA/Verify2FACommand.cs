using MediatR;
using AuthService.Domain.Enums;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.Verify2FA;

public record Verify2FACommand(string UserId, string Code, TwoFactorAuthType Type) : IRequest<Verify2FAResponse>;
