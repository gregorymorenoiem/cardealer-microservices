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
    /// Comprador registrado (gratis).
    /// - Favoritos, alertas, comparación, contactar vendedores
    /// - NO puede publicar vehículos
    /// </summary>
    Buyer = 1,

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
    PlatformEmployee = 5,

    /// <summary>
    /// Vendedor individual registrado ($29/listing).
    /// - Puede publicar vehículos propios
    /// - Acceso a panel de vendedor
    /// </summary>
    Seller = 6
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
        accountType == AccountType.Seller ||
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
        AccountType.Buyer => "Comprador",
        AccountType.Dealer => "Dealer",
        AccountType.DealerEmployee => "Empleado de Dealer",
        AccountType.Admin => "Administrador",
        AccountType.PlatformEmployee => "Empleado de Plataforma",
        AccountType.Seller => "Vendedor Individual",
        _ => "Desconocido"
    };

    /// <summary>
    /// Obtiene la descripción del tipo de cuenta.
    /// </summary>
    public static string GetDescription(this AccountType accountType) => accountType switch
    {
        AccountType.Guest => "Usuario no registrado con acceso limitado a navegación",
        AccountType.Buyer => "Comprador registrado que busca y compra vehículos",
        AccountType.Dealer => "Propietario de concesionario con suscripción activa",
        AccountType.DealerEmployee => "Empleado de concesionario con permisos delegados",
        AccountType.Admin => "Administrador de la plataforma con rol específico",
        AccountType.PlatformEmployee => "Empleado operativo de la plataforma OKLA",
        AccountType.Seller => "Vendedor individual que publica vehículos propios",
        _ => "Tipo de cuenta desconocido"
    };
}
