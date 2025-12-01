namespace AuthService.Application.DTOs.ExternalAuth;

public record ExternalAuthCallbackRequest(
    string Provider,
    string? Code,
    string? IdToken,
    string? RedirectUri,
    string? State
);
