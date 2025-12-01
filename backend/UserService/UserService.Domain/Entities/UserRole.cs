using System;

namespace UserService.Domain.Entities
{
    /// <summary>
    /// Entidad que vincula usuarios con roles (del RoleService)
    /// </summary>
    public class UserRole
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }  // NO FK - referencia a RoleService

        // Metadata de asignación
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string AssignedBy { get; set; } = "system";
        public DateTime? RevokedAt { get; set; }
        public string? RevokedBy { get; set; }
        public bool IsActive { get; set; } = true;

        // Navegación
        public User User { get; set; } = null!;
    }
}
