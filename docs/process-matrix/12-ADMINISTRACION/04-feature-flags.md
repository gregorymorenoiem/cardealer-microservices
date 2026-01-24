# 🚩 Feature Flags - Gestión de Características - Matriz de Procesos

> **Servicio:** AdminService / FeatureFlagService  
> **Base de datos:** PostgreSQL + Redis  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente                     | Total | Implementado | Pendiente | Estado         |
| ------------------------------ | ----- | ------------ | --------- | -------------- |
| **Controllers**                | 1     | 0            | 1         | 🔴 Pendiente   |
| **FF-CRUD-\*** (CRUD Flags)    | 5     | 0            | 5         | 🔴 Pendiente   |
| **FF-EVAL-\*** (Evaluación)    | 4     | 0            | 4         | 🔴 Pendiente   |
| **FF-TARGET-\*** (Targeting)   | 4     | 0            | 4         | 🔴 Pendiente   |
| **FF-ROLLOUT-\*** (Despliegue) | 3     | 0            | 3         | 🔴 Pendiente   |
| **FF-AUDIT-\*** (Auditoría)    | 2     | 0            | 2         | 🔴 Pendiente   |
| **Tests**                      | 0     | 0            | 20        | 🔴 Pendiente   |
| **TOTAL**                      | 19    | 0            | 19        | 🔴 0% Completo |

---

## 1. Información General

### 1.1 Descripción

Sistema de feature flags que permite habilitar/deshabilitar funcionalidades de forma dinámica sin desplegar nuevo código. Soporta rollout gradual, targeting por usuario/segmento, y A/B testing.

### 1.2 Tipos de Feature Flags

| Tipo            | Descripción                       | Ejemplo                    |
| --------------- | --------------------------------- | -------------------------- |
| **Release**     | Nuevas features en desarrollo     | `enable_new_checkout`      |
| **Experiment**  | A/B testing                       | `ab_test_hero_design`      |
| **Ops**         | Control operacional               | `enable_maintenance_mode`  |
| **Kill Switch** | Desactivar features problemáticas | `disable_image_processing` |
| **Permission**  | Features por plan/rol             | `dealer_pro_analytics`     |

### 1.3 Targeting

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Feature Flag Targeting                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   ┌───────────────────────────────────────────────────────────────────┐ │
│   │                        Feature Flag                                │ │
│   │                   "enable_new_search"                              │ │
│   └───────────────────────────────────────────────────────────────────┘ │
│                                │                                         │
│                                ▼                                         │
│                    ┌───────────────────────┐                            │
│                    │     Is Enabled?       │                            │
│                    └───────────┬───────────┘                            │
│                                │                                         │
│         ┌──────────────────────┼──────────────────────┐                 │
│         ▼                      ▼                      ▼                 │
│   ┌───────────┐         ┌───────────┐         ┌───────────┐            │
│   │  Global   │         │  Segment  │         │   User    │            │
│   │  100% ON  │         │  Dealers  │         │  Specific │            │
│   └───────────┘         │   Only    │         │   Beta    │            │
│                         └───────────┘         │  Testers  │            │
│                                               └───────────┘            │
│                                                                          │
│   Targeting Rules (Priority Order):                                     │
│   1. User Override (específico para usuario)                            │
│   2. Segment Match (dealer, admin, premium)                             │
│   3. Percentage Rollout (10%, 25%, 50%, 100%)                          │
│   4. Default Value (off)                                                │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

| Método   | Endpoint                             | Descripción                 | Rol Requerido |
| -------- | ------------------------------------ | --------------------------- | ------------- |
| `GET`    | `/api/features`                      | Listar feature flags        | Admin         |
| `GET`    | `/api/features/{key}`                | Obtener feature flag        | Admin         |
| `POST`   | `/api/features`                      | Crear feature flag          | SuperAdmin    |
| `PUT`    | `/api/features/{key}`                | Actualizar feature flag     | SuperAdmin    |
| `DELETE` | `/api/features/{key}`                | Eliminar feature flag       | SuperAdmin    |
| `PUT`    | `/api/features/{key}/toggle`         | Toggle on/off               | SuperAdmin    |
| `PUT`    | `/api/features/{key}/rollout`        | Actualizar rollout %        | SuperAdmin    |
| `POST`   | `/api/features/{key}/segments`       | Agregar segmento            | SuperAdmin    |
| `DELETE` | `/api/features/{key}/segments/{id}`  | Quitar segmento             | SuperAdmin    |
| `POST`   | `/api/features/{key}/users`          | Agregar override de usuario | SuperAdmin    |
| `DELETE` | `/api/features/{key}/users/{userId}` | Quitar override             | SuperAdmin    |
| `GET`    | `/api/features/evaluate`             | Evaluar flags para usuario  | Authenticated |
| `GET`    | `/api/features/{key}/metrics`        | Métricas del flag           | Admin         |

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
    public bool IsEnabled { get; set; }
    public int RolloutPercentage { get; set; } // 0-100

    public string? Variants { get; set; } // JSON para A/B testing
    public string? DefaultVariant { get; set; }

    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public ICollection<FeatureFlagSegment> Segments { get; set; } = new List<FeatureFlagSegment>();
    public ICollection<FeatureFlagUserOverride> UserOverrides { get; set; } = new List<FeatureFlagUserOverride>();
}

public enum FeatureFlagType
{
    Release,
    Experiment,
    Ops,
    KillSwitch,
    Permission
}
```

### 3.2 FeatureFlagSegment

```csharp
public class FeatureFlagSegment
{
    public Guid Id { get; set; }
    public Guid FeatureFlagId { get; set; }

    public string SegmentType { get; set; } = string.Empty; // Role, Plan, AccountType, Custom
    public string SegmentValue { get; set; } = string.Empty; // Admin, Pro, Dealer, etc.
    public bool IsEnabled { get; set; } = true;
    public string? Variant { get; set; } // Para A/B testing

    // Navigation
    public FeatureFlag FeatureFlag { get; set; } = null!;
}
```

### 3.3 FeatureFlagUserOverride

```csharp
public class FeatureFlagUserOverride
{
    public Guid Id { get; set; }
    public Guid FeatureFlagId { get; set; }
    public Guid UserId { get; set; }

    public bool IsEnabled { get; set; }
    public string? Variant { get; set; }
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }

    // Navigation
    public FeatureFlag FeatureFlag { get; set; } = null!;
}
```

### 3.4 FeatureFlagEvaluation (para métricas)

```csharp
public class FeatureFlagEvaluation
{
    public Guid Id { get; set; }
    public Guid FeatureFlagId { get; set; }
    public Guid? UserId { get; set; }

    public bool Result { get; set; }
    public string? Variant { get; set; }
    public string MatchReason { get; set; } = string.Empty; // UserOverride, Segment, Rollout, Default

    public DateTime EvaluatedAt { get; set; }
    public string? Context { get; set; } // JSON con contexto adicional
}
```

---

## 4. Feature Flags del Sistema

### 4.1 Release Flags

```json
{
  "enable_new_search_ui": {
    "type": "Release",
    "description": "Nueva UI de búsqueda con filtros avanzados",
    "rollout": 50,
    "segments": ["internal_testers"]
  },
  "enable_ai_pricing": {
    "type": "Release",
    "description": "Sugerencias de precio con IA",
    "rollout": 0,
    "segments": ["dealer_enterprise"]
  },
  "enable_360_view": {
    "type": "Release",
    "description": "Vista 360° de vehículos",
    "rollout": 100,
    "segments": []
  }
}
```

### 4.2 Experiment Flags

```json
{
  "ab_hero_cta_color": {
    "type": "Experiment",
    "variants": {
      "control": { "color": "#3B82F6" },
      "variant_a": { "color": "#10B981" },
      "variant_b": { "color": "#F59E0B" }
    },
    "distribution": [50, 25, 25]
  },
  "ab_pricing_display": {
    "type": "Experiment",
    "variants": {
      "monthly": { "display": "monthly" },
      "yearly": { "display": "yearly_with_savings" }
    },
    "distribution": [50, 50]
  }
}
```

### 4.3 Ops Flags

```json
{
  "maintenance_mode": {
    "type": "Ops",
    "description": "Modo mantenimiento del sitio",
    "enabled": false
  },
  "enable_new_payment_gateway": {
    "type": "Ops",
    "description": "Activar nueva pasarela de pagos",
    "enabled": false,
    "segments": ["internal_only"]
  },
  "log_verbose_mode": {
    "type": "Ops",
    "description": "Logging detallado para debugging",
    "enabled": false
  }
}
```

### 4.4 Kill Switches

```json
{
  "disable_image_upload": {
    "type": "KillSwitch",
    "description": "Desactivar upload de imágenes (emergencia)",
    "enabled": false
  },
  "disable_external_payments": {
    "type": "KillSwitch",
    "description": "Desactivar pagos externos",
    "enabled": false
  },
  "disable_notifications": {
    "type": "KillSwitch",
    "description": "Desactivar envío de notificaciones",
    "enabled": false
  }
}
```

### 4.5 Permission Flags

```json
{
  "dealer_analytics_dashboard": {
    "type": "Permission",
    "description": "Dashboard de analytics para dealers",
    "segments": ["dealer_pro", "dealer_enterprise"]
  },
  "bulk_import_vehicles": {
    "type": "Permission",
    "description": "Importación masiva de vehículos",
    "segments": ["dealer_pro", "dealer_enterprise"]
  },
  "api_access": {
    "type": "Permission",
    "description": "Acceso a API pública",
    "segments": ["dealer_enterprise"]
  }
}
```

---

## 5. Procesos Detallados

### 5.1 FF-001: Evaluar Feature Flag

| Paso | Acción                                | Sistema        | Validación                   |
| ---- | ------------------------------------- | -------------- | ---------------------------- |
| 1    | Request llega con contexto de usuario | API            | Usuario identificado         |
| 2    | Buscar flag en cache                  | Redis          | Cache hit/miss               |
| 3    | Si cache miss, cargar de DB           | PostgreSQL     | Flag existe                  |
| 4    | Verificar si flag está habilitado     | FeatureService | IsEnabled                    |
| 5    | Verificar fecha de vigencia           | FeatureService | StartsAt/ExpiresAt           |
| 6    | Buscar override de usuario            | FeatureService | Override existe?             |
| 7    | Si no override, evaluar segmentos     | FeatureService | Segment match?               |
| 8    | Si no segment, evaluar rollout %      | FeatureService | Hash(userId) % 100 < rollout |
| 9    | Registrar evaluación (async)          | FeatureService | Métricas                     |
| 10   | Retornar resultado                    | API            | Boolean/Variant              |

```csharp
public class FeatureFlagEvaluator
{
    public async Task<FeatureEvaluationResult> EvaluateAsync(
        string flagKey,
        FeatureContext context,
        CancellationToken ct = default)
    {
        // 1. Get flag (cached)
        var flag = await _flagRepository.GetByKeyAsync(flagKey, ct);

        if (flag == null)
            return FeatureEvaluationResult.Disabled("flag_not_found");

        // 2. Check if globally enabled
        if (!flag.IsEnabled)
            return FeatureEvaluationResult.Disabled("globally_disabled");

        // 3. Check date range
        if (flag.StartsAt.HasValue && DateTime.UtcNow < flag.StartsAt.Value)
            return FeatureEvaluationResult.Disabled("not_started");

        if (flag.ExpiresAt.HasValue && DateTime.UtcNow > flag.ExpiresAt.Value)
            return FeatureEvaluationResult.Disabled("expired");

        // 4. Check user override (highest priority)
        if (context.UserId.HasValue)
        {
            var userOverride = flag.UserOverrides
                .FirstOrDefault(o => o.UserId == context.UserId.Value);

            if (userOverride != null)
                return new FeatureEvaluationResult(
                    userOverride.IsEnabled,
                    userOverride.Variant,
                    "user_override");
        }

        // 5. Check segments
        foreach (var segment in flag.Segments.Where(s => s.IsEnabled))
        {
            if (MatchesSegment(segment, context))
            {
                return new FeatureEvaluationResult(
                    true,
                    segment.Variant,
                    $"segment:{segment.SegmentType}:{segment.SegmentValue}");
            }
        }

        // 6. Check percentage rollout
        if (flag.RolloutPercentage > 0 && context.UserId.HasValue)
        {
            var hash = ComputeHash(flagKey, context.UserId.Value);
            var bucket = hash % 100;

            if (bucket < flag.RolloutPercentage)
            {
                var variant = flag.Type == FeatureFlagType.Experiment
                    ? SelectVariant(flag, hash)
                    : null;

                return new FeatureEvaluationResult(
                    true,
                    variant,
                    $"rollout:{bucket}/{flag.RolloutPercentage}");
            }
        }

        // 7. Default: disabled
        return FeatureEvaluationResult.Disabled("default");
    }

    private bool MatchesSegment(FeatureFlagSegment segment, FeatureContext context)
    {
        return segment.SegmentType switch
        {
            "Role" => context.Roles?.Contains(segment.SegmentValue) == true,
            "Plan" => context.Plan == segment.SegmentValue,
            "AccountType" => context.AccountType == segment.SegmentValue,
            "Email" => context.Email?.EndsWith(segment.SegmentValue) == true,
            _ => false
        };
    }

    private int ComputeHash(string flagKey, Guid userId)
    {
        // Consistent hashing for stable rollout
        var input = $"{flagKey}:{userId}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToInt32(hash, 0) & 0x7FFFFFFF;
    }
}
```

### 5.2 FF-002: Rollout Gradual

| Paso | Acción                                | Sistema        | Validación        |
| ---- | ------------------------------------- | -------------- | ----------------- |
| 1    | Admin configura rollout inicial (10%) | Frontend Admin | 0-100%            |
| 2    | Monitorear métricas                   | Grafana        | Sin errores       |
| 3    | Incrementar a 25%                     | Admin          | Métricas OK       |
| 4    | Monitorear 24h                        | Grafana        | Sin degradación   |
| 5    | Incrementar a 50%                     | Admin          | Métricas OK       |
| 6    | Monitorear 48h                        | Grafana        | Sin issues        |
| 7    | Incrementar a 100%                    | Admin          | Full rollout      |
| 8    | Marcar flag como deprecated           | Admin          | Cleanup scheduled |

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Gradual Rollout Strategy                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Day 1        Day 3        Day 5        Day 7        Day 14           │
│     │            │            │            │            │               │
│     ▼            ▼            ▼            ▼            ▼               │
│   ┌────┐      ┌────┐      ┌────┐      ┌────┐      ┌────┐              │
│   │10% │ ───▶ │25% │ ───▶ │50% │ ───▶ │75% │ ───▶ │100%│              │
│   └────┘      └────┘      └────┘      └────┘      └────┘              │
│                                                                          │
│   Monitor:                                                              │
│   ├── Error rate < 0.5%                                                │
│   ├── Latency P95 < +10%                                               │
│   ├── No critical alerts                                               │
│   └── User feedback positive                                           │
│                                                                          │
│   Rollback Criteria:                                                    │
│   ├── Error rate > 1%                                                  │
│   ├── Latency P95 > +50%                                               │
│   └── Critical user complaints                                         │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.3 FF-003: A/B Testing

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     A/B Test Lifecycle                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  1. SETUP                                                               │
│     ├── Define hypothesis                                               │
│     ├── Define success metrics (CTR, conversion, etc.)                 │
│     ├── Calculate sample size                                          │
│     └── Create variants                                                 │
│                                                                          │
│  2. RUNNING                                                             │
│     ┌─────────────────────────────────────────────────────────────────┐│
│     │                      Traffic Split                               ││
│     │                           │                                      ││
│     │           ┌───────────────┼───────────────┐                     ││
│     │           ▼               ▼               ▼                     ││
│     │      ┌─────────┐    ┌─────────┐    ┌─────────┐                  ││
│     │      │ Control │    │Variant A│    │Variant B│                  ││
│     │      │   34%   │    │   33%   │    │   33%   │                  ││
│     │      └─────────┘    └─────────┘    └─────────┘                  ││
│     │                                                                  ││
│     │  Track Events:                                                   ││
│     │  - variant_shown                                                 ││
│     │  - button_clicked                                                ││
│     │  - conversion_completed                                          ││
│     └─────────────────────────────────────────────────────────────────┘│
│                                                                          │
│  3. ANALYSIS                                                            │
│     ├── Statistical significance (p < 0.05)                            │
│     ├── Confidence interval                                            │
│     └── Declare winner                                                 │
│                                                                          │
│  4. ROLLOUT                                                             │
│     ├── 100% to winning variant                                        │
│     ├── Update codebase to remove variants                             │
│     └── Archive experiment                                             │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 6. SDK/Client Usage

### 6.1 Backend Usage

```csharp
public class VehicleSearchController : ControllerBase
{
    private readonly IFeatureFlagService _features;

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchQuery query)
    {
        var context = new FeatureContext
        {
            UserId = User.GetUserId(),
            Roles = User.GetRoles(),
            Plan = User.GetPlan(),
            AccountType = User.GetAccountType()
        };

        // Simple boolean check
        if (await _features.IsEnabledAsync("enable_new_search_ui", context))
        {
            return await ExecuteNewSearch(query);
        }

        // A/B test with variant
        var heroResult = await _features.EvaluateAsync("ab_hero_cta_color", context);
        ViewData["CtaColor"] = heroResult.Variant ?? "default";

        return await ExecuteLegacySearch(query);
    }
}
```

### 6.2 Frontend Usage

```typescript
// hooks/useFeatureFlag.ts
export function useFeatureFlag(flagKey: string): boolean {
  const { user } = useAuth();
  const { data } = useQuery({
    queryKey: ['features', flagKey, user?.id],
    queryFn: () => featureApi.evaluate(flagKey),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  return data?.enabled ?? false;
}

// Usage in component
function SearchPage() {
  const enableNewSearch = useFeatureFlag('enable_new_search_ui');

  if (enableNewSearch) {
    return <NewSearchUI />;
  }

  return <LegacySearchUI />;
}
```

---

## 7. Reglas de Negocio

| Código | Regla                                               | Validación                              |
| ------ | --------------------------------------------------- | --------------------------------------- |
| FF-R01 | Kill switches tienen prioridad máxima               | Type == KillSwitch → ignore other rules |
| FF-R02 | User overrides tienen mayor prioridad que segmentos | Orden: Override > Segment > Rollout     |
| FF-R03 | Rollout debe ser consistente por usuario            | Hash(flagKey + userId)                  |
| FF-R04 | Experiments requieren mínimo 2 variantes            | Variants.Count >= 2                     |
| FF-R05 | Flags expirados se deshabilitan automáticamente     | ExpiresAt < Now                         |
| FF-R06 | Cambios se propagan en <1 minuto                    | Cache TTL = 60s                         |

---

## 8. Códigos de Error

| Código   | HTTP | Mensaje                      | Causa          |
| -------- | ---- | ---------------------------- | -------------- |
| `FF_001` | 404  | Feature flag not found       | Key no existe  |
| `FF_002` | 400  | Invalid rollout percentage   | Fuera de 0-100 |
| `FF_003` | 400  | Invalid variant distribution | No suma 100%   |
| `FF_004` | 409  | Flag key already exists      | Key duplicado  |
| `FF_005` | 400  | Experiment requires variants | Sin variantes  |

---

## 9. Eventos RabbitMQ

| Evento                    | Exchange          | Descripción      |
| ------------------------- | ----------------- | ---------------- |
| `FeatureFlagCreatedEvent` | `features.events` | Flag creado      |
| `FeatureFlagUpdatedEvent` | `features.events` | Flag actualizado |
| `FeatureFlagToggledEvent` | `features.events` | Flag toggled     |
| `FeatureFlagDeletedEvent` | `features.events` | Flag eliminado   |

---

## 10. Métricas Prometheus

```
# Evaluaciones de flags
feature_flag_evaluations_total{flag="...", result="enabled|disabled", reason="..."}

# Latencia de evaluación
feature_flag_evaluation_duration_seconds

# Flags activos
feature_flags_active_total{type="..."}

# A/B test impressions
ab_test_impressions_total{experiment="...", variant="..."}

# A/B test conversions
ab_test_conversions_total{experiment="...", variant="..."}
```

---

## 📚 Referencias

- [03-system-config.md](03-system-config.md) - Configuración del sistema
- [Martin Fowler - Feature Toggles](https://martinfowler.com/articles/feature-toggles.html)
