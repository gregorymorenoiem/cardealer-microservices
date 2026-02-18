using System.ComponentModel.DataAnnotations;

namespace RoleService.Application.DTOs.RolePermissions;

/// <summary>
/// Request para asignar o remover un permiso de un rol.
/// </summary>
public record AssignPermissionRequest
{
    /// <summary>
    /// ID del rol al que asignar/remover el permiso
    /// </summary>
    [Required]
    public Guid RoleId { get; init; }

    /// <summary>
    /// ID del permiso a asignar/remover
    /// </summary>
    [Required]
    public Guid PermissionId { get; init; }
}
