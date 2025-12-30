# 📦 CarDealer.Shared - Librerías Compartidas

Librerías compartidas para todos los microservicios de CarDealer. Proporciona utilidades comunes para:

- 🔐 **Gestión de Secretos** - Abstracción para leer secretos de múltiples fuentes
- 🗄️ **Base de Datos** - Soporte multi-provider (PostgreSQL, SQL Server, Oracle)
- 🏢 **Multi-Tenancy** - Soporte para arquitectura multi-tenant
- 📝 **Logging y Middleware** - Componentes compartidos

---

## 🔐 Sistema de Secretos

El sistema de secretos permite que los microservicios lean configuración sensible de múltiples fuentes de forma transparente.

### Fuentes de Secretos (en orden de prioridad)

1. **Variables de Entorno** - Mayor prioridad, permite override en runtime
2. **Docker Secrets** - Lee de `/run/secrets/` (Docker Swarm/Kubernetes)
3. **Proveedores adicionales** - Vault, Azure Key Vault, etc.

### Uso Básico

```csharp
// En Program.cs
var builder = WebApplication.CreateBuilder(args);

// Añadir sistema de secretos
builder.Services.AddSecretProvider();

// Usar secretos
var app = builder.Build();
var secretProvider = app.Services.GetRequiredService<ISecretProvider>();

// Obtener un secreto
var jwtKey = secretProvider.GetRequiredSecret("JWT_SECRET_KEY");

// Obtener con valor por defecto
var redisHost = secretProvider.GetSecretOrDefault("REDIS_HOST", "localhost");
```

### Configuración de Base de Datos con Secretos

```csharp
// Usar secretos para connection string (RECOMENDADO para producción)
builder.Services.AddDatabaseFromSecrets<MyDbContext>(
    builder.Configuration,
    servicePrefix: "AUTH"); // Busca AUTH_DATABASE_HOST, AUTH_DATABASE_PASSWORD, etc.

// O el método tradicional con appsettings (solo desarrollo)
builder.Services.AddDatabaseProvider<MyDbContext>(builder.Configuration);
```

### Variables de Entorno Requeridas

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `DATABASE_CONNECTION_STRING` | Connection string completo | `Host=db;Database=mydb;...` |
| `DATABASE_HOST` | Host de la DB (alternativa) | `localhost` |
| `DATABASE_PORT` | Puerto de la DB | `5432` |
| `DATABASE_NAME` | Nombre de la DB | `authservice` |
| `DATABASE_USER` | Usuario | `postgres` |
| `DATABASE_PASSWORD` | Contraseña | `secret` |
| `JWT_SECRET_KEY` | Clave para JWT | `min-32-characters` |
| `REDIS_CONNECTION_STRING` | Redis connection | `redis:6379` |
| `RABBITMQ_HOST` | RabbitMQ host | `rabbitmq` |

### Secretos con Prefijo por Servicio

Para evitar colisiones entre servicios:

```bash
# AuthService
AUTH_DATABASE_HOST=authdb
AUTH_DATABASE_PASSWORD=secret1

# NotificationService  
NOTIFICATION_DATABASE_HOST=notificationdb
NOTIFICATION_DATABASE_PASSWORD=secret2
```

```csharp
// En AuthService
builder.Services.AddDatabaseFromSecrets<AuthDbContext>(
    builder.Configuration,
    servicePrefix: "AUTH");
```

---

## 🗄️ Configuración de Base de Datos

### appsettings.json (sin secretos)

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "AutoMigrate": false,
    "CommandTimeout": 30,
    "MaxRetryCount": 3,
    "MaxRetryDelay": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false
  }
}
```

### Proveedores Soportados

- `PostgreSQL` - Usando Npgsql
- `SqlServer` - Usando Microsoft.Data.SqlClient
- `Oracle` - Usando Oracle.ManagedDataAccess
- `InMemory` - Solo para testing

---

## 🐳 Uso con Docker

### docker-compose.yml

```yaml
services:
  myservice:
    image: myservice:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DATABASE_CONNECTION_STRING=Host=db;Database=mydb;Username=user;Password=${DB_PASSWORD}
      - JWT_SECRET_KEY=${JWT_SECRET}
    secrets:
      - db_password
      - jwt_secret

secrets:
  db_password:
    external: true
  jwt_secret:
    external: true
```

### Usando Docker Secrets

Los secretos se montan en `/run/secrets/`:

```bash
# Crear secreto
echo "my-secret-password" | docker secret create db_password -

# El servicio puede leer desde /run/secrets/db_password
```

---

## 📋 SecretKeys - Constantes

Use las constantes de `SecretKeys` para evitar errores de tipeo:

```csharp
using CarDealer.Shared.Secrets;

var jwtKey = secretProvider.GetRequiredSecret(SecretKeys.JwtSecretKey);
var dbConn = secretProvider.GetSecret(SecretKeys.DatabaseConnectionString);
```

---

## ✅ Validación de Secretos al Startup

```csharp
// Validar que todos los secretos requeridos existan
var secretProvider = app.Services.GetRequiredService<ISecretProvider>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

if (!secretProvider.TryValidateRequiredSecrets(logger,
    SecretKeys.DatabaseConnectionString,
    SecretKeys.JwtSecretKey))
{
    // Fallar rápido si faltan secretos
    throw new InvalidOperationException("Missing required secrets");
}
```

---

## 🔄 Migración desde appsettings con secretos

### Antes (NO recomendado)

```json
{
  "Database": {
    "ConnectionStrings": {
      "PostgreSQL": "Host=db;Password=HARDCODED_PASSWORD;..."
    }
  },
  "Jwt": {
    "Key": "HARDCODED_SECRET_KEY"
  }
}
```

### Después (RECOMENDADO)

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "AutoMigrate": false
  }
}
```

```bash
# Variables de entorno (o Docker secrets)
DATABASE_CONNECTION_STRING=Host=db;Password=${SECRET};...
JWT_SECRET_KEY=${JWT_SECRET}
```

---

## 📚 Estructura del Proyecto

```
CarDealer.Shared/
├── Database/
│   ├── DatabaseConfiguration.cs      # Configuración desde appsettings
│   ├── DatabaseExtensions.cs         # Método tradicional (appsettings)
│   ├── DatabaseSecretExtensions.cs   # Método con secretos (NUEVO)
│   ├── DatabaseMigrationService.cs   # Auto-migración
│   └── DatabaseProvider.cs           # Enum de proveedores
├── Secrets/
│   ├── ISecretProvider.cs            # Interface principal
│   ├── EnvironmentSecretProvider.cs  # Lee de ENV vars
│   ├── DockerSecretProvider.cs       # Lee de /run/secrets/
│   ├── CompositeSecretProvider.cs    # Combina múltiples fuentes
│   ├── SecretProviderExtensions.cs   # DI extensions
│   ├── SecretKeys.cs                 # Constantes de nombres
│   └── ConnectionStringBuilder.cs    # Helpers para construir connections
├── Middleware/
├── MultiTenancy/
└── Services/
```

---

## 🚀 Quick Start

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 1. Añadir sistema de secretos
builder.Services.AddSecretProvider();

// 2. Configurar DB desde secretos
builder.Services.AddDatabaseFromSecrets<MyDbContext>(builder.Configuration);

// 3. Resto de la configuración...
var app = builder.Build();

// 4. Validar secretos requeridos
var secrets = app.Services.GetRequiredService<ISecretProvider>();
secrets.ValidateRequiredSecrets(
    SecretKeys.DatabaseConnectionString,
    SecretKeys.JwtSecretKey);

app.Run();
```

---

*Última actualización: Diciembre 2025*
