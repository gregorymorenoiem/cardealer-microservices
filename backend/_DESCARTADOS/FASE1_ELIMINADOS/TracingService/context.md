# TracingService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** TracingService
- **Puerto en Desarrollo:** 5013
- **Estado:** ⚠️ **SOLO DESARROLLO LOCAL**
- **Backend:** Jaeger / Zipkin
- **Base de Datos:** N/A (usa Jaeger backend)
- **Imagen Docker:** Local only

### Propósito
Servicio de distributed tracing para seguimiento de requests a través de múltiples microservicios. Implementa OpenTelemetry para correlacionar logs, métricas y traces. En producción se usa Jaeger directamente.

---

## 🏗️ ARQUITECTURA

```
TracingService/
├── TracingService.Api/
│   ├── Controllers/
│   │   ├── TracesController.cs
│   │   └── SpansController.cs
│   └── Program.cs
├── TracingService.Application/
│   └── Services/
│       └── TraceAnalysisService.cs
└── TracingService.Infrastructure/
    └── Jaeger/
        └── JaegerTraceRepository.cs
```

---

## 📊 CONCEPTOS

### Trace
Request completo desde el cliente hasta respuesta final, pasando por múltiples servicios.

### Span
Una operación individual dentro de un trace (ej: llamada a BD, HTTP request).

### Context Propagation
TraceId y SpanId se propagan via headers HTTP:
- `traceparent`: W3C Trace Context standard
- `X-B3-TraceId`: Zipkin format (legacy)

---

## 📡 ENDPOINTS API

#### GET `/api/traces`
Buscar traces.

**Query Parameters:**
- `service`: Filtrar por servicio
- `operation`: Filtrar por operación
- `minDuration`: Duración mínima (ms)
- `tags`: Filtrar por tags

**Response (200 OK):**
```json
{
  "traces": [
    {
      "traceId": "abc123...",
      "rootSpan": "GET /api/vehicles/123",
      "duration": 245,
      "spanCount": 8,
      "services": ["Gateway", "VehiclesSaleService", "MediaService"],
      "timestamp": "2026-01-07T10:30:00Z"
    }
  ]
}
```

#### GET `/api/traces/{traceId}`
Obtener detalle completo de un trace.

**Response (200 OK):**
```json
{
  "traceId": "abc123...",
  "rootSpan": {
    "spanId": "span1",
    "operation": "GET /api/vehicles/123",
    "service": "Gateway",
    "duration": 245,
    "tags": {"http.method": "GET", "http.status_code": 200}
  },
  "spans": [
    {
      "spanId": "span2",
      "parentSpanId": "span1",
      "operation": "GetVehicleByIdQuery",
      "service": "VehiclesSaleService",
      "duration": 125,
      "tags": {"db.system": "postgresql"}
    },
    {
      "spanId": "span3",
      "parentSpanId": "span2",
      "operation": "SELECT vehicles",
      "service": "VehiclesSaleService",
      "duration": 45,
      "tags": {"db.statement": "SELECT * FROM vehicles..."}
    }
  ],
  "totalDuration": 245
}
```

---

## 🔧 CONFIGURACIÓN

```json
{
  "Jaeger": {
    "AgentHost": "localhost",
    "AgentPort": 6831,
    "CollectorEndpoint": "http://localhost:14268/api/traces",
    "ServiceName": "TracingService",
    "Sampler": {
      "Type": "probabilistic",
      "Param": 1.0
    }
  },
  "OpenTelemetry": {
    "Endpoint": "http://localhost:4317"
  }
}
```

---

## 📈 ANÁLISIS DE TRACES

### Casos de Uso

#### Debugging de Latencia
- Identificar qué span toma más tiempo
- Detectar N+1 queries
- Optimizar rutas lentas

#### Error Correlation
- Ver todos los spans relacionados con un error
- Trace completo cuando falla un request

#### Service Dependencies
- Visualizar qué servicios llaman a qué servicios
- Detectar dependencies circulares

---

## 🚀 ALTERNATIVAS EN PRODUCCIÓN

- **Jaeger**: Self-hosted, open source
- **Zipkin**: Alternative to Jaeger
- **AWS X-Ray**: Para infra en AWS
- **Google Cloud Trace**: Para GCP
- **Datadog APM**: Solución managed

---

**Estado:** Solo desarrollo - En prod se usa OpenTelemetry → Jaeger directo  
**Versión:** 1.0.0
