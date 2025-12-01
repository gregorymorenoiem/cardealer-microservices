using AuthService.Application.DTOs.Auth;
using MediatR;

namespace AuthService.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;
