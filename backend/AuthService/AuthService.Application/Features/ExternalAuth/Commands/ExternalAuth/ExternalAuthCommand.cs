using AuthService.Application.DTOs.ExternalAuth;
using MediatR;

namespace AuthService.Application.Features.ExternalAuth.Commands.ExternalAuth;


public record ExternalAuthCommand(string Provider, string IdToken) : IRequest<ExternalAuthResponse>;