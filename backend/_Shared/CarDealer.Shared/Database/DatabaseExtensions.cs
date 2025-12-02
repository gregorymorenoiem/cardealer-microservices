using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Database;

/// <summary>
/// Extensiones para configurar DbContext con múltiples proveedores de base de datos
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Registra el DbContext con el proveedor configurado en appsettings.json
    /// </summary>
    /// <typeparam name="TContext">Tipo del DbContext a registrar</typeparam>
    /// <param name="services">Colección de servicios de DI</param>
    /// <param name="configuration">Configuración de la aplicación</param>
    /// <param name="configSection">Nombre de la sección de configuración (default: "Database")</param>
    /// <returns>ServiceCollection para chaining</returns>
    public static IServiceCollection AddDatabaseProvider<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = "Database")
        where TContext : DbContext
    {
        // Leer configuración
        var config = configuration.GetSection(configSection).Get<DatabaseConfiguration>()
            ?? throw new InvalidOperationException(
                $"No se encontró la sección '{configSection}' en appsettings.json. " +
                $"Verifique la configuración de base de datos.");

        var connectionString = config.GetConnectionString();
        var provider = config.Provider;

        // Logger para diagnóstico
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<TContext>>();

        logger?.LogInformation(
            "Configurando DbContext {ContextType} con proveedor {Provider}",
            typeof(TContext).Name,
            provider);

        // Registrar DbContext con el proveedor seleccionado
        services.AddDbContext<TContext>(options =>
        {
            ConfigureDatabaseProvider(options, provider, connectionString, config, logger);

            // Opciones comunes
            if (config.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
                logger?.LogWarning(
                    "Sensitive data logging está HABILITADO. No usar en producción.");
            }

            if (config.EnableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }
        });

        // Aplicar migraciones automáticamente si está configurado
        if (config.AutoMigrate)
        {
            logger?.LogInformation("AutoMigrate está habilitado. Las migraciones se aplicarán al iniciar.");

            services.AddHostedService<DatabaseMigrationService<TContext>>();
        }

        return services;
    }

    /// <summary>
    /// Configura el proveedor de base de datos específico
    /// </summary>
    private static void ConfigureDatabaseProvider<TContext>(
        DbContextOptionsBuilder options,
        DatabaseProvider provider,
        string connectionString,
        DatabaseConfiguration config,
        ILogger<TContext>? logger)
        where TContext : DbContext
    {
        switch (provider)
        {
            case DatabaseProvider.PostgreSQL:
                logger?.LogInformation("Configurando PostgreSQL con Npgsql");
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: config.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(config.MaxRetryDelay),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(config.CommandTimeout);
                    npgsqlOptions.MigrationsAssembly(GetMigrationsAssembly<TContext>());
                });
                break;

            case DatabaseProvider.SqlServer:
                logger?.LogInformation("Configurando SQL Server");
                options.UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: config.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(config.MaxRetryDelay),
                        errorNumbersToAdd: null);
                    sqlServerOptions.CommandTimeout(config.CommandTimeout);
                    sqlServerOptions.MigrationsAssembly(GetMigrationsAssembly<TContext>());
                });
                break;

            case DatabaseProvider.MySQL:
                logger?.LogInformation("MySQL provider requested but not configured.");
                throw new NotSupportedException(
                    "MySQL provider is not configured in this build. " +
                    "If you need MySQL support add a compatible MySQL EF Core provider (e.g. Pomelo) " +
                    "and align package versions in Directory.Packages.props.");


            case DatabaseProvider.Oracle:
                logger?.LogInformation("Configurando Oracle Database");
                options.UseOracle(connectionString, oracleOptions =>
                {
                    oracleOptions.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
                    oracleOptions.CommandTimeout(config.CommandTimeout);
                    oracleOptions.MigrationsAssembly(GetMigrationsAssembly<TContext>());
                    // Oracle tiene retry automático en el driver
                    oracleOptions.MaxBatchSize(config.MaxRetryCount);
                });
                break;

            case DatabaseProvider.InMemory:
                logger?.LogWarning("Configurando InMemory database. Solo para testing.");
                var databaseName = $"{typeof(TContext).Name}_{Guid.NewGuid()}";
                options.UseInMemoryDatabase(databaseName);
                break;

            default:
                throw new NotSupportedException(
                    $"El proveedor '{provider}' no está soportado. " +
                    $"Proveedores válidos: PostgreSQL, SqlServer, MySQL, Oracle, InMemory");
        }
    }

    /// <summary>
    /// Obtiene el assembly donde se encuentran las migraciones
    /// </summary>
    private static string GetMigrationsAssembly<TContext>() where TContext : DbContext
    {
        // Buscar assembly con nombre que contenga "Infrastructure"
        var contextAssembly = typeof(TContext).Assembly;
        var assemblyName = contextAssembly.GetName().Name;

        // Si el contexto está en Infrastructure, usar ese assembly
        if (assemblyName?.Contains("Infrastructure") == true)
        {
            return assemblyName;
        }

        // Buscar assembly de Infrastructure en el mismo proyecto
        var infrastructureAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name?.Contains("Infrastructure") == true
                && a.GetName().Name?.StartsWith(assemblyName?.Split('.')[0] ?? "") == true);

        return infrastructureAssembly?.GetName().Name ?? contextAssembly.GetName().Name!;
    }
}
