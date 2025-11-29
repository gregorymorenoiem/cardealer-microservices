using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Database;

/// <summary>
/// Servicio en background para aplicar migraciones automáticamente al iniciar
/// </summary>
public class DatabaseMigrationService<TContext> : IHostedService
    where TContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrationService<TContext>> _logger;

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
            "Iniciando aplicación de migraciones para {ContextType}...",
            typeof(TContext).Name);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingCount = pendingMigrations.Count();

            if (pendingCount > 0)
            {
                _logger.LogInformation(
                    "Se encontraron {Count} migraciones pendientes. Aplicando...",
                    pendingCount);

                await context.Database.MigrateAsync(cancellationToken);

                _logger.LogInformation(
                    "Migraciones aplicadas exitosamente para {ContextType}",
                    typeof(TContext).Name);
            }
            else
            {
                _logger.LogInformation(
                    "No hay migraciones pendientes para {ContextType}",
                    typeof(TContext).Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al aplicar migraciones para {ContextType}: {Message}",
                typeof(TContext).Name,
                ex.Message);

            // No lanzar excepción para permitir que la app inicie
            // En producción, las migraciones deben aplicarse manualmente
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
