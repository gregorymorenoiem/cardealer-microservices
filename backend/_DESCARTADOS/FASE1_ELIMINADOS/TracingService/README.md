# TracingService - Distributed Tracing Service

## 📋 Descripción

**TracingService** es un servicio de consulta (query gateway) para trazas distribuidas almacenadas en **Jaeger**. Proporciona una API REST simplificada para consultar trazas, spans, estadísticas y servicios instrumentados.

## 🏗️ Arquitectura

### Clean Architecture

```
TracingService/
├── TracingService.Domain/          # Entidades de dominio
│   ├── Entities/
│   │   ├── Span.cs                # Span individual
│   │   ├── Trace.cs               # Traza completa
│   │   ├── SpanEvent.cs           # Eventos en spans
│   │   └── TraceStatistics.cs     # Estadísticas
│   └── Enums/
│       ├── SpanKind.cs            # Client, Server, Producer, Consumer, Internal
│       └── SpanStatus.cs          # Unset, Ok, Error
│
├── TracingService.Application/     # Lógica de aplicación (CQRS)
│   ├── Interfaces/
│   │   └── ITraceQueryService.cs  # Interfaz de consultas
│   ├── Queries/
│   │   └── TraceQueries.cs        # Queries de MediatR
│   └── Handlers/
│       └── TraceQueryHandlers.cs  # Handlers de MediatR
│
├── TracingService.Infrastructure/  # Implementación de infraestructura
│   ├── Services/
│   │   └── JaegerTraceQueryService.cs  # Cliente Jaeger HTTP API
│   └── DependencyInjection.cs
│
├── TracingService.Api/             # API REST
│   └── Controllers/
│       ├── TracesController.cs    # Consultas de trazas
│       └── ServicesController.cs  # Servicios y operaciones
│
└── TracingService.Tests/           # Tests unitarios (13 tests)
```

### Patrones Implementados

- **Clean Architecture**: Separación de capas por responsabilidad
- **CQRS**: Separación de consultas usando MediatR
- **Repository Pattern**: Abstracción del acceso a Jaeger API
- **Dependency Injection**: Inyección de dependencias

## 🚀 Características

### Core Features

- ✅ **Consulta de trazas por ID**: Obtener una traza completa con todos sus spans
- ✅ **Búsqueda avanzada**: Filtrar por servicio, operación, duración, errores, rango de tiempo
- ✅ **Estadísticas de trazas**: P95, P99, promedio, mediana de duraciones
- ✅ **Lista de servicios**: Ver todos los servicios instrumentados
- ✅ **Lista de operaciones**: Ver operaciones disponibles por servicio
- ✅ **Detección de errores**: Identificar trazas y spans con errores
- ✅ **Análisis de latencia**: Visualizar duraciones y detectar cuellos de botella

### Span Information

Cada span incluye:
- TraceId, SpanId, ParentSpanId
- Nombre de operación y servicio
- Timestamps (start/end) y duración
- Tipo (Client, Server, Producer, Consumer, Internal)
- Estado (Ok, Error) con mensajes
- Tags/atributos customizados
- Eventos (excepciones, logs)
- Información HTTP (método, URL, status code)

## 📡 API Endpoints

### Traces

#### GET /api/traces/{traceId}
Obtener una traza específica por ID.

**Response:**
```json
{
  "traceId": "abc123def456",
  "rootSpan": { ... },
  "spans": [ ... ],
  "startTime": "2024-12-02T10:00:00Z",
  "endTime": "2024-12-02T10:00:01.500Z",
  "durationMs": 1500,
  "spanCount": 12,
  "serviceCount": 4,
  "servicesInvolved": ["ServiceA", "ServiceB", "ServiceC", "ServiceD"],
  "hasError": false,
  "errorCount": 0
}
```

#### GET /api/traces
Buscar trazas con filtros opcionales.

**Query Parameters:**
- `serviceName` (string?): Filtrar por servicio
- `operationName` (string?): Filtrar por operación
- `startTime` (DateTime?): Fecha/hora de inicio
- `endTime` (DateTime?): Fecha/hora de fin
- `minDurationMs` (int?): Duración mínima en ms
- `maxDurationMs` (int?): Duración máxima en ms
- `hasError` (bool?): Solo trazas con errores
- `limit` (int): Límite de resultados (default: 100, max: 1000)

**Response:**
```json
{
  "traces": [ ... ],
  "count": 25,
  "filters": {
    "serviceName": "AuthService",
    "hasError": true,
    "limit": 100
  }
}
```

#### GET /api/traces/{traceId}/spans
Obtener todos los spans de una traza.

**Response:**
```json
{
  "traceId": "abc123",
  "spans": [
    {
      "spanId": "span1",
      "traceId": "abc123",
      "parentSpanId": null,
      "name": "GET /api/users",
      "kind": "Server",
      "status": "Ok",
      "startTime": "2024-12-02T10:00:00Z",
      "endTime": "2024-12-02T10:00:00.500Z",
      "durationMs": 500,
      "serviceName": "UserService",
      "httpMethod": "GET",
      "httpUrl": "http://localhost:5001/api/users",
      "httpStatusCode": 200,
      "tags": {
        "http.method": "GET",
        "http.status_code": "200"
      },
      "events": []
    }
  ],
  "count": 12
}
```

#### GET /api/traces/statistics
Obtener estadísticas de trazas.

**Query Parameters:**
- `startTime` (DateTime?): Fecha de inicio
- `endTime` (DateTime?): Fecha de fin
- `serviceName` (string?): Filtrar por servicio

**Response:**
```json
{
  "totalTraces": 1000,
  "totalSpans": 12500,
  "tracesWithErrors": 45,
  "averageDurationMs": 250.5,
  "medianDurationMs": 180.0,
  "p95DurationMs": 850.0,
  "p99DurationMs": 1500.0,
  "slowestTraceId": "xyz789",
  "slowestTraceDurationMs": 3200.0,
  "mostActiveService": "UserService",
  "mostActiveServiceSpanCount": 4500,
  "spansByService": {
    "UserService": 4500,
    "AuthService": 3000,
    "OrderService": 5000
  },
  "errorsByService": {
    "OrderService": 30,
    "PaymentService": 15
  },
  "startTime": "2024-12-02T00:00:00Z",
  "endTime": "2024-12-02T23:59:59Z"
}
```

### Services

#### GET /api/services
Obtener lista de servicios instrumentados.

**Response:**
```json
{
  "services": [
    "AuthService",
    "UserService",
    "OrderService",
    "PaymentService"
  ],
  "count": 4
}
```

#### GET /api/services/{serviceName}/operations
Obtener operaciones de un servicio.

**Response:**
```json
{
  "serviceName": "UserService",
  "operations": [
    "GET /api/users",
    "GET /api/users/{id}",
    "POST /api/users",
    "PUT /api/users/{id}",
    "DELETE /api/users/{id}"
  ],
  "count": 5
}
```

## 🔧 Configuración

### appsettings.json

```json
{
  "Jaeger": {
    "QueryUrl": "http://localhost:16686"
  }
}
```

### Docker Compose

El servicio está configurado en `docker-compose.yml`:

```yaml
jaeger:
  image: jaegertracing/all-in-one:1.51
  ports:
    - "16686:16686"  # Web UI
    - "4317:4317"    # OTLP gRPC
    - "4318:4318"    # OTLP HTTP
  networks:
    - cargurus-net

tracingservice:
  build:
    context: ./TracingService
  environment:
    Jaeger__QueryUrl: "http://jaeger:16686"
  ports:
    - "5097:80"
  depends_on:
    - jaeger
```

## 🧪 Testing

### Ejecutar Tests

```bash
cd TracingService
dotnet test
```

### Cobertura de Tests

- ✅ **13 tests unitarios** (100% passing)
- ✅ Tests de dominio (Span, Trace calculations)
- ✅ Tests de propiedades calculadas
- ✅ Tests de validación de errores
- ✅ Tests de conteo de servicios

## 🌐 Jaeger UI

### Acceso a Jaeger

Una vez iniciado el contenedor de Jaeger:

```
http://localhost:16686
```

### Características de Jaeger UI

- 🔍 **Búsqueda de trazas**: Interfaz visual para explorar trazas
- 📊 **Visualización de spans**: Ver dependencias entre servicios
- ⏱️ **Timeline**: Ver duración de cada span
- 🎯 **Comparación de trazas**: Comparar múltiples trazas
- 📈 **Gráficos de dependencias**: Ver arquitectura de microservicios

## 🔗 Integración con Microservicios

Para instrumentar un microservicio con OpenTelemetry y enviar trazas a Jaeger:

### 1. Instalar Paquetes NuGet

```bash
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
```

### 2. Configurar Program.cs

```csharp
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("YourServiceName"))
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://jaeger:4317");
            });
    });

var app = builder.Build();
app.Run();
```

### 3. Instrumentación Manual (Opcional)

```csharp
using System.Diagnostics;

public class MyService
{
    private static readonly ActivitySource ActivitySource = new("YourServiceName");
    
    public async Task DoSomethingAsync()
    {
        using var activity = ActivitySource.StartActivity("DoSomething");
        activity?.SetTag("custom.tag", "value");
        
        try
        {
            // Your code here
            await Task.Delay(100);
            
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
```

## 📊 Casos de Uso

### 1. Debugging de Latencia

Buscar las trazas más lentas:

```bash
GET /api/traces?minDurationMs=1000&limit=10
```

### 2. Detección de Errores

Buscar trazas con errores en las últimas 24 horas:

```bash
GET /api/traces?hasError=true&startTime=2024-12-01T00:00:00Z
```

### 3. Análisis de Servicio

Ver operaciones de un servicio específico:

```bash
GET /api/services/UserService/operations
```

### 4. Monitoreo de Performance

Obtener estadísticas de latencia:

```bash
GET /api/traces/statistics?serviceName=OrderService
```

## 🔐 Seguridad

### Recomendaciones

- ✅ **Autenticación**: Integrar con AuthService para proteger endpoints
- ✅ **Rate Limiting**: Limitar consultas para evitar sobrecarga
- ✅ **Filtrado de datos sensibles**: No incluir PII en tags/logs
- ✅ **CORS**: Configurar políticas CORS apropiadas
- ✅ **HTTPS**: Usar HTTPS en producción

## 📈 Métricas y Monitoreo

### Health Check

```
GET http://localhost:5097/health
```

### Métricas a Monitorear

- Número de trazas por minuto
- P95/P99 de duración de trazas
- Tasa de errores por servicio
- Servicios más activos
- Operaciones más lentas

## 🐛 Troubleshooting

### Jaeger no está disponible

1. Verificar que el contenedor de Jaeger esté corriendo:
   ```bash
   docker ps | grep jaeger
   ```

2. Verificar logs de Jaeger:
   ```bash
   docker logs jaeger
   ```

### No se ven trazas

1. Verificar que los servicios estén enviando trazas a Jaeger (puerto 4317)
2. Verificar configuración de OTLP exporter en los servicios
3. Revisar logs de los servicios instrumentados

### TracingService no se conecta a Jaeger

1. Verificar `Jaeger__QueryUrl` en appsettings.json
2. Verificar conectividad de red entre contenedores
3. Revisar logs del TracingService

## 📝 Roadmap

- [ ] Agregar cache Redis para consultas frecuentes
- [ ] Implementar paginación avanzada
- [ ] Agregar comparación de trazas (diff)
- [ ] Dashboard personalizado con SignalR
- [ ] Exportación de trazas a diferentes formatos
- [ ] Integración con sistemas de alertas

## 📚 Referencias

- [Jaeger Documentation](https://www.jaegertracing.io/docs/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [OpenTelemetry Specification](https://opentelemetry.io/docs/reference/specification/)
- [Distributed Tracing Best Practices](https://opentelemetry.io/docs/concepts/observability-primer/)

---

**Servicio #6** del roadmap de servicios transversales - ✅ Completado

**Stack:** Jaeger 1.51, OpenTelemetry, ASP.NET Core 8.0, MediatR, CQRS
