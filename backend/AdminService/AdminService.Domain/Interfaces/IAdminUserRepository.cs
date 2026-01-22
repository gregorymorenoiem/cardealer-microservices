using AdminService.Domain.Entities;
using AdminService.Domain.Enums;

namespace AdminService.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de usuarios administradores.
/// </summary>
public interface IAdminUserRepository
{
    // =========================================================================
    // Basic CRUD Operations
    // =========================================================================

    /// <summary>
    /// Obtiene un administrador por su ID.
    /// </summary>
    Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un administrador por el ID del usuario vinculado.
    /// </summary>
    Task<AdminUser?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un administrador por email.
    /// </summary>
    Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un nuevo administrador.
    /// </summary>
    Task<AdminUser> CreateAsync(AdminUser adminUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un administrador existente.
    /// </summary>
    Task<AdminUser> UpdateAsync(AdminUser adminUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un administrador (soft delete - desactiva).
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // =========================================================================
    // Query Operations
    // =========================================================================

    /// <summary>
    /// Obtiene todos los administradores con paginación.
    /// </summary>
    Task<(IEnumerable<AdminUser> Items, int TotalCount)> GetAllAsync(
        int page = 1,
        int pageSize = 20,
        AdminRole? roleFilter = null,
        bool? isActiveFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los administradores por rol.
    /// </summary>
    Task<IEnumerable<AdminUser>> GetByRoleAsync(AdminRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los administradores activos.
    /// </summary>
    Task<IEnumerable<AdminUser>> GetActiveAdminsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el conteo de administradores por rol.
    /// </summary>
    Task<Dictionary<AdminRole, int>> GetAdminCountByRoleAsync(CancellationToken cancellationToken = default);

    // =========================================================================
    // Permission Operations
    // =========================================================================

    /// <summary>
    /// Verifica si un administrador tiene un permiso específico.
    /// </summary>
    Task<bool> HasPermissionAsync(Guid adminId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los permisos efectivos de un administrador.
    /// </summary>
    Task<IEnumerable<string>> GetEffectivePermissionsAsync(Guid adminId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega permisos personalizados a un administrador.
    /// </summary>
    Task<bool> AddCustomPermissionsAsync(Guid adminId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoca permisos específicos de un administrador.
    /// </summary>
    Task<bool> RevokePermissionsAsync(Guid adminId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);

    // =========================================================================
    // Authentication & Security
    // =========================================================================

    /// <summary>
    /// Registra un login exitoso.
    /// </summary>
    Task RecordSuccessfulLoginAsync(Guid adminId, string ipAddress, string userAgent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra un intento de login fallido.
    /// </summary>
    Task RecordFailedLoginAsync(Guid adminId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Desbloquea una cuenta de administrador.
    /// </summary>
    Task<bool> UnlockAccountAsync(Guid adminId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un administrador puede acceder al sistema.
    /// </summary>
    Task<bool> CanAccessAsync(Guid adminId, CancellationToken cancellationToken = default);

    // =========================================================================
    // MFA Operations
    // =========================================================================

    /// <summary>
    /// Habilita MFA para un administrador.
    /// </summary>
    Task<bool> EnableMfaAsync(Guid adminId, MfaMethod method, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deshabilita MFA para un administrador.
    /// </summary>
    Task<bool> DisableMfaAsync(Guid adminId, CancellationToken cancellationToken = default);

    // =========================================================================
    // Validation
    // =========================================================================

    /// <summary>
    /// Verifica si existe un administrador con el email dado.
    /// </summary>
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si ya existe un administrador para el usuario dado.
    /// </summary>
    Task<bool> ExistsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
