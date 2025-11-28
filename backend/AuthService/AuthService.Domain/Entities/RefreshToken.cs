using AuthService.Domain.Common;

namespace AuthService.Domain.Entities;

public class RefreshToken : EntityBase
{
    public string Token { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? RevokedReason { get; private set; }
    public string? ReplacedByToken { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    // Navigation property
    public virtual ApplicationUser User { get; private set; } = null!;

    // EF Core constructor
    private RefreshToken() { }


    // Este es el constructor correcto para RefreshToken
    public RefreshToken(string userId, string token, DateTime expiresAt, string createdByIp)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        Id = Guid.NewGuid().ToString(); // Asegúrate que EntityBase maneje la asignación de ID o hazla aquí
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
        // CreatedAt se asigna en EntityBase si lo heredas
    }

    public void Revoke(string revokedByIp, string reason = "revoked", string? replacedByToken = null)
    {
        if (IsRevoked)
            throw new InvalidOperationException("Token is already revoked");

        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        RevokedReason = reason;
        ReplacedByToken = replacedByToken;
        MarkAsUpdated(); // Si EntityBase tiene este método
    }

    // Nota: El método Rotate debería probablemente estar en un servicio, no en la entidad.
    // Pero si lo dejas aquí, asegúrate que funcione correctamente.
    public RefreshToken Rotate(string newToken, DateTime newExpiresAt, string createdByIp)
    {
        Revoke(createdByIp, "rotated", newToken);
        return new RefreshToken(UserId, newToken, newExpiresAt, createdByIp);
    }
}