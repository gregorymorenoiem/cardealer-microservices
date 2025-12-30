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

```
cardealer-microservices/
├── backend/                          # Microservicios .NET 8
│   ├── _Shared/                      # Librerías compartidas
│   │   ├── CarDealer.Contracts/      # DTOs y Events para comunicación
│   │   └── CarDealer.Shared/         # Utilidades y Multi-tenancy
│   ├── _Tests/IntegrationTests/      # Tests de integración
│   ├── Gateway/                      # API Gateway (Ocelot)
│   ├── ServiceDiscovery/             # Consul integration
│   ├── AuthService/                  # Autenticación y autorización
│   ├── UserService/                  # Gestión de usuarios
│   ├── RoleService/                  # Gestión de roles y permisos
│   ├── ProductService/               # Productos genéricos marketplace
│   ├── MediaService/                 # Gestión de archivos multimedia
│   ├── NotificationService/          # Email, SMS, Push notifications
│   ├── BillingService/               # Facturación y pagos
│   ├── CRMService/                   # Gestión de clientes
│   ├── ErrorService/                 # Centralización de errores
│   ├── AuditService/                 # Auditoría y compliance
│   ├── CacheService/                 # Cache distribuido
│   ├── MessageBusService/            # RabbitMQ abstraction
│   ├── SchedulerService/             # Jobs con Hangfire
│   ├── SearchService/                # Búsqueda (Elasticsearch)
│   ├── ReportsService/               # Reportes y analytics
│   ├── HealthCheckService/           # Health monitoring
│   ├── LoggingService/               # Logging centralizado
│   ├── TracingService/               # Distributed tracing
│   ├── ConfigurationService/         # Configuración dinámica
│   ├── FeatureToggleService/         # Feature flags
│   ├── FileStorageService/           # S3/Azure Blob storage
│   ├── BackupDRService/              # Backup y Disaster Recovery
│   ├── MarketingService/             # Campañas marketing
│   ├── IntegrationService/           # Integraciones externas
│   ├── FinanceService/               # Finanzas y contabilidad
│   ├── InvoicingService/             # Facturación electrónica
│   ├── ContactService/               # Gestión de contactos
│   ├── AppointmentService/           # Citas y agenda
│   ├── AdminService/                 # Panel de administración
│   ├── ApiDocsService/               # Documentación API
│   ├── RateLimitingService/          # Rate limiting
│   ├── IdempotencyService/           # Idempotencia
│   ├── RealEstateService/            # Vertical inmobiliario
│   ├── observability/                # Configs OpenTelemetry
│   └── monitoring/                   # Prometheus/Grafana configs
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

| Servicio | Puerto |
|----------|--------|
| Gateway | 8080 |
| AuthService | 15085 |
| ErrorService | 15083 |
| Redis | 6379 |
| RabbitMQ | 5672, 15672 (UI) |
| PostgreSQL | 5432 |
| Consul | 8500 |
| Prometheus | 9090 |
| Grafana | 3000 |
| Jaeger | 16686 |

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

*Última actualización: Diciembre 2025*
