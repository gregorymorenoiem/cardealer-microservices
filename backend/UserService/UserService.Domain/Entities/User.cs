using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities
{
    /// <summary>
    /// Entidad de dominio User
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool EmailConfirmed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navegación a roles
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        // Propiedades calculadas
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
