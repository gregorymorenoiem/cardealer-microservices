# POLÍTICA 03: TESTING - PRUEBAS AUTOMATIZADAS

**Versión**: 1.0  
**Última Actualización**: 2025-11-30  
**Estado**: OBLIGATORIO ✅  
**Responsable**: Equipo de Arquitectura CarDealer

---

## 📋 RESUMEN EJECUTIVO

**POLÍTICA CRÍTICA**: Todos los microservicios deben tener cobertura mínima de tests: 80% Unit, 60% Integration, 40% E2E. Tests son requisito obligatorio para merge a develop/main.

**Objetivo**: Garantizar calidad del código, prevenir regresiones, facilitar refactoring seguro y documentar comportamiento esperado.

**Alcance**: Aplica a TODOS los microservicios del ecosistema CarDealer.

---

## 🎯 NIVELES DE COBERTURA OBLIGATORIOS

### Matriz de Cobertura Mínima

| Tipo de Test | Cobertura Mínima | Propósito | Velocidad |
|--------------|------------------|-----------|-----------|
| **Unit Tests** | **≥ 80%** | Validar lógica de negocio, handlers, validators | ⚡ Rápido (< 1s) |
| **Integration Tests** | **≥ 60%** | Validar APIs, base de datos, autenticación | 🔄 Medio (1-5s) |
| **E2E Tests** | **≥ 40%** | Validar flujos completos end-to-end | 🐢 Lento (5-30s) |

**REGLA**: Pull Requests con cobertura < mínimos son BLOQUEADOS automáticamente.

---

## 🧪 ESTRUCTURA DE TESTS OBLIGATORIA

### Organización de Proyecto de Tests

```
{ServiceName}.Tests/
├── {ServiceName}.Tests.csproj
├── Unit/                                          # Unit Tests (≥80%)
│   ├── Controllers/
│   │   └── ErrorsControllerTests.cs
│   ├── Handlers/
│   │   ├── Commands/
│   │   │   └── LogErrorCommandHandlerTests.cs
│   │   └── Queries/
│   │       └── GetErrorByIdQueryHandlerTests.cs
│   ├── Validators/
│   │   └── LogErrorCommandValidatorTests.cs
│   ├── Domain/
│   │   └── Entities/
│   │       └── ErrorLogTests.cs
│   └── Services/
│       └── RateLimitingServiceTests.cs
│
├── Integration/                                   # Integration Tests (≥60%)
│   ├── Api/
│   │   ├── ErrorsEndpointTests.cs
│   │   ├── AuthenticationTests.cs
│   │   └── AuthorizationTests.cs
│   ├── Database/
│   │   └── ErrorLogRepositoryTests.cs
│   └── Factories/
│       └── CustomWebApplicationFactory.cs         # ✅ OBLIGATORIO
│
├── E2E/                                           # E2E Tests (≥40%)
│   ├── Scenarios/
│   │   ├── LogErrorScenarioTests.cs
│   │   └── GetErrorScenarioTests.cs
│   └── Scripts/
│       └── E2E-TESTING-SCRIPT.ps1                 # ✅ OBLIGATORIO
│
└── Helpers/
    ├── TestDataBuilder.cs
    ├── MockDataProvider.cs
    └── TestConstants.cs
```

---

## 🔬 UNIT TESTS (≥ 80% Cobertura)

### Principios de Unit Tests

**REGLA**: Unit tests deben ser:
- ✅ **F.I.R.S.T.**: Fast, Isolated, Repeatable, Self-validating, Timely
- ✅ **A.A.A.**: Arrange, Act, Assert
- ✅ **Sin dependencias externas** (DB, RabbitMQ, APIs)
- ✅ **Usar mocks** para todas las dependencias

---

### Template: Controller Tests

```csharp
// ErrorsControllerTests.cs
using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ErrorService.Api.Controllers;
using ErrorService.Application.Commands.LogError;
using ErrorService.Application.Queries.GetErrorById;
using ErrorService.Domain.Entities;

namespace ErrorService.Tests.Unit.Controllers
{
    public class ErrorsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ErrorsController _controller;
        
        public ErrorsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new ErrorsController(_mediatorMock.Object);
        }
        
        [Fact]
        public async Task LogError_ValidCommand_ReturnsCreatedResult()
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "NullReferenceException",
                Message = "Test error message",
                StatusCode = 500
            };
            
            var expectedId = Guid.NewGuid();
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<LogErrorCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedId);
            
            // Act
            var result = await _controller.LogError(command);
            
            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(expectedId, createdResult.Value);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            
            _mediatorMock.Verify(
                m => m.Send(It.IsAny<LogErrorCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task GetById_ExistingId_ReturnsOkWithError()
        {
            // Arrange
            var errorId = Guid.NewGuid();
            var expectedError = new ErrorLog
            {
                Id = errorId,
                ServiceName = "TestService",
                Message = "Test error"
            };
            
            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetErrorByIdQuery>(q => q.Id == errorId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedError);
            
            // Act
            var result = await _controller.GetById(errorId);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedError = Assert.IsType<ErrorLog>(okResult.Value);
            Assert.Equal(errorId, returnedError.Id);
        }
        
        [Fact]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var errorId = Guid.NewGuid();
            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetErrorByIdQuery>(q => q.Id == errorId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ErrorLog?)null);
            
            // Act
            var result = await _controller.GetById(errorId);
            
            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
```

---

### Template: Command Handler Tests

```csharp
// LogErrorCommandHandlerTests.cs
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using ErrorService.Application.Commands.LogError;
using ErrorService.Domain.Entities;
using ErrorService.Domain.Interfaces;
using ErrorService.Shared.Events;

namespace ErrorService.Tests.Unit.Handlers.Commands
{
    public class LogErrorCommandHandlerTests
    {
        private readonly Mock<IErrorLogRepository> _repositoryMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<ILogger<LogErrorCommandHandler>> _loggerMock;
        private readonly LogErrorCommandHandler _handler;
        
        public LogErrorCommandHandlerTests()
        {
            _repositoryMock = new Mock<IErrorLogRepository>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _loggerMock = new Mock<ILogger<LogErrorCommandHandler>>();
            
            _handler = new LogErrorCommandHandler(
                _repositoryMock.Object,
                _eventPublisherMock.Object,
                _loggerMock.Object);
        }
        
        [Fact]
        public async Task Handle_ValidCommand_SavesErrorAndReturnsId()
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "NullReferenceException",
                Message = "Test error message",
                StackTrace = "at TestClass.TestMethod()",
                StatusCode = 500
            };
            
            ErrorLog? capturedErrorLog = null;
            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<ErrorLog>(), It.IsAny<CancellationToken>()))
                .Callback<ErrorLog, CancellationToken>((e, ct) => capturedErrorLog = e)
                .ReturnsAsync((ErrorLog e, CancellationToken ct) => e);
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.NotEqual(Guid.Empty, result);
            Assert.NotNull(capturedErrorLog);
            Assert.Equal(command.ServiceName, capturedErrorLog.ServiceName);
            Assert.Equal(command.Message, capturedErrorLog.Message);
            Assert.Equal(command.StatusCode, capturedErrorLog.StatusCode);
            
            _repositoryMock.Verify(
                r => r.AddAsync(It.IsAny<ErrorLog>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_CriticalError_PublishesEvent()
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "DatabaseException",
                Message = "Database connection failed",
                StatusCode = 500  // Critical
            };
            
            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<ErrorLog>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ErrorLog e, CancellationToken ct) => e);
            
            // Act
            await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            _eventPublisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<ErrorCriticalEvent>(e => 
                        e.ServiceName == command.ServiceName &&
                        e.Message == command.Message),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_NonCriticalError_DoesNotPublishEvent()
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "ValidationException",
                Message = "Invalid input",
                StatusCode = 400  // Not critical
            };
            
            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<ErrorLog>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ErrorLog e, CancellationToken ct) => e);
            
            // Act
            await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            _eventPublisherMock.Verify(
                p => p.PublishAsync(
                    It.IsAny<ErrorCriticalEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        
        [Theory]
        [InlineData(400)]
        [InlineData(401)]
        [InlineData(404)]
        [InlineData(500)]
        [InlineData(503)]
        public async Task Handle_VariousStatusCodes_SavesCorrectly(int statusCode)
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "Exception",
                Message = "Test",
                StatusCode = statusCode
            };
            
            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<ErrorLog>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ErrorLog e, CancellationToken ct) => e);
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _repositoryMock.Verify(
                r => r.AddAsync(
                    It.Is<ErrorLog>(e => e.StatusCode == statusCode),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
```

---

### Template: Validator Tests

```csharp
// LogErrorCommandValidatorTests.cs
using Xunit;
using FluentValidation.TestHelper;
using ErrorService.Application.Commands.LogError;

namespace ErrorService.Tests.Unit.Validators
{
    public class LogErrorCommandValidatorTests
    {
        private readonly LogErrorCommandValidator _validator;
        
        public LogErrorCommandValidatorTests()
        {
            _validator = new LogErrorCommandValidator();
        }
        
        [Fact]
        public void Validate_ValidCommand_PassesValidation()
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "NullReferenceException",
                Message = "Test error message",
                StatusCode = 500
            };
            
            // Act
            var result = _validator.TestValidate(command);
            
            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Validate_EmptyServiceName_FailsValidation(string serviceName)
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = serviceName,
                ExceptionType = "Exception",
                Message = "Test",
                StatusCode = 500
            };
            
            // Act
            var result = _validator.TestValidate(command);
            
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ServiceName);
        }
        
        [Fact]
        public void Validate_ServiceNameTooLong_FailsValidation()
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = new string('A', 101), // Max 100
                ExceptionType = "Exception",
                Message = "Test",
                StatusCode = 500
            };
            
            // Act
            var result = _validator.TestValidate(command);
            
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ServiceName);
        }
        
        [Theory]
        [InlineData("test'; DROP TABLE users;--")]
        [InlineData("test' OR '1'='1")]
        [InlineData("test UNION SELECT * FROM passwords")]
        public void Validate_SqlInjectionInMessage_FailsValidation(string maliciousMessage)
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
            var result = _validator.TestValidate(command);
            
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Message)
                .WithErrorMessage("*SQL injection*");
        }
        
        [Theory]
        [InlineData("<script>alert('XSS')</script>")]
        [InlineData("javascript:alert('XSS')")]
        [InlineData("<img src=x onerror=alert('XSS')>")]
        public void Validate_XssInMessage_FailsValidation(string xssPayload)
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
            var result = _validator.TestValidate(command);
            
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Message)
                .WithErrorMessage("*XSS*");
        }
        
        [Theory]
        [InlineData(99)]
        [InlineData(600)]
        [InlineData(1000)]
        public void Validate_InvalidStatusCode_FailsValidation(int invalidStatusCode)
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "Exception",
                Message = "Test",
                StatusCode = invalidStatusCode
            };
            
            // Act
            var result = _validator.TestValidate(command);
            
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.StatusCode);
        }
        
        [Fact]
        public void Validate_MessageExceedsMaxLength_FailsValidation()
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = "TestService",
                ExceptionType = "Exception",
                Message = new string('A', 5001), // Max 5000
                StatusCode = 500
            };
            
            // Act
            var result = _validator.TestValidate(command);
            
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Message);
        }
    }
}
```

---

## 🔗 INTEGRATION TESTS (≥ 60% Cobertura)

### CustomWebApplicationFactory (OBLIGATORIO)

```csharp
// CustomWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ErrorService.Infrastructure.Persistence;

namespace ErrorService.Tests.Integration.Factories
{
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
                {
                    services.Remove(descriptor);
                }
                
                // Agregar DbContext InMemory
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("ErrorServiceTestDb");
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });
                
                // Crear y poblar base de datos de prueba
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                
                db.Database.EnsureCreated();
                
                // Seed data si es necesario
                SeedTestData(db);
            });
            
            builder.UseEnvironment("Testing");
        }
        
        private void SeedTestData(ApplicationDbContext context)
        {
            // Agregar datos de prueba
            context.ErrorLogs.AddRange(
                new ErrorLog
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    ServiceName = "TestService",
                    ExceptionType = "NullReferenceException",
                    Message = "Test error 1",
                    StatusCode = 500,
                    OccurredAt = DateTime.UtcNow.AddHours(-1)
                },
                new ErrorLog
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    ServiceName = "TestService",
                    ExceptionType = "ValidationException",
                    Message = "Test error 2",
                    StatusCode = 400,
                    OccurredAt = DateTime.UtcNow.AddMinutes(-30)
                }
            );
            
            context.SaveChanges();
        }
    }
}
```

---

### Template: Endpoint Integration Tests

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
        public async Task POST_LogError_ReturnsCreated()
        {
            // Arrange
            var command = new LogErrorCommand
            {
                ServiceName = "IntegrationTestService",
                ExceptionType = "TestException",
                Message = "Integration test error",
                StatusCode = 500
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", command);
            
            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var errorId = await response.Content.ReadFromJsonAsync<Guid>();
            Assert.NotEqual(Guid.Empty, errorId);
        }
        
        [Fact]
        public async Task GET_ExistingError_ReturnsOk()
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
        public async Task POST_InvalidCommand_ReturnsBadRequest()
        {
            // Arrange - Command sin ServiceName (requerido)
            var invalidCommand = new LogErrorCommand
            {
                ServiceName = "",  // Inválido
                ExceptionType = "TestException",
                Message = "Test",
                StatusCode = 500
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/errors", invalidCommand);
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Fact]
        public async Task GET_HealthCheck_ReturnsHealthy()
        {
            // Act
            var response = await _client.GetAsync("/health");
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("healthy", content.ToLower());
        }
    }
}
```

---

### Template: Authentication Integration Tests

```csharp
// AuthenticationTests.cs
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
    public class AuthenticationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly string _jwtSecret = "test-secret-key-min-32-chars-long-for-testing-purposes!";
        
        public AuthenticationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }
        
        [Fact]
        public async Task POST_WithoutToken_ReturnsUnauthorized()
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
        public async Task POST_WithValidToken_ReturnsCreated()
        {
            // Arrange
            var token = GenerateJwtToken("testuser", "errorservice");
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
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        
        [Fact]
        public async Task POST_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", "invalid-token-123");
            
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
        public async Task POST_WithExpiredToken_ReturnsUnauthorized()
        {
            // Arrange
            var expiredToken = GenerateJwtToken(
                "testuser", 
                "errorservice", 
                expiresInMinutes: -10); // Expirado hace 10 minutos
            
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
        
        private string GenerateJwtToken(
            string username, 
            string service, 
            int expiresInMinutes = 60)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("service", service)
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

## 🎬 E2E TESTS (≥ 40% Cobertura)

### E2E-TESTING-SCRIPT.ps1 (OBLIGATORIO)

```powershell
# E2E-TESTING-SCRIPT.ps1
param(
    [Parameter(Mandatory=$false)]
    [string]$BaseUrl = "http://localhost:5000",
    
    [Parameter(Mandatory=$false)]
    [string]$JwtSecret = "test-secret-key-min-32-chars-long-for-testing-purposes!"
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  E2E TESTING - ErrorService" -ForegroundColor Cyan
Write-Host "  Base URL: $BaseUrl" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Resultados
$TestResults = @{
    Total = 0
    Passed = 0
    Failed = 0
}

function Test-Endpoint {
    param(
        [string]$Name,
        [scriptblock]$Test
    )
    
    $TestResults.Total++
    Write-Host "[TEST $($TestResults.Total)] $Name..." -NoNewline
    
    try {
        & $Test
        $TestResults.Passed++
        Write-Host " ✅ PASSED" -ForegroundColor Green
        return $true
    }
    catch {
        $TestResults.Failed++
        Write-Host " ❌ FAILED" -ForegroundColor Red
        Write-Host "  Error: $_" -ForegroundColor Red
        return $false
    }
}

# TEST 1: Health Check
Test-Endpoint "Health Check (Sin Autenticación)" {
    $response = Invoke-RestMethod -Uri "$BaseUrl/health" -Method GET -TimeoutSec 5
    if ($response.status -ne "healthy") {
        throw "Health check failed"
    }
}

# TEST 2: Protected Endpoint sin Token
Test-Endpoint "Protected Endpoint SIN Token (Debe devolver 401)" {
    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/errors" -Method POST -TimeoutSec 5
        throw "Should have returned 401"
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -ne 401) {
            throw "Expected 401, got $($_.Exception.Response.StatusCode.value__)"
        }
    }
}

# TEST 3: Generar JWT Token
Write-Host "[TEST 3] Generando JWT Token válido..." -NoNewline
try {
    # Generar token usando C# inline
    $tokenCode = @"
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class TokenGenerator
{
    public static string Generate(string secret)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "e2e-test-user"),
            new Claim("service", "errorservice")
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: "cardealer-auth",
            audience: "cardealer-services",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
"@
    
    Add-Type -TypeDefinition $tokenCode -ReferencedAssemblies @(
        "System.IdentityModel.Tokens.Jwt",
        "Microsoft.IdentityModel.Tokens"
    )
    
    $token = [TokenGenerator]::Generate($JwtSecret)
    Write-Host " ✅ Token generado" -ForegroundColor Green
}
catch {
    Write-Host " ⚠️ SKIPPED (usando token hardcoded)" -ForegroundColor Yellow
    # Fallback: token pre-generado (válido por 1 hora)
    $token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}

# TEST 4: POST Error con Token válido
Test-Endpoint "POST Error con JWT válido" {
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $body = @{
        serviceName = "E2ETestService"
        exceptionType = "TestException"
        message = "E2E test error"
        statusCode = 500
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod `
        -Uri "$BaseUrl/api/errors" `
        -Method POST `
        -Headers $headers `
        -Body $body `
        -TimeoutSec 10
    
    if ([string]::IsNullOrEmpty($response)) {
        throw "Response is empty"
    }
    
    $script:errorId = $response
}

# TEST 5: GET Error creado
Test-Endpoint "GET Error por ID" {
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    
    $response = Invoke-RestMethod `
        -Uri "$BaseUrl/api/errors/$script:errorId" `
        -Method GET `
        -Headers $headers `
        -TimeoutSec 5
    
    if ($response.serviceName -ne "E2ETestService") {
        throw "Unexpected service name: $($response.serviceName)"
    }
}

# TEST 6: SQL Injection Detection
Test-Endpoint "SQL Injection Detection (Debe devolver 400)" {
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $body = @{
        serviceName = "TestService"
        exceptionType = "Exception"
        message = "Test'; DROP TABLE users;--"
        statusCode = 500
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod `
            -Uri "$BaseUrl/api/errors" `
            -Method POST `
            -Headers $headers `
            -Body $body `
            -TimeoutSec 5
        throw "Should have returned 400"
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -ne 400) {
            throw "Expected 400, got $($_.Exception.Response.StatusCode.value__)"
        }
    }
}

# TEST 7: XSS Detection
Test-Endpoint "XSS Detection (Debe devolver 400)" {
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $body = @{
        serviceName = "TestService"
        exceptionType = "Exception"
        message = "<script>alert('XSS')</script>"
        statusCode = 500
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod `
            -Uri "$BaseUrl/api/errors" `
            -Method POST `
            -Headers $headers `
            -Body $body `
            -TimeoutSec 5
        throw "Should have returned 400"
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -ne 400) {
            throw "Expected 400, got $($_.Exception.Response.StatusCode.value__)"
        }
    }
}

# RESUMEN
Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  RESULTADOS E2E TESTING" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  Total Tests: $($TestResults.Total)" -ForegroundColor White
Write-Host "  Passed:      $($TestResults.Passed)" -ForegroundColor Green
Write-Host "  Failed:      $($TestResults.Failed)" -ForegroundColor Red
Write-Host ""

if ($TestResults.Failed -eq 0) {
    Write-Host "✅ ErrorService E2E Testing: PASSED ✅" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "❌ ErrorService E2E Testing: FAILED ❌" -ForegroundColor Red
    exit 1
}
```

---

## 📊 COVERAGE REPORTING

### Generar Reportes de Cobertura

```bash
# Instalar herramienta de coverage
dotnet tool install -g dotnet-reportgenerator-globaltool

# Ejecutar tests con coverage
dotnet test `
    --collect:"XPlat Code Coverage" `
    --results-directory:./TestResults `
    --logger:"console;verbosity=detailed"

# Generar reporte HTML
reportgenerator `
    -reports:"./TestResults/**/coverage.cobertura.xml" `
    -targetdir:"./TestResults/CoverageReport" `
    -reporttypes:"Html;Cobertura"

# Abrir reporte
start ./TestResults/CoverageReport/index.html
```

### .csproj Configuration para Coverage

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    
    <!-- Coverage Settings -->
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>cobertura</CoverletOutputFormat>
    <CoverletOutput>./TestResults/</CoverletOutput>
    <Threshold>80</Threshold>
    <ThresholdType>line</ThresholdType>
    <ThresholdStat>total</ThresholdStat>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageReference Include="Moq" Version="4.20.69" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="FluentValidation.TestHelper" Version="11.9.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
  </ItemGroup>
</Project>
```

---

## ✅ CHECKLIST DE CUMPLIMIENTO

- [ ] Proyecto {ServiceName}.Tests creado
- [ ] Estructura Unit/Integration/E2E implementada
- [ ] Unit tests ≥ 80% cobertura
- [ ] Integration tests ≥ 60% cobertura
- [ ] E2E tests ≥ 40% cobertura
- [ ] CustomWebApplicationFactory creado
- [ ] E2E-TESTING-SCRIPT.ps1 funcional
- [ ] Tests de Controllers implementados
- [ ] Tests de Handlers implementados
- [ ] Tests de Validators implementados
- [ ] Tests de Authentication/Authorization
- [ ] Tests de SQL Injection detection
- [ ] Tests de XSS detection
- [ ] Coverage reporting configurado
- [ ] CI/CD pipeline ejecuta tests automáticamente
- [ ] Quality gate configurado (coverage mínimo)

---

## 📚 RECURSOS Y REFERENCIAS

- **Microservicio de Referencia**: `ErrorService.Tests/`
- **xUnit Documentation**: [xunit.net](https://xunit.net/)
- **Moq Documentation**: [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- **FluentValidation Testing**: [Testing](https://docs.fluentvalidation.net/en/latest/testing.html)
- **Integration Testing**: [Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)

---

**Fecha de Vigencia**: 2025-11-30  
**Aprobado por**: Equipo de Arquitectura CarDealer  
**Revisión**: Trimestral

**NOTA**: PRs con cobertura < mínimos son BLOQUEADOS. Tests no son opcionales, son OBLIGATORIOS.
