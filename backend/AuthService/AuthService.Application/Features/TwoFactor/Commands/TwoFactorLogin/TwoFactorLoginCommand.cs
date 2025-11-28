using MediatR;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Commands.TwoFactorLogin;

public record TwoFactorLoginCommand(string TempToken, string TwoFactorCode) : IRequest<TwoFactorLoginResponse>;
