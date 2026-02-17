# Feature Toggle Service

Microservicio para gestión de Feature Flags y toggles de funcionalidades para el sistema CarDealer.

## 📋 Descripción

El Feature Toggle Service permite controlar la habilitación/deshabilitación de funcionalidades en tiempo de ejecución sin necesidad de desplegar nuevo código. Soporta múltiples estrategias de rollout, targeting por usuario/grupo, y evaluación en tiempo real.

## 🏗️ Arquitectura

El servicio sigue el patrón de **Clean Architecture** con las siguientes capas:

```
FeatureToggleService/
├── FeatureToggleService.Domain/        # Entidades, Enums, Interfaces de dominio
├── FeatureToggleService.Application/   # CQRS, Commands, Queries, Handlers
├── FeatureToggleService.Infrastructure/# EF Core, Repositorios, Servicios
├── FeatureToggleService.Api/           # Controllers, Program.cs, Configuración
└── FeatureToggleService.Tests/         # Tests unitarios
```

## 🚀 Características

### Feature Flags
- ✅ CRUD completo de feature flags
- ✅ Activación/Desactivación instantánea
- ✅ Kill Switch (deshabilitación de emergencia)
- ✅ Expiración automática por fecha
- ✅ Múltiples entornos (Development, Staging, Production)

### Rollout Strategies
- ✅ **Percentage Rollout**: Despliegue gradual por porcentaje
- ✅ **User Targeting**: Habilitación para usuarios específicos
- ✅ **Group Targeting**: Habilitación por grupos de usuarios
- ✅ **Environment-based**: Flags por entorno

### Evaluación
- ✅ Evaluación en tiempo real con caché
- ✅ Evaluación múltiple de flags en una sola llamada
- ✅ Contexto de evaluación (userId, environment, attributes)

### Auditoría
- ✅ Historial completo de cambios
- ✅ Registro de quién hizo cada cambio
- ✅ Estadísticas de uso

## 📦 Instalación

### Requisitos
- .NET 8.0 SDK
- PostgreSQL 14+
- Docker (opcional)

### Configuración Local

1. **Clonar el repositorio** (si no está ya clonado)

2. **Configurar la base de datos** en `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=featuretoggle_dev;Username=postgres;Password=postgres"
  }
}
```

3. **Ejecutar migraciones**:
```bash
cd backend/FeatureToggleService/FeatureToggleService.Api
dotnet ef database update
```

4. **Iniciar el servicio**:
```bash
dotnet run
```

### Docker

```bash
cd backend
docker-compose up -d featuretoggleservice
```

## 🔌 API Endpoints

### Feature Flags CRUD

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/featureflags` | Obtener todos los flags |
| GET | `/api/featureflags/{id}` | Obtener flag por ID |
| GET | `/api/featureflags/key/{key}` | Obtener flag por clave |
| POST | `/api/featureflags` | Crear nuevo flag |
| PUT | `/api/featureflags/{id}` | Actualizar flag |
| DELETE | `/api/featureflags/{id}` | Eliminar flag |

### Control de Estado

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/featureflags/{id}/enable` | Habilitar flag |
| POST | `/api/featureflags/{id}/disable` | Deshabilitar flag |
| POST | `/api/featureflags/{id}/archive` | Archivar flag |
| POST | `/api/featureflags/{id}/restore` | Restaurar flag |
| POST | `/api/featureflags/{id}/kill-switch` | Activar kill switch |

### Filtrado

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/featureflags/environment/{env}` | Flags por entorno |
| GET | `/api/featureflags/status/{status}` | Flags por estado |
| GET | `/api/featureflags/tag/{tag}` | Flags por etiqueta |
| GET | `/api/featureflags/active` | Flags activos |
| GET | `/api/featureflags/expired` | Flags expirados |

### Evaluación

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/featureflags/evaluate` | Evaluar un flag |
| POST | `/api/featureflags/evaluate-multiple` | Evaluar múltiples flags |

### Rollout

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/featureflags/{id}/rollout-percentage?percentage=50` | Configurar porcentaje |
| POST | `/api/featureflags/{id}/target-users` | Agregar usuarios target |
| DELETE | `/api/featureflags/{id}/target-users` | Remover usuarios target |

### Historial y Stats

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/featureflags/{id}/history` | Historial de cambios |
| GET | `/api/featureflags/stats` | Estadísticas |

## 📝 Ejemplos de Uso

### Crear un Feature Flag

```bash
curl -X POST http://localhost:5000/api/featureflags \
  -H "Content-Type: application/json" \
  -d '{
    "key": "new-checkout-flow",
    "name": "Nuevo Flujo de Checkout",
    "description": "Flujo de checkout rediseñado con mejor UX",
    "environment": "Development",
    "tags": ["checkout", "ux"],
    "createdBy": "admin"
  }'
```

### Habilitar un Flag

```bash
curl -X POST "http://localhost:5000/api/featureflags/{id}/enable?modifiedBy=admin"
```

### Configurar Rollout Gradual (50%)

```bash
curl -X POST "http://localhost:5000/api/featureflags/{id}/rollout-percentage?percentage=50&modifiedBy=admin"
```

### Agregar Usuarios Target (Beta Testers)

```bash
curl -X POST http://localhost:5000/api/featureflags/{id}/target-users \
  -H "Content-Type: application/json" \
  -d '["user-123", "user-456", "user-789"]'
```

### Evaluar un Flag

```bash
curl -X POST http://localhost:5000/api/featureflags/evaluate \
  -H "Content-Type: application/json" \
  -d '{
    "flagKey": "new-checkout-flow",
    "context": {
      "userId": "user-123",
      "environment": "Development"
    }
  }'
```

**Respuesta:**
```json
{
  "flagKey": "new-checkout-flow",
  "isEnabled": true
}
```

### Evaluar Múltiples Flags

```bash
curl -X POST http://localhost:5000/api/featureflags/evaluate-multiple \
  -H "Content-Type: application/json" \
  -d '{
    "flagKeys": ["new-checkout-flow", "dark-mode", "premium-features"],
    "context": {
      "userId": "user-123",
      "environment": "Production"
    }
  }'
```

**Respuesta:**
```json
{
  "new-checkout-flow": true,
  "dark-mode": false,
  "premium-features": true
}
```

### Activar Kill Switch (Emergencia)

```bash
curl -X POST "http://localhost:5000/api/featureflags/{id}/kill-switch?triggeredBy=ops-team&reason=Critical%20bug%20detected"
```

## 🧪 Tests

### Ejecutar todos los tests

```bash
cd backend/FeatureToggleService/FeatureToggleService.Tests
dotnet test
```

### Ejecutar tests con cobertura

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Tests incluidos

- **Domain Tests**: Validación de entidades y reglas de negocio
- **Service Tests**: Lógica de evaluación de flags
- **Handler Tests**: Comandos y queries CQRS

## 📊 Modelo de Datos

### FeatureFlag

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | Guid | Identificador único |
| Key | string | Clave única del flag |
| Name | string | Nombre descriptivo |
| Description | string | Descripción detallada |
| IsEnabled | bool | Estado de habilitación |
| Status | FlagStatus | Draft, Active, Inactive, Archived |
| Environment | Environment | Development, Staging, Production, All |
| RolloutPercentage | int | Porcentaje de despliegue (0-100) |
| TargetUserIds | List<string> | IDs de usuarios target |
| TargetGroups | List<string> | Grupos target |
| Tags | List<string> | Etiquetas para categorización |
| ExpiresAt | DateTime? | Fecha de expiración |
| KillSwitchTriggered | bool | Flag de emergencia activado |

### FeatureFlagHistory

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | Guid | Identificador único |
| FeatureFlagId | Guid | Referencia al flag |
| ChangeType | ChangeType | Created, Enabled, Disabled, etc. |
| PreviousValue | string | Valor anterior |
| NewValue | string | Nuevo valor |
| ChangedBy | string | Usuario que hizo el cambio |
| ChangedAt | DateTime | Fecha del cambio |

## 🔧 Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=featuretoggle_db;Username=postgres;Password=postgres"
  },
  "FeatureToggle": {
    "CacheExpirationMinutes": 5,
    "DefaultEnvironment": "Development",
    "EnableMetrics": true
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5173"]
  }
}
```

## 🏷️ Estados de Feature Flag

| Estado | Descripción |
|--------|-------------|
| **Draft** | Flag creado pero no activo |
| **Active** | Flag habilitado y evaluándose |
| **Inactive** | Flag deshabilitado |
| **Archived** | Flag archivado (no se evalúa) |

## 📈 Estrategias de Rollout

### 1. Percentage Rollout
Despliega la funcionalidad gradualmente basándose en un porcentaje.

```json
{
  "rolloutPercentage": 25,
  "isEnabled": true
}
```

### 2. User Targeting
Habilita la funcionalidad solo para usuarios específicos.

```json
{
  "targetUserIds": ["user-1", "user-2"],
  "isEnabled": true
}
```

### 3. Group Targeting
Habilita la funcionalidad para grupos de usuarios.

```json
{
  "targetGroups": ["beta-testers", "internal"],
  "isEnabled": true
}
```

## 📚 Integración con Otros Servicios

### Ejemplo de uso en código C#

```csharp
// Inyectar el servicio
private readonly IFeatureFlagEvaluator _featureFlagEvaluator;

// Evaluar un flag
var context = new EvaluationContext 
{ 
    UserId = currentUser.Id,
    Environment = "Production"
};

if (await _featureFlagEvaluator.EvaluateAsync("new-checkout-flow", context))
{
    // Mostrar nuevo flujo de checkout
}
else
{
    // Mostrar flujo tradicional
}
```

### Ejemplo de cliente HTTP

```csharp
public class FeatureFlagClient
{
    private readonly HttpClient _httpClient;
    
    public async Task<bool> IsEnabledAsync(string flagKey, string userId)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/featureflags/evaluate", new
        {
            FlagKey = flagKey,
            Context = new { UserId = userId }
        });
        
        var result = await response.Content.ReadFromJsonAsync<EvaluationResult>();
        return result?.IsEnabled ?? false;
    }
}
```

## 🔍 Health Check

El servicio expone un endpoint de health check:

```bash
curl http://localhost:5000/health
```

## 📖 Swagger/OpenAPI

Documentación interactiva disponible en desarrollo:

```
http://localhost:5000/swagger
```

## 🛠️ Tecnologías

- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8.0
- **Base de Datos**: PostgreSQL
- **CQRS**: MediatR 12.4.1
- **Logging**: Serilog
- **Documentación**: Swagger/OpenAPI
- **Testing**: xUnit, Moq, FluentAssertions

## 📌 Roadmap

- [ ] SDK para clientes (JavaScript, Python)
- [ ] Dashboard de administración
- [ ] Webhooks para cambios de estado
- [ ] A/B Testing integrado
- [ ] Métricas de uso por flag
- [ ] Integración con OpenTelemetry

## 👥 Contribución

1. Fork el repositorio
2. Crear rama feature (`git checkout -b feature/amazing-feature`)
3. Commit cambios (`git commit -m 'Add amazing feature'`)
4. Push a la rama (`git push origin feature/amazing-feature`)
5. Abrir Pull Request

## 📄 Licencia

Este proyecto es parte del sistema CarDealer y sigue las políticas internas de desarrollo.
