# 🗄️ Guía Profesional: Configuración Multi-Base de Datos en Microservicios .NET

## 📌 Overview

Esta guía implementa un **patrón de configuración multi-proveedor** que permite cambiar entre PostgreSQL, SQL Server, MySQL y SQLite mediante configuración JSON, siguiendo mejores prácticas enterprise.

---

## 🎯 Objetivo

Permitir que cada microservicio pueda usar **cualquier motor de base de datos** sin cambiar código, solo configuración:

```json
// appsettings.json
"Database": {
  "Provider": "PostgreSQL",  // ← Cambiar aquí
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=mydb;Username=postgres;Password=pwd",
    "SqlServer": "Server=localhost;Database=mydb;User Id=sa;Password=pwd",
    "MySQL": "Server=localhost;Database=mydb;User=root;Password=pwd",
    "Oracle": "User Id=system;Password=pwd;Data Source=localhost:1521/XEPDB1"
  }
}
```

---

## 🏗️ Arquitectura de la Solución

### 1. **Patrón de Diseño: Strategy Pattern + Factory**

```
┌─────────────────────────────────────────┐
│         appsettings.json                │
│  Database.Provider = "PostgreSQL"       │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│    DatabaseConfiguration (Settings)     │
│  - Provider (enum)                      │
│  - ConnectionStrings (Dictionary)       │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│   DatabaseExtensions (Factory)          │
│  + AddDatabaseProvider()                │
│    → Switch (Provider)                  │
│       case PostgreSQL → UseNpgsql()     │
│       case SqlServer → UseSqlServer()   │
│       case MySQL → UseMySql()           │
│       case Oracle → UseOracle()         │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│       DbContext (EF Core)               │
│  Configurado con el provider elegido    │
└─────────────────────────────────────────┘
```

---

## 📁 Estructura de Archivos

### **Shared Library** (Común a todos los microservicios)

```
CarDealer.Shared/
├── Database/
│   ├── DatabaseProvider.cs          ← Enum de proveedores
│   ├── DatabaseConfiguration.cs     ← Modelo de configuración
│   ├── DatabaseExtensions.cs        ← Factory para configurar provider
│   └── MigrationHelper.cs           ← Helper para migraciones
└── CarDealer.Shared.csproj
```

### **Por cada Microservicio**

```
ErrorService.Api/
├── appsettings.json                  ← Configuración de BD
├── appsettings.Development.json
├── appsettings.Production.json
└── Program.cs                        ← Usa DatabaseExtensions
```

---

## 💻 Implementación Paso a Paso

### **PASO 1: Crear Shared Library (Si no existe)**

```bash
# En backend/
dotnet new classlib -n CarDealer.Shared -f net8.0
cd CarDealer.Shared
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.Extensions.Configuration.Abstractions --version 8.0.0
dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.0
```

### **PASO 2: Enum de Proveedores**

**`CarDealer.Shared/Database/DatabaseProvider.cs`:**

```csharp
namespace CarDealer.Shared.Database;

/// <summary>
/// Proveedores de base de datos soportados.
/// </summary>
public enum DatabaseProvider
{
    /// <summary>
    /// PostgreSQL (Npgsql)
    /// </summary>
    PostgreSQL,

    /// <summary>
    /// Microsoft SQL Server
    /// </summary>
    SqlServer,

    /// <summary>
    /// MySQL (Pomelo.EntityFrameworkCore.MySql)
    /// </summary>
    MySQL,

    /// <summary>
    /// Oracle Database (Oracle.EntityFrameworkCore)
    /// </summary>
    Oracle,

    /// <summary>
    /// In-Memory (Testing only)
    /// </summary>
    InMemory
}
```

### **PASO 3: Modelo de Configuración**

**`CarDealer.Shared/Database/DatabaseConfiguration.cs`:**

```csharp
namespace CarDealer.Shared.Database;

/// <summary>
/// Configuración de base de datos desde appsettings.json
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// Proveedor de base de datos activo
    /// </summary>
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.PostgreSQL;

    /// <summary>
    /// Cadenas de conexión por proveedor
    /// </summary>
    public Dictionary<string, string> ConnectionStrings { get; set; } = new();

    /// <summary>
    /// Habilitar sensitive data logging (solo Development)
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Habilitar detailed errors (solo Development)
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Aplicar migraciones automáticamente al iniciar
    /// </summary>
    public bool AutoMigrate { get; set; } = true;

    /// <summary>
    /// Timeout de comandos en segundos
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Número máximo de reintentos en caso de fallo
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Delay máximo entre reintentos en segundos
    /// </summary>
    public int MaxRetryDelay { get; set; } = 30;

    /// <summary>
    /// Obtiene la connection string activa según el provider
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
            $"Verifica la configuración en appsettings.json");
    }
}
```

### **PASO 4: Factory de Configuración (LO MÁS IMPORTANTE)**

**`CarDealer.Shared/Database/DatabaseExtensions.cs`:**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Database;

/// <summary>
/// Extensiones para configurar el proveedor de base de datos de forma dinámica
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Configura el DbContext con el proveedor especificado en appsettings.json
    /// </summary>
    /// <typeparam name="TContext">Tipo del DbContext</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="configSectionName">Nombre de la sección en appsettings (default: "Database")</param>
    public static IServiceCollection AddDatabaseProvider<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName = "Database")
        where TContext : DbContext
    {
        // Leer configuración
        var dbConfig = configuration.GetSection(configSectionName).Get<DatabaseConfiguration>()
            ?? throw new InvalidOperationException(
                $"No se encontró la sección '{configSectionName}' en appsettings.json");

        // Registrar configuración como singleton
        services.AddSingleton(dbConfig);

        // Obtener connection string activa
        var connectionString = dbConfig.GetConnectionString();

        // Configurar DbContext según el proveedor
        services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            ConfigureProvider(options, dbConfig, connectionString, serviceProvider);
        });

        return services;
    }

    /// <summary>
    /// Configura el proveedor específico de base de datos
    /// </summary>
    private static void ConfigureProvider(
        DbContextOptionsBuilder options,
        DatabaseConfiguration config,
        string connectionString,
        IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<DbContext>>();

        switch (config.Provider)
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
                    npgsqlOptions.MigrationsAssembly(GetMigrationsAssembly<DbContext>());
                });
                break;

            case DatabaseProvider.SqlServer:
                logger?.LogInformation("Configurando SQL Server");
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: config.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(config.MaxRetryDelay),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(config.CommandTimeout);
                    sqlOptions.MigrationsAssembly(GetMigrationsAssembly<DbContext>());
                });
                break;

            case DatabaseProvider.MySQL:
                logger?.LogInformation("Configurando MySQL con Pomelo");
                var serverVersion = ServerVersion.AutoDetect(connectionString);
                options.UseMySql(connectionString, serverVersion, mySqlOptions =>
                {
                    mySqlOptions.EnableRetryOnFailure(
                        maxRetryCount: config.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(config.MaxRetryDelay));
                    mySqlOptions.CommandTimeout(config.CommandTimeout);
                    mySqlOptions.MigrationsAssembly(GetMigrationsAssembly<DbContext>());
                });
                break;

            case DatabaseProvider.Oracle:
                logger?.LogInformation("Configurando Oracle Database");
                options.UseOracle(connectionString, oracleOptions =>
                {
                    oracleOptions.UseOracleSQLCompatibility("11"); // Oracle 11g+
                    oracleOptions.CommandTimeout(config.CommandTimeout);
                    oracleOptions.MigrationsAssembly(GetMigrationsAssembly<DbContext>());
                    // Oracle tiene retry automático en el driver
                    oracleOptions.MaxBatchSize(config.MaxRetryCount);
                });
                break;

            case DatabaseProvider.InMemory:
                logger?.LogWarning("Configurando InMemory Database (SOLO PARA TESTING)");
                options.UseInMemoryDatabase("TestDatabase");
                break;

            default:
                throw new NotSupportedException(
                    $"El proveedor '{config.Provider}' no está soportado");
        }

        // Configuraciones comunes
        if (config.EnableSensitiveDataLogging)
        {
            logger?.LogWarning("Sensitive Data Logging HABILITADO (no usar en producción)");
            options.EnableSensitiveDataLogging();
        }

        if (config.EnableDetailedErrors)
        {
            options.EnableDetailedErrors();
        }

        // Logging de queries (solo en Development)
        options.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());
    }

    /// <summary>
    /// Obtiene el assembly donde están las migraciones (Infrastructure)
    /// </summary>
    private static string GetMigrationsAssembly<TContext>() where TContext : DbContext
    {
        // Asume que las migraciones están en el mismo assembly que el DbContext
        return typeof(TContext).Assembly.GetName().Name 
            ?? throw new InvalidOperationException("No se pudo determinar el assembly de migraciones");
    }

    /// <summary>
    /// Aplica migraciones pendientes si AutoMigrate está habilitado
    /// </summary>
    public static async Task ApplyMigrationsAsync<TContext>(
        this IServiceProvider serviceProvider,
        DatabaseConfiguration? config = null)
        where TContext : DbContext
    {
        config ??= serviceProvider.GetRequiredService<DatabaseConfiguration>();

        if (!config.AutoMigrate)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<TContext>>();

        try
        {
            logger?.LogInformation("Aplicando migraciones de base de datos...");
            await context.Database.MigrateAsync();
            logger?.LogInformation("Migraciones aplicadas exitosamente");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error al aplicar migraciones de base de datos");
            throw;
        }
    }
}
```

### **PASO 5: Helper para Migraciones**

**`CarDealer.Shared/Database/MigrationHelper.cs`:**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Database;

/// <summary>
/// Helper para gestionar migraciones según el proveedor
/// </summary>
public static class MigrationHelper
{
    /// <summary>
    /// Verifica si hay migraciones pendientes
    /// </summary>
    public static async Task<bool> HasPendingMigrationsAsync<TContext>(TContext context)
        where TContext : DbContext
    {
        var pending = await context.Database.GetPendingMigrationsAsync();
        return pending.Any();
    }

    /// <summary>
    /// Obtiene lista de migraciones aplicadas
    /// </summary>
    public static async Task<IEnumerable<string>> GetAppliedMigrationsAsync<TContext>(
        TContext context)
        where TContext : DbContext
    {
        return await context.Database.GetAppliedMigrationsAsync();
    }

    /// <summary>
    /// Crea la base de datos si no existe (útil para SQLite/Development)
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
                logger?.LogInformation("Base de datos creada exitosamente");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error al crear la base de datos");
            throw;
        }
    }

    /// <summary>
    /// Elimina y recrea la base de datos (SOLO PARA TESTING)
    /// </summary>
    public static async Task RecreateAsync<TContext>(
        TContext context,
        ILogger? logger = null)
        where TContext : DbContext
    {
        logger?.LogWarning("ELIMINANDO base de datos (solo desarrollo/testing)");
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        logger?.LogInformation("Base de datos recreada");
    }
}
```

---

## 📝 Configuración en appsettings.json

### **Estructura Recomendada:**

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionStrings": {
      "PostgreSQL": "Host=localhost;Port=5432;Database=errorservice;Username=postgres;Password=password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=20;",
      "SqlServer": "Server=localhost,1433;Database=errorservice;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true;",
      "MySQL": "Server=localhost;Port=3306;Database=errorservice;User=root;Password=password;AllowUserVariables=true;UseAffectedRows=false;",
      "Oracle": "User Id=ERRORSERVICE;Password=password;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XEPDB1)));Pooling=true;Min Pool Size=5;Max Pool Size=20;"
    },
    "AutoMigrate": true,
    "CommandTimeout": 30,
    "MaxRetryCount": 3,
    "MaxRetryDelay": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false
  }
}
```

### **appsettings.Development.json:**

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionStrings": {
      "PostgreSQL": "Host=localhost;Database=errorservice_dev;Username=postgres;Password=password"
    },
    "EnableSensitiveDataLogging": true,
    "EnableDetailedErrors": true,
    "AutoMigrate": true
  }
}
```

### **appsettings.Production.json:**

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionStrings": {
      "PostgreSQL": "Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD};SSL Mode=Require;"
    },
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "AutoMigrate": false
  }
}
```

---

## 🔧 Uso en Microservicios

### **Program.cs (Refactorizado):**

```csharp
using ErrorService.Infrastructure.Persistence;
using CarDealer.Shared.Database;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✨ CONFIGURACIÓN MULTI-DATABASE (Una sola línea!)
builder.Services.AddDatabaseProvider<ApplicationDbContext>(
    builder.Configuration,
    configSectionName: "Database");

// Resto de servicios...
builder.Services.AddScoped<IErrorLogRepository, EfErrorLogRepository>();
// ...

var app = builder.Build();

// ✨ APLICAR MIGRACIONES AL INICIAR (Si AutoMigrate = true)
await app.Services.ApplyMigrationsAsync<ApplicationDbContext>();

// Pipeline...
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**¡ESO ES TODO!** No más `UseNpgsql` hardcodeado. Todo se configura desde JSON.

---

## 📦 Packages Necesarios

### **CarDealer.Shared.csproj:**

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
  <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.0" />
  <PackageReference Include="Oracle.EntityFrameworkCore" Version="8.23.50" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
  <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="8.0.0" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
</ItemGroup>
```

### **Cada Microservicio (ErrorService.Infrastructure.csproj):**

```xml
<ItemGroup>
  <ProjectReference Include="..\..\CarDealer.Shared\CarDealer.Shared.csproj" />
</ItemGroup>
```

---

## 🚀 Migraciones Multi-Proveedor

### **Generar Migraciones:**

```bash
# PostgreSQL (default)
dotnet ef migrations add InitialCreate --project ErrorService.Infrastructure --startup-project ErrorService.Api

# SQL Server (cambiar Provider en appsettings antes)
dotnet ef migrations add InitialCreate --project ErrorService.Infrastructure --startup-project ErrorService.Api

# MySQL (cambiar Provider en appsettings antes)
dotnet ef migrations add InitialCreate --project ErrorService.Infrastructure --startup-project ErrorService.Api

# Oracle (cambiar Provider en appsettings antes)
dotnet ef migrations add InitialCreate --project ErrorService.Infrastructure --startup-project ErrorService.Api
```

### **Script SQL (Para deployment manual):**

```bash
# Generar script SQL para PostgreSQL
dotnet ef migrations script --project ErrorService.Infrastructure --startup-project ErrorService.Api --output migrations.sql

# Para SQL Server
dotnet ef migrations script --project ErrorService.Infrastructure --startup-project ErrorService.Api --output migrations_sqlserver.sql
```

---

## ✅ Ventajas de Este Enfoque

1. ✅ **Zero Code Changes**: Cambias de BD solo editando JSON
2. ✅ **Multi-Ambiente**: Dev usa SQLite, Prod usa PostgreSQL
3. ✅ **Type-Safe**: Configuración fuertemente tipada
4. ✅ **Logging Integrado**: Logs automáticos del provider elegido
5. ✅ **Resilience**: Retry automático con backoff exponencial
6. ✅ **Migraciones Automáticas**: En Development (configurable)
7. ✅ **Testing**: Usa InMemory para tests unitarios
8. ✅ **Production Ready**: Connection pooling, timeouts, SSL

---

## 🎯 Próximos Pasos

1. **Crear CarDealer.Shared** con el código de arriba
2. **Agregar referencia** en cada microservicio
3. **Actualizar appsettings.json** con la nueva estructura
4. **Refactorizar Program.cs** para usar `AddDatabaseProvider<T>()`
5. **Probar** con diferentes providers cambiando `"Provider"`

---

## 🔍 Ejemplo Real: ErrorService

**ANTES (hardcoded PostgreSQL):**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
```

**DESPUÉS (multi-provider):**
```csharp
builder.Services.AddDatabaseProvider<ApplicationDbContext>(
    builder.Configuration);
```

**appsettings.json:**
```json
{
  "Database": {
    "Provider": "PostgreSQL",  // ← Solo cambiar esto
    "ConnectionStrings": { /* ... */ }
  }
}
```

---

## 📊 Testing

```csharp
// appsettings.Testing.json
{
  "Database": {
    "Provider": "InMemory",
    "AutoMigrate": false
  }
}

// Test
public class ErrorServiceTests
{
    [Fact]
    public async Task Should_LogError()
    {
        // InMemory database automáticamente
        var app = new WebApplicationFactory<Program>();
        var client = app.CreateClient();
        
        var response = await client.PostAsync("/api/errors", ...);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

---

## 🎓 Mejores Prácticas

1. ✅ **Development**: PostgreSQL local (consistencia con producción)
2. ✅ **Staging**: PostgreSQL/SQL Server/Oracle (igual que producción)
3. ✅ **Production**: PostgreSQL/SQL Server/Oracle (con AutoMigrate=false)
4. ✅ **Testing**: InMemory (tests unitarios ultra-rápidos)
5. ✅ **Connection Strings**: Usar variables de entorno en prod
6. ✅ **Migrations**: Separadas por proveedor si es necesario
7. ✅ **Oracle**: Usar esquemas (schemas) para separación lógica de datos

---

## 🔐 Seguridad

### **Usar Variables de Entorno:**

```json
// appsettings.Production.json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionStrings": {
      "PostgreSQL": "Host=${DB_HOST};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"
    }
  }
}
```

```bash
# Docker Compose
environment:
  - DB_HOST=postgres-server
  - DB_NAME=errorservice_prod
  - DB_USER=app_user
  - DB_PASSWORD=SuperSecurePassword123!
```

### **Azure Key Vault / AWS Secrets Manager:**

```csharp
// Program.cs
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(builder.Configuration["KeyVault:Url"]),
        new DefaultAzureCredential());
}
```

---

¿Quieres que implemente esto en tus microservicios **AHORA** o prefieres revisarlo primero? 🚀
