# 🤖 GitHub Copilot Instructions - CarDealer Microservices

Este documento proporciona contexto completo para que GitHub Copilot pueda asistir efectivamente en el desarrollo de este proyecto de marketplace multi-vertical.

---

## 📋 RESUMEN DEL PROYECTO

**CarDealer** es una plataforma **SaaS multi-tenant** de marketplace para compra y venta de vehículos (extensible a otros verticales como bienes raíces). Implementa una arquitectura de **microservicios** con Clean Architecture.

### Stack Tecnológico Principal

| Capa | Tecnología | Versión |
|------|------------|---------|
| **Backend** | .NET 8.0 LTS | net8.0 |
| **Frontend Web** | React 19 + TypeScript + Vite | ^19.0.0 |
| **Frontend Mobile** | Flutter + Dart | SDK >=3.2.0 |
| **Base de Datos** | PostgreSQL (principal), SQL Server, Oracle | 16+ |
| **Cache** | Redis (StackExchange.Redis) | 2.8.22 |
| **Message Broker** | RabbitMQ | 6.8.1 |
| **API Gateway** | Ocelot | 22.0.1 |
| **Service Discovery** | Consul | 1.7.14.9 |
| **Observabilidad** | OpenTelemetry, Serilog, Prometheus, Grafana | 1.14.0 |
| **Contenedores** | Docker + Docker Compose | - |

---

## 🤖 CONFIGURACIÓN DEL MODELO AI Y ESTIMACIÓN DE TOKENS

### Parámetros del Modelo

| Parámetro | Valor |
|-----------|-------|
| **Modelo** | Claude Opus 4.5 |
| **Context Window (Input)** | 128,000 tokens |
| **Max Output** | 16,000 tokens |
| **Multiplier** | 1x |
| **Tokens Útiles por Sesión** | ~110,000 tokens (reservando 18k para sistema/instrucciones) |

### ⚠️ REGLA OBLIGATORIA: Estimación Antes de Ejecutar

**ANTES de ejecutar cualquier tarea, SIEMPRE debes:**

1. **Estimar tokens de entrada** (código a leer + contexto)
2. **Estimar tokens de salida** (código a generar)
3. **Determinar si cabe en una sesión** (total < 110,000 input + 16,000 output)
4. **Dividir en subtareas si excede los límites**

### Fórmulas de Estimación

```
Tokens de Lectura = (Líneas de código × 4) + (Archivos × 500)
Tokens de Escritura = (Líneas nuevas/modificadas × 5)
Tokens de Contexto = Instrucciones + Historial (~8,000 base)
Buffer de Seguridad = 15%

Total Estimado = (Lectura + Escritura + Contexto) × 1.15
```

### Factores de Complejidad

| Nivel | Multiplicador | Descripción |
|-------|--------------|-------------|
| Simple | 1.0x | Cambios menores, archivos pequeños |
| Medio | 1.3x | Múltiples archivos, lógica moderada |
| Complejo | 1.6x | Refactoring, nuevos patterns |
| Muy Complejo | 2.0x | Arquitectura, múltiples sistemas |

### Tabla de Referencia Rápida

| Tipo de Tarea | Tokens Est. | ¿Cabe en 1 sesión? |
|---------------|-------------|---------------------|
| Actualizar 1 paquete | ~5,000 | ✅ Sí |
| Crear 1 archivo nuevo (~100 líneas) | ~8,000 | ✅ Sí |
| Modificar 3-5 archivos relacionados | ~15,000 | ✅ Sí |
| Crear feature CQRS completa | ~25,000 | ✅ Sí |
| Refactoring de módulo completo | ~45,000 | ✅ Sí (límite) |
| Breaking change (ej: Firebase 2→3) | ~85,000 | ⚠️ Dividir en 4-5 subtareas |
| Nuevo microservicio completo | ~120,000 | 🔴 Dividir en 6-8 subtareas |

### Proceso de División de Tareas

Si una tarea excede **80,000 tokens** (margen de seguridad), dividir así:

```
Tarea Grande (120k tokens)
├── Subtarea 1: Estructura base (~20k)
├── Subtarea 2: Entities y Models (~18k)
├── Subtarea 3: Repositories (~18k)
├── Subtarea 4: Use Cases (~20k)
├── Subtarea 5: Controllers/API (~22k)
└── Subtarea 6: Tests (~22k)
```

### Ejemplo de Estimación

```markdown
## Tarea: Implementar VehicleRemoteDataSource

**Estimación:**
- Archivos a leer: 5 (~400 líneas)
- Archivos a crear/modificar: 3 (~200 líneas)
- Complejidad: Medio (1.3x)

**Cálculo:**
Lectura: 400 × 4 + 5 × 500 = 4,100 tokens
Escritura: 200 × 5 = 1,000 tokens
Contexto: 8,000 tokens
Total: (4,100 + 1,000 + 8,000) × 1.15 × 1.3 = ~19,600 tokens

**Decisión:** ✅ Cabe en 1 sesión
```

### Planes de Sprint Disponibles

Los planes detallados con estimaciones de tokens están en:

| Documento | Tokens Totales | Sesiones Est. |
|-----------|----------------|---------------|
| `BACKEND_IMPROVEMENT_SPRINT_PLAN.md` | ~482,000 | 25-26 |
| `FRONTEND_IMPROVEMENT_SPRINT_PLAN.md` | ~371,300 | 20 |
| `MOBILE_IMPROVEMENT_SPRINT_PLAN.md` | ~463,000 | 25-30 |

---

## 🏗️ ESTRUCTURA DEL PROYECTO

> **Estado Docker (31 Dic 2025):** ✅ **Todos los 35 microservicios tienen Dockerfile Y están en docker-compose.yml**  
> **✅ AuthService FUNCIONAL:** Dockerfile.dev corregido (dotnet build + dotnet run), variables de entorno Database__* configuradas  
> **✅ Credenciales de Prueba:** `test@example.com` / `Admin123!` (email confirmado, login funcional)

```
cardealer-microservices/
├── backend/                          # Microservicios .NET 8 (35 servicios)
│   ├── _Shared/                      # Librerías compartidas
│   │   ├── CarDealer.Contracts/      # DTOs y Events para comunicación
│   │   └── CarDealer.Shared/         # Utilidades y Multi-tenancy
│   ├── _Tests/IntegrationTests/      # Tests de integración
│   ├── Gateway/                      # API Gateway (Ocelot) ✅
│   ├── ServiceDiscovery/             # Consul integration ✅
│   ├── AuthService/                  # Autenticación y autorización ✅🟢 FUNCIONAL
│   ├── UserService/                  # Gestión de usuarios ✅
│   ├── RoleService/                  # Gestión de roles y permisos ✅
│   ├── ProductService/               # Productos genéricos marketplace ✅
│   ├── MediaService/                 # Gestión de archivos multimedia ✅
│   ├── NotificationService/          # Email, SMS, Push notifications ✅
│   ├── BillingService/               # Facturación y pagos ✅
│   ├── CRMService/                   # Gestión de clientes ✅
│   ├── ErrorService/                 # Centralización de errores ✅
│   ├── AuditService/                 # Auditoría y compliance ✅
│   ├── CacheService/                 # Cache distribuido ✅
│   ├── MessageBusService/            # RabbitMQ abstraction ✅
│   ├── SchedulerService/             # Jobs con Hangfire ✅
│   ├── SearchService/                # Búsqueda (Elasticsearch) ✅
│   ├── ReportsService/               # Reportes y analytics ✅
│   ├── HealthCheckService/           # Health monitoring ✅
│   ├── LoggingService/               # Logging centralizado ✅
│   ├── TracingService/               # Distributed tracing ✅
│   ├── ConfigurationService/         # Configuración dinámica ✅
│   ├── FeatureToggleService/         # Feature flags ✅
│   ├── FileStorageService/           # S3/Azure Blob storage ✅
│   ├── BackupDRService/              # Backup y Disaster Recovery ✅
│   ├── MarketingService/             # Campañas marketing ✅
│   ├── IntegrationService/           # Integraciones externas ✅
│   ├── FinanceService/               # Finanzas y contabilidad ✅
│   ├── InvoicingService/             # Facturación electrónica ✅
│   ├── ContactService/               # Gestión de contactos ✅
│   ├── AppointmentService/           # Citas y agenda ✅
│   ├── AdminService/                 # Panel de administración ✅
│   ├── ApiDocsService/               # Documentación API ✅
│   ├── RateLimitingService/          # Rate limiting ✅
│   ├── IdempotencyService/           # Idempotencia ✅
│   ├── RealEstateService/            # Vertical inmobiliario ✅
│   ├── observability/                # Configs OpenTelemetry
│   └── monitoring/                   # Prometheus/Grafana configs
│   # ✅ = En docker-compose.yml (35/35 servicios) | 🟢 = Probado y funcional
│
├── frontend/
│   ├── web/                          # React 19 + Vite + TailwindCSS
│   │   ├── src/
│   │   ├── cardealer/                # App CarDealer
│   │   └── okla/                     # App alternativa OKLA
│   ├── mobile/cardealer/             # Flutter app
│   │   ├── lib/
│   │   │   ├── core/                 # Core utilities
│   │   │   ├── data/                 # Data layer (repos, datasources)
│   │   │   ├── domain/               # Domain layer (entities, usecases)
│   │   │   └── presentation/         # UI (pages, widgets, blocs)
│   │   ├── android/
│   │   └── ios/
│   └── shared/                       # Componentes compartidos
│
├── policies/                         # Políticas de seguridad
├── scripts/                          # Scripts de utilidad
├── compose.yaml                      # Docker Compose principal
└── cardealer.sln                     # Solución .NET
```

---

## 🎯 ARQUITECTURA POR MICROSERVICIO

Cada microservicio sigue **Clean Architecture**:

```
{ServiceName}/
├── {ServiceName}.Api/                # Capa de presentación
│   ├── Controllers/                  # REST Controllers
│   ├── Middleware/                   # Custom middleware
│   ├── Program.cs                    # Entry point
│   └── appsettings.json
├── {ServiceName}.Application/        # Capa de aplicación
│   ├── Features/                     # CQRS con MediatR
│   │   ├── Commands/
│   │   └── Queries/
│   ├── DTOs/
│   ├── Validators/                   # FluentValidation
│   └── Common/Behaviours/            # Pipeline behaviors
├── {ServiceName}.Domain/             # Capa de dominio
│   ├── Entities/                     # Entidades de dominio
│   ├── ValueObjects/
│   ├── Events/                       # Domain events
│   ├── Interfaces/
│   ├── Enums/
│   └── Exceptions/
├── {ServiceName}.Infrastructure/     # Capa de infraestructura
│   ├── Persistence/                  # DbContext, Repositories
│   ├── Services/                     # Implementaciones externas
│   ├── Messaging/                    # RabbitMQ publishers/consumers
│   └── Extensions/                   # DI extensions
├── {ServiceName}.Shared/             # DTOs compartidos (opcional)
├── {ServiceName}.Tests/              # Unit tests
├── Dockerfile
└── {ServiceName}.sln
```

---

## 📦 PATRONES Y CONVENCIONES

### 1. CQRS con MediatR

```csharp
// Command
public record CreateUserCommand(string Email, string Password) : IRequest<Result<UserDto>>;

// Handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Implementación
    }
}
```

### 2. Domain Events (Event-Driven)

```csharp
// Definir evento en CarDealer.Contracts
public class UserRegisteredEvent : EventBase
{
    public override string EventType => "auth.user.registered";
    public Guid UserId { get; set; }
    public string Email { get; set; }
}

// Publicar via RabbitMQ
await _eventPublisher.PublishAsync(new UserRegisteredEvent { ... });
```

### 3. Multi-Tenancy

Todas las entidades multi-tenant implementan `ITenantEntity`:

```csharp
public class Product : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }  // Tenant ID
    // ...
}
```

### 4. Result Pattern

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    
    public static Result<T> Success(T value) => new(value, null, true);
    public static Result<T> Failure(string error) => new(default, error, false);
}
```

### 5. Repository Pattern

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}
```

---

## 🔐 AUTENTICACIÓN Y AUTORIZACIÓN

- **JWT Bearer Tokens** con refresh tokens
- **ASP.NET Core Identity** para gestión de usuarios
- **2FA** con TOTP (Otp.NET + QRCoder)
- **OAuth2** con Google y Microsoft
- **Rate Limiting** por IP y usuario
- **CORS** configurado por entorno

### Claims estándar

```csharp
public static class ClaimTypes
{
    public const string UserId = "sub";
    public const string Email = "email";
    public const string DealerId = "dealer_id";    // Tenant
    public const string Role = "role";
    public const string Permissions = "permissions";
}
```

---

## 📡 COMUNICACIÓN ENTRE SERVICIOS

### ❌ NO hacer (Anti-pattern)

```csharp
// NUNCA llamar directamente entre servicios
var response = await _httpClient.GetAsync("http://authservice/api/users/123");
```

### ✅ SÍ hacer

```csharp
// 1. Via Gateway (para clientes externos)
// Cliente → Gateway → Servicio

// 2. Via RabbitMQ (entre servicios)
await _eventPublisher.PublishAsync(new UserRegisteredEvent { UserId = user.Id });
```

### Exchanges y Queues (RabbitMQ)

| Exchange | Tipo | Descripción |
|----------|------|-------------|
| `cardealer.events` | topic | Eventos de dominio |
| `cardealer.commands` | direct | Comandos directos |
| `cardealer.dlx` | fanout | Dead Letter Exchange |

---

## 🗄️ BASE DE DATOS

### Configuración Multi-Provider

```json
// appsettings.json
{
  "Database": {
    "Provider": "PostgreSQL",  // PostgreSQL, SqlServer, Oracle
    "Host": "localhost",
    "Port": 5432,
    "Database": "authservice",
    "Username": "postgres",
    "Password": "password",
    "AutoMigrate": true
  }
}
```

```csharp
// Program.cs
builder.Services.AddDatabaseProvider<ApplicationDbContext>(builder.Configuration);
```

### Migraciones

```powershell
# Crear migración
dotnet ef migrations add InitialCreate -p AuthService.Infrastructure -s AuthService.Api

# Aplicar migración
dotnet ef database update -p AuthService.Infrastructure -s AuthService.Api
```

---

## 🔄 PROCESO DE COMPILACIÓN Y TESTING DE MICROSERVICIOS

### ⚠️ FLUJO OBLIGATORIO - Orden de Ejecución

**SIEMPRE seguir este proceso en orden para cada microservicio:**

### 1️⃣ COMPILACIÓN LOCAL (Capa por Capa)

**ANTES de crear o probar contenedores Docker, SIEMPRE compilar localmente:**

```powershell
# Navegar al servicio
cd backend/{ServiceName}

# Compilar cada capa en orden de dependencias
dotnet build {ServiceName}.Domain/{ServiceName}.Domain.csproj
dotnet build {ServiceName}.Application/{ServiceName}.Application.csproj  
dotnet build {ServiceName}.Infrastructure/{ServiceName}.Infrastructure.csproj
dotnet build {ServiceName}.Api/{ServiceName}.Api.csproj

# O compilar todo el servicio (más rápido si todas las capas están OK)
dotnet build {ServiceName}.sln
```

**✅ Validación Exitosa:** 
- `Build succeeded. 0 Error(s)`
- **Solo si NO hay errores**, proceder al siguiente paso

**❌ Si hay errores:**
- **NO crear Docker images**
- **NO levantar contenedores**
- Corregir errores en el código primero
- Repetir compilación local hasta 0 errores

### 2️⃣ CREACIÓN DE IMAGEN DOCKER

**Solo después de compilación local exitosa:**

```powershell
# Build de imagen Docker
docker build -t cardealer-microservices-{servicename}:latest \
  -f backend/{ServiceName}/{ServiceName}.Api/Dockerfile.dev \
  backend

# Verificar imagen creada
docker images | Select-String "{servicename}"
```

### 3️⃣ PRUEBA DE COMPILACIÓN EN DOCKER

**Levantar contenedor y verificar compilación dentro de Docker:**

```powershell
# Iniciar contenedor
docker-compose up -d {servicename}

# O manualmente si no está en compose.yaml
docker run -d --name {servicename} \
  --network cardealer-microservices_cargurus-net \
  -p {port}:80 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e "ConnectionStrings__DefaultConnection=Host={servicename}-db;..." \
  cardealer-microservices-{servicename}:latest

# Esperar compilación (60-100 segundos sin watch mode)
Start-Sleep -Seconds 90

# Verificar logs - NO debe haber errores
docker logs {servicename} --tail 50
```

**🔍 Signos de Compilación Exitosa:**
- Logs muestran: `"Now listening on: http://[::]:80"`
- No hay excepciones de tipo `System.*Exception`
- No hay errores de DI (Dependency Injection)
- No hay errores de EF Core migrations

**❌ Si hay errores en Docker:**
- Revisar logs completos: `docker logs {servicename}`
- Problema común: `dotnet watch` puede congelarse → Usar `dotnet run` en Dockerfile
- Verificar variables de entorno y conexión a DB
- Si persiste: Bajar contenedor, corregir código local, recompilar, rebuild Docker

### 4️⃣ PRUEBAS DE API (Solo cuando servicio esté UP)

**Solo después de que el contenedor esté HEALTHY:**

```powershell
# Verificar health check
Invoke-WebRequest "http://localhost:{port}/health" -UseBasicParsing

# Verificar Swagger UI
Invoke-WebRequest "http://localhost:{port}/swagger" -UseBasicParsing

# Contar endpoints disponibles
$swagger = Invoke-WebRequest "http://localhost:{port}/swagger/v1/swagger.json" -UseBasicParsing
$json = $swagger.Content | ConvertFrom-Json
$endpointCount = ($json.paths.PSObject.Properties | ForEach-Object { 
  $_.Value.PSObject.Properties.Count 
} | Measure-Object -Sum).Sum
Write-Host "✅ $endpointCount endpoints operacionales"

# Probar endpoint específico (ejemplo)
Invoke-WebRequest "http://localhost:{port}/api/{resource}?page=1&pageSize=5" -UseBasicParsing
```

---

### 🎯 OPTIMIZACIÓN DE RECURSOS - Gestión de Contenedores

**⚠️ IMPORTANTE:** PC con recursos limitados (~8GB RAM) - NO correr todos los contenedores simultáneamente.

#### Estrategia: Levantar Solo lo Necesario

**Antes de iniciar un test, identificar dependencias:**

```powershell
# 1. Listar todos los contenedores corriendo
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# 2. Identificar servicios NO necesarios para la prueba actual
# Ejemplo: Si vas a probar CRMService, NO necesitas:
# - ProductService, NotificationService, SearchService, etc.

# 3. Bajar servicios innecesarios
docker stop productservice notificationservice searchservice
# O usar docker-compose
docker-compose stop productservice notificationservice searchservice

# 4. Verificar liberación de recursos
docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}"
```

#### Perfiles de Contenedores por Tipo de Prueba

**A. Prueba de Servicio Individual (Mínimo):**
```powershell
# Solo levantar:
# - Servicio a probar
# - Su base de datos
# - Redis (si el servicio usa cache)
# - RabbitMQ (si usa mensajería)

# Ejemplo para CRMService:
docker-compose up -d redis rabbitmq crmservice-db crmservice

# Bajar todo lo demás:
docker-compose stop $(docker-compose ps --services | Where-Object { $_ -notmatch "redis|rabbitmq|crmservice" })
```

**B. Prueba de Integración entre 2-3 Servicios:**
```powershell
# Ejemplo: AuthService + ErrorService + Gateway
docker-compose up -d redis rabbitmq \
  authservice-db authservice \
  errorservice-db errorservice \
  gateway
```

**C. Prueba Completa del Sistema (Requiere más RAM):**
```powershell
# Solo para validación final - consume ~14GB RAM
docker-compose up -d
```

#### Limpieza Post-Prueba

```powershell
# Bajar servicios probados (libera RAM inmediatamente)
docker-compose stop {servicename}

# Ver recursos liberados
docker stats --no-stream

# Limpiar contenedores detenidos (opcional)
docker container prune -f

# Limpiar imágenes antiguas (opcional)
docker image prune -f
```

---

### 🐛 TROUBLESHOOTING - Problemas Comunes

| Problema | Causa | Solución |
|----------|-------|----------|
| Compilación local OK, Docker FAIL | `dotnet watch` se congela | Cambiar ENTRYPOINT a `dotnet run` en Dockerfile |
| "Column does not exist" en API | Migraciones EF desincronizadas | Regenerar migraciones desde cero |
| Servicio no responde después de 60s | Aún compilando o error silencioso | Esperar 30s más o revisar `docker logs` |
| `IHttpClientFactory` no registrado | Falta `AddHttpClient()` en DI | Agregar en Program.cs |
| Contenedor crashea al inicio | Middleware requiere DB migrada | Comentar middleware o agregar auto-migration |
| Docker consume toda la RAM | Demasiados contenedores activos | Bajar servicios innecesarios (ver perfil mínimo) |

---

### ✅ CHECKLIST DE VALIDACIÓN

Antes de considerar un microservicio "listo":

- [ ] ✅ Compilación local sin errores (capa por capa)
- [ ] ✅ Imagen Docker creada exitosamente
- [ ] ✅ Contenedor inicia sin errores en logs
- [ ] ✅ Health check responde 200 OK
- [ ] ✅ Swagger UI accesible
- [ ] ✅ Endpoints cuentan correctamente (>0)
- [ ] ✅ API responde a requests (aunque sea 401 Unauthorized)
- [ ] ✅ Base de datos conectada y con tablas migradas
- [ ] ✅ No hay errores en logs después de 2 minutos de ejecución

**Solo después de cumplir TODOS los puntos, el servicio se considera operacional.**

---

## 🧪 TESTING

### Stack de Testing

| Tipo | Framework |
|------|-----------|
| Unit Tests | xUnit 2.7+ |
| Mocking | Moq 4.20+ |
| Assertions | FluentAssertions 6.12+ |
| Integration | Testcontainers 3.9+ |
| API Testing | Microsoft.AspNetCore.Mvc.Testing |
| Fake Data | Bogus 35.5+ |

### Estructura de Tests

```csharp
public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo;
    private readonly CreateUserCommandHandler _handler;
    
    [Fact]
    public async Task Handle_ValidUser_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateUserCommand("test@email.com", "Password123!");
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
```

---

## 🐳 DOCKER

### Comandos frecuentes

```powershell
# Levantar todo el stack
docker-compose up -d

# Solo backend
docker-compose -f backend/docker-compose.yml up -d

# Con observabilidad
docker-compose -f backend/observability/docker-compose.observability.yml up -d

# Logs de un servicio
docker logs -f authservice
```

### Puertos importantes

| Servicio | Puerto | Estado |
|----------|--------|--------|
| Gateway | 18443 | ✅ |
| AuthService | 15085 | ✅ |
| ErrorService | 15083 | ✅ |
| NotificationService | 15084 | ✅ |
| ProductService | 15006 | ✅ |
| UserService | 15100 | ❌ Falta en compose |
| RoleService | 15101 | ❌ Falta en compose |
| Redis | 6379 | ✅ |
| RabbitMQ | 5672, 15672 (UI) | ✅ |
| PostgreSQL | 25432-25446 | ✅ |
| Consul | 8500 | ✅ |
| Prometheus | 9090 | ⚪ |
| Grafana | 3000 | ⚪ |
| Jaeger | 16686 | ⚪ |

> ✅ = Configurado y funcionando | ❌ = Falta en docker-compose | ⚪ = No desplegado

### ⚠️ LÍMITES DE RECURSOS OBLIGATORIOS PARA DOCKER

**CONTEXTO:** El entorno de desarrollo tiene recursos limitados (~8GB RAM, 8 CPUs). Todos los 35 microservicios deben poder correr simultáneamente en Docker para pruebas de integración. Por lo tanto, **SIEMPRE** que se cree o modifique un servicio en `compose.yaml`, se DEBEN incluir límites de recursos.

#### Límites Estándar por Tipo de Servicio

| Tipo de Servicio | CPU Límite | RAM Límite | RAM Reservada |
|------------------|------------|------------|---------------|
| **PostgreSQL DB** | 0.25 | 256M | 128M |
| **Redis** | 0.1 | 128M | 64M |
| **RabbitMQ** | 0.25 | 256M | 128M |
| **API .NET (Microservicio)** | 0.5 | 384M | 256M |
| **Gateway** | 0.25 | 256M | 128M |
| **Elasticsearch** | 0.5 | 512M | 256M |
| **Consul** | 0.1 | 128M | 64M |

#### 📋 Recursos por Microservicio (35 servicios)

Basado en la complejidad del código (archivos .cs) y funcionalidad, cada servicio tiene asignados recursos específicos:

##### 🔴 Servicios CORE (Alta prioridad, más recursos)

| Servicio | Archivos | CPU | RAM | RAM Res. | Requiere DB | Descripción |
|----------|:--------:|:---:|:---:|:--------:|:-----------:|-------------|
| **AuthService** | 283 | 0.5 | 384M | 256M | ✅ PostgreSQL | Autenticación, JWT, Identity, 2FA |
| **Gateway** | 39 | 0.25 | 256M | 128M | ❌ | API Gateway con Ocelot |
| **UserService** | 143 | 0.5 | 384M | 256M | ✅ PostgreSQL | Gestión de usuarios |
| **RoleService** | 170 | 0.5 | 384M | 256M | ✅ PostgreSQL | Roles y permisos |
| **ProductService** | 53 | 0.5 | 384M | 256M | ✅ PostgreSQL | Productos del marketplace |
| **NotificationService** | 163 | 0.5 | 384M | 256M | ✅ PostgreSQL | Email, SMS, Push (SendGrid, Twilio) |
| **ErrorService** | 112 | 0.5 | 384M | 256M | ✅ PostgreSQL | Centralización de errores |

##### 🟡 Servicios SECUNDARIOS (Uso moderado)

| Servicio | Archivos | CPU | RAM | RAM Res. | Requiere DB | Descripción |
|----------|:--------:|:---:|:---:|:--------:|:-----------:|-------------|
| **MediaService** | 146 | 0.4 | 320M | 192M | ✅ PostgreSQL | Archivos multimedia, S3/Azure |
| **AdminService** | 119 | 0.4 | 320M | 192M | ✅ PostgreSQL | Panel de administración |
| **AuditService** | 86 | 0.3 | 256M | 128M | ✅ PostgreSQL | Auditoría y compliance |
| **BillingService** | 51 | 0.3 | 256M | 128M | ✅ PostgreSQL | Facturación, Stripe |
| **ContactService** | 83 | 0.3 | 256M | 128M | ✅ PostgreSQL | Gestión de contactos |
| **CRMService** | 40 | 0.3 | 256M | 128M | ✅ PostgreSQL | CRM básico |
| **MessageBusService** | 85 | 0.3 | 256M | 128M | ❌ | Abstracción RabbitMQ |

##### 🟢 Servicios LIGEROS (Poco consumo)

| Servicio | Archivos | CPU | RAM | RAM Res. | Requiere DB | Descripción |
|----------|:--------:|:---:|:---:|:--------:|:-----------:|-------------|
| **ConfigurationService** | 61 | 0.2 | 192M | 96M | ✅ PostgreSQL | Config dinámica |
| **FeatureToggleService** | 71 | 0.2 | 192M | 96M | ✅ PostgreSQL | Feature flags |
| **HealthCheckService** | 36 | 0.15 | 128M | 64M | ❌ | Health monitoring |
| **LoggingService** | 55 | 0.2 | 192M | 96M | ✅ PostgreSQL | Logging centralizado |
| **TracingService** | 32 | 0.15 | 128M | 64M | ❌ | Distributed tracing |
| **CacheService** | 49 | 0.15 | 128M | 64M | ❌ | Proxy a Redis |
| **IdempotencyService** | 36 | 0.15 | 128M | 64M | ✅ PostgreSQL | Idempotencia |
| **RateLimitingService** | 53 | 0.2 | 192M | 96M | ❌ | Rate limiting |
| **ApiDocsService** | 25 | 0.1 | 128M | 64M | ❌ | Documentación API |
| **ServiceDiscovery** | 48 | 0.15 | 128M | 64M | ❌ | Integración Consul |

##### 🔵 Servicios ESPECIALIZADOS

| Servicio | Archivos | CPU | RAM | RAM Res. | Requiere DB | Descripción |
|----------|:--------:|:---:|:---:|:--------:|:-----------:|-------------|
| **SearchService** | 50 | 0.4 | 320M | 192M | ❌ (Elastic) | Búsqueda con Elasticsearch |
| **SchedulerService** | 54 | 0.3 | 256M | 128M | ✅ PostgreSQL | Jobs con Hangfire |
| **BackupDRService** | 76 | 0.3 | 256M | 128M | ✅ PostgreSQL | Backup y Disaster Recovery |
| **ReportsService** | 36 | 0.3 | 256M | 128M | ✅ PostgreSQL | Reportes y analytics |

##### ⚪ Servicios VERTICALES/NEGOCIO

| Servicio | Archivos | CPU | RAM | RAM Res. | Requiere DB | Descripción |
|----------|:--------:|:---:|:---:|:--------:|:-----------:|-------------|
| **RealEstateService** | 25 | 0.25 | 192M | 96M | ✅ PostgreSQL | Vertical inmobiliario |
| **FinanceService** | 42 | 0.25 | 192M | 96M | ✅ PostgreSQL | Finanzas y contabilidad |
| **InvoicingService** | 43 | 0.25 | 192M | 96M | ✅ PostgreSQL | Facturación electrónica |
| **AppointmentService** | 30 | 0.2 | 192M | 96M | ✅ PostgreSQL | Citas y agenda |
| **MarketingService** | 36 | 0.2 | 192M | 96M | ✅ PostgreSQL | Campañas marketing |
| **IntegrationService** | 36 | 0.2 | 192M | 96M | ✅ PostgreSQL | Integraciones externas |
| **FileStorageService** | 36 | 0.2 | 192M | 96M | ✅ PostgreSQL | S3/Azure Blob storage |

#### 📊 Resumen de Recursos Totales

| Categoría | Servicios | CPU Total | RAM Total |
|-----------|:---------:|:---------:|:---------:|
| 🔴 Core | 7 | 3.25 | 2.4GB |
| 🟡 Secundarios | 7 | 2.2 | 1.8GB |
| 🟢 Ligeros | 10 | 1.65 | 1.5GB |
| 🔵 Especializados | 4 | 1.3 | 1.1GB |
| ⚪ Verticales | 7 | 1.55 | 1.3GB |
| **APIs Total** | **35** | **10.0** | **~8.1GB** |
| PostgreSQL (×20) | 20 | 5.0 | 5.0GB |
| Redis | 1 | 0.1 | 128M |
| RabbitMQ | 1 | 0.25 | 256M |
| Elasticsearch | 1 | 0.5 | 512M |
| Consul | 1 | 0.1 | 128M |
| **TOTAL MÁXIMO** | - | **~16** | **~14GB** |

#### ⚠️ Estrategia para PC con 8GB RAM

**NO es posible correr todos los servicios simultáneamente.** Usar perfiles:

```powershell
# Perfil MÍNIMO (~2GB RAM) - Solo auth y errores
docker-compose up -d redis rabbitmq authservice-db authservice errorservice-db errorservice gateway

# Perfil BÁSICO (~3.5GB RAM) - Core funcional
docker-compose up -d redis rabbitmq \
  authservice-db authservice \
  errorservice-db errorservice \
  userservice-db userservice \
  notificationservice-db notificationservice \
  gateway

# Perfil DESARROLLO (~5GB RAM) - Con productos y media
docker-compose up -d redis rabbitmq \
  authservice-db authservice \
  errorservice-db errorservice \
  userservice-db userservice \
  productservice-db productservice \
  mediaservice-db mediaservice \
  notificationservice-db notificationservice \
  gateway
```

#### Template para compose.yaml

```yaml
# Para bases de datos PostgreSQL
service-db:
  image: postgres:16
  container_name: service-db
  deploy:
    resources:
      limits:
        cpus: '0.25'
        memory: 256M
      reservations:
        memory: 128M
  # ... resto de configuración

# Para APIs .NET
servicename:
  build:
    context: ./backend
    dockerfile: ServiceName/ServiceName.Api/Dockerfile.dev
  container_name: servicename
  deploy:
    resources:
      limits:
        cpus: '0.5'
        memory: 384M
      reservations:
        memory: 256M
  # ... resto de configuración

# Para Redis
redis:
  image: redis:7-alpine
  container_name: redis
  deploy:
    resources:
      limits:
        cpus: '0.1'
        memory: 128M
      reservations:
        memory: 64M
  # ... resto de configuración

# Para RabbitMQ
rabbitmq:
  image: rabbitmq:3.12-management
  container_name: rabbitmq
  deploy:
    resources:
      limits:
        cpus: '0.25'
        memory: 256M
      reservations:
        memory: 128M
  # ... resto de configuración
```

#### Monitoreo de Recursos

```powershell
# Ver uso de recursos de todos los contenedores
docker stats --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}"

# Ver contenedores que exceden límites
docker stats --no-stream --format "{{.Name}}: {{.MemPerc}}" | findstr /V "0.00%"
```

---

## 📱 FRONTEND WEB (React)

### Estructura

```
frontend/web/src/
├── components/           # Componentes reutilizables
├── pages/               # Páginas/Rutas
├── hooks/               # Custom hooks
├── services/            # API clients (axios)
├── stores/              # Zustand stores
├── utils/               # Utilidades
├── types/               # TypeScript types
└── i18n/                # Internacionalización
```

### Tech Stack

- **React 19** + TypeScript 5.6
- **Vite 7** para bundling
- **TailwindCSS 3.4** para estilos
- **Zustand 5** para state management
- **TanStack Query 5** para server state
- **React Router 7** para routing
- **React Hook Form + Zod** para forms
- **i18next** para i18n (ES/EN)

### Comandos

```powershell
cd frontend/web
npm install
npm run dev      # Development server
npm run build    # Production build
npm run test     # Vitest
```

### ⚠️ NOTAS CRÍTICAS TYPESCRIPT/MONOREPO

1. **Estructura Monorepo**: El proyecto usa npm workspaces. `node_modules` está en `frontend/` NO en `frontend/web/`

2. **Configuración de typeRoots**: Los tsconfig deben apuntar al directorio padre:
   ```json
   // tsconfig.app.json y tsconfig.node.json
   "typeRoots": ["../node_modules/@types"]
   ```

3. **Tipos de Vite y Node**: Usar triple-slash directives en lugar de `types` en tsconfig:
   ```typescript
   // src/vite-env.d.ts (DEBE existir)
   /// <reference types="vite/client" />
   
   // vite.config.ts (al inicio del archivo)
   /// <reference types="node" />
   ```

4. **verbatimModuleSyntax**: TypeScript 5.6 requiere imports de tipo explícitos:
   ```typescript
   // ❌ Incorrecto
   import { ReactNode, ErrorInfo } from 'react';
   
   // ✅ Correcto - usar 'import type' para tipos
   import type { ReactNode, ErrorInfo } from 'react';
   import { Component } from 'react';  // solo valores
   ```

5. **Dos archivos de tipos User**: Existen diferencias entre:
   - `src/types/index.ts` - Tipos locales simplificados
   - `src/shared/types/index.ts` - Tipos compartidos completos
   - **Importante**: User tiene `subscription` directamente, NO `dealer.subscription`

6. **AccountType**: Debe incluir `'guest'` como valor válido:
   ```typescript
   type AccountType = 'guest' | 'individual' | 'dealer' | 'dealer_employee' | 'admin' | 'platform_employee';
   ```

7. **Sentry browserTracingIntegration**: No usar `tracePropagationTargets` dentro del integration:
   ```typescript
   // ❌ Deprecated
   Sentry.browserTracingIntegration({
     tracePropagationTargets: [...]  // NO
   })
   
   // ✅ Correcto
   Sentry.browserTracingIntegration()
   // tracePropagationTargets va en Sentry.init() directamente
   ```

---

## 📱 FRONTEND MOBILE (Flutter)

### Arquitectura

- **Clean Architecture** con capas separadas
- **BLoC Pattern** para state management
- **GetIt + Injectable** para DI
- **Dio + Retrofit** para networking
- **Hive** para local storage

### Estructura

```
lib/
├── core/                # Utilidades, themes, constants
├── data/
│   ├── datasources/     # Remote y local datasources
│   ├── models/          # Data models (JSON serializable)
│   └── repositories/    # Repository implementations
├── domain/
│   ├── entities/        # Business entities
│   ├── repositories/    # Repository contracts
│   └── usecases/        # Use cases
├── presentation/
│   ├── blocs/           # BLoC state management
│   ├── pages/           # Screens
│   └── widgets/         # Reusable widgets
├── l10n/                # Localization
├── main.dart
├── main_dev.dart        # Flavor: development
├── main_staging.dart    # Flavor: staging
└── main_prod.dart       # Flavor: production
```

### Comandos

```powershell
cd frontend/mobile/cardealer
flutter pub get
flutter run                      # Debug
flutter run --flavor dev         # Dev flavor
flutter build apk --release      # Android release
flutter build ios --release      # iOS release
```

### ⚠️ NOTAS CRÍTICAS FLUTTER/DART (APIs que han cambiado)

1. **connectivity_plus**: El listener ahora retorna `ConnectivityResult` (single), NO `List<ConnectivityResult>`:
   ```dart
   // ❌ Incorrecto (API antigua)
   Connectivity().onConnectivityChanged.listen((List<ConnectivityResult> results) {
     final result = results.first;
   });
   
   // ✅ Correcto (API actual)
   Connectivity().onConnectivityChanged.listen((ConnectivityResult result) {
     // usar result directamente
   });
   ```

2. **fl_chart SideTitleWidget**: Usar `axisSide` en lugar de `meta`:
   ```dart
   // ❌ Incorrecto
   SideTitleWidget(meta: meta, child: Text('...'))
   
   // ✅ Correcto
   SideTitleWidget(axisSide: meta.axisSide, child: Text('...'))
   ```

3. **Color.withOpacity deprecated**: Usar `withValues(alpha:)`:
   ```dart
   // ❌ Deprecated
   color.withOpacity(0.5)
   
   // ✅ Correcto
   color.withValues(alpha: 0.5)
   ```

4. **Uso de context después de async**: Siempre verificar `mounted`:
   ```dart
   // ❌ Incorrecto - puede fallar si widget fue desmontado
   final image = await picker.pickImage(source: ImageSource.camera);
   if (image != null) {
     ScaffoldMessenger.of(context).showSnackBar(...);
   }
   
   // ✅ Correcto - guardar referencias ANTES del await
   final navigator = Navigator.of(context);
   final messenger = ScaffoldMessenger.of(context);
   navigator.pop();
   final image = await picker.pickImage(source: ImageSource.camera);
   if (image != null && mounted) {
     messenger.showSnackBar(...);
   }
   ```

5. **Scripts de utilidad**: Agregar `// ignore_for_file: avoid_print` en archivos tool/:
   ```dart
   // ignore_for_file: avoid_print
   import 'dart:io';
   
   void main() {
     print('This is allowed in scripts');
   }
   ```

6. **Constantes en widgets**: Usar `const` cuando sea posible para mejor rendimiento:
   ```dart
   // ❌ Sin const - crea nueva instancia cada rebuild
   Icon(Icons.home, color: Colors.blue)
   
   // ✅ Con const - misma instancia
   const Icon(Icons.home, color: Colors.blue)
   ```

---

## 🔧 CONFIGURACIÓN DE DESARROLLO

### Requisitos

- .NET SDK 8.0+
- Node.js 20+
- Flutter SDK 3.2+
- Docker Desktop
- Visual Studio Code / Rider

### Setup inicial

```powershell
# Clonar repositorio
git clone https://github.com/gregorymorenoiem/cardealer-microservices.git
cd cardealer-microservices

# Backend
cd backend
dotnet restore
docker-compose up -d  # Levantar dependencias

# Frontend Web
cd ../frontend/web
npm install
npm run dev

# Frontend Mobile
cd ../frontend/mobile/cardealer
flutter pub get
flutter run
```

---

## 📝 CONVENCIONES DE CÓDIGO

### C# / .NET

```csharp
// Namespaces: File-scoped
namespace AuthService.Domain.Entities;

// Clases: PascalCase
public class ApplicationUser { }

// Interfaces: I + PascalCase
public interface IUserRepository { }

// Métodos async: sufijo Async
public async Task<User> GetUserAsync(Guid id, CancellationToken ct);

// Records para DTOs inmutables
public record UserDto(Guid Id, string Email, string FullName);

// Primary constructors para DI
public class UserService(IUserRepository repo, ILogger<UserService> logger)
{
    public async Task<User?> GetAsync(Guid id) => await repo.GetByIdAsync(id);
}
```

### TypeScript / React

```typescript
// Interfaces: I prefix opcional, preferir types
type User = {
  id: string;
  email: string;
  fullName: string;
};

// Components: PascalCase, función arrow
export const UserCard = ({ user }: { user: User }) => {
  return <div>{user.fullName}</div>;
};

// Hooks: use prefix
export const useAuth = () => {
  // ...
};

// API calls: sufijo Api o Service
export const userApi = {
  getById: (id: string) => axios.get<User>(`/api/users/${id}`),
};
```

### Dart / Flutter

```dart
// Classes: PascalCase
class UserEntity {
  final String id;
  final String email;
  
  const UserEntity({required this.id, required this.email});
}

// BLoC naming
class AuthBloc extends Bloc<AuthEvent, AuthState> { }

// Widgets: sufijo Widget o Page
class LoginPage extends StatelessWidget { }
class UserCardWidget extends StatelessWidget { }
```

---

## 🚀 CI/CD

### GitHub Actions

- `.github/workflows/` contiene workflows de CI/CD
- Build y test automáticos en PR
- Deploy a staging/production

### Ambientes

| Ambiente | Descripción |
|----------|-------------|
| `Development` | Local con Docker |
| `Staging` | Pre-producción |
| `Production` | Producción |

---

## 📚 DOCUMENTACIÓN ADICIONAL

| Documento | Descripción |
|-----------|-------------|
| `ARQUITECTURA_MICROSERVICIOS.md` | Diseño de arquitectura |
| `SECURITY_POLICIES.md` | Políticas de seguridad |
| `VAULT_INTEGRATION_GUIDE.md` | Gestión de secretos |
| `CI_CD_MONITORING_GUIDE.md` | Monitoreo y CI/CD |
| `GUIA_MULTI_DATABASE_CONFIGURATION.md` | Multi-provider DB |
| `CONVERSION_A_SISTEMA_MULT-TENANT.md` | Multi-tenancy |
| `MICROSERVICES_AUDIT_SPRINT_PLAN.md` | **Plan de auditoría Docker** |
| `MICROSERVICES_AUDIT_REPORT.md` | Reporte de auditoría |

---

## ⚠️ NOTAS IMPORTANTES PARA COPILOT

1. **NO crear referencias cruzadas** entre microservicios
2. **Usar RabbitMQ** para comunicación inter-servicios
3. **Siempre implementar** `ITenantEntity` para entidades multi-tenant
4. **Validar con FluentValidation** antes de procesar commands
5. **Publicar Domain Events** para operaciones importantes
6. **Usar Result Pattern** en lugar de excepciones para flujo de control
7. **Incluir CancellationToken** en métodos async
8. **Documentar** endpoints con XML comments para Swagger
9. **Seguir naming conventions** del proyecto
10. **Tests**: mínimo 80% coverage para nuevas features

---

## 🚨 ESTADO DEL PROYECTO (1 Enero 2026 - 04:00)

### 🎉 FASE 0 COMPLETADA AL 100% (1 Ene 2026 - 04:00)

**Estado:** ✅ **11/11 sprints completados** - Infraestructura lista para FASE 1

**Sprints completados:**
- ✅ Sprint 0.1-0.2: Infraestructura Docker y credenciales de prueba
- ✅ Sprint 0.5.1-0.5.5: Docker Services (5 sprints)
- ✅ Sprint 0.6.1: AuthService Dockerfile Fix
- ✅ Sprint 0.6.2: ProductService Fix
- ✅ Sprint 0.6.3: **Schema Validation** (1 Ene 2026 - 02:00)
- ✅ Sprint 0.7.1: **Gestión de Secretos** (36 secretos reemplazados)
- ✅ Sprint 0.7.2: **Validación de Secretos** (1 Ene 2026 - 04:00)

**Sprint 0.7.2 - Validación de Secretos (Completado):**
- ✅ RabbitMQ audit: 8/8 servicios con configuración correcta
- ✅ TODOS los servicios usan `"Host"` NO `"HostName"` (0 fixes requeridos)
- ✅ Infraestructura validada: Redis, RabbitMQ, Consul operacionales
- ✅ 4/4 servicios core healthy: AuthService, ErrorService, UserService, RoleService
- ✅ Startup incremental exitoso: Infrastructure → DBs → Services
- 📄 Documentación: `SPRINT_0.7.2_SECRETS_VALIDATION_COMPLETION.md`

**Sprint 0.6.3 - Schema Validation (Completado):**
- ✅ Script creado: `scripts/Validate-DatabaseSchemas.ps1` (300+ líneas)
- ✅ Valida C# entities vs PostgreSQL columns automáticamente
- ✅ 4/4 servicios core: 0 desincronizaciones detectadas
- ✅ Herramienta reutilizable para QA continuo
- 📄 Documentación: `SPRINT_0.6.3_SCHEMA_VALIDATION_COMPLETION.md`

**Sprint 0.7.1 - Gestión de Secretos (Completado):**
- ✅ 36 secretos reemplazados con variables de entorno
- ✅ 12 JWT keys: `Jwt__Key: "${JWT__KEY:-default}"`
- ✅ 24 PostgreSQL passwords: `POSTGRES_PASSWORD: "${POSTGRES_PASSWORD:-password}"`
- ✅ Script: `scripts/replace-secrets-clean.ps1` (92 líneas)
- 📄 Documentación: `SPRINT_0.7.1_SECRETS_MANAGEMENT_COMPLETION.md`

**Infraestructura validada:**
- ✅ Redis: UP and healthy
- ✅ RabbitMQ: UP and healthy (8 servicios con configuración correcta)
- ✅ Consul: UP and healthy
- ✅ PostgreSQL: 7/7 DB instances para servicios core
- ✅ 4/4 servicios core operacionales

**Progreso global:** 62.2% (23/37 sprints)
- FASE 0: 11/11 sprints = 100% ✅
- FASE 1: 4/4 sprints = 100% ✅ (AuthService, ErrorService, Gateway, NotificationService)
- FASE 2: 4/4 sprints = 100% ✅ (CacheService, MessageBusService, ConfigurationService, ServiceDiscovery)
- FASE 3: 3/3 sprints = 100% ✅ (LoggingService, TracingService, HealthCheckService)
- FASE 4: 0/15 sprints = 0%

**FASE 1 Completada (1 Ene 2026):**
- ✅ Sprint 1.1: AuthService - 11 endpoints auditados
- ✅ Sprint 1.2: ErrorService - 6 endpoints auditados
- ✅ Sprint 1.3: Gateway - Ocelot routing validado, 7 rutas configuradas
- ✅ Sprint 1.4: NotificationService - 17 endpoints auditados (Email, SMS, Push, Teams)
- 📄 Documentación: `FASE_1_PROGRESS_REPORT.md`

**FASE 2 Completada (1 Ene 2026):**
- ✅ Sprint 2.1: CacheService - 7 endpoints auditados (Redis, Distributed Locks, Statistics)
- ✅ Sprint 2.2: MessageBusService - 17 endpoints auditados (RabbitMQ, Sagas, Dead Letters)
- ✅ Sprint 2.3: ConfigurationService - 7 endpoints auditados (Config dinámica, Feature Flags)
- ✅ Sprint 2.4: ServiceDiscovery - 10 endpoints auditados (Consul, Health Checks)
- 📄 Documentación: `FASE_1_PROGRESS_REPORT.md` (incluye FASE 2)

**FASE 3 Completada (1 Ene 2026):**
- ✅ Sprint 3.1: LoggingService - 23 endpoints auditados (Logs, Alerts, Analysis)
- ✅ Sprint 3.2: TracingService - 6 endpoints auditados (Traces, Spans, Services)
- ✅ Sprint 3.3: HealthCheckService - 4 endpoints auditados (System Health, Service Health)
- 📄 Documentación: `FASE_1_PROGRESS_REPORT.md` (incluye FASE 3)

**Próximo paso:** FASE 4 - Sprint 4.1 UserService Audit

### ✅ RESUELTO: Migraciones EF Core

**Estado:** ✅ **VERIFICADO** - Las migraciones están correctas.

**Verificación realizada (31 Dic 2025):**
- ✅ AuthService: Todas las columnas existen en la BD (`CreatedAt`, `UpdatedAt`, `DealerId`, `ExternalAuthProvider`, `ExternalUserId`)
- ✅ RefreshTokens: `Id` existe como PK
- ✅ ProductService: DealerId agregado a products, product_images, categories
- ✅ UserService: Users, UserRoles con DealerId
- ✅ RoleService: Roles, Permissions, RolePermissions con DealerId
- ✅ ErrorService: error_logs con DealerId

### ✅ RESUELTO: Servicios en docker-compose.yml

**Estado:** ✅ **COMPLETADO** - Todos los 35 servicios están en docker-compose.yml

**Verificado el 31 Dic 2025:**
- ✅ Todos los servicios tienen configuración en compose.yaml
- ✅ Todos los servicios tienen Dockerfile.dev
- ✅ Todos los servicios tienen bases de datos PostgreSQL configuradas (donde aplica)
- ✅ Variables de entorno `Database__*` configuradas correctamente

### ✅ RESUELTO: AuthService funcional

**Estado:** 🟢 **FUNCIONAL** (31 Dic 2025)

**Correcciones aplicadas:**
1. ✅ Dockerfile.dev cambiado de `dotnet watch` a `dotnet build + dotnet run`
2. ✅ Variables de entorno agregadas en compose.yaml:
   - `Database__Provider: "PostgreSQL"`
   - `Database__Host`, `Database__Port`, `Database__Database`
   - `Database__Username`, `Database__Password`
   - `Database__ConnectionStrings__PostgreSQL` (connection string completo)
   - `Database__AutoMigrate: "true"`
3. ✅ Health check responde 200 OK
4. ✅ Endpoints `/api/auth/register` y `/api/auth/login` funcionales
5. ✅ Tokens JWT generados correctamente

**Credenciales de prueba creadas:**
```
Email: test@example.com
Password: Admin123!
UserName: testuser
AccountType: individual
EmailConfirmed: true
```

---

## 🔐 SECRETOS REQUERIDOS PARA PRODUCCIÓN

Para que los microservicios funcionen, solo se necesita suministrar estos secretos:

| Servicio | Secreto | Variable de Entorno | Obligatorio |
|----------|---------|---------------------|:-----------:|
| **AuthService** | JWT Secret Key | `JWT__KEY` | ✅ |
| | Google Client ID | `AUTHENTICATION__GOOGLE__CLIENTID` | ⚪ |
| | Google Client Secret | `AUTHENTICATION__GOOGLE__CLIENTSECRET` | ⚪ |
| | Microsoft Client ID | `AUTHENTICATION__MICROSOFT__CLIENTID` | ⚪ |
| | Microsoft Client Secret | `AUTHENTICATION__MICROSOFT__CLIENTSECRET` | ⚪ |
| **NotificationService** | SendGrid API Key | `NOTIFICATIONSETTINGS__SENDGRID__APIKEY` | ⚪* |
| | Twilio Account SID | `NOTIFICATIONSETTINGS__TWILIO__ACCOUNTSID` | ⚪* |
| | Twilio Auth Token | `NOTIFICATIONSETTINGS__TWILIO__AUTHTOKEN` | ⚪* |
| | Firebase Service Account | Archivo JSON montado | ⚪* |
| **BillingService** | Stripe Secret Key | `STRIPE__SECRETKEY` | ✅ |
| | Stripe Webhook Secret | `STRIPE__WEBHOOKSECRET` | ✅ |
| **MediaService** | AWS Access Key | `S3STORAGE__ACCESSKEY` | ⚪** |
| | AWS Secret Key | `S3STORAGE__SECRETKEY` | ⚪** |
| | Azure Connection String | `AZUREBLOBSTORAGE__CONNECTIONSTRING` | ⚪** |

> ✅ = Obligatorio | ⚪ = Opcional | ⚪* = Al menos un canal requerido | ⚪** = Según provider

---

## 🔄 VERSIONES DE PAQUETES RECOMENDADAS

### .NET Packages (actualizado 2025)

```xml
<!-- Core -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.11" />

<!-- Auth -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.11" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.2.1" />

<!-- CQRS -->
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="FluentValidation" Version="11.11.0" />

<!-- Messaging -->
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />

<!-- Resilience -->
<PackageReference Include="Polly" Version="8.5.2" />

<!-- Cache -->
<PackageReference Include="StackExchange.Redis" Version="2.8.22" />

<!-- Service Discovery -->
<PackageReference Include="Consul" Version="1.7.14.9" />

<!-- Scheduler -->
<PackageReference Include="Hangfire.Core" Version="1.8.17" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.17" />

<!-- Observability -->
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.14.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />

<!-- API Docs -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.8.1" />

<!-- Testing -->
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="FluentAssertions" Version="7.0.0" />
<PackageReference Include="Moq" Version="4.20.72" />
```

---

## � CHECKLIST PRE-EJECUCIÓN DE TAREAS

Antes de ejecutar CUALQUIER tarea de los sprint plans, verificar:

### 1. Estimación de Tokens
```
[ ] Calculé tokens de lectura (archivos × líneas × 4)
[ ] Calculé tokens de escritura (líneas nuevas × 5)
[ ] Sumé contexto base (~8,000)
[ ] Apliqué multiplicador de complejidad
[ ] Total < 110,000 tokens de input
[ ] Output esperado < 16,000 tokens
```

### 2. División si Excede Límites
```
[ ] Si total > 80,000: dividir en subtareas
[ ] Cada subtarea debe ser independiente y testeable
[ ] Definir orden de ejecución
[ ] Documentar dependencias entre subtareas
```

### 3. Contexto Necesario
```
[ ] Tengo acceso a todos los archivos requeridos
[ ] Las dependencias están identificadas
[ ] Los tests existentes están considerados
[ ] El breaking change está documentado
```

### 4. Validación Post-Tarea
```
[ ] Código compila sin errores
[ ] Tests pasan (si aplica)
[ ] Lint/format aplicado
[ ] Commit message sigue convención
```

---

## �📁 TEMPLATES Y SAMPLES

Para tareas comunes, consulta los templates en `.github/copilot-samples/`:

| Template | Descripción |
|----------|-------------|
| [new-microservice-template.md](copilot-samples/new-microservice-template.md) | Crear nuevo microservicio |
| [cqrs-feature-template.md](copilot-samples/cqrs-feature-template.md) | Crear Commands/Queries con MediatR |
| [domain-events-template.md](copilot-samples/domain-events-template.md) | Eventos de dominio y RabbitMQ |
| [testing-template.md](copilot-samples/testing-template.md) | Unit tests e Integration tests |
| [quick-reference.md](copilot-samples/quick-reference.md) | Comandos y endpoints frecuentes |

---

## 📱 FLUTTER MOBILE - NOTAS CRÍTICAS

### Información del Proyecto

| Aspecto | Valor |
|---------|-------|
| **Nombre del paquete** | `cardealer_mobile` (NO `cardealer`) |
| **Ruta del proyecto** | `frontend/mobile/cardealer` |
| **SDK Flutter** | >=3.4.0 (stable 3.35.4+) |
| **SDK Dart** | >=3.4.0 <4.0.0 (3.9.2+) |

### ⚠️ ERRORES COMUNES A EVITAR

1. **Imports del paquete**: SIEMPRE usar `package:cardealer_mobile/...` NO `package:cardealer/...`

2. **Dos archivos de Failures con sintaxis diferente**:
   - `core/error/failures.dart` - Usa parámetros NOMBRADOS: `const AuthFailure({required super.message});`
   - `core/errors/failures.dart` - Usa parámetros POSICIONALES: `const AuthFailure(super.message);`
   - Los usecases de Auth importan `core/errors/failures.dart` (posicional)
   - Los usecases de Vehicle importan `core/error/failures.dart` (nombrado)

3. **Testing con mocktail (NO mockito)**:
   - El proyecto usa `mocktail` para mocking - NO requiere code generation
   - NO usar `@GenerateMocks` ni `build_runner`
   - Sintaxis: `class MockRepo extends Mock implements Repo {}`
   - When: `when(() => mock.method()).thenReturn(value)`
   - Any: `any(named: 'param')` en lugar de `anyNamed('param')`
   - Registrar fallback values: `setUpAll(() { registerFallbackValue(UserRole.individual); })`

4. **Use cases sin parámetros**: Usar `.call()` explícito
   ```dart
   // ✅ Correcto
   when(() => mockLogoutUseCase.call()).thenAnswer((_) async => const Right(null));
   
   // ❌ Incorrecto
   when(() => mockLogoutUseCase()).thenAnswer(...);  // No funciona con mocktail
   ```

5. **AuthBloc estados de registro**: El registro emite `AuthRegistrationSuccess` NO `AuthAuthenticated`

6. **Vehicle entity**: Requiere `createdAt` como parámetro obligatorio
   ```dart
   Vehicle(id: '1', name: 'Test', createdAt: DateTime(2024, 1, 1), ...)
   ```

### 🔧 COMANDOS FLUTTER

```powershell
# ⚠️ CRÍTICO: Los comandos flutter (analyze, test) pueden quedarse esperando input
# SIEMPRE agregar `; echo ""` al final del comando para forzar que termine
# O enviar ENTER manualmente si el proceso se queda colgado

# Análisis - USAR ESTE FORMATO:
flutter analyze --no-fatal-infos --no-fatal-warnings 2>&1; echo ""

# Tests - USAR ESTE FORMATO:
flutter test 2>&1; echo ""
flutter test test/presentation/bloc/ 2>&1; echo ""
flutter test --reporter compact 2>&1; echo ""

# Build runner (si fuera necesario - NO requerido con mocktail)
dart run build_runner build --delete-conflicting-outputs

# Limpiar y reconstruir
flutter clean
flutter pub get
```

### 📁 ESTRUCTURA DE TESTS

```
test/
├── presentation/
│   └── bloc/
│       ├── auth/
│       │   └── auth_bloc_test.dart      # 9 tests - mocktail
│       └── vehicles/
│           └── vehicles_bloc_test.dart  # 16 tests - mocktail
└── ... (otros tests)
```

### 🧪 TEMPLATE DE TEST CON MOCKTAIL

```dart
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:dartz/dartz.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:cardealer_mobile/core/errors/failures.dart'; // posicional

// Mock classes - NO code generation needed
class MockMyUseCase extends Mock implements MyUseCase {}

void main() {
  late MyBloc bloc;
  late MockMyUseCase mockUseCase;

  // Register fallback values for non-primitive types
  setUpAll(() {
    registerFallbackValue(UserRole.individual);
  });

  setUp(() {
    mockUseCase = MockMyUseCase();
    bloc = MyBloc(myUseCase: mockUseCase);
  });

  tearDown(() {
    bloc.close();
  });

  blocTest<MyBloc, MyState>(
    'emits [Loading, Success] when successful',
    build: () {
      when(() => mockUseCase.call()).thenAnswer((_) async => const Right(result));
      return bloc;
    },
    act: (bloc) => bloc.add(MyEvent()),
    expect: () => [MyLoading(), MySuccess(result)],
  );
}
```

---

## 🏷️ COMMITS Y BRANCHES

### Convención de Commits

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

**Tipos:**
- `feat`: Nueva funcionalidad
- `fix`: Corrección de bug
- `docs`: Documentación
- `style`: Formato (no cambia código)
- `refactor`: Refactorización
- `test`: Agregar tests
- `chore`: Tareas de mantenimiento

**Ejemplos:**
```
feat(auth): add 2FA support with TOTP
fix(product): resolve pagination issue with custom fields
docs(readme): update API documentation
test(user): add integration tests for user creation
```

### Convención de Branches

```
<type>/<ticket-id>-<short-description>
```

**Ejemplos:**
```
feature/CD-123-add-2fa-support
bugfix/CD-456-fix-login-error
hotfix/CD-789-security-patch
```

---

*Última actualización: 30 Diciembre 2025*
