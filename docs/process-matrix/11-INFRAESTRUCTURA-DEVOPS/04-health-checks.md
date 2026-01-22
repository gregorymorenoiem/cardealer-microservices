# ❤️ Health Checks - Monitoreo de Servicios - Matriz de Procesos

> **Componente:** Health Check System  
> **Framework:** ASP.NET Core Health Checks  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 1. Información General

### 1.1 Descripción

Sistema de health checks que monitorea el estado de todos los microservicios, sus dependencias y la salud general de la plataforma OKLA. Utilizado por Kubernetes para readiness/liveness probes y por el equipo de operaciones para monitoreo.

### 1.2 Tipos de Health Checks

| Tipo          | Descripción                       | Uso                       |
| ------------- | --------------------------------- | ------------------------- |
| **Liveness**  | El servicio está vivo             | Kubernetes restart        |
| **Readiness** | El servicio puede recibir tráfico | Kubernetes load balancing |
| **Startup**   | El servicio ha iniciado           | Kubernetes startup probe  |
| **Deep**      | Todas las dependencias            | Diagnóstico detallado     |

### 1.3 Dependencias

| Servicio                   | Propósito      |
| -------------------------- | -------------- |
| ASP.NET Health Checks      | Framework base |
| AspNetCore.HealthChecks.\* | Extensiones    |
| Prometheus                 | Métricas       |
| Grafana                    | Visualización  |
| AlertManager               | Alertas        |

---

## 2. Endpoints

### 2.1 Health Check Endpoints

| Método | Endpoint          | Descripción         | Auth |
| ------ | ----------------- | ------------------- | ---- |
| `GET`  | `/health`         | Health check básico | ❌   |
| `GET`  | `/health/live`    | Liveness probe      | ❌   |
| `GET`  | `/health/ready`   | Readiness probe     | ❌   |
| `GET`  | `/health/startup` | Startup probe       | ❌   |
| `GET`  | `/health/deep`    | Check detallado     | ❌   |

### 2.2 Health Check Responses

#### Healthy Response

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "self": {
      "status": "Healthy",
      "duration": "00:00:00.0001234"
    },
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0123456",
      "data": {
        "server": "postgres-0.okla.internal",
        "database": "vehiclessaleservice"
      }
    },
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.0054321"
    }
  }
}
```

#### Unhealthy Response

```json
{
  "status": "Unhealthy",
  "totalDuration": "00:00:05.1234567",
  "entries": {
    "database": {
      "status": "Unhealthy",
      "duration": "00:00:05.0000000",
      "exception": "Connection timeout",
      "data": {
        "server": "postgres-0.okla.internal",
        "lastError": "Unable to connect"
      }
    }
  }
}
```

---

## 3. Health Checks por Servicio

### 3.1 Gateway (Ocelot)

```csharp
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddRedis(Configuration["Redis:ConnectionString"], name: "redis")
    .AddCheck<DownstreamServicesHealthCheck>("downstream-services");
```

| Check                 | Descripción            | Crítico |
| --------------------- | ---------------------- | ------- |
| `self`                | El gateway está vivo   | ✅      |
| `redis`               | Cache de rate limiting | ✅      |
| `downstream-services` | Servicios backend      | ⚠️      |

### 3.2 AuthService

```csharp
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddNpgSql(Configuration["ConnectionStrings:DefaultConnection"], name: "database")
    .AddRedis(Configuration["Redis:ConnectionString"], name: "redis-session")
    .AddRabbitMQ(Configuration["RabbitMQ:ConnectionString"], name: "rabbitmq");
```

| Check           | Descripción           | Crítico |
| --------------- | --------------------- | ------- |
| `self`          | El servicio está vivo | ✅      |
| `database`      | PostgreSQL usuarios   | ✅      |
| `redis-session` | Cache de sesiones     | ✅      |
| `rabbitmq`      | Message broker        | ✅      |

### 3.3 VehiclesSaleService

```csharp
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddNpgSql(Configuration["ConnectionStrings:DefaultConnection"], name: "database")
    .AddRedis(Configuration["Redis:ConnectionString"], name: "redis-cache")
    .AddRabbitMQ(Configuration["RabbitMQ:ConnectionString"], name: "rabbitmq")
    .AddElasticsearch(Configuration["Elasticsearch:Url"], name: "elasticsearch");
```

| Check           | Descripción           | Crítico |
| --------------- | --------------------- | ------- |
| `self`          | El servicio está vivo | ✅      |
| `database`      | PostgreSQL vehículos  | ✅      |
| `redis-cache`   | Cache de vehículos    | ⚠️      |
| `rabbitmq`      | Message broker        | ⚠️      |
| `elasticsearch` | Motor de búsqueda     | ⚠️      |

### 3.4 BillingService

```csharp
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddNpgSql(Configuration["ConnectionStrings:DefaultConnection"], name: "database")
    .AddRedis(Configuration["Redis:ConnectionString"], name: "redis")
    .AddRabbitMQ(Configuration["RabbitMQ:ConnectionString"], name: "rabbitmq")
    .AddCheck<StripeHealthCheck>("stripe-api")
    .AddCheck<AzulHealthCheck>("azul-api");
```

| Check        | Descripción           | Crítico |
| ------------ | --------------------- | ------- |
| `self`       | El servicio está vivo | ✅      |
| `database`   | PostgreSQL pagos      | ✅      |
| `stripe-api` | API de Stripe         | ⚠️      |
| `azul-api`   | API de Azul           | ⚠️      |

### 3.5 NotificationService

```csharp
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddNpgSql(Configuration["ConnectionStrings:DefaultConnection"], name: "database")
    .AddRabbitMQ(Configuration["RabbitMQ:ConnectionString"], name: "rabbitmq")
    .AddCheck<SmtpHealthCheck>("smtp")
    .AddCheck<TwilioHealthCheck>("twilio");
```

| Check    | Descripción           | Crítico |
| -------- | --------------------- | ------- |
| `self`   | El servicio está vivo | ✅      |
| `smtp`   | Servidor de email     | ⚠️      |
| `twilio` | SMS/WhatsApp          | ⚠️      |

### 3.6 MediaService

```csharp
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddNpgSql(Configuration["ConnectionStrings:DefaultConnection"], name: "database")
    .AddRedis(Configuration["Redis:ConnectionString"], name: "redis-queue")
    .AddCheck<S3HealthCheck>("digitalocean-spaces");
```

| Check                 | Descripción           | Crítico |
| --------------------- | --------------------- | ------- |
| `self`                | El servicio está vivo | ✅      |
| `digitalocean-spaces` | Object storage        | ✅      |
| `redis-queue`         | Cola de procesamiento | ⚠️      |

---

## 4. Custom Health Checks

### 4.1 Downstream Services Check

```csharp
public class DownstreamServicesHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var services = new[]
        {
            ("authservice", "http://authservice:8080/health"),
            ("userservice", "http://userservice:8080/health"),
            ("vehiclessaleservice", "http://vehiclessaleservice:8080/health"),
        };

        var results = new Dictionary<string, object>();
        var unhealthyCount = 0;

        foreach (var (name, url) in services)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = await client.GetAsync(url, cancellationToken);

                results[name] = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy";
                if (!response.IsSuccessStatusCode) unhealthyCount++;
            }
            catch (Exception ex)
            {
                results[name] = $"Unhealthy: {ex.Message}";
                unhealthyCount++;
            }
        }

        if (unhealthyCount == 0)
            return HealthCheckResult.Healthy("All downstream services are healthy", results);

        if (unhealthyCount < services.Length / 2)
            return HealthCheckResult.Degraded($"{unhealthyCount} services unhealthy", data: results);

        return HealthCheckResult.Unhealthy($"{unhealthyCount} services unhealthy", data: results);
    }
}
```

### 4.2 Stripe Health Check

```csharp
public class StripeHealthCheck : IHealthCheck
{
    private readonly IStripeClient _stripeClient;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple API call to verify connectivity
            var service = new BalanceService(_stripeClient);
            var balance = await service.GetAsync(cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("Stripe API is reachable", new Dictionary<string, object>
            {
                ["available"] = balance.Available?.FirstOrDefault()?.Amount ?? 0
            });
        }
        catch (StripeException ex)
        {
            return HealthCheckResult.Degraded($"Stripe API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Stripe API unreachable: {ex.Message}");
        }
    }
}
```

### 4.3 Azul Health Check

```csharp
public class AzulHealthCheck : IHealthCheck
{
    private readonly IAzulPaymentService _azulService;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isAvailable = await _azulService.PingAsync(cancellationToken);

            if (isAvailable)
                return HealthCheckResult.Healthy("Azul API is reachable");

            return HealthCheckResult.Degraded("Azul API responded but may be degraded");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Azul API unreachable: {ex.Message}");
        }
    }
}
```

### 4.4 Database Connection Pool Check

```csharp
public class DatabasePoolHealthCheck : IHealthCheck
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var dbContext = _contextFactory.CreateDbContext();
            var connection = dbContext.Database.GetDbConnection();

            await connection.OpenAsync(cancellationToken);

            var poolInfo = new Dictionary<string, object>
            {
                ["state"] = connection.State.ToString(),
                ["database"] = connection.Database,
                ["server"] = connection.DataSource
            };

            return HealthCheckResult.Healthy("Database connection pool healthy", poolInfo);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database connection failed: {ex.Message}");
        }
    }
}
```

---

## 5. Kubernetes Probes

### 5.1 Configuración en Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vehiclessaleservice
  namespace: okla
spec:
  template:
    spec:
      containers:
        - name: vehiclessaleservice
          image: ghcr.io/okla/vehiclessaleservice:latest
          ports:
            - containerPort: 8080

          # Startup probe - espera hasta 5 minutos para iniciar
          startupProbe:
            httpGet:
              path: /health/startup
              port: 8080
            initialDelaySeconds: 10
            periodSeconds: 10
            timeoutSeconds: 5
            failureThreshold: 30

          # Liveness probe - reinicia si no responde
          livenessProbe:
            httpGet:
              path: /health/live
              port: 8080
            initialDelaySeconds: 0
            periodSeconds: 10
            timeoutSeconds: 5
            failureThreshold: 3

          # Readiness probe - quita del load balancer si no está listo
          readinessProbe:
            httpGet:
              path: /health/ready
              port: 8080
            initialDelaySeconds: 5
            periodSeconds: 5
            timeoutSeconds: 3
            failureThreshold: 3
```

### 5.2 Comportamiento de Probes

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Kubernetes Probe Behavior                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Pod Startup                                                           │
│   ═══════════                                                           │
│   ┌───────────────────────────────────────────────────────────────────┐ │
│   │ 1. Container starts                                                │ │
│   │ 2. Startup probe begins (waits for app to initialize)            │ │
│   │ 3. If startup probe fails 30 times → Container killed & restarted│ │
│   │ 4. If startup probe succeeds → Liveness & Readiness begin        │ │
│   └───────────────────────────────────────────────────────────────────┘ │
│                                                                          │
│   Normal Operation                                                      │
│   ════════════════                                                      │
│   ┌───────────────────────────────────────────────────────────────────┐ │
│   │                                                                    │ │
│   │   Liveness Probe (every 10s)         Readiness Probe (every 5s)  │ │
│   │   ┌─────────────────────────┐        ┌─────────────────────────┐ │ │
│   │   │ Healthy? ───▶ Continue  │        │ Ready? ───▶ Receive     │ │ │
│   │   │                         │        │             Traffic      │ │ │
│   │   │ Unhealthy (3x)?         │        │                         │ │ │
│   │   │    ▼                    │        │ Not Ready (3x)?         │ │ │
│   │   │ Kill & Restart Pod      │        │    ▼                    │ │ │
│   │   │                         │        │ Remove from Service     │ │ │
│   │   │                         │        │ (no traffic until ready)│ │ │
│   │   └─────────────────────────┘        └─────────────────────────┘ │ │
│   │                                                                    │ │
│   └───────────────────────────────────────────────────────────────────┘ │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Health Check UI

### 6.1 Configuración de UI

```csharp
// En Gateway o servicio dedicado de monitoring
services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(30);
    options.MaximumHistoryEntriesPerEndpoint(60);

    // Agregar todos los servicios
    options.AddHealthCheckEndpoint("Gateway", "http://gateway:8080/health/deep");
    options.AddHealthCheckEndpoint("Auth Service", "http://authservice:8080/health/deep");
    options.AddHealthCheckEndpoint("User Service", "http://userservice:8080/health/deep");
    options.AddHealthCheckEndpoint("Vehicles Service", "http://vehiclessaleservice:8080/health/deep");
    options.AddHealthCheckEndpoint("Billing Service", "http://billingservice:8080/health/deep");
    options.AddHealthCheckEndpoint("Notification Service", "http://notificationservice:8080/health/deep");
    options.AddHealthCheckEndpoint("Media Service", "http://mediaservice:8080/health/deep");
})
.AddInMemoryStorage();

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
    options.ApiPath = "/health-api";
});
```

### 6.2 Dashboard Visual

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        OKLA Health Dashboard                             │
│                    Last updated: 2026-01-21 10:30:00                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Overall Status: 🟢 HEALTHY                                              │
│                                                                          │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Service                │ Status  │ Duration │ Last Check          │  │
│  ├────────────────────────┼─────────┼──────────┼─────────────────────┤  │
│  │ 🟢 Gateway             │ Healthy │ 12ms     │ 10:30:00            │  │
│  │ 🟢 Auth Service        │ Healthy │ 45ms     │ 10:30:00            │  │
│  │ 🟢 User Service        │ Healthy │ 38ms     │ 10:30:00            │  │
│  │ 🟢 Vehicles Service    │ Healthy │ 89ms     │ 10:30:00            │  │
│  │ 🟡 Billing Service     │ Degraded│ 234ms    │ 10:30:00            │  │
│  │ 🟢 Notification Service│ Healthy │ 56ms     │ 10:30:00            │  │
│  │ 🟢 Media Service       │ Healthy │ 78ms     │ 10:30:00            │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  ⚠️ Billing Service Degraded:                                           │
│     - Azul API: Response time > 200ms                                   │
│                                                                          │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Dependency Health                                                  │  │
│  ├───────────────────────────────────────────────────────────────────┤  │
│  │ 🟢 PostgreSQL (primary)     │ 🟢 Redis                            │  │
│  │ 🟢 PostgreSQL (replica)     │ 🟢 RabbitMQ                         │  │
│  │ 🟢 Elasticsearch            │ 🟡 Azul API                         │  │
│  │ 🟢 DigitalOcean Spaces      │ 🟢 Stripe API                       │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 7. Alertas

### 7.1 Prometheus Alert Rules

```yaml
groups:
  - name: okla-health-alerts
    rules:
      # Servicio no saludable
      - alert: ServiceUnhealthy
        expr: health_check_status{status="Unhealthy"} == 1
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Service {{ $labels.service }} is unhealthy"
          description: "{{ $labels.service }} health check has been failing for more than 1 minute"

      # Servicio degradado
      - alert: ServiceDegraded
        expr: health_check_status{status="Degraded"} == 1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Service {{ $labels.service }} is degraded"
          description: "{{ $labels.service }} has been in degraded state for more than 5 minutes"

      # Health check timeout
      - alert: HealthCheckTimeout
        expr: health_check_duration_seconds > 5
        for: 2m
        labels:
          severity: warning
        annotations:
          summary: "Health check for {{ $labels.service }} is slow"
          description: "Health check taking more than 5 seconds"

      # Database connection issues
      - alert: DatabaseConnectionPoolExhausted
        expr: health_check_dependency_status{dependency="database", status="Unhealthy"} == 1
        for: 30s
        labels:
          severity: critical
        annotations:
          summary: "Database connection pool exhausted for {{ $labels.service }}"

      # Redis unavailable
      - alert: RedisUnavailable
        expr: health_check_dependency_status{dependency="redis", status="Unhealthy"} == 1
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Redis unavailable for {{ $labels.service }}"
```

### 7.2 PagerDuty Integration

```json
{
  "receivers": [
    {
      "name": "pagerduty-critical",
      "pagerduty_configs": [
        {
          "service_key": "${PAGERDUTY_SERVICE_KEY}",
          "severity": "critical"
        }
      ]
    },
    {
      "name": "slack-warnings",
      "slack_configs": [
        {
          "api_url": "${SLACK_WEBHOOK_URL}",
          "channel": "#okla-alerts",
          "text": "{{ .CommonAnnotations.summary }}"
        }
      ]
    }
  ],
  "route": {
    "receiver": "slack-warnings",
    "routes": [
      {
        "match": { "severity": "critical" },
        "receiver": "pagerduty-critical"
      }
    ]
  }
}
```

---

## 8. Métricas Prometheus

```
# Health check status (0 = Healthy, 1 = Degraded, 2 = Unhealthy)
health_check_status{service="...", check="..."}

# Health check duration
health_check_duration_seconds{service="...", check="..."}

# Dependency status
health_check_dependency_status{service="...", dependency="...", status="..."}

# Health check execution count
health_check_executions_total{service="...", result="..."}
```

---

## 9. Configuración

```json
{
  "HealthChecks": {
    "Enabled": true,
    "DetailedErrors": false,
    "Timeout": "00:00:05",
    "CacheDuration": "00:00:05",
    "UI": {
      "Enabled": true,
      "Path": "/health-ui",
      "EvaluationInterval": 30
    },
    "Endpoints": {
      "Basic": "/health",
      "Liveness": "/health/live",
      "Readiness": "/health/ready",
      "Startup": "/health/startup",
      "Deep": "/health/deep"
    }
  }
}
```

---

## 📚 Referencias

- [ASP.NET Health Checks](https://docs.microsoft.com/aspnet/core/host-and-deploy/health-checks) - Documentación oficial
- [05-monitoring.md](05-monitoring.md) - Monitoreo general
- [04-logging-service.md](04-logging-service.md) - Logging centralizado
