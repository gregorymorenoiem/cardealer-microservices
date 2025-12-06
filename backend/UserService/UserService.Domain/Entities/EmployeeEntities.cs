using System;

namespace UserService.Domain.Entities
{
    /// <summary>
    /// Empleado de un dealer
    /// </summary>
    public class DealerEmployee
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid DealerId { get; set; }
        public DealerRole DealerRole { get; set; }
        public string Permissions { get; set; } = "[]"; // JSON array
        public Guid InvitedBy { get; set; }
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Pending;
        public DateTime InvitationDate { get; set; } = DateTime.UtcNow;
        public DateTime? ActivationDate { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
    }

    /// <summary>
    /// Empleado de la plataforma
    /// </summary>
    public class PlatformEmployee
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public PlatformRole PlatformRole { get; set; }
        public string Permissions { get; set; } = "[]"; // JSON array
        public Guid AssignedBy { get; set; }
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        public string? Department { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
    }

    /// <summary>
    /// Estado de empleado
    /// </summary>
    public enum EmployeeStatus
    {
        Pending,
        Active,
        Suspended
    }

    /// <summary>
    /// Invitación para empleado de dealer
    /// </summary>
    public class DealerEmployeeInvitation
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DealerRole DealerRole { get; set; }
        public string Permissions { get; set; } = "[]";
        public Guid InvitedBy { get; set; }
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
        public DateTime InvitationDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpirationDate { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public string Token { get; set; } = string.Empty; // Token único para aceptar invitación
    }

    /// <summary>
    /// Invitación para empleado de plataforma
    /// </summary>
    public class PlatformEmployeeInvitation
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public PlatformRole PlatformRole { get; set; }
        public string Permissions { get; set; } = "[]";
        public Guid InvitedBy { get; set; }
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
        public DateTime InvitationDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpirationDate { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public string Token { get; set; } = string.Empty;
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
}
