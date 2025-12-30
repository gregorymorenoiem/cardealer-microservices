using CarDealer.Shared.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Database;

/// <summary>
/// Extensiones para configurar DbContext con múltiples proveedores de base de datos
/// usando secretos externalizados (variables de entorno o Docker Secrets).
/// </summary>
public static class DatabaseSecretExtensions
{
    /// <summary>
    /// Registra el DbContext usando secretos externalizados para la conexión.
    /// Esta es la forma recomendada para entornos de producción y CI/CD.
    /// </summary>
    /// <typeparam name="TContext">Tipo del DbContext a registrar</typeparam>
    /// <param name="services">Colección de servicios de DI</param>
    /// <param name="configuration">Configuración de la aplicación (para opciones no secretas)</param>
    /// <param name="secretProvider">Proveedor de secretos (null = usa CompositeSecretProvider por defecto)</param>
    /// <param name="servicePrefix">Prefijo para las variables de entorno del servicio (ej: "AUTH" -> AUTH_DATABASE_HOST)</param>
    /// <returns>ServiceCollection para chaining</returns>
    public static IServiceCollection AddDatabaseFromSecrets<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        ISecretProvider? secretProvider = null,
        string? servicePrefix = null)
        where TContext : DbContext
    {
        secretProvider ??= CompositeSecretProvider.CreateDefault();

        // Leer configuración base (no secretos) desde appsettings
        var configSection = configuration.GetSection("Database");
        var providerStr = configSection["Provider"] ?? "PostgreSQL";
        
        if (!Enum.TryParse<DatabaseProvider>(providerStr, ignoreCase: true, out var provider))
        {
            provider = DatabaseProvider.PostgreSQL;
        }

        var autoMigrate = configSection.GetValue("AutoMigrate", false);
        var commandTimeout = configSection.GetValue("CommandTimeout", 30);
        var maxRetryCount = configSection.GetValue("MaxRetryCount", 3);
        var maxRetryDelay = configSection.GetValue("MaxRetryDelay", 30);
        var enableSensitiveLogging = configSection.GetValue("EnableSensitiveDataLogging", false);
        var enableDetailedErrors = configSection.GetValue("EnableDetailedErrors", false);

        // Construir connection string desde secretos
        var connectionString = BuildConnectionString(secretProvider, provider, servicePrefix);

        // Logger para diagnóstico
        var tempProvider = services.BuildServiceProvider();
        var logger = tempProvider.GetService<ILogger<TContext>>();

        logger?.LogInformation(
            "Configuring DbContext {ContextType} with provider {Provider} from secrets",
            typeof(TContext).Name,
            provider);

        // Registrar DbContext
        services.AddDbContext<TContext>(options =>
        {
            ConfigureDatabaseProvider(options, provider, connectionString, 
                commandTimeout, maxRetryCount, maxRetryDelay, logger);

            if (enableSensitiveLogging)
            {
                options.EnableSensitiveDataLogging();
                logger?.LogWarning("Sensitive data logging is ENABLED. Do not use in production.");
            }

            if (enableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }
        });

        if (autoMigrate)
        {
            logger?.LogInformation("AutoMigrate is enabled. Migrations will be applied on startup.");
            services.AddHostedService<DatabaseMigrationService<TContext>>();
        }

        return services;
    }

    /// <summary>
    /// Construye el connection string desde secretos según el proveedor.
    /// </summary>
    private static string BuildConnectionString(
        ISecretProvider secrets,
        DatabaseProvider provider,
        string? servicePrefix)
    {
        var prefix = string.IsNullOrEmpty(servicePrefix) ? "" : servicePrefix;

        return provider switch
        {
            DatabaseProvider.PostgreSQL => ConnectionStringBuilder.BuildPostgreSqlConnectionString(secrets, prefix),
            DatabaseProvider.SqlServer => ConnectionStringBuilder.BuildSqlServerConnectionString(secrets, prefix),
            DatabaseProvider.InMemory => $"InMemory_{Guid.NewGuid()}",
            _ => ConnectionStringBuilder.BuildPostgreSqlConnectionString(secrets, prefix)
        };
    }

    /// <summary>
    /// Configura el proveedor de base de datos específico.
    /// </summary>
    private static void ConfigureDatabaseProvider<TContext>(
        DbContextOptionsBuilder options,
        DatabaseProvider provider,
        string connectionString,
        int commandTimeout,
        int maxRetryCount,
        int maxRetryDelay,
        ILogger<TContext>? logger)
        where TContext : DbContext
    {
        switch (provider)
        {
            case DatabaseProvider.PostgreSQL:
                logger?.LogInformation("Configuring PostgreSQL with Npgsql");
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: maxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelay),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(commandTimeout);
                    npgsqlOptions.MigrationsAssembly(GetMigrationsAssembly<TContext>());
                });
                break;

            case DatabaseProvider.SqlServer:
                logger?.LogInformation("Configuring SQL Server");
                options.UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: maxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelay),
                        errorNumbersToAdd: null);
                    sqlServerOptions.CommandTimeout(commandTimeout);
                    sqlServerOptions.MigrationsAssembly(GetMigrationsAssembly<TContext>());
                });
                break;

            case DatabaseProvider.Oracle:
                logger?.LogInformation("Configuring Oracle Database");
                options.UseOracle(connectionString, oracleOptions =>
                {
                    oracleOptions.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
                    oracleOptions.CommandTimeout(commandTimeout);
                    oracleOptions.MigrationsAssembly(GetMigrationsAssembly<TContext>());
                });
                break;

            case DatabaseProvider.InMemory:
                logger?.LogWarning("Configuring InMemory database. Only for testing.");
                options.UseInMemoryDatabase(connectionString);
                break;

            default:
                throw new NotSupportedException(
                    $"Provider '{provider}' is not supported. " +
                    $"Valid providers: PostgreSQL, SqlServer, Oracle, InMemory");
        }
    }

    private static string GetMigrationsAssembly<TContext>() where TContext : DbContext
    {
        var contextAssembly = typeof(TContext).Assembly;
        var assemblyName = contextAssembly.GetName().Name;

        if (assemblyName?.Contains("Infrastructure") == true)
        {
            return assemblyName;
        }

        var infrastructureAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name?.Contains("Infrastructure") == true
                && a.GetName().Name?.StartsWith(assemblyName?.Split('.')[0] ?? "") == true);

        return infrastructureAssembly?.GetName().Name ?? contextAssembly.GetName().Name!;
    }
}
