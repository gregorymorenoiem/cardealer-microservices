using AuthService.Domain.Exceptions;

namespace AuthService.Domain.ValueObjects;

public record Password
{
    public string Hash { get; }
    public bool IsHashed { get; }

    private Password(string value, bool isHashed)
    {
        Hash = value;
        IsHashed = isHashed;
    }

    public static Password CreatePlain(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            throw new DomainException("Password cannot be empty");

        if (plainPassword.Length < 8)
            throw new DomainException("Password must be at least 8 characters long");

        if (!HasRequiredComplexity(plainPassword))
            throw new DomainException("Password must contain at least one uppercase letter, one lowercase letter, and one number");

        return new Password(plainPassword, false);
    }

    public static Password CreateHashed(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Password hash cannot be empty");

        return new Password(hash, true);
    }

    private static bool HasRequiredComplexity(string password)
    {
        return password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit);
    }

    public override string ToString() => IsHashed ? "[HASHED]" : "[PLAIN]";

    public override int GetHashCode() => Hash.GetHashCode();

    public virtual bool Equals(Password? other)
    {
        if (other is null) return false;
        return Hash == other.Hash && IsHashed == other.IsHashed;
    }
}
