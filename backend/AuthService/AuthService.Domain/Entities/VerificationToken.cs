using AuthService.Domain.Enums;
using System; // <--- Asegúrate de tener este using

namespace AuthService.Domain.Entities;

public class VerificationToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public VerificationTokenType Type { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public string UserId { get; set; } = string.Empty;

    // Asegúrate de que esta propiedad existe y es pública
    public virtual ApplicationUser User { get; set; } = null!;

    // Constructor vacío (necesario para EF Core y los Factory Methods)
    public VerificationToken()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    // --- ESTE ES EL CONSTRUCTOR QUE FALTABA ---
    public VerificationToken(string userId, VerificationTokenType type, TimeSpan expiresIn) : this()
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId), "User ID cannot be empty when creating a verification token.");

        UserId = userId;
        Type = type;
        ExpiresAt = DateTime.UtcNow.Add(expiresIn);

        byte[] tokenData = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenData);
        }
        Token = Convert.ToBase64String(tokenData)
                       .Replace('+', '-')
                       .Replace('/', '_')
                       .TrimEnd('=');
    }

    // Factory methods (Estos ahora usan el constructor vacío y asignan propiedades)
    // Nota: Podrían modificarse para usar el nuevo constructor si tiene sentido
    public static VerificationToken CreateEmailVerificationToken(string email, string userId, int expiryHours = 24)
    {
        var token = new VerificationToken(userId, VerificationTokenType.EmailVerification, TimeSpan.FromHours(expiryHours));
        token.Email = email; // Asigna el email si aún lo necesitas por separado
        // El token ya se genera en el constructor
        return token;
    }

    public static VerificationToken CreatePasswordResetToken(string email, string userId, int expiryHours = 1)
    {
        var token = new VerificationToken(userId, VerificationTokenType.PasswordReset, TimeSpan.FromHours(expiryHours));
        token.Email = email; // Asigna el email si aún lo necesitas por separado
                             // El token ya se genera en el constructor
        return token;
    }

    // Business methods
    public bool IsValid()
    {
        return !IsUsed && ExpiresAt > DateTime.UtcNow;
    }

    public void MarkAsUsed()
    {
        if (IsUsed) return; // Evitar marcar como usado múltiples veces
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpiresAt;
    }
}