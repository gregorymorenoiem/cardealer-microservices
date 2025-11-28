using MediatR;
using AuthService.Domain.Enums;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.Enable2FA;

public record Enable2FACommand(string UserId, TwoFactorAuthType Type) : IRequest<Enable2FAResponse>;