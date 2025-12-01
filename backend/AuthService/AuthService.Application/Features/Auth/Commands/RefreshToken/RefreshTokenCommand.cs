using AuthService.Application.DTOs.Auth;
using MediatR;

namespace AuthService.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResponse>;
