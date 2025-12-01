# POLÍTICA 04: INTEGRATION TESTING - PRUEBAS DE INTEGRACIÓN

**Versión**: 1.0  
**Última Actualización**: 2025-11-30  
**Estado**: OBLIGATORIO ✅  
**Responsable**: Equipo de Arquitectura CarDealer

---

## 📋 RESUMEN EJECUTIVO

**POLÍTICA CRÍTICA**: Integration tests con ≥60% cobertura son OBLIGATORIOS. Deben validar integración entre componentes (API + DB + Auth) usando CustomWebApplicationFactory con InMemory database.

**Objetivo**: Validar que todos los componentes del microservicio funcionan correctamente integrados, sin depender de infraestructura externa en CI/CD.

**Alcance**: Aplica a TODOS los microservicios del ecosistema CarDealer.

---

## 🎯 QUÉ SON INTEGRATION TESTS

### Definición

**Integration Tests** validan la interacción entre múltiples componentes del sistema:
- ✅ API Controllers + MediatR + Handlers
- ✅ Handlers + Repositories + Database
- ✅ Autenticación JWT + Authorization Policies
- ✅ Validación FluentValidation en pipeline completo
- ✅ Middleware (Error Handling, Rate Limiting)

### Diferencia con Unit Tests

| Aspecto | Unit Tests | Integration Tests |
|---------|------------|-------------------|
| **Alcance** | 1 clase aislada | Múltiples componentes integrados |
| **Dependencias** | Mocks/Stubs | Dependencias reales (InMemory DB) |
| **Velocidad** | ⚡ Muy rápido (<1s) | 🔄 Medio (1-5s) |
| **Objetivo** | Lógica de negocio | Integración entre capas |
| **Base de Datos** | Sin DB | InMemory Database |
| **HTTP Requests** | No | Sí (HttpClient) |

---

## 🏗️ CUSTOMWEBAPPLICATIONFACTORY (OBLIGATORIO)

### Implementación Estándar

```csharp
// CustomWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ErrorService.Infrastructure.Persistence;

namespace ErrorService.Tests.Integration.Factories
{
    /// <summary>
    /// Factory para Integration Tests con InMemory Database.
    /// OBLIGATORIO: Todos los microservicios deben implementar esta clase.
    /// </summary>
    public class CustomWebApplicationFactory<TProgram>
        : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1. REMOVER DbContext REAL (PostgreSQL/SQL Server/etc)
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                
                // 2. AGREGAR DbContext InMemory
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("ErrorServiceTestDb");
                    
                    // IMPORTANTE: Habilitar para debugging
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });
                
                // 3. REMOVER RabbitMQ Publisher REAL (opcional)
                var rabbitDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IEventPublisher));
                
                if (rabbitDescriptor != null)
                {
                    services.Remove(rabbitDescriptor);
                    // Agregar Mock si es necesario
                    services.AddSingleton<IEventPublisher, MockEventPublisher>();
                }
                
                // 4. CREAR Y POBLAR BASE DE DATOS DE PRUEBA
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();
                
                // Asegurar que DB está creada
                dbContext.Database.EnsureCreated();
                
                // Seed data de prueba
                SeedTestData(dbContext);
            });
            
            // 5. USAR AMBIENTE "Testing"
            builder.UseEnvironment("Testing");
        }
        
        /// <summary>
        /// Seed data inicial para tests.
        /// REGLA: Usar GUIDs predecibles para facilitar tests.
        /// </summary>
        private void SeedTestData(ApplicationDbContext context)
        {
            // Limpiar data existente (en caso de re-uso)
            context.ErrorLogs.RemoveRange(context.ErrorLogs);
            context.SaveChanges();
            
            // Agregar datos de prueba con GUIDs predecibles
            context.ErrorLogs.AddRange(
                new ErrorLog
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    ServiceName = "TestService",
                    ExceptionType = "NullReferenceException",
                    Message = "Test error 1 - Seeded data",
                    StatusCode = 500,
                    OccurredAt = DateTime.UtcNow.AddHours(-1)
                },
                new ErrorLog
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    ServiceName = "TestService",
                    ExceptionType = "ValidationException",
                    Message = "Test error 2 - Seeded data",
                    StatusCode = 400,
                    OccurredAt = DateTime.UtcNow.AddMinutes(-30)
                },
                new ErrorLog
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    ServiceName = "AnotherService",
                    ExceptionType = "DatabaseException",
                    Message = "Test error 3 - Seeded data",
                    StatusCode = 503,
                    OccurredAt = DateTime.UtcNow.AddMinutes(-15)
                }
            );
            
            context.SaveChanges();
        }
    }
    
    /// <summary>
    /// Mock EventPublisher para tests (evita dependencia de RabbitMQ)
    /// </summary>
    public class MockEventPublisher : IEventPublisher
    {
        public List<IEvent> PublishedEvents { get; } = new();
        
        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct)
            where TEvent : IEvent
        {
            PublishedEvents.Add(@event);
            return Task.CompletedTask;
        }
    }
}
```

---

## 🧪 TIPOS DE INTEGRATION TESTS

### 1. API Endpoint Tests

**Objetivo**: Validar endpoints HTTP completos (Request → Controller → Handler → Repository → Response).

```csharp
// ErrorsEndpointTests.cs
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using ErrorService.Application.Commands.LogError;
using ErrorService.Domain.Entities;

namespace ErrorService.Tests.Integration.Api
{
    public class ErrorsEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        
        public ErrorsEndpointTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }
        
        [Fact]
        public async Task POST_LogError_ValidCommand_ReturnsCreatedWithLocation()
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = "IntegrationTestService",
                ExceptionType = "TestException",
                Message = "Integration test error message",
                StackTrace = "at TestClass.TestMethod()",
                StatusCode = 500
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", command);
            
            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            
            var errorId = await response.Content.ReadFromJsonAsync<Guid>();
            Assert.NotEqual(Guid.Empty, errorId);
        }
        
        [Fact]
        public async Task GET_ExistingError_ReturnsOkWithCorrectData()
        {
            // Arrange - Usar ID del seed data
            var existingId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            
            // Act
            var response = await _client.GetAsync($"/api/errors/{existingId}");
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var error = await response.Content.ReadFromJsonAsync<ErrorLog>();
            Assert.NotNull(error);
            Assert.Equal(existingId, error.Id);
            Assert.Equal("TestService", error.ServiceName);
            Assert.Equal("NullReferenceException", error.ExceptionType);
            Assert.Equal(500, error.StatusCode);
        }
        
        [Fact]
        public async Task GET_NonExistingError_ReturnsNotFound()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            
            // Act
            var response = await _client.GetAsync($"/api/errors/{nonExistingId}");
            
            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        public async Task POST_InvalidCommand_EmptyServiceName_ReturnsBadRequest()
        {
            // Arrange - FluentValidation debe detectar ServiceName vacío
            var invalidCommand = new LogErrorCommand
            {
                ServiceName = "",  // INVÁLIDO
                ExceptionType = "TestException",
                Message = "Test message",
                StatusCode = 500
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", invalidCommand);
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("ServiceName", content);
        }
        
        [Fact]
        public async Task POST_InvalidStatusCode_ReturnsBadRequest()
        {
            // Arrange - StatusCode fuera del rango 100-599
            var invalidCommand = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "Exception",
                Message = "Test",
                StatusCode = 999  // INVÁLIDO
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", invalidCommand);
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Fact]
        public async Task GET_AllErrors_ReturnsListWithSeedData()
        {
            // Act
            var response = await _client.GetAsync("/api/errors");
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var errors = await response.Content.ReadFromJsonAsync<List<ErrorLog>>();
            Assert.NotNull(errors);
            Assert.True(errors.Count >= 3); // Al menos los 3 del seed
        }
        
        [Fact]
        public async Task GET_ErrorsByServiceName_FiltersCorrectly()
        {
            // Act
            var response = await _client.GetAsync("/api/errors?serviceName=TestService");
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var errors = await response.Content.ReadFromJsonAsync<List<ErrorLog>>();
            Assert.NotNull(errors);
            Assert.All(errors, e => Assert.Equal("TestService", e.ServiceName));
        }
        
        [Fact]
        public async Task DELETE_ExistingError_ReturnsNoContent()
        {
            // Arrange
            var existingId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            
            // Act
            var response = await _client.DeleteAsync($"/api/errors/{existingId}");
            
            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            // Verificar que ya no existe
            var getResponse = await _client.GetAsync($"/api/errors/{existingId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
```

---

### 2. Authentication & Authorization Tests

**Objetivo**: Validar JWT authentication y authorization policies.

```csharp
// AuthenticationIntegrationTests.cs
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using ErrorService.Application.Commands.LogError;

namespace ErrorService.Tests.Integration.Api
{
    public class AuthenticationIntegrationTests 
        : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly string _jwtSecret = "test-secret-key-min-32-chars-long-for-testing-purposes!";
        
        public AuthenticationIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }
        
        [Fact]
        public async Task POST_WithoutAuthorizationHeader_Returns401()
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "Exception",
                Message = "Test",
                StatusCode = 500
            };
            
            // Act - Sin Authorization header
            var response = await _client.PostAsJsonAsync("/api/errors", command);
            
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        
        [Fact]
        public async Task POST_WithValidJwtToken_Returns201()
        {
            // Arrange
            var token = GenerateValidJwtToken(
                username: "testuser",
                service: "errorservice");
            
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
            
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "Exception",
                Message = "Test with valid JWT",
                StatusCode = 500
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", command);
            
            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        
        [Fact]
        public async Task POST_WithInvalidToken_Returns401()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", "invalid.jwt.token");
            
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "Exception",
                Message = "Test",
                StatusCode = 500
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", command);
            
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        
        [Fact]
        public async Task POST_WithExpiredToken_Returns401()
        {
            // Arrange - Token expirado hace 10 minutos
            var expiredToken = GenerateValidJwtToken(
                username: "testuser",
                service: "errorservice",
                expiresInMinutes: -10);
            
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", expiredToken);
            
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "Exception",
                Message = "Test",
                StatusCode = 500
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", command);
            
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        
        [Fact]
        public async Task POST_WithWrongServiceClaim_Returns403()
        {
            // Arrange - Token con claim "service" = "otherservice"
            var token = GenerateValidJwtToken(
                username: "testuser",
                service: "otherservice");  // NO "errorservice"
            
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
            
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "Exception",
                Message = "Test",
                StatusCode = 500
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", command);
            
            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        
        [Fact]
        public async Task GET_HealthCheck_WithoutAuth_Returns200()
        {
            // Act - Health check NO requiere autenticación
            var response = await _client.GetAsync("/health");
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("healthy", content.ToLower());
        }
        
        /// <summary>
        /// Helper: Generar JWT token válido para tests
        /// </summary>
        private string GenerateValidJwtToken(
            string username,
            string service,
            int expiresInMinutes = 60)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("service", service),
                new Claim(ClaimTypes.Role, "user")
            };
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var token = new JwtSecurityToken(
                issuer: "cardealer-auth",
                audience: "cardealer-services",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: credentials);
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
```

---

### 3. Validation Integration Tests

**Objetivo**: Validar que FluentValidation funciona correctamente en el pipeline completo.

```csharp
// ValidationIntegrationTests.cs
using System.Net;
using System.Net.Http.Json;
using Xunit;
using ErrorService.Application.Commands.LogError;

namespace ErrorService.Tests.Integration.Api
{
    public class ValidationIntegrationTests 
        : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        
        public ValidationIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }
        
        [Theory]
        [InlineData("test'; DROP TABLE users;--")]
        [InlineData("test' OR '1'='1")]
        [InlineData("test UNION SELECT * FROM passwords")]
        [InlineData("test; DELETE FROM errors;--")]
        public async Task POST_SqlInjectionInMessage_Returns400(string maliciousMessage)
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "Exception",
                Message = maliciousMessage,
                StatusCode = 500
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", command);
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("SQL injection", content, StringComparison.OrdinalIgnoreCase);
        }
        
        [Theory]
        [InlineData("<script>alert('XSS')</script>")]
        [InlineData("javascript:alert('XSS')")]
        [InlineData("<img src=x onerror=alert('XSS')>")]
        [InlineData("<iframe src='javascript:alert(1)'></iframe>")]
        public async Task POST_XssInMessage_Returns400(string xssPayload)
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "Exception",
                Message = xssPayload,
                StatusCode = 500
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", command);
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("XSS", content, StringComparison.OrdinalIgnoreCase);
        }
        
        [Fact]
        public async Task POST_MessageExceedsMaxLength_Returns400()
        {
            // Arrange - Message > 5000 caracteres
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "Exception",
                Message = new string('A', 5001),
                StatusCode = 500
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", command);
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Fact]
        public async Task POST_StackTraceExceedsMaxLength_Returns400()
        {
            // Arrange - StackTrace > 50000 caracteres
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "Exception",
                Message = "Test",
                StackTrace = new string('A', 50001),
                StatusCode = 500
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", command);
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Theory]
        [InlineData("Service@Name")]  // Caracteres especiales no permitidos
        [InlineData("Service Name")]  // Espacios no permitidos
        [InlineData("Service/Name")]
        public async Task POST_InvalidServiceNameFormat_Returns400(string invalidServiceName)
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = invalidServiceName,
                ExceptionType = "Exception",
                Message = "Test",
                StatusCode = 500
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", command);
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
```

---

### 4. Database Integration Tests

**Objetivo**: Validar operaciones CRUD en InMemory database.

```csharp
// DatabaseIntegrationTests.cs
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ErrorService.Domain.Entities;
using ErrorService.Domain.Interfaces;
using ErrorService.Infrastructure.Persistence;

namespace ErrorService.Tests.Integration.Database
{
    public class DatabaseIntegrationTests 
        : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        
        public DatabaseIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }
        
        [Fact]
        public async Task AddAsync_ValidError_SavesSuccessfully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IErrorLogRepository>();
            
            var newError = new ErrorLog
            {
                Id = Guid.NewGuid(),
                ServiceName = "DbTestService",
                ExceptionType = "TestException",
                Message = "Database integration test",
                StatusCode = 500,
                OccurredAt = DateTime.UtcNow
            };
            
            // Act
            var result = await repository.AddAsync(newError, CancellationToken.None);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(newError.Id, result.Id);
            Assert.Equal(newError.ServiceName, result.ServiceName);
        }
        
        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsError()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IErrorLogRepository>();
            
            var existingId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            
            // Act
            var result = await repository.GetByIdAsync(existingId, CancellationToken.None);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingId, result.Id);
            Assert.Equal("TestService", result.ServiceName);
        }
        
        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IErrorLogRepository>();
            
            var nonExistingId = Guid.NewGuid();
            
            // Act
            var result = await repository.GetByIdAsync(nonExistingId, CancellationToken.None);
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public async Task GetAllAsync_NoFilters_ReturnsAllErrors()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IErrorLogRepository>();
            
            // Act
            var results = await repository.GetAllAsync(
                serviceName: null,
                startDate: null,
                endDate: null,
                CancellationToken.None);
            
            // Assert
            Assert.NotNull(results);
            Assert.True(results.Count() >= 3); // Al menos los 3 del seed
        }
        
        [Fact]
        public async Task GetAllAsync_FilterByServiceName_ReturnsFilteredErrors()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IErrorLogRepository>();
            
            // Act
            var results = await repository.GetAllAsync(
                serviceName: "TestService",
                startDate: null,
                endDate: null,
                CancellationToken.None);
            
            // Assert
            Assert.NotNull(results);
            Assert.All(results, e => Assert.Equal("TestService", e.ServiceName));
        }
        
        [Fact]
        public async Task DeleteAsync_ExistingId_DeletesSuccessfully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IErrorLogRepository>();
            
            var existingId = Guid.Parse("00000000-0000-0000-0000-000000000003");
            
            // Act
            var result = await repository.DeleteAsync(existingId, CancellationToken.None);
            
            // Assert
            Assert.True(result);
            
            // Verificar que ya no existe
            var deletedError = await repository.GetByIdAsync(existingId, CancellationToken.None);
            Assert.Null(deletedError);
        }
        
        [Fact]
        public async Task UpdateAsync_ExistingError_UpdatesSuccessfully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IErrorLogRepository>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var existingId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var error = await repository.GetByIdAsync(existingId, CancellationToken.None);
            
            Assert.NotNull(error);
            var originalMessage = error.Message;
            error.Message = "UPDATED MESSAGE";
            
            // Act
            context.ErrorLogs.Update(error);
            await context.SaveChangesAsync();
            
            // Assert
            var updatedError = await repository.GetByIdAsync(existingId, CancellationToken.None);
            Assert.NotNull(updatedError);
            Assert.Equal("UPDATED MESSAGE", updatedError.Message);
            Assert.NotEqual(originalMessage, updatedError.Message);
        }
    }
}
```

---

## 📊 COBERTURA MÍNIMA REQUERIDA

### Componentes que DEBEN tener Integration Tests

| Componente | Cobertura Mínima | Ejemplos de Tests |
|------------|------------------|-------------------|
| **API Endpoints** | 100% | POST, GET, PUT, DELETE todos los endpoints |
| **Authentication** | 100% | Token válido, inválido, expirado, sin token |
| **Authorization** | 100% | Policies correctas, claims incorrectos |
| **FluentValidation** | 80% | SQL Injection, XSS, size limits, regex |
| **Database CRUD** | 80% | Add, Get, Update, Delete, filters |
| **Error Handling** | 60% | 400, 401, 403, 404, 500 responses |

---

## ✅ MEJORES PRÁCTICAS

### DO's (Hacer)

1. **Usar IClassFixture para compartir setup**
   ```csharp
   public class MyTests : IClassFixture<CustomWebApplicationFactory<Program>>
   {
       private readonly HttpClient _client;
       
       public MyTests(CustomWebApplicationFactory<Program> factory)
       {
           _client = factory.CreateClient();
       }
   }
   ```

2. **Usar GUIDs predecibles en seed data**
   ```csharp
   Id = Guid.Parse("00000000-0000-0000-0000-000000000001")
   ```

3. **Limpiar database entre tests si es necesario**
   ```csharp
   context.Database.EnsureDeleted();
   context.Database.EnsureCreated();
   ```

4. **Verificar StatusCode AND Response Content**
   ```csharp
   Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
   var content = await response.Content.ReadAsStringAsync();
   Assert.Contains("expected error", content);
   ```

5. **Usar Theory para múltiples casos**
   ```csharp
   [Theory]
   [InlineData("payload1")]
   [InlineData("payload2")]
   public async Task Test(string payload) { }
   ```

---

### DON'Ts (No Hacer)

1. **❌ NO usar base de datos real**
   ```csharp
   // ❌ PROHIBIDO
   options.UseNpgsql("Host=localhost;Database=real_db");
   
   // ✅ CORRECTO
   options.UseInMemoryDatabase("TestDb");
   ```

2. **❌ NO compartir estado entre tests**
   ```csharp
   // ❌ PROHIBIDO - variable estática compartida
   private static int _counter = 0;
   ```

3. **❌ NO depender de orden de ejecución**
   ```csharp
   // ❌ PROHIBIDO - Test2 depende de Test1
   [Fact] public async Task Test1() { /* crea registro */ }
   [Fact] public async Task Test2() { /* usa registro de Test1 */ }
   ```

4. **❌ NO usar Thread.Sleep**
   ```csharp
   // ❌ PROHIBIDO
   Thread.Sleep(1000);
   
   // ✅ CORRECTO
   await Task.Delay(100); // Solo si es absolutamente necesario
   ```

---

## 📋 CHECKLIST DE CUMPLIMIENTO

- [ ] CustomWebApplicationFactory implementado
- [ ] InMemory Database configurado
- [ ] Seed data con GUIDs predecibles
- [ ] Tests de todos los endpoints (GET, POST, PUT, DELETE)
- [ ] Tests de Authentication (con/sin token, válido/inválido/expirado)
- [ ] Tests de Authorization (policies correctas/incorrectas)
- [ ] Tests de FluentValidation (SQL Injection, XSS, size limits)
- [ ] Tests de Database CRUD operations
- [ ] Tests de Error Responses (400, 401, 403, 404, 500)
- [ ] Cobertura ≥ 60% en Integration Tests
- [ ] Tests ejecutándose en CI/CD pipeline
- [ ] Sin dependencias de infraestructura externa
- [ ] Tests aislados (no comparten estado)
- [ ] Usando IClassFixture correctamente

---

## 📚 RECURSOS Y REFERENCIAS

- **Microservicio de Referencia**: `ErrorService.Tests/Integration/`
- **CustomWebApplicationFactory Template**: `ErrorService.Tests/Integration/Factories/`
- **Integration Testing Guide**: [Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
- **WebApplicationFactory**: [ASP.NET Core Testing](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests#basic-tests-with-the-default-webapplicationfactory)

---

**Fecha de Vigencia**: 2025-11-30  
**Aprobado por**: Equipo de Arquitectura CarDealer  
**Revisión**: Trimestral

**NOTA**: Integration Tests con cobertura < 60% BLOQUEAN merge. CustomWebApplicationFactory es OBLIGATORIO en todos los microservicios.
