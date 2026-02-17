# HealthCheckService

Servicio centralizado de monitoreo y agregación de health checks para todos los microservicios del ecosistema CarDealer. Proporciona visibilidad en tiempo real del estado de salud del sistema completo.

## 🏗️ Arquitectura

El servicio sigue **Clean Architecture** con separación clara de responsabilidades:

```
HealthCheckService/
├── HealthCheckService.Domain/         # Entidades y lógica de negocio
│   ├── Entities/
│   │   ├── SystemHealth.cs            # Estado agregado del sistema
│   │   ├── ServiceHealth.cs           # Estado de un servicio individual
│   │   └── DependencyHealth.cs        # Estado de una dependencia
│   ├── Enums/
│   │   ├── HealthStatus.cs            # Healthy, Degraded, Unhealthy, Unknown
│   │   └── DependencyType.cs          # Database, Cache, MessageQueue, etc.
│   └── Interfaces/
│       ├── IHealthAggregator.cs       # Agregación de health checks
│       └── IHealthChecker.cs          # Verificación individual
│
├── HealthCheckService.Application/    # Lógica de aplicación y CQRS
│   ├── Queries/
│   │   └── HealthQueries.cs           # GetSystemHealth, GetServiceHealth
│   └── Handlers/
│       └── HealthQueryHandlers.cs     # Manejadores con MediatR
│
├── HealthCheckService.Infrastructure/ # Infraestructura técnica
│   ├── Services/
│   │   ├── HttpHealthChecker.cs       # Health check vía HTTP
│   │   └── HealthAggregatorService.cs # Agregación y caché
│   └── DependencyInjection.cs         # Registro de servicios
│
└── HealthCheckService.Api/            # API REST
    ├── Controllers/
    │   └── HealthController.cs        # Endpoints de health
    └── Program.cs                     # Configuración y arranque
```

## 🚀 Características

### Monitoreo Centralizado
- ✅ **Agregación de Health Checks**: Consolida el estado de todos los servicios
- ✅ **Verificación Paralela**: Checks simultáneos para respuesta rápida
- ✅ **Estados Jerárquicos**: Healthy, Degraded, Unhealthy, Unknown
- ✅ **Tiempo de Respuesta**: Métricas de latencia por servicio
- ✅ **Registro Dinámico**: Añadir/remover servicios sin reinicio

### Visibilidad del Sistema
- 📊 **Vista Agregada**: Estado general del sistema completo
- 🔍 **Vista por Servicio**: Detalle de cada microservicio
- 📈 **Métricas de Salud**: Porcentaje de disponibilidad
- ⏱️ **Timestamps**: Última verificación de cada servicio

### Integración
- 🔌 **RESTful API**: Endpoints HTTP estándar
- 📡 **Sin Base de Datos**: Stateless y ligero
- 🐳 **Docker Ready**: Contenedor optimizado
- 🌐 **CORS Habilitado**: Consumo desde frontend

## 📡 API REST

### Endpoints Principales

#### Obtener estado del sistema completo
```http
GET /api/health/system
```

**Respuesta (200 OK / 503 Service Unavailable):**
```json
{
  "overallStatus": "Healthy",
  "checkedAt": "2025-12-02T14:30:00Z",
  "totalServices": 5,
  "healthyServices": 4,
  "degradedServices": 1,
  "unhealthyServices": 0,
  "services": [
    {
      "serviceName": "ErrorService",
      "serviceUrl": "http://errorservice",
      "status": "Healthy",
      "description": "Service is healthy",
      "checkedAt": "2025-12-02T14:30:00Z",
      "responseTimeMs": 45,
      "dependencies": [],
      "metadata": {}
    },
    {
      "serviceName": "AuthService",
      "serviceUrl": "http://authservice",
      "status": "Degraded",
      "description": "Service returned ServiceUnavailable",
      "checkedAt": "2025-12-02T14:30:00Z",
      "responseTimeMs": 1200,
      "dependencies": [],
      "metadata": {}
    }
  ],
  "metadata": {}
}
```

#### Obtener estado de un servicio específico
```http
GET /api/health/service/{serviceName}
```

**Ejemplo:**
```http
GET /api/health/service/ErrorService
```

**Respuesta (200 OK / 404 Not Found / 503 Service Unavailable):**
```json
{
  "serviceName": "ErrorService",
  "serviceUrl": "http://errorservice",
  "status": "Healthy",
  "description": "Service is healthy",
  "checkedAt": "2025-12-02T14:30:00Z",
  "responseTimeMs": 45,
  "dependencies": [],
  "metadata": {}
}
```

#### Obtener servicios registrados
```http
GET /api/health/services
```

**Respuesta (200 OK):**
```json
[
  "ErrorService",
  "AuthService",
  "NotificationService",
  "SchedulerService",
  "AuditService"
]
```

#### Health check del servicio mismo
```http
GET /api/health
```

**Respuesta (200 OK):**
```json
{
  "status": "Healthy",
  "service": "HealthCheckService",
  "timestamp": "2025-12-02T14:30:00Z"
}
```

## 🎯 Estados de Salud

### HealthStatus Enum

| Estado | Código HTTP | Descripción |
|--------|-------------|-------------|
| `Healthy` | 200 | Servicio completamente operacional |
| `Degraded` | 200 | Servicio funcionando pero con funcionalidad reducida |
| `Unhealthy` | 503 | Servicio no operacional o inalcanzable |
| `Unknown` | 500 | Estado indeterminado (error en health check) |

### Lógica de Agregación

El estado general del sistema se calcula así:

1. **Unhealthy**: Si al menos un servicio está Unhealthy
2. **Degraded**: Si al menos un servicio está Degraded (y ninguno Unhealthy)
3. **Healthy**: Si todos los servicios están Healthy
4. **Unknown**: En cualquier otro caso

## 🔧 Configuración

### appsettings.json
```json
{
  "Services": {
    "ErrorService": "http://errorservice",
    "AuthService": "http://authservice",
    "NotificationService": "http://notificationservice",
    "SchedulerService": "http://schedulerservice",
    "AuditService": "http://auditservice"
  },
  "HealthCheck": {
    "CheckIntervalSeconds": 30,
    "TimeoutSeconds": 10
  }
}
```

### appsettings.Development.json
```json
{
  "Services": {
    "ErrorService": "http://localhost:15083",
    "AuthService": "http://localhost:15085",
    "NotificationService": "http://localhost:15086",
    "SchedulerService": "http://localhost:15091",
    "AuditService": "http://localhost:15082"
  },
  "HealthCheck": {
    "CheckIntervalSeconds": 10,
    "TimeoutSeconds": 5
  }
}
```

### Variables de Entorno (Docker)
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:80
Services__ErrorService=http://errorservice
Services__AuthService=http://authservice
Services__NotificationService=http://notificationservice
Services__SchedulerService=http://schedulerservice
Services__AuditService=http://auditservice
```

## 🐳 Docker

### Construcción
```bash
docker build -t healthcheckservice:latest -f Dockerfile .
```

### Ejecución con Docker Compose
```bash
# Desde el directorio backend/
docker-compose up -d healthcheckservice

# Ver logs
docker-compose logs -f healthcheckservice

# Detener
docker-compose down
```

El servicio estará disponible en:
- **API**: http://localhost:15092
- **System Health**: http://localhost:15092/api/health/system
- **Swagger**: http://localhost:15092/swagger

## 💻 Desarrollo Local

### Prerrequisitos
- .NET 8.0 SDK
- (Opcional) Docker Desktop

### Configuración y Ejecución

1. Restaurar paquetes:
```bash
dotnet restore
```

2. Ejecutar:
```bash
dotnet run --project HealthCheckService.Api
```

3. Acceder a Swagger:
```
http://localhost:5000/swagger
```

### Tests
```bash
# Ejecutar todos los tests
dotnet test

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Tests específicos
dotnet test --filter "FullyQualifiedName~SystemHealthTests"
```

## 📊 Integración con Servicios

### Requisito para Servicios Monitoreados

Cada servicio debe exponer un endpoint `/health` que retorne:

```http
GET /health
```

**Respuesta esperada (200 OK):**
```json
{
  "status": "Healthy",
  "name": "ServiceName",
  "timestamp": "2025-12-02T14:30:00Z"
}
```

### Ejemplo de Implementación en ASP.NET Core

```csharp
// En Program.cs de cada servicio
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "database")
    .AddRedis(redisConnection, name: "cache");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            name = "ServiceName",
            timestamp = DateTime.UtcNow
        });
        await context.Response.WriteAsync(result);
    }
});
```

## 🔍 Monitoreo y Observabilidad

### Métricas Expuestas

- **Total de Servicios**: Cantidad de servicios registrados
- **Servicios Healthy**: Conteo de servicios operacionales
- **Servicios Degraded**: Conteo de servicios con funcionalidad reducida
- **Servicios Unhealthy**: Conteo de servicios caídos
- **Health Percentage**: Porcentaje de disponibilidad del sistema
- **Response Time**: Latencia de cada servicio en milisegundos

### Logs Estructurados

El servicio genera logs detallados con información de:
- Inicio y fin de agregación de health checks
- Servicios registrados/desregistrados
- Errores en health checks individuales
- Tiempo de respuesta de cada servicio

## 🚨 Casos de Uso

### Dashboard de Monitoreo
```bash
# Polling periódico para dashboard
curl http://localhost:15092/api/health/system

# Mostrar estado visual basado en overallStatus
# - Healthy: ✅ Verde
# - Degraded: ⚠️ Amarillo
# - Unhealthy: ❌ Rojo
```

### Alertas Automáticas
```bash
# Script de monitoreo con alerta
#!/bin/bash
RESPONSE=$(curl -s http://localhost:15092/api/health/system)
STATUS=$(echo $RESPONSE | jq -r '.overallStatus')

if [ "$STATUS" = "Unhealthy" ]; then
  # Enviar alerta (email, Slack, PagerDuty, etc.)
  echo "🚨 SYSTEM UNHEALTHY!" | mail -s "Alert" ops@company.com
fi
```

### Verificación Pre-Deployment
```bash
# Antes de desplegar, verificar que todos los servicios estén healthy
HEALTH=$(curl -s http://localhost:15092/api/health/system | jq -r '.overallStatus')
if [ "$HEALTH" != "Healthy" ]; then
  echo "❌ Cannot deploy - system is not healthy"
  exit 1
fi
```

## 🛠️ Troubleshooting

### El servicio no detecta otros servicios

1. Verificar que los servicios estén en la misma red Docker:
```bash
docker network inspect cargurus-net
```

2. Verificar configuración de URLs en `appsettings.json`

3. Comprobar que los servicios tengan endpoint `/health`

### Timeout en health checks

- Aumentar `HealthCheck:TimeoutSeconds` en configuración
- Verificar latencia de red entre servicios
- Revisar logs del servicio que falla

### Estado siempre "Unknown"

- Verificar que el endpoint `/health` retorne 200 OK
- Comprobar formato de respuesta JSON
- Revisar logs de HealthCheckService para errores

## 📚 Stack Tecnológico

- **ASP.NET Core 8.0**: Framework web
- **MediatR 12.4.1**: Patrón CQRS para queries
- **HttpClient**: Health checks vía HTTP
- **xUnit**: Framework de testing
- **Swagger/OpenAPI**: Documentación interactiva

## 🔒 Seguridad

### Consideraciones

- El servicio no requiere autenticación por defecto (es interno)
- Para producción, considerar agregar autenticación API Key
- Limitar acceso a la red interna del cluster
- No exponer públicamente, usar solo en red privada

### Habilitar Autenticación (Opcional)

```csharp
// En Program.cs
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", options => {});

app.UseAuthentication();
app.UseAuthorization();
```

## 📄 Licencia

MIT License - Ver archivo LICENSE para más detalles.

## 🤝 Contribuir

1. Fork el proyecto
2. Crear una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir un Pull Request

## 📞 Soporte

Para reportar bugs o solicitar features, crear un issue en GitHub.

---

**Nota**: Este servicio es crítico para la observabilidad del sistema. Mantenerlo siempre actualizado y monitoreado.
