# 🛡️ Server Actions — Protección de Endpoints Críticos

**Fecha de implementación:** Febrero 14, 2026
**Proyecto:** OKLA (CarDealer Microservices)
**Tecnología:** Next.js 14 Server Actions + BFF Pattern

---

## 📖 ¿Qué son los Server Actions?

**Server Actions** son una funcionalidad de Next.js 13.4+ (estable desde Next.js 14) que permite ejecutar funciones **exclusivamente en el servidor** desde componentes del cliente. Se declaran con la directiva `'use server'` al inicio del archivo.

### La idea simple

Normalmente, cuando un usuario hace login en una aplicación web, el navegador envía un `fetch()` o `XMLHttpRequest` al backend. Esa petición es **100% visible** en la pestaña **Network** de DevTools del navegador:

```
❌ SIN Server Actions (visible en DevTools):

POST https://okla.com.do/api/auth/login
Request Body: { "email": "user@example.com", "password": "MiPassword123!" }
Response: { "accessToken": "eyJhbGciOi...", "refreshToken": "abc123..." }
```

Un atacante, un usuario curioso, o cualquier extensión del navegador puede ver:

- La URL exacta del endpoint (`/api/auth/login`)
- Los datos enviados (email, contraseña)
- Los tokens recibidos
- Headers de autenticación
- Estructura de la respuesta del API

**Con Server Actions**, el navegador solo ve:

```
✅ CON Server Actions (opaco en DevTools):

POST https://okla.com.do/   (o /_next/...)
Request Body: [datos binarios serializados — ilegibles]
Response: [datos binarios serializados — ilegibles]
```

El usuario **no puede ver** qué endpoint se llama, qué datos se envían, ni qué responde el servidor.

---

## 🔬 ¿Cómo funciona técnicamente?

### Flujo tradicional (sin Server Actions)

```
┌──────────────┐     fetch('/api/auth/login')      ┌──────────────┐
│   Browser    │ ──────────────────────────────────▶│   Gateway    │
│  (React)     │◀──────────────────────────────────│  (Ocelot)    │
│              │     { accessToken: "eyJ..." }      │              │
└──────────────┘                                    └──────┬───────┘
     ▲                                                      │
     │  TODO visible en DevTools Network tab                │
     │  - URL del endpoint                                  ▼
     │  - Body con credenciales                     ┌──────────────┐
     │  - Respuesta con tokens                      │ AuthService  │
     │  - Headers de autenticación                  └──────────────┘
```

### Flujo con Server Actions

```
┌──────────────┐    POST opaco (serializado)     ┌──────────────────┐
│   Browser    │ ──────────────────────────────▶ │   Next.js Server │
│  (React)     │◀──────────────────────────────  │  (Server Action) │
│              │    resultado serializado         │                  │
└──────────────┘                                  └────────┬─────────┘
     ▲                                                      │
     │  En DevTools solo se ve:                             │ fetch interno
     │  - POST a la misma URL                               │ (red privada K8s)
     │  - Body: binario/ilegible                            ▼
     │  - Response: binario/ilegible              ┌──────────────────┐
     │                                             │    Gateway       │
     │  ❌ NO se ve:                               │  (gateway:8080)  │
     │  - /api/auth/login                          └────────┬─────────┘
     │  - email/password                                    │
     │  - tokens                                            ▼
     │                                             ┌──────────────────┐
     │                                             │   AuthService    │
     └─────────────────────────────────────────────└──────────────────┘
```

### ¿Qué pasa exactamente en el navegador?

Cuando un componente React llama a un Server Action, Next.js:

1. **Serializa** los argumentos de la función usando un formato interno (no JSON legible)
2. Envía un **POST** a la misma URL de la página (con un header especial `Next-Action`)
3. El **servidor Next.js** recibe la petición, deserializa los argumentos
4. Ejecuta la función del Server Action **en el servidor** (Node.js)
5. Dentro del Server Action, se hace un `fetch()` interno al Gateway (red privada)
6. **Serializa** el resultado y lo devuelve al browser
7. React deserializa el resultado y lo usa en el componente

El punto clave: **los pasos 4-5 ocurren en el servidor**. El navegador nunca sabe que se llamó a `/api/auth/login`.

---

## 🏗️ Arquitectura implementada en OKLA

### Capas de protección combinadas

OKLA implementa **3 capas de invisibilidad** para los endpoints críticos:

```
                          CAPA 3: Server Actions
                          El navegador NO ve qué endpoint
                          se llama ni los datos enviados
                                    │
┌──────────┐    POST opaco    ┌─────▼──────┐    fetch interno    ┌──────────┐
│ Browser  │ ───────────────▶ │  Next.js   │ ──────────────────▶ │ Gateway  │
│          │ ◀─────────────── │  Server    │ ◀────────────────── │ (8080)   │
└──────────┘                  └────────────┘                     └──────────┘
                                    │                                  │
                          CAPA 2: BFF Pattern                   CAPA 1: K8s
                          Gateway NO tiene IP                   NetworkPolicy
                          pública. Solo Next.js                 Solo frontend-web
                          puede acceder.                        puede hablar con
                                                                Gateway.
```

| Capa                            | Tecnología                         | ¿Qué protege?                                      |
| ------------------------------- | ---------------------------------- | -------------------------------------------------- |
| **1. Kubernetes NetworkPolicy** | K8s ClusterIP + NetworkPolicy      | Gateway solo acepta tráfico del pod `frontend-web` |
| **2. BFF Pattern**              | Next.js rewrites → Gateway interno | Gateway no tiene IP pública ni Ingress             |
| **3. Server Actions**           | `'use server'` + `internalFetch()` | El browser no ve endpoints, datos ni respuestas    |

### Archivos creados

```
frontend/web-next/src/
├── actions/                    # 🆕 Server Actions (server-only)
│   ├── auth.ts                 # 15 acciones de autenticación
│   ├── checkout.ts             # 4 acciones de pagos
│   └── kyc.ts                  # 8 acciones de verificación KYC
├── services/                   # Servicios cliente (actualizados)
│   ├── auth.ts                 # Delegó mutaciones → actions/auth.ts
│   ├── checkout.ts             # Delegó mutaciones → actions/checkout.ts
│   └── kyc.ts                  # Delegó mutaciones → actions/kyc.ts
└── lib/
    └── api-url.ts              # getInternalApiUrl() → gateway:8080
```

---

## 📋 Inventario de endpoints protegidos

### 🔐 Autenticación (15 Server Actions)

| Server Action                  | Endpoint protegido                    | Datos sensibles ocultos             |
| ------------------------------ | ------------------------------------- | ----------------------------------- |
| `serverLogin`                  | `/api/auth/login`                     | Email, contraseña, tokens JWT       |
| `serverVerify2FA`              | `/api/auth/2fa/login`                 | Código 2FA, temp token              |
| `serverRegister`               | `/api/auth/register`                  | Nombre, email, contraseña, teléfono |
| `serverForgotPassword`         | `/api/auth/forgot-password`           | Email del usuario                   |
| `serverResetPassword`          | `/api/auth/reset-password`            | Token de reset, nueva contraseña    |
| `serverVerifyEmail`            | `/api/auth/verify-email`              | Token de verificación               |
| `serverResendVerification`     | `/api/auth/resend-verification`       | Email                               |
| `serverChangePassword`         | `/api/auth/security/change-password`  | Contraseña actual y nueva           |
| `serverSetPassword`            | `/api/auth/set-password`              | Nueva contraseña (OAuth users)      |
| `serverLogout`                 | `/api/auth/logout`                    | Refresh token                       |
| `serverSetup2FA`               | `/api/auth/2fa/enable`                | QR code, secret, backup codes       |
| `serverEnable2FA`              | `/api/auth/2fa/verify`                | Código de verificación              |
| `serverDisable2FA`             | `/api/auth/2fa/disable`               | Contraseña de confirmación          |
| `serverRequestAccountDeletion` | `/api/privacy/delete-account/request` | Razón de eliminación                |
| `serverConfirmAccountDeletion` | `/api/privacy/delete-account/confirm` | Código de confirmación, contraseña  |

### 💳 Pagos (4 Server Actions)

| Server Action                 | Endpoint protegido                           | Datos sensibles ocultos                |
| ----------------------------- | -------------------------------------------- | -------------------------------------- |
| `serverCreateCheckoutSession` | `/api/checkout/sessions`                     | Producto, método de pago, promo code   |
| `serverCreatePaymentIntent`   | `/api/checkout/sessions/{id}/payment-intent` | Client secret de Stripe                |
| `serverProcessPayment`        | `/api/checkout/process-payment`              | Card token, session ID, transaction ID |
| `serverValidatePromoCode`     | `/api/checkout/validate-promo`               | Código promocional, descuento          |

### 🪪 KYC — Verificación de Identidad (8 Server Actions)

| Server Action                       | Endpoint protegido                                       | Datos sensibles ocultos                      |
| ----------------------------------- | -------------------------------------------------------- | -------------------------------------------- |
| `serverCreateKYCProfile`            | `/api/kyc/kycprofiles`                                   | Nombre, cédula, fecha nacimiento, dirección  |
| `serverUpdateKYCProfile`            | `/api/kyc/kycprofiles/{id}`                              | Datos personales actualizados                |
| `serverSubmitKYCForReview`          | `/api/kyc/kycprofiles/{id}/submit`                       | ID del perfil enviado                        |
| `serverUploadKYCDocument`           | `/api/media/upload` + `/api/kyc/profiles/{id}/documents` | Archivo de documento, storage keys, S3 paths |
| `serverDeleteKYCDocument`           | `/api/kyc/documents/{id}`                                | ID del documento                             |
| `serverProcessIdentityVerification` | `/api/kyc/identity-verification/verify`                  | Selfie, liveness data, scores biométricos    |
| `serverApproveKYCProfile`           | `/api/kyc/kycprofiles/{id}/approve`                      | Identidad del admin, notas                   |
| `serverRejectKYCProfile`            | `/api/kyc/kycprofiles/{id}/reject`                       | Razón de rechazo, identidad del admin        |

---

## 💻 Ejemplos de código

### Anatomía de un Server Action

```typescript
// src/actions/auth.ts
"use server"; // ← Esta directiva es OBLIGATORIA. Marca TODO el archivo como server-only.

import { getInternalApiUrl } from "@/lib/api-url";

// Tipo estándar de retorno para todas las acciones
export interface ActionResult<T = void> {
  success: boolean;
  data?: T;
  error?: string;
  code?: string;
}

// Helper que hace fetch al Gateway por la red INTERNA de Kubernetes
async function internalFetch<T>(path: string, options = {}): Promise<T> {
  const url = `${getInternalApiUrl()}${path}`; // → http://gateway:8080/api/...
  const response = await fetch(url, { ...options, cache: "no-store" });
  // ... manejo de errores
  return response.json();
}

// Server Action exportado — esto es lo que el componente React llama
export async function serverLogin(
  email: string,
  password: string,
): Promise<ActionResult<{ accessToken: string; refreshToken: string }>> {
  try {
    const response = await internalFetch("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });
    return { success: true, data: response };
  } catch (error) {
    return { success: false, error: "Error al iniciar sesión" };
  }
}
```

### Cómo el servicio cliente delega al Server Action

```typescript
// src/services/auth.ts (ANTES — visible en Network tab)
export async function login(data: LoginRequest): Promise<{ user: User }> {
  // ❌ Esto aparece en DevTools como: POST /api/auth/login {email, password}
  const response = await apiClient.post("/api/auth/login", data);
  authTokens.setTokens(response.data.accessToken, response.data.refreshToken);
  // ...
}

// src/services/auth.ts (DESPUÉS — invisible en Network tab)
export async function login(data: LoginRequest): Promise<{ user: User }> {
  // ✅ Esto aparece en DevTools como: POST / [datos binarios]
  const result = await serverLogin(data.email, data.password, data.rememberMe);

  if (!result.success) throw new Error(result.error);

  // Tokens se almacenan en localStorage client-side
  // (Server Actions no tienen acceso a localStorage)
  authTokens.setTokens(result.data!.accessToken, result.data!.refreshToken);
  // ...
}
```

### Cómo los componentes React usan las acciones (sin cambios)

```tsx
// src/app/(auth)/login/page.tsx — NO necesitó cambios
// El componente sigue llamando a useAuth().login() → authService.login()
// La diferencia es que internamente authService.login() ahora usa serverLogin()

const { login } = useAuth();

const handleSubmit = async (e: React.FormEvent) => {
  await login({ email, password, rememberMe });
  // En DevTools: POST opaco, NO se ve /api/auth/login
  router.push("/");
};
```

---

## ❓ Preguntas frecuentes

### ¿Un Server Action es un API endpoint?

**Sí y no.** Técnicamente Next.js crea un endpoint interno, pero:

- No tiene una URL predecible (usa hashes internos)
- Los datos se serializan en formato propietario (no JSON legible)
- No aparece como `/api/auth/login` en Network
- Cambia de hash con cada build

### ¿Afecta el rendimiento?

**No significativamente.** Comparación:

| Aspecto         | fetch() directo           | Server Action                        |
| --------------- | ------------------------- | ------------------------------------ |
| Latencia de red | 1 hop (browser → gateway) | 2 hops (browser → next.js → gateway) |
| Serialización   | JSON.stringify            | React serialization                  |
| Impacto real    | ~0ms                      | ~1-3ms adicionales                   |

En producción OKLA, ambos hops están dentro del **mismo cluster Kubernetes**, por lo que la latencia adicional es <1ms.

### ¿Los GET requests también usan Server Actions?

**No.** Solo las **mutaciones** (POST, PUT, DELETE) usan Server Actions. Los GET requests son menos sensibles porque:

- No envían datos modificadores al servidor
- La información que obtienen generalmente ya la tiene el usuario
- Muchos son datos públicos (listado de vehículos, productos, etc.)

### ¿Por qué no usar Server Components directamente?

Los **Server Components** (RSC) son la opción ideal cuando toda la página se renderiza en el servidor. Pero las páginas de OKLA que hacen login, checkout y KYC son **interactivas** (`'use client'`):

- Formularios con estado (useState)
- Validación en tiempo real
- Feedback visual de carga
- Redirecciones post-acción

Server Actions son el puente: permiten que un componente interactivo (`'use client'`) ejecute lógica en el servidor.

### ¿Qué pasa si alguien intercepta el POST del Server Action?

Vería algo como:

```
POST / HTTP/1.1
Host: okla.com.do
Content-Type: text/x-component
Next-Action: a1b2c3d4e5f6...

0:["$@1",["serverLogin","email@example.com","$undefined"]]
```

Esto **no revela**:

- La URL del backend (`/api/auth/login`)
- La estructura de la API
- Tokens o respuestas del servidor
- Información sobre la arquitectura interna

### ¿Y el token de autenticación?

Los tokens JWT se manejan así:

1. **Login:** Server Action llama al backend, recibe tokens, los devuelve al browser
2. **Browser:** Almacena tokens en `localStorage` (via `authTokens.setTokens()`)
3. **Operaciones autenticadas:** El servicio lee el token de `localStorage` y lo pasa como parámetro al Server Action
4. **Server Action:** Incluye el token en el header `Authorization: Bearer` al llamar al Gateway internamente

```
Browser (tiene token en localStorage)
  → serverChangePassword(currentPwd, newPwd, accessToken)
    → Server Action recibe accessToken como parámetro
      → internalFetch('/api/auth/...', { token: accessToken })
        → Gateway recibe: Authorization: Bearer eyJ...
```

---

## 🔒 Qué operaciones quedan visibles vs. invisibles

### ✅ Invisibles en DevTools (Server Actions)

| Categoría | Operaciones                                                                          |
| --------- | ------------------------------------------------------------------------------------ |
| **Auth**  | Login, registro, logout, cambio de contraseña, 2FA, eliminación de cuenta            |
| **Pagos** | Crear sesión de pago, procesar pago, validar código promo                            |
| **KYC**   | Crear/actualizar perfil, subir documentos, verificación biométrica, aprobar/rechazar |

### 👁️ Visibles en DevTools (fetch directo — menos sensibles)

| Categoría     | Operaciones                                                             | Justificación                      |
| ------------- | ----------------------------------------------------------------------- | ---------------------------------- |
| **Auth**      | `getCurrentUser()`, `getSessions()`, `getSecuritySettings()`            | Solo leen datos del propio usuario |
| **Pagos**     | `getProduct()`, `getProducts()`, `getAvailableGateways()`               | Datos públicos del catálogo        |
| **KYC**       | `getKYCProfileByUserId()`, `getKYCDocuments()`, `getDocumentFreshUrl()` | Solo lectura del propio perfil     |
| **Vehículos** | Todo el CRUD de vehículos                                               | Datos públicos del marketplace     |

---

## 📐 Reglas para nuevos Server Actions

Al crear un nuevo Server Action, seguir este patrón:

### 1. Crear archivo en `src/actions/`

```typescript
"use server"; // ← OBLIGATORIO

import { getInternalApiUrl } from "@/lib/api-url";

export interface ActionResult<T = void> {
  success: boolean;
  data?: T;
  error?: string;
  code?: string;
}

export async function serverMyAction(
  param1: string,
  accessToken: string, // ← Token siempre como último parámetro
): Promise<ActionResult<MyResultType>> {
  try {
    const response = await internalFetch<MyResultType>("/api/my-endpoint", {
      method: "POST",
      body: { param1 },
      token: accessToken,
    });
    return { success: true, data: response };
  } catch (error: unknown) {
    const err = error as Error;
    return {
      success: false,
      error: err.message || "Error genérico",
      code: "MY_ACTION_FAILED",
    };
  }
}
```

### 2. Actualizar servicio cliente en `src/services/`

```typescript
import { serverMyAction } from "@/actions/my-actions";

export async function myAction(param1: string): Promise<MyResult> {
  const accessToken = authTokens.getAccessToken();
  const result = await serverMyAction(param1, accessToken || "");

  if (!result.success || !result.data) {
    throw new Error(result.error || "Error");
  }

  return result.data;
}
```

### 3. Reglas obligatorias

- ✅ **SIEMPRE** usar `'use server'` al inicio del archivo
- ✅ **SIEMPRE** retornar `ActionResult<T>` (nunca throw desde un Server Action)
- ✅ **SIEMPRE** usar `internalFetch()` con `getInternalApiUrl()` — nunca apiClient
- ✅ **SIEMPRE** pasar el token como parámetro (Server Actions no acceden a localStorage)
- ✅ **SIEMPRE** usar `cache: 'no-store'` en fetch para evitar cache de datos sensibles
- ❌ **NUNCA** importar `apiClient` en un archivo `'use server'`
- ❌ **NUNCA** acceder a `window`, `document`, o `localStorage` en Server Actions
- ❌ **NUNCA** pasar objetos no serializables (como `File`) directamente — usar `FormData`

---

## 🔗 Referencias

- [Next.js Server Actions Documentation](https://nextjs.org/docs/app/building-your-application/data-fetching/server-actions-and-mutations)
- [React Server Actions RFC](https://github.com/reactjs/rfcs/pull/227)
- [OWASP API Security Top 10](https://owasp.org/API-Security/)
- [BFF Pattern (Backend for Frontend)](https://learn.microsoft.com/en-us/azure/architecture/patterns/backends-for-frontends)

---

## 📊 Antes vs. Después — Resumen visual

```
╔══════════════════════════════════════════════════════════════════════════╗
║                    ANTES (sin Server Actions)                          ║
╠══════════════════════════════════════════════════════════════════════════╣
║                                                                        ║
║  DevTools Network Tab:                                                 ║
║  ┌────────────────────────────────────────────────────────────────┐    ║
║  │ POST /api/auth/login          200  application/json  45ms     │    ║
║  │ POST /api/checkout/process    200  application/json  120ms    │    ║
║  │ POST /api/kyc/kycprofiles     201  application/json  89ms     │    ║
║  │ POST /api/kyc/identity/verify 200  application/json  340ms    │    ║
║  └────────────────────────────────────────────────────────────────┘    ║
║                                                                        ║
║  Request Body visible:                                                 ║
║  { "email": "user@email.com", "password": "Secret123!" }              ║
║                                                                        ║
║  Response visible:                                                     ║
║  { "accessToken": "eyJhbGciOi...", "refreshToken": "xyz..." }         ║
║                                                                        ║
╚══════════════════════════════════════════════════════════════════════════╝

╔══════════════════════════════════════════════════════════════════════════╗
║                    DESPUÉS (con Server Actions)                        ║
╠══════════════════════════════════════════════════════════════════════════╣
║                                                                        ║
║  DevTools Network Tab:                                                 ║
║  ┌────────────────────────────────────────────────────────────────┐    ║
║  │ POST /login         200  text/x-component          48ms       │    ║
║  │ POST /checkout      200  text/x-component          125ms      │    ║
║  │ POST /cuenta/...    200  text/x-component          92ms       │    ║
║  └────────────────────────────────────────────────────────────────┘    ║
║                                                                        ║
║  Request Body: 0:["$@1",["serverLogin",...]]  (serializado)           ║
║                                                                        ║
║  Response: 0:["$@1",{"success":true}]  (sin tokens ni datos)          ║
║                                                                        ║
║  ❌ NO visible: /api/auth/login, credenciales, tokens, endpoints      ║
║                                                                        ║
╚══════════════════════════════════════════════════════════════════════════╝
```

---

_Documentación creada por el equipo de seguridad — Febrero 2026_
_OKLA | Next.js 14 Server Actions | BFF Pattern | Kubernetes NetworkPolicies_
