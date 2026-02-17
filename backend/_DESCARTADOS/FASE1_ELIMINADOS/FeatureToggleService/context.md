# FeatureToggleService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** FeatureToggleService
- **Puerto en Desarrollo:** 5016
- **Estado:** ⚠️ **SOLO DESARROLLO LOCAL**
- **Base de Datos:** PostgreSQL (`featuretoggleservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de feature flags / feature toggles para habilitar/deshabilitar funcionalidades en producción sin redeploy. Permite A/B testing, gradual rollouts y kill switches.

---

## 🏗️ ARQUITECTURA

```
FeatureToggleService/
├── FeatureToggleService.Api/
│   ├── Controllers/
│   │   ├── FeaturesController.cs
│   │   └── ToggleController.cs
│   └── Program.cs
├── FeatureToggleService.Application/
│   └── Services/
│       ├── FeatureEvaluator.cs
│       └── TargetingService.cs
├── FeatureToggleService.Domain/
│   ├── Entities/
│   │   ├── FeatureFlag.cs
│   │   ├── FeatureVariant.cs
│   │   └── FeatureTarget.cs
│   └── Enums/
│       └── TargetingStrategy.cs
└── FeatureToggleService.Infrastructure/
```

---

## 📦 ENTIDADES

### FeatureFlag
```csharp
public class FeatureFlag
{
    public Guid Id { get; set; }
    public string Key { get; set; }                 // "new-vehicle-dashboard"
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public TargetingStrategy Strategy { get; set; } // All, Percentage, UserIds, Roles
    public string? TargetingRules { get; set; }     // JSON
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Environment { get; set; }
}
```

### TargetingStrategy Enum
```csharp
public enum TargetingStrategy
{
    All,                // Todos los usuarios
    None,               // Ninguno (disabled)
    Percentage,         // % de usuarios
    UserIds,            // Lista específica de user IDs
    Roles,              // Por roles (Admin, Premium, etc.)
    Custom              // Reglas custom en JSON
}
```

---

## 📡 ENDPOINTS API

#### GET `/api/features/{key}/enabled`
Verificar si feature está habilitado para un usuario.

**Query Parameters:**
- `userId`: ID del usuario
- `context`: JSON con contexto adicional (roles, plan, etc.)

**Response (200 OK):**
```json
{
  "key": "new-vehicle-dashboard",
  "enabled": true,
  "variant": "variant-a",
  "reason": "User in test group"
}
```

#### GET `/api/features`
Listar todos los feature flags.

**Response (200 OK):**
```json
{
  "features": [
    {
      "key": "new-vehicle-dashboard",
      "name": "Nuevo Dashboard de Vehículos",
      "isEnabled": true,
      "strategy": "Percentage",
      "targetPercentage": 25
    },
    {
      "key": "crypto-payments",
      "name": "Pagos con Criptomonedas",
      "isEnabled": false,
      "strategy": "None"
    }
  ]
}
```

#### POST `/api/features`
Crear feature flag.

**Request:**
```json
{
  "key": "ai-vehicle-recommendations",
  "name": "Recomendaciones AI",
  "description": "Sistema de recomendaciones basado en ML",
  "isEnabled": true,
  "strategy": "Percentage",
  "targetingRules": {
    "percentage": 10
  },
  "environment": "Production"
}
```

#### PUT `/api/features/{key}/toggle`
Habilitar/Deshabilitar feature.

**Request:**
```json
{
  "enabled": false
}
```

---

## 🎯 ESTRATEGIAS DE TARGETING

### 1. All (Todos)
Feature habilitado para todos los usuarios.

### 2. Percentage (Porcentaje)
Feature habilitado para X% de usuarios (determinista por user ID).

```json
{
  "strategy": "Percentage",
  "targetingRules": {
    "percentage": 25
  }
}
```

### 3. UserIds (Lista de Usuarios)
Feature habilitado solo para usuarios específicos.

```json
{
  "strategy": "UserIds",
  "targetingRules": {
    "userIds": ["user123", "user456"]
  }
}
```

### 4. Roles (Por Roles)
Feature habilitado según rol del usuario.

```json
{
  "strategy": "Roles",
  "targetingRules": {
    "roles": ["Admin", "PremiumUser"]
  }
}
```

### 5. Custom (Reglas Personalizadas)
Reglas complejas combinando múltiples condiciones.

```json
{
  "strategy": "Custom",
  "targetingRules": {
    "conditions": [
      {
        "field": "plan",
        "operator": "equals",
        "value": "Premium"
      },
      {
        "field": "country",
        "operator": "in",
        "value": ["DO", "US"]
      }
    ],
    "operator": "AND"
  }
}
```

---

## 💡 CASOS DE USO

### 1. Gradual Rollout
Lanzar feature a 10% → 25% → 50% → 100% de usuarios.

```
Day 1: 10% enabled
Day 3: 25% enabled
Day 7: 50% enabled
Day 14: 100% enabled
```

### 2. A/B Testing
Probar 2 variantes de una funcionalidad.

```json
{
  "key": "checkout-flow",
  "variants": [
    { "name": "variant-a", "percentage": 50 },
    { "name": "variant-b", "percentage": 50 }
  ]
}
```

### 3. Kill Switch
Deshabilitar feature rápidamente en caso de problemas.

```bash
curl -X PUT /api/features/problematic-feature/toggle \
  -d '{"enabled": false}'
```

### 4. Beta Testing
Habilitar solo para usuarios beta.

```json
{
  "strategy": "Roles",
  "targetingRules": {
    "roles": ["BetaTester"]
  }
}
```

---

## 🔄 EVALUACIÓN EN CLIENTE

### Backend (C#)
```csharp
public async Task<bool> IsFeatureEnabled(string featureKey, Guid userId)
{
    var response = await _httpClient.GetAsync(
        $"/api/features/{featureKey}/enabled?userId={userId}"
    );
    var result = await response.Content.ReadFromJsonAsync<FeatureCheckResult>();
    return result.Enabled;
}
```

### Frontend (TypeScript)
```typescript
async function isFeatureEnabled(key: string, userId: string): Promise<boolean> {
  const response = await fetch(
    `/api/features/${key}/enabled?userId=${userId}`
  );
  const result = await response.json();
  return result.enabled;
}
```

---

## 🚀 ALTERNATIVAS EN PRODUCCIÓN

- **LaunchDarkly**: Feature flags managed service (líder del mercado)
- **Unleash**: Open source, self-hosted
- **Split.io**: Feature flags + A/B testing
- **ConfigCat**: Feature flags managed
- **Azure App Configuration**: Feature flags de Microsoft
- **Firebase Remote Config**: Para apps mobile

---

## 📝 BEST PRACTICES

### Naming Convention
```
feature-name-action
```
Ejemplos:
- `vehicle-recommendations-enabled`
- `payment-crypto-enabled`
- `dashboard-redesign-enabled`

### Cleanup
Eliminar feature flags después de rollout completo (100%).

### Documentation
Documentar por qué existe cada feature flag y cuándo se puede eliminar.

---

**Estado:** Solo desarrollo - Considerar integración con LaunchDarkly en prod  
**Versión:** 1.0.0
