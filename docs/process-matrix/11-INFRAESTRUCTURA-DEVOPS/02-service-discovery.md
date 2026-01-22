# 🔍 Service Discovery - Descubrimiento de Servicios - Matriz de Procesos

> **Servicio:** Infrastructure / Consul  
> **Puerto:** 8500 (Consul UI)  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 1. Información General

### 1.1 Descripción

Sistema de descubrimiento de servicios basado en Consul para la plataforma OKLA. Permite que los microservicios se registren automáticamente y descubran otros servicios sin configuración manual de endpoints. Incluye health checks, load balancing, y DNS integrado.

### 1.2 Componentes

| Componente        | Descripción                            |
| ----------------- | -------------------------------------- |
| **Consul Server** | Cluster de servidores Consul (3 nodos) |
| **Consul Agent**  | Sidecar en cada servicio               |
| **Health Checks** | Verificación de salud de servicios     |
| **DNS Interface** | Resolución de servicios via DNS        |
| **KV Store**      | Almacén de configuración               |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Service Discovery Architecture                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│                        Consul Server Cluster                            │
│   ┌──────────────────────────────────────────────────────────────────┐  │
│   │                                                                   │  │
│   │   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │  │
│   │   │   consul-1   │  │   consul-2   │  │   consul-3   │          │  │
│   │   │   (Leader)   │◄─┤   (Follower) │◄─┤   (Follower) │          │  │
│   │   └──────────────┘  └──────────────┘  └──────────────┘          │  │
│   │          │                  │                  │                 │  │
│   │          └──────────────────┴──────────────────┘                 │  │
│   │                         Raft Consensus                           │  │
│   └──────────────────────────────────────────────────────────────────┘  │
│                                   │                                      │
│                                   │                                      │
│   ┌───────────────────────────────┴───────────────────────────────────┐ │
│   │                                                                    │ │
│   │   ┌─────────────────────────────────────────────────────────────┐ │ │
│   │   │                     Service Registration                     │ │ │
│   │   └─────────────────────────────────────────────────────────────┘ │ │
│   │                                                                    │ │
│   │   AuthService        UserService       VehiclesSaleService        │ │
│   │   ┌──────────────┐   ┌──────────────┐   ┌──────────────┐         │ │
│   │   │   Service    │   │   Service    │   │   Service    │         │ │
│   │   │   Instance   │   │   Instance   │   │   Instance   │         │ │
│   │   │   + Agent    │   │   + Agent    │   │   + Agent    │         │ │
│   │   └──────────────┘   └──────────────┘   └──────────────┘         │ │
│   │         │                   │                   │                 │ │
│   │         └───────────────────┴───────────────────┘                 │ │
│   │                             │                                      │ │
│   │                             ▼                                      │ │
│   │   ┌─────────────────────────────────────────────────────────────┐ │ │
│   │   │                   Service Discovery                          │ │ │
│   │   │                                                              │ │ │
│   │   │   DNS: authservice.service.consul → 10.0.1.5:8080           │ │ │
│   │   │   HTTP: GET /v1/catalog/service/authservice                 │ │ │
│   │   └─────────────────────────────────────────────────────────────┘ │ │
│   │                                                                    │ │
│   └────────────────────────────────────────────────────────────────────┘│
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Configuración de Servicios

### 2.1 Service Registration en .NET

```csharp
// Program.cs - Configuración de Consul
builder.Services.AddConsulServiceDiscovery(options =>
{
    options.ServiceName = "authservice";
    options.ServiceId = $"authservice-{Environment.MachineName}";
    options.Port = 8080;
    options.Tags = new[] { "api", "auth", "v1" };
    options.HealthCheckPath = "/health";
    options.HealthCheckInterval = TimeSpan.FromSeconds(10);
    options.DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(5);
});

// Extension method
public static class ConsulExtensions
{
    public static IServiceCollection AddConsulServiceDiscovery(
        this IServiceCollection services,
        Action<ConsulOptions> configure)
    {
        var options = new ConsulOptions();
        configure(options);

        services.AddSingleton<IConsulClient, ConsulClient>(sp =>
        {
            return new ConsulClient(config =>
            {
                config.Address = new Uri("http://consul:8500");
            });
        });

        services.AddHostedService<ConsulHostedService>(sp =>
        {
            var client = sp.GetRequiredService<IConsulClient>();
            var logger = sp.GetRequiredService<ILogger<ConsulHostedService>>();
            return new ConsulHostedService(client, options, logger);
        });

        return services;
    }
}

// Hosted service for registration/deregistration
public class ConsulHostedService : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly ConsulOptions _options;
    private readonly ILogger<ConsulHostedService> _logger;

    public async Task StartAsync(CancellationToken ct)
    {
        var registration = new AgentServiceRegistration
        {
            ID = _options.ServiceId,
            Name = _options.ServiceName,
            Address = GetLocalIpAddress(),
            Port = _options.Port,
            Tags = _options.Tags,
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{GetLocalIpAddress()}:{_options.Port}{_options.HealthCheckPath}",
                Interval = _options.HealthCheckInterval,
                DeregisterCriticalServiceAfter = _options.DeregisterCriticalServiceAfter
            }
        };

        _logger.LogInformation("Registering service {ServiceId} with Consul", _options.ServiceId);
        await _consulClient.Agent.ServiceRegister(registration, ct);
    }

    public async Task StopAsync(CancellationToken ct)
    {
        _logger.LogInformation("Deregistering service {ServiceId} from Consul", _options.ServiceId);
        await _consulClient.Agent.ServiceDeregister(_options.ServiceId, ct);
    }
}
```

### 2.2 Service Discovery Client

```csharp
public interface IServiceDiscovery
{
    Task<ServiceEndpoint?> GetServiceAsync(string serviceName, CancellationToken ct = default);
    Task<IEnumerable<ServiceEndpoint>> GetAllInstancesAsync(string serviceName, CancellationToken ct = default);
}

public class ConsulServiceDiscovery : IServiceDiscovery
{
    private readonly IConsulClient _consulClient;
    private readonly IMemoryCache _cache;

    public async Task<ServiceEndpoint?> GetServiceAsync(string serviceName, CancellationToken ct = default)
    {
        // Check cache first
        if (_cache.TryGetValue($"service:{serviceName}", out ServiceEndpoint? cached))
        {
            return cached;
        }

        var services = await _consulClient.Health.Service(serviceName, tag: null, passingOnly: true, ct);

        if (services.Response.Length == 0)
        {
            return null;
        }

        // Round-robin load balancing
        var index = GetNextIndex(serviceName, services.Response.Length);
        var service = services.Response[index];

        var endpoint = new ServiceEndpoint
        {
            Host = service.Service.Address,
            Port = service.Service.Port,
            Tags = service.Service.Tags
        };

        // Cache for 30 seconds
        _cache.Set($"service:{serviceName}", endpoint, TimeSpan.FromSeconds(30));

        return endpoint;
    }

    public async Task<IEnumerable<ServiceEndpoint>> GetAllInstancesAsync(string serviceName, CancellationToken ct = default)
    {
        var services = await _consulClient.Health.Service(serviceName, tag: null, passingOnly: true, ct);

        return services.Response.Select(s => new ServiceEndpoint
        {
            Host = s.Service.Address,
            Port = s.Service.Port,
            Tags = s.Service.Tags,
            InstanceId = s.Service.ID
        });
    }
}

public class ServiceEndpoint
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string? InstanceId { get; set; }

    public Uri ToUri() => new Uri($"http://{Host}:{Port}");
}
```

### 2.3 HttpClient con Service Discovery

```csharp
// Typed HttpClient que usa service discovery
public class UserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IServiceDiscovery _discovery;

    public UserServiceClient(HttpClient httpClient, IServiceDiscovery discovery)
    {
        _httpClient = httpClient;
        _discovery = discovery;
    }

    public async Task<UserDto?> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var endpoint = await _discovery.GetServiceAsync("userservice", ct);
        if (endpoint == null)
        {
            throw new ServiceNotFoundException("userservice");
        }

        _httpClient.BaseAddress = endpoint.ToUri();

        var response = await _httpClient.GetAsync($"/api/users/{userId}", ct);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserDto>(ct);
        }

        return null;
    }
}

// Delegating handler for automatic service discovery
public class ServiceDiscoveryHandler : DelegatingHandler
{
    private readonly IServiceDiscovery _discovery;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken ct)
    {
        // Extract service name from request URI
        // e.g., http://userservice/api/users → userservice
        var serviceName = request.RequestUri!.Host;

        var endpoint = await _discovery.GetServiceAsync(serviceName, ct);
        if (endpoint == null)
        {
            throw new ServiceNotFoundException(serviceName);
        }

        // Rewrite request URI with discovered endpoint
        var originalPath = request.RequestUri.PathAndQuery;
        request.RequestUri = new Uri(endpoint.ToUri(), originalPath);

        return await base.SendAsync(request, ct);
    }
}
```

---

## 3. Catálogo de Servicios OKLA

### 3.1 Servicios Registrados

| Servicio                  | Tags                   | Health Check | Instancias |
| ------------------------- | ---------------------- | ------------ | ---------- |
| `gateway`                 | api, gateway, v1       | `/health`    | 2          |
| `authservice`             | api, auth, v1          | `/health`    | 2          |
| `userservice`             | api, users, v1         | `/health`    | 2          |
| `vehiclessaleservice`     | api, vehicles, v1      | `/health`    | 3          |
| `mediaservice`            | api, media, v1         | `/health`    | 2          |
| `billingservice`          | api, billing, v1       | `/health`    | 2          |
| `notificationservice`     | api, notifications, v1 | `/health`    | 2          |
| `dealermanagementservice` | api, dealers, v1       | `/health`    | 2          |
| `crmservice`              | api, crm, v1           | `/health`    | 2          |

### 3.2 DNS Resolution

```bash
# Cada servicio es resolvible via DNS de Consul
$ dig @consul authservice.service.consul

; ANSWER SECTION:
authservice.service.consul. 0 IN A 10.0.1.5
authservice.service.consul. 0 IN A 10.0.1.6

# También disponible con SRV records para puerto
$ dig @consul SRV authservice.service.consul

; ANSWER SECTION:
authservice.service.consul. 0 IN SRV 1 1 8080 authservice-1.node.consul.
authservice.service.consul. 0 IN SRV 1 1 8080 authservice-2.node.consul.
```

---

## 4. Health Checks

### 4.1 Tipos de Health Checks

| Tipo       | Descripción              | Uso            |
| ---------- | ------------------------ | -------------- |
| **HTTP**   | GET a endpoint `/health` | Servicios web  |
| **TCP**    | Conexión TCP             | Bases de datos |
| **Script** | Ejecuta script/comando   | Custom checks  |
| **TTL**    | Time-to-live update      | Self-reporting |
| **gRPC**   | gRPC health protocol     | gRPC services  |

### 4.2 Configuración de Health Check

```json
{
  "service": {
    "name": "authservice",
    "id": "authservice-1",
    "port": 8080,
    "checks": [
      {
        "id": "api",
        "name": "HTTP API Health",
        "http": "http://localhost:8080/health",
        "method": "GET",
        "interval": "10s",
        "timeout": "5s",
        "deregister_critical_service_after": "5m"
      },
      {
        "id": "db",
        "name": "Database Connection",
        "tcp": "postgres:5432",
        "interval": "30s",
        "timeout": "10s"
      }
    ]
  }
}
```

### 4.3 Health Check States

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Health Check State Machine                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   ┌─────────────┐                                                       │
│   │   PASSING   │◄─────────────────────────────────────┐                │
│   │  (Healthy)  │                                       │                │
│   └──────┬──────┘                                       │                │
│          │                                              │                │
│          │ Check fails                                  │                │
│          ▼                                              │                │
│   ┌─────────────┐                                       │                │
│   │   WARNING   │ (Optional state)                      │                │
│   │             │                                       │                │
│   └──────┬──────┘                                       │                │
│          │                                              │                │
│          │ Continues failing                   Check succeeds            │
│          ▼                                              │                │
│   ┌─────────────┐                                       │                │
│   │  CRITICAL   │───────────────────────────────────────┘                │
│   │  (Unhealthy)│                                                        │
│   └──────┬──────┘                                                        │
│          │                                                               │
│          │ After DeregisterCriticalServiceAfter                         │
│          ▼                                                               │
│   ┌─────────────┐                                                        │
│   │ DEREGISTERED│                                                        │
│   │  (Removed)  │                                                        │
│   └─────────────┘                                                        │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Procesos Detallados

### 5.1 SD-001: Registro de Servicio

| Paso | Acción                           | Sistema      | Validación       |
| ---- | -------------------------------- | ------------ | ---------------- |
| 1    | Servicio inicia                  | Pod K8s      | Container starts |
| 2    | ConsulHostedService.StartAsync() | Service      | Hosted service   |
| 3    | Obtener IP local                 | Service      | IP resolved      |
| 4    | Crear AgentServiceRegistration   | Consul SDK   | Registration obj |
| 5    | PUT /v1/agent/service/register   | Consul Agent | 200 OK           |
| 6    | Consul Agent → Consul Server     | Consul       | Raft consensus   |
| 7    | Health check comienza            | Consul Agent | Interval loop    |
| 8    | Log registro exitoso             | Service      | Logger           |

### 5.2 SD-002: Descubrimiento de Servicio

| Paso | Acción                                | Sistema     | Validación         |
| ---- | ------------------------------------- | ----------- | ------------------ |
| 1    | Cliente necesita llamar a UserService | AuthService | Inter-service call |
| 2    | Check cache local                     | AuthService | Cache miss         |
| 3    | GET /v1/health/service/userservice    | Consul      | Query              |
| 4    | Consul retorna instancias healthy     | Consul      | Response           |
| 5    | Load balancer selecciona instancia    | AuthService | Round-robin        |
| 6    | Cache resultado                       | AuthService | 30s TTL            |
| 7    | HTTP request a endpoint               | AuthService | Call               |

### 5.3 SD-003: Failover Automático

| Paso | Acción                         | Sistema           | Validación      |
| ---- | ------------------------------ | ----------------- | --------------- |
| 1    | Instancia A falla health check | Consul Agent      | 3 checks failed |
| 2    | Mark as CRITICAL               | Consul            | State change    |
| 3    | Remove from healthy list       | Consul            | Catalog update  |
| 4    | Próxima query excluye A        | Service Discovery | Only B returned |
| 5    | Si A no se recupera en 5min    | Consul            | Timer           |
| 6    | Deregister servicio A          | Consul            | Removed         |
| 7    | Alert enviado                  | Alertmanager      | Team notified   |

---

## 6. Key/Value Store

### 6.1 Configuración Centralizada

```csharp
// Leer configuración desde Consul KV
public class ConsulConfigurationProvider : ConfigurationProvider
{
    private readonly IConsulClient _consulClient;
    private readonly string _prefix;

    public override void Load()
    {
        var kvPairs = _consulClient.KV.List(_prefix).Result.Response;

        if (kvPairs == null) return;

        foreach (var pair in kvPairs)
        {
            var key = pair.Key.Substring(_prefix.Length).Replace("/", ":");
            var value = Encoding.UTF8.GetString(pair.Value);
            Data[key] = value;
        }
    }
}

// Uso en Program.cs
builder.Configuration.AddConsul("okla/authservice/", options =>
{
    options.ConsulAddress = "http://consul:8500";
    options.ReloadOnChange = true;
    options.PollInterval = TimeSpan.FromSeconds(30);
});
```

### 6.2 Estructura KV

```
okla/
├── global/
│   ├── feature-flags/
│   │   ├── early-bird-enabled = true
│   │   └── new-search-ui = false
│   └── maintenance/
│       └── active = false
├── authservice/
│   ├── jwt-secret = <encrypted>
│   ├── token-expiry-minutes = 60
│   └── refresh-token-days = 30
├── userservice/
│   ├── max-profile-photo-size = 5242880
│   └── avatar-processing-enabled = true
└── billingservice/
    ├── stripe-key = <encrypted>
    └── azul-merchant-id = <encrypted>
```

---

## 7. Reglas de Negocio

| Código | Regla                                | Validación      |
| ------ | ------------------------------------ | --------------- |
| SD-R01 | Health check cada 10 segundos        | Interval config |
| SD-R02 | 3 fallos consecutivos = CRITICAL     | Check logic     |
| SD-R03 | Deregister después de 5 min critical | Config          |
| SD-R04 | Cache de discovery: 30 segundos      | TTL             |
| SD-R05 | Mínimo 2 instancias por servicio     | Deployment      |
| SD-R06 | KV changes triggerean reload         | Poll interval   |

---

## 8. Configuración Consul

```yaml
# consul-server.yaml (Kubernetes ConfigMap)
apiVersion: v1
kind: ConfigMap
metadata:
  name: consul-config
  namespace: okla
data:
  consul.hcl: |
    datacenter = "okla-do"
    data_dir = "/consul/data"
    log_level = "INFO"

    server = true
    bootstrap_expect = 3

    ui_config {
      enabled = true
    }

    connect {
      enabled = true
    }

    ports {
      grpc = 8502
    }

    telemetry {
      prometheus_retention_time = "60s"
      disable_hostname = true
    }
```

---

## 9. Métricas Prometheus

```
# Consul metrics
consul_catalog_services{dc="okla-do"}
consul_health_service_status{service="...", status="passing|warning|critical"}

# Service discovery client metrics
service_discovery_requests_total{service="..."}
service_discovery_cache_hit_ratio{service="..."}
service_discovery_latency_seconds{service="..."}
```

---

## 10. Comandos Útiles

```bash
# Ver todos los servicios registrados
consul catalog services

# Ver instancias de un servicio
consul catalog nodes -service=authservice

# Ver health de un servicio
consul health service authservice

# Registrar servicio manualmente (debug)
consul services register service.json

# Deregistrar servicio
consul services deregister authservice-1

# Leer KV
consul kv get okla/global/maintenance/active

# Escribir KV
consul kv put okla/global/maintenance/active true
```

---

## 📚 Referencias

- [01-gateway-service.md](01-gateway-service.md) - API Gateway
- [04-health-checks.md](04-health-checks.md) - Health checks detallados
- [10-configuration-service.md](10-configuration-service.md) - Configuración centralizada
