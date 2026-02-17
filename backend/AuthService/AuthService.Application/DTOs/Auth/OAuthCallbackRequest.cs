namespace AuthService.Application.DTOs.Auth;

public record OAuthCallbackRequest(string Code, string RedirectUri);
