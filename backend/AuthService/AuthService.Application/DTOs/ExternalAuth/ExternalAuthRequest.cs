namespace AuthService.Application.DTOs.ExternalAuth;

public record ExternalAuthRequest(string Provider, string IdToken);
