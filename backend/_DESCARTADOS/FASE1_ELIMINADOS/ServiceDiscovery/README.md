# 🔍 ServiceDiscovery - Service Discovery with Consul

## 📋 Overview

ServiceDiscovery es un servicio transversal que implementa **Service Discovery** usando **HashiCorp Consul** para el proyecto CarDealer Microservices. Permite el registro dinámico, descubrimiento y monitoreo de salud de todos los servicios en la arquitectura de microservicios.

## 🎯 Features

### ✅ Service Registration
- ✅ Registro automático de servicios con Consul
- ✅ Deregistro al apagar servicios
- ✅ Metadata customizable por servicio
- ✅ Tags para categorización

### ✅ Service Discovery
- ✅ Búsqueda de servicios por nombre
- ✅ Filtrado de instancias saludables
- ✅ Load balancing client-side (Round-robin)
- ✅ Service catalog completo

### ✅ Health Checks
- ✅ Health checks HTTP automáticos
- ✅ Configuración de intervalos y timeouts
- ✅ Deregistro automático de servicios críticos
- ✅ Monitoreo de salud agregado

### ✅ Architecture
- ✅ Clean Architecture (Domain, Application, Infrastructure, API)
- ✅ CQRS con MediatR
- ✅ Consul client integration
- ✅ RESTful API

## 🏗️ Architecture

```
ServiceDiscovery/
├── ServiceDiscovery.Domain/           # Entities & Business Logic
│   ├── Entities/
│   │   ├── ServiceInstance.cs         # Service instance entity
│   │   ├── ServiceCatalog.cs          # In-memory catalog
│   │   └── HealthCheckResult.cs       # Health check results
│   └── Enums/
│       ├── HealthStatus.cs            # Healthy, Degraded, Unhealthy
│       └── ServiceStatus.cs           # Active, Inactive, Deregistered
│
├── ServiceDiscovery.Application/      # Use Cases & Abstractions
│   ├── Interfaces/
│   │   ├── IServiceRegistry.cs        # Registration interface
│   │   ├── IServiceDiscovery.cs       # Discovery interface
│   │   └── IHealthChecker.cs          # Health check interface
│   ├── Commands/
│   │   ├── RegisterServiceCommand.cs
│   │   └── DeregisterServiceCommand.cs
│   ├── Queries/
│   │   ├── GetServiceInstancesQuery.cs
│   │   ├── GetServiceNamesQuery.cs
│   │   └── GetServiceInstanceByIdQuery.cs
│   └── Handlers/                      # MediatR handlers
│
├── ServiceDiscovery.Infrastructure/   # External Services
│   └── Services/
│       ├── ConsulServiceRegistry.cs   # Consul registration
│       ├── ConsulServiceDiscovery.cs  # Consul discovery
│       └── HttpHealthChecker.cs       # HTTP health checks
│
├── ServiceDiscovery.Api/              # REST API
│   ├── Controllers/
│   │   ├── ServicesController.cs      # Service management
│   │   └── HealthController.cs        # Health checks
│   ├── Middleware/
│   │   └── ServiceRegistrationMiddleware.cs
│   └── Program.cs
│
└── ServiceDiscovery.Tests/            # Unit Tests (21 tests)
    └── Domain/
        ├── ServiceInstanceTests.cs
        ├── ServiceCatalogTests.cs
        └── HealthCheckResultTests.cs
```

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK
- Docker & Docker Compose
- Consul 1.18+ (or use Docker)

### Installation

1. **Clone the repository**
```bash
cd backend/ServiceDiscovery
```

2. **Restore dependencies**
```bash
dotnet restore ServiceDiscovery.sln
```

3. **Build the solution**
```bash
dotnet build ServiceDiscovery.sln
```

4. **Run tests**
```bash
dotnet test ServiceDiscovery.sln
# Output: 21 tests passed
```

### Running with Docker

1. **Start Consul and ServiceDiscovery**
```bash
cd backend
docker-compose up -d consul servicediscovery
```

2. **Verify Consul is running**
```
Open browser: http://localhost:8500
```

3. **Verify ServiceDiscovery API**
```bash
curl http://localhost:5096/api/health
```

### Running Locally (Development)

1. **Start Consul locally**
```bash
docker run -d --name consul -p 8500:8500 hashicorp/consul:1.18 agent -server -ui -bootstrap-expect=1 -client=0.0.0.0
```

2. **Run the API**
```bash
cd ServiceDiscovery.Api
dotnet run
```

3. **Access Swagger UI**
```
http://localhost:5096/swagger
```

## 📚 API Endpoints

### Services Management

#### Register a Service
```http
POST /api/services/register
Content-Type: application/json

{
  "serviceName": "authservice",
  "host": "localhost",
  "port": 5001,
  "healthCheckUrl": "/health",
  "healthCheckInterval": 10,
  "healthCheckTimeout": 5,
  "tags": ["v1", "api"],
  "metadata": {
    "environment": "Production"
  },
  "version": "1.0.0"
}

Response: 200 OK
{
  "id": "auth-001-guid",
  "serviceName": "authservice",
  "host": "localhost",
  "port": 5001,
  "address": "http://localhost:5001",
  "status": "Active",
  "healthStatus": "Unknown",
  "tags": ["v1", "api"],
  ...
}
```

#### Get Service Instances
```http
GET /api/services/authservice?onlyHealthy=true

Response: 200 OK
[
  {
    "id": "auth-001",
    "serviceName": "authservice",
    "host": "localhost",
    "port": 5001,
    "address": "http://localhost:5001",
    "healthStatus": "Healthy",
    ...
  }
]
```

#### Get All Service Names
```http
GET /api/services/names

Response: 200 OK
[
  "authservice",
  "userservice",
  "vehicleservice",
  ...
]
```

#### Get Service Instance by ID
```http
GET /api/services/instance/{instanceId}

Response: 200 OK
{
  "id": "auth-001",
  "serviceName": "authservice",
  ...
}
```

#### Deregister a Service
```http
DELETE /api/services/{instanceId}

Response: 200 OK
{
  "message": "Service deregistered successfully"
}
```

### Health Checks

#### Check All Services Health
```http
GET /api/health/all

Response: 200 OK
[
  {
    "instanceId": "auth-001",
    "status": "Healthy",
    "responseTimeMs": 150,
    "statusCode": 200,
    "message": "Service is healthy",
    "checkedAt": "2025-12-01T12:00:00Z"
  },
  ...
]
```

#### Check Specific Service Health
```http
GET /api/health/service/authservice

Response: 200 OK
[
  {
    "instanceId": "auth-001",
    "status": "Healthy",
    ...
  }
]
```

#### Check Instance Health
```http
GET /api/health/instance/{instanceId}

Response: 200 OK
{
  "instanceId": "auth-001",
  "status": "Healthy",
  "responseTimeMs": 120,
  ...
}
```

#### ServiceDiscovery Health Check
```http
GET /api/health

Response: 200 OK
{
  "status": "Healthy",
  "service": "ServiceDiscovery",
  "timestamp": "2025-12-01T12:00:00Z"
}
```

## 🔧 Configuration

### appsettings.json
```json
{
  "Consul": {
    "Address": "http://localhost:8500"
  },
  "Service": {
    "Name": "myservice",
    "Host": "localhost",
    "Port": "5000",
    "HealthCheckUrl": "/api/health"
  }
}
```

### Environment Variables
```bash
Consul__Address=http://consul:8500
Service__Name=authservice
Service__Host=authservice
Service__Port=80
Service__HealthCheckUrl=/health
```

## 🧪 Testing

### Run All Tests
```bash
dotnet test ServiceDiscovery.sln

# Output:
# Passed!  - Failed: 0, Passed: 21, Skipped: 0
```

### Test Coverage
- ✅ **ServiceInstanceTests** - 7 tests
  - Validation logic
  - Health status updates
  - Address formatting
  
- ✅ **ServiceCatalogTests** - 10 tests
  - Registration/deregistration
  - Instance retrieval
  - Healthy instance filtering
  - Statistics

- ✅ **HealthCheckResultTests** - 4 tests
  - Result creation
  - Status mapping

## 🐳 Docker

### Build Image
```bash
cd ServiceDiscovery
docker build -t servicediscovery:latest .
```

### Run Container
```bash
docker run -d \
  --name servicediscovery \
  -p 5096:80 \
  -e Consul__Address=http://consul:8500 \
  servicediscovery:latest
```

### Docker Compose
```yaml
consul:
  image: hashicorp/consul:1.18
  ports:
    - "8500:8500"
  command: agent -server -ui -bootstrap-expect=1 -client=0.0.0.0

servicediscovery:
  build: ./ServiceDiscovery
  ports:
    - "5096:80"
  environment:
    Consul__Address: http://consul:8500
  depends_on:
    - consul
```

## 💡 Usage Examples

### Client-Side Integration

#### 1. Register Service on Startup
```csharp
// In Program.cs or Startup
var serviceRegistry = services.BuildServiceProvider().GetService<IServiceRegistry>();

var instance = new ServiceInstance
{
    Id = $"authservice-{Guid.NewGuid()}",
    ServiceName = "authservice",
    Host = "localhost",
    Port = 5001,
    HealthCheckUrl = "/health",
    HealthCheckInterval = 10,
    Tags = new List<string> { "v1", "api" }
};

await serviceRegistry.RegisterServiceAsync(instance);
```

#### 2. Discover Service
```csharp
var discovery = services.GetService<IServiceDiscovery>();

// Get healthy instances
var instances = await discovery.GetHealthyInstancesAsync("authservice");

// Round-robin load balancing
var instance = await discovery.FindServiceInstanceAsync("authservice");

// Call the service
var httpClient = new HttpClient();
var response = await httpClient.GetAsync($"{instance.Address}/api/users");
```

#### 3. Health Check Integration
```csharp
var healthChecker = services.GetService<IHealthChecker>();

// Check all services
var results = await healthChecker.CheckAllServicesHealthAsync();

// Check specific service
var authResults = await healthChecker.CheckServiceHealthAsync("authservice");

// Alert on unhealthy services
foreach (var result in results.Where(r => r.Status == HealthStatus.Unhealthy))
{
    logger.LogWarning("Service {InstanceId} is unhealthy: {Error}", 
        result.InstanceId, result.Error);
}
```

## 🔌 Integration with Other Services

### Auto-Registration Middleware

Add to any service's `Program.cs`:

```csharp
// Configure service settings
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
{
    {"Service:Name", "authservice"},
    {"Service:Host", "authservice"},
    {"Service:Port", "80"},
    {"Service:HealthCheckUrl", "/health"}
});

// Use auto-registration middleware
app.UseServiceRegistration();
```

## 📊 Consul UI

Access Consul UI at: **http://localhost:8500**

Features:
- View all registered services
- Check health status
- Browse service catalog
- View service metadata and tags
- Monitor health check history

## 🛠️ Tech Stack

- **ASP.NET Core 8.0** - API Framework
- **HashiCorp Consul 1.18** - Service Discovery
- **MediatR 12.2.0** - CQRS pattern
- **Consul .NET Client 1.7.14** - Consul integration
- **xUnit** - Unit testing
- **FluentAssertions** - Test assertions
- **Moq** - Mocking framework

## 📈 Performance

- **Load Balancing**: Round-robin client-side
- **Health Check Interval**: 10 seconds (configurable)
- **Health Check Timeout**: 5 seconds (configurable)
- **Auto-Deregister**: 1 minute after critical failure

## 🔒 Security Considerations

- Consul ACLs not enabled by default (enable in production)
- No authentication on ServiceDiscovery API (add in production)
- Health check URLs should be public
- Consider TLS for Consul communication in production

## 🐛 Troubleshooting

### Service not appearing in Consul
1. Check Consul is running: `curl http://localhost:8500/v1/status/leader`
2. Verify service configuration (name, host, port)
3. Check ServiceDiscovery logs
4. Ensure network connectivity between service and Consul

### Health checks failing
1. Verify health endpoint is accessible: `curl http://service:port/health`
2. Check health check interval/timeout settings
3. Review service logs for errors
4. Verify firewall rules

### Consul UI not accessible
1. Ensure Consul is running on port 8500
2. Check Docker port mappings
3. Verify `-ui` flag is set in Consul command

## 🚀 Next Steps

- [ ] Implement Consul ACLs for security
- [ ] Add authentication/authorization to API
- [ ] Implement gRPC health checks
- [ ] Add distributed tracing integration
- [ ] Create client SDK for easier integration
- [ ] Add integration tests with real Consul
- [ ] Implement service mesh features

## 📝 License

This project is part of CarDealer Microservices architecture.

---

**Status:** ✅ Production Ready  
**Version:** 1.0.0  
**Last Updated:** December 1, 2025
