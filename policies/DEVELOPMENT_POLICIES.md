# 📋 POLÍTICAS DE DESARROLLO - CarDealer Microservices

> **Versión**: 1.0  
> **Fecha**: 2025-11-29  
> **Propósito**: Estandarizar el desarrollo, testing y documentación de microservicios

---

## 🎯 ÍNDICE DE POLÍTICAS

1. [Arquitectura y Estructura de Proyectos](#1-arquitectura-y-estructura-de-proyectos)
2. [Configuración y Gestión de Entornos](#2-configuración-y-gestión-de-entornos)
3. [Testing - Pruebas Automatizadas](#3-testing---pruebas-automatizadas)
4. [Integration Testing - Pruebas de Integración](#4-integration-testing---pruebas-de-integración)
5. [Real Infrastructure Testing](#5-real-infrastructure-testing)
6. [Troubleshooting y Debugging](#6-troubleshooting-y-debugging)
7. [Observabilidad y Logging](#7-observabilidad-y-logging)
8. [Seguridad y Autenticación](#8-seguridad-y-autenticación)
9. [Resiliencia y Manejo de Errores](#9-resiliencia-y-manejo-de-errores)
10. [Documentación Obligatoria](#10-documentación-obligatoria)
11. [Git y Control de Versiones](#11-git-y-control-de-versiones)
12. [Code Review y Quality Gates](#12-code-review-y-quality-gates)
13. [Deployment y CI/CD](#13-deployment-y-cicd)
14. [Performance y Optimización](#14-performance-y-optimización)
15. [Dependency Management](#15-dependency-management)

---

## 1. ARQUITECTURA Y ESTRUCTURA DE PROYECTOS

### 1.1 Clean Architecture Obligatoria

**POLÍTICA**: Todos los microservicios DEBEN seguir Clean Architecture con las siguientes capas:

```
{ServiceName}/
├── {ServiceName}.Api/              # Presentation Layer
│   ├── Controllers/
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
├── {ServiceName}.Application/      # Application Layer
│   ├── UseCases/
│   │   └── {Feature}/
│   │       ├── {Feature}Command.cs
│   │       ├── {Feature}CommandHandler.cs
│   │       └── {Feature}CommandValidator.cs
│   ├── DTOs/
│   ├── Behaviors/
│   └── Metrics/
├── {ServiceName}.Domain/           # Domain Layer
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Interfaces/
│   └── Events/
├── {ServiceName}.Infrastructure/   # Infrastructure Layer
│   ├── Persistence/
│   │   ├── {Context}DbContext.cs
│   │   ├── Repositories/
│   │   └── Configurations/
│   ├── Messaging/
│   ├── External/
│   └── Migrations/
├── {ServiceName}.Shared/           # Shared Layer
│   ├── Middleware/
│   ├── Extensions/
│   └── RateLimiting/
└── {ServiceName}.Tests/            # Testing Project
    ├── Unit/
    ├── Integration/
    └── E2E/
```

**PROHIBIDO**:
- ❌ Mezclar lógica de negocio en Controllers
- ❌ Referencias directas de Api → Infrastructure (solo vía DI)
- ❌ Dependencias circulares entre capas
- ❌ Crear carpetas fuera de esta estructura sin aprobación

**VALIDACIÓN**: Code review rechazará PRs que no sigan esta estructura

---

### 1.2 Naming Conventions

**POLÍTICA**: Nomenclatura estricta para archivos y clases:

| Tipo | Patrón | Ejemplo |
|------|--------|---------|
| Entidades | `{Nombre}.cs` | `ErrorLog.cs`, `User.cs` |
| Commands | `{Acción}{Entidad}Command.cs` | `LogErrorCommand.cs` |
| Handlers | `{Acción}{Entidad}CommandHandler.cs` | `LogErrorCommandHandler.cs` |
| Validators | `{Acción}{Entidad}CommandValidator.cs` | `LogErrorCommandValidator.cs` |
| DTOs | `{Entidad}Dto.cs` | `ErrorLogDto.cs` |
| Repositories | `I{Entidad}Repository.cs` (interface) | `IErrorLogRepository.cs` |
| Implementations | `Ef{Entidad}Repository.cs` | `EfErrorLogRepository.cs` |
| Controllers | `{Plural}Controller.cs` | `ErrorsController.cs` |

**PROHIBIDO**:
- ❌ Abreviaciones no estándar (`ErrCtrl.cs`)
- ❌ Nombres genéricos (`Helper.cs`, `Util.cs`)
- ❌ Mezcla de idiomas (`UserServicio.cs`)

---

### 1.3 Dependency Injection

**POLÍTICA**: Registrar TODOS los servicios en `Program.cs` con lifetime correcto:

```csharp
// ✅ CORRECTO - Lifetime apropiado
builder.Services.AddScoped<IErrorLogRepository, EfErrorLogRepository>();
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddTransient<IEmailService, SmtpEmailService>();

// ❌ PROHIBIDO - new manual de dependencias
var repo = new EfErrorLogRepository(context); // NO!
```

**REGLAS**:
- **Scoped**: Repositorios, DbContext, servicios por request
- **Singleton**: Caches, métricas, event publishers, configuraciones
- **Transient**: Validadores, mappers, servicios stateless

---

## 2. CONFIGURACIÓN Y GESTIÓN DE ENTORNOS

### 2.1 appsettings.json Hierarchy

**POLÍTICA**: Configuración multi-entorno OBLIGATORIA:

```
appsettings.json              # Configuración base (sin secretos)
appsettings.Development.json  # Desarrollo local
appsettings.Staging.json      # QA/Staging
appsettings.Production.json   # Producción (solo estructura, valores en Key Vault)
```

**CONTENIDO OBLIGATORIO** en `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionStrings": {
      "PostgreSQL": "Host=localhost;Port=25432;Database={servicename};Username=postgres;Password=password;",
      "SqlServer": "...",
      "Oracle": "..."
    },
    "AutoMigrate": false,
    "CommandTimeout": 30,
    "MaxRetryCount": 3,
    "MaxRetryDelay": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": "5672",
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "ExchangeName": "cardealer.events"
  },
  "Jwt": {
    "Issuer": "cardealer-auth",
    "Audience": "cardealer-services",
    "Key": "cardealer-super-secret-key-min-32-characters-long-for-production!",
    "ExpirationMinutes": 60
  },
  "RateLimiting": {
    "Enabled": true,
    "MaxRequests": 100,
    "WindowSeconds": 60
  },
  "OpenTelemetry": {
    "ServiceName": "{ServiceName}",
    "ServiceVersion": "1.0.0",
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

**PROHIBIDO**:
- ❌ Hardcodear connection strings en código
- ❌ Secretos en appsettings.json (usar User Secrets en dev, Key Vault en prod)
- ❌ Diferentes estructuras entre microservicios

---

### 2.2 Environment Variables

**POLÍTICA**: Variables de entorno para overrides:

```powershell
# Development
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=25432;..."

# Staging
$env:ASPNETCORE_ENVIRONMENT = "Staging"

# Production
$env:ASPNETCORE_ENVIRONMENT = "Production"
```

**REGLA**: Environment variables tienen prioridad sobre appsettings.json

---

### 2.3 Database Multi-Provider

**POLÍTICA**: TODOS los microservicios DEBEN soportar múltiples bases de datos:

```csharp
// ✅ OBLIGATORIO - Usar DatabaseExtensions compartido
builder.Services.AddDatabaseProvider<ApplicationDbContext>(
    builder.Configuration, 
    configSection: "Database"
);

// ❌ PROHIBIDO - Hardcodear provider
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)); // NO!
```

**PROVEEDORES SOPORTADOS**:
- PostgreSQL (preferido para producción)
- SQL Server (legacy/Windows)
- MySQL (opcional)
- Oracle (opcional)
- InMemory (SOLO para testing)

---

## 3. TESTING - PRUEBAS AUTOMATIZADAS

### 3.1 Cobertura Mínima Obligatoria

**POLÍTICA**: Todos los microservicios DEBEN tener:

| Tipo de Test | Cobertura Mínima | Obligatorio |
|--------------|------------------|-------------|
| Unit Tests | 80% | ✅ Sí |
| Integration Tests | 60% | ✅ Sí |
| E2E Tests | 40% | ✅ Sí |

**VALIDACIÓN**: CI/CD bloqueará deploys con cobertura < mínimo

---

### 3.2 Unit Tests - Naming Convention

**POLÍTICA**: Nomenclatura estricta para tests:

```csharp
// ✅ CORRECTO - Patrón: MethodName_Scenario_ExpectedBehavior
[Fact]
public void LogError_ValidCommand_ReturnsSuccessResult() { }

[Fact]
public void LogError_NullMessage_ThrowsValidationException() { }

[Theory]
[InlineData("")]
[InlineData(null)]
public void ValidateMessage_EmptyOrNull_ReturnsFalse(string message) { }
```

**PROHIBIDO**:
- ❌ `Test1()`, `TestMethod()` - nombres genéricos
- ❌ `Should_Work()` - no describe escenario
- ❌ Tests sin Assert (`// TODO: implement`)

---

### 3.3 Unit Tests - AAA Pattern

**POLÍTICA**: TODOS los tests DEBEN seguir patrón Arrange-Act-Assert:

```csharp
[Fact]
public void LogError_ValidCommand_ReturnsSuccessResult()
{
    // Arrange
    var repository = new Mock<IErrorLogRepository>();
    var publisher = new Mock<IEventPublisher>();
    var handler = new LogErrorCommandHandler(repository.Object, publisher.Object);
    var command = new LogErrorCommand
    {
        ServiceName = "TestService",
        Message = "Test error"
    };

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.Success);
    repository.Verify(r => r.AddAsync(It.IsAny<ErrorLog>()), Times.Once);
}
```

**PROHIBIDO**:
- ❌ Mezclar Arrange y Act sin comentarios
- ❌ Múltiples Acts en un test
- ❌ Asserts sin mensaje descriptivo en casos complejos

---

### 3.4 Mocking con Moq

**POLÍTICA**: Usar Moq para dependencias externas:

```csharp
// ✅ CORRECTO - Setup específico
var mockRepo = new Mock<IErrorLogRepository>();
mockRepo
    .Setup(r => r.AddAsync(It.IsAny<ErrorLog>()))
    .ReturnsAsync(new ErrorLog { Id = Guid.NewGuid() });

// ✅ CORRECTO - Verify comportamiento
mockRepo.Verify(
    r => r.AddAsync(It.Is<ErrorLog>(e => e.ServiceName == "TestService")), 
    Times.Once
);

// ❌ PROHIBIDO - Mock sin Setup (comportamiento indefinido)
var mockRepo = new Mock<IErrorLogRepository>(); // Falta Setup
```

**REGLA**: Verificar SIEMPRE que métodos críticos se llamaron correctamente

---

## 4. INTEGRATION TESTING - PRUEBAS DE INTEGRACIÓN

### 4.1 CustomWebApplicationFactory

**POLÍTICA**: OBLIGATORIO crear factory para integration tests:

```csharp
public class CustomWebApplicationFactory<TProgram> 
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover DbContext real
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Usar InMemory database
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
                options.EnableSensitiveDataLogging();
            });

            // Seed data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
            SeedTestData(context);
        });
    }
}
```

**OBLIGATORIO**:
- InMemory database para integration tests
- Seed data consistente
- Cleanup después de cada test

---

### 4.2 Integration Tests - HTTP Endpoints

**POLÍTICA**: Validar TODOS los endpoints principales:

```csharp
public class ErrorsControllerIntegrationTests 
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    [Fact]
    public async Task POST_CreateError_WithValidJWT_Returns201()
    {
        // Arrange
        var token = GenerateValidJwtToken();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        var errorDto = new CreateErrorDto { /* ... */ };

        // Act
        var response = await _client.PostAsJsonAsync("/api/errors", errorDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ErrorLogDto>();
        Assert.NotNull(result?.Id);
    }

    [Fact]
    public async Task POST_CreateError_WithoutJWT_Returns401()
    {
        // Act
        var response = await _client.PostAsync("/api/errors", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
```

**COBERTURA OBLIGATORIA**:
- ✅ Autenticación (401 sin token, 200/201 con token válido)
- ✅ Autorización (403 con permisos insuficientes)
- ✅ Validación (400 con datos inválidos)
- ✅ SQL Injection detection (400)
- ✅ XSS detection (400)

---

### 4.3 JWT Token Generation para Tests

**POLÍTICA**: Helper method para generar tokens válidos:

```csharp
private string GenerateValidJwtToken(
    string userId = "test-user",
    string role = "admin",
    string service = "all",
    int expirationMinutes = 180)
{
    var secretKey = "cardealer-super-secret-key-min-32-characters-long-for-production!";
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, userId),
        new Claim("role", role),
        new Claim("service", service)
    };

    var token = new JwtSecurityToken(
        issuer: "cardealer-auth",
        audience: "cardealer-services",
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

**PROHIBIDO**:
- ❌ Tokens hardcodeados que expiran
- ❌ Diferentes configuraciones JWT entre tests y código real

---

## 5. REAL INFRASTRUCTURE TESTING

### 5.1 Validación Pre-Producción

**POLÍTICA**: ANTES de deployment a producción, OBLIGATORIO validar:

**CHECKLIST OBLIGATORIO**:
```markdown
- [ ] PostgreSQL/SQL Server conecta correctamente (puerto correcto)
- [ ] Migraciones de base de datos ejecutan sin errores
- [ ] RabbitMQ conecta y publica eventos
- [ ] Circuit Breaker funciona correctamente
- [ ] Health endpoint responde 200 OK
- [ ] Swagger UI accesible
- [ ] JWT authentication funciona
- [ ] Rate limiting configurado correctamente
- [ ] Logs estructurados (JSON) con TraceId/SpanId
- [ ] Graceful degradation validado (DB/RabbitMQ down)
```

**VALIDACIÓN**: No deployment sin checklist completo ✅

---

### 5.2 Docker Compose para Testing Local

**POLÍTICA**: TODOS los microservicios DEBEN tener docker-compose.yml:

```yaml
version: '3.8'

services:
  {servicename}-db:
    image: postgres:16
    container_name: {servicename}-db
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
      POSTGRES_DB: {servicename}
    ports:
      - "25432:5432"  # Puerto externo diferente por servicio
    networks:
      - cardealer-net
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  rabbitmq:
    image: rabbitmq:3.12-management
    container_name: cargurus_rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    networks:
      - cardealer-net

networks:
  cardealer-net:
    driver: bridge
```

**REGLA**: Cada microservicio usa puerto PostgreSQL diferente (25432, 25433, 25434...)

---

### 5.3 E2E Testing Script

**POLÍTICA**: Crear script PowerShell `E2E-TESTING-SCRIPT.ps1`:

```powershell
# E2E-TESTING-SCRIPT.ps1
param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$JwtSecret = "cardealer-super-secret-key-min-32-characters-long-for-production!"
)

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "  E2E TESTING - {ServiceName}" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Cyan

# Test 1: Health Check
try {
    $health = Invoke-RestMethod -Uri "$BaseUrl/health" -Method GET
    Write-Host "[✓] Health Check: $($health.status)" -ForegroundColor Green
} catch {
    Write-Host "[✗] Health Check FAILED: $_" -ForegroundColor Red
    exit 1
}

# Test 2: Protected Endpoint sin JWT (debe devolver 401)
try {
    Invoke-RestMethod -Uri "$BaseUrl/api/{resource}" -Method GET -ErrorAction Stop
    Write-Host "[✗] Authentication FAILED: Expected 401" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "[✓] Authentication: 401 Unauthorized (esperado)" -ForegroundColor Green
    }
}

# Test 3-7: CRUD operations con JWT válido...
# (Ver ErrorService/E2E-TESTING-SCRIPT.ps1 como ejemplo)
```

**OBLIGATORIO**: Script debe testear mínimo 7 escenarios

---

### 5.4 Troubleshooting Checklist

**POLÍTICA**: Ante fallos en real infrastructure, seguir este orden:

**PASO 1: Verificar Infraestructura**
```powershell
# PostgreSQL
docker ps --filter "name={servicename}-db"
docker logs {servicename}-db --tail 50

# RabbitMQ
docker ps --filter "name=rabbitmq"
curl http://localhost:15672  # Management UI
```

**PASO 2: Verificar Configuración**
```powershell
# Connection String correcto
Get-Content appsettings.Development.json | Select-String "ConnectionString"

# Environment variables
$env:ASPNETCORE_ENVIRONMENT
$env:ConnectionStrings__DefaultConnection
```

**PASO 3: Verificar Logs del Servicio**
```powershell
# Buscar errores en startup
dotnet run 2>&1 | Select-String "ERR"

# Verificar conexión DB
dotnet run 2>&1 | Select-String "Executed DbCommand"
```

**PASO 4: Limpiar y Rebuild**
```powershell
dotnet clean
Remove-Item -Recurse -Force bin,obj
dotnet build
dotnet run
```

---

## 6. TROUBLESHOOTING Y DEBUGGING

### 6.1 Logging Levels

**POLÍTICA**: Usar niveles de log correctamente:

```csharp
// ✅ CORRECTO
_logger.LogTrace("Entering method {MethodName}", nameof(Handle));          // Desarrollo
_logger.LogDebug("Processing command {@Command}", command);                 // Desarrollo
_logger.LogInformation("Error logged successfully with ID {ErrorId}", id);  // Producción
_logger.LogWarning("Retry attempt {Attempt} for {Operation}", 1, "SaveError"); // Importante
_logger.LogError(exception, "Failed to save error: {Message}", ex.Message); // Errores
_logger.LogCritical(exception, "Database connection lost");                 // Crítico

// ❌ PROHIBIDO
Console.WriteLine("Debug: " + message);  // NO usar Console
_logger.LogInformation(exception.StackTrace);  // NO log masivo en Information
```

**REGLAS**:
- **Trace**: Entry/Exit de métodos (solo desarrollo)
- **Debug**: Variables, estado interno (solo desarrollo)
- **Information**: Flujo normal de aplicación (producción)
- **Warning**: Eventos inusuales pero manejables
- **Error**: Excepciones que afectan operación actual
- **Critical**: Fallos que comprometen el sistema

---

### 6.2 Structured Logging

**POLÍTICA**: OBLIGATORIO usar structured logging con propiedades:

```csharp
// ✅ CORRECTO - Structured logging
_logger.LogInformation(
    "Error logged for service {ServiceName} with severity {Severity}",
    errorLog.ServiceName,
    errorLog.Severity
);

// ❌ PROHIBIDO - String interpolation
_logger.LogInformation($"Error logged for {errorLog.ServiceName}"); // NO!
```

**BENEFICIO**: Permite queries en sistemas de logging (ELK, Application Insights)

---

### 6.3 Exception Handling

**POLÍTICA**: Manejo de excepciones estandarizado:

```csharp
// ✅ CORRECTO - Catch específico y re-throw
try
{
    await _repository.SaveAsync(entity);
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Database update failed for entity {EntityId}", entity.Id);
    throw new ApplicationException("Failed to save data", ex);
}
catch (Exception ex)
{
    _logger.LogCritical(ex, "Unexpected error saving entity");
    throw;
}

// ❌ PROHIBIDO - Catch genérico sin log
try { /* ... */ } 
catch { }  // NO! Silencia errores

// ❌ PROHIBIDO - throw ex (pierde stack trace)
catch (Exception ex) 
{ 
    throw ex;  // NO! Usar throw; sin ex
}
```

---

### 6.4 Debugging con VS Code

**POLÍTICA**: Configurar `launch.json` para debugging:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch ({ServiceName})",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/{ServiceName}.Api/bin/Debug/net8.0/{ServiceName}.Api.dll",
            "args": [],
            "cwd": "${workspaceFolder}/{ServiceName}.Api",
            "stopAtEntry": false,
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        }
    ]
}
```

---

## 7. OBSERVABILIDAD Y LOGGING

### 7.1 OpenTelemetry Obligatorio

**POLÍTICA**: TODOS los microservicios DEBEN implementar OpenTelemetry:

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "{ServiceName}", serviceVersion: "1.0.0")
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName,
            ["service.namespace"] = "cardealer"
        }))
    .WithTracing(tracing => tracing
        .SetSampler(new ParentBasedSampler(
            new TraceIdRatioBasedSampler(builder.Environment.IsProduction() ? 0.1 : 1.0)))
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
        })
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddSource("RabbitMQ.*")
        .AddConsoleExporter());  // Dev: Console, Prod: OTLP
```

**OBLIGATORIO**:
- TraceId y SpanId en todos los logs
- Sampling: 100% en desarrollo, 10% en producción
- Excluir `/health` del tracing

---

### 7.2 Serilog Configuration

**POLÍTICA**: Usar Serilog con enrichers:

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()  // TraceId, SpanId
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} " +
        "{Properties:j} TraceId={TraceId} SpanId={SpanId}{NewLine}{Exception}")
    .WriteTo.File("logs/{ServiceName}-.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();
```

---

### 7.3 Health Checks

**POLÍTICA**: Implementar health checks completos:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database")
    .AddRabbitMQ(rabbitConnectionString, name: "rabbitmq")
    .AddCheck<CustomHealthCheck>("custom-logic");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            service = "{ServiceName}",
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});
```

---

## 8. SEGURIDAD Y AUTENTICACIÓN

### 8.1 JWT Authentication

**POLÍTICA**: JWT obligatorio en TODOS los endpoints (excepto /health):

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"])),
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });
```

---

### 8.2 Authorization Policies

**POLÍTICA**: Definir políticas granulares:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("{ServiceName}Access", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("service", "{servicename}", "all");
    });

    options.AddPolicy("{ServiceName}Admin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("role", "admin", "{servicename}-admin");
    });
});

// En Controllers
[Authorize(Policy = "{ServiceName}Access")]
public class {Resource}Controller : ControllerBase { }
```

---

### 8.3 Input Validation

**POLÍTICA**: FluentValidation OBLIGATORIO para todos los Commands/DTOs:

```csharp
public class CreateErrorCommandValidator : AbstractValidator<CreateErrorCommand>
{
    public CreateErrorCommandValidator()
    {
        RuleFor(x => x.ServiceName)
            .NotEmpty().WithMessage("ServiceName es obligatorio")
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9\-_]+$")
            .WithMessage("ServiceName contiene caracteres inválidos");

        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(5000)
            .Must(NotContainSqlInjection)
            .WithMessage("Posible SQL Injection detectado")
            .Must(NotContainXss)
            .WithMessage("Posible XSS detectado");
    }

    private bool NotContainSqlInjection(string input)
    {
        var sqlPatterns = new[] { "--", ";", "/*", "*/", "xp_", "sp_", "DROP", "ALTER" };
        return !sqlPatterns.Any(p => input.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    private bool NotContainXss(string input)
    {
        return !input.Contains("<script", StringComparison.OrdinalIgnoreCase);
    }
}
```

---

## 9. RESILIENCIA Y MANEJO DE ERRORES

### 9.1 Circuit Breaker Pattern

**POLÍTICA**: Implementar Circuit Breaker para servicios externos:

```csharp
public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly CircuitBreakerPolicy _circuitBreaker;

    public RabbitMqEventPublisher()
    {
        _circuitBreaker = Policy
            .Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .CircuitBreaker(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (ex, duration) =>
                {
                    _logger.LogWarning("Circuit Breaker OPEN por {Duration}s", duration.TotalSeconds);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit Breaker CLOSED");
                });
    }

    public async Task PublishAsync<T>(T @event)
    {
        await _circuitBreaker.ExecuteAsync(async () =>
        {
            // Publicar evento
        });
    }
}
```

---

### 9.2 Retry Policies

**POLÍTICA**: Retry automático para operaciones transitorias:

```csharp
// Database - configurado en DatabaseExtensions
npgsqlOptions.EnableRetryOnFailure(
    maxRetryCount: 3,
    maxRetryDelay: TimeSpan.FromSeconds(30),
    errorCodesToAdd: null
);

// HTTP Clients
builder.Services.AddHttpClient<IExternalService, ExternalService>()
    .AddTransientHttpErrorPolicy(policy => 
        policy.WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

---

### 9.3 Graceful Degradation

**POLÍTICA**: Servicio DEBE continuar funcionando aunque dependencias fallen:

```csharp
// ✅ CORRECTO - Try-catch en startup, log error pero continúa
try
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    Log.Information("Database migrations applied successfully");
}
catch (Exception ex)
{
    Log.Error(ex, "Database migration failed - service will continue");
    // NO throw - permitir que servicio arranque
}

// ✅ CORRECTO - Fallback cuando RabbitMQ falla
public async Task PublishAsync<T>(T @event)
{
    try
    {
        await _rabbitMqPublisher.PublishAsync(@event);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "RabbitMQ unavailable, storing in Dead Letter Queue");
        await _deadLetterQueue.EnqueueAsync(@event);
    }
}
```

---

## 10. DOCUMENTACIÓN OBLIGATORIA

### 10.1 README.md por Microservicio

**POLÍTICA**: OBLIGATORIO crear README.md completo:

```markdown
# {ServiceName}

## 📋 Descripción
Breve descripción del propósito del microservicio.

## 🏗️ Arquitectura
- **Clean Architecture**: Api → Application → Domain → Infrastructure
- **Database**: PostgreSQL (primary), SQL Server (secondary)
- **Messaging**: RabbitMQ (Exchange: cardealer.events)

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- Docker Desktop
- PostgreSQL 16 (via Docker)
- RabbitMQ 3.12 (via Docker)

### Setup
\`\`\`bash
# 1. Clonar repositorio
git clone https://github.com/{org}/cardealer-microservices.git

# 2. Iniciar infraestructura
cd backend
docker-compose up -d {servicename}-db rabbitmq

# 3. Aplicar migraciones
cd {ServiceName}/{ServiceName}.Api
dotnet ef database update

# 4. Ejecutar servicio
dotnet run
\`\`\`

### Endpoints
- Health: `GET /health`
- Swagger: `GET /swagger`
- API Base: `https://localhost:5001/api/{resource}`

## 🧪 Testing

### Unit Tests
\`\`\`bash
dotnet test --filter Category=Unit
\`\`\`

### Integration Tests
\`\`\`bash
dotnet test --filter Category=Integration
\`\`\`

### E2E Tests
\`\`\`bash
.\E2E-TESTING-SCRIPT.ps1 -BaseUrl "http://localhost:5000"
\`\`\`

## 📊 Metrics
- OpenTelemetry: http://localhost:4317
- Health Checks: GET /health
- Custom Metrics: {ServiceName}.Application.Metrics

## 🔒 Security
- JWT Authentication (Bearer token)
- Policies: {ServiceName}Access, {ServiceName}Admin
- Rate Limiting: 100 req/60s (dev), 1000 req/60s (prod)

## 📝 Configuration
Ver `appsettings.json` y `appsettings.Development.json`

## 🐛 Troubleshooting
Ver [TROUBLESHOOTING.md](./TROUBLESHOOTING.md)

## 📚 Documentation
- [API Documentation](./docs/API.md)
- [Architecture Decision Records](./docs/ADR/)
```

---

### 10.2 TROUBLESHOOTING.md

**POLÍTICA**: Documentar problemas comunes y soluciones:

```markdown
# Troubleshooting - {ServiceName}

## Database Connection Issues

### Problema: "Failed to connect to 127.0.0.1:5432"
**Causa**: Puerto PostgreSQL incorrecto en appsettings.json

**Solución**:
\`\`\`json
"PostgreSQL": "Host=localhost;Port=25432;..." // Verificar puerto
\`\`\`

### Problema: "relation 'table' already exists"
**Causa**: Migraciones ya aplicadas

**Solución**: Normal en re-starts, el servicio continúa funcionando

## RabbitMQ Issues

### Problema: "BrokerUnreachableException"
**Causa**: RabbitMQ no iniciado

**Solución**:
\`\`\`bash
docker-compose up -d rabbitmq
\`\`\`

### Problema: Circuit Breaker OPEN
**Causa**: RabbitMQ falló 3 veces consecutivas

**Solución**: 
- Verificar RabbitMQ: `docker logs cargurus_rabbitmq`
- Esperar 1 minuto para Circuit Breaker reset automático

## Testing Issues

### Problema: Integration tests fallan con 401
**Causa**: JWT token expirado

**Solución**: Regenerar token en CustomWebApplicationFactory

## Build Issues

### Problema: MSB3026 - Cannot copy DLL
**Causa**: Proceso dotnet anterior no detenido

**Solución**:
\`\`\`powershell
Get-Process -Name dotnet | Stop-Process -Force
dotnet clean
dotnet build
\`\`\`
```

---

### 10.3 E2E_TESTING_RESULTS.md

**POLÍTICA**: Documentar resultados de E2E tests:

```markdown
# E2E Testing Results - {ServiceName}

**Fecha**: 2025-11-29  
**Versión**: 1.0.0  
**Ambiente**: Development (Real Infrastructure)

## 📊 Resumen de Ejecución

| Categoría | Tests | Passed | Failed | Skipped |
|-----------|-------|--------|--------|---------|
| Unit Tests | 20 | 20 | 0 | 0 |
| Integration Tests | 9 | 9 | 0 | 0 |
| E2E Tests | 6 | 6 | 0 | 0 |
| **TOTAL** | **35** | **35** | **0** | **0** |

## ✅ Tests Passing (35/35 - 100%)

### Unit Tests (20/20)
- ✅ LogErrorCommandHandler - ValidCommand_ReturnsSuccess
- ✅ LogErrorCommandValidator - EmptyMessage_ReturnsError
... (listar todos)

### Integration Tests (9/9)
- ✅ POST /api/errors - ValidJWT_Returns201
- ✅ POST /api/errors - NoJWT_Returns401
... (listar todos)

### E2E Tests (6/6)
- ✅ Health Check - Returns 200 OK
- ✅ Protected Endpoint - Returns 401 without JWT
... (listar todos)

## 🔍 Real Infrastructure Validation

### PostgreSQL Connection
- ✅ Connected to localhost:25432
- ✅ Database: {servicename}
- ✅ Migrations applied: InitialCreate, AddIndexes
- ✅ Query performance: < 35ms average

### RabbitMQ Connection
- ✅ Connected to localhost:5672
- ✅ Exchange: cardealer.events
- ✅ Circuit Breaker: Active
- ✅ Dead Letter Queue: Processing

### Service Health
- ✅ HTTP endpoint: http://localhost:5000
- ✅ HTTPS endpoint: https://localhost:5001
- ✅ Swagger UI: Accessible
- ✅ Health check: 200 OK

## 📝 Notas
- Todos los tests pasan con 100% de éxito
- Infraestructura real validada (PostgreSQL + RabbitMQ)
- Listo para deployment a QA/Staging
```

---

## 11. GIT Y CONTROL DE VERSIONES

### 11.1 Branch Strategy

**POLÍTICA**: Gitflow obligatorio:

```
main                    # Producción (protected)
├── develop             # Desarrollo (protected)
│   ├── feature/US-123-add-error-logging
│   ├── feature/US-124-implement-rabbitmq
│   ├── bugfix/BUG-456-fix-db-connection
│   └── hotfix/HOTFIX-789-critical-security-patch
└── release/v1.2.0      # Release candidates
```

**REGLAS**:
- ✅ `feature/*` para nuevas funcionalidades
- ✅ `bugfix/*` para correcciones no críticas
- ✅ `hotfix/*` para emergencias en producción
- ✅ PR obligatorio para merge a `develop` y `main`
- ❌ Commits directos a `main` o `develop` PROHIBIDOS

---

### 11.2 Commit Messages

**POLÍTICA**: Conventional Commits obligatorio:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**TIPOS**:
- `feat`: Nueva funcionalidad
- `fix`: Corrección de bug
- `docs`: Documentación
- `test`: Tests
- `refactor`: Refactorización
- `perf`: Mejora de performance
- `chore`: Mantenimiento

**EJEMPLOS**:
```
feat(errors): add error logging endpoint

Implements POST /api/errors endpoint with JWT authentication,
FluentValidation, and RabbitMQ event publishing.

Closes #123
```

```
fix(database): correct PostgreSQL port configuration

Changed connection string to use Port=25432 instead of
default 5432 to match docker-compose configuration.

Fixes #456
```

---

### 11.3 .gitignore

**POLÍTICA**: Usar .gitignore estándar:

```gitignore
## Build artifacts
bin/
obj/
*.dll
*.exe
*.pdb

## User-specific files
*.suo
*.user
*.userosscache
*.sln.docstates

## Secrets
appsettings.Development.json  # Solo si tiene secretos
*.env
secrets.json

## IDE
.vscode/
.vs/
.idea/

## Logs
logs/
*.log

## Docker
docker-compose.override.yml  # Overrides locales

## OS
.DS_Store
Thumbs.db
```

---

## 12. CODE REVIEW Y QUALITY GATES

### 12.1 Pull Request Template

**POLÍTICA**: OBLIGATORIO completar PR template:

```markdown
## 📋 Descripción
Breve descripción de los cambios.

## 🎯 Issue/Ticket
Closes #123

## 🔄 Tipo de Cambio
- [ ] ✨ Feature (nueva funcionalidad)
- [ ] 🐛 Bug fix
- [ ] 📝 Documentación
- [ ] ♻️ Refactor
- [ ] ✅ Tests

## ✅ Checklist

### Código
- [ ] Código sigue Clean Architecture
- [ ] Naming conventions correctas
- [ ] Sin código comentado o TODOs
- [ ] Sin Console.WriteLine o debugs

### Testing
- [ ] Unit tests agregados/actualizados
- [ ] Integration tests agregados/actualizados
- [ ] Todos los tests pasan (35/35)
- [ ] Cobertura > 80%

### Documentación
- [ ] README.md actualizado
- [ ] TROUBLESHOOTING.md actualizado (si aplica)
- [ ] Comentarios XML en código público

### Seguridad
- [ ] Sin secretos hardcodeados
- [ ] Validación de inputs implementada
- [ ] Authorization policies correctas

### Performance
- [ ] Sin queries N+1
- [ ] Async/await usado correctamente
- [ ] Connection pooling configurado

## 📸 Screenshots (si aplica)
```

---

### 12.2 Code Review Checklist

**POLÍTICA**: Reviewers DEBEN verificar:

**ARQUITECTURA**:
- [ ] Clean Architecture respetada
- [ ] No dependencias circulares
- [ ] Inyección de dependencias correcta

**CÓDIGO**:
- [ ] Naming conventions
- [ ] Sin código duplicado
- [ ] Métodos < 50 líneas
- [ ] Clases < 500 líneas

**TESTING**:
- [ ] Tests pasan (100%)
- [ ] Cobertura adecuada
- [ ] Tests significativos (no dummy)

**SEGURIDAD**:
- [ ] Sin secretos
- [ ] Validación de inputs
- [ ] Authorization correcta

**PERFORMANCE**:
- [ ] Async/await correcto
- [ ] Sin memory leaks
- [ ] Queries optimizadas

---

### 12.3 Quality Gates (CI/CD)

**POLÍTICA**: Bloquear merge si:

```yaml
quality_gates:
  - unit_test_coverage: >= 80%
  - integration_test_coverage: >= 60%
  - build: success
  - security_scan: no_critical_issues
  - code_analysis: grade_A_or_B
  - performance_tests: < 500ms_p95
```

---

## 13. DEPLOYMENT Y CI/CD

### 13.1 Pipeline Stages

**POLÍTICA**: Pipeline obligatorio con estas etapas:

```yaml
stages:
  - build          # dotnet build
  - test           # dotnet test
  - code_analysis  # SonarQube
  - security_scan  # OWASP Dependency Check
  - package        # Docker build
  - deploy_dev     # Auto-deploy a Dev
  - deploy_qa      # Manual approval
  - deploy_staging # Manual approval
  - deploy_prod    # Manual approval + rollback plan
```

---

### 13.2 Environment Variables por Ambiente

**POLÍTICA**: Variables separadas por ambiente:

```yaml
# Development
ASPNETCORE_ENVIRONMENT: Development
DATABASE_PROVIDER: PostgreSQL
DATABASE_PORT: 25432
RABBITMQ_HOST: localhost
LOG_LEVEL: Debug

# Staging
ASPNETCORE_ENVIRONMENT: Staging
DATABASE_PROVIDER: PostgreSQL
DATABASE_PORT: 5432
RABBITMQ_HOST: rabbitmq-staging.internal
LOG_LEVEL: Information

# Production
ASPNETCORE_ENVIRONMENT: Production
DATABASE_PROVIDER: PostgreSQL  # Azure Database for PostgreSQL
DATABASE_PORT: 5432
RABBITMQ_HOST: rabbitmq-prod.internal
LOG_LEVEL: Warning
```

---

### 13.3 Rollback Plan

**POLÍTICA**: OBLIGATORIO tener plan de rollback documentado:

```markdown
## Rollback Procedure

1. Identificar versión anterior estable: `v1.2.3`
2. Detener tráfico al servicio: `kubectl scale deployment {service} --replicas=0`
3. Revertir deployment: `kubectl rollout undo deployment/{service}`
4. Verificar health checks: `curl https://{service}/health`
5. Restaurar tráfico gradualmente: 10% → 50% → 100%
6. Monitorear logs y métricas durante 30 minutos
```

---

## 14. PERFORMANCE Y OPTIMIZACIÓN

### 14.1 Database Optimization

**POLÍTICA**: Queries optimizadas obligatorias:

```csharp
// ✅ CORRECTO - Proyección, paginación, índices
public async Task<IEnumerable<ErrorLogDto>> GetRecentErrorsAsync(
    int page = 1, 
    int pageSize = 20)
{
    return await _context.ErrorLogs
        .AsNoTracking()  // Read-only
        .Where(e => e.OccurredAt >= DateTime.UtcNow.AddDays(-7))
        .OrderByDescending(e => e.OccurredAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(e => new ErrorLogDto  // Proyección
        {
            Id = e.Id,
            ServiceName = e.ServiceName,
            Message = e.Message
        })
        .ToListAsync();
}

// ❌ PROHIBIDO - Cargar todo y filtrar en memoria
var errors = await _context.ErrorLogs.ToListAsync();  // NO!
return errors.Where(e => e.OccurredAt >= DateTime.UtcNow.AddDays(-7));
```

---

### 14.2 Async/Await Best Practices

**POLÍTICA**: Async correcto en toda la cadena:

```csharp
// ✅ CORRECTO - Async hasta el final
public async Task<IActionResult> CreateError([FromBody] CreateErrorDto dto)
{
    var command = _mapper.Map<CreateErrorCommand>(dto);
    var result = await _mediator.Send(command);  // Async
    return CreatedAtAction(nameof(GetError), new { id = result.Id }, result);
}

// ❌ PROHIBIDO - .Result bloquea thread
var result = _mediator.Send(command).Result;  // NO!

// ❌ PROHIBIDO - async sin await
public async Task DoSomething()
{
    // No hay await - warning CS1998
}
```

---

### 14.3 Memory Management

**POLÍTICA**: IDisposable implementado correctamente:

```csharp
// ✅ CORRECTO - using statement
public async Task ProcessFileAsync(string path)
{
    using var stream = File.OpenRead(path);
    using var reader = new StreamReader(stream);
    var content = await reader.ReadToEndAsync();
    // Dispose automático
}

// ✅ CORRECTO - IDisposable en clases
public class RabbitMqPublisher : IDisposable
{
    private IConnection _connection;
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        
        _connection?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
```

---

## 15. DEPENDENCY MANAGEMENT

### 15.1 NuGet Packages

**POLÍTICA**: Versiones específicas y actualizadas:

```xml
<!-- ✅ CORRECTO - Versiones específicas -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="FluentValidation" Version="11.8.0" />

<!-- ❌ PROHIBIDO - Wildcards -->
<PackageReference Include="Newtonsoft.Json" Version="*" />  <!-- NO! -->
```

**PROCESO DE ACTUALIZACIÓN**:
1. Revisar release notes y breaking changes
2. Actualizar en branch separado
3. Ejecutar TODOS los tests
4. Verificar en ambiente de staging
5. Merge con aprobación

---

### 15.2 Shared Libraries

**POLÍTICA**: Centralizar código compartido:

```
backend/
├── _Shared/
│   ├── CarDealer.Shared/
│   │   ├── Database/
│   │   │   ├── DatabaseExtensions.cs
│   │   │   └── DatabaseConfiguration.cs
│   │   ├── Messaging/
│   │   └── Security/
│   └── CarDealer.Contracts/
│       └── Events/
│           ├── ErrorCriticalEvent.cs
│           └── UserCreatedEvent.cs
```

**REGLA**: No duplicar código entre microservicios - crear shared library

---

## 📌 RESUMEN EJECUTIVO

### Políticas Críticas (No Negociables)

1. **✅ Clean Architecture**: Estructura de 6 capas obligatoria
2. **✅ Testing**: 80% unit, 60% integration, 40% E2E
3. **✅ Security**: JWT + Authorization policies en todos los endpoints
4. **✅ Observability**: OpenTelemetry + Serilog + Health checks
5. **✅ Resiliencia**: Circuit Breaker + Retry + Graceful degradation
6. **✅ Documentation**: README + TROUBLESHOOTING + E2E_RESULTS
7. **✅ Git**: Gitflow + Conventional Commits + PR template
8. **✅ Code Review**: Checklist completo antes de merge
9. **✅ CI/CD**: Pipeline con quality gates + rollback plan
10. **✅ Real Testing**: Validar con PostgreSQL y RabbitMQ reales antes de producción

---

### 📊 MATRIZ DE READINESS - Niveles Mínimos Obligatorios

**POLÍTICA CRÍTICA**: Un microservicio NO está listo para producción si alguna categoría está en 🟡 o 🔴.

| Categoría | Nivel Mínimo | Componentes Obligatorios |
|-----------|--------------|--------------------------|
| **Funcionalidad Core** | 🟢 100% | ✅ CQRS, Persistence, RabbitMQ + DLQ, JWT funcionando |
| **Seguridad** | 🟢 100% | ✅ JWT + Validación robusta + SQL/XSS detection |
| **Resiliencia** | 🟢 100% | ✅ Circuit Breaker + Auto-recovery implementado |
| **Observabilidad** | 🟢 100% | ✅ Logs + OpenTelemetry + TraceId + Sampling + Alerts |
| **Testing** | 🟢 100% | ✅ Tests completos + JWT + Integration Tests |
| **Producción Ready** | 🟢 100% | ✅ Seguridad + Resiliencia + Observabilidad COMPLETAS |

**EVALUACIÓN**:
- 🟢 100% = COMPLETO - Listo para producción
- 🟡 60-99% = EN PROGRESO - NO deployar
- 🔴 <60% = CRÍTICO - Bloquea merge a develop/main

---

### Checklist de Cumplimiento por Microservicio

#### FUNCIONALIDAD CORE (100% Obligatorio)
```markdown
- [ ] Estructura Clean Architecture completa (6 capas)
- [ ] CQRS con MediatR (Commands/Queries separados)
- [ ] Repository Pattern implementado
- [ ] Database multi-provider configurado
- [ ] RabbitMQ Event Publisher con DLQ
- [ ] appsettings.json con todas las secciones
```

#### SEGURIDAD (100% Obligatorio)
```markdown
- [ ] JWT authentication implementado
- [ ] Authorization policies definidas (mínimo 3)
- [ ] FluentValidation en todos los Commands/DTOs
- [ ] SQL Injection detection (11 patrones)
- [ ] XSS detection (8 patrones)
- [ ] Size limits en payloads (Message: 5KB, StackTrace: 50KB)
```

#### RESILIENCIA (100% Obligatorio)
```markdown
- [ ] Circuit Breaker configurado (Polly 8.4.2+)
- [ ] Retry policies implementadas
- [ ] Graceful degradation validado
- [ ] Dead Letter Queue funcionando
- [ ] Auto-recovery testeado manualmente
```

#### OBSERVABILIDAD (100% Obligatorio)
```markdown
- [ ] OpenTelemetry configurado (Tracing + Metrics)
- [ ] Serilog con structured logging
- [ ] TraceId y SpanId en logs (Serilog.Enrichers.Span)
- [ ] Sampling Strategy (10% prod, 100% dev)
- [ ] Health checks implementados
- [ ] Prometheus alerts configuradas (mínimo 5 reglas)
```

#### TESTING (100% Obligatorio)
```markdown
- [ ] Unit tests >= 80% cobertura
- [ ] Integration tests >= 60% cobertura
- [ ] E2E tests >= 40% cobertura
- [ ] CustomWebApplicationFactory creado
- [ ] Tests de JWT authorization
- [ ] E2E-TESTING-SCRIPT.ps1 funcional
- [ ] E2E_TESTING_RESULTS.md documentado
```

#### DOCUMENTACIÓN (Obligatorio)
```markdown
- [ ] README.md completo (estructura, setup, testing)
- [ ] TROUBLESHOOTING.md creado (4-step checklist)
- [ ] SECURITY_IMPLEMENTATION.md (JWT + Validation)
- [ ] RESILIENCE_IMPLEMENTATION.md (Circuit Breaker)
- [ ] OBSERVABILITY_IMPLEMENTATION.md (OpenTelemetry)
```

#### INFRAESTRUCTURA REAL (100% Obligatorio)
```markdown
- [ ] Real infrastructure testing ejecutado
- [ ] PostgreSQL conectando correctamente (port 25432)
- [ ] RabbitMQ publicando eventos (port 5672)
- [ ] Circuit Breaker validado manualmente (detener RabbitMQ)
- [ ] Health endpoint: 200 OK con JSON response
- [ ] Graceful degradation confirmado
```

#### GIT Y CI/CD (Obligatorio)
```markdown
- [ ] docker-compose.yml para testing local
- [ ] .gitignore configurado correctamente
- [ ] PR template completo usado
- [ ] Code review checklist completo (30+ items)
- [ ] Pipeline CI/CD configurado con quality gates
- [ ] Quality gates pasando (coverage thresholds)
- [ ] Rollback plan documentado
```

---

## 📚 Referencias y Templates

- **ErrorService**: Microservicio de referencia con implementación completa
- **E2E-TESTING-SCRIPT.ps1**: Template de testing automatizado
- **CustomWebApplicationFactory.cs**: Template de integration testing
- **DatabaseExtensions.cs**: Configuración multi-provider compartida
- **TROUBLESHOOTING.md**: Guía de resolución de problemas comunes

---

**Versión del Documento**: 1.0  
**Última Actualización**: 2025-11-29  
**Responsable**: Equipo de Arquitectura CarDealer

**NOTA**: Este documento es un ESTÁNDAR OBLIGATORIO. Desviaciones requieren aprobación escrita del Arquitecto de Software.
