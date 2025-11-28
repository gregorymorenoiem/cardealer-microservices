using AuthService.Application.DTOs.ExternalAuth;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using AuthService.Application.Features.ExternalAuth.Commands.ExternalAuth;

namespace AuthService.Application.Features.ExternalAuth.Commands.ExternalAuthCallback;

public class ExternalAuthCallbackCommandHandler : IRequestHandler<ExternalAuthCallbackCommand, ExternalAuthResponse>
{
    private readonly IExternalAuthService _externalAuthService;
    private readonly ILogger<ExternalAuthCallbackCommandHandler> _logger;

    public ExternalAuthCallbackCommandHandler(
        IExternalAuthService externalAuthService,
        ILogger<ExternalAuthCallbackCommandHandler> logger)
    {
        _externalAuthService = externalAuthService;
        _logger = logger;
    }

    public async Task<ExternalAuthResponse> Handle(ExternalAuthCallbackCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate provider
            if (!Enum.TryParse<ExternalAuthProvider>(request.Provider, true, out var provider))
                throw new ArgumentException($"Unsupported provider: {request.Provider}");

            string idToken;

            // If we have a code, we need to exchange it for an ID token
            if (!string.IsNullOrEmpty(request.Code))
            {
                idToken = await ExchangeCodeForIdToken(provider, request.Code, request.RedirectUri);
            }
            else if (!string.IsNullOrEmpty(request.IdToken))
            {
                idToken = request.IdToken;
            }
            else
            {
                throw new ArgumentException("Either Code or IdToken must be provided");
            }

            // Use the existing ExternalAuthCommand flow
            var authCommand = new ExternalAuthCommand(
                request.Provider, idToken);

            // Since we can't directly call another handler, we'll simulate the flow
            // In a real scenario, you might refactor to use a shared service
            var (user, isNewUser) = await _externalAuthService.AuthenticateAsync(provider, idToken);

            _logger.LogInformation("External auth callback processed successfully for user {UserId}",
                user.Id);

            // Return a response (you would generate actual tokens here)
            return new ExternalAuthResponse(
                user.Id,
                user.UserName!,
                user.Email!,
                "access_token_placeholder", // Generate actual token
                "refresh_token_placeholder", // Generate actual token
                DateTime.UtcNow.AddHours(1),
                isNewUser
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing external auth callback for provider {Provider}",
                request.Provider);
            throw;
        }
    }

    private async Task<string> ExchangeCodeForIdToken(ExternalAuthProvider provider, string code, string? redirectUri)
    {
        // Implement OAuth code exchange logic here
        // This would make a server-side call to the provider's token endpoint
        // For now, return a placeholder
        _logger.LogInformation("Exchanging code for ID token for provider {Provider}", provider);

        // TODO: Implement actual OAuth code exchange
        await Task.Delay(100); // Simulate async work

        return $"id_token_placeholder_for_{provider}";
    }
}