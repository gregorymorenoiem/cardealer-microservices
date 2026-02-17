# ServiceDiscovery - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** ServiceDiscovery
- **Puerto en Desarrollo:** 5018
- **Estado:** ⚠️ **SOLO DESARROLLO LOCAL**
- **Backend:** Consul / Eureka
- **Base de Datos:** N/A
- **Imagen Docker:** Local only

### Propósito
Servicio de descubrimiento de servicios (Service Discovery) para ambientes dinámicos donde las IPs/puertos de servicios cambian. Permite registro automático y health checks. En producción con Kubernetes, se usa DNS de K8s.

---

## 🏗️ ARQUITECTURA

```
ServiceDiscovery/
├── ServiceDiscovery.Api/
│   ├── Controllers/
│   │   ├── ServicesController.cs
│   │   └── HealthController.cs
│   └── Program.cs
├── ServiceDiscovery.Application/
│   ├── Interfaces/
│   │   └── IServiceRegistry.cs
│   └── Services/
│       └── ConsulServiceRegistry.cs
└── ServiceDiscovery.Infrastructure/
    └── Consul/
        └── ConsulClient.cs
```

---

## 📦 CONCEPTOS

### Service Registry
Catálogo centralizado de todos los servicios disponibles y sus instancias.

### Health Checks
Verificaciones periódicas para detectar servicios caídos.

### Load Balancing
Distribuir requests entre múltiples instancias del mismo servicio.

---

## 📡 ENDPOINTS API

#### POST `/api/services/register`
Registrar instancia de servicio.

**Request:**
```json
{
  "serviceName": "VehiclesSaleService",
  "serviceId": "vehicles-sale-instance-1",
  "host": "192.168.1.100",
  "port": 5004,
  "healthCheckUrl": "http://192.168.1.100:5004/health",
  "tags": ["vehicles", "sale", "v1"],
  "metadata": {
    "version": "1.0.0",
    "region": "us-east-1"
  }
}
```

**Response (201 Created):**
```json
{
  "serviceId": "vehicles-sale-instance-1",
  "status": "Registered",
  "message": "Service registered successfully"
}
```

#### DELETE `/api/services/{serviceId}`
Deregistrar instancia (cuando se apaga).

#### GET `/api/services/{serviceName}`
Obtener instancias disponibles de un servicio.

**Response (200 OK):**
```json
{
  "serviceName": "VehiclesSaleService",
  "instances": [
    {
      "serviceId": "vehicles-sale-instance-1",
      "host": "192.168.1.100",
      "port": 5004,
      "status": "Healthy",
      "lastHealthCheck": "2026-01-07T10:30:00Z"
    },
    {
      "serviceId": "vehicles-sale-instance-2",
      "host": "192.168.1.101",
      "port": 5004,
      "status": "Healthy",
      "lastHealthCheck": "2026-01-07T10:30:00Z"
    }
  ]
}
```

#### GET `/api/services`
Listar todos los servicios registrados.

---

## 🔄 FLUJO TÍPICO

### 1. Registro al Iniciar
Cuando un servicio arranca, se registra:

```csharp
public class ServiceDiscoveryHostedService : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var registration = new ServiceRegistration
        {
            ServiceName = "VehiclesSaleService",
            ServiceId = $"vehicles-{Environment.MachineName}",
            Host = GetLocalIpAddress(),
            Port = 5004,
            HealthCheckUrl = "http://localhost:5004/health"
        };
        
        await _serviceRegistry.RegisterAsync(registration);
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _serviceRegistry.DeregisterAsync(serviceId);
    }
}
```

### 2. Descubrimiento
Cliente busca servicio disponible:

```csharp
public async Task<string> GetVehicleServiceUrl()
{
    var instances = await _serviceDiscovery.GetServiceInstances("VehiclesSaleService");
    
    // Load balancing: round-robin, random, least connections
    var instance = instances.OrderBy(_ => Random.Shared.Next()).First();
    
    return $"http://{instance.Host}:{instance.Port}";
}
```

### 3. Health Checks
Service Discovery hace ping periódico a `/health`:

```
Cada 10 segundos:
  GET http://192.168.1.100:5004/health
  
Si falla 3 veces consecutivas:
  Marcar instancia como Unhealthy
  No incluir en load balancing
```

---

## 🔧 CONSUL INTEGRATION

### Registro con Consul
```csharp
using Consul;

var consulClient = new ConsulClient(config =>
{
    config.Address = new Uri("http://localhost:8500");
});

var registration = new AgentServiceRegistration
{
    ID = "vehicles-sale-instance-1",
    Name = "VehiclesSaleService",
    Address = "192.168.1.100",
    Port = 5004,
    Check = new AgentServiceCheck
    {
        HTTP = "http://192.168.1.100:5004/health",
        Interval = TimeSpan.FromSeconds(10),
        Timeout = TimeSpan.FromSeconds(5)
    }
};

await consulClient.Agent.ServiceRegister(registration);
```

### Descubrimiento
```csharp
var services = await consulClient.Health.Service("VehiclesSaleService", "", true);

foreach (var service in services.Response)
{
    Console.WriteLine($"{service.Service.ID} - {service.Service.Address}:{service.Service.Port}");
}
```

---

## 🚀 EN KUBERNETES

En Kubernetes, Service Discovery es built-in:

### Service DNS
Cada servicio tiene DNS automático:

```
servicename.namespace.svc.cluster.local
```

Ejemplos:
```
vehiclessaleservice.okla.svc.cluster.local
authservice.okla.svc.cluster.local
```

### Load Balancing
Kubernetes distribuye automáticamente entre pods:

```yaml
apiVersion: v1
kind: Service
metadata:
  name: vehiclessaleservice
spec:
  selector:
    app: vehiclessaleservice
  ports:
  - port: 8080
    targetPort: 8080
```

### Health Checks
Kubernetes usa Liveness/Readiness Probes:

```yaml
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

---

## 📊 COMPARACIÓN: CONSUL VS KUBERNETES

| Feature | Consul | Kubernetes |
|---------|--------|------------|
| **Service Registry** | ✅ Manual register | ✅ Automático |
| **Health Checks** | ✅ HTTP/TCP/Script | ✅ Liveness/Readiness |
| **Load Balancing** | ✅ Client-side | ✅ Server-side |
| **DNS** | ✅ consul.service | ✅ {service}.{ns}.svc |
| **Multi-DC** | ✅ Sí | ❌ Necesita mesh |
| **Key-Value Store** | ✅ Sí | ❌ Usar ConfigMaps |

---

## 🌐 ALTERNATIVAS

### Cloud Providers
- **AWS Cloud Map**: Service discovery en AWS
- **Azure Service Fabric**: Service mesh de Azure
- **Google Cloud Service Directory**: Service registry en GCP

### Service Mesh
- **Istio**: Service mesh completo (discovery + security + observability)
- **Linkerd**: Service mesh ligero
- **Consul Connect**: Service mesh de HashiCorp

### Simple Approaches
- **DNS Round Robin**: Múltiples IPs en DNS
- **Load Balancer**: HAProxy, NGINX
- **API Gateway**: Gateway conoce ubicación de servicios

---

## 📝 CUÁNDO USAR SERVICE DISCOVERY

### ✅ Usar Cuando:
- Servicios desplegados en múltiples hosts
- IPs dinámicas (AWS Auto Scaling, K8s pods)
- Necesitas health checks automáticos
- Multiple datacenters/regions

### ❌ No Usar Cuando:
- Ya usas Kubernetes (tiene discovery built-in)
- Pocos servicios con IPs estáticas
- Monolito o microservicios en single host

---

**Estado:** Solo desarrollo - Kubernetes DNS en producción  
**Versión:** 1.0.0
