namespace AuthService.Application.DTOs.ExternalAuth;

public record ExternalLoginRequest(string Provider, string RedirectUri);