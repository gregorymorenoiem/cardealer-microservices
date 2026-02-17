# HealthCheckService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** HealthCheckService
- **Puerto en Desarrollo:** 5012
- **Estado:** ⚠️ **SOLO DESARROLLO LOCAL**
- **Base de Datos:** PostgreSQL (`healthcheckservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio centralizado de health checks y status monitoring. Monitorea la salud de todos los microservicios, bases de datos y servicios externos. Proporciona dashboard y alertas de disponibilidad.

---

## 🏗️ ARQUITECTURA

```
HealthCheckService/
├── HealthCheckService.Api/
│   ├── Controllers/
│   │   ├── HealthController.cs
│   │   ├── ServicesController.cs
│   │   └── StatusController.cs
│   └── Program.cs
├── HealthCheckService.Application/
│   └── Services/
│       ├── HealthCheckOrchestrator.cs
│       └── ServiceMonitor.cs
├── HealthCheckService.Domain/
│   ├── Entities/
│   │   ├── ServiceHealth.cs
│   │   └── HealthCheckResult.cs
│   └── Enums/
│       └── HealthStatus.cs
└── HealthCheckService.Infrastructure/
    └── BackgroundServices/
        └── HealthCheckWorker.cs            # Check cada 30 segundos
```

---

## 📦 ENTIDADES

### ServiceHealth
```csharp
public class ServiceHealth
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; }
    public string HealthCheckUrl { get; set; }      // https://service:8080/health
    public HealthStatus Status { get; set; }        // Healthy, Degraded, Unhealthy
    public DateTime LastCheckAt { get; set; }
    public int ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public int ConsecutiveFailures { get; set; }
}
```

### HealthStatus Enum
```csharp
public enum HealthStatus
{
    Healthy = 0,
    Degraded = 1,
    Unhealthy = 2,
    Unknown = 3
}
```

---

## 📡 ENDPOINTS API

#### GET `/api/health`
Health check del propio servicio.

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "timestamp": "2026-01-07T10:30:00Z"
}
```

#### GET `/api/services`
Estado de todos los servicios monitoreados.

**Response (200 OK):**
```json
{
  "services": [
    {
      "name": "VehiclesSaleService",
      "status": "Healthy",
      "lastCheckAt": "2026-01-07T10:30:00Z",
      "responseTimeMs": 45,
      "consecutiveFailures": 0
    },
    {
      "name": "AuthService",
      "status": "Unhealthy",
      "lastCheckAt": "2026-01-07T10:30:00Z",
      "errorMessage": "Connection timeout",
      "consecutiveFailures": 3
    }
  ],
  "totalServices": 10,
  "healthyCount": 9,
  "unhealthyCount": 1
}
```

#### GET `/api/services/{serviceName}`
Detalle de un servicio específico.

#### GET `/api/status/summary`
Resumen general del sistema.

**Response (200 OK):**
```json
{
  "systemStatus": "Degraded",
  "totalServices": 10,
  "healthyServices": 9,
  "degradedServices": 0,
  "unhealthyServices": 1,
  "averageResponseTimeMs": 78,
  "lastUpdatedAt": "2026-01-07T10:30:00Z"
}
```

---

## 🔧 CONFIGURACIÓN

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=healthcheckservice;..."
  },
  "HealthCheck": {
    "CheckIntervalSeconds": 30,
    "TimeoutSeconds": 10,
    "FailureThreshold": 3,
    "AlertOnFailure": true
  },
  "Services": [
    {
      "name": "Gateway",
      "healthCheckUrl": "http://localhost:18443/health"
    },
    {
      "name": "AuthService",
      "healthCheckUrl": "http://localhost:5001/health"
    },
    {
      "name": "VehiclesSaleService",
      "healthCheckUrl": "http://localhost:5004/health"
    }
  ]
}
```

---

## 🔄 HEALTH CHECK WORKER

### Funcionamiento
1. **Cada 30 segundos**: Hace request GET a `/health` de cada servicio
2. **Timeout**: 10 segundos
3. **Registro**: Guarda resultado en base de datos
4. **Alertas**: Si `ConsecutiveFailures >= 3`, publica evento de alerta

### Eventos Publicados

#### ServiceHealthChangedEvent
```csharp
public record ServiceHealthChangedEvent(
    string ServiceName,
    HealthStatus PreviousStatus,
    HealthStatus CurrentStatus,
    DateTime ChangedAt
);
```

**Consumidores:**
- **NotificationService**: Alerta a equipo de ops
- **AdminService**: Dashboard en tiempo real

---

## 📊 DASHBOARD (Propuesto)

El servicio puede proporcionar un dashboard HTML simple mostrando:

- 🟢 Servicios healthy
- 🟡 Servicios degraded
- 🔴 Servicios unhealthy
- Tiempo de respuesta por servicio
- Uptime percentage (últimas 24h)
- Historial de incidentes

---

## 📝 CASOS DE USO

### Monitoreo Proactivo
- Detectar servicios caídos antes que los usuarios
- Alertas automáticas a equipo de ops
- Dashboards de disponibilidad

### Debugging
- Identificar servicios lentos
- Correlacionar incidentes con deployments
- Análisis de tendencias de uptime

---

## 🚀 ALTERNATIVAS EN PRODUCCIÓN

En producción, en lugar de este servicio custom, se pueden usar:

- **Kubernetes Liveness/Readiness Probes**: Built-in health checks
- **Prometheus + Grafana**: Métricas y alertas
- **Uptime Robot / Pingdom**: Monitoreo externo
- **AWS CloudWatch**: Para infra en AWS

---

**Estado:** Solo desarrollo local - K8s maneja health checks en producción  
**Versión:** 1.0.0
