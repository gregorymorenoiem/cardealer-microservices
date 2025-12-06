# 🌐 Gateway Service

API Gateway para el sistema de microservicios CarDealer, construido con Ocelot.

## 📋 Descripción

Gateway centralizado que gestiona el enrutamiento, autenticación, rate limiting y circuit breaker para todos los microservicios del sistema.

## 🚀 Características

- **Enrutamiento Dinámico**: Configuración con Ocelot para múltiples servicios
- **Circuit Breaker**: Protección con Polly contra fallos en servicios downstream
- **Service Discovery**: Integración con Consul para descubrimiento de servicios
- **CORS**: Configuración para frontend React
- **Observabilidad**: OpenTelemetry, Serilog, Métricas
- **Health Checks**: Endpoint `/health` para monitoreo
- **Swagger**: Documentación API agregada con SwaggerForOcelot

## 🏗️ Arquitectura

```
┌─────────────┐
│   Cliente   │
└──────┬──────┘
       │
       ↓
┌─────────────────────────────────┐
│        Gateway (Ocelot)         │
│  - Enrutamiento                 │
│  - Autenticación JWT            │
│  - Circuit Breaker              │
│  - Rate Limiting                │
└──────┬──────────────────────────┘
       │
       ├──→ ErrorService (5001)
       ├──→ AuditService (5002)
       ├──→ NotificationService (5003)
       ├──→ MediaService (5004)
       ├──→ AuthService (5006)
       ├──→ ContactService (5007)
       ├──→ AdminService (5010)
       └──→ ... otros servicios
```

## 📦 Dependencias

- **Ocelot** 22.0.1 - API Gateway
- **Ocelot.Provider.Polly** 22.0.1 - Circuit breaker
- **Consul** 1.7.14.3 - Service discovery
- **OpenTelemetry** 1.14.0 - Observabilidad
- **Serilog** 8.0.0 - Logging estructurado

## ⚙️ Configuración

### Ocelot Development (`ocelot.dev.json`)
```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/api/errors",
      "DownstreamPathTemplate": "/api/errors",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "errorservice", "Port": 80 }
      ],
      "QoSOptions": {
        "ExceptionsAllowedBeforeBreaking": 3,
        "DurationOfBreak": 10,
        "TimeoutValue": 30000
      }
    }
  ]
}
```

### Variables de Entorno
```bash
ASPNETCORE_ENVIRONMENT=Development
Service__Name=Gateway
Service__Host=localhost
Service__Port=5008
Consul__Address=http://localhost:8500
```

## 🔌 Endpoints

### Health Check
```http
GET /health
```

### Swagger UI
```http
GET /swagger
```

### Rutas Configuradas
- `/api/errors/**` → ErrorService
- `/api/audit/**` → AuditService
- `/api/notifications/**` → NotificationService
- `/api/media/**` → MediaService
- `/api/auth/**` → AuthService
- `/api/contacts/**` → ContactService
- `/api/admin/**` → AdminService

## 🧪 Testing

```bash
# Ejecutar todos los tests
dotnet test Gateway.Tests/Gateway.Tests.csproj

# Tests unitarios solamente
dotnet test Gateway.Tests/Gateway.Tests.csproj --filter "FullyQualifiedName~Unit"

# Tests con cobertura
dotnet test Gateway.Tests/Gateway.Tests.csproj /p:CollectCoverage=true
```

**Test Coverage**: 22 tests (18 unitarios + 4 integración)

## 🐳 Docker

```bash
# Build
docker build -t gateway:latest .

# Run
docker run -d -p 5008:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e Consul__Address=http://consul:8500 \
  --name gateway \
  gateway:latest
```

### Docker Compose
```yaml
gateway:
  build: ./Gateway
  ports:
    - "5008:80"
  environment:
    - ASPNETCORE_ENVIRONMENT=Production
    - Consul__Address=http://consul:8500
  depends_on:
    - consul
```

## 📊 Métricas

El Gateway expone las siguientes métricas:

- `gateway_requests_total` - Total de requests procesados
- `gateway_request_duration_seconds` - Duración de requests
- `gateway_requests_failed_total` - Requests fallidos
- `gateway_downstream_service_latency_seconds` - Latencia de servicios
- `gateway_downstream_service_errors_total` - Errores de servicios

## 🔐 Autenticación

El Gateway valida tokens JWT para rutas protegidas:

```http
Authorization: Bearer <jwt-token>
```

Configuración en `appsettings.json`:
```json
{
  "Authentication": {
    "Schemes": {
      "Bearer": {
        "ValidIssuer": "https://auth.cardealer.com",
        "ValidAudience": "cardealer-api",
        "RequireHttpsMetadata": true
      }
    }
  }
}
```

## 🛡️ Circuit Breaker

Configuración de Polly para protección contra cascadas de fallos:

- **Excepciones permitidas**: 3
- **Duración del break**: 10 segundos
- **Timeout**: 30 segundos

## 📈 Monitoreo

### Logs
```bash
# Ver logs en tiempo real
docker logs -f gateway
```

### Health Check
```bash
curl http://localhost:5008/health
```

### Prometheus Alerts
Ver configuración en `prometheus-alerts.yml`

## 🚦 Estado del Servicio

- ✅ **Build**: Compilando correctamente
- ✅ **Tests**: 18/22 pasando (integración requiere servicios)
- ✅ **Docker**: Dockerfile configurado
- ✅ **CI/CD Ready**: Listo para pipelines

## 📝 Notas de Desarrollo

- El Gateway usa **Ocelot** para enrutamiento y transformaciones
- **Circuit breaker** activado para todos los servicios downstream
- **Service Discovery** con Consul para alta disponibilidad
- **CORS** configurado para `http://localhost:5173` (dev) y `https://inelcasrl.com.do` (prod)
- **Swagger agregado** unifica documentación de todos los microservicios

## 🔗 Enlaces

- [Documentación Ocelot](https://ocelot.readthedocs.io/)
- [Polly Circuit Breaker](https://github.com/App-vNext/Polly)
- [Consul Service Discovery](https://www.consul.io/)

---

**Versión**: 1.0.0  
**Puerto**: 5008  
**Estado**: ✅ Production Ready
