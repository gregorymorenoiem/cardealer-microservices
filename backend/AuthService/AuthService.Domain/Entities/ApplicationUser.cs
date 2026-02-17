using Microsoft.AspNetCore.Identity;
using AuthService.Domain.Common;
using AuthService.Domain.Events;
using AuthService.Domain.Exceptions;
using AuthService.Domain.Interfaces.Services;
using AuthService.Domain.Enums;
using MediatR;

namespace AuthService.Domain.Entities;

public class ApplicationUser : IdentityUser, IAggregateRoot
{
    private readonly List<INotification> _domainEvents = new();

    // Navigation properties
    public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public virtual ICollection<VerificationToken> VerificationTokens { get; private set; } = new List<VerificationToken>();

    // Relación 1:1 con TwoFactorAuth
    public virtual TwoFactorAuth? TwoFactorAuth { get; private set; }

    // Propiedades adicionales
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Propiedad computada para 2FA
    public bool IsTwoFactorEnabled => TwoFactorAuth != null && TwoFactorAuth.Status == TwoFactorAuthStatus.Enabled;

    // Domain Events
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(INotification eventItem) => _domainEvents.Add(eventItem);
    public void RemoveDomainEvent(INotification eventItem) => _domainEvents.Remove(eventItem);
    public void ClearDomainEvents() => _domainEvents.Clear();
    public void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;

    // Multi-tenancy: DealerId
    public string? DealerId { get; set; }

    // Account type for role-based access
    public AccountType AccountType { get; set; } = AccountType.Buyer;

    // User intent - what the user wants to do (buy, sell, both)
    public UserIntent UserIntent { get; set; } = UserIntent.Browse;

    // Profile information
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }

    // Nuevas propiedades para autenticación externa
    public ExternalAuthProvider? ExternalAuthProvider { get; private set; }
    public string? ExternalUserId { get; private set; }
    public bool IsExternalUser => ExternalAuthProvider.HasValue;


    // EF Core constructor
    protected ApplicationUser() : base() { }

    public ApplicationUser(string userName, string email, string passwordHash) : base(userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("Username cannot be empty");

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            throw new InvalidEmailException(email);

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash cannot be empty");

        Email = email.ToLower().Trim();
        NormalizedEmail = Email.ToUpperInvariant();
        PasswordHash = passwordHash;
        EmailConfirmed = false;
        AccessFailedCount = 0;
        LockoutEnabled = true;

        AddDomainEvent(new UserRegisteredEvent(Id, Email, UserName!));
    }

    // Métodos para 2FA
    public void EnableTwoFactorAuth(TwoFactorAuth twoFactorAuth)
    {
        TwoFactorAuth = twoFactorAuth;
        MarkAsUpdated();
    }

    public void DisableTwoFactorAuth()
    {
        TwoFactorAuth = null;
        MarkAsUpdated();
    }

    public void ConfirmEmail()
    {
        if (EmailConfirmed)
            throw new DomainException("Email is already confirmed");

        EmailConfirmed = true;
        AddDomainEvent(new EmailConfirmedEvent(Id, Email!));
    }

    public void UpdatePassword(string newPlainPassword, IPasswordHasher passwordHasher)
    {
        if (string.IsNullOrWhiteSpace(newPlainPassword))
            throw new DomainException("Password cannot be empty");

        if (string.IsNullOrWhiteSpace(PasswordHash))
            throw new DomainException("No password is set for this user");

        if (passwordHasher.Verify(newPlainPassword, PasswordHash))
            throw new DomainException("New password must be different from current password");

        PasswordHash = passwordHasher.Hash(newPlainPassword);
        SecurityStamp = Guid.NewGuid().ToString();
        MarkAsUpdated();
        AddDomainEvent(new PasswordChangedEvent(Id));
    }

    /// <summary>
    /// Security: Configurable lockout parameters.
    /// Read from SecuritySettings in production; defaults here are conservative.
    /// </summary>
    private const int DefaultMaxFailedAttempts = 5;
    private const int DefaultLockoutMinutes = 30;

    public void IncrementAccessFailedCount(int? maxAttempts = null, int? lockoutMinutes = null)
    {
        AccessFailedCount++;

        var threshold = maxAttempts ?? DefaultMaxFailedAttempts;
        var duration = lockoutMinutes ?? DefaultLockoutMinutes;

        if (AccessFailedCount >= threshold)
        {
            LockoutEnabled = true;
            LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(duration);
        }
        MarkAsUpdated();
    }

    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
        MarkAsUpdated();
    }

    public bool IsLockedOut() => LockoutEnabled && LockoutEnd.HasValue && LockoutEnd > DateTimeOffset.UtcNow;

    public bool CheckPassword(string password, IPasswordHasher passwordHasher)
    {
        if (string.IsNullOrWhiteSpace(PasswordHash))
            throw new DomainException("No password is set for this user");

        return passwordHasher.Verify(password, PasswordHash);
    }

    public void AddRefreshToken(RefreshToken refreshToken) => RefreshTokens.Add(refreshToken);

    public void RevokeAllRefreshTokens(string reason = "user_action")
    {
        foreach (var token in RefreshTokens.Where(t => !t.IsRevoked))
            token.Revoke(reason, "system");
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

    // Método para crear usuario externo
    public static ApplicationUser CreateExternalUser(
        string userName,
        string email,
        ExternalAuthProvider provider,
        string externalUserId,
        string? firstName = null,
        string? lastName = null,
        string? profilePictureUrl = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("Username cannot be empty");

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            throw new InvalidEmailException(email);

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email.ToLower().Trim(),
            NormalizedEmail = email.ToUpperInvariant(),
            ExternalAuthProvider = provider,
            ExternalUserId = externalUserId,
            EmailConfirmed = true, // Los emails de proveedores externos están confirmados
            SecurityStamp = Guid.NewGuid().ToString(),
            FirstName = firstName,
            LastName = lastName,
            ProfilePictureUrl = profilePictureUrl
        };

        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email, user.UserName!));
        return user;
    }

    // Método para vincular cuenta externa a usuario existente
    public void LinkExternalAccount(ExternalAuthProvider provider, string externalUserId)
    {
        if (IsExternalUser)
            throw new DomainException("User already has an external account linked");

        ExternalAuthProvider = provider;
        ExternalUserId = externalUserId;
        MarkAsUpdated();
    }

    public void UnlinkExternalAccount()
    {
        if (!IsExternalUser)
            throw new DomainException("User does not have a linked external account.");

        ExternalAuthProvider = null;
        ExternalUserId = null;
        MarkAsUpdated();

    }




}
