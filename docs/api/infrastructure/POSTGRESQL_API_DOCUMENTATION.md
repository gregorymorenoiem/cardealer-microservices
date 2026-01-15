# 🐘 API PostgreSQL

**Proveedor:** PostgreSQL Community (Open Source)  
**Documentación oficial:** https://www.postgresql.org/docs/  
**Versión:** 16+  
**Actualizado:** Enero 2026

---

## 📋 Tabla de Contenidos

1. [Introducción](#introducción)
2. [Configuración](#configuración)
3. [Bases de datos](#bases-de-datos)
4. [Conexión desde .NET](#conexión-desde-net)
5. [Entity Framework Core](#entity-framework-core)
6. [Migraciones](#migraciones)
7. [Queries avanzados](#queries-avanzados)
8. [Performance](#performance)
9. [Backup y recovery](#backup-y-recovery)

---

## 🎯 Introducción

PostgreSQL es la base de datos relacional principal de OKLA, usada por todos los microservicios para persistencia de datos.

### ¿Por qué PostgreSQL?

- ✅ **Open source y gratuito**
- ✅ **ACID compliant** (transacciones confiables)
- ✅ **Extensibilidad** (JSON, full-text search, PostGIS)
- ✅ **Performance** (indexes, partitioning)
- ✅ **Comunidad activa** y soporte enterprise

---

## 🔧 Configuración

### Docker Compose

```yaml
postgres:
  image: postgres:16
  container_name: postgres_db
  environment:
    POSTGRES_USER: postgres
    POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-password}
  ports:
    - "5432:5432"
  volumes:
    - postgres_data:/var/lib/postgresql/data
    - ./scripts/postgres-init.sh:/docker-entrypoint-initdb.d/init.sh
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U postgres"]
    interval: 10s
    timeout: 5s
    retries: 5
```

### Connection String

```
Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=password;
```

**Producción (DOKS):**

```
Host=postgres.okla.svc.cluster.local;Port=5432;Database=vehiclessaleservice;Username=postgres;Password=${POSTGRES_PASSWORD};SSL Mode=Require;
```

---

## 🗄️ Bases de Datos

### Bases de Datos por Microservicio

| Database                     | Servicio                   | Propósito                    | Tamaño Aprox |
| ---------------------------- | -------------------------- | ---------------------------- | ------------ |
| `authservice`                | AuthService                | Users, JWT tokens, passwords | 100 MB       |
| `userservice`                | UserService                | User profiles, preferences   | 50 MB        |
| `roleservice`                | RoleService                | Roles, permissions, RBAC     | 10 MB        |
| `vehiclessaleservice`        | VehiclesSaleService        | Vehículos, homepage sections | 2 GB         |
| `mediaservice`               | MediaService               | File metadata, S3 keys       | 500 MB       |
| `billingservice`             | BillingService             | Payments, subscriptions      | 200 MB       |
| `dealermanagementservice`    | DealerManagementService    | Dealers, locations           | 150 MB       |
| `errorservice`               | ErrorService               | Errors centralizados         | 1 GB         |
| `notificationservice`        | NotificationService        | Notifications log            | 300 MB       |
| `contactservice`             | ContactService             | Messages entre usuarios      | 400 MB       |
| `maintenanceservice`         | MaintenanceService         | Maintenance windows          | 5 MB         |
| `comparisonservice`          | ComparisonService          | Vehicle comparisons          | 50 MB        |
| `alertservice`               | AlertService               | Price alerts, searches       | 100 MB       |
| `inventorymanagementservice` | InventoryManagementService | Dealer inventory             | 800 MB       |
| `dealeranalyticsservice`     | DealerAnalyticsService     | Analytics data               | 1.5 GB       |
| `reviewservice`              | ReviewService              | Reviews y ratings            | 200 MB       |

**Total:** ~8 GB (Enero 2026)

---

## 🔌 Conexión desde .NET

### Instalar Npgsql

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.11
```

### Configuración en appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=vehiclessaleservice;Username=postgres;Password=password;"
  }
}
```

### Program.cs

```csharp
using Npgsql;
using Microsoft.EntityFrameworkCore;

// Entity Framework Core con PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null
            );
            npgsqlOptions.CommandTimeout(30);
        }
    )
);
```

---

## 🏗️ Entity Framework Core

### DbContext

```csharp
using Microsoft.EntityFrameworkCore;

public class VehicleDbContext : DbContext
{
    public VehicleDbContext(DbContextOptions<VehicleDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehicleImage> VehicleImages { get; set; }
    public DbSet<HomepageSectionConfig> HomepageSectionConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VehicleDbContext).Assembly);

        // Indexes
        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.Status);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => new { v.Make, v.Model, v.Year });

        // Valores por defecto
        modelBuilder.Entity<Vehicle>()
            .Property(v => v.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
```

### Entity Configuration

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Make)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Price)
            .HasPrecision(18, 2);

        builder.Property(v => v.Description)
            .HasMaxLength(2000);

        // Relaciones
        builder.HasOne(v => v.User)
            .WithMany(u => u.Vehicles)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Images)
            .WithOne(i => i.Vehicle)
            .HasForeignKey(i => i.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

## 🔄 Migraciones

### Crear Migración

```bash
dotnet ef migrations add InitialCreate \
  --project VehiclesSaleService.Infrastructure \
  --startup-project VehiclesSaleService.Api \
  --output-dir Persistence/Migrations
```

### Aplicar Migración

```bash
# Desarrollo (local)
dotnet ef database update \
  --project VehiclesSaleService.Infrastructure \
  --startup-project VehiclesSaleService.Api

# Producción (automático al iniciar)
public static async Task Main(string[] args)
{
    var app = builder.Build();

    // Auto-migrate en startup
    if (app.Environment.IsProduction())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<VehicleDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    await app.RunAsync();
}
```

### Rollback Migración

```bash
dotnet ef database update PreviousMigrationName \
  --project VehiclesSaleService.Infrastructure \
  --startup-project VehiclesSaleService.Api
```

---

## 📊 Queries Avanzados

### 1. Full-Text Search

```csharp
// Configurar en migration
modelBuilder.Entity<Vehicle>()
    .HasGeneratedTsVectorColumn(
        v => v.SearchVector,
        "english",
        v => new { v.Make, v.Model, v.Description }
    )
    .HasIndex(v => v.SearchVector)
    .HasMethod("GIN");

// Usar en query
var results = await _context.Vehicles
    .Where(v => v.SearchVector.Matches(EF.Functions.ToTsQuery("english", searchTerm)))
    .ToListAsync();
```

### 2. JSON Columns

```csharp
// Configurar propiedad JSON
builder.Property(v => v.Features)
    .HasColumnType("jsonb");

// Query
var withFeature = await _context.Vehicles
    .Where(v => EF.Functions.JsonContains(v.Features, "{\"sunroof\": true}"))
    .ToListAsync();
```

### 3. Window Functions

```csharp
var ranked = await _context.Vehicles
    .Select(v => new
    {
        Vehicle = v,
        Rank = EF.Functions.RowNumber(
            EF.Functions.OrderBy(v.Price),
            EF.Functions.PartitionBy(v.Make)
        )
    })
    .ToListAsync();
```

### 4. CTEs (Common Table Expressions)

```sql
WITH recent_vehicles AS (
    SELECT * FROM vehicles
    WHERE created_at > NOW() - INTERVAL '30 days'
)
SELECT make, COUNT(*) as count
FROM recent_vehicles
GROUP BY make
ORDER BY count DESC;
```

```csharp
var query = _context.Database.SqlQuery<MakeCountResult>(
    $@"WITH recent_vehicles AS (
        SELECT * FROM vehicles
        WHERE created_at > NOW() - INTERVAL '30 days'
    )
    SELECT make, COUNT(*) as count
    FROM recent_vehicles
    GROUP BY make
    ORDER BY count DESC"
);
```

---

## ⚡ Performance

### 1. Indexes

```sql
-- Index simple
CREATE INDEX idx_vehicles_status ON vehicles(status);

-- Index compuesto
CREATE INDEX idx_vehicles_make_model_year ON vehicles(make, model, year);

-- Index parcial (solo vehículos activos)
CREATE INDEX idx_vehicles_active ON vehicles(status)
WHERE status = 'Active';

-- Index GIN para full-text search
CREATE INDEX idx_vehicles_search ON vehicles USING GIN(search_vector);

-- Index B-tree para rangos
CREATE INDEX idx_vehicles_price ON vehicles(price)
WHERE status = 'Active';
```

### 2. Explain Analyze

```sql
EXPLAIN ANALYZE
SELECT * FROM vehicles
WHERE make = 'Toyota'
  AND year >= 2020
  AND status = 'Active'
ORDER BY price
LIMIT 20;
```

### 3. Connection Pooling

```csharp
builder.Services.AddDbContext<VehicleDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.SetPostgresVersion(16, 0);
        npgsqlOptions.EnableRetryOnFailure(3);
    }),
    ServiceLifetime.Scoped,
    ServiceLifetime.Singleton // Connection pool es singleton
);
```

**appsettings.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=password;Minimum Pool Size=10;Maximum Pool Size=100;Connection Lifetime=300;"
  }
}
```

### 4. Bulk Operations

```csharp
// Bulk insert (mejor performance que Add múltiples veces)
await _context.Vehicles.AddRangeAsync(vehiclesList);
await _context.SaveChangesAsync();

// Bulk update con EF Core Extensions
await _context.Vehicles
    .Where(v => v.Status == "Pending")
    .ExecuteUpdateAsync(s => s
        .SetProperty(v => v.Status, "Active")
        .SetProperty(v => v.UpdatedAt, DateTime.UtcNow)
    );

// Bulk delete
await _context.Vehicles
    .Where(v => v.CreatedAt < DateTime.UtcNow.AddYears(-2))
    .ExecuteDeleteAsync();
```

---

## 💾 Backup y Recovery

### Backup Manual

```bash
# Backup de una base de datos
docker exec postgres_db pg_dump -U postgres vehiclessaleservice > backup_vehicles_$(date +%Y%m%d).sql

# Backup de todas las bases de datos
docker exec postgres_db pg_dumpall -U postgres > backup_all_$(date +%Y%m%d).sql

# Backup comprimido
docker exec postgres_db pg_dump -U postgres -Fc vehiclessaleservice > backup_vehicles_$(date +%Y%m%d).dump
```

### Restore

```bash
# Restore desde SQL
docker exec -i postgres_db psql -U postgres vehiclessaleservice < backup_vehicles_20260115.sql

# Restore desde dump comprimido
docker exec -i postgres_db pg_restore -U postgres -d vehiclessaleservice backup_vehicles_20260115.dump
```

### Backup Automático (Kubernetes)

```yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: postgres-backup
spec:
  schedule: "0 2 * * *" # Diario a las 2 AM
  jobTemplate:
    spec:
      template:
        spec:
          containers:
            - name: backup
              image: postgres:16
              command:
                - /bin/bash
                - -c
                - |
                  pg_dumpall -h postgres.okla.svc.cluster.local -U postgres > /backup/backup_$(date +%Y%m%d).sql
                  # Upload to S3
                  aws s3 cp /backup/backup_$(date +%Y%m%d).sql s3://okla-backups/postgres/
          restartPolicy: OnFailure
```

---

## 🔐 Seguridad

### 1. Crear Usuario Limitado

```sql
-- Crear usuario para servicio específico
CREATE USER vehiclesale_user WITH PASSWORD 'strong_password';

-- Otorgar permisos solo a su DB
GRANT CONNECT ON DATABASE vehiclessaleservice TO vehiclesale_user;
GRANT USAGE ON SCHEMA public TO vehiclesale_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO vehiclesale_user;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO vehiclesale_user;
```

### 2. SSL/TLS

```
Host=postgres.okla.com;Port=5432;Database=mydb;Username=postgres;Password=password;SSL Mode=Require;Trust Server Certificate=true;
```

### 3. Row-Level Security

```sql
-- Habilitar RLS
ALTER TABLE vehicles ENABLE ROW LEVEL SECURITY;

-- Política: usuarios solo ven sus propios vehículos
CREATE POLICY user_vehicles_policy ON vehicles
    FOR ALL
    USING (user_id = current_setting('app.current_user_id')::uuid);

-- Aplicar en código
await _context.Database.ExecuteSqlRawAsync(
    "SET app.current_user_id = {0}", userId
);
```

---

## 📊 Monitoring

### Métricas Importantes

```sql
-- Tamaño de bases de datos
SELECT datname, pg_size_pretty(pg_database_size(datname))
FROM pg_database
ORDER BY pg_database_size(datname) DESC;

-- Tamaño de tablas
SELECT tablename, pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename))
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC
LIMIT 10;

-- Queries lentos
SELECT pid, now() - pg_stat_activity.query_start AS duration, query
FROM pg_stat_activity
WHERE state = 'active' AND now() - pg_stat_activity.query_start > interval '5 seconds'
ORDER BY duration DESC;

-- Locks
SELECT * FROM pg_locks WHERE NOT granted;

-- Cache hit ratio (ideal >99%)
SELECT
  sum(heap_blks_read) as heap_read,
  sum(heap_blks_hit) as heap_hit,
  sum(heap_blks_hit) / (sum(heap_blks_hit) + sum(heap_blks_read)) * 100 AS ratio
FROM pg_statio_user_tables;
```

---

## 🧪 Testing

### Testcontainers para Integration Tests

```csharp
using Testcontainers.PostgreSql;

public class VehicleRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("test")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var optionsBuilder = new DbContextOptionsBuilder<VehicleDbContext>();
        optionsBuilder.UseNpgsql(_postgresContainer.GetConnectionString());

        await using var context = new VehicleDbContext(optionsBuilder.Options);
        await context.Database.MigrateAsync();
    }

    [Fact]
    public async Task CanInsertAndRetrieveVehicle()
    {
        // Test code
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
```

---

## 📚 Referencias

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
- [EF Core PostgreSQL Provider](https://www.npgsql.org/efcore/)
- [PostgreSQL Performance Tips](https://wiki.postgresql.org/wiki/Performance_Optimization)

---

**Implementado en:** Todos los microservicios  
**Versión:** 16  
**Última actualización:** Enero 15, 2026
