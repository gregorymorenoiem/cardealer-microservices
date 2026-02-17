# Gateway (Ocelot API Gateway) - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** Gateway
- **Tipo:** API Gateway (Ocelot)
- **Puerto en Kubernetes:** 8080
- **Puerto en Desarrollo:** 18443 (HTTPS)
- **Estado:** ✅ **EN PRODUCCIÓN**
- **URL Producción:** https://api.okla.com.do
- **Imagen Docker:** ghcr.io/gregorymorenoiem/cardealer-gateway:latest

### Propósito
API Gateway centralizado que actúa como punto de entrada único para todos los microservicios. Maneja enrutamiento, autenticación JWT, rate limiting, logging y métricas.

---

## 🏗️ ARQUITECTURA

### Clean Architecture Layers

```
Gateway/
├── Gateway.Api/                    # Capa de presentación
│   ├── Controllers/
│   │   └── HealthController.cs     # Health checks
│   ├── Middleware/
│   │   ├── RequestLoggingMiddleware.cs
│   │   ├── DownstreamCallMiddleware.cs
│   │   └── RateLimitingMiddleware.cs
│   ├── Program.cs                  # Entry point
│   ├── ocelot.dev.json             # Config para desarrollo (puerto 80)
│   ├── ocelot.prod.json            # Config para producción (puerto 8080)
│   ├── appsettings.json
│   └── Dockerfile
├── Gateway.Application/            # Casos de uso
│   └── UseCases/
│       ├── CheckRouteExistsUseCase.cs
│       ├── ResolveDownstreamPathUseCase.cs
│       ├── CheckServiceHealthUseCase.cs
│       ├── GetServicesHealthUseCase.cs
│       ├── RecordRequestMetricsUseCase.cs
│       └── RecordDownstreamCallMetricsUseCase.cs
├── Gateway.Domain/                 # Lógica de dominio
│   └── Interfaces/
│       ├── IRoutingService.cs
│       ├── IMetricsService.cs
│       └── IHealthCheckService.cs
└── Gateway.Infrastructure/         # Implementaciones
    └── Services/
        ├── RoutingService.cs
        ├── MetricsService.cs
        └── HealthCheckService.cs
```

---

## 🎯 FUNCIONALIDADES PRINCIPALES

### 1. Enrutamiento Dinámico (Ocelot)
- Configuración basada en JSON (`ocelot.dev.json` / `ocelot.prod.json`)
- Enrutamiento a microservicios por prefijo de ruta
- Reescritura de rutas upstream → downstream

### 2. Autenticación JWT
- Validación de tokens JWT en todas las rutas protegidas
- Bearer Token Authentication
- Issuer/Audience validation

### 3. Rate Limiting
- Límite de peticiones por cliente/IP
- Configuración por ruta

### 4. Logging y Métricas
- Serilog con TraceId/SpanId enrichment
- OpenTelemetry para distributed tracing
- Métricas de latencia y errores por ruta

### 5. Health Checks
- `/health` endpoint para monitoreo
- Health checks de servicios downstream

---

## 📡 CONFIGURACIÓN DE RUTAS (ocelot.prod.json)

### ⚠️ REGLA CRÍTICA
**TODOS los servicios downstream deben usar puerto 8080 en producción/Kubernetes.**

### Estructura de Ruta

```json
{
  "UpstreamPathTemplate": "/api/{service}/{everything}",
  "DownstreamPathTemplate": "/api/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [
    {
      "Host": "servicename",
      "Port": 8080  // ← SIEMPRE 8080 en Kubernetes
    }
  ]
}
```

### Servicios Configurados en Producción

| Upstream Path              | Downstream Service          | Puerto |
|----------------------------|-----------------------------|--------|
| `/api/auth/*`              | `authservice:8080`          | 8080   |
| `/api/users/*`             | `userservice:8080`          | 8080   |
| `/api/roles/*`             | `roleservice:8080`          | 8080   |
| `/api/vehicles/*`          | `vehiclessaleservice:8080`  | 8080   |
| `/api/catalog/*`           | `vehiclessaleservice:8080`  | 8080   |
| `/api/homepagesections/*`  | `vehiclessaleservice:8080`  | 8080   |
| `/api/media/*`             | `mediaservice:8080`         | 8080   |
| `/api/notifications/*`     | `notificationservice:8080`  | 8080   |
| `/api/billing/*`           | `billingservice:8080`       | 8080   |
| `/api/errors/*`            | `errorservice:8080`         | 8080   |

---

## 🔧 TECNOLOGÍAS Y DEPENDENCIAS

### Paquetes NuGet Principales

```xml
<PackageReference Include="Ocelot" Version="22.0.1" />
<PackageReference Include="Ocelot.Provider.Polly" Version="22.0.1" />
<PackageReference Include="MMLib.SwaggerForOcelot" Version="8.2.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Enrichers.Span" Version="3.1.0" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.7.0" />
<PackageReference Include="Consul" Version="1.7.14.3" />
```

### Servicios Externos
- **Consul** (opcional): Service Discovery
- **Jaeger/OTLP**: Distributed Tracing
- **Prometheus**: Métricas

---

## ⚙️ CONFIGURACIÓN (appsettings.json)

### Variables de Entorno Requeridas

```json
{
  "Jwt": {
    "Key": "${JWT_SECRET_KEY}",
    "Issuer": "okla-auth-service",
    "Audience": "okla-api",
    "ExpirationInMinutes": 1440
  },
  "OpenTelemetry": {
    "Exporter": {
      "Otlp": {
        "Endpoint": "http://localhost:4317"
      }
    }
  },
  "Consul": {
    "Host": "consul",
    "Port": 8500
  }
}
```

### Secrets de Kubernetes

```yaml
env:
  - name: Jwt__Key
    valueFrom:
      secretKeyRef:
        name: jwt-secret
        key: jwt-key
```

---

## 🔄 MIDDLEWARE PIPELINE

Orden de ejecución del middleware:

1. **Serilog Request Logging** - Logging de request/response
2. **Rate Limiting** - Control de tasa de peticiones
3. **JWT Authentication** - Validación de tokens
4. **Request Logging Middleware** - Logging personalizado con TraceId
5. **Downstream Call Middleware** - Métricas de llamadas a servicios
6. **Ocelot Middleware** - Enrutamiento y proxy
7. **Exception Handling** - Manejo global de errores

---

## 📊 MÉTRICAS Y OBSERVABILIDAD

### OpenTelemetry Traces
- **Sampler**: 10% en producción, 100% en desarrollo
- **Exporters**: OTLP (Jaeger/Tempo)
- **Instrumentación**:
  - ASP.NET Core requests
  - HttpClient calls (downstream)
  - Custom spans para enrutamiento

### Métricas Personalizadas
- `gateway_request_total` - Total de requests
- `gateway_request_duration_seconds` - Latencia por ruta
- `gateway_downstream_call_total` - Llamadas a servicios
- `gateway_downstream_call_duration_seconds` - Latencia downstream

### Logs Estructurados (Serilog)
```
[{Timestamp}] [{Level}] {Message} TraceId={TraceId} SpanId={SpanId}
```

---

## 🚀 DESPLIEGUE

### Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gateway
  namespace: okla
spec:
  replicas: 2
  selector:
    matchLabels:
      app: gateway
  template:
    spec:
      containers:
      - name: gateway
        image: ghcr.io/gregorymorenoiem/cardealer-gateway:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "http://+:8080"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

### ConfigMap para ocelot.json

```bash
kubectl create configmap gateway-config \
  --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json \
  -n okla
```

**IMPORTANTE:** Después de actualizar el ConfigMap, reiniciar el deployment:
```bash
kubectl rollout restart deployment/gateway -n okla
```

---

## 🐛 TROUBLESHOOTING

### 404 Not Found en Gateway

**Causa:** Ruta no existe en `ocelot.prod.json` o ConfigMap desactualizado.

**Solución:**
```bash
# 1. Verificar ConfigMap
kubectl get configmap gateway-config -n okla -o yaml

# 2. Actualizar ConfigMap
kubectl delete configmap gateway-config -n okla
kubectl create configmap gateway-config \
  --from-file=ocelot.json=ocelot.prod.json -n okla

# 3. Reiniciar Gateway
kubectl rollout restart deployment/gateway -n okla
```

### 503 Service Unavailable / Timeout

**Causa:** Puerto incorrecto en `DownstreamHostAndPorts` (debe ser 8080).

**Solución:**
```json
// ❌ INCORRECTO
"DownstreamHostAndPorts": [{ "Host": "vehiclessaleservice", "Port": 80 }]

// ✅ CORRECTO
"DownstreamHostAndPorts": [{ "Host": "vehiclessaleservice", "Port": 8080 }]
```

### JWT Token Inválido (401 Unauthorized)

**Causa:** Configuración de JWT no coincide con AuthService.

**Verificar:**
- `Jwt__Key` es idéntico en Gateway y AuthService
- `Jwt__Issuer` y `Jwt__Audience` coinciden

---

## 🔗 RELACIONES CON OTROS SERVICIOS

### Upstream (Clientes)
- **Frontend Web**: `https://okla.com.do` → `https://api.okla.com.do`
- **Frontend Mobile**: Flutter App → `https://api.okla.com.do`

### Downstream (Microservicios)
- **AuthService** - Autenticación y registro
- **UserService** - Gestión de usuarios
- **RoleService** - Roles y permisos
- **VehiclesSaleService** - Catálogo de vehículos
- **MediaService** - Imágenes y archivos
- **NotificationService** - Notificaciones
- **BillingService** - Pagos
- **ErrorService** - Registro de errores

---

## 📝 REGLAS DE NEGOCIO

### 1. Todas las rutas pasan por el Gateway
No se permite acceso directo a microservicios desde el cliente.

### 2. Autenticación JWT obligatoria
Excepto rutas públicas:
- `/health`
- `/api/auth/login`
- `/api/auth/register`
- `/api/vehicles` (GET público)

### 3. Rate Limiting por IP
- **Default:** 100 req/min por IP
- **Auth endpoints:** 20 req/min por IP

### 4. CORS configurado
- **Development:** AllowAnyOrigin
- **Production:** Solo dominios autorizados (`okla.com.do`)

---

## 📚 COMANDOS ÚTILES

### Desarrollo Local (Docker Compose)

```bash
# Levantar Gateway
docker-compose up -d gateway

# Ver logs en tiempo real
docker-compose logs -f gateway

# Verificar health
curl http://localhost:18443/health
```

### Producción (Kubernetes)

```bash
# Ver pods
kubectl get pods -n okla -l app=gateway

# Ver logs
kubectl logs -f deployment/gateway -n okla

# Port-forward para debugging
kubectl port-forward svc/gateway 8080:8080 -n okla

# Ejecutar comando en pod
kubectl exec -it deployment/gateway -n okla -- /bin/sh
```

### Verificar Conectividad Interna (desde Gateway)

```bash
# Probar conexión a servicio
kubectl exec -it deployment/gateway -n okla -- \
  wget -qO- http://vehiclessaleservice:8080/health
```

---

## 🔐 SEGURIDAD

### JWT Validation
- Validación de firma con clave secreta compartida
- Validación de Issuer y Audience
- Expiración automática de tokens

### HTTPS en Producción
- Ingress con certificado Let's Encrypt
- Redirect HTTP → HTTPS automático

### Rate Limiting
- Protección contra ataques DDoS
- Configuración por ruta y método HTTP

---

## 📅 ÚLTIMA ACTUALIZACIÓN

**Fecha:** Enero 7, 2026  
**Versión:** 1.0.0  
**Estado:** Producción estable en Digital Ocean Kubernetes (DOKS)

---

## 📖 REFERENCIAS

- [Documentación Ocelot](https://ocelot.readthedocs.io/)
- [Tutorial 11: Troubleshooting Gateway](../../docs/tutorials/11-troubleshooting-gateway.md)
- [Tutorial 15: Deploy Completo](../../docs/tutorials/15-deploy-completo-0-a-produccion.md)
- [GitHub Copilot Instructions](../../.github/copilot-instructions.md)
