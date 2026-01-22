namespace AuthService.Domain.Enums;

/// <summary>
/// Representa el tipo de cuenta de usuario en la plataforma OKLA.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Usuario invitado (no registrado).
    /// Solo puede navegar y buscar. No puede guardar favoritos ni contactar vendedores.
    /// </summary>
    Guest = 0,

    /// <summary>
    /// Usuario individual registrado.
    /// Puede ser comprador (gratis) o vendedor ($29/listing).
    /// - Comprador: Favoritos, alertas, comparación, contactar vendedores
    /// - Vendedor: Publicar vehículos propios
    /// </summary>
    Individual = 1,

    /// <summary>
    /// Propietario de concesionario (Dealer).
    /// Requiere suscripción mensual ($49-$299/mes).
    /// - Acceso a dashboard de dealer
    /// - Gestión de inventario
    /// - Analytics
    /// - CRM de leads
    /// </summary>
    Dealer = 2,

    /// <summary>
    /// Empleado de un concesionario.
    /// Acceso limitado según rol asignado por el Dealer.
    /// - Puede gestionar inventario y leads (según permisos)
    /// - NO puede acceder a billing ni gestión de empleados
    /// </summary>
    DealerEmployee = 3,

    /// <summary>
    /// Administrador de la plataforma.
    /// Tiene un AdminRole específico que define sus permisos exactos:
    /// - SuperAdmin: Acceso total
    /// - PlatformAdmin: Configuración y mantenimiento
    /// - ModerationAdmin: Moderación de contenido
    /// - SupportAdmin: Soporte al cliente
    /// - AnalyticsAdmin: Solo lectura de analytics
    /// </summary>
    Admin = 4,

    /// <summary>
    /// Empleado de la plataforma OKLA (staff operativo).
    /// Puede tener permisos específicos pero NO es administrador.
    /// - Soporte nivel 1
    /// - Operaciones de contenido
    /// - NO puede moderar ni aprobar contenido
    /// </summary>
    PlatformEmployee = 5
}

/// <summary>
/// Extensiones para el enum AccountType.
/// </summary>
public static class AccountTypeExtensions
{
    /// <summary>
    /// Indica si el tipo de cuenta puede publicar vehículos.
    /// </summary>
    public static bool CanPublishListings(this AccountType accountType) =>
        accountType == AccountType.Individual ||
        accountType == AccountType.Dealer ||
        accountType == AccountType.DealerEmployee;

    /// <summary>
    /// Indica si el tipo de cuenta requiere verificación de dealer.
    /// </summary>
    public static bool RequiresDealerVerification(this AccountType accountType) =>
        accountType == AccountType.Dealer ||
        accountType == AccountType.DealerEmployee;

    /// <summary>
    /// Indica si el tipo de cuenta es de administración.
    /// </summary>
    public static bool IsAdministrative(this AccountType accountType) =>
        accountType == AccountType.Admin ||
        accountType == AccountType.PlatformEmployee;

    /// <summary>
    /// Indica si el tipo de cuenta puede acceder al panel de admin.
    /// </summary>
    public static bool CanAccessAdminPanel(this AccountType accountType) =>
        accountType == AccountType.Admin;

    /// <summary>
    /// Indica si el tipo de cuenta puede acceder al panel de dealer.
    /// </summary>
    public static bool CanAccessDealerPanel(this AccountType accountType) =>
        accountType == AccountType.Dealer ||
        accountType == AccountType.DealerEmployee;

    /// <summary>
    /// Obtiene el nombre descriptivo en español.
    /// </summary>
    public static string GetDisplayName(this AccountType accountType) => accountType switch
    {
        AccountType.Guest => "Invitado",
        AccountType.Individual => "Individual",
        AccountType.Dealer => "Dealer",
        AccountType.DealerEmployee => "Empleado de Dealer",
        AccountType.Admin => "Administrador",
        AccountType.PlatformEmployee => "Empleado de Plataforma",
        _ => "Desconocido"
    };

    /// <summary>
    /// Obtiene la descripción del tipo de cuenta.
    /// </summary>
    public static string GetDescription(this AccountType accountType) => accountType switch
    {
        AccountType.Guest => "Usuario no registrado con acceso limitado a navegación",
        AccountType.Individual => "Usuario registrado que compra o vende vehículos personales",
        AccountType.Dealer => "Propietario de concesionario con suscripción activa",
        AccountType.DealerEmployee => "Empleado de concesionario con permisos delegados",
        AccountType.Admin => "Administrador de la plataforma con rol específico",
        AccountType.PlatformEmployee => "Empleado operativo de la plataforma OKLA",
        _ => "Tipo de cuenta desconocido"
    };
}
