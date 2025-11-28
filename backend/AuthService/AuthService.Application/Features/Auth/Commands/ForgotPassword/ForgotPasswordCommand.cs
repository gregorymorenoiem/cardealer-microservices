using MediatR;
using AuthService.Application.DTOs.Auth;

namespace AuthService.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResponse>;