# POLÍTICA 05: REAL INFRASTRUCTURE TESTING - PRUEBAS CON INFRAESTRUCTURA REAL

**Versión**: 1.0  
**Última Actualización**: 2025-11-30  
**Estado**: OBLIGATORIO ✅  
**Responsable**: Equipo de Arquitectura CarDealer

---

## 📋 RESUMEN EJECUTIVO

**POLÍTICA CRÍTICA**: Antes de merge a `main`, TODOS los microservicios deben validarse contra infraestructura real (PostgreSQL, RabbitMQ, Redis) usando docker-compose. Integration tests con InMemory NO son suficientes.

**Objetivo**: Garantizar que el microservicio funciona correctamente con dependencias reales antes de deployment a producción.

**Alcance**: Aplica a TODOS los microservicios del ecosistema CarDealer.

---

## 🎯 DIFERENCIA: INMEMORY VS REAL INFRASTRUCTURE

### Limitaciones de InMemory Testing

| Aspecto | InMemory Database | Real Database |
|---------|-------------------|---------------|
| **Constraints** | Parcial | ✅ Completo (FK, Unique, Check) |
| **Transactions** | Simulado | ✅ ACID real |
| **Concurrency** | No detecta race conditions | ✅ Locks, deadlocks reales |
| **Performance** | No realista | ✅ Query performance real |
| **SQL Dialect** | Genérico | ✅ PostgreSQL/SQL Server específico |
| **Triggers** | ❌ No soportado | ✅ Soportado |
| **Stored Procedures** | ❌ No soportado | ✅ Soportado |

**REGLA**: InMemory es bueno para CI/CD rápido, pero SIEMPRE validar contra infraestructura real antes de merge a `main`.

---

## 🐳 DOCKER-COMPOSE PARA TESTING (OBLIGATORIO)

### Estructura de Archivos

```
{ServiceName}/
├── docker-compose.yml              # Producción
├── docker-compose.debug.yml        # Development con debugging
└── docker-compose.test.yml         # ✅ TESTING con infraestructura real
```

---

### Template: docker-compose.test.yml

```yaml
# docker-compose.test.yml
# Infraestructura REAL para testing local antes de merge a main
version: '3.8'

services:
  # PostgreSQL para Tests
  postgres-test:
    image: postgres:16-alpine
    container_name: errorservice-postgres-test
    environment:
      POSTGRES_DB: errorservice_test
      POSTGRES_USER: errorservice_test
      POSTGRES_PASSWORD: test_password_123
      POSTGRES_HOST_AUTH_METHOD: scram-sha-256
    ports:
      - "25432:5432"  # Puerto diferente para no conflicto con dev
    volumes:
      - postgres_test_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U errorservice_test -d errorservice_test"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - errorservice-test-network

  # RabbitMQ para Tests
  rabbitmq-test:
    image: rabbitmq:3.12-management-alpine
    container_name: errorservice-rabbitmq-test
    environment:
      RABBITMQ_DEFAULT_USER: errorservice_test
      RABBITMQ_DEFAULT_PASS: test_password_123
      RABBITMQ_DEFAULT_VHOST: errorservice_test
    ports:
      - "5672:5672"    # AMQP
      - "15672:15672"  # Management UI
    volumes:
      - rabbitmq_test_data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - errorservice-test-network

  # Redis para Tests (si se usa caching)
  redis-test:
    image: redis:7-alpine
    container_name: errorservice-redis-test
    ports:
      - "6379:6379"
    command: redis-server --requirepass test_password_123
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - errorservice-test-network

  # Jaeger para OpenTelemetry (opcional para tests)
  jaeger-test:
    image: jaegertracing/all-in-one:1.51
    container_name: errorservice-jaeger-test
    environment:
      COLLECTOR_OTLP_ENABLED: "true"
    ports:
      - "4318:4318"   # OTLP HTTP
      - "16686:16686" # Jaeger UI
    networks:
      - errorservice-test-network

volumes:
  postgres_test_data:
    driver: local
  rabbitmq_test_data:
    driver: local

networks:
  errorservice-test-network:
    driver: bridge
```

---

### Comandos para Testing

```powershell
# 1. Levantar infraestructura de test
docker-compose -f docker-compose.test.yml up -d

# 2. Esperar a que esté healthy
docker-compose -f docker-compose.test.yml ps

# 3. Ejecutar tests contra infraestructura real
dotnet test --filter "Category=RealInfrastructure"

# 4. Ver logs si hay errores
docker-compose -f docker-compose.test.yml logs postgres-test
docker-compose -f docker-compose.test.yml logs rabbitmq-test

# 5. Limpiar después de tests
docker-compose -f docker-compose.test.yml down -v
```

---

## 🧪 APPSETTINGS.TESTING.JSON

### Configuración para Tests con Infraestructura Real

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=25432;Database=errorservice_test;Username=errorservice_test;Password=test_password_123;Include Error Detail=true"
  },
  
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "errorservice_test",
    "Username": "errorservice_test",
    "Password": "test_password_123",
    "Exchange": "errorservice.test.exchange",
    "Queue": "errorservice.test.queue",
    "DeadLetterExchange": "errorservice.test.dlx",
    "DeadLetterQueue": "errorservice.test.dlq"
  },
  
  "Redis": {
    "ConnectionString": "localhost:6379,password=test_password_123",
    "InstanceName": "ErrorServiceTest:"
  },
  
  "Jwt": {
    "SecretKey": "test-secret-key-min-32-chars-long-for-testing-purposes!",
    "Issuer": "cardealer-auth-test",
    "Audience": "cardealer-services-test",
    "ExpirationMinutes": 60
  },
  
  "OpenTelemetry": {
    "ServiceName": "ErrorService-Test",
    "JaegerEndpoint": "http://localhost:4318",
    "EnableTracing": true,
    "EnableMetrics": true,
    "SamplingProbability": 1.0
  },
  
  "RateLimiting": {
    "EnableRateLimiting": true,
    "PermitLimit": 100,
    "WindowSeconds": 60,
    "QueueLimit": 10
  }
}
```

---

## 🎯 CATEGORÍAS DE TESTS

### Uso de [Trait] para Categorizar Tests

```csharp
// Usar [Trait] para diferenciar tests
[Trait("Category", "Unit")]           // InMemory, rápido
[Trait("Category", "Integration")]    // InMemory, medio
[Trait("Category", "RealInfrastructure")] // ✅ PostgreSQL + RabbitMQ real
[Trait("Category", "E2E")]            // End-to-end completo
```

### Ejecutar Tests por Categoría

```powershell
# Solo Unit Tests (CI/CD rápido)
dotnet test --filter "Category=Unit"

# Solo Integration Tests (InMemory)
dotnet test --filter "Category=Integration"

# Solo Real Infrastructure Tests (antes de merge a main)
dotnet test --filter "Category=RealInfrastructure"

# Todos menos E2E (para CI/CD)
dotnet test --filter "Category!=E2E"
```

---

## 🏗️ REALINFRASTRUCTURETESTFIXTURE

### Clase Base para Tests con Infraestructura Real

```csharp
// RealInfrastructureTestFixture.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ErrorService.Infrastructure.Persistence;
using RabbitMQ.Client;

namespace ErrorService.Tests.RealInfrastructure
{
    /// <summary>
    /// Fixture para tests con infraestructura REAL (PostgreSQL, RabbitMQ).
    /// IMPORTANTE: Requiere docker-compose.test.yml activo.
    /// </summary>
    public class RealInfrastructureTestFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public IConfiguration Configuration { get; private set; }
        
        public RealInfrastructureTestFixture()
        {
            // 1. Cargar appsettings.Testing.json
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Testing.json", optional: false)
                .Build();
            
            // 2. Configurar Services
            var services = new ServiceCollection();
            
            // DbContext con PostgreSQL REAL
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
            
            // RabbitMQ REAL
            services.AddSingleton<IConnection>(sp =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = Configuration["RabbitMQ:Host"],
                    Port = int.Parse(Configuration["RabbitMQ:Port"]),
                    VirtualHost = Configuration["RabbitMQ:VirtualHost"],
                    UserName = Configuration["RabbitMQ:Username"],
                    Password = Configuration["RabbitMQ:Password"]
                };
                return factory.CreateConnection();
            });
            
            ServiceProvider = services.BuildServiceProvider();
            
            // 3. Inicializar Database
            InitializeDatabase();
            
            // 4. Verificar RabbitMQ
            VerifyRabbitMQ();
        }
        
        private void InitializeDatabase()
        {
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Limpiar database antes de cada ejecución
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            // Aplicar migraciones si existen
            context.Database.Migrate();
            
            // Seed data de prueba
            SeedTestData(context);
        }
        
        private void SeedTestData(ApplicationDbContext context)
        {
            context.ErrorLogs.AddRange(
                new ErrorLog
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    ServiceName = "RealInfraTest",
                    ExceptionType = "TestException",
                    Message = "Real infrastructure test error 1",
                    StatusCode = 500,
                    OccurredAt = DateTime.UtcNow.AddHours(-1)
                },
                new ErrorLog
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                    ServiceName = "RealInfraTest",
                    ExceptionType = "ValidationException",
                    Message = "Real infrastructure test error 2",
                    StatusCode = 400,
                    OccurredAt = DateTime.UtcNow.AddMinutes(-30)
                }
            );
            
            context.SaveChanges();
        }
        
        private void VerifyRabbitMQ()
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var connection = scope.ServiceProvider.GetRequiredService<IConnection>();
                
                if (!connection.IsOpen)
                {
                    throw new InvalidOperationException(
                        "RabbitMQ connection is not open. " +
                        "Ensure docker-compose.test.yml is running.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to connect to RabbitMQ. " +
                    "Ensure docker-compose.test.yml is running.", ex);
            }
        }
        
        public void Dispose()
        {
            // Cleanup
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureDeleted();
            
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
```

---

## 🧪 TESTS CON POSTGRESQL REAL

### Template: Database Real Infrastructure Tests

```csharp
// PostgreSqlRealTests.cs
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ErrorService.Domain.Entities;
using ErrorService.Infrastructure.Persistence;

namespace ErrorService.Tests.RealInfrastructure.Database
{
    [Trait("Category", "RealInfrastructure")]
    public class PostgreSqlRealTests : IClassFixture<RealInfrastructureTestFixture>
    {
        private readonly RealInfrastructureTestFixture _fixture;
        
        public PostgreSqlRealTests(RealInfrastructureTestFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public async Task PostgreSQL_Connection_IsHealthy()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Act
            var canConnect = await context.Database.CanConnectAsync();
            
            // Assert
            Assert.True(canConnect, "Cannot connect to PostgreSQL test database");
        }
        
        [Fact]
        public async Task PostgreSQL_UniqueConstraint_IsEnforced()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var errorId = Guid.NewGuid();
            var error1 = new ErrorLog
            {
                Id = errorId,
                ServiceName = "UniqueTest",
                Message = "Test",
                StatusCode = 500,
                OccurredAt = DateTime.UtcNow
            };
            
            var error2 = new ErrorLog
            {
                Id = errorId,  // MISMO ID - debe fallar
                ServiceName = "UniqueTest",
                Message = "Test 2",
                StatusCode = 500,
                OccurredAt = DateTime.UtcNow
            };
            
            // Act & Assert
            await context.ErrorLogs.AddAsync(error1);
            await context.SaveChangesAsync();
            
            await context.ErrorLogs.AddAsync(error2);
            
            // PostgreSQL REAL debe lanzar DbUpdateException por Primary Key violation
            await Assert.ThrowsAsync<DbUpdateException>(
                async () => await context.SaveChangesAsync());
        }
        
        [Fact]
        public async Task PostgreSQL_Transaction_RollbackWorksCorrectly()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var error = new ErrorLog
            {
                Id = Guid.NewGuid(),
                ServiceName = "TransactionTest",
                Message = "Test transaction",
                StatusCode = 500,
                OccurredAt = DateTime.UtcNow
            };
            
            // Act
            using var transaction = await context.Database.BeginTransactionAsync();
            
            await context.ErrorLogs.AddAsync(error);
            await context.SaveChangesAsync();
            
            // Verificar que está en DB (dentro de transacción)
            var countInsideTransaction = await context.ErrorLogs
                .CountAsync(e => e.ServiceName == "TransactionTest");
            Assert.Equal(1, countInsideTransaction);
            
            // Rollback
            await transaction.RollbackAsync();
            
            // Assert - No debe existir después del rollback
            var countAfterRollback = await context.ErrorLogs
                .CountAsync(e => e.ServiceName == "TransactionTest");
            Assert.Equal(0, countAfterRollback);
        }
        
        [Fact]
        public async Task PostgreSQL_ConcurrentInserts_HandleCorrectly()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Act - Insertar 10 registros concurrentemente
            var tasks = Enumerable.Range(1, 10).Select(async i =>
            {
                var error = new ErrorLog
                {
                    Id = Guid.NewGuid(),
                    ServiceName = "ConcurrencyTest",
                    Message = $"Concurrent insert {i}",
                    StatusCode = 500,
                    OccurredAt = DateTime.UtcNow
                };
                
                await context.ErrorLogs.AddAsync(error);
                await context.SaveChangesAsync();
            });
            
            await Task.WhenAll(tasks);
            
            // Assert
            var count = await context.ErrorLogs
                .CountAsync(e => e.ServiceName == "ConcurrencyTest");
            Assert.Equal(10, count);
        }
        
        [Fact]
        public async Task PostgreSQL_ComplexQuery_WithJoinsAndFilters()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Act - Query compleja con filtros
            var result = await context.ErrorLogs
                .Where(e => e.ServiceName == "RealInfraTest")
                .Where(e => e.StatusCode >= 400 && e.StatusCode < 600)
                .Where(e => e.OccurredAt >= DateTime.UtcNow.AddHours(-2))
                .OrderByDescending(e => e.OccurredAt)
                .Take(10)
                .ToListAsync();
            
            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, e => Assert.Equal("RealInfraTest", e.ServiceName));
        }
        
        [Fact]
        public async Task PostgreSQL_DateTimeHandling_PreservesUtc()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var now = DateTime.UtcNow;
            var error = new ErrorLog
            {
                Id = Guid.NewGuid(),
                ServiceName = "DateTimeTest",
                Message = "Test UTC",
                StatusCode = 500,
                OccurredAt = now
            };
            
            // Act
            await context.ErrorLogs.AddAsync(error);
            await context.SaveChangesAsync();
            
            var retrieved = await context.ErrorLogs.FindAsync(error.Id);
            
            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(DateTimeKind.Utc, retrieved.OccurredAt.Kind);
            
            // Tolerancia de 1 segundo (por redondeo en DB)
            var diff = Math.Abs((retrieved.OccurredAt - now).TotalSeconds);
            Assert.True(diff < 1, $"DateTime difference too large: {diff} seconds");
        }
    }
}
```

---

## 🐰 TESTS CON RABBITMQ REAL

### Template: RabbitMQ Real Infrastructure Tests

```csharp
// RabbitMqRealTests.cs
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using ErrorService.Shared.Events;

namespace ErrorService.Tests.RealInfrastructure.Messaging
{
    [Trait("Category", "RealInfrastructure")]
    public class RabbitMqRealTests : IClassFixture<RealInfrastructureTestFixture>
    {
        private readonly RealInfrastructureTestFixture _fixture;
        
        public RabbitMqRealTests(RealInfrastructureTestFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public void RabbitMQ_Connection_IsOpen()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var connection = scope.ServiceProvider.GetRequiredService<IConnection>();
            
            // Assert
            Assert.True(connection.IsOpen, "RabbitMQ connection is not open");
        }
        
        [Fact]
        public async Task RabbitMQ_PublishAndConsume_MessageDelivered()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var connection = scope.ServiceProvider.GetRequiredService<IConnection>();
            using var channel = connection.CreateModel();
            
            var exchangeName = "test.exchange";
            var queueName = "test.queue";
            var routingKey = "test.routing.key";
            
            // Declarar exchange y queue
            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: false);
            channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false);
            channel.QueueBind(queueName, exchangeName, routingKey);
            
            var messageReceived = new TaskCompletionSource<string>();
            
            // Consumer
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                messageReceived.SetResult(message);
            };
            
            channel.BasicConsume(queueName, autoAck: true, consumer);
            
            // Act - Publicar mensaje
            var testMessage = "Test message from RabbitMQ real test";
            var messageBytes = Encoding.UTF8.GetBytes(testMessage);
            
            channel.BasicPublish(
                exchange: exchangeName,
                routingKey: routingKey,
                basicProperties: null,
                body: messageBytes);
            
            // Wait for message (timeout 5s)
            var receivedMessage = await messageReceived.Task
                .WaitAsync(TimeSpan.FromSeconds(5));
            
            // Assert
            Assert.Equal(testMessage, receivedMessage);
            
            // Cleanup
            channel.QueueDelete(queueName);
            channel.ExchangeDelete(exchangeName);
        }
        
        [Fact]
        public async Task RabbitMQ_DeadLetterQueue_MovesRejectedMessages()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var connection = scope.ServiceProvider.GetRequiredService<IConnection>();
            using var channel = connection.CreateModel();
            
            var mainExchange = "test.main.exchange";
            var mainQueue = "test.main.queue";
            var dlxExchange = "test.dlx.exchange";
            var dlxQueue = "test.dlx.queue";
            var routingKey = "test.key";
            
            // Declarar DLX
            channel.ExchangeDeclare(dlxExchange, ExchangeType.Direct, durable: false);
            channel.QueueDeclare(dlxQueue, durable: false, exclusive: false, autoDelete: false);
            channel.QueueBind(dlxQueue, dlxExchange, routingKey);
            
            // Declarar queue principal con DLX
            var arguments = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", dlxExchange },
                { "x-dead-letter-routing-key", routingKey }
            };
            
            channel.ExchangeDeclare(mainExchange, ExchangeType.Direct, durable: false);
            channel.QueueDeclare(mainQueue, durable: false, exclusive: false, autoDelete: false, arguments);
            channel.QueueBind(mainQueue, mainExchange, routingKey);
            
            var dlxMessageReceived = new TaskCompletionSource<string>();
            
            // Consumer para DLX
            var dlxConsumer = new EventingBasicConsumer(channel);
            dlxConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                dlxMessageReceived.SetResult(message);
            };
            
            channel.BasicConsume(dlxQueue, autoAck: true, dlxConsumer);
            
            // Consumer para queue principal (RECHAZA mensaje)
            var mainConsumer = new EventingBasicConsumer(channel);
            mainConsumer.Received += (model, ea) =>
            {
                // Rechazar mensaje → debe ir a DLX
                channel.BasicReject(ea.DeliveryTag, requeue: false);
            };
            
            channel.BasicConsume(mainQueue, autoAck: false, mainConsumer);
            
            // Act - Publicar mensaje
            var testMessage = "Message to be rejected and moved to DLX";
            var messageBytes = Encoding.UTF8.GetBytes(testMessage);
            
            channel.BasicPublish(mainExchange, routingKey, null, messageBytes);
            
            // Wait for message in DLX (timeout 5s)
            var dlxMessage = await dlxMessageReceived.Task
                .WaitAsync(TimeSpan.FromSeconds(5));
            
            // Assert
            Assert.Equal(testMessage, dlxMessage);
            
            // Cleanup
            channel.QueueDelete(mainQueue);
            channel.QueueDelete(dlxQueue);
            channel.ExchangeDelete(mainExchange);
            channel.ExchangeDelete(dlxExchange);
        }
        
        [Fact]
        public async Task RabbitMQ_PublishEvent_JsonSerialization()
        {
            // Arrange
            using var scope = _fixture.ServiceProvider.CreateScope();
            var connection = scope.ServiceProvider.GetRequiredService<IConnection>();
            using var channel = connection.CreateModel();
            
            var exchangeName = "test.event.exchange";
            var queueName = "test.event.queue";
            var routingKey = "error.critical";
            
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: false);
            channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false);
            channel.QueueBind(queueName, exchangeName, "error.*");
            
            var eventReceived = new TaskCompletionSource<ErrorCriticalEvent>();
            
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var @event = JsonSerializer.Deserialize<ErrorCriticalEvent>(json);
                eventReceived.SetResult(@event);
            };
            
            channel.BasicConsume(queueName, autoAck: true, consumer);
            
            // Act - Publicar evento serializado
            var testEvent = new ErrorCriticalEvent
            {
                EventId = Guid.NewGuid(),
                ServiceName = "TestService",
                Message = "Critical error from real RabbitMQ test",
                OccurredAt = DateTime.UtcNow
            };
            
            var json = JsonSerializer.Serialize(testEvent);
            var messageBytes = Encoding.UTF8.GetBytes(json);
            
            channel.BasicPublish(exchangeName, routingKey, null, messageBytes);
            
            // Wait for event
            var receivedEvent = await eventReceived.Task
                .WaitAsync(TimeSpan.FromSeconds(5));
            
            // Assert
            Assert.NotNull(receivedEvent);
            Assert.Equal(testEvent.EventId, receivedEvent.EventId);
            Assert.Equal(testEvent.ServiceName, receivedEvent.ServiceName);
            Assert.Equal(testEvent.Message, receivedEvent.Message);
            
            // Cleanup
            channel.QueueDelete(queueName);
            channel.ExchangeDelete(exchangeName);
        }
    }
}
```

---

## 📜 POWERSHELL SCRIPT PARA VALIDACIÓN COMPLETA

### VALIDATE-REAL-INFRASTRUCTURE.ps1

```powershell
# VALIDATE-REAL-INFRASTRUCTURE.ps1
# Script para validar microservicio contra infraestructura REAL
param(
    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "ErrorService",
    
    [Parameter(Mandatory=$false)]
    [string]$ComposeFile = "docker-compose.test.yml"
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  REAL INFRASTRUCTURE VALIDATION" -ForegroundColor Cyan
Write-Host "  Service: $ServiceName" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# STEP 1: Verificar que docker-compose.test.yml existe
Write-Host "[STEP 1] Verificando docker-compose.test.yml..." -NoNewline
if (-not (Test-Path $ComposeFile)) {
    Write-Host " ❌ FAILED" -ForegroundColor Red
    Write-Host "  Error: $ComposeFile no encontrado" -ForegroundColor Red
    exit 1
}
Write-Host " ✅ OK" -ForegroundColor Green

# STEP 2: Detener contenedores existentes
Write-Host "[STEP 2] Deteniendo contenedores existentes..." -NoNewline
docker-compose -f $ComposeFile down -v 2>&1 | Out-Null
Write-Host " ✅ OK" -ForegroundColor Green

# STEP 3: Levantar infraestructura
Write-Host "[STEP 3] Levantando infraestructura de test..." -NoNewline
docker-compose -f $ComposeFile up -d
if ($LASTEXITCODE -ne 0) {
    Write-Host " ❌ FAILED" -ForegroundColor Red
    exit 1
}
Write-Host " ✅ OK" -ForegroundColor Green

# STEP 4: Esperar a que servicios estén healthy
Write-Host "[STEP 4] Esperando servicios (max 60s)..." -NoNewline
$timeout = 60
$elapsed = 0
$allHealthy = $false

while ($elapsed -lt $timeout -and -not $allHealthy) {
    Start-Sleep -Seconds 2
    $elapsed += 2
    
    $services = docker-compose -f $ComposeFile ps --format json | ConvertFrom-Json
    $healthyCount = ($services | Where-Object { $_.Health -eq "healthy" -or $_.State -eq "running" }).Count
    $totalCount = $services.Count
    
    if ($healthyCount -eq $totalCount) {
        $allHealthy = $true
    }
}

if (-not $allHealthy) {
    Write-Host " ❌ TIMEOUT" -ForegroundColor Red
    Write-Host "  Servicios no están healthy después de ${timeout}s" -ForegroundColor Red
    docker-compose -f $ComposeFile ps
    docker-compose -f $ComposeFile logs
    exit 1
}
Write-Host " ✅ OK (${elapsed}s)" -ForegroundColor Green

# STEP 5: Verificar conectividad PostgreSQL
Write-Host "[STEP 5] Verificando PostgreSQL..." -NoNewline
try {
    $pgTest = docker exec errorservice-postgres-test pg_isready -U errorservice_test -d errorservice_test
    if ($LASTEXITCODE -ne 0) {
        throw "PostgreSQL not ready"
    }
    Write-Host " ✅ OK" -ForegroundColor Green
}
catch {
    Write-Host " ❌ FAILED" -ForegroundColor Red
    Write-Host "  Error: $_" -ForegroundColor Red
    exit 1
}

# STEP 6: Verificar RabbitMQ
Write-Host "[STEP 6] Verificando RabbitMQ..." -NoNewline
try {
    $rabbitTest = docker exec errorservice-rabbitmq-test rabbitmq-diagnostics ping
    if ($LASTEXITCODE -ne 0) {
        throw "RabbitMQ not ready"
    }
    Write-Host " ✅ OK" -ForegroundColor Green
}
catch {
    Write-Host " ❌ FAILED" -ForegroundColor Red
    Write-Host "  Error: $_" -ForegroundColor Red
    exit 1
}

# STEP 7: Ejecutar Real Infrastructure Tests
Write-Host "[STEP 7] Ejecutando Real Infrastructure Tests..." -ForegroundColor Yellow
Write-Host ""

dotnet test `
    --filter "Category=RealInfrastructure" `
    --logger:"console;verbosity=detailed" `
    --results-directory:./TestResults

$testExitCode = $LASTEXITCODE

Write-Host ""

# STEP 8: Mostrar logs si hubo errores
if ($testExitCode -ne 0) {
    Write-Host "[STEP 8] Tests FAILED - Mostrando logs..." -ForegroundColor Red
    Write-Host ""
    Write-Host "=== PostgreSQL Logs ===" -ForegroundColor Yellow
    docker-compose -f $ComposeFile logs postgres-test --tail=50
    Write-Host ""
    Write-Host "=== RabbitMQ Logs ===" -ForegroundColor Yellow
    docker-compose -f $ComposeFile logs rabbitmq-test --tail=50
}

# STEP 9: Cleanup (opcional)
Write-Host "[STEP 9] Limpieza (mantener contenedores para debugging)..." -NoNewline
# docker-compose -f $ComposeFile down -v
Write-Host " ⏭️ SKIPPED" -ForegroundColor Yellow

# RESUMEN
Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  RESULTADO" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan

if ($testExitCode -eq 0) {
    Write-Host "✅ REAL INFRASTRUCTURE VALIDATION: PASSED ✅" -ForegroundColor Green
    Write-Host ""
    Write-Host "El microservicio está listo para merge a main." -ForegroundColor Green
    exit 0
}
else {
    Write-Host "❌ REAL INFRASTRUCTURE VALIDATION: FAILED ❌" -ForegroundColor Red
    Write-Host ""
    Write-Host "Revisar logs arriba. Contenedores siguen activos para debugging." -ForegroundColor Yellow
    Write-Host "Para detener: docker-compose -f $ComposeFile down -v" -ForegroundColor Yellow
    exit 1
}
```

---

## ✅ CHECKLIST PRE-MERGE A MAIN

- [ ] `docker-compose.test.yml` creado con PostgreSQL, RabbitMQ, Redis
- [ ] `appsettings.Testing.json` configurado con conexiones reales
- [ ] `RealInfrastructureTestFixture` implementado
- [ ] Tests con `[Trait("Category", "RealInfrastructure")]`
- [ ] Tests de PostgreSQL (constraints, transactions, concurrency)
- [ ] Tests de RabbitMQ (publish/consume, DLX, serialization)
- [ ] Script `VALIDATE-REAL-INFRASTRUCTURE.ps1` ejecutado exitosamente
- [ ] Todos los tests RealInfrastructure pasan (100%)
- [ ] Sin errores en logs de PostgreSQL
- [ ] Sin errores en logs de RabbitMQ
- [ ] Migraciones de DB aplicadas correctamente
- [ ] Performance de queries aceptable (< 100ms para queries simples)

---

## 📊 MATRIZ DE COBERTURA COMPLETA

| Tipo de Test | Database | RabbitMQ | Velocidad | Cuándo Ejecutar |
|--------------|----------|----------|-----------|-----------------|
| **Unit** | ❌ Mock | ❌ Mock | ⚡ <1s | Cada commit |
| **Integration (InMemory)** | ✅ InMemory | ❌ Mock | 🔄 1-5s | Cada push |
| **RealInfrastructure** | ✅ PostgreSQL | ✅ RabbitMQ | 🐢 5-30s | Pre-merge a main |
| **E2E** | ✅ PostgreSQL | ✅ RabbitMQ | 🐌 30-120s | Pre-release |

---

## 📚 RECURSOS Y REFERENCIAS

- **Microservicio de Referencia**: `ErrorService/docker-compose.test.yml`
- **RealInfrastructureTestFixture**: `ErrorService.Tests/RealInfrastructure/`
- **Docker Compose Documentation**: [Docker Compose](https://docs.docker.com/compose/)
- **PostgreSQL Testing**: [Npgsql Documentation](https://www.npgsql.org/doc/index.html)
- **RabbitMQ Testing**: [RabbitMQ .NET Client](https://www.rabbitmq.com/dotnet-api-guide.html)

---

**Fecha de Vigencia**: 2025-11-30  
**Aprobado por**: Equipo de Arquitectura CarDealer  
**Revisión**: Trimestral

**NOTA**: Merge a `main` BLOQUEADO si Real Infrastructure Tests fallan. InMemory tests NO son suficientes.
