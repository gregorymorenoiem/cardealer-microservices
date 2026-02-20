# 🔧 Prompt: Frontend Merge de Duplicados + Pruebas de Producción (okla.com.do)

**version:** 2.0  
**lastUpdated:** 2026-02-20  
**author:** Gregory Moreno

---

## CONTEXTO GENERAL

Eres un ingeniero fullstack senior trabajando en el proyecto OKLA (cardealer-microservices). Se realizó una auditoría completa del frontend en `frontend/web-next` y se encontraron **19 duplicaciones** de código. Tu tarea es ejecutar el merge/consolidación de TODAS las duplicaciones y después hacer pruebas end-to-end en producción (https://okla.com.do) para validar que todo funciona correctamente.

**Stack tecnológico:**

- Frontend: Next.js 16 + TypeScript + App Router + pnpm (⚠️ NO npm/yarn)
- Backend: .NET 8 microservicios desplegados en DOKS (Digital Ocean Kubernetes)
- DB: PostgreSQL (Managed DO: `okla-db-do-user-31493168-0.g.db.ondigitalocean.com:25060`)
- API Gateway: Ocelot (interno, solo accesible desde pods K8s)
- BFF Pattern: Browser → okla.com.do/api/\* → Next.js rewrite → gateway:8080 → microservicios
- CI/CD: GitHub Actions → GHCR → DOKS (auto-deploy en push a `main`)
- Imágenes Docker: `ghcr.io/gregorymorenoiem/frontend-web:latest`

**Rutas de trabajo:**

- Workspace root: `/Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices`
- Frontend: `frontend/web-next/`
- K8s manifests: `k8s/`
- CI/CD: `.github/workflows/smart-cicd.yml` + `_reusable-frontend.yml`

---

## PARTE 1: MERGE Y CONSOLIDACIÓN DE DUPLICADOS

### TAREA 1.1 — Mover OAuth Callback y eliminar directorio `app/auth/`

**Problema:** Existen dos árboles de autenticación: `app/(auth)/` (canónico, con backend real) y `app/auth/` (legacy, con código fake). El único archivo útil en `app/auth/` es el OAuth callback.

**Acciones específicas:**

1. **Crear** `src/app/(auth)/callback/[provider]/page.tsx` con el contenido EXACTO de `src/app/auth/callback/[provider]/page.tsx` (179 líneas). Este archivo maneja:
   - Exchange del authorization code con el backend via `POST /api/auth/oauth/${provider}/callback`
   - Validación CWE-601 de redirect URLs
   - Manejo de HttpOnly cookies (no localStorage)
   - Refresh de auth state via `useAuth().refreshUser()`

2. **IMPORTANTE al mover el callback:** La URL de redirect OAuth configurada en Google/Apple podría estar apuntando a `/auth/callback/google`. Verificar si `next.config.ts` tiene un redirect de `/auth/callback/:provider` → `/(auth)/callback/:provider`, y si no, agregar uno:

   ```typescript
   // En next.config.ts → redirects()
   {
     source: '/auth/callback/:provider',
     destination: '/callback/:provider', // (auth) es route group invisible
     permanent: false,
   },
   ```

3. **Eliminar** los siguientes archivos (todos son duplicados fake o stubs):
   - `src/app/auth/login/page.tsx` (284 líneas — duplica `(auth)/login/page.tsx`, NO tiene 2FA, NO tiene redirect seguro)
   - `src/app/auth/registro/page.tsx` (14 líneas — ya es un redirect stub)
   - `src/app/auth/recuperar/page.tsx` (124 líneas — usa `setTimeout` fake, NO llama al backend)
   - `src/app/auth/verificar/page.tsx` (147 líneas — usa flujo de 6 dígitos que NO coincide con el backend que es token-based)

4. **Verificar** que no quede ningún archivo en `src/app/auth/` después de mover el callback y eliminar los duplicados. Si el directorio queda vacío, eliminarlo.

5. **Buscar y actualizar** cualquier import o Link que apunte a rutas `/auth/login`, `/auth/registro`, `/auth/recuperar`, `/auth/verificar` en todo el codebase y cambiarlos a:
   - `/auth/login` → `/login`
   - `/auth/registro` → `/registro`
   - `/auth/recuperar` → `/recuperar-contrasena`
   - `/auth/verificar` → `/verificar-email`

   Buscar con: `grep -rn "'/auth/login\|'/auth/registro\|'/auth/recuperar\|'/auth/verificar\|href=\"/auth/" src/`

---

### TAREA 1.2 — Reemplazar `/vender/publicar` con redirect a `/publicar`

**Problema:** Existen dos wizards de publicación de vehículos:

- `/publicar` (canónico): Usa `<SmartPublishWizard mode="individual" />` — moderno, limpio, ~35 líneas
- `/vender/publicar` (legacy): Wizard monolítico inline de ~1,024 líneas con manejo manual de estado

**Acciones específicas:**

1. **Reemplazar** el contenido completo de `src/app/(main)/vender/publicar/page.tsx` (1,024 líneas) con un redirect:

   ```tsx
   /**
    * Legacy publish route — redirects to /publicar (SmartPublishWizard)
    */
   import { redirect } from "next/navigation";

   export default function LegacyPublishPage() {
     redirect("/publicar");
   }
   ```

2. **Buscar** cualquier Link o router.push que apunte a `/vender/publicar` y cambiarlo a `/publicar`:
   ```bash
   grep -rn "vender/publicar" src/
   ```
   Archivos probables a actualizar:
   - `src/app/(main)/vender/page.tsx` (landing page de ventas — tiene CTAs)
   - `src/app/(main)/vender/dashboard/page.tsx`
   - `src/app/(main)/vender/vender-cta.tsx`

---

### TAREA 1.3 — Consolidar `/mis-vehiculos` → redirect a `/cuenta/mis-vehiculos`

**Problema:** Dos páginas listan los vehículos del usuario:

- `/cuenta/mis-vehiculos` (canónico): Dentro del layout de cuenta, usa `useSellerVehicles`, tiene tabs por estado
- `/mis-vehiculos` (redundante): 438 líneas, usa `/api/vehicles/seller/me` directo, tiene stat cards extra

**Acciones específicas:**

1. **Migrar las stat cards** de `/mis-vehiculos` a `/cuenta/mis-vehiculos`. Las stats son:
   - Vehículos activos (count)
   - Vistas totales (viewCount sum)
   - Llamadas (callCount sum)
   - Consultas (inquiryCount sum)

   Agregar estas stats como un `<div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">` al inicio de `src/app/(main)/cuenta/mis-vehiculos/page.tsx`.

2. **Reemplazar** `src/app/(main)/mis-vehiculos/page.tsx` con un redirect:

   ```tsx
   import { redirect } from "next/navigation";
   export default function LegacyMyVehiclesPage() {
     redirect("/cuenta/mis-vehiculos");
   }
   ```

3. **Buscar y actualizar** links a `/mis-vehiculos`:
   ```bash
   grep -rn "'/mis-vehiculos\|href=\"/mis-vehiculos" src/
   ```

---

### TAREA 1.4 — Eliminar `/dashboard` (mock data)

**Problema:** `src/app/(main)/dashboard/page.tsx` (269 líneas) usa datos mock hardcoded (favoritos: 12, alertas: 5, mensajes: 3). El dashboard real con APIs es `/cuenta`.

**Acciones específicas:**

1. **Reemplazar** `src/app/(main)/dashboard/page.tsx` con redirect:

   ```tsx
   import { redirect } from "next/navigation";
   export default function LegacyDashboardPage() {
     redirect("/cuenta");
   }
   ```

2. **Reemplazar** `src/app/(main)/dashboard/layout.tsx` con passthrough simple:

   ```tsx
   export default function DashboardLayout({
     children,
   }: {
     children: React.ReactNode;
   }) {
     return <>{children}</>;
   }
   ```

3. **Buscar** links a `/dashboard`:
   ```bash
   grep -rn "'/dashboard\|href=\"/dashboard" src/
   ```

---

### TAREA 1.5 — Eliminar funciones duplicadas de `sellers.ts`

**Problema:** `src/services/sellers.ts` contiene `registerDealer()` y `getMyDealer()` que duplican `createDealer()` y `getMyDealer()` de `src/services/dealers.ts`. Ambos llaman a los mismos endpoints (`POST /api/dealers` y `GET /api/dealers/me`).

**Acciones específicas:**

1. **Eliminar** de `src/services/sellers.ts` las siguientes funciones y sus tipos asociados (líneas ~148-200):
   - `RegisterDealerRequest` interface
   - `DealerRegistrationResult` interface
   - `registerDealer()` function
   - `getMyDealer()` function

2. **Buscar** imports de estas funciones y reemplazarlos con imports de `dealers.ts`:
   ```bash
   grep -rn "from.*services/sellers.*registerDealer\|from.*services/sellers.*getMyDealer\|import.*registerDealer.*sellers\|import.*getMyDealer.*sellers" src/
   ```
   Cambiar a: `import { createDealer, getMyDealer } from '@/services/dealers';`

---

### TAREA 1.6 — Renombrar `useVehicleSearch` en `use-vehicles.ts`

**Problema:** Dos hooks exportan el mismo nombre `useVehicleSearch`:

- `src/hooks/use-vehicles.ts` línea 86: `export function useVehicleSearch(params: VehicleSearchParams, options?: { enabled?: boolean })` — wrapper simple de TanStack Query
- `src/hooks/use-vehicle-search.ts` línea 369: `export function useVehicleSearch(options: UseVehicleSearchOptions = {})` — hook completo con URL sync, debounce, filter management

**Acciones específicas:**

1. **Renombrar** en `src/hooks/use-vehicles.ts` (línea 86):
   - De: `export function useVehicleSearch(`
   - A: `export function useVehicleList(`

2. **Actualizar imports** que usen `useVehicleSearch` de `use-vehicles.ts`:

   ```bash
   grep -rn "useVehicleSearch.*from.*use-vehicles\|from.*use-vehicles.*useVehicleSearch" src/
   ```

   El único archivo que importa `useVehicleSearch` de `use-vehicles` es:
   - `src/app/(main)/buscar/search.integration.test.tsx` línea 39

   Cambiar a: `import { useVehicleList } from '@/hooks/use-vehicles';`

3. **NO modificar** `src/hooks/use-vehicle-search.ts` — este es el canónico para búsqueda con URL sync.

---

### TAREA 1.7 — Extraer `formatPrice()` a utilidad compartida

**Problema:** `formatPrice()` está implementada 3 veces con variaciones menores en:

- `src/services/checkout.ts`
- `src/services/dealer-billing.ts`
- `src/services/user-billing.ts`

**Acciones específicas:**

1. **Crear** `src/lib/format.ts`:

   ```typescript
   /**
    * Shared formatting utilities
    */

   /**
    * Format price with currency symbol and locale
    */
   export function formatPrice(
     amount: number,
     currency: "DOP" | "USD" = "DOP",
   ): string {
     const formatter = new Intl.NumberFormat("es-DO", {
       style: "currency",
       currency: currency,
       minimumFractionDigits: 0,
       maximumFractionDigits: 2,
     });
     return formatter.format(amount);
   }

   /**
    * Format price with short notation (e.g., RD$1.5M)
    */
   export function formatPriceShort(
     amount: number,
     currency: "DOP" | "USD" = "DOP",
   ): string {
     const prefix = currency === "DOP" ? "RD$" : "US$";
     if (amount >= 1_000_000)
       return `${prefix}${(amount / 1_000_000).toFixed(1)}M`;
     if (amount >= 1_000) return `${prefix}${(amount / 1_000).toFixed(0)}K`;
     return `${prefix}${amount}`;
   }
   ```

2. **Reemplazar** las implementaciones locales en los 3 servicios con:
   ```typescript
   import { formatPrice } from "@/lib/format";
   ```
   Eliminar las funciones `formatPrice` locales de cada archivo.

---

### TAREA 1.8 — Actualizar lazy components con imports reales

**Problema:** `src/components/lazy/index.tsx` tiene placeholders "Componente próximamente" para componentes que ya existen.

**Acciones específicas:**

1. **Actualizar** `src/components/lazy/index.tsx`:
   - `LazyViewer360` → `React.lazy(() => import('@/components/vehicles/viewer-360'))`
   - `LazyChatPanel` → `React.lazy(() => import('@/components/chat/chat-panel'))`
   - Mantener Suspense wrappers con fallbacks de skeleton/spinner

---

### TAREA 1.9 — Reemplazar stubs de seguridad en `dealer/configuracion`

**Problema:** `src/app/(main)/dealer/configuracion/page.tsx` (529 líneas) tiene botones stub para cambiar contraseña, 2FA, y sesiones que NO funcionan. La implementación real está en `src/app/(main)/cuenta/seguridad/page.tsx` (1,414 líneas).

**Acciones específicas:**

1. **Reemplazar** la sección de seguridad en `dealer/configuracion/page.tsx` con links a la página de seguridad real:

   ```tsx
   {
     /* Seguridad - Enlace a configuración completa */
   }
   <Card>
     <CardHeader>
       <CardTitle className="flex items-center gap-2">
         <Shield className="h-5 w-5" />
         Seguridad
       </CardTitle>
       <CardDescription>
         Gestiona tu contraseña, autenticación de dos factores y sesiones
         activas.
       </CardDescription>
     </CardHeader>
     <CardContent>
       <Link href="/cuenta/seguridad">
         <Button variant="outline" className="w-full">
           Ir a configuración de seguridad
           <ArrowRight className="ml-2 h-4 w-4" />
         </Button>
       </Link>
     </CardContent>
   </Card>;
   ```

   Eliminar: los estados `showPasswordForm`, `show2FASetup`, los formularios inline de contraseña y 2FA, y los badges de sesiones estáticos ("3 sesiones activas").

---

## PARTE 2: VERIFICACIÓN LOCAL

Antes de hacer push, ejecutar estas verificaciones:

### 2.1 — Type Check

```bash
cd frontend/web-next && pnpm tsc --noEmit
```

### 2.2 — Lint

```bash
pnpm lint
```

### 2.3 — Build

```bash
pnpm build
```

### 2.4 — Tests unitarios

```bash
pnpm test
```

Si hay errores de tipo, lint, o build, **corregirlos antes de continuar**. Errores comunes:

- Imports rotos por archivos eliminados → actualizar imports
- Tipos no encontrados → verificar que las interfaces se exportan desde el archivo correcto
- Tests que referencian hooks renombrados → actualizar el nombre del hook

---

## PARTE 3: DEPLOY A PRODUCCIÓN

### 3.1 — Commit y Push

```bash
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices
git add -A
git commit -m "refactor(frontend): consolidate duplicate pages, components, and services

- Move OAuth callback from /auth/callback to /(auth)/callback
- Delete legacy /auth/ directory (fake login, registro, recuperar, verificar)
- Redirect /vender/publicar → /publicar (SmartPublishWizard)
- Redirect /mis-vehiculos → /cuenta/mis-vehiculos (merge stat cards)
- Redirect /dashboard → /cuenta (eliminate mock data page)
- Remove duplicate registerDealer/getMyDealer from sellers.ts
- Rename useVehicleSearch → useVehicleList in use-vehicles.ts
- Extract formatPrice to shared @/lib/format.ts
- Update lazy components with real imports
- Replace security stubs in dealer/configuracion with links to cuenta/seguridad

Audit: 19 duplications resolved, ~2,500 lines removed"
git push origin main
```

### 3.2 — Monitorear CI/CD

El push a `main` dispara `smart-cicd.yml` que:

1. Detecta cambios en `frontend/web-next/`
2. Ejecuta `_reusable-frontend.yml` (lint → typecheck → test → build → docker push)
3. Pushea imagen `ghcr.io/gregorymorenoiem/frontend-web:latest` a GHCR
4. Luego `deploy-digitalocean.yml` se ejecuta y actualiza el deployment en DOKS

**Monitorear en GitHub:** https://github.com/gregorymorenoiem/cardealer-microservices/actions

### 3.3 — Verificar deploy en K8s

```bash
kubectl rollout status deployment/frontend-web -n okla
kubectl get pods -n okla -l app=frontend-web
kubectl logs -f deployment/frontend-web -n okla --tail=50
```

Si el pod no arranca, verificar:

```bash
kubectl describe pod -n okla -l app=frontend-web
```

---

## PARTE 4: PREPARAR CUENTAS DE PRUEBA EN BASE DE DATOS

### 4.0 — Conexión a la base de datos

**Host:** `okla-db-do-user-31493168-0.g.db.ondigitalocean.com`  
**Puerto:** `25060`  
**User:** `okla_admin`  
**Password:** `CarDealerDBPassword2026Secure!`  
**SSL:** `sslmode=require`

Comando de conexión:

```bash
psql "host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com port=25060 dbname=authservice_db user=okla_admin password=CarDealerDBPassword2026Secure! sslmode=require"
```

### 4.1 — Crear cuenta de vendedor individual de prueba

**Paso 1:** Registrar la cuenta vía la UI en https://okla.com.do/registro con:

- **Email:** `seller-test@okla.com.do`
- **Password:** `Test2026Seller!@#`
- **Nombre:** `Carlos`
- **Apellido:** `Ventas`
- **Teléfono:** `8091234567`

**Paso 2:** Después de registrar, verificar email vía base de datos (para no depender del email real):

```sql
-- En authservice_db
UPDATE "AspNetUsers"
SET "EmailConfirmed" = true
WHERE "Email" = 'seller-test@okla.com.do';
```

**Paso 3:** Obtener el UserId para las siguientes queries:

```sql
-- En authservice_db
SELECT "Id", "Email", "UserName", "EmailConfirmed"
FROM "AspNetUsers"
WHERE "Email" = 'seller-test@okla.com.do';
```

Guardar el valor de `Id` como `<SELLER_USER_ID>`.

**Paso 4:** Crear perfil KYC aprobado:

```sql
-- En kycservice_db
-- Primero verificar si ya existe un perfil KYC
SELECT * FROM kyc_profiles WHERE "UserId" = '<SELLER_USER_ID>';

-- Si NO existe, crear uno:
INSERT INTO kyc_profiles ("Id", "UserId", "Status", "RiskLevel", "CreatedAt", "UpdatedAt", "ApprovedAt")
VALUES (
  gen_random_uuid(),
  '<SELLER_USER_ID>',
  5,              -- KYCStatus.Approved
  1,              -- RiskLevel.Low
  NOW(),
  NOW(),
  NOW()
);

-- Si YA existe, actualizar:
UPDATE kyc_profiles
SET "Status" = 5,
    "RiskLevel" = 1,
    "ApprovedAt" = NOW(),
    "UpdatedAt" = NOW()
WHERE "UserId" = '<SELLER_USER_ID>';
```

**Paso 5:** Actualizar el tipo de cuenta a Seller en UserService:

```sql
-- En userservice_db
-- Verificar si el usuario existe
SELECT "Id", "Email", "AccountType", "UserIntent" FROM "Users" WHERE "Email" = 'seller-test@okla.com.do';

-- Actualizar a tipo Seller
UPDATE "Users"
SET "AccountType" = 6,        -- AccountType.Seller
    "UserIntent" = 2,          -- UserIntent.Sell
    "IsActive" = true,
    "IsEmailVerified" = true,
    "UpdatedAt" = NOW()
WHERE "Email" = 'seller-test@okla.com.do';
```

**Paso 6:** Si la tabla SellerProfiles existe, crear/verificar perfil de seller:

```sql
-- En userservice_db
-- Verificar si existe
SELECT * FROM "SellerProfiles" WHERE "UserId" = '<SELLER_USER_ID>';

-- Si no existe, crear:
INSERT INTO "SellerProfiles" ("Id", "UserId", "BusinessName", "DisplayName", "VerificationStatus", "IsIdentityVerified", "VerifiedAt", "CreatedAt", "UpdatedAt")
VALUES (
  gen_random_uuid(),
  '<SELLER_USER_ID>',
  'Carlos Ventas - Vehículos',
  'Carlos Ventas',
  3,           -- SellerVerificationStatus.Verified
  true,
  NOW(),
  NOW(),
  NOW()
);

-- Si ya existe:
UPDATE "SellerProfiles"
SET "VerificationStatus" = 3,
    "IsIdentityVerified" = true,
    "VerifiedAt" = NOW(),
    "UpdatedAt" = NOW()
WHERE "UserId" = '<SELLER_USER_ID>';
```

---

### 4.2 — Crear cuenta de dealer de prueba

**Paso 1:** Registrar la cuenta vía la UI en https://okla.com.do/registro con:

- **Email:** `dealer-test@okla.com.do`
- **Password:** `Test2026Dealer!@#`
- **Nombre:** `María`
- **Apellido:** `Dealer`
- **Teléfono:** `8099876543`

**Paso 2:** Verificar email:

```sql
-- En authservice_db
UPDATE "AspNetUsers"
SET "EmailConfirmed" = true
WHERE "Email" = 'dealer-test@okla.com.do';
```

**Paso 3:** Obtener UserId:

```sql
SELECT "Id" FROM "AspNetUsers" WHERE "Email" = 'dealer-test@okla.com.do';
```

Guardar como `<DEALER_USER_ID>`.

**Paso 4:** KYC aprobado:

```sql
-- En kycservice_db
INSERT INTO kyc_profiles ("Id", "UserId", "Status", "RiskLevel", "CreatedAt", "UpdatedAt", "ApprovedAt")
VALUES (
  gen_random_uuid(),
  '<DEALER_USER_ID>',
  5, 1, NOW(), NOW(), NOW()
)
ON CONFLICT ("UserId") DO UPDATE SET "Status" = 5, "RiskLevel" = 1, "ApprovedAt" = NOW(), "UpdatedAt" = NOW();
```

**Paso 5:** Actualizar a tipo Dealer en UserService:

```sql
-- En userservice_db
UPDATE "Users"
SET "AccountType" = 2,        -- AccountType.Dealer
    "UserIntent" = 3,          -- UserIntent.BuyAndSell
    "IsActive" = true,
    "IsEmailVerified" = true,
    "UpdatedAt" = NOW()
WHERE "Email" = 'dealer-test@okla.com.do';
```

**Paso 6:** Crear perfil de dealer:

```sql
-- En userservice_db
-- Verificar si existe tabla Dealers
SELECT * FROM "Dealers" WHERE "OwnerUserId" = '<DEALER_USER_ID>';

-- Si no existe crear:
INSERT INTO "Dealers" ("Id", "OwnerUserId", "BusinessName", "TradeName", "VerificationStatus", "IsActive", "VerifiedAt", "CreatedAt", "UpdatedAt")
VALUES (
  gen_random_uuid(),
  '<DEALER_USER_ID>',
  'Auto Test Premium RD',
  'Auto Test RD',
  2,           -- DealerVerificationStatus.Verified
  true,
  NOW(),
  NOW(),
  NOW()
);
```

Guardar el `Id` generado como `<DEALER_ID>`.

**Paso 7:** Si DealerManagementService tiene su propia DB, crear también ahí:

```sql
-- En dealermanagementservice_db (si existe la base)
INSERT INTO dealers ("Id", "OwnerUserId", "BusinessName", "TradeName", "VerificationStatus", "Status", "IsTrustedDealer", "VerifiedAt", "CreatedAt", "UpdatedAt", "Email", "Phone")
VALUES (
  '<DEALER_ID>',            -- Usar el MISMO ID que en userservice_db
  '<DEALER_USER_ID>',
  'Auto Test Premium RD',
  'Auto Test RD',
  3,           -- VerificationStatus.Verified (enum diferente en este servicio)
  2,           -- DealerStatus.Active
  true,
  NOW(),
  NOW(),
  NOW(),
  'dealer-test@okla.com.do',
  '8099876543'
);
```

---

## PARTE 5: PRUEBAS DE FUNCIONALIDAD — VENDEDOR INDIVIDUAL

### 5.0 — Descargar imágenes de prueba

Antes de las pruebas, descargar 5 fotos de vehículos reales para usar en las publicaciones. Usar URLs de Unsplash sin copyright:

```bash
mkdir -p /tmp/okla-test-photos
# Toyota Corolla blanco
curl -L "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=1200" -o /tmp/okla-test-photos/vehiculo-1-frente.jpg
# Interior de carro
curl -L "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=1200" -o /tmp/okla-test-photos/vehiculo-2-interior.jpg
# Carro lateral
curl -L "https://images.unsplash.com/photo-1494976388531-d1058494cdd8?w=1200" -o /tmp/okla-test-photos/vehiculo-3-lateral.jpg
# Carro trasero
curl -L "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=1200" -o /tmp/okla-test-photos/vehiculo-4-trasero.jpg
# Motor
curl -L "https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=1200" -o /tmp/okla-test-photos/vehiculo-5-detalle.jpg
```

Verificar que se descargaron correctamente:

```bash
ls -la /tmp/okla-test-photos/
# Cada archivo debe ser > 100KB
```

### 5.1 — Login como vendedor

1. Navegar a https://okla.com.do/login
2. Iniciar sesión con:
   - Email: `seller-test@okla.com.do`
   - Password: `Test2026Seller!@#`
3. **Verificar:** Redirección a `/cuenta` o `/` después del login
4. **Verificar:** El navbar muestra el nombre del usuario ("Carlos V.")
5. **Verificar:** El menú de usuario tiene opciones de vendedor

### 5.2 — Publicar un vehículo (flujo principal)

1. Navegar a https://okla.com.do/publicar
2. **Verificar:** La página carga `<SmartPublishWizard>` (NO el wizard viejo de 1,024 líneas)
3. **Verificar:** Se ve el título "Publicar Vehículo" y "detección automática por VIN"
4. Completar el wizard con estos datos:
   - **Método:** Manual (si VIN decode no está disponible)
   - **Marca:** Toyota
   - **Modelo:** Corolla
   - **Año:** 2022
   - **Tipo de cuerpo:** Sedán
   - **Combustible:** Gasolina
   - **Transmisión:** Automática
   - **Color exterior:** Blanco
   - **Kilometraje:** 35,000
   - **Condición:** Usado
   - **Provincia:** Santo Domingo
   - **Precio:** 1,200,000 (DOP)
   - **Descripción:** "Toyota Corolla 2022, único dueño, mantenimiento al día en casa Toyota. Incluye cámara reversa, bluetooth, pantalla táctil."
   - **Fotos:** Subir las 5 fotos descargadas de `/tmp/okla-test-photos/`
5. **Verificar:** Las fotos se suben correctamente (progress bar, preview)
6. **Verificar:** Se puede reordenar las fotos (drag & drop)
7. **Verificar:** Se puede seleccionar la foto principal
8. Completar y publicar
9. **Verificar:** Mensaje de éxito y redirección
10. **Guardar** el ID o slug del vehículo publicado como `<VEHICLE_ID>` para pruebas posteriores

### 5.3 — Verificar que el redirect de `/vender/publicar` funciona

1. Navegar a https://okla.com.do/vender/publicar
2. **Verificar:** Redirige automáticamente a https://okla.com.do/publicar
3. **Verificar:** La URL en el browser cambia a `/publicar`

### 5.4 — Ver mis vehículos publicados

1. Navegar a https://okla.com.do/cuenta/mis-vehiculos
2. **Verificar:** El vehículo publicado aparece en la lista
3. **Verificar:** El vehículo muestra: título, precio, estado (activo/pendiente), foto
4. **Verificar:** Las stat cards están visibles (vehículos activos, vistas, etc.)

### 5.5 — Verificar redirect de `/mis-vehiculos`

1. Navegar a https://okla.com.do/mis-vehiculos
2. **Verificar:** Redirige a https://okla.com.do/cuenta/mis-vehiculos

### 5.6 — Verificar redirect de `/dashboard`

1. Navegar a https://okla.com.do/dashboard
2. **Verificar:** Redirige a https://okla.com.do/cuenta

### 5.7 — Ver el vehículo en el catálogo público

1. Navegar a https://okla.com.do/vehiculos
2. **Verificar:** El Toyota Corolla aparece en el listado (puede estar en estado "pendiente" si requiere moderación)
3. Buscar el vehículo por slug: https://okla.com.do/vehiculos/<slug-del-vehiculo>
4. **Verificar:** La página de detalle carga correctamente con fotos, precio, descripción

### 5.8 — Probar la búsqueda

1. Navegar a https://okla.com.do/buscar
2. Buscar "Toyota Corolla"
3. **Verificar:** Los filtros funcionan (marca, modelo, año, precio, combustible, transmisión)
4. **Verificar:** El vehículo publicado aparece en los resultados (si está activo)

### 5.9 — Probar checkout / destacar vehículo

1. Ir al vehículo publicado en `/cuenta/mis-vehiculos`
2. Buscar la opción de "Destacar" o "Boost"
3. Si existe botón de destacar, hacer clic y verificar que lleva al checkout
4. Navegar directamente a: https://okla.com.do/checkout?product=boost-basic&vehicleId=<VEHICLE_ID>
5. **Verificar:** La página de checkout carga con el producto correcto
6. **NOTA:** No completar el pago real — solo verificar que el flujo de checkout funciona hasta el formulario de pago

### 5.10 — Marcar vehículo como destacado vía DB

Para probar que aparece en la homepage como destacado:

```sql
-- En vehiclessaleservice_db
-- Primero encontrar el vehículo
SELECT "Id", "Make", "Model", "Year", "Status", "IsFeatured", "HomepageSections"
FROM vehicles
WHERE "Make" ILIKE '%Toyota%' AND "Model" ILIKE '%Corolla%'
ORDER BY "CreatedAt" DESC
LIMIT 5;

-- Activar el vehículo y marcarlo como destacado
UPDATE vehicles
SET "IsFeatured" = true,
    "Status" = 2,                    -- VehicleStatus.Active
    "HomepageSections" = 33,         -- Carousel(1) + Destacados(32)
    "PublishedAt" = COALESCE("PublishedAt", NOW()),
    "UpdatedAt" = NOW()
WHERE "Id" = '<VEHICLE_ID>';

-- También insertarlo en la tabla de secciones del homepage (si existe)
-- Verificar primero qué secciones existen:
SELECT "Id", "Name", "Slug", "LayoutType", "IsActive" FROM homepage_section_configs;

-- Insertar en la sección "destacados" (o la que corresponda):
INSERT INTO vehicle_homepage_sections ("Id", "VehicleId", "HomepageSectionConfigId", "SortOrder", "IsPinned", "CreatedAt")
SELECT gen_random_uuid(), '<VEHICLE_ID>', "Id", 0, true, NOW()
FROM homepage_section_configs
WHERE "Slug" IN ('destacados', 'featured', 'carousel')
AND NOT EXISTS (
  SELECT 1 FROM vehicle_homepage_sections
  WHERE "VehicleId" = '<VEHICLE_ID>' AND "HomepageSectionConfigId" = homepage_section_configs."Id"
);
```

### 5.11 — Verificar vehículo destacado en homepage

1. Navegar a https://okla.com.do
2. **Verificar:** El Toyota Corolla 2022 aparece en la sección de "Vehículos Destacados"
3. **Verificar:** La foto principal se muestra correctamente
4. **Verificar:** El precio (RD$1,200,000) se muestra correctamente
5. Hacer clic en el vehículo → debe llevar a la página de detalle

### 5.12 — Probar cuenta y configuración del vendedor

1. Navegar a https://okla.com.do/cuenta
2. **Verificar:** Stats del vendedor visibles (vehículos, vistas, consultas)
3. Navegar a https://okla.com.do/cuenta/perfil
4. **Verificar:** Formulario de perfil carga con los datos del usuario
5. Navegar a https://okla.com.do/cuenta/configuracion
6. **Verificar:** Opciones de configuración (tema, idioma, notificaciones)
7. Navegar a https://okla.com.do/cuenta/seguridad
8. **Verificar:** Opciones de contraseña, 2FA, sesiones activas

### 5.13 — Logout del vendedor

1. Hacer logout
2. **Verificar:** Redirección a la homepage
3. **Verificar:** El navbar vuelve al estado de visitante

---

## PARTE 6: PRUEBAS DE FUNCIONALIDAD — DEALER

### 6.1 — Login como dealer

1. Navegar a https://okla.com.do/login
2. Iniciar sesión con:
   - Email: `dealer-test@okla.com.do`
   - Password: `Test2026Dealer!@#`
3. **Verificar:** Login exitoso

### 6.2 — Acceder al dashboard de dealer

1. Navegar a https://okla.com.do/dealer
2. **Verificar:** El dashboard de dealer carga
3. **Verificar:** Se muestran stats (si hay datos)
4. **Verificar:** El menú lateral del dealer tiene las secciones esperadas:
   - Dashboard, Inventario, Publicar, Leads, Analytics, Empleados, Configuración, Facturación

### 6.3 — Publicar vehículo como dealer

1. Navegar a https://okla.com.do/dealer/publicar
2. **Verificar:** Carga `<SmartPublishWizard>` con mode="dealer"
3. Publicar un vehículo con estos datos:
   - **Marca:** Honda
   - **Modelo:** CR-V
   - **Año:** 2023
   - **Tipo:** SUV
   - **Combustible:** Gasolina
   - **Transmisión:** Automática
   - **Color:** Gris
   - **Kilometraje:** 20,000
   - **Condición:** Usado
   - **Provincia:** Santiago
   - **Precio:** 2,100,000 (DOP)
   - **Descripción:** "Honda CR-V 2023 EX-L, AWD, techo panorámico, asientos en cuero, Honda Sensing."
   - **Fotos:** Subir las mismas 5 fotos de prueba
4. **Verificar:** Publicación exitosa

### 6.4 — Verificar inventario del dealer

1. Navegar a https://okla.com.do/dealer/inventario (o la ruta equivalente)
2. **Verificar:** El Honda CR-V aparece en el inventario
3. **Verificar:** Se pueden filtrar/buscar vehículos

### 6.5 — Probar analytics del dealer

1. Navegar a https://okla.com.do/dealer/analytics
2. **Verificar:** La página de analytics carga (puede mostrar datos vacíos si es cuenta nueva)

### 6.6 — Probar configuración del dealer

1. Navegar a https://okla.com.do/dealer/configuracion
2. **Verificar:** La sección de seguridad ahora muestra un LINK a `/cuenta/seguridad` (NO botones stub)
3. **Verificar:** Las notificaciones del dealer son diferentes a las del usuario normal
4. Hacer clic en "Ir a configuración de seguridad"
5. **Verificar:** Navega a `/cuenta/seguridad` con las opciones completas de contraseña, 2FA, sesiones

### 6.7 — Probar facturación del dealer

1. Navegar a https://okla.com.do/dealer/facturacion
2. **Verificar:** La página carga (puede mostrar estado de suscripción vacío)

### 6.8 — Verificar perfil público del dealer

1. Navegar a https://okla.com.do/dealers
2. **Verificar:** "Auto Test Premium RD" aparece en el listado (si hay listado público)
3. Si tiene slug, navegar a https://okla.com.do/dealers/<slug>
4. **Verificar:** Página de perfil público del dealer con sus vehículos

### 6.9 — Marcar vehículo del dealer como destacado

```sql
-- En vehiclessaleservice_db
SELECT "Id", "Make", "Model", "Year", "Status", "IsFeatured"
FROM vehicles
WHERE "Make" ILIKE '%Honda%' AND "Model" ILIKE '%CR-V%'
ORDER BY "CreatedAt" DESC LIMIT 5;

UPDATE vehicles
SET "IsFeatured" = true,
    "Status" = 2,
    "HomepageSections" = 37,         -- Carousel(1) + SUVs(4) + Destacados(32)
    "PublishedAt" = COALESCE("PublishedAt", NOW()),
    "UpdatedAt" = NOW()
WHERE "Id" = '<HONDA_CRV_VEHICLE_ID>';
```

### 6.10 — Verificar ambos vehículos en homepage

1. Navegar a https://okla.com.do
2. **Verificar:** Tanto el Toyota Corolla como el Honda CR-V aparecen en la sección de destacados
3. **Verificar:** Las fotos, precios y detalles se muestran correctamente

### 6.11 — Logout del dealer

1. Hacer logout
2. **Verificar:** Redirección exitosa

---

## PARTE 7: PRUEBAS DE REGRESIÓN (RUTAS QUE NO DEBEN ROMPERSE)

Navegar a cada una de estas URLs y verificar que cargan sin error:

### Rutas públicas

- [ ] https://okla.com.do — Homepage con sección de destacados
- [ ] https://okla.com.do/vehiculos — Catálogo de vehículos
- [ ] https://okla.com.do/buscar — Búsqueda de vehículos
- [ ] https://okla.com.do/comparar — Comparador
- [ ] https://okla.com.do/contacto — Página de contacto
- [ ] https://okla.com.do/nosotros — About us
- [ ] https://okla.com.do/about — Redirect a /nosotros
- [ ] https://okla.com.do/dealers — Listado de dealers
- [ ] https://okla.com.do/vender — Landing page de ventas (marketing)
- [ ] https://okla.com.do/privacidad — Política de privacidad
- [ ] https://okla.com.do/terminos — Términos y condiciones

### Rutas de autenticación

- [ ] https://okla.com.do/login — Login (debe estar en `(auth)` layout)
- [ ] https://okla.com.do/registro — Registro
- [ ] https://okla.com.do/recuperar-contrasena — Recuperar contraseña

### Redirects (deben funcionar)

- [ ] https://okla.com.do/vender/publicar → /publicar
- [ ] https://okla.com.do/mis-vehiculos → /cuenta/mis-vehiculos
- [ ] https://okla.com.do/dashboard → /cuenta

### Rutas que NO deben existir más (deben dar 404)

- [ ] https://okla.com.do/auth/login — 404 (o redirect via next.config.ts)
- [ ] https://okla.com.do/auth/recuperar — 404
- [ ] https://okla.com.do/auth/verificar — 404

**NOTA:** Si el redirect de `/auth/callback/:provider` es necesario para OAuth, verificar que `/auth/callback/google` sigue funcionando (via redirect en next.config.ts).

---

## PARTE 8: LIMPIEZA POST-PRUEBAS

### 8.1 — Si las pruebas pasaron, los vehículos de prueba pueden quedarse o limpiarse:

```sql
-- OPCIONAL: Limpiar vehículos de prueba en vehiclessaleservice_db
-- Solo hacer esto si los vehículos no deben quedarse
UPDATE vehicles SET "IsDeleted" = true, "DeletedAt" = NOW()
WHERE "Id" IN ('<VEHICLE_ID>', '<HONDA_CRV_VEHICLE_ID>');
```

### 8.2 — Las cuentas de prueba pueden quedarse como cuentas de QA:

- `seller-test@okla.com.do` — Para pruebas futuras de vendedor
- `dealer-test@okla.com.do` — Para pruebas futuras de dealer

---

## CHECKLIST FINAL

- [ ] **Fase 1:** Todas las 9 tareas de merge completadas
- [ ] **Fase 2:** `pnpm tsc --noEmit` pasa sin errores
- [ ] **Fase 2:** `pnpm lint` pasa sin errores
- [ ] **Fase 2:** `pnpm build` exitoso
- [ ] **Fase 2:** `pnpm test` pasa (o tests actualizados)
- [ ] **Fase 3:** Push a main y CI/CD exitoso
- [ ] **Fase 3:** Pod frontend-web running en K8s
- [ ] **Fase 4:** Cuentas de prueba creadas y KYC aprobado
- [ ] **Fase 5:** Login vendedor ✓
- [ ] **Fase 5:** Publicar vehículo ✓
- [ ] **Fase 5:** Redirect /vender/publicar → /publicar ✓
- [ ] **Fase 5:** Redirect /mis-vehiculos → /cuenta/mis-vehiculos ✓
- [ ] **Fase 5:** Redirect /dashboard → /cuenta ✓
- [ ] **Fase 5:** Vehículo visible en catálogo ✓
- [ ] **Fase 5:** Búsqueda funciona ✓
- [ ] **Fase 5:** Checkout/boost carga ✓
- [ ] **Fase 5:** Vehículo destacado en homepage ✓
- [ ] **Fase 5:** Configuración y seguridad ✓
- [ ] **Fase 6:** Login dealer ✓
- [ ] **Fase 6:** Dashboard dealer ✓
- [ ] **Fase 6:** Publicar vehículo como dealer ✓
- [ ] **Fase 6:** Inventario dealer ✓
- [ ] **Fase 6:** Configuración dealer sin stubs ✓
- [ ] **Fase 6:** Vehículo dealer destacado en homepage ✓
- [ ] **Fase 7:** Todas las rutas públicas cargan ✓
- [ ] **Fase 7:** Todos los redirects funcionan ✓
- [ ] **Fase 7:** Rutas legacy eliminadas devuelven 404 ✓
