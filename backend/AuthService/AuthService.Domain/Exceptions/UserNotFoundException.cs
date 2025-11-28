namespace AuthService.Domain.Exceptions;

public class UserNotFoundException : DomainException
{
    public string UserId { get; }
    public string? Email { get; }

    public UserNotFoundException(string userId)
        : base($"User with ID '{userId}' was not found")
    {
        UserId = userId;
    }

    public UserNotFoundException(string userId, string email)
        : base($"User with ID '{userId}' and email '{email}' was not found")
    {
        UserId = userId;
        Email = email;
    }
}