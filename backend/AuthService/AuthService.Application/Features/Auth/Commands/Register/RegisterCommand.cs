using AuthService.Application.DTOs.Auth;
using MediatR;

namespace AuthService.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string UserName, string Email, string Password)
    : IRequest<RegisterResponse>;
