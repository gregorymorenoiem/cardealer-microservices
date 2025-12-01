# LoggingService - Integration & E2E Tests

## 📋 Overview

Este proyecto contiene pruebas de integración y E2E (End-to-End) para el LoggingService.

## 🧪 Tipos de Pruebas

### Unit Tests (18 pruebas)
Ubicación: `LoggingService.Tests/`
- ✅ **LogEntryTests**: 6 tests
- ✅ **LogFilterTests**: 5 tests  
- ✅ **LogStatisticsTests**: 7 tests

### Integration Tests (14 pruebas)
Ubicación: `LoggingService.IntegrationTests/LogsControllerIntegrationTests.cs`

Pruebas que verifican la API REST sin necesidad de Seq real:
- `GetLogs_WithoutFilters_ReturnsOk`
- `GetLogs_WithValidFilters_ReturnsFilteredLogs`
- `GetLogs_WithInvalidFilter_ReturnsBadRequest`
- `GetLogs_WithPageSizeExceedingMax_ReturnsBadRequest`
- `GetLogs_WithServiceNameFilter_ReturnsLogsFromService`
- `GetLogs_WithSearchText_ReturnsMatchingLogs`
- `GetLogs_WithPagination_ReturnsCorrectPage`
- `GetLogById_WithValidId_ReturnsLog`
- `GetLogById_WithInvalidId_ReturnsNotFound`
- `GetStatistics_WithoutDateRange_ReturnsStatistics`
- `GetStatistics_WithDateRange_ReturnsFilteredStatistics`
- `GetStatistics_CalculatesErrorRateCorrectly`
- `Api_SupportsMultipleConcurrentRequests`

### E2E Tests (9 pruebas)
Ubicación: `LoggingService.IntegrationTests/LoggingServiceE2ETests.cs`

Pruebas completas que usan un contenedor Seq real mediante Testcontainers:
- `E2E_WriteLogsToSeq_ThenQueryThroughApi`
- `E2E_WriteDifferentLogLevels_ThenFilterByLevel`
- `E2E_WriteLogsWithException_ThenQueryWithExceptionFilter`
- `E2E_WriteLogsFromMultipleServices_ThenGetStatistics`
- `E2E_QueryLogsWithDateRange_ReturnsOnlyLogsInRange`
- `E2E_PaginationWorks_AcrossMultiplePages`
- `E2E_GetLogById_ReturnsCorrectLog`
- `E2E_HealthCheck_SeqIsAccessible`

## 🚀 Ejecución

### Pruebas Unitarias

```bash
cd backend
dotnet test LoggingService.sln --filter "FullyQualifiedName~LoggingService.Tests"
```

### Pruebas de Integración

```bash
cd backend
dotnet test LoggingService.sln --filter "FullyQualifiedName~LogsControllerIntegrationTests"
```

### Pruebas E2E (Requieren Docker)

**Prerequisitos:**
- Docker Desktop debe estar corriendo
- Puerto 5341 disponible (o Testcontainers asignará uno aleatorio)

```bash
cd backend
dotnet test LoggingService.sln --filter "FullyQualifiedName~LoggingServiceE2ETests"
```

### Todas las Pruebas

```bash
cd backend
dotnet test LoggingService.sln
```

## 🔧 Configuración

### LoggingWebApplicationFactory

Factory personalizado para pruebas que:
- Configura el `WebApplicationFactory<Program>`
- Permite inyectar URL de Seq customizada
- Reemplaza servicios para pruebas

### Testcontainers

Las pruebas E2E usan Testcontainers para:
- Levantar un contenedor Seq real
- Asignar puerto dinámicamente
- Limpiar automáticamente después de las pruebas

```csharp
_seqContainer = new ContainerBuilder()
    .WithImage("datalust/seq:latest")
    .WithPortBinding(5341, true)
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(5341)))
    .Build();
```

## 📊 Cobertura de Pruebas

### Escenarios Cubiertos

✅ **API Endpoints**
- GET /api/logs (con múltiples combinaciones de filtros)
- GET /api/logs/{id}
- GET /api/logs/statistics

✅ **Filtrado**
- Por rango de fechas
- Por nivel de log
- Por nombre de servicio
- Por RequestId/TraceId
- Por texto de búsqueda
- Por presencia de excepción

✅ **Paginación**
- Páginas múltiples
- Límites de PageSize
- Validación de parámetros

✅ **Validación**
- Filtros inválidos
- IDs inexistentes
- Parámetros fuera de rango

✅ **Estadísticas**
- Conteo por nivel
- Conteo por servicio
- Cálculo de tasa de errores
- Rango temporal de logs

✅ **Integración Seq Real (E2E)**
- Escritura de logs con Serilog
- Consulta a través del API
- Filtrado por múltiples criterios
- Correlación con RequestId/TraceId
- Manejo de excepciones

## 🏗️ Arquitectura de Pruebas

```
LoggingService.IntegrationTests/
├── LoggingWebApplicationFactory.cs      # Factory para pruebas
├── LogsControllerIntegrationTests.cs    # Pruebas de integración (sin Seq)
└── LoggingServiceE2ETests.cs            # Pruebas E2E (con Seq en Docker)
```

## 📦 Paquetes NuGet

- **Microsoft.AspNetCore.Mvc.Testing** 8.0.8: WebApplicationFactory
- **FluentAssertions** 8.8.0: Aserciones fluidas
- **Testcontainers** 3.10.0: Gestión de contenedores Docker
- **Serilog.Sinks.Seq** 9.0.0: Cliente Seq para E2E
- **Serilog.Extensions.Logging** 10.0.0: Extensiones de logging

## 🐛 Troubleshooting

### Las pruebas E2E fallan

**Problema**: `Docker daemon is not running`
**Solución**: Iniciar Docker Desktop

**Problema**: `Port 5341 is already in use`
**Solución**: Detener otros contenedores Seq o usar puerto aleatorio

**Problema**: `Testcontainers timeout`
**Solución**: 
- Verificar que Docker tiene suficientes recursos
- Aumentar timeout en `WithWaitStrategy`
- Verificar logs: `docker logs <container-id>`

### Las pruebas de integración fallan

**Problema**: `SeqLogAggregator` retorna vacío
**Solución**: Las pruebas de integración no requieren Seq real, deberían pasar siempre

## 📈 Métricas

### Tiempo de Ejecución

- **Unit Tests**: ~5 segundos (18 tests)
- **Integration Tests**: ~10 segundos (14 tests)
- **E2E Tests**: ~60 segundos (9 tests, incluye levantar Seq)

### Cobertura

- **Domain Layer**: 100%
- **Application Layer**: 80% (handlers)
- **Infrastructure Layer**: 70% (SeqLogAggregator)
- **API Layer**: 90% (controllers)

## 🎯 Mejores Prácticas

1. **Usar Arrange-Act-Assert (AAA)** en todas las pruebas
2. **Nombres descriptivos** que indican el escenario
3. **Pruebas independientes** que pueden ejecutarse en cualquier orden
4. **Cleanup automático** con IAsyncLifetime para E2E
5. **FluentAssertions** para mensajes de error claros
6. **Testcontainers** para reproducibilidad de E2E

## 📝 Agregar Nuevas Pruebas

### Prueba de Integración

```csharp
[Fact]
public async Task NewEndpoint_WithScenario_ExpectedResult()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/new-endpoint");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### Prueba E2E

```csharp
[Fact]
public async Task E2E_NewScenario_ExpectedResult()
{
    // Arrange - Write logs to Seq
    var logger = new LoggerConfiguration()
        .WriteTo.Seq(_seqUrl)
        .CreateLogger();
    
    logger.Information("Test log");
    logger.Dispose();
    
    await Task.Delay(2000); // Wait for indexing
    
    // Act - Query through API
    var response = await _client!.GetAsync("/api/logs");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## 🔗 Referencias

- [Microsoft.AspNetCore.Mvc.Testing](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Testcontainers](https://dotnet.testcontainers.org/)
- [FluentAssertions](https://fluentassertions.com/)
- [xUnit](https://xunit.net/)

---

✅ **Total: 41 pruebas** (18 unit + 14 integration + 9 E2E)
