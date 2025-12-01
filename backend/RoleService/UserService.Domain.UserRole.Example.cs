using System;

namespace UserService.Domain.Entities
{
    /// <summary>
    /// Entidad que vincula usuarios con roles
    /// </summary>
    public class UserRole
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        // Metadata de asignación
        public DateTime AssignedAt { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
        public DateTime? RevokedAt { get; set; }
        public string? RevokedBy { get; set; }
        public bool IsActive { get; set; } = true;

        // Navegación a User (si existe en el dominio)
        // public User User { get; set; }

        // Nota: RoleId es un Guid que referencia al rol en RoleService
        // No hay FK directa porque está en otro microservicio
    }
}
