# 🚩 Feature Toggle Service - Feature Flags - Matriz de Procesos

> **Servicio:** Feature Toggle (no implementado)  
> **Puerto:** N/A  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** 🔴 0% Backend | 🔴 0% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso            | Backend | UI Access | Observación     |
| ------------------ | ------- | --------- | --------------- |
| Feature Flags CRUD | 🔴 0%   | 🔴 0%     | No implementado |
| Flag Evaluation    | 🔴 0%   | 🔴 0%     | No implementado |
| Targeting          | 🔴 0%   | 🔴 0%     | No implementado |
| A/B Testing        | 🔴 0%   | 🔴 0%     | No implementado |

### Rutas UI Existentes ✅

- Ninguna

### Rutas UI Faltantes 🔴

- `/admin/features` - Gestión de feature flags
- `/admin/features/ab-tests` - Configuración A/B

**Nota:** Feature toggles es una funcionalidad de fase 2. Considerar LaunchDarkly o implementación custom.

---

## 📊 Resumen de Implementación

| Componente                   | Total | Implementado | Pendiente | Estado         |
| ---------------------------- | ----- | ------------ | --------- | -------------- |
| **FT-CRUD-\*** (CRUD Flags)  | 5     | 0            | 5         | 🔴 Pendiente   |
| **FT-EVAL-\*** (Evaluación)  | 4     | 0            | 4         | 🔴 Pendiente   |
| **FT-TARGET-\*** (Targeting) | 4     | 0            | 4         | 🔴 Pendiente   |
| **FT-ROLLOUT-\*** (Gradual)  | 3     | 0            | 3         | 🔴 Pendiente   |
| **FT-AB-\*** (A/B Testing)   | 3     | 0            | 3         | 🔴 Pendiente   |
| **Tests**                    | 0     | 0            | 18        | 🔴 Pendiente   |
| **TOTAL**                    | 19    | 0            | 19        | 🔴 0% Completo |

---

## 1. Información General

### 1.1 Descripción

Sistema de feature flags para controlar el lanzamiento gradual de funcionalidades, A/B testing, y kill switches. Permite activar/desactivar features sin redeployment, con targeting por usuario, dealer, o segmento.

### 1.2 Tipos de Feature Flags

| Tipo           | Descripción                 | Ejemplo              |
| -------------- | --------------------------- | -------------------- |
| **Release**    | On/Off para nuevas features | `new-search-ui`      |
| **Experiment** | A/B testing                 | `checkout-variant-b` |
| **Ops**        | Kill switches               | `disable-payments`   |
| **Permission** | Por usuario/rol             | `beta-testers`       |
| **Gradual**    | Rollout %                   | `new-algorithm: 25%` |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Feature Toggle Architecture                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Evaluation Request                                                    │
│   { userId: "abc", dealerId: "xyz", context: {...} }                    │
│        │                                                                │
│        ▼                                                                │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                  Feature Toggle Engine                           │   │
│   │                                                                   │   │
│   │   1. Load feature definition                                     │   │
│   │   2. Check if globally enabled                                   │   │
│   │   3. Evaluate targeting rules                                    │   │
│   │   4. Check percentage rollout (consistent hashing)               │   │
│   │   5. Return: enabled/disabled + variant                          │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│              ┌────────────────────┼────────────────────┐                │
│              │                    │                    │                 │
│              ▼                    ▼                    ▼                 │
│   ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐        │
│   │   PostgreSQL     │ │      Redis       │ │   Analytics      │        │
│   │   (Definitions)  │ │   (Evaluation    │ │   (Exposure      │        │
│   │                  │ │    Cache)        │ │    Events)       │        │
│   └──────────────────┘ └──────────────────┘ └──────────────────┘        │
│                                                                          │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                     Feature Flag Definition                      │   │
│   │                                                                   │   │
│   │   {                                                              │   │
│   │     "key": "new-search-ui",                                      │   │
│   │     "enabled": true,                                             │   │
│   │     "type": "release",                                           │   │
│   │     "rolloutPercentage": 50,                                     │   │
│   │     "targeting": [                                               │   │
│   │       { "attribute": "dealerPlan", "operator": "in",             │   │
│   │         "values": ["pro", "enterprise"] }                        │   │
│   │     ],                                                           │   │
│   │     "variants": [                                                │   │
│   │       { "name": "control", "weight": 50 },                       │   │
│   │       { "name": "treatment", "weight": 50 }                      │   │
│   │     ]                                                            │   │
│   │   }                                                              │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

### 2.1 Feature Flags CRUD

| Método   | Endpoint              | Descripción            | Auth       |
| -------- | --------------------- | ---------------------- | ---------- |
| `GET`    | `/api/features`       | Listar todos los flags | Admin      |
| `GET`    | `/api/features/{key}` | Obtener flag           | Admin      |
| `POST`   | `/api/features`       | Crear flag             | Admin      |
| `PUT`    | `/api/features/{key}` | Actualizar flag        | Admin      |
| `DELETE` | `/api/features/{key}` | Eliminar flag          | SuperAdmin |

### 2.2 Evaluation

| Método | Endpoint                       | Descripción             | Auth    |
| ------ | ------------------------------ | ----------------------- | ------- |
| `POST` | `/api/features/evaluate`       | Evaluar múltiples flags | Service |
| `GET`  | `/api/features/{key}/evaluate` | Evaluar un flag         | Service |
| `GET`  | `/api/features/client`         | Flags para frontend     | Public  |

### 2.3 Targeting

| Método   | Endpoint                             | Descripción       | Auth  |
| -------- | ------------------------------------ | ----------------- | ----- |
| `POST`   | `/api/features/{key}/targeting`      | Agregar regla     | Admin |
| `DELETE` | `/api/features/{key}/targeting/{id}` | Eliminar regla    | Admin |
| `PUT`    | `/api/features/{key}/rollout`        | Cambiar % rollout | Admin |

### 2.4 Analytics

| Método | Endpoint                        | Descripción         | Auth  |
| ------ | ------------------------------- | ------------------- | ----- |
| `GET`  | `/api/features/{key}/analytics` | Métricas del flag   | Admin |
| `GET`  | `/api/features/{key}/exposures` | Historial exposures | Admin |

---

## 3. Entidades

### 3.1 FeatureFlag

```csharp
public class FeatureFlag
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public FeatureFlagType Type { get; set; }
    public bool Enabled { get; set; }
    public bool Archived { get; set; }

    // Rollout
    public int RolloutPercentage { get; set; } = 100;
    public string? RolloutSalt { get; set; } // For consistent hashing

    // Variants (A/B testing)
    public List<FeatureVariant> Variants { get; set; } = new();

    // Targeting rules
    public List<TargetingRule> TargetingRules { get; set; } = new();

    // Scheduling
    public DateTime? EnabledFrom { get; set; }
    public DateTime? EnabledUntil { get; set; }

    // Metadata
    public Dictionary<string, string> Tags { get; set; } = new();
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum FeatureFlagType
{
    Release,      // Simple on/off
    Experiment,   // A/B test with variants
    Ops,          // Kill switch
    Permission    // User/role based
}

public class FeatureVariant
{
    public string Name { get; set; } = string.Empty;
    public int Weight { get; set; } // 0-100
    public Dictionary<string, object>? Payload { get; set; }
}

public class TargetingRule
{
    public Guid Id { get; set; }
    public int Priority { get; set; } // Lower = higher priority

    public string Attribute { get; set; } = string.Empty; // userId, dealerId, plan, etc.
    public TargetingOperator Operator { get; set; }
    public List<string> Values { get; set; } = new();

    public bool ServeEnabled { get; set; } = true;
    public string? ServeVariant { get; set; }
}

public enum TargetingOperator
{
    Equals,
    NotEquals,
    In,
    NotIn,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
    Regex
}
```

### 3.2 FeatureEvaluation

```csharp
public class EvaluationContext
{
    public string? UserId { get; set; }
    public string? DealerId { get; set; }
    public string? SessionId { get; set; }

    public Dictionary<string, object> Attributes { get; set; } = new();
    // Examples:
    // - "plan": "pro"
    // - "country": "DO"
    // - "signupDate": "2025-06-15"
    // - "totalListings": 45
}

public class EvaluationResult
{
    public string FeatureKey { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string? Variant { get; set; }
    public string Reason { get; set; } = string.Empty; // Why this result
    public Dictionary<string, object>? Payload { get; set; }
}
```

### 3.3 ExposureEvent

```csharp
public class ExposureEvent
{
    public Guid Id { get; set; }
    public string FeatureKey { get; set; } = string.Empty;

    public string? UserId { get; set; }
    public string? DealerId { get; set; }
    public string? SessionId { get; set; }

    public bool Enabled { get; set; }
    public string? Variant { get; set; }
    public string Reason { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 FF-001: Evaluar Feature Flag

| Paso | Acción                   | Sistema    | Validación        |
| ---- | ------------------------ | ---------- | ----------------- |
| 1    | Request con context      | Client     | Context provided  |
| 2    | Buscar flag en cache     | Redis      | Cache hit/miss    |
| 3    | Si miss: cargar de DB    | PostgreSQL | Flag exists       |
| 4    | Verificar si habilitado  | Engine     | enabled = true    |
| 5    | Verificar schedule       | Engine     | Within time range |
| 6    | Evaluar targeting rules  | Engine     | Rules match       |
| 7    | Si experiment: hash user | Engine     | Consistent bucket |
| 8    | Calcular rollout %       | Engine     | Within percentage |
| 9    | Determinar variant       | Engine     | Weighted random   |
| 10   | Registrar exposure       | Analytics  | Event saved       |
| 11   | Retornar resultado       | API        | Response          |

```csharp
public class FeatureEvaluator
{
    public async Task<EvaluationResult> EvaluateAsync(
        string featureKey,
        EvaluationContext context,
        CancellationToken ct = default)
    {
        // 1. Get feature definition
        var feature = await GetFeatureAsync(featureKey, ct);

        if (feature == null)
        {
            return new EvaluationResult
            {
                FeatureKey = featureKey,
                Enabled = false,
                Reason = "Feature not found"
            };
        }

        // 2. Check if globally disabled
        if (!feature.Enabled)
        {
            return new EvaluationResult
            {
                FeatureKey = featureKey,
                Enabled = false,
                Reason = "Feature disabled"
            };
        }

        // 3. Check schedule
        var now = DateTime.UtcNow;
        if (feature.EnabledFrom.HasValue && now < feature.EnabledFrom.Value)
        {
            return new EvaluationResult
            {
                FeatureKey = featureKey,
                Enabled = false,
                Reason = "Not yet enabled (scheduled)"
            };
        }

        if (feature.EnabledUntil.HasValue && now > feature.EnabledUntil.Value)
        {
            return new EvaluationResult
            {
                FeatureKey = featureKey,
                Enabled = false,
                Reason = "Expired (scheduled)"
            };
        }

        // 4. Evaluate targeting rules (in priority order)
        foreach (var rule in feature.TargetingRules.OrderBy(r => r.Priority))
        {
            if (EvaluateRule(rule, context))
            {
                var variant = rule.ServeVariant ?? GetVariant(feature, context);

                await RecordExposureAsync(feature, context, rule.ServeEnabled, variant, "Targeting rule matched");

                return new EvaluationResult
                {
                    FeatureKey = featureKey,
                    Enabled = rule.ServeEnabled,
                    Variant = variant,
                    Reason = $"Targeting rule: {rule.Attribute} {rule.Operator} {string.Join(",", rule.Values)}"
                };
            }
        }

        // 5. Check rollout percentage
        if (feature.RolloutPercentage < 100)
        {
            var bucket = GetConsistentBucket(context.UserId ?? context.SessionId, feature.RolloutSalt);

            if (bucket > feature.RolloutPercentage)
            {
                await RecordExposureAsync(feature, context, false, null, "Outside rollout percentage");

                return new EvaluationResult
                {
                    FeatureKey = featureKey,
                    Enabled = false,
                    Reason = $"Outside rollout ({bucket}% > {feature.RolloutPercentage}%)"
                };
            }
        }

        // 6. Determine variant (for experiments)
        var finalVariant = GetVariant(feature, context);

        await RecordExposureAsync(feature, context, true, finalVariant, "Default enabled");

        return new EvaluationResult
        {
            FeatureKey = featureKey,
            Enabled = true,
            Variant = finalVariant,
            Payload = feature.Variants.FirstOrDefault(v => v.Name == finalVariant)?.Payload,
            Reason = "Default enabled"
        };
    }

    private int GetConsistentBucket(string? identifier, string? salt)
    {
        if (string.IsNullOrEmpty(identifier))
            return Random.Shared.Next(0, 100);

        var input = $"{salt ?? "default"}:{identifier}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        var value = BitConverter.ToUInt32(hash, 0);

        return (int)(value % 100);
    }

    private string? GetVariant(FeatureFlag feature, EvaluationContext context)
    {
        if (feature.Variants.Count == 0)
            return null;

        var bucket = GetConsistentBucket(context.UserId ?? context.SessionId, $"{feature.Key}:variant");
        var cumulative = 0;

        foreach (var variant in feature.Variants)
        {
            cumulative += variant.Weight;
            if (bucket < cumulative)
                return variant.Name;
        }

        return feature.Variants.Last().Name;
    }
}
```

### 4.2 FF-002: Rollout Gradual

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Gradual Rollout Example                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Feature: new-search-ui                                                │
│                                                                          │
│   Day 1: 5%  ▓░░░░░░░░░░░░░░░░░░░                                       │
│   Day 2: 10% ▓▓░░░░░░░░░░░░░░░░░░                                       │
│   Day 3: 25% ▓▓▓▓▓░░░░░░░░░░░░░░░                                       │
│   Day 5: 50% ▓▓▓▓▓▓▓▓▓▓░░░░░░░░░░                                       │
│   Day 7: 75% ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░░                                       │
│   Day 10: 100% ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓                                     │
│                                                                          │
│   Note: Same users always get same result (consistent hashing)          │
│   User "abc123" at 5% rollout:                                          │
│     - Hash("new-search-ui:abc123") = 23                                 │
│     - 23 > 5 → NOT in rollout                                           │
│     - At 25% rollout: 23 < 25 → IN rollout                              │
│     - User gets feature and keeps it                                    │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Feature Flags de OKLA

### 5.1 Flags Actuales

| Key                     | Type       | Status     | Rollout | Targeting      |
| ----------------------- | ---------- | ---------- | ------- | -------------- |
| `early-bird-promo`      | Release    | ✅ On      | 100%    | All users      |
| `new-search-ui`         | Release    | 🔄 Rollout | 50%     | None           |
| `checkout-redesign`     | Experiment | ✅ On      | 100%    | 2 variants     |
| `dealer-analytics-v2`   | Release    | ✅ On      | 100%    | Pro/Enterprise |
| `disable-azul-payments` | Ops        | ❌ Off     | 0%      | Kill switch    |
| `beta-features`         | Permission | ✅ On      | 100%    | Beta testers   |
| `ai-recommendations`    | Experiment | 🔄 Rollout | 25%     | 3 variants     |

### 5.2 Configuración de Early Bird

```json
{
  "key": "early-bird-promo",
  "name": "Early Bird Promotion",
  "type": "release",
  "enabled": true,
  "rolloutPercentage": 100,
  "enabledUntil": "2026-01-31T23:59:59Z",
  "targetingRules": [
    {
      "attribute": "isDealer",
      "operator": "equals",
      "values": ["true"],
      "serveEnabled": true
    }
  ],
  "tags": {
    "team": "growth",
    "sprint": "1"
  }
}
```

---

## 6. SDK para Frontend

```typescript
// Feature Flag Client
class FeatureFlagClient {
  private flags: Map<string, EvaluationResult> = new Map();

  async initialize(context: EvaluationContext): Promise<void> {
    const response = await fetch('/api/features/client', {
      method: 'POST',
      body: JSON.stringify(context)
    });

    const results = await response.json();

    for (const result of results) {
      this.flags.set(result.featureKey, result);
    }
  }

  isEnabled(featureKey: string): boolean {
    return this.flags.get(featureKey)?.enabled ?? false;
  }

  getVariant(featureKey: string): string | null {
    return this.flags.get(featureKey)?.variant ?? null;
  }

  getPayload<T>(featureKey: string): T | null {
    return this.flags.get(featureKey)?.payload as T ?? null;
  }
}

// React Hook
function useFeatureFlag(featureKey: string) {
  const client = useFeatureFlagClient();

  return {
    enabled: client.isEnabled(featureKey),
    variant: client.getVariant(featureKey),
    payload: client.getPayload(featureKey)
  };
}

// Uso
function SearchPage() {
  const { enabled: newSearchUI } = useFeatureFlag('new-search-ui');

  return newSearchUI ? <NewSearchUI /> : <LegacySearchUI />;
}

// A/B Testing
function CheckoutPage() {
  const { variant } = useFeatureFlag('checkout-redesign');

  switch (variant) {
    case 'control':
      return <CheckoutV1 />;
    case 'treatment-a':
      return <CheckoutV2 />;
    case 'treatment-b':
      return <CheckoutV3 />;
    default:
      return <CheckoutV1 />;
  }
}
```

---

## 7. Reglas de Negocio

| Código | Regla                           | Validación              |
| ------ | ------------------------------- | ----------------------- |
| FF-R01 | Kill switches tienen prioridad  | Type = Ops override     |
| FF-R02 | Consistent hashing por user     | Same user = same result |
| FF-R03 | Exposure events para A/B tests  | Track required          |
| FF-R04 | Rollback inmediato disponible   | Enabled = false         |
| FF-R05 | Archivado no elimina historial  | Archived = true         |
| FF-R06 | Schedule override manual toggle | enabledUntil check      |

---

## 8. Métricas Prometheus

```
# Evaluations
feature_evaluations_total{key="...", enabled="true|false", variant="..."}
feature_evaluation_duration_ms{key="..."}

# Rollout
feature_rollout_percentage{key="..."}
feature_targeting_matches_total{key="...", rule="..."}

# Cache
feature_cache_hit_ratio
```

---

## 📚 Referencias

- [11-configuration-service.md](11-configuration-service.md) - Servicio de configuración
- [04-feature-flags.md](../12-ADMINISTRACION/04-feature-flags.md) - Admin de feature flags
- [03-event-tracking.md](../09-REPORTES-ANALYTICS/03-event-tracking.md) - Event tracking
