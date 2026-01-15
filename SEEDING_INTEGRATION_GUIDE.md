# 🔧 INTEGRACIÓN EN PROGRAMA PRINCIPAL

**Cómo hacer que el seeding se ejecute automáticamente al iniciar la aplicación**

---

## 📍 UBICACIÓN: VehiclesSaleService.Api/Program.cs

### Paso 1: Agregar Seeding Service al DI Container

```csharp
// En Program.cs, después de builder.Services.AddScoped<IVehicleRepository>...

// Registrar el seeding service
builder.Services.AddScoped<DatabaseSeedingService>();
```

### Paso 2: Configurar Seeding en Startup

```csharp
var app = builder.Build();

// === SEEDING (SOLO EN DESARROLLO) ===
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var seeding = scope.ServiceProvider.GetRequiredService<DatabaseSeedingService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("🌱 Iniciando seeding de datos...");
            await seeding.SeedAllAsync();
            logger.LogInformation("✅ Seeding completado");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error en seeding");
            // No lanzar excepción para no bloquear startup
        }
    }
}
// === FIN SEEDING ===

app.UseSwagger();
app.UseSwaggerUI();
// ... resto de configuración
app.Run();
```

### Paso 3: Alternativa - CLI Commands

Si prefieres ejecutar seeding manualmente:

```csharp
// En Program.cs
var app = builder.Build();

// Agregar comandos personalizados
if (args.Contains("seed:all"))
{
    using (var scope = app.Services.CreateScope())
    {
        var seeding = scope.ServiceProvider.GetRequiredService<DatabaseSeedingService>();
        await seeding.SeedAllAsync();
        return;
    }
}

if (args.Contains("seed:clean"))
{
    using (var scope = app.Services.CreateScope())
    {
        var seeding = scope.ServiceProvider.GetRequiredService<DatabaseSeedingService>();
        await seeding.CleanAllAsync();
        return;
    }
}

app.UseSwagger();
// ... resto de configuración
app.Run();
```

**Uso:**

```bash
# Ejecutar seeding
dotnet run --project VehiclesSaleService.Api -- seed:all

# Limpiar datos
dotnet run --project VehiclesSaleService.Api -- seed:clean

# Normal (sin seeding)
dotnet run --project VehiclesSaleService.Api
```

---

## 🔐 OPCIONES AVANZADAS

### Seeding Condicional (Verificar si está vacío)

```csharp
// Antes de seedear, verificar si hay datos
var seedingService = scope.ServiceProvider.GetRequiredService<DatabaseSeedingService>();
var vehicleRepository = scope.ServiceProvider.GetRequiredService<IVehicleRepository>();

var vehicleCount = await vehicleRepository.CountAsync();
if (vehicleCount == 0)
{
    logger.LogInformation("Base de datos vacía, ejecutando seeding...");
    await seedingService.SeedAllAsync();
}
else
{
    logger.LogInformation("Base de datos ya contiene {Count} vehículos, skipping seeding", vehicleCount);
}
```

### Seeding Selectivo (Solo usuarios O solo vehículos)

```csharp
// Ejecutar solo fase específica
if (app.Environment.IsDevelopment() && args.Contains("--seed-users"))
{
    using (var scope = app.Services.CreateScope())
    {
        var seeding = scope.ServiceProvider.GetRequiredService<DatabaseSeedingService>();
        // Refactorizar SeedAllAsync para hacer métodos públicos
        await seeding.SeedUsersAsync();
    }
}
```

### Con Feature Flags

```csharp
var configuration = builder.Configuration;
var enableSeeding = configuration.GetValue<bool>("Features:EnableAutoSeeding", false);

if (app.Environment.IsDevelopment() && enableSeeding)
{
    using (var scope = app.Services.CreateScope())
    {
        var seeding = scope.ServiceProvider.GetRequiredService<DatabaseSeedingService>();
        await seeding.SeedAllAsync();
    }
}
```

**appsettings.Development.json:**

```json
{
  "Features": {
    "EnableAutoSeeding": true
  }
}
```

---

## 🧪 TESTING CON DATOS SEEDED

### Usar SeedingService en Tests

```csharp
// TestFixture.cs
public class SeededDatabaseFixture : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider;

    public async Task InitializeAsync()
    {
        // Setup database
        // Run seeding
        var seeding = _serviceProvider.GetRequiredService<DatabaseSeedingService>();
        await seeding.SeedAllAsync();
    }

    public async Task DisposeAsync()
    {
        // Clean up
        var seeding = _serviceProvider.GetRequiredService<DatabaseSeedingService>();
        await seeding.CleanAllAsync();
    }
}

// En tests
[Collection("Seeded Database")]
public class VehicleApiTests : IClassFixture<SeededDatabaseFixture>
{
    [Fact]
    public async Task GetVehicles_ShouldReturn150Items()
    {
        var response = await _client.GetAsync("/api/vehicles");
        var data = await response.Content.ReadAsAsync<VehiclesResponse>();

        Assert.Equal(150, data.Data.Count());
    }
}
```

---

## 📋 CHECKLIST DE INTEGRACIÓN

- [ ] Importar `using CarDealer.DataSeeding.Services;`
- [ ] Registrar service en DI container
- [ ] Agregar logika de seeding en startup
- [ ] Probar en desarrollo
- [ ] Verificar logs
- [ ] Validar datos en BD
- [ ] Configurar environment-specific behavior
- [ ] Documentar para el equipo

---

## 🚨 NOTAS IMPORTANTES

### ⚠️ NO ejecutar seeding en Producción

```csharp
// ✅ CORRECTO: Solo en desarrollo
if (app.Environment.IsDevelopment())
{
    // seeding
}

// ❌ INCORRECTO: Nunca hagas esto
if (app.Environment.IsProduction())
{
    await seeding.SeedAllAsync(); // ¡NUNCA!
}
```

### ⚠️ Usar transacciones

```csharp
try
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    await SeedAllAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### ⚠️ Idempotencia

```csharp
// Verificar que no existen antes de insertar
if (!await context.Vehicles.AnyAsync())
{
    // Seed vehicles
}
```

---

## 📝 EJEMPLO COMPLETO

```csharp
// VehiclesSaleService.Api/Program.cs

using CarDealer.DataSeeding.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Agregar servicios
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<DatabaseSeedingService>();  // ← Aquí
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// === SEEDING AUTOMÁTICO (SOLO DESARROLLO) ===
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var seeding = scope.ServiceProvider
            .GetRequiredService<DatabaseSeedingService>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("🌱 Iniciando seeding...");
            await seeding.SeedAllAsync();
            logger.LogInformation("✅ Seeding completado");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error en seeding");
        }
    }
}
// === FIN SEEDING ===

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
```

---

**¡Listo! El seeding ahora se ejecutará automáticamente cuando inicies la app.** 🚀
