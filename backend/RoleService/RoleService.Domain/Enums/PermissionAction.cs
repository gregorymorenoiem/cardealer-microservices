namespace RoleService.Domain.Enums;

/// <summary>
/// Acciones permitidas en el sistema RBAC.
/// Estas acciones se combinan con recursos para formar permisos (ej: vehicles:create)
/// </summary>
public enum PermissionAction
{
    /// <summary>Crear recursos</summary>
    Create = 1,

    /// <summary>Leer/Ver recursos</summary>
    Read = 2,

    /// <summary>Actualizar recursos</summary>
    Update = 3,

    /// <summary>Eliminar recursos</summary>
    Delete = 4,

    /// <summary>Ejecutar acciones</summary>
    Execute = 5,

    /// <summary>Publicar/Despublicar</summary>
    Publish = 6,

    /// <summary>Destacar/Feature</summary>
    Feature = 7,

    /// <summary>Importar en batch</summary>
    Import = 8,

    /// <summary>Exportar datos</summary>
    Export = 9,

    /// <summary>Banear usuarios</summary>
    Ban = 10,

    /// <summary>Verificar identidad/documentos</summary>
    Verify = 11,

    /// <summary>Suspender recursos</summary>
    Suspend = 12,

    /// <summary>Reembolsar pagos</summary>
    Refund = 13,

    /// <summary>Gestionar planes</summary>
    ManagePlans = 14,

    /// <summary>Programar tareas</summary>
    Schedule = 15,

    /// <summary>Aprobar solicitudes</summary>
    Approve = 16,

    /// <summary>Rechazar solicitudes</summary>
    Reject = 17,

    /// <summary>Escalar casos</summary>
    Escalate = 18,

    /// <summary>Limpiar/Clear</summary>
    Clear = 19,

    /// <summary>Acceso general</summary>
    Access = 20,

    /// <summary>Gestionar roles</summary>
    ManageRoles = 21,

    /// <summary>Gestionar permisos</summary>
    ManagePermissions = 22,

    /// <summary>Asignar roles a usuarios</summary>
    AssignRoles = 23,

    /// <summary>Solicitar documentos</summary>
    RequestDocuments = 24,

    /// <summary>Crear STR (Suspicious Transaction Report)</summary>
    CreateStr = 25,

    /// <summary>Todos los permisos sobre el recurso</summary>
    All = 99
}
