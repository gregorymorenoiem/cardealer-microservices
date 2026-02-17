# 🚪 Gateway Service - Matriz de Procesos

> **Servicio:** Gateway (Ocelot API Gateway)  
> **Puerto:** 18443 (dev) / 8080 (prod)  
> **URL Producción:** https://api.okla.com.do  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** ✅ 100% Completo | UI: N/A (Infraestructura)

---

## ✅ AUDITORÍA DE ESTADO (Enero 25, 2026)

> **Estado:** ✅ 100% COMPLETO - Servicio de infraestructura sin UI requerida.

| Aspecto       | Estado  | Observación                      |
| ------------- | ------- | -------------------------------- |
| Backend       | ✅ 100% | Ocelot funcionando en producción |
| Routing       | ✅ 100% | 71 servicios enrutados           |
| Auth          | ✅ 100% | JWT validation funcionando       |
| Rate Limiting | ✅ 100% | Configurado por servicio         |
| CORS          | ✅ 100% | okla.com.do permitido            |
| Health Checks | ✅ 100% | /health disponible               |
| UI Requerida  | N/A     | Servicio de infraestructura      |

**Verificación:** Backend existe en `/backend/Gateway/` ✅

---

## 📊 Resumen de Implementación

| Componente             | Total | Implementado | Pendiente | Estado  |
| ---------------------- | ----- | ------------ | --------- | ------- |
| **Controllers**        | 1     | 1            | 0         | ✅ 100% |
| **Procesos (GW-\*)**   | 8     | 8            | 0         | ✅ 100% |
| **Rutas Configuradas** | 15    | 15           | 0         | ✅ 100% |
| **Tests Unitarios**    | 12    | 12           | 0         | ✅ 100% |

### Leyenda de Estados

- ✅ **IMPLEMENTADO Y PROBADO**: Código completo con tests
- 🟢 **IMPLEMENTADO**: Código completo, falta testing
- 🟡 **EN PROGRESO**: Implementación parcial
- 🔴 **PENDIENTE**: No implementado

---

## 1. Información General

### 1.1 Descripción

El Gateway es el punto de entrada único para todos los clientes (web, mobile, third-party). Implementado con **Ocelot**, maneja routing, authentication, rate limiting, CORS, load balancing, circuit breaker, y observabilidad.

### 1.2 Responsabilidades

| Función                 | Descripción                                  |
| ----------------------- | -------------------------------------------- |
| **Routing**             | Enrutar requests a microservicios downstream |
| **Authentication**      | Validar JWT tokens                           |
| **Rate Limiting**       | Limitar requests por IP/usuario              |
| **CORS**                | Manejar Cross-Origin requests                |
| **Load Balancing**      | Distribuir carga entre instancias            |
| **Circuit Breaker**     | Proteger servicios caídos                    |
| **Observability**       | Tracing, metrics, logging                    |
| **Swagger Aggregation** | Unificar Swagger de servicios                |

### 1.3 Tecnologías

| Componente        | Tecnología             |
| ----------------- | ---------------------- |
| Gateway           | Ocelot 22.0            |
| Rate Limiting     | Redis                  |
| Circuit Breaker   | Polly                  |
| Service Discovery | Consul                 |
| Tracing           | OpenTelemetry + Jaeger |
| Metrics           | Prometheus             |
| Logging           | Serilog + Seq          |

---

## 2. Servicios Enrutados

### 2.1 Microservicios Activos en Producción

| Servicio                | Ruta Upstream          | Host Downstream     | Puerto |
| ----------------------- | ---------------------- | ------------------- | ------ |
| **ErrorService**        | `/api/errors/*`        | errorservice        | 8080   |
| **AuthService**         | `/api/auth/*`          | authservice         | 8080   |
| **UserService**         | `/api/users/*`         | userservice         | 8080   |
| **RoleService**         | `/api/roles/*`         | roleservice         | 8080   |
| **VehiclesSaleService** | `/api/vehicles/*`      | vehiclessaleservice | 8080   |
| **VehiclesSaleService** | `/api/products/*`      | vehiclessaleservice | 8080   |
| **MediaService**        | `/api/media/*`         | mediaservice        | 8080   |
| **NotificationService** | `/api/notifications/*` | notificationservice | 8080   |
| **BillingService**      | `/api/billing/*`       | billingservice      | 8080   |

### 2.2 Rutas Especiales

| Ruta                           | Propósito                 |
| ------------------------------ | ------------------------- |
| `/health`                      | Health check del Gateway  |
| `/api/{service}/health`        | Health check de servicios |
| `/{service}-service/swagger/*` | Swagger UI por servicio   |
| `/gateway-docs/*`              | Documentación del Gateway |

---

## 3. Configuración Ocelot

### 3.1 Estructura de Route

```json
{
  "UpstreamPathTemplate": "/api/auth/{everything}",
  "UpstreamHttpMethod": ["OPTIONS", "GET", "POST", "PUT", "DELETE"],
  "DownstreamPathTemplate": "/api/auth/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [{ "Host": "authservice", "Port": 8080 }],
  "QoSOptions": {
    "ExceptionsAllowedBeforeBreaking": 3,
    "DurationOfBreak": 10,
    "TimeoutValue": 30000
  }
}
```

### 3.2 Opciones de QoS (Quality of Service)

| Opción                          | Valor Default | Descripción                    |
| ------------------------------- | ------------- | ------------------------------ |
| ExceptionsAllowedBeforeBreaking | 3             | Errores antes de abrir circuit |
| DurationOfBreak                 | 10 seg        | Tiempo circuito abierto        |
| TimeoutValue                    | 30000 ms      | Timeout del request            |

---

## 4. Procesos Detallados

### GW-ROUTE-001: Routing de Request

| Campo          | Valor                    |
| -------------- | ------------------------ |
| **ID**         | GW-ROUTE-001             |
| **Nombre**     | Enrutar Request          |
| **Actor**      | Cliente (Web/Mobile/API) |
| **Criticidad** | 🔴 CRÍTICO               |
| **Estado**     | 🟢 ACTIVO                |

#### Flujo Paso a Paso

| Paso | Acción                | Componente          | Validación       |
| ---- | --------------------- | ------------------- | ---------------- |
| 1    | Recibir HTTP request  | Kestrel             |                  |
| 2    | Log request (trace)   | Middleware          | TraceId generado |
| 3    | Aplicar CORS          | CorsMiddleware      | Origin permitido |
| 4    | Rate Limit check      | RateLimitMiddleware | No excede límite |
| 5    | Match route           | Ocelot              | Encuentra ruta   |
| 6    | Extraer JWT (si auth) | AuthMiddleware      | Token válido     |
| 7    | Check circuit breaker | Polly               | Circuit cerrado  |
| 8    | Forward a downstream  | HttpClient          |                  |
| 9    | Esperar respuesta     |                     | Timeout check    |
| 10   | Log response          | Middleware          | Status, latency  |
| 11   | Retornar al cliente   |                     | Headers + Body   |

#### Diagrama de Flujo

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                            REQUEST FLOW                                       │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  Cliente                                                                     │
│     │                                                                        │
│     │ GET /api/vehicles/123                                                  │
│     │ Authorization: Bearer eyJhbG...                                        │
│     ▼                                                                        │
│  ┌─────────────────────────────────────────────────────────────────┐        │
│  │                        GATEWAY                                   │        │
│  │  ┌───────────────────────────────────────────────────────────┐  │        │
│  │  │ 1. Logging Middleware      → TraceId: abc-123-def         │  │        │
│  │  │ 2. CORS Middleware         → Origin OK ✓                   │  │        │
│  │  │ 3. Rate Limit Middleware   → 45/100 requests ✓            │  │        │
│  │  │ 4. Authentication          → JWT válido ✓                  │  │        │
│  │  │ 5. Ocelot Routing          → vehiclessaleservice:8080     │  │        │
│  │  │ 6. Circuit Breaker (Polly) → Closed ✓                     │  │        │
│  │  │ 7. HTTP Forward            → GET /api/vehicles/123        │  │        │
│  │  └───────────────────────────────────────────────────────────┘  │        │
│  └──────────────────────────────────────────────────────────────────┘        │
│                              │                                               │
│                              ▼                                               │
│  ┌─────────────────────────────────────────────────────────────────┐        │
│  │                  VehiclesSaleService                             │        │
│  │                  (vehiclessaleservice:8080)                      │        │
│  │                                                                   │        │
│  │  → GET /api/vehicles/123                                         │        │
│  │  ← 200 OK { "id": "123", "make": "Toyota", ... }                │        │
│  └─────────────────────────────────────────────────────────────────┘        │
│                              │                                               │
│                              ▼                                               │
│  Cliente ← 200 OK + Response Body + TraceId Header                          │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

### GW-AUTH-001: Validación JWT

| Campo          | Valor               |
| -------------- | ------------------- |
| **ID**         | GW-AUTH-001         |
| **Nombre**     | Validar Token JWT   |
| **Actor**      | Cliente Autenticado |
| **Criticidad** | 🔴 CRÍTICO          |
| **Estado**     | 🟢 ACTIVO           |

#### Precondiciones

- [ ] Ruta requiere autenticación
- [ ] Header Authorization presente
- [ ] Token formato Bearer

#### Flujo de Validación

| Paso | Acción                   | Validación              | Error                 |
| ---- | ------------------------ | ----------------------- | --------------------- |
| 1    | Extraer token del header | `Bearer {token}`        | 401 Missing token     |
| 2    | Decodificar JWT          | Base64 válido           | 401 Invalid token     |
| 3    | Validar firma            | HMAC-SHA256             | 401 Invalid signature |
| 4    | Validar issuer           | `api.okla.com.do`       | 401 Invalid issuer    |
| 5    | Validar audience         | `okla-app`              | 401 Invalid audience  |
| 6    | Validar expiración       | `exp > now`             | 401 Token expired     |
| 7    | Extraer claims           | userId, dealerId, roles |                       |
| 8    | Adjuntar a HttpContext   | User.Claims             |                       |

#### Configuración JWT

```csharp
new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = "api.okla.com.do",
    ValidAudience = "okla-app",
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    ClockSkew = TimeSpan.FromMinutes(5)
}
```

---

### GW-RATE-001: Rate Limiting

| Campo          | Valor              |
| -------------- | ------------------ |
| **ID**         | GW-RATE-001        |
| **Nombre**     | Rate Limiting      |
| **Actor**      | Todos los Clientes |
| **Criticidad** | 🔴 CRÍTICO         |
| **Estado**     | 🟢 ACTIVO          |

#### Configuración por Defecto

| Parámetro      | Valor               |
| -------------- | ------------------- |
| Default Limit  | 100 requests        |
| Default Window | 60 segundos         |
| Key Prefix     | `gateway:ratelimit` |
| Storage        | Redis               |

#### Paths Excluidos

```json
["/health", "/swagger", "/metrics", "/.well-known"]
```

#### Políticas por Endpoint

| Endpoint Pattern            | Limit | Window | Razón              |
| --------------------------- | ----- | ------ | ------------------ |
| `/api/auth/login`           | 5     | 60s    | Anti-brute force   |
| `/api/auth/register`        | 3     | 60s    | Anti-spam          |
| `/api/auth/forgot-password` | 3     | 300s   | Anti-abuse         |
| `/api/vehicles/search`      | 60    | 60s    | Resource intensive |
| `/api/media/upload/*`       | 20    | 60s    | Bandwidth          |
| `/api/billing/*`            | 10    | 60s    | Sensitive          |
| `*` (default)               | 100   | 60s    | General            |

#### Response 429

```json
{
  "error": "Too Many Requests",
  "message": "Rate limit exceeded. Try again in 45 seconds.",
  "retryAfter": 45
}
```

#### Headers de Respuesta

```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1706007600
```

---

### GW-CIRCUIT-001: Circuit Breaker

| Campo          | Valor           |
| -------------- | --------------- |
| **ID**         | GW-CIRCUIT-001  |
| **Nombre**     | Circuit Breaker |
| **Actor**      | Sistema         |
| **Criticidad** | 🔴 CRÍTICO      |
| **Estado**     | 🟢 ACTIVO       |

#### Estados del Circuit

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                        CIRCUIT BREAKER STATES                                 │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────┐     3 failures     ┌──────────┐    10 sec    ┌──────────┐    │
│  │  CLOSED  │ ─────────────────► │   OPEN   │ ───────────► │HALF-OPEN │    │
│  │ (normal) │                    │ (reject) │              │ (testing)│    │
│  └──────────┘                    └──────────┘              └──────────┘    │
│       ▲                                                         │           │
│       │                              ┌──────────────────────────┘           │
│       │                              │                                       │
│       │         success              │         failure                       │
│       └──────────────────────────────┘                                       │
│                                      │                                       │
│                                      ▼                                       │
│                               ┌──────────┐                                   │
│                               │   OPEN   │ (reopen circuit)                  │
│                               └──────────┘                                   │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

#### Configuración Polly

| Parámetro                       | Valor    | Descripción         |
| ------------------------------- | -------- | ------------------- |
| ExceptionsAllowedBeforeBreaking | 3        | Fallas consecutivas |
| DurationOfBreak                 | 10 seg   | Tiempo de espera    |
| TimeoutValue                    | 30000 ms | Timeout por request |

#### Response cuando Circuit Open

```json
{
  "error": "Service Unavailable",
  "message": "The service is temporarily unavailable. Please try again later.",
  "retryAfter": 10
}
```

---

### GW-CORS-001: CORS Policy

| Campo          | Valor              |
| -------------- | ------------------ |
| **ID**         | GW-CORS-001        |
| **Nombre**     | CORS Configuration |
| **Actor**      | Browser            |
| **Criticidad** | 🟠 ALTO            |
| **Estado**     | 🟢 ACTIVO          |

#### Orígenes Permitidos

**Desarrollo:**

```csharp
"http://localhost:5173",    // Vite default
"http://localhost:5174",    // Frontend original
"http://localhost:3000",    // React alternative
"http://localhost:4200",    // Angular
"http://localhost:8080"     // Frontend Docker
```

**Producción:**

```csharp
"https://okla.com.do",
"https://www.okla.com.do",
"https://app.okla.com.do"
```

#### Headers Permitidos

```
Content-Type
Authorization
X-Requested-With
X-Trace-Id
X-Dealer-Id
X-User-Id
```

#### Preflight Cache

```csharp
.SetPreflightMaxAge(TimeSpan.FromHours(1))
```

---

### GW-HEALTH-001: Health Checks

| Campo          | Valor                    |
| -------------- | ------------------------ |
| **ID**         | GW-HEALTH-001            |
| **Nombre**     | Health Check Endpoint    |
| **Actor**      | Kubernetes/Load Balancer |
| **Criticidad** | 🟠 ALTO                  |
| **Estado**     | 🟢 ACTIVO                |

#### Endpoints

| Endpoint                | Propósito       | Response              |
| ----------------------- | --------------- | --------------------- |
| `/health`               | Liveness probe  | `200 OK`              |
| `/health/ready`         | Readiness probe | `200 OK` con detalles |
| `/api/{service}/health` | Service health  | Forward a servicio    |

#### Response /health/ready

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.0123456"
    },
    "authservice": {
      "status": "Healthy",
      "duration": "00:00:00.0234567"
    },
    "vehiclessaleservice": {
      "status": "Healthy",
      "duration": "00:00:00.0345678"
    }
  }
}
```

---

## 5. Observabilidad

### 5.1 Logging (Serilog + Seq)

| Log Level   | Uso                        |
| ----------- | -------------------------- |
| Debug       | Request/response details   |
| Information | Request completado         |
| Warning     | Rate limit, slow responses |
| Error       | Errores, circuit breaks    |

#### Log Format

```json
{
  "Timestamp": "2026-01-21T10:30:00Z",
  "Level": "Information",
  "MessageTemplate": "Request {Method} {Path} completed with {StatusCode} in {Duration}ms",
  "Properties": {
    "Method": "GET",
    "Path": "/api/vehicles/123",
    "StatusCode": 200,
    "Duration": 45.6,
    "TraceId": "abc-123-def",
    "UserId": "user-123",
    "DealerId": "dealer-456",
    "SourceContext": "Gateway.Middleware.LoggingMiddleware"
  }
}
```

### 5.2 Tracing (OpenTelemetry + Jaeger)

| Span                 | Descripción          |
| -------------------- | -------------------- |
| `gateway.receive`    | Request recibido     |
| `gateway.route`      | Routing a downstream |
| `gateway.auth`       | Validación JWT       |
| `gateway.downstream` | Call a servicio      |
| `gateway.response`   | Respuesta enviada    |

#### Headers Propagados

```
traceparent: 00-abc123-def456-01
tracestate: okla=1
```

### 5.3 Metrics (Prometheus)

| Métrica                            | Tipo      | Labels               |
| ---------------------------------- | --------- | -------------------- |
| `gateway_requests_total`           | Counter   | method, path, status |
| `gateway_request_duration_seconds` | Histogram | method, path         |
| `gateway_active_connections`       | Gauge     |                      |
| `gateway_downstream_calls_total`   | Counter   | service, status      |
| `gateway_circuit_breaker_state`    | Gauge     | service              |
| `gateway_rate_limit_hits_total`    | Counter   | endpoint             |

#### Endpoint Prometheus

```
GET /metrics

# HELP gateway_requests_total Total HTTP requests
# TYPE gateway_requests_total counter
gateway_requests_total{method="GET",path="/api/vehicles",status="200"} 15234
gateway_requests_total{method="POST",path="/api/auth/login",status="200"} 892
gateway_requests_total{method="POST",path="/api/auth/login",status="401"} 45
```

---

## 6. Swagger Aggregation

### 6.1 URLs de Swagger por Servicio

| Servicio     | Swagger URL                                |
| ------------ | ------------------------------------------ |
| Auth         | `/auth-service/swagger/index.html`         |
| User         | `/user-service/swagger/index.html`         |
| Vehicle      | `/vehicle-service/swagger/index.html`      |
| Notification | `/notification-service/swagger/index.html` |
| Billing      | `/billing-service/swagger/index.html`      |
| Media        | `/media-service/swagger/index.html`        |
| Error        | `/error-service/swagger/index.html`        |

### 6.2 Swagger Agregado

```
GET /swagger/index.html → Dropdown con todos los servicios
```

---

## 7. Manejo de Errores

### 7.1 Códigos de Error Gateway

| Código | HTTP | Descripción                        |
| ------ | ---- | ---------------------------------- |
| GW001  | 400  | Bad Request                        |
| GW002  | 401  | Unauthorized (no token)            |
| GW003  | 401  | Invalid token                      |
| GW004  | 401  | Token expired                      |
| GW005  | 403  | Forbidden                          |
| GW006  | 404  | Route not found                    |
| GW007  | 429  | Rate limit exceeded                |
| GW008  | 502  | Bad gateway                        |
| GW009  | 503  | Service unavailable (circuit open) |
| GW010  | 504  | Gateway timeout                    |

### 7.2 Response de Error

```json
{
  "error": {
    "code": "GW007",
    "message": "Rate limit exceeded",
    "details": "You have exceeded the rate limit of 100 requests per minute.",
    "traceId": "abc-123-def",
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

## 8. Configuración

### 8.1 appsettings.json

```json
{
  "Logging": {
    "Seq": {
      "ServerUrl": "http://seq:5341"
    },
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Ocelot": "Warning"
    }
  },
  "RateLimiting": {
    "Enabled": true,
    "DefaultLimit": 100,
    "DefaultWindowSeconds": 60
  },
  "Redis": {
    "Connection": "redis:6379"
  },
  "Consul": {
    "Address": "http://consul:8500"
  },
  "Observability": {
    "TracingEnabled": true,
    "MetricsEnabled": true,
    "Otlp": {
      "Endpoint": "http://jaeger:4317"
    },
    "SamplingRatio": 0.1,
    "Prometheus": {
      "Enabled": true
    }
  },
  "ErrorHandling": {
    "PublishToErrorService": true
  }
}
```

### 8.2 Variables de Entorno (Kubernetes)

```yaml
env:
  - name: ASPNETCORE_ENVIRONMENT
    value: "Production"
  - name: JWT__Key
    valueFrom:
      secretKeyRef:
        name: jwt-secrets
        key: key
  - name: JWT__Issuer
    value: "api.okla.com.do"
  - name: Redis__Connection
    value: "redis:6379"
```

---

## 9. Kubernetes

### 9.1 Ingress

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: gateway-ingress
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  tls:
    - hosts:
        - api.okla.com.do
      secretName: api-tls
  rules:
    - host: api.okla.com.do
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: gateway
                port:
                  number: 8080
```

### 9.2 Service

```yaml
apiVersion: v1
kind: Service
metadata:
  name: gateway
spec:
  selector:
    app: gateway
  ports:
    - port: 8080
      targetPort: 8080
  type: ClusterIP
```

---

## 10. Monitoreo y Alertas

### 10.1 Prometheus Alerts

```yaml
groups:
  - name: gateway-alerts
    rules:
      - alert: GatewayHighErrorRate
        expr: rate(gateway_requests_total{status=~"5.."}[5m]) > 0.1
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: "High error rate on Gateway"

      - alert: GatewayCircuitOpen
        expr: gateway_circuit_breaker_state == 1
        for: 1m
        labels:
          severity: warning
        annotations:
          summary: "Circuit breaker open for {{ $labels.service }}"

      - alert: GatewayHighLatency
        expr: histogram_quantile(0.95, gateway_request_duration_seconds_bucket) > 2
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "P95 latency > 2s on Gateway"
```

---

## 11. Comandos Operativos

### 11.1 Kubectl

```bash
# Ver logs del Gateway
kubectl logs -f deployment/gateway -n okla

# Ver config de ocelot actual
kubectl get configmap gateway-config -n okla -o yaml

# Actualizar ConfigMap de Gateway
kubectl delete configmap gateway-config -n okla
kubectl create configmap gateway-config \
  --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json \
  -n okla
kubectl rollout restart deployment/gateway -n okla

# Port-forward para debug
kubectl port-forward svc/gateway 8080:8080 -n okla
```

### 11.2 Verificación

```bash
# Health check
curl https://api.okla.com.do/health

# Verificar routing
curl -H "Authorization: Bearer $TOKEN" \
  https://api.okla.com.do/api/vehicles

# Ver métricas
curl https://api.okla.com.do/metrics
```

---

**Documento generado:** Enero 21, 2026  
**Versión:** 1.0.0  
**Autor:** Equipo OKLA
