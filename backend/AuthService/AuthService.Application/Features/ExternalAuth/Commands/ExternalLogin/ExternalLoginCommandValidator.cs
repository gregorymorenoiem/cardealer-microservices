using AuthService.Application.DTOs.ExternalAuth;
using AuthService.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.ExternalAuth.Commands.ExternalLogin;

public class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, ExternalLoginResponse>
{
    private readonly IExternalAuthService _externalAuthService;
    private readonly ILogger<ExternalLoginCommandHandler> _logger;

    public ExternalLoginCommandHandler(
        IExternalAuthService externalAuthService,
        ILogger<ExternalLoginCommandHandler> logger)
    {
        _externalAuthService = externalAuthService;
        _logger = logger;
    }

    public Task<ExternalLoginResponse> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Generate authorization URL for the external provider
            var authorizationUrl = GenerateAuthorizationUrl(request.Provider, request.RedirectUri);
            var response = new ExternalLoginResponse(authorizationUrl);

            _logger.LogInformation("Generated authorization URL for provider {Provider}", request.Provider);

            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating authorization URL for provider {Provider}", request.Provider);
            throw;
        }
    }

    private string GenerateAuthorizationUrl(string provider, string redirectUri)
    {
        return provider.ToLower() switch
        {
            "google" => $"https://accounts.google.com/o/oauth2/v2/auth?client_id=YOUR_CLIENT_ID&redirect_uri={redirectUri}&response_type=code&scope=openid%20email%20profile",
            "microsoft" => $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=YOUR_CLIENT_ID&redirect_uri={redirectUri}&response_type=code&scope=openid%20email%20profile",
            "facebook" => $"https://www.facebook.com/v12.0/dialog/oauth?client_id=YOUR_CLIENT_ID&redirect_uri={redirectUri}&response_type=code&scope=email",
            _ => throw new ArgumentException($"Unsupported provider: {provider}")
        };
    }
}