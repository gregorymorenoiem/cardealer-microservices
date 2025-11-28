# 📋 PLAN DE REFACTORIZACIÓN - Sistema de Microservicios CarDealer

**Fecha:** 28 Noviembre 2025  
**Objetivo:** Eliminar dependencias circulares y establecer arquitectura profesional event-driven  
**Duración Estimada:** 12-15 días hábiles  
**Prioridad:** ALTA - Crítico para escalabilidad y mantenibilidad

---

## 🎯 OBJETIVOS PRINCIPALES

1. ✅ Eliminar todas las referencias cruzadas entre microservicios
2. ✅ Implementar comunicación event-driven con RabbitMQ
3. ✅ Crear librería compartida solo para contratos (DTOs de eventos)
4. ✅ Establecer patrón Publisher/Subscriber consistente
5. ✅ Implementar alertas a Microsoft Teams en NotificationService
6. ✅ Mantener 100% de funcionalidad existente

---

## 📊 FASES DEL PLAN

### **FASE 0: Preparación y Análisis** (1 día)

#### ✅ Tareas:
- [x] Análisis de referencias cruzadas actual
- [x] Documentación de arquitectura propuesta
- [x] Backup de código actual
- [x] Crear rama feature/refactor-microservices
- [x] Planificación de testing
- [x] Configurar entorno de pruebas

#### 📝 Comandos:
```powershell
# Backup
git checkout -b backup/before-refactor-$(Get-Date -Format "yyyyMMdd")
git push origin backup/before-refactor-$(Get-Date -Format "yyyyMMdd")

# Nueva rama de trabajo
git checkout develop
git pull origin develop
git checkout -b feature/refactor-microservices

# Crear directorio de tests
New-Item -ItemType Directory -Path "backend/IntegrationTests" -Force
```

#### 📦 Entregables:
- ✅ Documentación de arquitectura (ARQUITECTURA_MICROSERVICIOS.md)
- ✅ Plan de refactorización (este documento)
- ✅ Backup en Git (https://github.com/gmorenotrade/cardealer-microservices)
- ✅ Rama de trabajo creada (feature/refactor-microservices)
- ✅ Entorno de testing configurado (backend/IntegrationTests)
- ✅ Plan de testing completo (backend/IntegrationTests/TEST_PLAN.md)

---

### **FASE 1: Crear Librería Compartida (CarDealer.Contracts)** (2 días)

#### 🎯 Objetivo:
Crear una librería NuGet interna con SOLO contratos de eventos y DTOs comunes que todos los servicios puedan referenciar sin crear dependencias circulares.

#### ✅ Tareas:

##### Día 1: Estructura y Eventos Base
- [ ] Crear proyecto CarDealer.Contracts (.NET 8.0 Class Library)
- [ ] Configurar como NuGet package
- [ ] Definir estructura de carpetas
- [ ] Crear eventos de AuthService
- [ ] Crear eventos de VehicleService
- [ ] Crear eventos de MediaService

##### Día 2: Eventos Restantes y DTOs
- [ ] Crear eventos de NotificationService
- [ ] Crear eventos de ErrorService
- [ ] Crear eventos de AuditService
- [ ] Crear DTOs comunes
- [ ] Crear enums compartidos
- [ ] Documentar todos los contratos

#### 📁 Estructura:
```
backend/CarDealer.Contracts/
├── CarDealer.Contracts.csproj
├── Events/
│   ├── Auth/
│   │   ├── UserRegisteredEvent.cs
│   │   ├── UserLoggedInEvent.cs
│   │   ├── UserLoggedOutEvent.cs
│   │   ├── PasswordChangedEvent.cs
│   │   └── UserDeletedEvent.cs
│   ├── Vehicle/
│   │   ├── VehicleCreatedEvent.cs
│   │   ├── VehicleUpdatedEvent.cs
│   │   ├── VehicleDeletedEvent.cs
│   │   └── VehicleSoldEvent.cs
│   ├── Media/
│   │   ├── MediaUploadedEvent.cs
│   │   ├── MediaProcessedEvent.cs
│   │   ├── MediaDeletedEvent.cs
│   │   └── MediaProcessingFailedEvent.cs
│   ├── Notification/
│   │   ├── NotificationSentEvent.cs
│   │   ├── NotificationFailedEvent.cs
│   │   └── TeamsAlertSentEvent.cs
│   ├── Error/
│   │   ├── ErrorLoggedEvent.cs
│   │   ├── ErrorCriticalEvent.cs
│   │   ├── ErrorSpikeDetectedEvent.cs
│   │   └── ServiceDownDetectedEvent.cs
│   └── Audit/
│       ├── AuditLogCreatedEvent.cs
│       └── ComplianceEventRecordedEvent.cs
├── DTOs/
│   └── Common/
│       ├── PaginationDto.cs
│       ├── ApiResponse.cs
│       └── ErrorDetailsDto.cs
├── Enums/
│   └── ServiceNames.cs
└── Abstractions/
    ├── IEvent.cs
    └── EventBase.cs
```

#### 💻 Código Ejemplo:

**EventBase.cs:**
```csharp
namespace CarDealer.Contracts.Abstractions;

public interface IEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
}

public abstract class EventBase : IEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
```

**UserRegisteredEvent.cs:**
```csharp
using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Auth;

public class UserRegisteredEvent : EventBase
{
    public override string EventType => "auth.user.registered";
    
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
```

**ErrorCriticalEvent.cs:**
```csharp
using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Error;

public class ErrorCriticalEvent : EventBase
{
    public override string EventType => "error.critical";
    
    public Guid ErrorId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public int StatusCode { get; set; }
    public string? Endpoint { get; set; }
    public string? UserId { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
```

#### 📝 Comandos:
```powershell
# Crear proyecto
dotnet new classlib -n CarDealer.Contracts -o backend/CarDealer.Contracts -f net8.0

# Configurar como NuGet
cd backend/CarDealer.Contracts
dotnet pack -c Release -o ./nupkg

# Agregar a solución
cd ..
dotnet sln CarDealer.sln add CarDealer.Contracts/CarDealer.Contracts.csproj
```

#### 📦 Entregables:
- [ ] Proyecto CarDealer.Contracts creado
- [ ] 20+ eventos definidos
- [ ] DTOs comunes creados
- [ ] NuGet package generado
- [ ] Documentación de contratos
- [ ] Tests unitarios de serialización

---

### **FASE 2: Limpiar ErrorService** (1-2 días)

#### 🎯 Objetivo:
ErrorService es el servicio más referenciado. Debe ser 100% autónomo y solo publicar eventos.

#### ✅ Tareas:

##### Paso 1: Actualizar ErrorService
- [ ] Agregar referencia a CarDealer.Contracts
- [ ] Agregar RabbitMQ.Client package
- [ ] Crear IEventPublisher interface
- [ ] Implementar RabbitMQEventPublisher
- [ ] Modificar LogErrorCommandHandler para publicar eventos
- [ ] Crear ErrorCriticalEvent publisher
- [ ] Crear ErrorSpikeDetectedEvent publisher

##### Paso 2: Crear Consumers en ErrorService
- [ ] Consumer para *.error.* (todos los errores de otros servicios)
- [ ] Consumer para auth.error.*
- [ ] Consumer para vehicle.error.*
- [ ] Consumer para media.error.*

##### Paso 3: Testing
- [ ] Unit tests de publishers
- [ ] Integration tests de consumers
- [ ] Verificar persistencia en BD
- [ ] Verificar publicación de eventos

#### 💻 Código Ejemplo:

**IEventPublisher.cs:**
```csharp
namespace ErrorService.Domain.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, string exchange, string routingKey) 
        where TEvent : class;
}
```

**LogErrorCommandHandler.cs (modificado):**
```csharp
public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, LogErrorResponse>
{
    private readonly IErrorLogRepository _errorLogRepository;
    private readonly IEventPublisher _eventPublisher; // ← NUEVO

    public async Task<LogErrorResponse> Handle(LogErrorCommand command)
    {
        var errorLog = new ErrorLog { /* ... */ };
        await _errorLogRepository.AddAsync(errorLog);
        
        // ✅ PUBLICAR EVENTO si es crítico
        if (errorLog.StatusCode >= 500)
        {
            var criticalEvent = new ErrorCriticalEvent
            {
                ErrorId = errorLog.Id,
                ServiceName = errorLog.ServiceName,
                ExceptionType = errorLog.ExceptionType,
                Message = errorLog.Message,
                StackTrace = errorLog.StackTrace,
                StatusCode = errorLog.StatusCode ?? 500,
                Endpoint = errorLog.Endpoint,
                UserId = errorLog.UserId,
                Metadata = errorLog.Metadata
            };
            
            await _eventPublisher.PublishAsync(
                criticalEvent, 
                "error.events", 
                "error.critical"
            );
        }
        
        return new LogErrorResponse(errorLog.Id);
    }
}
```

#### 📝 Comandos:
```powershell
cd backend/ErrorService

# Agregar packages
dotnet add ErrorService.Infrastructure/ErrorService.Infrastructure.csproj package RabbitMQ.Client
dotnet add ErrorService.Infrastructure/ErrorService.Infrastructure.csproj reference ../CarDealer.Contracts/CarDealer.Contracts.csproj

# Compilar
dotnet build ErrorService.sln
```

#### 📦 Entregables:
- [ ] ErrorService sin dependencias de otros servicios
- [ ] Event publisher implementado
- [ ] Consumers implementados
- [ ] Tests pasando
- [ ] Documentación actualizada

---

### **FASE 3: Refactorizar NotificationService** (2-3 días)

#### 🎯 Objetivo:
Convertir NotificationService en hub de comunicaciones con soporte para Teams alerts.

#### ✅ Tareas:

##### Día 1: Limpiar Referencias
- [ ] **ELIMINAR** ProjectReference a AuthService.Shared
- [ ] **ELIMINAR** ProjectReference a AuthService.Infrastructure  
- [ ] **ELIMINAR** ProjectReference a ErrorService.Shared
- [ ] **AGREGAR** CarDealer.Contracts
- [ ] Compilar y verificar errores

##### Día 2: Implementar Teams Provider
- [ ] Crear ITeamsProvider interface
- [ ] Implementar TeamsProvider (Adaptive Cards)
- [ ] Agregar TeamsSettings en NotificationSettings.cs
- [ ] Agregar NotificationType.Teams enum
- [ ] Crear endpoint POST /api/notifications/teams
- [ ] Crear DTOs (SendTeamsNotificationRequest/Response)
- [ ] Implementar SendTeamsNotificationCommand/Handler

##### Día 3: Implementar Event Consumers
- [ ] Consumer para auth.user.registered → Welcome Email
- [ ] Consumer para error.critical → Teams Alert ⭐
- [ ] Consumer para vehicle.sold → Confirmation Email
- [ ] Consumer para media.processing.failed → Alert Email
- [ ] Configurar RabbitMQ bindings

#### 📁 Archivos Nuevos:

```
NotificationService.Domain/Interfaces/External/
└── ITeamsProvider.cs

NotificationService.Infrastructure/External/
└── MicrosoftTeamsProvider.cs

NotificationService.Application/UseCases/SendTeamsNotification/
├── SendTeamsNotificationCommand.cs
├── SendTeamsNotificationCommandHandler.cs
└── SendTeamsNotificationValidator.cs

NotificationService.Infrastructure/Messaging/Consumers/
├── UserRegisteredEventConsumer.cs
├── ErrorCriticalEventConsumer.cs        ← ⭐ TEAMS ALERTS
├── VehicleSoldEventConsumer.cs
└── MediaProcessingFailedEventConsumer.cs
```

#### 💻 Código Ejemplo:

**ITeamsProvider.cs:**
```csharp
namespace NotificationService.Domain.Interfaces.External;

public interface ITeamsProvider
{
    string ProviderName { get; }
    
    Task<(bool success, string? messageId, string? error)> SendAsync(
        string webhookUrl,
        string title,
        string message,
        string severity = "Info",
        Dictionary<string, string>? facts = null,
        Dictionary<string, object>? metadata = null);
}
```

**ErrorCriticalEventConsumer.cs:**
```csharp
using CarDealer.Contracts.Events.Error;
using RabbitMQ.Client.Events;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class ErrorCriticalEventConsumer : BackgroundService
{
    private readonly ITeamsProvider _teamsProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ErrorCriticalEventConsumer> _logger;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var @event = JsonSerializer.Deserialize<ErrorCriticalEvent>(json);

            if (@event != null)
            {
                // 📧 Enviar alerta a Teams
                var webhookUrl = _configuration["NotificationSettings:Teams:CriticalAlertsWebhook"];
                
                await _teamsProvider.SendAsync(
                    webhookUrl: webhookUrl,
                    title: $"🔴 Error Crítico en {@event.ServiceName}",
                    message: @event.Message,
                    severity: "Critical",
                    facts: new Dictionary<string, string>
                    {
                        ["Error ID"] = @event.ErrorId.ToString(),
                        ["Servicio"] = @event.ServiceName,
                        ["Tipo"] = @event.ExceptionType,
                        ["Código HTTP"] = @event.StatusCode.ToString(),
                        ["Endpoint"] = @event.Endpoint ?? "N/A",
                        ["Fecha"] = @event.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss UTC")
                    }
                );

                _logger.LogInformation(
                    "Teams alert sent for critical error {ErrorId} from {ServiceName}",
                    @event.ErrorId, @event.ServiceName
                );
            }

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume("notification.error.critical", false, consumer);
        return Task.CompletedTask;
    }
}
```

**NotificationService.Api.csproj (actualizado):**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <ItemGroup>
    <!-- ❌ ELIMINAR ESTAS REFERENCIAS -->
    <!-- <ProjectReference Include="..\..\AuthService\AuthService.Shared\..." /> -->
    <!-- <ProjectReference Include="..\..\ErrorService\ErrorService.Shared\..." /> -->
    
    <!-- ✅ AGREGAR ESTA REFERENCIA -->
    <ProjectReference Include="..\..\CarDealer.Contracts\CarDealer.Contracts.csproj" />
    
    <!-- Referencias propias del servicio -->
    <ProjectReference Include="..\NotificationService.Application\..." />
    <ProjectReference Include="..\NotificationService.Infrastructure\..." />
    <ProjectReference Include="..\NotificationService.Shared\..." />
  </ItemGroup>
</Project>
```

#### 📝 Configuración:

**appsettings.json:**
```json
{
  "NotificationSettings": {
    "Teams": {
      "Enabled": true,
      "CriticalAlertsWebhook": "https://outlook.office.com/webhook/xxxxx",
      "GeneralAlertsWebhook": "https://outlook.office.com/webhook/yyyyy",
      "DefaultWebhook": "https://outlook.office.com/webhook/zzzzz"
    },
    "SendGrid": { /* ... */ },
    "Twilio": { /* ... */ },
    "Firebase": { /* ... */ }
  },
  "RabbitMQ": {
    "HostName": "rabbitmq",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "Consumers": {
      "ErrorCritical": {
        "QueueName": "notification.error.critical",
        "Exchange": "error.events",
        "RoutingKey": "error.critical"
      },
      "UserRegistered": {
        "QueueName": "notification.auth.registered",
        "Exchange": "auth.events",
        "RoutingKey": "auth.user.registered"
      }
    }
  }
}
```

#### 📦 Entregables:
- [ ] Referencias cruzadas eliminadas
- [ ] ITeamsProvider implementado
- [ ] Endpoint /api/notifications/teams funcionando
- [ ] 4 consumers implementados
- [ ] Tests de integración pasando
- [ ] Documentación de API actualizada

---

### **FASE 4: Refactorizar AuthService** (1-2 días)

#### 🎯 Objetivo:
AuthService debe publicar eventos de autenticación sin depender de ErrorService.

#### ✅ Tareas:

##### Paso 1: Limpiar Referencias
- [ ] **ELIMINAR** ProjectReference a ErrorService.Shared
- [ ] **AGREGAR** CarDealer.Contracts
- [ ] **AGREGAR** RabbitMQ.Client
- [ ] Reemplazar uso de ErrorService.Shared.Exceptions con propias

##### Paso 2: Crear Exceptions Propias
- [ ] AuthService.Shared/Exceptions/UnauthorizedException.cs
- [ ] AuthService.Shared/Exceptions/BadRequestException.cs
- [ ] AuthService.Shared/Exceptions/NotFoundException.cs
- [ ] Middleware de manejo de errores propio

##### Paso 3: Implementar Event Publishers
- [ ] Publisher para UserRegisteredEvent
- [ ] Publisher para UserLoggedInEvent
- [ ] Publisher para PasswordChangedEvent
- [ ] Publisher para UserDeletedEvent

##### Paso 4: Publicar Errores como Eventos
- [ ] En catch blocks, publicar AuthErrorEvent
- [ ] ErrorService consumirá estos eventos

#### 💻 Código Ejemplo:

**RegisterUserCommandHandler.cs (actualizado):**
```csharp
using CarDealer.Contracts.Events.Auth;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventPublisher _eventPublisher; // ← NUEVO
    private readonly IPasswordHasher _passwordHasher;

    public async Task<RegisterUserResponse> Handle(RegisterUserCommand command)
    {
        try
        {
            // Validar que no exista
            var existingUser = await _userRepository.GetByEmailAsync(command.Email);
            if (existingUser != null)
                throw new BadRequestException("User already exists");

            // Crear usuario
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = command.Email,
                FullName = command.FullName,
                PasswordHash = _passwordHasher.Hash(command.Password),
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            // ✅ PUBLICAR EVENTO
            var @event = new UserRegisteredEvent
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                RegisteredAt = user.CreatedAt
            };

            await _eventPublisher.PublishAsync(@event, "auth.events", "auth.user.registered");

            return new RegisterUserResponse(user.Id);
        }
        catch (Exception ex)
        {
            // ✅ PUBLICAR ERROR como evento
            var errorEvent = new AuthErrorEvent
            {
                ServiceName = "AuthService",
                ErrorCode = "REGISTRATION_FAILED",
                ErrorMessage = ex.Message,
                StackTrace = ex.StackTrace,
                Endpoint = "/api/auth/register",
                StatusCode = ex is BadRequestException ? 400 : 500
            };

            await _eventPublisher.PublishAsync(errorEvent, "auth.events", "auth.error.registration");
            throw;
        }
    }
}
```

#### 📦 Entregables:
- [ ] AuthService sin referencias a ErrorService
- [ ] Excepciones propias creadas
- [ ] Event publishers implementados
- [ ] Errores publicados como eventos
- [ ] Tests pasando

---

### **FASE 5: Refactorizar VehicleService y MediaService** (2 días)

#### 🎯 Objetivo:
Implementar event publishers en servicios de negocio principales.

#### ✅ Tareas:

##### VehicleService (1 día)
- [ ] Agregar CarDealer.Contracts
- [ ] Agregar RabbitMQ.Client
- [ ] Publisher para VehicleCreatedEvent
- [ ] Publisher para VehicleUpdatedEvent
- [ ] Publisher para VehicleDeletedEvent
- [ ] Publisher para VehicleSoldEvent
- [ ] Publisher para VehicleErrorEvent

##### MediaService (1 día)
- [ ] Agregar CarDealer.Contracts
- [ ] Publisher para MediaUploadedEvent
- [ ] Publisher para MediaProcessedEvent
- [ ] Publisher para MediaDeletedEvent
- [ ] Publisher para MediaProcessingFailedEvent
- [ ] Publisher para MediaErrorEvent

#### 💻 Código Ejemplo:

**CreateVehicleCommandHandler.cs:**
```csharp
using CarDealer.Contracts.Events.Vehicle;

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, CreateVehicleResponse>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IEventPublisher _eventPublisher;

    public async Task<CreateVehicleResponse> Handle(CreateVehicleCommand command)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Make = command.Make,
            Model = command.Model,
            Year = command.Year,
            Price = command.Price,
            CreatedAt = DateTime.UtcNow
        };

        await _vehicleRepository.AddAsync(vehicle);

        // ✅ PUBLICAR EVENTO
        var @event = new VehicleCreatedEvent
        {
            VehicleId = vehicle.Id,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Price = vehicle.Price,
            CreatedBy = command.UserId,
            CreatedAt = vehicle.CreatedAt
        };

        await _eventPublisher.PublishAsync(@event, "vehicle.events", "vehicle.created");

        return new CreateVehicleResponse(vehicle.Id);
    }
}
```

#### 📦 Entregables:
- [ ] VehicleService publicando eventos
- [ ] MediaService publicando eventos
- [ ] Tests de integración
- [ ] Documentación actualizada

---

### **FASE 6: Configurar AuditService como Consumer Universal** (1 día)

#### 🎯 Objetivo:
AuditService escucha TODOS los eventos para auditoría.

#### ✅ Tareas:
- [ ] Consumer para auth.* (todos los eventos de auth)
- [ ] Consumer para vehicle.* (todos los eventos de vehículos)
- [ ] Consumer para media.* (todos los eventos de media)
- [ ] Consumer para notification.* (auditar notificaciones)
- [ ] Persistir en BD con metadata completa

#### 💻 Código Ejemplo:

**UniversalEventConsumer.cs:**
```csharp
public class UniversalEventConsumer : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var routingKey = ea.RoutingKey;
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            // Guardar auditoría
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EventType = routingKey,
                Payload = json,
                Exchange = ea.Exchange,
                OccurredAt = DateTime.UtcNow
            };

            await _auditRepository.AddAsync(auditLog);
            _channel.BasicAck(ea.DeliveryTag, false);
        };

        // Consumir TODO
        _channel.QueueBind("audit.all", "auth.events", "#");
        _channel.QueueBind("audit.all", "vehicle.events", "#");
        _channel.QueueBind("audit.all", "media.events", "#");
        _channel.QueueBind("audit.all", "notification.events", "#");
        _channel.QueueBind("audit.all", "error.events", "#");

        _channel.BasicConsume("audit.all", false, consumer);
        return Task.CompletedTask;
    }
}
```

#### 📦 Entregables:
- [ ] AuditService consumiendo todos los eventos
- [ ] BD de auditoría completa
- [ ] Dashboard de auditoría
- [ ] Tests de integración

---

### **FASE 7: Testing de Integración End-to-End** (2-3 días)

#### 🎯 Objetivo:
Validar que toda la arquitectura funciona correctamente.

#### ✅ Tareas:

##### Día 1: Tests de Flujos Principales
- [ ] Test: Registro de usuario → Email bienvenida
- [ ] Test: Error crítico → Alerta Teams ⭐
- [ ] Test: Creación de vehículo → Auditoría
- [ ] Test: Upload de media → Procesamiento → Notificación

##### Día 2: Tests de Resiliencia
- [ ] Test: RabbitMQ caído → Retry logic
- [ ] Test: Servicio caído → DLQ (Dead Letter Queue)
- [ ] Test: Mensaje malformado → Logging sin crash
- [ ] Test: Concurrencia alta → No duplicados

##### Día 3: Tests de Performance
- [ ] Load testing con 1000 eventos/seg
- [ ] Latencia de end-to-end
- [ ] Memory leaks en consumers
- [ ] Throughput de RabbitMQ

#### 🧪 Tests Ejemplo:

**ErrorCriticalToTeamsTest.cs:**
```csharp
[Fact]
public async Task ErrorCritical_Should_SendTeamsAlert()
{
    // Arrange
    var errorEvent = new ErrorCriticalEvent
    {
        ErrorId = Guid.NewGuid(),
        ServiceName = "VehicleService",
        Message = "Database connection failed",
        StatusCode = 500
    };

    // Act
    await _eventPublisher.PublishAsync(errorEvent, "error.events", "error.critical");
    await Task.Delay(2000); // Wait for consumer

    // Assert
    var teamsCalls = _teamsMock.GetCalls();
    Assert.Single(teamsCalls);
    Assert.Contains("🔴 Error Crítico", teamsCalls[0].Title);
    Assert.Contains("VehicleService", teamsCalls[0].Message);
}

[Fact]
public async Task UserRegistered_Should_SendWelcomeEmail()
{
    // Arrange
    var @event = new UserRegisteredEvent
    {
        UserId = Guid.NewGuid(),
        Email = "test@test.com",
        FullName = "Test User"
    };

    // Act
    await _eventPublisher.PublishAsync(@event, "auth.events", "auth.user.registered");
    await Task.Delay(2000);

    // Assert
    var emails = await _emailRepository.GetByRecipientAsync("test@test.com");
    Assert.Single(emails);
    Assert.Contains("Bienvenido", emails[0].Subject);
}
```

#### 📦 Entregables:
- [ ] 20+ integration tests pasando
- [ ] Performance tests exitosos
- [ ] Reporte de cobertura >80%
- [ ] Documentación de tests

---

### **FASE 8: Configuración de Infraestructura** (1 día)

#### 🎯 Objetivo:
Configurar RabbitMQ, monitoring y deployment.

#### ✅ Tareas:

##### RabbitMQ
- [ ] Configurar exchanges con durabilidad
- [ ] Configurar queues con DLQ
- [ ] Configurar TTL para mensajes
- [ ] Configurar políticas de retry
- [ ] Configurar monitoreo de RabbitMQ

##### Monitoring
- [ ] Dashboard de RabbitMQ
- [ ] Alertas de queues llenas
- [ ] Métricas de latencia
- [ ] Logs centralizados (ELK/Seq)

##### Docker Compose
- [ ] Actualizar docker-compose.yml
- [ ] Configurar health checks
- [ ] Configurar restart policies
- [ ] Variables de entorno

#### 📝 docker-compose.yml (actualizado):

```yaml
version: '3.8'

services:
  rabbitmq:
    image: rabbitmq:3-management
    container_name: cardealer-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}
    ports:
      - "5672:5672"   # AMQP
      - "15672:15672" # Management UI
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  errorservice:
    depends_on:
      rabbitmq:
        condition: service_healthy
    environment:
      RabbitMQ__Host: rabbitmq
      RabbitMQ__Username: admin
      RabbitMQ__Password: ${RABBITMQ_PASSWORD}

  notificationservice:
    depends_on:
      rabbitmq:
        condition: service_healthy
    environment:
      RabbitMQ__Host: rabbitmq
      NotificationSettings__Teams__CriticalAlertsWebhook: ${TEAMS_WEBHOOK_CRITICAL}

volumes:
  rabbitmq_data:
```

#### 📦 Entregables:
- [ ] RabbitMQ configurado en producción
- [ ] Monitoring dashboard activo
- [ ] Docker compose actualizado
- [ ] Scripts de deployment

---

### **FASE 9: Documentación y Capacitación** (1 día)

#### 🎯 Objetivo:
Documentar toda la arquitectura y capacitar al equipo.

#### ✅ Tareas:
- [ ] Actualizar README.md de cada servicio
- [ ] Documentar eventos en Confluence/Wiki
- [ ] Crear diagramas de secuencia
- [ ] Crear guía de troubleshooting
- [ ] Sesión de capacitación con equipo
- [ ] Documentar runbooks de operaciones

#### 📚 Documentos a Crear:
- [ ] ARCHITECTURE.md - Visión general
- [ ] EVENTS_CATALOG.md - Catálogo de eventos
- [ ] TROUBLESHOOTING.md - Solución de problemas
- [ ] DEPLOYMENT.md - Guía de deployment
- [ ] MONITORING.md - Guía de monitoreo

#### 📦 Entregables:
- [ ] Documentación completa
- [ ] Equipo capacitado
- [ ] Runbooks creados

---

### **FASE 10: Deployment a Producción** (1 día)

#### 🎯 Objetivo:
Deployment controlado con rollback plan.

#### ✅ Tareas:

##### Pre-deployment
- [ ] Code review completo
- [ ] Approval de stakeholders
- [ ] Backup de BD producción
- [ ] Snapshot de infraestructura

##### Deployment
- [ ] Deploy a staging
- [ ] Smoke tests en staging
- [ ] Deploy a producción (blue-green)
- [ ] Verificación de health checks
- [ ] Monitoreo activo por 2 horas

##### Post-deployment
- [ ] Validación de funcionalidad
- [ ] Review de métricas
- [ ] Documentación de incidencias
- [ ] Retrospectiva de deployment

#### 📦 Entregables:
- [ ] Sistema en producción
- [ ] Monitoring activo
- [ ] Rollback plan documentado
- [ ] Post-mortem si hay issues

---

## 📊 RESUMEN DE TIEMPOS

| Fase | Duración | Dependencias | Riesgo |
|------|----------|--------------|--------|
| 0. Preparación | 1 día | - | Bajo |
| 1. Contracts | 2 días | Fase 0 | Bajo |
| 2. ErrorService | 1-2 días | Fase 1 | Medio |
| 3. NotificationService | 2-3 días | Fase 1 | Medio |
| 4. AuthService | 1-2 días | Fase 1 | Medio |
| 5. Vehicle/MediaService | 2 días | Fase 1 | Bajo |
| 6. AuditService | 1 día | Fase 1 | Bajo |
| 7. Testing E2E | 2-3 días | Fases 2-6 | Alto |
| 8. Infraestructura | 1 día | Fase 7 | Medio |
| 9. Documentación | 1 día | Fase 8 | Bajo |
| 10. Deployment | 1 día | Fase 9 | Alto |
| **TOTAL** | **12-15 días** | | |

---

## 🎯 HITOS CLAVE

### ✅ Semana 1 (Días 1-5)
- [x] Preparación completa
- [ ] CarDealer.Contracts creado y publicado
- [ ] ErrorService refactorizado
- [ ] NotificationService con Teams alerts ⭐

### ✅ Semana 2 (Días 6-10)
- [ ] AuthService refactorizado
- [ ] VehicleService y MediaService con eventos
- [ ] AuditService consumiendo eventos
- [ ] Tests de integración pasando

### ✅ Semana 3 (Días 11-15)
- [ ] Infraestructura configurada
- [ ] Documentación completa
- [ ] Deployment a producción ✅
- [ ] Sistema funcionando sin referencias cruzadas ✅

---

## ⚠️ RIESGOS Y MITIGACIONES

### Riesgo 1: Testing Insuficiente
**Probabilidad:** Media  
**Impacto:** Alto  
**Mitigación:**
- Dedicar 2-3 días completos a testing
- Automatizar tests de integración
- Smoke tests obligatorios antes de producción

### Riesgo 2: Pérdida de Mensajes en RabbitMQ
**Probabilidad:** Baja  
**Impacto:** Alto  
**Mitigación:**
- Configurar queues como durable
- Implementar DLQ (Dead Letter Queue)
- Logging exhaustivo de publicación/consumo
- Retry logic con exponential backoff

### Riesgo 3: Downtime en Deployment
**Probabilidad:** Media  
**Impacto:** Alto  
**Mitigación:**
- Blue-green deployment
- Feature flags para nuevos flujos
- Rollback plan probado
- Deploy fuera de horas pico

### Riesgo 4: Performance de RabbitMQ
**Probabilidad:** Baja  
**Impacto:** Medio  
**Mitigación:**
- Load testing antes de producción
- Configurar límites de memoria
- Monitoring de throughput
- Plan de escalado horizontal

---

## 📋 CHECKLIST FINAL

### Antes de Empezar
- [ ] Plan revisado y aprobado por equipo
- [ ] Recursos asignados (desarrolladores, DevOps)
- [ ] Entorno de staging disponible
- [ ] Acceso a RabbitMQ configurado
- [ ] Webhook de Teams creado y probado

### Durante Ejecución
- [ ] Daily standups para tracking
- [ ] Code reviews obligatorios
- [ ] Tests automatizados en CI/CD
- [ ] Documentación actualizada continuamente
- [ ] Monitoreo de progreso vs plan

### Antes de Producción
- [ ] Todos los tests pasando (unit + integration + E2E)
- [ ] Code coverage >80%
- [ ] Performance tests exitosos
- [ ] Documentación completa
- [ ] Runbooks de operaciones listos
- [ ] Equipo de soporte capacitado
- [ ] Rollback plan probado
- [ ] Stakeholders notificados

---

## 🚀 CRITERIOS DE ÉXITO

### Técnicos
✅ **Cero referencias cruzadas** entre microservicios  
✅ **100% de funcionalidad** mantenida  
✅ **Eventos publicados/consumidos** correctamente  
✅ **Teams alerts** funcionando para errores críticos  
✅ **Tests >80% coverage**  
✅ **Performance** igual o mejor que antes  
✅ **Zero downtime** en deployment  

### Negocio
✅ **Alertas en tiempo real** a Teams  
✅ **Reducción de MTTR** (Mean Time To Resolution)  
✅ **Mejor visibilidad** de errores del sistema  
✅ **Escalabilidad** para futuro crecimiento  
✅ **Mantenibilidad** mejorada  

---

## 📞 EQUIPO Y ROLES

| Rol | Responsable | Fases |
|-----|-------------|-------|
| Tech Lead | TBD | Todas |
| Backend Dev 1 | TBD | Fases 1, 2, 3 |
| Backend Dev 2 | TBD | Fases 4, 5 |
| DevOps | TBD | Fases 8, 10 |
| QA Lead | TBD | Fase 7 |
| Product Owner | TBD | Aprobación y priorización |

---

## 📅 PRÓXIMOS PASOS INMEDIATOS

1. **HOY:**
   - [ ] Crear backup del código actual
   - [ ] Crear rama feature/refactor-microservices
   - [ ] Kickoff meeting con equipo

2. **MAÑANA:**
   - [ ] Iniciar Fase 1: Crear CarDealer.Contracts
   - [ ] Definir estructura de eventos
   - [ ] Crear primeros 5 eventos

3. **ESTA SEMANA:**
   - [ ] Completar CarDealer.Contracts
   - [ ] Refactorizar ErrorService
   - [ ] Implementar Teams alerts en NotificationService

---

## ✅ APROBACIONES

| Stakeholder | Fecha | Firma |
|-------------|-------|-------|
| Tech Lead | ___ / ___ / 2025 | _________ |
| Product Owner | ___ / ___ / 2025 | _________ |
| DevOps Lead | ___ / ___ / 2025 | _________ |
| QA Lead | ___ / ___ / 2025 | _________ |

---

**Versión:** 1.0  
**Última Actualización:** 28 Noviembre 2025  
**Autor:** Equipo de Arquitectura CarDealer  
**Estado:** ✅ LISTO PARA EJECUCIÓN
