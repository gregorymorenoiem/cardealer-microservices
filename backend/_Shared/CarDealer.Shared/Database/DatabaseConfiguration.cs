namespace CarDealer.Shared.Database;

/// <summary>
/// Configuración del proveedor de base de datos desde appsettings.json
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// Proveedor de base de datos activo (PostgreSQL, SqlServer, MySQL, Oracle, InMemory)
    /// </summary>
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.PostgreSQL;

    /// <summary>
    /// Connection strings para cada proveedor
    /// </summary>
    public Dictionary<string, string> ConnectionStrings { get; set; } = new();

    /// <summary>
    /// Aplicar migraciones automáticamente al iniciar (default: false en producción)
    /// </summary>
    public bool AutoMigrate { get; set; } = false;

    /// <summary>
    /// Timeout para comandos de base de datos (segundos)
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Número máximo de reintentos en caso de error transitorio
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Delay máximo entre reintentos (segundos)
    /// </summary>
    public int MaxRetryDelay { get; set; } = 30;

    /// <summary>
    /// Habilitar logging de datos sensibles (solo desarrollo)
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Habilitar errores detallados (solo desarrollo)
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Obtiene el connection string para el proveedor activo
    /// </summary>
    public string GetConnectionString()
    {
        var key = Provider.ToString();

        if (ConnectionStrings.TryGetValue(key, out var connectionString))
        {
            return connectionString;
        }

        throw new InvalidOperationException(
            $"No se encontró connection string para el proveedor '{Provider}'. " +
            $"Verifique la sección 'Database.ConnectionStrings.{key}' en appsettings.json");
    }
}
