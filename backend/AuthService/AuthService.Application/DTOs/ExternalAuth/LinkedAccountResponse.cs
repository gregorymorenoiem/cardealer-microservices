namespace AuthService.Application.DTOs.ExternalAuth;

public record LinkedAccountResponse(
    string Provider,
    string ExternalUserId,
    string Email,
    DateTime LinkedAt
);