# 🧪 PLAN DE TESTING - Sistema de Microservicios CarDealer

**Fecha:** 28 Noviembre 2025  
**Proyecto:** Refactorización Event-Driven Architecture  
**Objetivo:** Garantizar 100% funcionalidad durante y después de la refactorización  
**Cobertura Objetivo:** >80%

---

## 📋 ÍNDICE

1. [Estrategia de Testing](#estrategia-de-testing)
2. [Tipos de Tests](#tipos-de-tests)
3. [Herramientas y Frameworks](#herramientas-y-frameworks)
4. [Estructura de Tests](#estructura-de-tests)
5. [Plan por Fase](#plan-por-fase)
6. [Tests Críticos](#tests-críticos)
7. [Configuración de Entorno](#configuración-de-entorno)
8. [Métricas y Reportes](#métricas-y-reportes)

---

## 🎯 ESTRATEGIA DE TESTING

### Principios Fundamentales

1. **Test First Approach:** Crear tests antes de refactorizar
2. **Continuous Testing:** Tests ejecutados en cada commit
3. **Isolation:** Tests independientes entre sí
4. **Repeatability:** Mismos resultados en cada ejecución
5. **Fast Feedback:** Tests rápidos para desarrollo ágil

### Pirámide de Testing

```
           /\
          /  \  E2E Tests (10%)
         /----\  
        /      \ Integration Tests (30%)
       /--------\
      /          \ Unit Tests (60%)
     /____________\
```

**Distribución:**
- **60% Unit Tests:** Lógica de negocio, handlers, validators
- **30% Integration Tests:** Servicios + BD + RabbitMQ
- **10% E2E Tests:** Flujos completos user → response

---

## 🧪 TIPOS DE TESTS

### 1. Unit Tests

**Qué testean:**
- Lógica de negocio aislada
- Command/Query Handlers
- Validators
- Domain entities
- Mappers

**Características:**
- ✅ Rápidos (< 100ms por test)
- ✅ No requieren BD ni servicios externos
- ✅ Usan mocks/stubs
- ✅ Alta cobertura (>90%)

**Ejemplo:**
```csharp
[Fact]
public async Task RegisterUser_WithValidData_ShouldPublishEvent()
{
    // Arrange
    var mockPublisher = new Mock<IEventPublisher>();
    var handler = new RegisterUserCommandHandler(
        _userRepository, 
        mockPublisher.Object
    );

    // Act
    await handler.Handle(new RegisterUserCommand 
    { 
        Email = "test@test.com" 
    });

    // Assert
    mockPublisher.Verify(p => p.PublishAsync(
        It.IsAny<UserRegisteredEvent>(), 
        "auth.events", 
        "auth.user.registered"
    ), Times.Once);
}
```

---

### 2. Integration Tests

**Qué testean:**
- Interacción con PostgreSQL
- Publicación/Consumo de eventos RabbitMQ
- APIs endpoints
- Event handlers con dependencias reales

**Características:**
- ⚡ Más lentos (1-5s por test)
- 🐳 Usan Testcontainers (Docker)
- 🔄 Reset de BD entre tests
- 📊 Cobertura de flujos completos

**Ejemplo:**
```csharp
[Fact]
public async Task ErrorCritical_ShouldBeConsumedByNotificationService()
{
    // Arrange
    var @event = new ErrorCriticalEvent
    {
        ErrorId = Guid.NewGuid(),
        ServiceName = "VehicleService",
        StatusCode = 500
    };

    // Act
    await _eventPublisher.PublishAsync(@event, "error.events", "error.critical");
    await Task.Delay(3000); // Wait for consumer

    // Assert
    var teamsCalls = await _teamsProviderMock.GetCallsAsync();
    Assert.Single(teamsCalls);
    Assert.Contains("Error Crítico", teamsCalls[0].Title);
}
```

---

### 3. E2E Tests

**Qué testean:**
- Flujos completos de usuario
- Múltiples servicios interactuando
- Gateway → Service → Event → Consumer

**Características:**
- 🐌 Lentos (5-30s por test)
- 🌐 Requieren todos los servicios running
- 🎭 Simulan comportamiento real
- 🎯 Pocos pero críticos

**Ejemplo:**
```csharp
[Fact]
public async Task UserRegistration_ShouldTriggerWelcomeEmail()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new RegisterUserRequest
    {
        Email = "newuser@test.com",
        Password = "Test123!",
        FullName = "Test User"
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/auth/register", request);
    await Task.Delay(5000); // Wait for event processing

    // Assert
    response.EnsureSuccessStatusCode();
    
    var emails = await _emailRepository.GetByRecipientAsync("newuser@test.com");
    Assert.Single(emails);
    Assert.Contains("Bienvenido", emails[0].Subject);
    
    var auditLogs = await _auditRepository.GetByEventTypeAsync("auth.user.registered");
    Assert.NotEmpty(auditLogs);
}
```

---

### 4. Performance Tests

**Qué testean:**
- Throughput de RabbitMQ
- Latencia end-to-end
- Memory leaks
- Concurrencia

**Herramientas:**
- BenchmarkDotNet
- k6
- Apache JMeter

**Ejemplo:**
```csharp
[Fact]
public async Task RabbitMQ_ShouldHandle1000EventsPerSecond()
{
    // Arrange
    var events = Enumerable.Range(0, 1000)
        .Select(_ => new ErrorCriticalEvent { /* ... */ })
        .ToList();

    var stopwatch = Stopwatch.StartNew();

    // Act
    foreach (var @event in events)
    {
        await _publisher.PublishAsync(@event, "error.events", "error.critical");
    }

    stopwatch.Stop();

    // Assert
    Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
        $"Took {stopwatch.ElapsedMilliseconds}ms, expected <1000ms");
}
```

---

### 5. Contract Tests

**Qué testean:**
- Serialización/Deserialización de eventos
- Compatibilidad de versiones
- Schema validation

**Ejemplo:**
```csharp
[Fact]
public void ErrorCriticalEvent_ShouldSerializeCorrectly()
{
    // Arrange
    var @event = new ErrorCriticalEvent
    {
        ErrorId = Guid.NewGuid(),
        ServiceName = "VehicleService",
        StatusCode = 500
    };

    // Act
    var json = JsonSerializer.Serialize(@event);
    var deserialized = JsonSerializer.Deserialize<ErrorCriticalEvent>(json);

    // Assert
    Assert.NotNull(deserialized);
    Assert.Equal(@event.ErrorId, deserialized.ErrorId);
    Assert.Equal(@event.ServiceName, deserialized.ServiceName);
    Assert.Equal("error.critical", deserialized.EventType);
}
```

---

## 🛠️ HERRAMIENTAS Y FRAMEWORKS

### Testing Frameworks

| Herramienta | Uso | Versión |
|-------------|-----|---------|
| **xUnit** | Unit & Integration Tests | 2.6.0+ |
| **Moq** | Mocking framework | 4.20.0+ |
| **FluentAssertions** | Assertions legibles | 6.12.0+ |
| **Testcontainers** | Containers para testing | 3.6.0+ |
| **WireMock.Net** | Mock de APIs externas | 1.5.0+ |
| **BenchmarkDotNet** | Performance testing | 0.13.0+ |

### Packages Necesarios

```xml
<ItemGroup>
  <!-- Testing Framework -->
  <PackageReference Include="xunit" Version="2.6.0" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
  
  <!-- Mocking -->
  <PackageReference Include="Moq" Version="4.20.70" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  
  <!-- Integration Testing -->
  <PackageReference Include="Testcontainers" Version="3.6.0" />
  <PackageReference Include="Testcontainers.PostgreSql" Version="3.6.0" />
  <PackageReference Include="Testcontainers.RabbitMq" Version="3.6.0" />
  
  <!-- Web Testing -->
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
  
  <!-- Utilities -->
  <PackageReference Include="Bogus" Version="35.0.0" />
  <PackageReference Include="WireMock.Net" Version="1.5.45" />
</ItemGroup>
```

---

## 📁 ESTRUCTURA DE TESTS

### Organización de Directorios

```
backend/IntegrationTests/
├── IntegrationTests.csproj
├── GlobalUsings.cs
├── appsettings.Test.json
│
├── Fixtures/                      # Configuración compartida
│   ├── PostgresFixture.cs         # BD de tests
│   ├── RabbitMQFixture.cs         # Message broker de tests
│   ├── TestWebApplicationFactory.cs
│   └── TestDataSeeder.cs
│
├── Unit/                          # Tests unitarios
│   ├── AuthService/
│   │   ├── Handlers/
│   │   │   ├── RegisterUserHandlerTests.cs
│   │   │   └── LoginUserHandlerTests.cs
│   │   └── Validators/
│   │       └── RegisterUserValidatorTests.cs
│   ├── ErrorService/
│   ├── NotificationService/
│   └── VehicleService/
│
├── Integration/                   # Tests de integración
│   ├── EventFlow/
│   │   ├── ErrorCriticalEventTests.cs
│   │   ├── UserRegisteredEventTests.cs
│   │   └── VehicleCreatedEventTests.cs
│   ├── Database/
│   │   ├── ErrorLogRepositoryTests.cs
│   │   └── UserRepositoryTests.cs
│   └── Messaging/
│       ├── RabbitMQPublisherTests.cs
│       └── RabbitMQConsumerTests.cs
│
├── E2E/                           # Tests end-to-end
│   ├── UserJourneys/
│   │   ├── UserRegistrationFlowTests.cs
│   │   └── VehiclePurchaseFlowTests.cs
│   └── ErrorHandling/
│       └── ErrorToTeamsAlertFlowTests.cs
│
├── Performance/                   # Tests de rendimiento
│   ├── RabbitMQThroughputTests.cs
│   └── ConcurrencyTests.cs
│
├── Contract/                      # Tests de contratos
│   └── EventSerializationTests.cs
│
└── Mocks/                         # Mocks compartidos
    ├── MockTeamsProvider.cs
    ├── MockEmailProvider.cs
    └── MockSmsProvider.cs
```

---

## 📅 PLAN POR FASE

### FASE 0: Preparación (ACTUAL)

**Tests a Crear:**
- ✅ Estructura de directorios
- ✅ Configuración de Testcontainers
- ✅ Fixtures base
- ✅ Mocks de providers externos

**Archivos:**
```
✅ backend/IntegrationTests/
✅ TEST_PLAN.md (este documento)
⬜ IntegrationTests.csproj
⬜ Fixtures/PostgresFixture.cs
⬜ Fixtures/RabbitMQFixture.cs
⬜ Mocks/MockTeamsProvider.cs
```

**Entregable:**
- [ ] Proyecto IntegrationTests compilando
- [ ] Testcontainers funcionando
- [ ] 1 test de ejemplo pasando

---

### FASE 1: CarDealer.Contracts

**Tests a Crear:**
- [ ] Serialization/Deserialization de eventos
- [ ] Validación de EventType
- [ ] Validación de required properties

**Coverage Objetivo:** 100% (son solo DTOs)

**Archivos:**
```
Contract/
├── EventSerializationTests.cs
├── UserRegisteredEventTests.cs
├── ErrorCriticalEventTests.cs
└── VehicleCreatedEventTests.cs
```

**Test Example:**
```csharp
public class EventSerializationTests
{
    [Theory]
    [InlineData(typeof(UserRegisteredEvent))]
    [InlineData(typeof(ErrorCriticalEvent))]
    [InlineData(typeof(VehicleCreatedEvent))]
    public void AllEvents_ShouldSerializeAndDeserialize(Type eventType)
    {
        // Arrange
        var instance = Activator.CreateInstance(eventType);
        
        // Act
        var json = JsonSerializer.Serialize(instance);
        var deserialized = JsonSerializer.Deserialize(json, eventType);
        
        // Assert
        deserialized.Should().NotBeNull();
    }
}
```

---

### FASE 2: ErrorService

**Tests a Crear:**

**Unit Tests:**
- [ ] LogErrorCommandHandler
- [ ] LogErrorValidator
- [ ] ErrorLog entity

**Integration Tests:**
- [ ] ErrorLog persistencia en BD
- [ ] Publicación de ErrorCriticalEvent
- [ ] Consumer de errores de otros servicios

**Coverage Objetivo:** >85%

**Archivos:**
```
Unit/ErrorService/
├── Handlers/LogErrorHandlerTests.cs
├── Validators/LogErrorValidatorTests.cs
└── Entities/ErrorLogTests.cs

Integration/ErrorService/
├── ErrorLogRepositoryTests.cs
├── ErrorCriticalEventPublisherTests.cs
└── ErrorEventConsumerTests.cs
```

---

### FASE 3: NotificationService + Teams Alerts

**Tests a Crear:**

**Unit Tests:**
- [ ] TeamsProvider
- [ ] SendTeamsNotificationHandler
- [ ] SendTeamsNotificationValidator

**Integration Tests:**
- [ ] ErrorCriticalEventConsumer → Teams Alert ⭐
- [ ] UserRegisteredEventConsumer → Welcome Email
- [ ] Teams API mock (WireMock)

**E2E Tests:**
- [ ] Error crítico → Teams alert enviada ⭐

**Coverage Objetivo:** >80%

**Archivos:**
```
Unit/NotificationService/
├── Providers/TeamsProviderTests.cs
├── Handlers/SendTeamsNotificationHandlerTests.cs
└── Consumers/ErrorCriticalEventConsumerTests.cs

Integration/NotificationService/
├── TeamsAlertIntegrationTests.cs
└── EventConsumersIntegrationTests.cs

E2E/ErrorHandling/
└── ErrorToTeamsAlertFlowTests.cs
```

**Test Crítico:**
```csharp
[Fact]
public async Task ErrorCritical_ShouldSendTeamsAlert_E2E()
{
    // Arrange
    var errorEvent = new ErrorCriticalEvent
    {
        ErrorId = Guid.NewGuid(),
        ServiceName = "VehicleService",
        Message = "Database connection failed",
        StatusCode = 500,
        ExceptionType = "SqlException"
    };

    // Act
    await _errorService.LogErrorAsync(errorEvent);
    await Task.Delay(5000); // Wait for event propagation

    // Assert
    var teamsCalls = _teamsMock.GetCalls();
    teamsCalls.Should().ContainSingle();
    
    var call = teamsCalls.First();
    call.Title.Should().Contain("Error Crítico");
    call.Title.Should().Contain("VehicleService");
    call.Facts["Código HTTP"].Should().Be("500");
    call.Facts["Tipo"].Should().Be("SqlException");
}
```

---

### FASE 4: AuthService

**Tests a Crear:**
- [ ] RegisterUserHandler con event publishing
- [ ] LoginUserHandler con event publishing
- [ ] Excepciones propias (UnauthorizedException, etc.)

**Coverage Objetivo:** >85%

---

### FASE 5: VehicleService + MediaService

**Tests a Crear:**
- [ ] CRUD handlers con event publishing
- [ ] Event consumers
- [ ] Repository tests

**Coverage Objetivo:** >80%

---

### FASE 6: AuditService

**Tests a Crear:**
- [ ] UniversalEventConsumer (todos los eventos)
- [ ] Persistencia de auditoría
- [ ] Query de auditoría

**Coverage Objetivo:** >75%

---

### FASE 7: Testing E2E Completo

**Flujos Críticos:**

1. **User Registration Flow**
   ```
   POST /api/auth/register
   → UserRegisteredEvent published
   → Welcome email sent
   → Audit logged
   ```

2. **Critical Error Flow** ⭐
   ```
   Error occurs in VehicleService
   → ErrorCriticalEvent published
   → ErrorService logs it
   → NotificationService sends Teams alert
   → Audit logged
   ```

3. **Vehicle Purchase Flow**
   ```
   POST /api/vehicles/purchase
   → VehicleSoldEvent published
   → Confirmation email sent
   → Invoice generated
   → Audit logged
   ```

**Tests de Resiliencia:**
- [ ] RabbitMQ down → Retry logic
- [ ] Service down → Circuit breaker
- [ ] Mensaje malformado → Dead Letter Queue
- [ ] Concurrencia → No duplicados

---

## 🎯 TESTS CRÍTICOS (MUST PASS)

### Top 10 Tests Más Importantes

1. ✅ **ErrorCritical_ToTeamsAlert_E2E**
   - Error crítico genera alerta en Teams
   - **Prioridad:** CRÍTICA

2. ✅ **UserRegistered_SendsWelcomeEmail**
   - Registro de usuario envía email
   - **Prioridad:** ALTA

3. ✅ **AllEvents_SerializeCorrectly**
   - Todos los eventos se serializan
   - **Prioridad:** CRÍTICA

4. ✅ **RabbitMQ_HandlesHighThroughput**
   - 1000 eventos/seg sin pérdida
   - **Prioridad:** ALTA

5. ✅ **EventPublisher_RetriesOnFailure**
   - Retry automático en fallas
   - **Prioridad:** ALTA

6. ✅ **Consumer_SendsToDeadLetterQueue_OnError**
   - Mensajes malformados a DLQ
   - **Prioridad:** ALTA

7. ✅ **CircularDependencies_DoNotExist**
   - No hay referencias cruzadas
   - **Prioridad:** CRÍTICA

8. ✅ **AuditService_LogsAllEvents**
   - Todos los eventos auditados
   - **Prioridad:** MEDIA

9. ✅ **TeamsProvider_HandlesRateLimiting**
   - Rate limiting de Teams API
   - **Prioridad:** MEDIA

10. ✅ **EndToEnd_CompletesUnder10Seconds**
    - Flujos E2E < 10s
    - **Prioridad:** BAJA

---

## 🐳 CONFIGURACIÓN DE ENTORNO

### Testcontainers Setup

**PostgresFixture.cs:**
```csharp
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("cardealer_test")
        .WithUsername("test")
        .WithPassword("test123")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        // Run migrations
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await FluentMigrator.Runner.MigrationRunner.MigrateUp(connection);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
```

**RabbitMQFixture.cs:**
```csharp
public class RabbitMQFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        // Configure exchanges and queues
        var factory = new ConnectionFactory { Uri = new Uri(ConnectionString) };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        
        channel.ExchangeDeclare("error.events", "topic", durable: true);
        channel.ExchangeDeclare("auth.events", "topic", durable: true);
        // ... more exchanges
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
```

### Test Configuration

**appsettings.Test.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=cardealer_test;Username=test;Password=test123"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  },
  "NotificationSettings": {
    "Teams": {
      "Enabled": true,
      "CriticalAlertsWebhook": "http://localhost:8080/teams/webhook"
    }
  }
}
```

---

## 📊 MÉTRICAS Y REPORTES

### Métricas a Monitorear

| Métrica | Objetivo | Crítico si |
|---------|----------|------------|
| **Code Coverage** | >80% | <70% |
| **Test Pass Rate** | 100% | <95% |
| **Execution Time** | <5 min | >10 min |
| **Flaky Tests** | 0% | >5% |
| **E2E Success** | 100% | <90% |

### Reporte de Cobertura

```powershell
# Generar reporte de cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generar HTML report
reportgenerator -reports:"coverage.opencover.xml" -targetdir:"coveragereport"
```

### CI/CD Integration

**GitHub Actions (.github/workflows/tests.yml):**
```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run Unit Tests
      run: dotnet test --filter Category=Unit --no-build --verbosity normal
    
    - name: Run Integration Tests
      run: dotnet test --filter Category=Integration --no-build --verbosity normal
    
    - name: Generate Coverage Report
      run: dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
    
    - name: Upload Coverage
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage.opencover.xml
```

---

## 🎓 BEST PRACTICES

### Naming Conventions

```csharp
// ✅ GOOD
[Fact]
public async Task RegisterUser_WithValidEmail_ShouldPublishUserRegisteredEvent()

// ❌ BAD
[Fact]
public async Task Test1()
```

### AAA Pattern (Arrange-Act-Assert)

```csharp
[Fact]
public async Task Example()
{
    // Arrange - Setup
    var command = new RegisterUserCommand { Email = "test@test.com" };
    
    // Act - Execute
    var result = await _handler.Handle(command);
    
    // Assert - Verify
    result.Should().NotBeNull();
}
```

### Test Data Builders

```csharp
public class UserBuilder
{
    private string _email = "test@test.com";
    private string _name = "Test User";
    
    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }
    
    public User Build() => new User { Email = _email, FullName = _name };
}

// Usage
var user = new UserBuilder()
    .WithEmail("custom@test.com")
    .Build();
```

---

## ✅ CHECKLIST DE TESTING

### Antes de Cada Fase
- [ ] Tests escritos ANTES de código
- [ ] Fixtures configurados
- [ ] Mocks preparados
- [ ] Testcontainers running

### Durante Desarrollo
- [ ] Tests ejecutados en cada commit
- [ ] Coverage monitoreado
- [ ] Flaky tests corregidos inmediatamente
- [ ] Red-Green-Refactor seguido

### Antes de PR
- [ ] Todos los tests pasando
- [ ] Coverage >80%
- [ ] 0 tests ignorados
- [ ] Performance tests OK

### Antes de Producción
- [ ] E2E tests pasando
- [ ] Load tests exitosos
- [ ] Smoke tests en staging
- [ ] Rollback plan probado

---

## 📞 CONTACTO Y SOPORTE

**QA Lead:** TBD  
**Test Framework Owner:** TBD  
**CI/CD Owner:** TBD

---

**Versión:** 1.0  
**Última Actualización:** 28 Noviembre 2025  
**Estado:** ✅ PLANIFICACIÓN COMPLETADA
