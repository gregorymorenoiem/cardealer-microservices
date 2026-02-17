using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CarDealer.Shared.Database;

/// <summary>
/// Servicio en background para aplicar migraciones automáticamente al iniciar.
/// Usa PostgreSQL Advisory Locks para evitar race conditions cuando múltiples
/// réplicas inician simultáneamente (auto-scaling, rolling updates).
/// </summary>
public class DatabaseMigrationService<TContext> : IHostedService
    where TContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrationService<TContext>> _logger;

    /// <summary>
    /// Lock ID derivado del nombre del DbContext para que cada servicio tenga su propio lock.
    /// Usa un hash estable para generar un int64 único por tipo de contexto.
    /// </summary>
    private static long GetAdvisoryLockId()
    {
        var contextName = typeof(TContext).FullName ?? typeof(TContext).Name;
        // Generar un hash estable como lock ID (positivo para evitar conflictos)
        long hash = 0;
        foreach (var c in contextName)
        {
            hash = (hash * 31 + c) & 0x7FFFFFFFFFFFFFFF;
        }
        return hash == 0 ? 1 : hash;
    }

    public DatabaseMigrationService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseMigrationService<TContext>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[Migration] Iniciando proceso de migraciones para {ContextType}...",
            typeof(TContext).Name);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        var lockId = GetAdvisoryLockId();
        var connectionString = context.Database.GetConnectionString();

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning(
                "[Migration] No se pudo obtener connection string para {ContextType}. Saltando migraciones.",
                typeof(TContext).Name);
            return;
        }

        // Usar una conexión separada para el advisory lock (no la del DbContext)
        await using var lockConnection = new NpgsqlConnection(connectionString);
        try
        {
            await lockConnection.OpenAsync(cancellationToken);

            // Intentar obtener el advisory lock con timeout de 60 segundos
            // pg_try_advisory_lock es session-level: se libera al cerrar la conexión
            var acquired = false;
            var maxWaitSeconds = 60;
            var waited = 0;

            while (!acquired && waited < maxWaitSeconds)
            {
                await using var cmd = lockConnection.CreateCommand();
                cmd.CommandText = $"SELECT pg_try_advisory_lock({lockId})";
                var result = await cmd.ExecuteScalarAsync(cancellationToken);
                acquired = result is true;

                if (!acquired)
                {
                    _logger.LogInformation(
                        "[Migration] Otra réplica está ejecutando migraciones para {ContextType}. Esperando... ({Waited}s)",
                        typeof(TContext).Name, waited);
                    await Task.Delay(2000, cancellationToken);
                    waited += 2;
                }
            }

            if (!acquired)
            {
                _logger.LogWarning(
                    "[Migration] Timeout esperando lock de migraciones para {ContextType} después de {MaxWait}s. " +
                    "Otra réplica probablemente completó las migraciones.",
                    typeof(TContext).Name, maxWaitSeconds);
                return;
            }

            _logger.LogInformation(
                "[Migration] Lock adquirido para {ContextType}. Verificando migraciones pendientes...",
                typeof(TContext).Name);

            // Ahora que tenemos el lock, verificar migraciones pendientes
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingCount = pendingMigrations.Count();

            if (pendingCount > 0)
            {
                _logger.LogInformation(
                    "[Migration] Se encontraron {Count} migraciones pendientes para {ContextType}. Aplicando...",
                    pendingCount, typeof(TContext).Name);

                await context.Database.MigrateAsync(cancellationToken);

                _logger.LogInformation(
                    "[Migration] ✅ Migraciones aplicadas exitosamente para {ContextType}",
                    typeof(TContext).Name);
            }
            else
            {
                _logger.LogInformation(
                    "[Migration] No hay migraciones pendientes para {ContextType}",
                    typeof(TContext).Name);
            }

            // Liberar advisory lock explícitamente
            await using var unlockCmd = lockConnection.CreateCommand();
            unlockCmd.CommandText = $"SELECT pg_advisory_unlock({lockId})";
            await unlockCmd.ExecuteScalarAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "[Migration] Proceso de migraciones cancelado para {ContextType}",
                typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[Migration] ❌ Error al aplicar migraciones para {ContextType}: {Message}",
                typeof(TContext).Name,
                ex.Message);

            // No lanzar excepción para permitir que la app inicie
            // En producción, las migraciones deben aplicarse con un Job de K8s
        }
        // lockConnection.Dispose() libera el advisory lock automáticamente si no se liberó explícitamente
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
