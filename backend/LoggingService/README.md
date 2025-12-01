# LoggingService - Centralized Logging Service

## 📋 Descripción

El **LoggingService** es un servicio centralizado de agregación y consulta de logs para la arquitectura de microservicios. Utiliza **Seq** como plataforma de almacenamiento y análisis de logs estructurados, y **Serilog** como biblioteca de logging.

## 🏗️ Arquitectura

### Clean Architecture

```
LoggingService/
├── LoggingService.Domain/          # Entidades de dominio
│   ├── LogEntry.cs                 # Entrada de log
│   ├── LogFilter.cs                # Filtro de consulta
│   ├── LogLevel.cs                 # Nivel de log
│   └── LogStatistics.cs            # Estadísticas de logs
├── LoggingService.Application/     # Lógica de aplicación (CQRS)
│   ├── Interfaces/
│   │   └── ILogAggregator.cs      # Interfaz de agregación
│   ├── Queries/
│   │   └── LogQueries.cs          # Queries de MediatR
│   └── Handlers/
│       └── LogQueryHandlers.cs    # Handlers de MediatR
├── LoggingService.Infrastructure/  # Implementación de infraestructura
│   ├── Services/
│   │   └── SeqLogAggregator.cs   # Cliente de Seq
│   └── DependencyInjection.cs
├── LoggingService.Api/             # API REST
│   └── Controllers/
│       └── LogsController.cs      # Controlador de logs
└── LoggingService.Tests/           # Tests unitarios
```

### Patrones Implementados

- **Clean Architecture**: Separación de capas por responsabilidad
- **CQRS**: Separación de comandos y consultas usando MediatR
- **Repository Pattern**: Abstracción del acceso a datos
- **Dependency Injection**: Inyección de dependencias

## 🚀 Características

### Core Features

- ✅ **Agregación centralizada** de logs de todos los microservicios
- ✅ **Consulta avanzada** con filtros múltiples
- ✅ **Logging estructurado** con Serilog
- ✅ **Correlación de requests** con RequestId, TraceId, SpanId
- ✅ **Estadísticas en tiempo real** de logs
- ✅ **Búsqueda por texto** en mensajes de log
- ✅ **Filtrado por nivel** de severidad
- ✅ **Paginación** de resultados

### Log Levels

```csharp
public enum LogLevel
{
    Trace = 0,         // Información de debugging muy detallada
    Debug = 1,         // Información de debugging
    Information = 2,   // Mensajes informativos generales
    Warning = 3,       // Advertencias
    Error = 4,         // Errores que no detienen la aplicación
    Critical = 5       // Errores críticos que requieren atención inmediata
}
```

### Correlation IDs

El servicio soporta tres tipos de IDs de correlación para rastreo distribuido:

- **RequestId**: Identificador único de la petición HTTP
- **TraceId**: Identificador de la traza distribuida (OpenTelemetry compatible)
- **SpanId**: Identificador del span dentro de la traza

## 📡 API Endpoints

### GET /api/logs

Obtener logs con filtros opcionales.

**Query Parameters:**
- `startDate` (DateTime?): Fecha de inicio
- `endDate` (DateTime?): Fecha de fin
- `minLevel` (LogLevel?): Nivel mínimo de log
- `serviceName` (string?): Nombre del servicio
- `requestId` (string?): ID de request
- `traceId` (string?): ID de trace
- `userId` (string?): ID de usuario
- `searchText` (string?): Texto a buscar
- `hasException` (bool?): Filtrar logs con excepción
- `pageNumber` (int): Número de página (default: 1)
- `pageSize` (int): Tamaño de página (default: 100, max: 1000)

**Response:**
```json
[
  {
    "id": "abc123",
    "timestamp": "2024-11-28T10:30:00Z",
    "level": "Error",
    "message": "Database connection failed",
    "serviceName": "AuthService",
    "requestId": "req-123",
    "traceId": "trace-456",
    "spanId": "span-789",
    "userId": "user-001",
    "exception": "System.Data.SqlClient.SqlException: Connection timeout",
    "properties": {
      "MachineName": "server-01",
      "Environment": "Production"
    }
  }
]
```

### GET /api/logs/{id}

Obtener un log específico por ID.

**Response:**
```json
{
  "id": "abc123",
  "timestamp": "2024-11-28T10:30:00Z",
  "level": "Error",
  "message": "Database connection failed",
  "serviceName": "AuthService",
  ...
}
```

### GET /api/logs/statistics

Obtener estadísticas de logs.

**Query Parameters:**
- `startDate` (DateTime?): Fecha de inicio
- `endDate` (DateTime?): Fecha de fin

**Response:**
```json
{
  "totalLogs": 15000,
  "traceCount": 500,
  "debugCount": 2000,
  "informationCount": 10000,
  "warningCount": 2000,
  "errorCount": 450,
  "criticalCount": 50,
  "logsByService": {
    "AuthService": 5000,
    "MediaService": 3000,
    "ErrorService": 7000
  },
  "oldestLog": "2024-11-28T00:00:00Z",
  "newestLog": "2024-11-28T23:59:59Z"
}
```

## 🔧 Configuración

### appsettings.json

```json
{
  "Seq": {
    "Url": "http://localhost:5341"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### Docker Compose

```yaml
seq:
  image: datalust/seq:latest
  container_name: seq
  environment:
    ACCEPT_EULA: "Y"
  ports:
    - "5341:80"      # Web UI
    - "5342:5341"    # Ingestion
  volumes:
    - seq_data:/data

loggingservice:
  build:
    context: ./LoggingService
    dockerfile: Dockerfile
  environment:
    Seq__Url: "http://seq:80"
  ports:
    - "5096:80"
  depends_on:
    - seq
```

## 📦 Integración con otros servicios

Para integrar Serilog + Seq en otros microservicios:

### 1. Instalar NuGet Packages

```bash
dotnet add package Serilog.AspNetCore --version 10.0.0
dotnet add package Serilog.Sinks.Seq --version 9.0.0
```

### 2. Configurar Program.cs

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("ServiceName", "YourServiceName")
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:Url"] ?? "http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();

app.UseSerilogRequestLogging();
```

### 3. Usar logging en código

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public async Task DoSomethingAsync()
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = "user-123",
            ["RequestId"] = "req-456"
        }))
        {
            _logger.LogInformation("Starting operation");
            
            try
            {
                // Código...
                _logger.LogInformation("Operation completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Operation failed");
                throw;
            }
        }
    }
}
```

## 🧪 Testing

### Ejecutar Tests

```bash
dotnet test LoggingService.sln
```

### Cobertura de Tests

- ✅ **18 tests unitarios** (100% passing)
- ✅ Tests de dominio (LogEntry, LogFilter, LogStatistics)
- ✅ Tests de validación
- ✅ Tests de cálculos estadísticos

## 📊 Seq Dashboard

### Acceso

Una vez iniciado el contenedor de Seq, acceder a:

```
http://localhost:5341
```

### Búsquedas útiles

```sql
-- Logs de error en las últimas 24 horas
Level = 'Error' OR Level = 'Fatal'

-- Logs de un servicio específico
ServiceName = 'AuthService'

-- Logs con excepción
Exception IS NOT NULL

-- Logs de un usuario específico
UserId = 'user-123'

-- Logs de una traza específica
TraceId = 'trace-456'

-- Logs por request
RequestId = 'req-789'
```

## 🐛 Troubleshooting

### Seq no está disponible

Si los logs no aparecen en Seq:

1. Verificar que el contenedor de Seq esté corriendo:
   ```bash
   docker ps | grep seq
   ```

2. Verificar la configuración de Seq en appsettings.json

3. Verificar logs del contenedor de Seq:
   ```bash
   docker logs seq
   ```

### Logs no se están enviando

1. Verificar que Serilog esté configurado correctamente en Program.cs
2. Verificar que la URL de Seq sea correcta
3. Verificar que el servicio tenga acceso de red a Seq

## 📈 Métricas y Monitoreo

### Health Check

```
GET http://localhost:5096/health
```

### Estadísticas de uso

Usar el endpoint `/api/logs/statistics` para obtener:
- Total de logs por servicio
- Tasa de errores
- Servicio más activo
- Rango de tiempo de logs

## 🔐 Seguridad

### Recomendaciones

- ✅ **No logear información sensible** (passwords, tokens, PII)
- ✅ **Usar Seq API Keys** en producción
- ✅ **Habilitar HTTPS** para Seq
- ✅ **Configurar retención** de logs en Seq
- ✅ **Implementar rate limiting** en el API

### Variables de entorno sensibles

En producción, usar variables de entorno para:
- `Seq__ApiKey`
- `Seq__Url`

## 📝 Roadmap

- [ ] Implementar alertas automáticas
- [ ] Soporte para OpenTelemetry
- [ ] Dashboard personalizado
- [ ] Exportación de logs a diferentes formatos
- [ ] Integración con sistemas de monitoreo (Grafana, Prometheus)

## 📚 Referencias

- [Serilog Documentation](https://serilog.net/)
- [Seq Documentation](https://docs.datalust.co/docs)
- [OpenTelemetry](https://opentelemetry.io/)
- [Structured Logging](https://stackify.com/what-is-structured-logging-and-why-developers-need-it/)

---

**Servicio #4** del roadmap de servicios transversales - ✅ Completado
