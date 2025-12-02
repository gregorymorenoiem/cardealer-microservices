# Configuration Service

## ✅ ConfigurationService - Deployment Completado

### 📊 Resumen de Implementación

**ConfigurationService** ha sido desplegado exitosamente con todas sus funcionalidades:

#### 🎯 Características Implementadas

1. **Gestión de Configuraciones Centralizada**
   - Configuraciones por ambiente (Dev/Staging/Prod)
   - Soporte multi-tenant
   - Versionado de configuraciones
   - Historial de cambios y auditoría
   - Hot-reload capabilities

2. **Secrets Encriptados (AES-256)**
   - Almacenamiento seguro de API keys, passwords, etc.
   - Soporte de expiración de secrets
   - Secrets específicos por ambiente
   - Aislamiento multi-tenant

3. **Feature Flags**
   - Toggle en runtime
   - Gradual rollout (canary releases) con distribución basada en porcentaje
   - Flags específicos por ambiente
   - Activación basada en tiempo (start/end dates)
   - Targeting por usuario para A/B testing

#### 🐳 Docker Deployment

- **Servicio**: `configurationservice` (puerto **5085**)
- **Base de Datos**: `configurationservice-db` PostgreSQL 15 (puerto **5434**)
- **Dockerfile**: Multi-stage build con ServiceDiscovery, curl para health checks
- **Integración**: Consul, PostgreSQL, Service Discovery

#### 🗄️ Base de Datos

**4 tablas creadas**:
- `configuration_items` - Configuraciones centralizadas
- `encrypted_secrets` - Secrets con encriptación AES-256
- `feature_flags` - Feature flags con rollout gradual
- `configuration_histories` - Historial de cambios

**Índices**: 4 índices únicos compuestos (Key, Environment, TenantId)

#### ✅ Testing

- **20/20 tests** pasando (13 unit + 7 integration E2E)
- **7/7 API tests** pasando:
  - ✅ Health Check
  - ✅ Create Configuration
  - ✅ Get Configuration by Key
  - ✅ Get All Configurations
  - ✅ Create Feature Flag
  - ✅ Check Feature Flag Status
  - ✅ Swagger UI Accessibility

#### 🔧 Modificaciones Realizadas

**11 archivos modificados/creados**:
- ✅ Dockerfile actualizado con ServiceDiscovery + curl
- ✅ docker-compose.yml con Consul integration
- ✅ Program.cs con HttpClient + health endpoint
- ✅ FeatureFlagsController con POST endpoint
- ✅ EF Core Design packages agregados
- ✅ 3 archivos de migrations creados
- ✅ 3 scripts de gestión (start/stop/test)

#### 🌐 Access Points

- **API**: http://localhost:5085
- **Swagger UI**: http://localhost:5085/swagger
- **Health**: http://localhost:5085/health
- **Database**: localhost:5434 (configurationservice/postgres/password)
- **Consul UI**: http://localhost:8500

#### 📝 API Endpoints

**Configurations**:
- `GET /api/configurations/{key}?environment=Dev&tenantId=xxx`
- `GET /api/configurations?environment=Dev&tenantId=xxx`
- `POST /api/configurations`
- `PUT /api/configurations/{id}`
- `DELETE /api/configurations/{id}`

**Feature Flags**:
- `POST /api/featureflags` - Crear feature flag
- `GET /api/featureflags/{key}/enabled?environment=Dev&userId=xxx`

---

## Overview

The Configuration Service provides centralized configuration management, encrypted secrets storage, and feature flags for the CarDealer microservices platform.

## Features

### 1. **Configuration Management**
- Centralized configuration per environment (Dev/Staging/Prod)
- Multi-tenant support
- Configuration versioning
- Change history and audit trail
- Hot reload capabilities (configurations can be updated without service restart)

### 2. **Encrypted Secrets**
- AES-256 encryption for sensitive data
- Secure storage of API keys, database passwords, etc.
- Secret expiration support
- Environment-specific secrets
- Multi-tenant isolation

### 3. **Feature Flags**
- Runtime feature toggling
- Gradual rollout (canary releases) with percentage-based distribution
- Environment-specific flags
- Time-based activation (start/end dates)
- User-based targeting for A/B testing

## Architecture

### Clean Architecture Layers

```
ConfigurationService/
├── ConfigurationService.Domain/         # Domain entities
│   └── Entities/
│       ├── ConfigurationItem.cs
│       ├── EncryptedSecret.cs
│       ├── FeatureFlag.cs
│       └── ConfigurationHistory.cs
│
├── ConfigurationService.Application/    # Business logic
│   ├── Commands/
│   ├── Queries/
│   ├── Handlers/
│   └── Interfaces/
│
├── ConfigurationService.Infrastructure/ # Data access
│   ├── Data/
│   │   └── ConfigurationDbContext.cs
│   └── Services/
│       ├── ConfigurationManager.cs
│       ├── SecretManager.cs
│       ├── FeatureFlagManager.cs
│       └── AesEncryptionService.cs
│
├── ConfigurationService.Api/            # API layer
│   └── Controllers/
│       ├── ConfigurationsController.cs
│       └── FeatureFlagsController.cs
│
└── ConfigurationService.Tests/          # Unit tests (13 tests)
```

## API Endpoints

### Configurations

- `GET /api/configurations/{key}?environment=Dev&tenantId=xxx` - Get configuration value
- `GET /api/configurations?environment=Dev&tenantId=xxx` - Get all configurations
- `POST /api/configurations` - Create configuration
- `PUT /api/configurations/{id}` - Update configuration
- `DELETE /api/configurations/{id}` - Delete configuration (soft delete)

### Feature Flags

- `GET /api/featureflags/{key}/enabled?environment=Dev&tenantId=xxx&userId=xxx` - Check if feature is enabled

## Configuration

### Database Connection

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=configurationservice;Username=postgres;Password=postgres"
  }
}
```

### Encryption Key

⚠️ **Important:** Change the encryption key in production!

```json
{
  "Encryption": {
    "Key": "YourSuperSecureEncryptionKey123!ChangeInProduction"
  }
}
```

## Running the Service

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 15.x

### Local Development

```bash
cd ConfigurationService
dotnet restore
dotnet build
dotnet run --project ConfigurationService.Api
```

### Running Tests

```bash
dotnet test
```

**Test Coverage:** 13 unit tests covering:
- Configuration CRUD operations
- Encryption/decryption
- Feature flag evaluation
- Rollout percentage logic
- History tracking

### Docker

```bash
docker build -t configurationservice:latest .
docker run -p 5000:8080 configurationservice:latest
```

## Usage Examples

### Creating a Configuration

```bash
POST /api/configurations
{
  "key": "ApiTimeout",
  "value": "30000",
  "environment": "Prod",
  "description": "API timeout in milliseconds",
  "createdBy": "admin"
}
```

### Creating a Secret

```csharp
var secret = await secretManager.CreateSecretAsync(
    key: "DatabasePassword",
    plainValue: "MySecurePassword123!",
    environment: "Prod",
    createdBy: "admin",
    expiresAt: DateTime.UtcNow.AddYears(1)
);
```

### Creating a Feature Flag with Gradual Rollout

```csharp
var flag = new FeatureFlag
{
    Name = "New Checkout Flow",
    Key = "new_checkout",
    IsEnabled = true,
    RolloutPercentage = 25, // 25% of users
    Environment = "Prod",
    CreatedBy = "product_team"
};
```

### Checking if Feature is Enabled

```bash
GET /api/featureflags/new_checkout/enabled?environment=Prod&userId=user123
```

## Security Considerations

1. **Encryption Keys**: Store encryption keys in secure vaults (Azure Key Vault, AWS Secrets Manager)
2. **Database Access**: Use least-privilege database accounts
3. **API Authentication**: Integrate with AuthService for endpoint protection
4. **Secret Rotation**: Implement regular secret rotation policies
5. **Audit Logging**: All configuration changes are tracked in history

## Multi-Tenant Support

The service supports multi-tenancy through the `TenantId` property:

```csharp
// Tenant-specific configuration
var config = await configManager.GetConfigurationAsync("MaxOrders", "Prod", tenantId: "tenant-A");

// Tenant-specific feature flag
var isEnabled = await flagManager.IsFeatureEnabledAsync("premium_features", "Prod", tenantId: "tenant-B");
```

## Performance

- **Caching**: Consider implementing Redis cache for frequently accessed configurations
- **Database Indexing**: Composite indexes on (Key, Environment, TenantId)
- **Hot Reload**: Services can subscribe to configuration changes via SignalR (future enhancement)

## Monitoring

### Health Checks
- Database connectivity
- Encryption service availability

### Metrics to Track
- Configuration read/write operations per second
- Secret decryption latency
- Feature flag evaluation time
- Configuration change frequency

## Roadmap

- [ ] Redis caching layer
- [ ] SignalR for real-time configuration updates
- [ ] Import/Export configurations (JSON/YAML)
- [ ] Configuration templates
- [ ] Advanced RBAC for configuration management
- [ ] Integration with Azure Key Vault for secret storage

## Contributing

Follow the project's Clean Architecture and CQRS patterns. All changes must include:
- Unit tests (maintain >80% coverage)
- API documentation updates
- Migration scripts if schema changes

## License

Internal use only - CarDealer Microservices Platform
