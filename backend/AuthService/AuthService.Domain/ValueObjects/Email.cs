using AuthService.Domain.Exceptions;

namespace AuthService.Domain.ValueObjects;

public record Email
{
    public string Value { get; }
    public string NormalizedValue { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty");

        if (!IsValidEmail(value))
            throw new InvalidEmailException(value);

        Value = value.Trim().ToLower();
        NormalizedValue = Value.ToUpperInvariant();
    }

    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string email) => new(email);

    public override string ToString() => Value;

    public string GetDomain()
    {
        var atIndex = Value.IndexOf('@');
        return atIndex > 0 ? Value[(atIndex + 1)..] : string.Empty;
    }

    public string GetUsername()
    {
        var atIndex = Value.IndexOf('@');
        return atIndex > 0 ? Value[..atIndex] : Value;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public override int GetHashCode() => Value.GetHashCode();

    public virtual bool Equals(Email? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }
}