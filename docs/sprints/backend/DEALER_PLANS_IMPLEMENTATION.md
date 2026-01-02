# 🎯 Sistema de Planes y Feature Flags - Implementación

## 📋 Resumen

Sistema de **Feature Flags basado en planes** que permite:
- ✅ **Plan FREE** por defecto (disponible desde el inicio)
- 🔒 **Planes pagados** (BASIC, PRO, ENTERPRISE) deshabilitados hasta tener tracción
- 🚀 **Feature Flags** dinámicos según el plan del usuario
- 📊 **Control de límites** (listings, featured, images, etc.)

---

## 🏗️ Arquitectura

### Frontend (`frontend/shared/src/types/index.ts`)
```typescript
export enum DealerPlan {
  FREE = 'free',         // ✅ Disponible desde el inicio
  BASIC = 'basic',       // 🔒 Disponible cuando tengas clientes
  PRO = 'pro',           // 🔒 Disponible cuando tengas clientes
  ENTERPRISE = 'enterprise' // 🔒 Disponible cuando tengas clientes
}

// Configuración de límites por plan
export const DEALER_PLAN_LIMITS: Record<DealerPlan, DealerPlanFeatures> = {
  [DealerPlan.FREE]: {
    maxListings: 3,           // Solo 3 vehículos
    maxImages: 5,             // 5 imágenes por vehículo
    analyticsAccess: false,   // ❌ Sin analytics
    bulkUpload: false,        // ❌ Sin carga masiva
    featuredListings: 0,      // ❌ Sin destacados
    // ... más features
  },
  // ... resto de planes
}
```

### Backend (`UserService.Domain/Entities/DealerSubscription.cs`)
```csharp
public enum DealerPlan {
    Free,
    Basic,
    Pro,
    Enterprise
}

public class DealerSubscription {
    public DealerPlan Plan { get; set; } = DealerPlan.Free;
    public int CurrentListings { get; set; }
    public int FeaturedUsed { get; set; }
    // ... más campos
}

// Helper estático con límites
public static class DealerPlanLimits {
    public static DealerPlanFeatures GetFeatures(DealerPlan plan) {
        // Retorna features según el plan
    }
}
```

---

## 💡 Uso en el Frontend

### 1. Hook `useDealerFeatures`

```tsx
import { useDealerFeatures } from '@/hooks/useDealerFeatures';

const MyComponent = () => {
  const user = useAuthStore(state => state.user);
  const { canAccess, hasReachedLimit, currentPlan } = useDealerFeatures(user?.subscription);

  // Verificar acceso a una feature
  if (!canAccess('analyticsAccess')) {
    return <UpgradePrompt feature="analyticsAccess" currentPlan={currentPlan} />;
  }

  // Verificar límite de listings
  if (hasReachedLimit('listings')) {
    return <LimitReachedBanner type="listings" />;
  }

  return <div>Feature habilitada!</div>;
};
```

### 2. Componente `UpgradePrompt`

Muestra automáticamente:
- 🔒 Feature bloqueada
- 📦 Plan recomendado para desbloquearla
- ⚠️ Si el plan no está disponible aún → botón "Join Waitlist"
- ✅ Si el plan está disponible → botón "Upgrade"

```tsx
<UpgradePrompt 
  feature="analyticsAccess" 
  currentPlan={currentPlan}
  onUpgrade={() => window.location.href = '/dealer/billing/upgrade'}
/>
```

### 3. Componente `LimitReachedBanner`

Muestra cuando el usuario alcanza el límite de su plan:

```tsx
<LimitReachedBanner 
  type="listings"
  current={usage.currentListings}
  max={limits.maxListings}
  currentPlan={currentPlan}
/>
```

---

## 🚀 Roadmap de Planes

### Fase 1: Lanzamiento (HOY)
```yaml
Plan disponible: FREE solamente
- maxListings: 3
- analyticsAccess: false
- Todo básico para validar el producto

Estado de planes pagados:
- BASIC: available = false
- PRO: available = false  
- ENTERPRISE: available = false
```

### Fase 2: Primeros 50-100 dealers
```yaml
Habilitar: BASIC ($99/mes)
- maxListings: 50
- analyticsAccess: true
- bulkUpload: true

Configuración:
// En frontend/web/src/hooks/useDealerFeatures.ts
export const DEALER_PLAN_PRICING = {
  [DealerPlan.BASIC]: {
    price: 99,
    available: true,  // 👈 Cambiar a true
  },
}
```

### Fase 3: Tracción significativa (200+ dealers)
```yaml
Habilitar: PRO ($199/mes)
- maxListings: 200
- marketPriceAnalysis: true
- emailAutomation: true
- whatsappIntegration: true
```

### Fase 4: Enterprise clients
```yaml
Habilitar: ENTERPRISE ($499/mes)
- maxListings: unlimited
- apiAccess: true
- customBranding: true
- Dedicated support
```

---

## 🗄️ Base de Datos

### Migración SQL (`20251205_AddMultiLevelRoleSystem.sql`)

```sql
-- Tabla DealerSubscriptions (NUEVA)
CREATE TABLE DealerSubscriptions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    DealerId UNIQUEIDENTIFIER NOT NULL,
    Plan INT NOT NULL DEFAULT 0,  -- 0=Free, 1=Basic, 2=Pro, 3=Enterprise
    Status INT NOT NULL DEFAULT 0, -- 0=Active, 1=Canceled, 2=Expired, 3=Trial
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NULL,
    CurrentListings INT NOT NULL DEFAULT 0,
    FeaturedUsed INT NOT NULL DEFAULT 0,
    StripeSubscriptionId NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Tabla SubscriptionHistory (NUEVA)
CREATE TABLE SubscriptionHistory (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    DealerSubscriptionId UNIQUEIDENTIFIER NOT NULL,
    FromPlan INT NOT NULL,
    ToPlan INT NOT NULL,
    Reason NVARCHAR(50) NOT NULL, -- 'upgrade', 'downgrade', 'canceled'
    ChangedAt DATETIME2 NOT NULL,
    ChangedBy UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_SubscriptionHistory_Subscription 
        FOREIGN KEY (DealerSubscriptionId) REFERENCES DealerSubscriptions(Id)
);
```

---

## 🎨 Ejemplos de Uso

### Ejemplo 1: Página de Analytics

```tsx
export const DealerAnalyticsPage = () => {
  const user = useAuthStore(state => state.user);
  const { canAccess, currentPlan } = useDealerFeatures(user?.subscription);

  if (!canAccess('analyticsAccess')) {
    return (
      <div className="max-w-4xl mx-auto p-6">
        <h1 className="text-3xl font-bold mb-6">Analytics Dashboard</h1>
        <UpgradePrompt 
          feature="analyticsAccess" 
          currentPlan={currentPlan}
          onUpgrade={() => window.location.href = '/dealer/billing/upgrade'}
        />
      </div>
    );
  }

  // Si tiene acceso, mostrar analytics...
  return <AnalyticsDashboard />;
};
```

### Ejemplo 2: Verificar límite antes de crear listing

```tsx
export const CreateListingPage = () => {
  const user = useAuthStore(state => state.user);
  const { hasReachedLimit, usage, limits, currentPlan } = useDealerFeatures(user?.subscription);

  if (hasReachedLimit('listings')) {
    return (
      <div className="max-w-4xl mx-auto p-6">
        <LimitReachedBanner 
          type="listings"
          current={usage.currentListings}
          max={limits.maxListings}
          currentPlan={currentPlan}
        />
      </div>
    );
  }

  // Formulario de creación...
  return <ListingForm />;
};
```

### Ejemplo 3: Mostrar progress bar de uso

```tsx
const { getUsageProgress, usage, limits } = useDealerFeatures(user?.subscription);
const progress = getUsageProgress('listings');

<div className="mb-4">
  <p className="text-sm text-gray-600 mb-2">
    Listings: {usage.currentListings} / {limits.maxListings}
  </p>
  <div className="w-full bg-gray-200 rounded-full h-2">
    <div 
      className={`h-2 rounded-full ${
        progress > 90 ? 'bg-red-500' : 
        progress > 70 ? 'bg-yellow-500' : 
        'bg-blue-500'
      }`}
      style={{ width: `${progress}%` }}
    />
  </div>
</div>
```

---

## 🔐 Control en el Backend

### Middleware de validación de features

```csharp
[HttpPost("bulk-upload")]
[Authorize]
public async Task<IActionResult> BulkUpload([FromBody] BulkUploadRequest request)
{
    var subscription = await _subscriptionService.GetSubscriptionAsync(User.GetDealerId());
    var features = DealerPlanLimits.GetFeatures(subscription.Plan);
    
    if (!features.BulkUpload)
    {
        return Forbidden(new { 
            error = "Feature not available in your plan",
            requiredPlan = "BASIC",
            upgradeUrl = "/dealer/billing/upgrade"
        });
    }
    
    // Procesar bulk upload...
}
```

### Validar límites antes de crear listing

```csharp
[HttpPost("listings")]
[Authorize]
public async Task<IActionResult> CreateListing([FromBody] CreateListingRequest request)
{
    var subscription = await _subscriptionService.GetSubscriptionAsync(User.GetDealerId());
    var features = DealerPlanLimits.GetFeatures(subscription.Plan);
    
    // Verificar límite
    if (subscription.CurrentListings >= features.MaxListings)
    {
        return BadRequest(new { 
            error = "Listing limit reached",
            current = subscription.CurrentListings,
            max = features.MaxListings,
            upgradeUrl = "/dealer/billing/upgrade"
        });
    }
    
    // Crear listing y actualizar contador
    var listing = await _listingService.CreateAsync(request);
    await _subscriptionService.IncrementListingsAsync(subscription.Id);
    
    return Ok(listing);
}
```

---

## ✅ Checklist de Implementación

### Backend
- [x] `DealerSubscription.cs` entity creada
- [x] `DealerPlanLimits` helper creado
- [x] Migración SQL preparada
- [ ] Ejecutar migración en DB
- [ ] Seed data: crear subscripciones FREE para dealers existentes
- [ ] Endpoint GET `/api/subscriptions/{dealerId}`
- [ ] Endpoint POST `/api/subscriptions/upgrade`
- [ ] Middleware de validación de features
- [ ] Incluir `subscription` en JWT claims

### Frontend
- [x] `DealerPlan` enum con FREE
- [x] `DEALER_PLAN_LIMITS` configurado
- [x] `DealerSubscription` interface
- [x] Hook `useDealerFeatures` creado
- [x] Componente `UpgradePrompt` creado
- [x] Componente `LimitReachedBanner` creado
- [ ] Página `/dealer/billing` (mostrar plan actual)
- [ ] Página `/dealer/billing/upgrade` (upgrade flow)
- [ ] Actualizar `authStore` con `subscription`
- [ ] Proteger rutas según features

---

## 🎯 Próximos Pasos

1. **Ejecutar migración SQL**
   ```bash
   # En SSMS o similar
   USE CarDealerDB;
   GO
   -- Pegar contenido de 20251205_AddMultiLevelRoleSystem.sql
   ```

2. **Seed subscripciones FREE para dealers existentes**
   ```sql
   INSERT INTO DealerSubscriptions (Id, DealerId, Plan, Status, StartDate, CurrentListings)
   SELECT NEWID(), Id, 0, 0, GETUTCDATE(), 0
   FROM Dealers
   WHERE NOT EXISTS (SELECT 1 FROM DealerSubscriptions WHERE DealerId = Dealers.Id);
   ```

3. **Actualizar JWT para incluir subscription**
   ```csharp
   // En AuthService
   var subscription = await _subscriptionService.GetSubscriptionAsync(user.DealerId);
   claims.Add(new Claim("subscription", JsonSerializer.Serialize(new {
       plan = subscription.Plan.ToString(),
       status = subscription.Status.ToString(),
       currentListings = subscription.CurrentListings
   })));
   ```

4. **Crear página de billing**
   ```tsx
   // frontend/web/src/pages/dealer/DealerBillingPage.tsx
   - Mostrar plan actual
   - Botón "Upgrade" (disabled si no hay planes disponibles)
   - Historial de cambios de plan
   ```

5. **Cuando estés listo para habilitar planes pagados**
   ```typescript
   // frontend/web/src/hooks/useDealerFeatures.ts
   export const DEALER_PLAN_PRICING = {
     [DealerPlan.BASIC]: {
       price: 99,
       available: true,  // 👈 Cambiar aquí
     },
   };
   ```

---

## 📞 Soporte

Para preguntas sobre implementación, consultar:
- Frontend: `frontend/web/src/pages/dealer/DealerAnalyticsPage.example.tsx`
- Backend: `backend/UserService/UserService.Domain/Entities/DealerSubscription.cs`
- Hook: `frontend/web/src/hooks/useDealerFeatures.ts`

**Autor**: GitHub Copilot  
**Fecha**: Diciembre 5, 2025
