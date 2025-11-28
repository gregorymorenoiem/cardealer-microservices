using MediatR;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.Disable2FA;

public record Disable2FACommand(string UserId, string Password) : IRequest<Verify2FAResponse>;