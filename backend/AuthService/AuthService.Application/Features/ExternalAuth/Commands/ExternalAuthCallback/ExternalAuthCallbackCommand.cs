using AuthService.Application.DTOs.ExternalAuth;
using MediatR;

namespace AuthService.Application.Features.ExternalAuth.Commands.ExternalAuthCallback;

public record ExternalAuthCallbackCommand(
    string Provider,
    string? Code,
    string? IdToken,
    string? RedirectUri,
    string? State)
    : IRequest<ExternalAuthResponse>;
