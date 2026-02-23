# HTTP Requests Audit: Seller & Dealer Conversion Flows

**Generated:** February 23, 2026  
**Repository:** cardealer-microservices  
**Audit Scope:** Guest → Seller and Guest → Dealer conversion flows

---

## Table of Contents

1. [Flujo Seller (Guest → Seller)](#flujo-seller-guest--seller)
2. [Flujo Dealer (Guest → Dealer)](#flujo-dealer-guest--dealer)
3. [Flujo Dealer Autenticado (Conversión de Dealer)](#flujo-dealer-autenticado-conversión-de-dealer)
4. [Hooks y Servicios Compartidos](#hooks-y-servicios-compartidos)
5. [Validación de Seguridad](#validación-de-seguridad)
6. [Endpoints Críticos y Discrepancias](#endpoints-críticos-y-discrepancias)

---

## Flujo Seller (Guest → Seller)

**Ruta:** `GET /vender/registro`  
**Componente:** [src/app/(main)/vender/registro/page.tsx](<src/app/(main)/vender/registro/page.tsx>)  
**Estado:** Draft autosave en localStorage, soporte para usuarios logueados y nuevos

### Paso 1: Registro de Cuenta (Nuevo Usuario)

**Componente:** [AccountStep](src/components/seller-wizard/account-step.tsx) (dentro del wizard)

```
REQUEST: POST /api/auth/register
METHOD: POST
HEADERS:
  - Content-Type: application/json
  - Authorization: (no requerido - es un guest)

BODY:
{
  "firstName": string,         // Sanitizado con sanitizeText()
  "lastName": string,          // Sanitizado con sanitizeText()
  "email": string,             // Sanitizado con sanitizeEmail()
  "phone": string | undefined, // Sanitizado con sanitizePhone()
  "password": string,          // NO sanitizado (mantiene contraseña segura)
  "acceptTerms": true,
  "accountType": "seller",     // O "dealer" si es dealer individual
  "userIntent": "sell"         // Auto-derived
}

VALIDATION (Frontend):
✓ firstName/lastName: min 2 caracteres, max 50
✓ email: formato válido, no SQL injection, no XSS
✓ phone: formato válido (opcional)
✓ password: 8+ chars, mayúscula, minúscula, número, carácter especial
✓ confirmPassword: debe coincidir con password
✓ acceptTerms: true requerido

RESPONSE (Server Action - serverRegister):
{
  "success": true,
  "error": null
}

FLOW NOTES:
- Se ejecuta en Server Action para seguridad (credentials no visibles en DevTools)
- NO se almacenan tokens después del registro
- Usuario DEBE verificar email antes de poder iniciar sesión
- Redirige automáticamente a login si el registro es exitoso
```

### Paso 2: Login Automático (Después de Registro)

```
REQUEST: POST /api/auth/login
METHOD: POST
HEADERS:
  - Content-Type: application/json

BODY:
{
  "email": string,
  "password": string,
  "rememberMe": false
}

VALIDATION (Frontend):
✓ email: formato válido
✓ password: no vacío

RESPONSE (Server Action - serverLogin):
{
  "success": true,
  "data": {
    "accessToken": string,
    "refreshToken": string,
    "user": {
      "id": string,
      "email": string,
      "firstName": string,
      "lastName": string,
      "fullName": string,
      "accountType": "seller",
      "isEmailVerified": false,
      "isVerified": false,
      ...
    },
    "requiresTwoFactor": false,
    "expiresIn": number
  }
}

STORE:
- accessToken → localStorage (legacy) + HttpOnly cookie (preferred)
- refreshToken → HttpOnly cookie
- user → context (useAuth)

ERROR CASES:
- 401: Email no verificado → redirect /verificar-email
- 429: Too many login attempts
- 2FA Required: Lanza TwoFactorRequiredError
```

### Paso 3: Crear Perfil de Vendedor (Nuevo Usuario)

**Componente:** [ProfileStep](src/components/seller-wizard/profile-step.tsx)

```
REQUEST: POST /api/sellers
METHOD: POST
HEADERS:
  - Content-Type: application/json
  - Authorization: Bearer {accessToken}

BODY (tipo Seller individual):
{
  "userId": string,
  "displayName": string,                    // Nombre de exhibición del vendedor
  "businessName": string,                   // Igual a displayName para individuales
  "description": string | undefined,        // Bio del vendedor
  "phone": string | undefined,
  "location": string | undefined,           // Ubicación / provincia
  "specialties": string[] | undefined       // Especialidades (ej: "Jeepetas", "Carros clásicos")
}

BODY (tipo Seller Dealer):
{
  "userId": string,
  "displayName": string,
  "businessName": string,                   // Nombre del negocio
  "rnc": string | undefined,                // RNC de empresa
  "description": string | undefined,
  "phone": string | undefined,
  "location": string | undefined,
  "specialties": string[] | undefined
}

VALIDATION (Frontend - Zod schemas):
✓ displayName: min 2 caracteres, no SQL injection, no XSS
✓ businessName: min 2 caracteres
✓ description: max 1000 caracteres
✓ specialties: array de strings, max 10

RESPONSE:
{
  "id": string,
  "userId": string,
  "displayName": string,
  "businessName": string,
  "description": string | null,
  "phone": string | null,
  "location": string | null,
  "specialties": string[],
  "isVerified": false,
  "averageRating": 0,
  "totalReviews": 0,
  "totalListings": 0,
  "activeSales": 0,
  "memberSince": ISO8601,
  "createdAt": ISO8601,
  "updatedAt": ISO8601 | null
}

REDIRECT:
- Success → /cuenta?registro=completado
- Storage: localStorage.removeItem('okla-seller-wizard-draft')
```

---

## Flujo Dealer (Guest → Dealer)

**Ruta:** `GET /registro/dealer`  
**Componente:** [src/app/(auth)/registro/dealer/page.tsx](<src/app/(auth)/registro/dealer/page.tsx>)  
**Estado:** Multi-step wizard (4 pasos), sin draft autosave

### Paso 1: Seleccionar Tipo de Dealer

```
ENTRADA:
- dealerType: "independent" | "chain"
- entityType: "individual" | "company"

VALIDACIÓN (Frontend):
✓ dealerType: requerido
✓ entityType: requerido
```

### Paso 2: Información del Negocio

```
REQUEST: POST /api/auth/register
METHOD: POST
HEADERS:
  - Content-Type: application/json

BODY:
{
  "firstName": string,          // Extraído de contactName (split por espacios)
  "lastName": string,           // Parte restante de contactName
  "email": string,              // Sanitizado
  "phone": string,              // Sanitizado
  "password": string,           // NO sanitizado
  "acceptTerms": true,
  "accountType": "dealer",
  "userIntent": "sell"
}

VALIDACIÓN (Frontend):
✓ businessName: min 1 carácter, max 200
✓ rnc: formato válido (si entityType=company)
✓ contactName: min 2 caracteres
✓ email: formato válido
✓ phone: formato válido
✓ password: 8+ chars, mayúscula, minúscula, número, carácter especial
✓ confirmPassword: debe coincidir
✓ All sanitized using:
   - sanitizeText (businessName, address, city)
   - sanitizeEmail (email)
   - sanitizePhone (phone)
   - sanitizeRNC (RNC)
   - sanitizeUrl (website)

RESPONSE:
{
  "success": true,
  "error": null
}

NOTES:
- Se almacena dealerPayload en localStorage como "pending-dealer-registration"
- Se ejecuta después de validaciones del paso 3 (ubicación)
```

### Paso 3: Ubicación del Negocio

```
ENTRADA:
- address: string (dirección completa)
- city: string
- province: string (provincia seleccionada)

VALIDACIÓN (Frontend):
✓ address: no vacío, max 300
✓ city: no vacío, max 100
✓ province: seleccionado de lista predefinida
```

### Paso 4: Auto-login y Crear Dealer

```
REQUEST #1: POST /api/auth/login
METHOD: POST
HEADERS:
  - Content-Type: application/json

BODY:
{
  "email": string,
  "password": string,
  "rememberMe": false
}

RESPONSE:
{
  "success": true,
  "data": {
    "accessToken": string,
    "refreshToken": string,
    "user": { ... },
    "expiresIn": number
  }
}

STORE: Tokens en cookies + localStorage (legacy)

---

REQUEST #2: POST /api/dealers
METHOD: POST
HEADERS:
  - Content-Type: application/json
  - Authorization: Bearer {accessToken}

BODY:
{
  "businessName": string,
  "businessRegistrationNumber": string | undefined,  // RNC
  "dealerType": "Chain" | "Independent",
  "email": string,
  "phone": string,
  "website": string | undefined,
  "address": string,
  "city": string,
  "state": string,                // Provincia
  "country": "DO"
}

RESPONSE:
{
  "id": string,
  "userId": string,
  "businessName": string,
  "email": string,
  "phone": string,
  "address": string,
  "city": string,
  "province": string,
  "type": "Independent" | "Chain",
  "status": "Pending" | "Active",
  "verificationStatus": "Pending" | "Approved",
  "currentPlan": string,
  "isSubscriptionActive": false,
  "maxActiveListings": number,
  "currentActiveListings": 0,
  "createdAt": ISO8601,
  "verifiedAt": null
}

REDIRECT:
- Success → /mis-vehiculos (dealer dashboard)
- Failure (auto-login required email verification) → /verificar-email?email=...&dealer=true
  - Guarda dealerPayload en localStorage para crear después de verificar email
```

---

## Flujo Dealer Autenticado (Conversión de Dealer)

**Ruta:** `GET /dealer/registro`  
**Componente:** [src/app/(main)/dealer/registro/page.tsx](<src/app/(main)/dealer/registro/page.tsx>)  
**Precondición:** Usuario autenticado con `accountType=dealer` sin dealerId

### Paso 1-3: Recopilación de Información

```
ENTRADA (formulario de 4 pasos):
- dealerType: "independent" | "chain"
- businessName: string
- rnc: string (opcional, según entityType)
- contactName: string
- email: string (pre-rellenado del usuario)
- phone: string (pre-rellenado del usuario)
- description: string
- address: string
- city: string
- province: string (lista predefinida)
- agreeTerms: boolean
- agreeVerification: boolean

VALIDACIÓN (Frontend):
- Paso 1: dealerType requerido
- Paso 2: businessName, email, phone, rnc (si company)
- Paso 3: address, city, province
- Paso 4: ambas aceptaciones requeridas
```

### Paso 4: Crear Perfil de Dealer (Usuario Autenticado)

```
REQUEST: POST /api/dealers
METHOD: POST
HEADERS:
  - Content-Type: application/json
  - Authorization: Bearer {accessToken}

BODY:
{
  "businessName": string,
  "legalName": string | undefined,
  "rnc": string | undefined,
  "type": "Independent" | "Chain",
  "description": string,
  "email": string,
  "phone": string,
  "website": string | undefined,
  "address": string,
  "city": string,
  "province": string,
  "logoUrl": undefined,
  "bannerUrl": undefined
}

RESPONSE:
(mismo que en flujo Guest → Dealer)

VALIDACIÓN (Frontend):
✓ dealerType: requerido
✓ businessName: min 1, max 200
✓ RNC: formato válido (si company)
✓ email: formato válido
✓ phone: formato válido
✓ address: min 1, max 300
✓ city: min 1
✓ province: seleccionado

ERROR CASES:
- 400: Validación fallida
- 401: Token expirado o usuario no autenticado
- 404: Endpoint no encontrado (servicio no disponible)
- 409: Dealer ya existe para este usuario

REDIRECT:
- Success → /dealer (dashboard)
- Error: Mostrar mensaje en pantalla, no redirect
```

---

## Hooks y Servicios Compartidos

### Hooks de Autenticación

**Hook:** [useAuth()](src/hooks/use-auth.tsx)

```typescript
// Exports
export function useAuth(): AuthContextValue
export function useRequireAuth(options?: { redirectTo?: string }): AuthContextValue

// Context Functions
login(data: LoginRequest): Promise<{ user: User }>
verifyTwoFactorLogin(tempToken: string, code: string): Promise<{ user: User }>
register(data: RegisterRequest): Promise<void>
logout(): Promise<void>
refreshUser(): Promise<void>
clearError(): void

// Uso en componentes
const { user, isAuthenticated, isLoading, register, login, logout } = useAuth()
```

### Hooks de Vendedor

**Hook:** [useSeller.ts](src/hooks/use-seller.ts)

```typescript
// Queries
useSellerProfile(sellerId: string | undefined): UseQuery
useSellerByUserId(userId: string | undefined): UseQuery
useSellerStats(sellerId: string | undefined): UseQuery

// Mutations
useConvertToSeller(): UseMutation<{
  data: ConvertToSellerRequest
  idempotencyKey?: string
}>
useCreateSellerProfile(): UseMutation<CreateSellerProfileRequest>
useUpdateSellerProfile(): UseMutation<{
  sellerId: string
  data: UpdateSellerProfileRequest
}>
```

### Hooks de Dealer

**Hook:** [useDealers.ts](src/hooks/use-dealers.ts)

```typescript
// Queries
useCurrentDealer(): UseQuery
useDealer(id: string | undefined): UseQuery
useDealerBySlug(slug: string | undefined): UseQuery
useDealers(params?: DealerSearchParams): UseQuery
useDealerStats(dealerId: string | undefined): UseQuery
useDealerLocations(dealerId: string | undefined): UseQuery

// Mutations
useCreateDealer(): UseMutation
useUpdateDealer(): UseMutation
useAddDealerLocation(dealerId: string): UseMutation
useUpdateDealerLocation(dealerId: string): UseMutation
useDeleteDealerLocation(dealerId: string): UseMutation

// Documents
useDealerDocuments(dealerId: string | undefined): UseQuery
useUploadDealerDocument(dealerId: string): UseMutation
useDeleteDealerDocument(dealerId: string): UseMutation
```

### Servicio de Autenticación

**Servicio:** [auth.ts](src/services/auth.ts)

```typescript
// Server Actions (ejecutadas en el servidor)
serverLogin(email, password, rememberMe?)
serverRegister(firstName, lastName, email, password, acceptTerms, phone?, accountType?, userIntent?)
serverLogout(refreshToken?, accessToken?)
serverVerify2FA(tempToken, code)
serverForgotPassword(email)
serverResetPassword(token, newPassword)
serverVerifyEmail(token)
serverResendVerification(email)

// Client functions
login(data: LoginRequest): Promise<{ user: User }>
register(data: RegisterRequest): Promise<{ email: string }>
logout(): Promise<void>
getCurrentUser(): Promise<User | null>
verifyTwoFactorLogin(tempToken, code): Promise<{ user: User }>
forgotPassword(email): Promise<void>
```

### Servicio de Vendedores

**Servicio:** [sellers.ts](src/services/sellers.ts)

```typescript
// API Calls
convertToSeller(
  data: ConvertToSellerRequest,
  idempotencyKey?: string
): Promise<SellerConversionResult>

getSellerProfile(sellerId: string): Promise<SellerProfile>
getSellerByUserId(userId: string): Promise<SellerProfile>
createSellerProfile(data: CreateSellerProfileRequest): Promise<SellerProfile>
updateSellerProfile(sellerId, data): Promise<SellerProfile>
getSellerStats(sellerId): Promise<SellerStats>
```

**Tipos:**

```typescript
interface ConvertToSellerRequest {
  businessName: string;
  description?: string;
  phone?: string;
  location?: string;
  specialties?: string[];
  acceptTerms: boolean;
}

interface CreateSellerProfileRequest {
  userId: string;
  businessName: string;
  displayName: string;
  description?: string;
  phone?: string;
  location?: string;
  specialties?: string[];
}
```

### Servicio de Dealers

**Servicio:** [dealers.ts](src/services/dealers.ts)

```typescript
// API Calls
createDealer(data: CreateDealerRequest): Promise<Dealer>
getDealerById(id: string): Promise<Dealer>
getDealerBySlug(slug: string): Promise<Dealer>
getDealerByUserId(userId: string): Promise<Dealer | null>
getCurrentDealer(): Promise<Dealer | null>
updateDealer(id, data): Promise<Dealer>
getDealerStats(dealerId): Promise<DealerStatsDto>
getDealerLocations(dealerId): Promise<DealerLocationDto[]>
addDealerLocation(dealerId, data): Promise<DealerLocationDto>
updateDealerLocation(dealerId, locationId, data): Promise<DealerLocationDto>
deleteDealerLocation(dealerId, locationId): Promise<void>
```

**Tipos:**

```typescript
interface CreateDealerRequest {
  businessName: string;
  dealerType?: "Independent" | "Chain";
  email: string;
  phone: string;
  address: string;
  city: string;
  state: string; // Province
  country?: string;
  businessRegistrationNumber?: string; // RNC
  website?: string;
  facebookUrl?: string;
  instagramUrl?: string;
  description?: string;
  logoUrl?: string;
  bannerUrl?: string;
  tradeName?: string;
  mobilePhone?: string;
  whatsApp?: string;
  taxId?: string;
  dealerLicenseNumber?: string;
}
```

---

## Validación de Seguridad

### Funciones de Sanitización (Frontend)

**Módulo:** [lib/security/sanitize.ts](src/lib/security/sanitize.ts)

```typescript
// Sanitizaciones aplicadas en TODOS los campos de entrada

sanitizeText(value: string, options?: {
  maxLength?: number
  allowSpecialChars?: boolean
}): string
// - Elimina XSS: scripts, HTML tags, eventos
// - Valida SQL injection
// - Limita longitud

sanitizeEmail(value: string): string
// - Valida formato RFC 5322
// - Normaliza (lowercase)
// - No permite SQL injection

sanitizePhone(value: string): string
// - Valida formato (números, +, -, espacios)
// - Elimina caracteres no válidos

sanitizeRNC(value: string): string
// - Formato: XXX-XXXXX-X
// - Solo números

sanitizeUrl(value: string): string | undefined
// - Valida protocolo (http/https)
// - Detecta JavaScript URLs
// - Retorna undefined si inválida

sanitizeHtml(html: string): string
// - DOMPurify
// - Whitelist de tags permitidos
```

### Validadores Zod (Frontend)

```typescript
// Seller wizard
sellerProfileSchema = z.object({
  displayName: z.string().min(2).max(100),
  businessName: z.string().min(2).max(200),
  description: z.string().max(1000).optional(),
  phone: z.string().optional(),
  location: z.string().optional(),
  specialties: z.array(z.string()).optional(),
});

sellerProfileDealerSchema = z.object({
  displayName: z.string().min(2).max(100),
  businessName: z.string().min(2).max(200),
  rnc: z.string().optional(),
  description: z.string().max(1000).optional(),
  phone: z.string().optional(),
  location: z.string().optional(),
  specialties: z.array(z.string()).optional(),
});

// Auth
registerSchema = z
  .object({
    firstName: z.string().min(2).max(50),
    lastName: z.string().min(2).max(50),
    email: z.string().email(),
    phone: z.string().optional(),
    password: z
      .string()
      .min(8)
      .regex(/[A-Z]/)
      .regex(/[a-z]/)
      .regex(/\d/)
      .regex(/[^a-zA-Z0-9]/),
    confirmPassword: z.string(),
    acceptTerms: z.boolean().refine((v) => v === true),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Las contraseñas no coinciden",
    path: ["confirmPassword"],
  });
```

### Headers de Seguridad

```
Authorization: Bearer {accessToken}
  - JWT token almacenado en HttpOnly cookie
  - Se envía automáticamente con apiClient

Content-Type: application/json
  - Explícito en todas las requests

CSRF Protection:
  - csrfFetch() wrapper para POST/PUT/DELETE
  - Token en X-CSRF-Token header
  - No necesario para requests dentro de mismo origin
```

---

## Endpoints Críticos y Discrepancias

### Endpoints Confirmados (Funcionando)

| Endpoint                            | Método   | Autenticado | Notas                                  |
| ----------------------------------- | -------- | ----------- | -------------------------------------- |
| `/api/auth/register`                | POST     | ❌          | Server Action, crea usuario base       |
| `/api/auth/login`                   | POST     | ❌          | Server Action, retorna tokens          |
| `/api/auth/me`                      | GET      | ✅          | Obtiene perfil autenticado             |
| `/api/sellers/convert`              | POST     | ✅          | Convierte buyer a seller, idempotencia |
| `/api/sellers`                      | POST     | ✅          | Crea perfil de vendedor                |
| `/api/sellers/{sellerId}`           | GET      | ❌          | Obtiene perfil de vendedor             |
| `/api/sellers/user/{userId}`        | GET      | ❌          | Obtiene vendedor por userId            |
| `/api/sellers/{sellerId}/stats`     | GET      | ❌          | Estadísticas del vendedor              |
| `/api/dealers`                      | POST     | ✅          | Crea perfil de dealer                  |
| `/api/dealers`                      | GET      | ❌          | Lista dealers con filtros              |
| `/api/dealers/{dealerId}`           | GET      | ❌          | Obtiene dealer por ID                  |
| `/api/dealers/slug/{slug}`          | GET      | ❌          | Obtiene dealer por slug                |
| `/api/dealers/user/{userId}`        | GET      | ❌          | Obtiene dealer por userId              |
| `/api/dealers/me`                   | GET      | ✅          | Obtiene dealer actual logueado         |
| `/api/dealers/{dealerId}/stats`     | GET      | ❌          | Estadísticas del dealer                |
| `/api/dealers/{dealerId}/locations` | GET/POST | ✅ (POST)   | Gestiona ubicaciones                   |
| `/api/dealers/{dealerId}/documents` | GET/POST | ✅ (POST)   | Gestiona documentos                    |

### Endpoints Potencialmente Problemáticos

| Endpoint                            | Estado         | Posible Problema              | Impacto                                       |
| ----------------------------------- | -------------- | ----------------------------- | --------------------------------------------- |
| `/api/sellers/convert`              | ⚠️ Investigar  | 404 si servicio no disponible | Conversión buyer→seller falla sin alternativa |
| `/api/dealers/{dealerId}/locations` | ⚠️ A verificar | Validación de geolocalización | Podría rechazar ubicaciones válidas           |
| `/api/dealers/{dealerId}/documents` | ⚠️ A verificar | Upload de archivos grandes    | Timeout si archivo > límite                   |
| `/api/auth/register`                | ✅ Confirmado  | Email verification requerido  | Bloquea login antes de verificar              |
| `/api/dealers/me`                   | ⚠️ Verificar   | CORS issue en dev             | Podría fallar en desarrollo local             |

### Errores Potenciales (Frontend)

1. **404 - Endpoint no encontrado**
   - `/api/sellers/convert` → Falla conversión buyer→seller
   - Solución: Fallback a crear nuevo perfil o guardar pending

2. **401 - No autenticado**
   - Token expirado
   - Cookie HttpOnly no se envía (CORS issue en dev)
   - Solución: Auto-logout y redirect a login

3. **400 - Validación**
   - RNC inválido
   - Email duplicado (409 mejor)
   - Dirección incompleta
   - Solución: Mostrar error específico, permitir corrección

4. **500 - Error servidor**
   - Podría ocurrir en cualquier endpoint
   - Solución: Retry automático, fallback, guardar state en localStorage

5. **429 - Rate limit**
   - Login/registro muy rápido
   - Solución: Mostrar cooldown, no reintentar automáticamente

### Draft & Offline Recovery

```typescript
// Seller wizard auto-save
localStorage.setItem('okla-seller-wizard-draft', JSON.stringify({
  step: number,
  account: { firstName, lastName, email, ... },  // Sin passwords
  profile: { displayName, businessName, ... },
  savedAt: ISO8601
}))

// Dealer pending registration
localStorage.setItem('pending-dealer-registration', JSON.stringify({
  businessName, email, phone, address, ...
}))

// Lectura en mount:
- Draft válido por 7 días
- Oferece "Continuar" vs "Empezar de nuevo"
- Genera toast con opción de reset
```

---

## Resumen de Validaciones

### Frontend (Antes de enviar)

✅ **Campos obligatorios**: Todos validados con Zod  
✅ **Formato correcto**: Email, teléfono, RNC, URL  
✅ **Longitud**: Min/Max por campo  
✅ **Sanitización**: XSS, SQL injection, caracteres especiales  
✅ **Contraseña**: Complejidad requerida (8+ chars, mayús, minús, número, especial)  
✅ **Coincidencia**: Confirmación de contraseña  
✅ **Términos**: Aceptación requerida

### Backend (Recibido)

⚠️ **A VERIFICAR**: Backend implementa validators similares?  
⚠️ **A VERIFICAR**: NoSqlInjection() y NoXss() validators en place?

---

## Flujo de Recuperación (Si Falla Creación de Dealer)

```
1. Usuario completa registration (POST /api/auth/register) ✅
2. Usuario se loguea (POST /api/auth/login) ✅
3. Intenta crear dealer (POST /api/dealers) ❌ FALLA

→ Guardar payload en localStorage: pending-dealer-registration
→ Redirect: /verificar-email?email=...&dealer=true
→ Usuario verifica email
→ Middleware detecta dealer=true param
→ Intenta crear dealer desde localStorage
→ Si éxito: Limpia localStorage, redirect /dealer
→ Si falla: Muestra error, permite reintentar manualmente
```

---

## Diferencias Clave: Seller vs Dealer

| Aspecto            | Seller                | Dealer                                   |
| ------------------ | --------------------- | ---------------------------------------- |
| **Página**         | `/vender/registro`    | `/registro/dealer`                       |
| **Pasos**          | 2 (Account + Profile) | 4 (Type + Business + Location + Confirm) |
| **Autenticado**    | Soporta ambos         | Solo guest                               |
| **RNC**            | Opcional              | Obligatorio si company                   |
| **Ubicación**      | Ingreso libre         | Selección provincias                     |
| **Draft**          | Sí (localStorage)     | No                                       |
| **Auto-login**     | Sí, después cuenta    | Sí, después registro                     |
| **Perfil tipo**    | Seller                | Dealer                                   |
| **Endpoint crear** | `/api/sellers`        | `/api/dealers`                           |
| **Verificación**   | Email + identidad     | Email + documentos                       |
| **Estado inicial** | `isVerified: false`   | `status: Pending`                        |

---

**Fin de Auditoría**

_Nota: Este documento debe ser actualizado si se detectan cambios en endpoints, payloads o flujos._
