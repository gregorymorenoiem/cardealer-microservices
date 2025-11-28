namespace AuthService.Domain.ValueObjects;

public record ExternalUserInfo(
    string UserId,
    string Email,
    string Name,
    string FirstName,
    string LastName,
    string? PictureUrl,
    string Provider,
    Dictionary<string, object> AdditionalData
);