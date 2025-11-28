using MediatR;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.GenerateRecoveryCodes;

public record GenerateRecoveryCodesCommand(string UserId, string Password) : IRequest<GenerateRecoveryCodesResponse>;