using AdminService.Domain.Enums;

namespace AdminService.Domain.Entities;

/// <summary>
/// Empleado de la plataforma OKLA
/// </summary>
public class PlatformEmployee
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AdminRole PlatformRole { get; set; }
    public string? Permissions { get; set; } // Comma-separated permissions
    public string? Department { get; set; }
    public Guid AssignedBy { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public DateTime HireDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public PlatformUser? User { get; set; }
}

/// <summary>
/// Usuario básico de plataforma (referencia a AuthService/UserService)
/// </summary>
public class PlatformUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Invitación para empleado de plataforma
/// </summary>
public class PlatformEmployeeInvitation
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public AdminRole PlatformRole { get; set; }
    public string? Permissions { get; set; }
    public string? Department { get; set; }
    public Guid InvitedBy { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime InvitationDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpirationDate { get; set; }
    public DateTime? AcceptedDate { get; set; }
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// Registro de actividad de un empleado de plataforma
/// </summary>
public class EmployeeActivity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TargetType { get; set; }
    public string? TargetId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Metadata { get; set; } // JSON
}

/// <summary>
/// Estado de empleado
/// </summary>
public enum EmployeeStatus
{
    Pending,
    Active,
    Suspended,
    Terminated
}

/// <summary>
/// Estado de invitación
/// </summary>
public enum InvitationStatus
{
    Pending,
    Accepted,
    Expired,
    Revoked
}
