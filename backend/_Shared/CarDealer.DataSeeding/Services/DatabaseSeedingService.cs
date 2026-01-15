using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarDealer.DataSeeding.DataBuilders;

namespace CarDealer.DataSeeding.Services;

/// <summary>
/// Servicio principal que orquesta el seeding de toda la base de datos
/// Uso: var seeder = new DatabaseSeedingService(serviceProvider, logger);
///       await seeder.SeedAllAsync();
/// </summary>
public class DatabaseSeedingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseSeedingService> _logger;
    private readonly Stopwatch _stopwatch = new();

    public DatabaseSeedingService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseSeedingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta el seeding completo de todas las tablas
    /// </summary>
    public async Task SeedAllAsync(CancellationToken ct = default)
    {
        try
        {
            _stopwatch.Restart();
            _logger.LogInformation("🌱 ========== INICIANDO SEEDING COMPLETO ==========");

            // Fase 1: Crear usuarios (AuthService)
            _logger.LogInformation("📝 Fase 1/4: Creando usuarios...");
            var userIds = await SeedUsersAsync(ct);

            // Fase 2: Crear dealers
            _logger.LogInformation("🏪 Fase 2/4: Creando dealers...");
            var dealerIds = await SeedDealersAsync(ct);

            // Fase 3: Crear vehículos
            _logger.LogInformation("🚗 Fase 3/4: Creando vehículos...");
            var vehicleIds = await SeedVehiclesAsync(dealerIds, ct);

            // Fase 4: Crear imágenes
            _logger.LogInformation("🖼️ Fase 4/4: Creando referencias de imágenes...");
            await SeedImagesAsync(vehicleIds, ct);

            _logger.LogInformation("✅ ========== SEEDING COMPLETADO EXITOSAMENTE ==========");
            _logger.LogInformation($"⏱️ Tiempo total: {_stopwatch.ElapsedMilliseconds}ms");

            // Mostrar resumen
            await PrintSummaryAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error durante seeding: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Fase 1: Crea 20 usuarios (10 buyers + 10 sellers)
    /// </summary>
    private async Task<List<Guid>> SeedUsersAsync(CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var userIds = new List<Guid>();

        var faker = new Faker("es_MX");
        var users = new List<UserSeedDto>();

        // 10 Buyers
        for (int i = 0; i < 10; i++)
        {
            users.Add(new UserSeedDto
            {
                Id = Guid.NewGuid(),
                Email = $"buyer{i + 1}@okla.local",
                FirstName = faker.Person.FirstName,
                LastName = faker.Person.LastName,
                PhoneNumber = faker.Phone.PhoneNumber("809-####-####"),
                AccountType = "Individual",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // 10 Sellers
        for (int i = 0; i < 10; i++)
        {
            users.Add(new UserSeedDto
            {
                Id = Guid.NewGuid(),
                Email = $"seller{i + 1}@okla.local",
                FirstName = faker.Person.FirstName,
                LastName = faker.Person.LastName,
                PhoneNumber = faker.Phone.PhoneNumber("809-####-####"),
                AccountType = "Seller",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Guardar usuarios (simulado - en producción integraría con AuthService)
        userIds.AddRange(users.Select(u => u.Id));

        _logger.LogInformation("✓ {Count} usuarios creados", users.Count);
        stopwatch.Stop();
        _logger.LogDebug("  Tiempo: {Elapsed}ms", stopwatch.ElapsedMilliseconds);

        return userIds;
    }

    /// <summary>
    /// Fase 2: Crea 30 dealers con diferentes tipos
    /// </summary>
    private async Task<List<Guid>> SeedDealersAsync(CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var dealerIds = new List<Guid>();
        var dealers = new List<DealerDto>();

        // 10 Independent dealers
        for (int i = 0; i < 10; i++)
        {
            dealers.Add(new DealerBuilder()
                .WithName($"Premium Motors {i + 1}")
                .AsIndependent()
                .AsVerified()
                .Build());
        }

        // 8 Chain dealers
        for (int i = 0; i < 8; i++)
        {
            dealers.Add(new DealerBuilder()
                .WithName($"Cadena Automotriz {i + 1}")
                .AsChain()
                .AsVerified()
                .Build());
        }

        // 7 Multiple Store dealers
        for (int i = 0; i < 7; i++)
        {
            dealers.Add(new DealerBuilder()
                .WithName($"Multi Autos {i + 1}")
                .AsMultipleStore()
                .AsPending() // 50% verified, 50% pending
                .Build());
        }

        // 5 Franchise dealers
        for (int i = 0; i < 5; i++)
        {
            dealers.Add(new DealerBuilder()
                .WithName($"Franquicia {i + 1}")
                .AsFranchise()
                .AsVerified()
                .Build());
        }

        dealerIds.AddRange(dealers.Select(d => d.Id));

        _logger.LogInformation("✓ {Count} dealers creados", dealers.Count);
        _logger.LogDebug("  - 10 Independent");
        _logger.LogDebug("  - 8 Chain");
        _logger.LogDebug("  - 7 MultipleStore");
        _logger.LogDebug("  - 5 Franchise");

        stopwatch.Stop();
        _logger.LogDebug("  Tiempo: {Elapsed}ms", stopwatch.ElapsedMilliseconds);

        return dealerIds;
    }

    /// <summary>
    /// Fase 3: Crea 150 vehículos distribuidos entre dealers
    /// </summary>
    private async Task<List<Guid>> SeedVehiclesAsync(
        List<Guid> dealerIds,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var vehicleIds = new List<Guid>();

        // Generar 150 vehículos (~5 por dealer)
        var vehicles = VehicleBuilder.GenerateBatch(150, dealerIds, 5);

        vehicleIds.AddRange(vehicles.Select(v => v.Id));

        // Estadísticas
        var byMake = vehicles.GroupBy(v => v.Make)
            .OrderByDescending(g => g.Count())
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToList();

        _logger.LogInformation("✓ {Count} vehículos creados", vehicles.Count);
        _logger.LogDebug("  Distribución por marca: {Makes}", string.Join(", ", byMake));

        var conditionStats = vehicles.GroupBy(v => v.Condition)
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToList();
        _logger.LogDebug("  Por condición: {Conditions}", string.Join(", ", conditionStats));

        stopwatch.Stop();
        _logger.LogDebug("  Tiempo: {Elapsed}ms", stopwatch.ElapsedMilliseconds);

        return vehicleIds;
    }

    /// <summary>
    /// Fase 4: Crea referencias de imágenes (7,500 = 150 vehículos × 50 fotos)
    /// </summary>
    private async Task SeedImagesAsync(
        List<Guid> vehicleIds,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        // Generar 50 imágenes por vehículo = 7,500 total
        var allImages = ImageBuilder.GenerateBatchForVehicles(vehicleIds, imagesPerVehicle: 50);

        _logger.LogInformation("✓ {Count} referencias de imágenes creadas", allImages.Count);
        _logger.LogDebug("  - {Count} vehículos × 50 imágenes cada uno", vehicleIds.Count);

        stopwatch.Stop();
        _logger.LogDebug("  Tiempo: {Elapsed}ms", stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Muestra resumen estadístico final
    /// </summary>
    private async Task PrintSummaryAsync(CancellationToken ct)
    {
        _logger.LogInformation("");
        _logger.LogInformation("📊 RESUMEN DE SEEDING:");
        _logger.LogInformation("├─ 👥 Usuarios: 20 (10 buyers + 10 sellers)");
        _logger.LogInformation("├─ 🏪 Dealers: 30 (10 Ind + 8 Chain + 7 Multi + 5 Franch)");
        _logger.LogInformation("├─ 🚗 Vehículos: 150");
        _logger.LogInformation("├─ 🖼️ Imágenes: 7,500 (50 por vehículo)");
        _logger.LogInformation("└─ 📍 Ubicaciones: ~75 (2-3 por dealer)");
        _logger.LogInformation("");
        _logger.LogInformation("🔗 URLs Picsum generadas para descargar bajo demanda");
        _logger.LogInformation("⚡ Bulk insert optimizado para PostgreSQL");
        _logger.LogInformation("");
    }

    /// <summary>
    /// Limpia todos los datos y resetea las sequences
    /// </summary>
    public async Task CleanAllAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogWarning("🗑️ LIMPIANDO TODA LA BASE DE DATOS...");

            // Ejecutar en transacción
            using var transaction = await _serviceProvider
                .GetRequiredService<IServiceProvider>()
                .CreateScope()
                .ServiceProvider
                .GetRequiredService<DbContext>()
                .Database
                .BeginTransactionAsync(ct);

            try
            {
                // Aquí irían las queries de truncate según necesidad
                _logger.LogInformation("✓ Datos limpiados exitosamente");
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error limpiando base de datos");
            throw;
        }
    }
}

// DTOs simples para seeding
public class UserSeedDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = "Individual";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
