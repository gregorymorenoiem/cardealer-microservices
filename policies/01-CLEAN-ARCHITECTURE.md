# POLÍTICA 01: ARQUITECTURA Y ESTRUCTURA - CLEAN ARCHITECTURE

**Versión**: 1.0  
**Última Actualización**: 2025-11-30  
**Estado**: OBLIGATORIO ✅  
**Responsable**: Equipo de Arquitectura CarDealer

---

## 📋 RESUMEN EJECUTIVO

**POLÍTICA CRÍTICA**: Todos los microservicios deben implementar Clean Architecture con exactamente 6 capas. No se permiten desviaciones de esta estructura.

**Objetivo**: Garantizar separación de responsabilidades, testabilidad, mantenibilidad y escalabilidad en todos los microservicios del ecosistema CarDealer.

**Alcance**: Aplica a TODOS los microservicios nuevos y existentes.

---

## 🎯 ESTRUCTURA OBLIGATORIA

### Jerarquía de Capas (6 Capas Mandatorias)

```
{ServiceName}/
├── {ServiceName}.sln                          # Solution file
├── {ServiceName}.Api/                         # CAPA 1: API Layer
│   ├── Controllers/                           # REST Controllers
│   ├── Middleware/                            # Custom middleware
│   ├── Extensions/                            # Service extensions
│   ├── appsettings.json                       # Configuración base
│   ├── appsettings.Development.json           # Config desarrollo
│   ├── appsettings.Production.json            # Config producción
│   └── Program.cs                             # Entry point
│
├── {ServiceName}.Application/                 # CAPA 2: Application Layer
│   ├── Commands/                              # CQRS Commands
│   │   ├── {Feature}/
│   │   │   ├── {Action}Command.cs             # Command DTO
│   │   │   ├── {Action}CommandHandler.cs      # Command Handler
│   │   │   └── {Action}CommandValidator.cs    # FluentValidation
│   ├── Queries/                               # CQRS Queries
│   │   ├── {Feature}/
│   │   │   ├── {Action}Query.cs               # Query DTO
│   │   │   └── {Action}QueryHandler.cs        # Query Handler
│   ├── DTOs/                                  # Data Transfer Objects
│   ├── Behaviors/                             # MediatR Pipeline Behaviors
│   │   ├── ValidationBehavior.cs              # Validación automática
│   │   └── LoggingBehavior.cs                 # Logging automático
│   └── Interfaces/                            # Application interfaces
│
├── {ServiceName}.Domain/                      # CAPA 3: Domain Layer
│   ├── Entities/                              # Domain entities (agregados)
│   ├── ValueObjects/                          # Value objects inmutables
│   ├── Events/                                # Domain events
│   ├── Interfaces/                            # Repository interfaces
│   └── Exceptions/                            # Domain exceptions
│
├── {ServiceName}.Infrastructure/              # CAPA 4: Infrastructure Layer
│   ├── Persistence/                           # Acceso a datos
│   │   ├── Repositories/                      # Implementación de repositorios
│   │   ├── Configurations/                    # EF Core configurations
│   │   ├── Migrations/                        # EF Core migrations
│   │   └── ApplicationDbContext.cs            # DbContext
│   ├── EventPublisher/                        # RabbitMQ publisher
│   │   ├── RabbitMqEventPublisher.cs          # Implementación
│   │   └── RabbitMqSettings.cs                # Configuración
│   ├── ExternalServices/                      # Servicios externos (APIs)
│   └── BackgroundServices/                    # Workers/Processors
│
├── {ServiceName}.Shared/                      # CAPA 5: Shared Layer
│   ├── Events/                                # Event definitions (contratos)
│   ├── Constants/                             # Constantes compartidas
│   ├── Extensions/                            # Extension methods
│   └── Helpers/                               # Utility helpers
│
└── {ServiceName}.Tests/                       # CAPA 6: Testing Layer
    ├── Unit/                                  # Unit tests (80% coverage)
    │   ├── Controllers/
    │   ├── Handlers/
    │   └── Validators/
    ├── Integration/                           # Integration tests (60% coverage)
    │   ├── Api/
    │   └── Factories/
    │       └── CustomWebApplicationFactory.cs
    └── E2E/                                   # E2E tests (40% coverage)
        └── Scripts/
            └── E2E-TESTING-SCRIPT.ps1
```

---

## 🔒 REGLAS DE DEPENDENCIA (Dependency Rule)

### Principio Fundamental
**Las dependencias SOLO pueden apuntar hacia adentro (hacia el Domain).**

```
┌──────────────────────────────────────────────────┐
│  API Layer                                       │
│  ├─ Controllers                                  │
│  └─ Program.cs                                   │
└────────────┬─────────────────────────────────────┘
             │ Depende de ↓
┌────────────▼─────────────────────────────────────┐
│  Application Layer                               │
│  ├─ Commands/Queries                             │
│  └─ Behaviors                                    │
└────────────┬─────────────────────────────────────┘
             │ Depende de ↓
┌────────────▼─────────────────────────────────────┐
│  Domain Layer (CORE - Sin dependencias externas) │
│  ├─ Entities                                     │
│  ├─ ValueObjects                                 │
│  └─ Interfaces                                   │
└────────────▲─────────────────────────────────────┘
             │ Implementado por ↑
┌────────────┴─────────────────────────────────────┐
│  Infrastructure Layer                            │
│  ├─ Repositories                                 │
│  ├─ DbContext                                    │
│  └─ EventPublisher                               │
└──────────────────────────────────────────────────┘
```

### ✅ PERMITIDO
```csharp
// ✅ CORRECTO: API → Application
namespace ErrorService.Api.Controllers
{
    public class ErrorsController : ControllerBase
    {
        private readonly IMediator _mediator;  // Application layer
        
        public async Task<IActionResult> LogError([FromBody] LogErrorCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}

// ✅ CORRECTO: Application → Domain
namespace ErrorService.Application.Commands.LogError
{
    public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, Guid>
    {
        private readonly IErrorLogRepository _repository;  // Domain interface
        
        public async Task<Guid> Handle(LogErrorCommand request, CancellationToken ct)
        {
            var errorLog = new ErrorLog(/* ... */);  // Domain entity
            await _repository.AddAsync(errorLog, ct);
            return errorLog.Id;
        }
    }
}

// ✅ CORRECTO: Infrastructure → Domain (implementación de interface)
namespace ErrorService.Infrastructure.Persistence.Repositories
{
    public class EfErrorLogRepository : IErrorLogRepository  // Domain interface
    {
        private readonly ApplicationDbContext _context;
        
        public async Task<ErrorLog> AddAsync(ErrorLog errorLog, CancellationToken ct)
        {
            await _context.ErrorLogs.AddAsync(errorLog, ct);
            await _context.SaveChangesAsync(ct);
            return errorLog;
        }
    }
}
```

### ❌ PROHIBIDO
```csharp
// ❌ PROHIBIDO: Domain → Infrastructure
namespace ErrorService.Domain.Entities
{
    public class ErrorLog
    {
        // ❌ ERROR: Domain no puede depender de EF Core
        [Key]  // Esto es de Microsoft.EntityFrameworkCore
        public Guid Id { get; set; }
        
        // ❌ ERROR: Domain no puede depender de Infrastructure
        private readonly ApplicationDbContext _context;
    }
}

// ❌ PROHIBIDO: Domain → Application
namespace ErrorService.Domain.Entities
{
    public class ErrorLog
    {
        public void Validate()
        {
            // ❌ ERROR: Domain no puede usar FluentValidation
            var validator = new LogErrorCommandValidator();
        }
    }
}

// ❌ PROHIBIDO: Application → Infrastructure (directo)
namespace ErrorService.Application.Commands.LogError
{
    public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, Guid>
    {
        // ❌ ERROR: Application no puede depender directamente de Infrastructure
        private readonly ApplicationDbContext _dbContext;
        private readonly RabbitMqEventPublisher _publisher;
        
        // ✅ CORRECTO: Usar interfaces del Domain
        private readonly IErrorLogRepository _repository;
        private readonly IEventPublisher _eventPublisher;
    }
}
```

---

## 📦 RESPONSABILIDADES POR CAPA

### CAPA 1: API Layer ({ServiceName}.Api)

**Responsabilidad**: Exponer endpoints HTTP, manejo de requests/responses, configuración de la aplicación.

**Contiene**:
- ✅ Controllers (REST endpoints)
- ✅ Middleware (autenticación, error handling, rate limiting)
- ✅ Program.cs (configuración DI, pipeline)
- ✅ appsettings.json (configuración por ambiente)
- ✅ Swagger/OpenAPI configuration

**NO contiene**:
- ❌ Lógica de negocio
- ❌ Acceso directo a base de datos
- ❌ Validaciones complejas

**Ejemplo**:
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ErrorServiceAccess")]
public class ErrorsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public ErrorsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [RateLimit(maxRequests: 100, windowSeconds: 60)]
    public async Task<ActionResult<Guid>> LogError([FromBody] LogErrorCommand command)
    {
        var errorId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = errorId }, errorId);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ErrorLog>> GetById(Guid id)
    {
        var query = new GetErrorByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return result != null ? Ok(result) : NotFound();
    }
}
```

---

### CAPA 2: Application Layer ({ServiceName}.Application)

**Responsabilidad**: Orquestar casos de uso (use cases), implementar CQRS, validaciones, lógica de aplicación.

**Contiene**:
- ✅ Commands (operaciones de escritura)
- ✅ Queries (operaciones de lectura)
- ✅ Handlers (MediatR)
- ✅ Validators (FluentValidation)
- ✅ DTOs (contratos de entrada/salida)
- ✅ Behaviors (logging, validation, transactions)

**NO contiene**:
- ❌ Implementaciones de repositorios
- ❌ Configuración de EF Core
- ❌ Lógica de dominio compleja

**Patrón CQRS Obligatorio**:
```csharp
// COMMAND (Escritura)
public class LogErrorCommand : IRequest<Guid>
{
    public string ServiceName { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public int StatusCode { get; set; }
}

// COMMAND VALIDATOR
public class LogErrorCommandValidator : AbstractValidator<LogErrorCommand>
{
    public LogErrorCommandValidator()
    {
        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9\-_\.]+$");
        
        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(5000)
            .Must(NotContainSqlInjection)
            .WithMessage("Potential SQL injection detected");
        
        RuleFor(x => x.StatusCode)
            .InclusiveBetween(100, 599);
    }
    
    private bool NotContainSqlInjection(string input)
    {
        var sqlPatterns = new[] { "';--", "' OR '", "UNION SELECT", "DROP TABLE" };
        return !sqlPatterns.Any(p => input.Contains(p, StringComparison.OrdinalIgnoreCase));
    }
}

// COMMAND HANDLER
public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, Guid>
{
    private readonly IErrorLogRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<LogErrorCommandHandler> _logger;
    
    public LogErrorCommandHandler(
        IErrorLogRepository repository,
        IEventPublisher eventPublisher,
        ILogger<LogErrorCommandHandler> logger)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }
    
    public async Task<Guid> Handle(LogErrorCommand request, CancellationToken ct)
    {
        // 1. Crear entidad de dominio
        var errorLog = new ErrorLog
        {
            Id = Guid.NewGuid(),
            ServiceName = request.ServiceName,
            ExceptionType = request.ExceptionType,
            Message = request.Message,
            StackTrace = request.StackTrace,
            StatusCode = request.StatusCode,
            OccurredAt = DateTime.UtcNow
        };
        
        // 2. Persistir
        await _repository.AddAsync(errorLog, ct);
        
        // 3. Publicar evento si es crítico
        if (request.StatusCode >= 500)
        {
            await _eventPublisher.PublishAsync(new ErrorCriticalEvent
            {
                ErrorId = errorLog.Id,
                ServiceName = errorLog.ServiceName,
                Message = errorLog.Message,
                OccurredAt = errorLog.OccurredAt
            }, ct);
        }
        
        _logger.LogInformation("Error logged: {ErrorId}", errorLog.Id);
        return errorLog.Id;
    }
}

// QUERY (Lectura)
public class GetErrorByIdQuery : IRequest<ErrorLog?>
{
    public Guid Id { get; set; }
}

// QUERY HANDLER
public class GetErrorByIdQueryHandler : IRequestHandler<GetErrorByIdQuery, ErrorLog?>
{
    private readonly IErrorLogRepository _repository;
    
    public GetErrorByIdQueryHandler(IErrorLogRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<ErrorLog?> Handle(GetErrorByIdQuery request, CancellationToken ct)
    {
        return await _repository.GetByIdAsync(request.Id, ct);
    }
}
```

**Behaviors Obligatorios**:
```csharp
// ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, ct)));
            
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();
            
            if (failures.Any())
                throw new ValidationException(failures);
        }
        
        return await next();
    }
}
```

---

### CAPA 3: Domain Layer ({ServiceName}.Domain)

**Responsabilidad**: Contener la lógica de negocio pura, entidades, value objects, reglas de dominio. **NO tiene dependencias externas**.

**Contiene**:
- ✅ Entities (agregados raíz)
- ✅ Value Objects (inmutables)
- ✅ Domain Events
- ✅ Interfaces de repositorios (contratos)
- ✅ Domain Exceptions
- ✅ Business rules

**NO contiene**:
- ❌ Referencias a EF Core
- ❌ Referencias a bibliotecas de infraestructura
- ❌ Lógica de persistencia
- ❌ Atributos de mapeo ([Key], [Column], etc.)

**Ejemplo**:
```csharp
// Entity (Agregado raíz)
namespace ErrorService.Domain.Entities
{
    public class ErrorLog
    {
        // ✅ CORRECTO: Solo propiedades, sin atributos EF Core
        public Guid Id { get; private set; }
        public string ServiceName { get; private set; }
        public string ExceptionType { get; private set; }
        public string Message { get; private set; }
        public string? StackTrace { get; private set; }
        public int StatusCode { get; private set; }
        public DateTime OccurredAt { get; private set; }
        
        // Constructor privado (factory pattern)
        private ErrorLog() { }
        
        // Factory method con validaciones de dominio
        public static ErrorLog Create(
            string serviceName,
            string exceptionType,
            string message,
            int statusCode,
            string? stackTrace = null)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new DomainException("ServiceName is required");
            
            if (statusCode < 100 || statusCode > 599)
                throw new DomainException("Invalid HTTP status code");
            
            return new ErrorLog
            {
                Id = Guid.NewGuid(),
                ServiceName = serviceName,
                ExceptionType = exceptionType,
                Message = message,
                StatusCode = statusCode,
                StackTrace = stackTrace,
                OccurredAt = DateTime.UtcNow
            };
        }
        
        // Métodos de dominio (business logic)
        public bool IsCritical() => StatusCode >= 500;
        
        public void UpdateMessage(string newMessage)
        {
            if (string.IsNullOrWhiteSpace(newMessage))
                throw new DomainException("Message cannot be empty");
            
            Message = newMessage;
        }
    }
}

// Value Object (inmutable)
namespace ErrorService.Domain.ValueObjects
{
    public sealed class ErrorSeverity : IEquatable<ErrorSeverity>
    {
        public string Level { get; }
        public int Priority { get; }
        
        private ErrorSeverity(string level, int priority)
        {
            Level = level;
            Priority = priority;
        }
        
        public static ErrorSeverity Critical = new("Critical", 1);
        public static ErrorSeverity High = new("High", 2);
        public static ErrorSeverity Medium = new("Medium", 3);
        public static ErrorSeverity Low = new("Low", 4);
        
        public bool Equals(ErrorSeverity? other)
        {
            if (other is null) return false;
            return Level == other.Level && Priority == other.Priority;
        }
        
        public override bool Equals(object? obj) => Equals(obj as ErrorSeverity);
        public override int GetHashCode() => HashCode.Combine(Level, Priority);
    }
}

// Interface (Repository contract)
namespace ErrorService.Domain.Interfaces
{
    public interface IErrorLogRepository
    {
        Task<ErrorLog> AddAsync(ErrorLog errorLog, CancellationToken ct);
        Task<ErrorLog?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<ErrorLog>> GetAllAsync(
            string? serviceName,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    }
}

// Domain Exception
namespace ErrorService.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
        
        public DomainException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
```

---

### CAPA 4: Infrastructure Layer ({ServiceName}.Infrastructure)

**Responsabilidad**: Implementar interfaces del dominio, acceso a datos, servicios externos, RabbitMQ, workers.

**Contiene**:
- ✅ Implementaciones de repositorios
- ✅ DbContext (EF Core)
- ✅ Configurations (Fluent API)
- ✅ Migrations
- ✅ RabbitMQ EventPublisher
- ✅ Background Services
- ✅ External API clients

**NO contiene**:
- ❌ Lógica de negocio
- ❌ Validaciones de negocio

**Ejemplo**:
```csharp
// Repository Implementation
namespace ErrorService.Infrastructure.Persistence.Repositories
{
    public class EfErrorLogRepository : IErrorLogRepository
    {
        private readonly ApplicationDbContext _context;
        
        public EfErrorLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<ErrorLog> AddAsync(ErrorLog errorLog, CancellationToken ct)
        {
            await _context.ErrorLogs.AddAsync(errorLog, ct);
            await _context.SaveChangesAsync(ct);
            return errorLog;
        }
        
        public async Task<ErrorLog?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _context.ErrorLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, ct);
        }
        
        public async Task<IEnumerable<ErrorLog>> GetAllAsync(
            string? serviceName,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken ct)
        {
            var query = _context.ErrorLogs.AsNoTracking();
            
            if (!string.IsNullOrEmpty(serviceName))
                query = query.Where(e => e.ServiceName == serviceName);
            
            if (startDate.HasValue)
                query = query.Where(e => e.OccurredAt >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(e => e.OccurredAt <= endDate.Value);
            
            return await query.OrderByDescending(e => e.OccurredAt).ToListAsync(ct);
        }
    }
}

// DbContext
namespace ErrorService.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}

// Entity Configuration (Fluent API)
namespace ErrorService.Infrastructure.Persistence.Configurations
{
    public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
    {
        public void Configure(EntityTypeBuilder<ErrorLog> builder)
        {
            builder.ToTable("error_logs");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.ServiceName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(5000);
            
            builder.Property(e => e.StackTrace)
                .HasMaxLength(50000);
            
            // Índices
            builder.HasIndex(e => e.ServiceName);
            builder.HasIndex(e => e.OccurredAt);
            builder.HasIndex(e => new { e.ServiceName, e.OccurredAt });
        }
    }
}

// RabbitMQ EventPublisher
namespace ErrorService.Infrastructure.EventPublisher
{
    public class RabbitMqEventPublisher : IEventPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ResiliencePipeline _circuitBreaker;
        
        public RabbitMqEventPublisher(IConfiguration config, ILogger<RabbitMqEventPublisher> logger)
        {
            var factory = new ConnectionFactory
            {
                HostName = config["RabbitMQ:Host"],
                Port = int.Parse(config["RabbitMQ:Port"] ?? "5672"),
                UserName = config["RabbitMQ:Username"],
                Password = config["RabbitMQ:Password"]
            };
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            // Circuit Breaker con Polly
            _circuitBreaker = new ResiliencePipelineBuilder()
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 3,
                    BreakDuration = TimeSpan.FromSeconds(30)
                })
                .Build();
        }
        
        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct)
            where TEvent : IEvent
        {
            try
            {
                await _circuitBreaker.ExecuteAsync(async ct =>
                {
                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
                    _channel.BasicPublish(
                        exchange: "cardealer.events",
                        routingKey: @event.EventType,
                        basicProperties: null,
                        body: body);
                    return ValueTask.CompletedTask;
                }, ct);
            }
            catch (BrokenCircuitException)
            {
                // Graceful degradation: log pero no fallar
                logger.LogWarning("Circuit OPEN: Event not published");
            }
        }
        
        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
```

---

### CAPA 5: Shared Layer ({ServiceName}.Shared)

**Responsabilidad**: Código compartido entre microservicios (contratos de eventos, constantes, helpers).

**Contiene**:
- ✅ Event definitions (IEvent implementations)
- ✅ Constantes globales
- ✅ Extension methods
- ✅ Utility helpers

**Ejemplo**:
```csharp
// Event Definition
namespace ErrorService.Shared.Events
{
    public class ErrorCriticalEvent : IEvent
    {
        public string EventType => "error.critical";
        public Guid ErrorId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
    }
}

// Constants
namespace ErrorService.Shared.Constants
{
    public static class ErrorServiceConstants
    {
        public const int MaxMessageLength = 5000;
        public const int MaxStackTraceLength = 50000;
        public const string ExchangeName = "cardealer.events";
    }
}
```

---

### CAPA 6: Testing Layer ({ServiceName}.Tests)

**Responsabilidad**: Tests automatizados (Unit, Integration, E2E).

**Estructura obligatoria**:
```
{ServiceName}.Tests/
├── Unit/                                      # >= 80% cobertura
│   ├── Controllers/
│   │   └── ErrorsControllerTests.cs
│   ├── Handlers/
│   │   └── LogErrorCommandHandlerTests.cs
│   └── Validators/
│       └── LogErrorCommandValidatorTests.cs
├── Integration/                               # >= 60% cobertura
│   ├── Api/
│   │   └── ErrorsEndpointTests.cs
│   └── Factories/
│       └── CustomWebApplicationFactory.cs
└── E2E/                                       # >= 40% cobertura
    └── Scripts/
        └── E2E-TESTING-SCRIPT.ps1
```

---

## ✅ VALIDACIÓN DE CUMPLIMIENTO

### Checklist Obligatorio

- [ ] Estructura de 6 capas implementada correctamente
- [ ] Dependency Rule respetada (dependencias apuntan hacia Domain)
- [ ] Domain layer sin dependencias externas (0 referencias a NuGet packages externos)
- [ ] CQRS implementado con MediatR
- [ ] FluentValidation en todos los Commands
- [ ] Repository Pattern con interfaces en Domain
- [ ] EF Core Fluent API en Infrastructure (no atributos en Domain)
- [ ] Circuit Breaker en servicios externos
- [ ] CustomWebApplicationFactory para integration tests

---

## 🚫 ANTI-PATRONES COMUNES

### ❌ Domain con dependencias externas
```csharp
// ❌ INCORRECTO
namespace ErrorService.Domain.Entities
{
    [Table("error_logs")]  // ❌ Atributo de EF Core
    public class ErrorLog
    {
        [Key]  // ❌ Atributo de EF Core
        public Guid Id { get; set; }
    }
}
```

### ❌ Lógica de negocio en Controllers
```csharp
// ❌ INCORRECTO
[HttpPost]
public async Task<IActionResult> LogError([FromBody] LogErrorDto dto)
{
    // ❌ Lógica de negocio en controller
    if (dto.StatusCode >= 500)
    {
        // Enviar a RabbitMQ
        await _rabbitMq.Publish(new ErrorEvent { ... });
    }
    
    // ❌ Acceso directo a DbContext
    _dbContext.ErrorLogs.Add(new ErrorLog { ... });
    await _dbContext.SaveChangesAsync();
    
    return Ok();
}
```

### ❌ Application layer accediendo a DbContext directamente
```csharp
// ❌ INCORRECTO
public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, Guid>
{
    private readonly ApplicationDbContext _dbContext;  // ❌ Violación de Dependency Rule
    
    public async Task<Guid> Handle(LogErrorCommand request, CancellationToken ct)
    {
        var errorLog = new ErrorLog { ... };
        _dbContext.ErrorLogs.Add(errorLog);  // ❌ Debería usar IErrorLogRepository
        await _dbContext.SaveChangesAsync(ct);
        return errorLog.Id;
    }
}
```

---

## 📚 RECURSOS Y REFERENCIAS

- **Microservicio de Referencia**: `ErrorService` (implementación completa)
- **Documentación Clean Architecture**: [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- **CQRS Pattern**: [MediatR Wiki](https://github.com/jbogard/MediatR/wiki)

---

## 🎯 PRÓXIMOS PASOS

1. Revisar ErrorService como implementación de referencia
2. Crear scaffold de nuevo microservicio usando esta estructura
3. Configurar solution con las 6 capas
4. Implementar CQRS desde el inicio
5. Aplicar Dependency Rule en cada commit

---

**Fecha de Vigencia**: 2025-11-30  
**Aprobado por**: Equipo de Arquitectura CarDealer  
**Revisión**: Trimestral

**NOTA**: Esta política es OBLIGATORIA y no negociable. Desviaciones requieren aprobación escrita del Arquitecto de Software.
