namespace RoleService.Application.DTOs.RolePermissions;

/// <summary>
/// Respuesta de asignación de permiso a un rol.
/// </summary>
public record AssignPermissionResponse
{
    /// <summary>
    /// Si la operación fue exitosa
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// ID del rol al que se asignó el permiso
    /// </summary>
    public Guid RoleId { get; init; }

    /// <summary>
    /// Nombre del rol al que se asignó el permiso
    /// </summary>
    public string RoleName { get; init; } = string.Empty;

    /// <summary>
    /// ID del permiso asignado
    /// </summary>
    public Guid PermissionId { get; init; }

    /// <summary>
    /// Nombre del permiso asignado (formato: resource:action)
    /// </summary>
    public string PermissionName { get; init; } = string.Empty;

    /// <summary>
    /// Fecha y hora de la asignación
    /// </summary>
    public DateTime AssignedAt { get; init; }

    /// <summary>
    /// ID del usuario que realizó la asignación
    /// </summary>
    public string AssignedBy { get; init; } = string.Empty;
}

/// <summary>
/// Respuesta de remoción de permiso de un rol.
/// </summary>
public record RemovePermissionResponse
{
    /// <summary>
    /// Si la operación fue exitosa
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// ID del rol del que se removió el permiso
    /// </summary>
    public Guid RoleId { get; init; }

    /// <summary>
    /// Nombre del rol del que se removió el permiso
    /// </summary>
    public string RoleName { get; init; } = string.Empty;

    /// <summary>
    /// ID del permiso removido
    /// </summary>
    public Guid PermissionId { get; init; }

    /// <summary>
    /// Nombre del permiso removido (formato: resource:action)
    /// </summary>
    public string PermissionName { get; init; } = string.Empty;

    /// <summary>
    /// Fecha y hora de la remoción
    /// </summary>
    public DateTime RemovedAt { get; init; }

    /// <summary>
    /// ID del usuario que realizó la remoción
    /// </summary>
    public string RemovedBy { get; init; } = string.Empty;
}
