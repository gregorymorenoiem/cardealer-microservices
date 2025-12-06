# 🧪 Testing de Planes y Feature Flags - Mock Data

## 📋 Archivos Implementados

### 1. Mock Data
- **`mockUsers.ts`** - 8 usuarios mock con diferentes planes
- **`MockLoginPage.tsx`** - Página de login con selector de usuarios

### 2. Páginas de Testing
- **`DealerAnalyticsTestPage.tsx`** - Testing de analytics con restricciones por plan
- **`CreateListingTestPage.tsx`** - Testing de límites de listings
- **`PlansComparisonTestPage.tsx`** - Comparación de todos los planes

### 3. Hooks y Componentes (Ya creados)
- **`useDealerFeatures.ts`** - Hook principal para feature flags
- **`UpgradePrompt.tsx`** - Componentes de upgrade y límites

---

## 🚀 Cómo Probar

### 1. Agregar Ruta de Mock Login

Edita `App.tsx` o tu router:

```typescript
import { MockLoginPage } from './pages/MockLoginPage';
import { DealerAnalyticsTestPage } from './pages/dealer/DealerAnalyticsTestPage';
import { CreateListingTestPage } from './pages/dealer/CreateListingTestPage';
import { PlansComparisonTestPage } from './pages/dealer/PlansComparisonTestPage';

// Agrega estas rutas:
<Route path="/mock-login" element={<MockLoginPage />} />
<Route path="/test/dealer/analytics" element={<DealerAnalyticsTestPage />} />
<Route path="/test/dealer/create-listing" element={<CreateListingTestPage />} />
<Route path="/test/dealer/plans" element={<PlansComparisonTestPage />} />
```

### 2. Navegar a Mock Login

```
http://localhost:5174/mock-login
```

### 3. Seleccionar un Usuario

Usuarios disponibles:

| Email | Plan | Listings | Descripción |
|-------|------|----------|-------------|
| `dealer.free@cardealer.com` | FREE | 2/3 | Usuario FREE con espacio disponible |
| `dealer.basic@cardealer.com` | BASIC | 35/50 | Usuario BASIC con uso normal |
| `dealer.pro@cardealer.com` | PRO | 120/200 | Usuario PRO con features avanzadas |
| `dealer.enterprise@cardealer.com` | ENTERPRISE | 550 | Usuario ENTERPRISE (ilimitado) |
| `dealer.freenearlimit@cardealer.com` | FREE | **3/3** | ⚠️ Límite alcanzado |
| `dealer.basicnearlimit@cardealer.com` | BASIC | **48/50** | ⚠️ Cerca del límite |
| `individual@cardealer.com` | N/A | - | Usuario individual (no dealer) |
| `admin@cardealer.com` | N/A | - | Administrador de plataforma |

---

## 🧪 Escenarios de Testing

### Escenario 1: Plan FREE - Analytics Bloqueado
```
1. Login como: dealer.free@cardealer.com
2. Navegar a: /test/dealer/analytics
3. Resultado: UpgradePrompt mostrando que analytics está bloqueado
4. Botón: "Join Waitlist" (planes pagados no disponibles aún)
```

### Escenario 2: Plan BASIC - Market Analysis Bloqueado
```
1. Login como: dealer.basic@cardealer.com
2. Navegar a: /test/dealer/analytics
3. Resultado: 
   - Analytics básicos ✅ funcionando
   - Market Price Analysis ❌ bloqueado (requiere PRO)
4. Botón: "Upgrade to PRO"
```

### Escenario 3: Plan PRO - Todas las Features Desbloqueadas
```
1. Login como: dealer.pro@cardealer.com
2. Navegar a: /test/dealer/analytics
3. Resultado: Todo desbloqueado (analytics + market analysis)
```

### Escenario 4: Plan FREE - Límite Alcanzado
```
1. Login como: dealer.freenearlimit@cardealer.com
2. Navegar a: /test/dealer/create-listing
3. Resultado:
   - Banner rojo: "You've reached your limit of 3 listings"
   - Formulario deshabilitado (opacity 50%)
   - Botón "Upgrade Plan" visible
```

### Escenario 5: Plan BASIC - Cerca del Límite
```
1. Login como: dealer.basicnearlimit@cardealer.com
2. Navegar a: /test/dealer/create-listing
3. Resultado:
   - Progress bar al 96% (48/50)
   - Warning amarillo: "You're running out of listing slots"
   - Formulario activo pero con advertencia
```

### Escenario 6: Comparación de Planes
```
1. Login como cualquier dealer
2. Navegar a: /test/dealer/plans
3. Resultado:
   - Tabla comparativa de los 4 planes
   - Tu plan actual marcado
   - Features con ✓ o ✗ según disponibilidad
   - Sección "Your Current Access" mostrando features desbloqueadas
```

---

## 📊 Verificación de Features por Plan

### FREE Plan (3 listings)
```typescript
{
  maxListings: 3,
  maxImages: 5,
  analyticsAccess: false,          // ❌ Bloqueado
  marketPriceAnalysis: false,      // ❌ Bloqueado
  bulkUpload: false,               // ❌ Bloqueado
  featuredListings: 0,
  leadManagement: false,
  emailAutomation: false,
  customBranding: false,
  apiAccess: false,
  prioritySupport: false,
  whatsappIntegration: false
}
```

### BASIC Plan (50 listings)
```typescript
{
  maxListings: 50,
  maxImages: 10,
  analyticsAccess: true,           // ✅ Desbloqueado
  marketPriceAnalysis: false,      // ❌ Requiere PRO
  bulkUpload: true,                // ✅ Desbloqueado
  featuredListings: 2,
  leadManagement: true,
  emailAutomation: false,
  customBranding: false,
  apiAccess: false,
  prioritySupport: false,
  whatsappIntegration: false
}
```

### PRO Plan (200 listings)
```typescript
{
  maxListings: 200,
  maxImages: 20,
  analyticsAccess: true,
  marketPriceAnalysis: true,       // ✅ Desbloqueado
  bulkUpload: true,
  featuredListings: 10,
  leadManagement: true,
  emailAutomation: true,           // ✅ Desbloqueado
  customBranding: true,            // ✅ Desbloqueado
  apiAccess: false,                // ❌ Requiere ENTERPRISE
  prioritySupport: true,
  whatsappIntegration: true
}
```

### ENTERPRISE Plan (∞ listings)
```typescript
{
  maxListings: 999999, // Ilimitado
  maxImages: 50,
  analyticsAccess: true,
  marketPriceAnalysis: true,
  bulkUpload: true,
  featuredListings: 50,
  leadManagement: true,
  emailAutomation: true,
  customBranding: true,
  apiAccess: true,                 // ✅ Desbloqueado
  prioritySupport: true,
  whatsappIntegration: true
}
```

---

## 🎨 Componentes UI a Verificar

### 1. UpgradePrompt
**Se muestra cuando:**
- FREE intenta acceder a analytics
- BASIC intenta acceder a market analysis
- Cualquier plan intenta acceder a una feature bloqueada

**Debe mostrar:**
- 🔒 Icono de lock
- Nombre de la feature bloqueada
- Plan recomendado para desbloquear
- Botón "Join Waitlist" o "Upgrade to X"
- Features incluidas en el plan recomendado

### 2. LimitReachedBanner
**Se muestra cuando:**
- Usuario alcanza el límite de listings
- Usuario alcanza el límite de featured listings

**Debe mostrar:**
- ⚠️ Mensaje de límite alcanzado
- Uso actual (ej: 3/3)
- Siguiente plan con límite mayor
- Botón "Upgrade Plan"

### 3. Progress Bar
**Se muestra en:**
- Página de crear listing
- Dashboard (opcional)

**Debe mostrar:**
- Porcentaje de uso (ej: 67%)
- Color dinámico:
  - Azul < 70%
  - Amarillo 70-90%
  - Rojo > 90%
- Warning cuando > 80%

---

## 🔍 Debugging

### Ver el usuario actual:
```typescript
const user = useAuthStore((state) => state.user);
console.log('User:', user);
console.log('Subscription:', user?.subscription);
```

### Ver features disponibles:
```typescript
const { limits, canAccess } = useDealerFeatures(user?.subscription);
console.log('Plan limits:', limits);
console.log('Can access analytics?', canAccess('analyticsAccess'));
```

### Ver uso actual:
```typescript
const { usage, hasReachedLimit } = useDealerFeatures(user?.subscription);
console.log('Usage:', usage);
console.log('At limit?', hasReachedLimit('listings'));
```

---

## ✅ Checklist de Testing

### Funcionalidad Básica
- [ ] Login con cada usuario funciona
- [ ] Usuario se almacena en localStorage (persist)
- [ ] Refresh mantiene el usuario logueado
- [ ] Logout limpia el estado

### Feature Flags
- [ ] FREE no ve analytics
- [ ] BASIC ve analytics pero no market analysis
- [ ] PRO ve todo analytics
- [ ] ENTERPRISE tiene acceso a todo

### Límites
- [ ] FREE con 3/3 no puede crear listing
- [ ] Progress bar cambia de color según %
- [ ] Warning aparece > 80%
- [ ] Banner de límite aparece cuando alcanza max

### UI/UX
- [ ] UpgradePrompt se ve bien
- [ ] Botones "Join Waitlist" para planes no disponibles
- [ ] Tabla de comparación muestra correctamente
- [ ] Progress bar anima suavemente
- [ ] Plan actual está destacado

---

## 🎯 Próximos Pasos (cuando estés listo)

1. **Conectar con backend real:**
   ```typescript
   // En lugar de MOCK_USERS, llamar a API
   const response = await api.post('/auth/login', { email, password });
   login(response.data);
   ```

2. **Implementar Stripe/PayPal:**
   ```typescript
   const handleUpgrade = async (plan: DealerPlan) => {
     const checkout = await api.post('/billing/create-checkout', { plan });
     window.location.href = checkout.url;
   };
   ```

3. **Habilitar planes pagados:**
   ```typescript
   // En useDealerFeatures.ts
   export const DEALER_PLAN_PRICING = {
     [DealerPlan.BASIC]: {
       available: true,  // 👈 Cambiar a true
     },
   };
   ```

---

## 📞 Soporte

Para preguntas:
- Hook: `frontend/web/src/hooks/useDealerFeatures.ts`
- Mock users: `frontend/web/src/mocks/mockUsers.ts`
- Componentes: `frontend/web/src/components/dealer/UpgradePrompt.tsx`

**Autor**: GitHub Copilot  
**Fecha**: Diciembre 5, 2025
