namespace AuthService.Domain.Exceptions;

public class InvalidEmailException : DomainException
{
    public string Email { get; }

    public InvalidEmailException(string email)
        : base($"The email '{email}' is not valid")
    {
        Email = email;
    }
}
