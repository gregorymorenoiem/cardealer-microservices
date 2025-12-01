using MediatR;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.VerifyRecoveryCode;

public record VerifyRecoveryCodeCommand(string UserId, string Code) : IRequest<Verify2FAResponse>;
