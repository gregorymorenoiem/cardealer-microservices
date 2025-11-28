using AuthService.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Infrastructure.Services.Identity;

public class PasswordHasher : IPasswordHasher
{
    private readonly IPasswordHasher<object> _passwordHasher;

    public PasswordHasher(IPasswordHasher<object> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        return _passwordHasher.HashPassword(null!, password);
    }

    public bool Verify(string providedPassword, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(providedPassword))
            return false;

        if (string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        var result = _passwordHasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}