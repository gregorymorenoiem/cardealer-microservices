using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Database;

/// <summary>
/// Utilidades para gestión de migraciones
/// </summary>
public static class MigrationHelper
{
    /// <summary>
    /// Obtiene lista de migraciones pendientes
    /// </summary>
    public static async Task<IEnumerable<string>> GetPendingMigrationsAsync<TContext>(
        TContext context,
        ILogger? logger = null)
        where TContext : DbContext
    {
        try
        {
            var pending = await context.Database.GetPendingMigrationsAsync();

            logger?.LogInformation(
                "Migraciones pendientes para {ContextType}: {Count}",
                typeof(TContext).Name,
                pending.Count());

            return pending;
        }
        catch (Exception ex)
        {
            logger?.LogError(
                ex,
                "Error al obtener migraciones pendientes: {Message}",
                ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Obtiene lista de migraciones aplicadas
    /// </summary>
    public static async Task<IEnumerable<string>> GetAppliedMigrationsAsync<TContext>(
        TContext context,
        ILogger? logger = null)
        where TContext : DbContext
    {
        try
        {
            var applied = await context.Database.GetAppliedMigrationsAsync();

            logger?.LogInformation(
                "Migraciones aplicadas en {ContextType}: {Count}",
                typeof(TContext).Name,
                applied.Count());

            return applied;
        }
        catch (Exception ex)
        {
            logger?.LogError(
                ex,
                "Error al obtener migraciones aplicadas: {Message}",
                ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Verifica si hay migraciones pendientes
    /// </summary>
    public static async Task<bool> HasPendingMigrationsAsync<TContext>(
        TContext context,
        ILogger? logger = null)
        where TContext : DbContext
    {
        var pending = await GetPendingMigrationsAsync(context, logger);
        return pending.Any();
    }

    /// <summary>
    /// Aplica todas las migraciones pendientes
    /// </summary>
    public static async Task ApplyMigrationsAsync<TContext>(
        TContext context,
        ILogger? logger = null)
        where TContext : DbContext
    {
        try
        {
            logger?.LogInformation("Aplicando migraciones...");

            await context.Database.MigrateAsync();

            logger?.LogInformation("Migraciones aplicadas exitosamente");
        }
        catch (Exception ex)
        {
            logger?.LogError(
                ex,
                "Error al aplicar migraciones: {Message}",
                ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Asegura que la base de datos esté creada (solo para desarrollo/testing)
    /// </summary>
    public static async Task EnsureCreatedAsync<TContext>(
        TContext context,
        ILogger? logger = null)
        where TContext : DbContext
    {
        try
        {
            var created = await context.Database.EnsureCreatedAsync();

            if (created)
            {
                logger?.LogWarning(
                    "Base de datos creada con EnsureCreated. No usar en producción.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(
                ex,
                "Error al crear base de datos: {Message}",
                ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Elimina y recrea la base de datos (solo para testing)
    /// </summary>
    public static async Task RecreateAsync<TContext>(
        TContext context,
        ILogger? logger = null)
        where TContext : DbContext
    {
        try
        {
            logger?.LogWarning("ELIMINANDO base de datos...");
            await context.Database.EnsureDeletedAsync();

            logger?.LogInformation("Recreando base de datos...");
            await context.Database.EnsureCreatedAsync();

            logger?.LogInformation("Base de datos recreada");
        }
        catch (Exception ex)
        {
            logger?.LogError(
                ex,
                "Error al recrear base de datos: {Message}",
                ex.Message);
            throw;
        }
    }
}
