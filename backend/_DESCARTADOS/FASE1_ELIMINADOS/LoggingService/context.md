# LoggingService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** LoggingService
- **Puerto en Desarrollo:** 5010
- **Estado:** ⚠️ **SOLO DESARROLLO LOCAL**
- **Backend:** Elasticsearch + Kibana
- **Base de Datos:** Elasticsearch
- **Imagen Docker:** Local only

### Propósito
Servicio centralizado de logging estructurado. Recibe logs de todos los microservicios, los indexa en Elasticsearch y proporciona API de búsqueda. Alternativa self-hosted a servicios como Loggly o Papertrail.

---

## 🏗️ ARQUITECTURA

```
LoggingService/
├── LoggingService.Api/
│   ├── Controllers/
│   │   ├── LogsController.cs
│   │   └── SearchController.cs
│   └── Program.cs
├── LoggingService.Application/
│   └── Services/
│       └── LogIndexingService.cs
└── LoggingService.Infrastructure/
    └── Elasticsearch/
        └── ElasticsearchLogRepository.cs
```

---

## 📦 ENTIDADES

### LogEntry
```csharp
public class LogEntry
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; }
    public string Level { get; set; }           // Debug, Info, Warning, Error
    public string Message { get; set; }
    public string? Exception { get; set; }
    public DateTime Timestamp { get; set; }
    public string? TraceId { get; set; }
    public string? SpanId { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}
```

---

## 📡 ENDPOINTS API

#### POST `/api/logs`
Enviar log entry.

**Request:**
```json
{
  "serviceName": "VehiclesSaleService",
  "level": "Error",
  "message": "Failed to load vehicle",
  "exception": "...",
  "properties": {
    "vehicleId": "123",
    "userId": "456"
  }
}
```

#### GET `/api/logs/search`
Buscar logs.

**Query Parameters:**
- `serviceName`: Filtrar por servicio
- `level`: Filtrar por nivel
- `from`: Fecha desde
- `to`: Fecha hasta
- `search`: Búsqueda full-text
- `traceId`: Filtrar por trace ID

---

## 🔧 CONFIGURACIÓN

```json
{
  "Elasticsearch": {
    "Url": "http://localhost:9200",
    "IndexPrefix": "okla-logs",
    "RetentionDays": 30
  }
}
```

---

## 📝 CASOS DE USO

- Debugging de errores en producción
- Auditoría de acciones
- Performance monitoring
- Análisis de patrones de uso

---

**Estado:** Solo desarrollo local - En producción se usa Serilog directo  
**Versión:** 1.0.0
