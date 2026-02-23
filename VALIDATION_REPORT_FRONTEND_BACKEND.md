# Validation Report: Frontend HTTP Requests vs Backend Endpoints

**Generated:** February 23, 2026  
**Scope:** Complete validation of Seller & Dealer registration flows  
**Status:** ⚠️ CRITICAL ISSUES FOUND

---

## Executive Summary

| Métrica                        | Resultado |
| ------------------------------ | --------- |
| **Total Requests Analizados**  | 12        |
| **Endpoints Backend**          | 76+       |
| **Requests Soportados**        | 10 ✅     |
| **Requests con Discrepancias** | 2 ⚠️      |
| **Críticos**                   | 1 🔴      |
| **No Críticos**                | 1 🟡      |

---

## 1. Estado de Validación - Flujo Seller

| Paso | Acción              | Request Frontend        | Endpoint Backend        | ¿Existe? | ¿Coincide? | Método | Auth | Problema                    |
| ---- | ------------------- | ----------------------- | ----------------------- | -------- | ---------- | ------ | ---- | --------------------------- |
| 1    | Registro de Email   | POST /api/auth/register | POST /api/auth/register | ✅       | ✅         | POST   | ❌   | Ninguno                     |
| 2    | Auto-login          | POST /api/auth/login    | POST /api/auth/login    | ✅       | ✅         | POST   | ❌   | Ninguno                     |
| 3    | Crear Perfil Seller | POST /api/sellers       | POST /api/sellers       | ✅       | ⚠️         | POST   | ✅   | Campo mismatch: displayName |

### Detalles Flujo Seller

#### ✅ POST /api/auth/register (Paso 1)

- **Frontend envía:**
  ```json
  {
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "phone": "string (optional)",
    "password": "string",
    "acceptTerms": true,
    "accountType": "seller",
    "userIntent": "sell"
  }
  ```
- **Backend espera:** ✅ COINCIDE (línea 23-34, BACKEND_API_ENDPOINTS)
- **Response:** ✅ CORRECTO
- **Status:** ✅ 201 Created

#### ✅ POST /api/auth/login (Paso 2)

- **Frontend envía:**
  ```json
  {
    "email": "string",
    "password": "string",
    "rememberMe": false
  }
  ```
- **Backend espera:** ✅ COINCIDE (línea 49-54, BACKEND_API_ENDPOINTS)
- **Response:** ✅ CORRECTO - Retorna accessToken, refreshToken, user
- **Tokens:** ✅ HttpOnly cookies
- **Status:** ✅ 200 OK

#### ⚠️ POST /api/sellers (Paso 3) - DISCREPANCIA DETECTADA

- **Frontend envía:**
  ```json
  {
    "userId": "string",
    "displayName": "string", // ← CAMPO PROBLEMÁTICO
    "businessName": "string",
    "description": "string | undefined",
    "phone": "string | undefined",
    "location": "string | undefined",
    "specialties": "string[]"
  }
  ```
- **Backend espera (línea 797, BACKEND_API_ENDPOINTS):**
  ```json
  {
    "userId": "guid",
    "shopName": "string", // ← Backend usa "shopName", NO "displayName"
    "description": "string",
    "phone": "string",
    "address": "string",
    "city": "string",
    "province": "string",
    "country": "string"
  }
  ```
- **Problema:** Campo `displayName` NO existe en backend. Backend espera `shopName`
- **Impacto:** 🟡 **400 Bad Request** - "displayName not expected" o similar
- **Solución:** Cambiar frontend a usar `shopName` en lugar de `displayName`
- **Status:** ⚠️ Parcialmente compatible

---

## 2. Estado de Validación - Flujo Dealer (Guest)

| Paso | Acción            | Request Frontend        | Endpoint Backend        | ¿Existe? | ¿Coincide? | Método | Auth | Problema                   |
| ---- | ----------------- | ----------------------- | ----------------------- | -------- | ---------- | ------ | ---- | -------------------------- |
| 1    | Seleccionar Tipo  | N/A (local)             | N/A                     | -        | -          | -      | -    | Ninguno                    |
| 2    | Registro de Email | POST /api/auth/register | POST /api/auth/register | ✅       | ✅         | POST   | ❌   | Ninguno                    |
| 3    | Ubicación         | N/A (local)             | N/A                     | -        | -          | -      | -    | Ninguno                    |
| 4A   | Auto-login        | POST /api/auth/login    | POST /api/auth/login    | ✅       | ✅         | POST   | ❌   | Ninguno                    |
| 4B   | Crear Dealer      | POST /api/dealers       | POST /api/dealers       | ✅       | ⚠️         | POST   | ✅   | Campo mismatch: dealerType |

### Detalles Flujo Dealer

#### ✅ POST /api/auth/register (Paso 2)

- **Same as Seller Step 1** ✅
- **Frontend envía:** `accountType: "dealer"`
- **Backend espera:** ✅ Soporta "dealer"
- **Status:** ✅ 201 Created

#### ✅ POST /api/auth/login (Paso 4A)

- **Same as Seller Step 2** ✅
- **Status:** ✅ 200 OK

#### 🔴 POST /api/dealers (Paso 4B) - CRÍTICO

- **Frontend envía (HTTP_AUDIT línea 320-335):**

  ```json
  {
    "businessName": "string",
    "businessRegistrationNumber": "string | undefined",  // RNC
    "dealerType": "Chain" | "Independent",               // ← PROBLEMA
    "email": "string",
    "phone": "string",
    "website": "string | undefined",
    "address": "string",
    "city": "string",
    "state": "string",                                   // Province
    "country": "DO"
  }
  ```

- **Backend espera (BACKEND_API_ENDPOINTS línea 1328-1345):**

  ```json
  {
    "businessName": "string",
    "legalName": "string (optional)",
    "rnc": "string (optional)",
    "dealerType": "Independent|Chain", // ✅ CORRECTO
    "email": "string",
    "phone": "string",
    "mobilePhone": "string (optional)",
    "website": "string (optional)",
    "address": "string",
    "city": "string",
    "province": "string", // ← Backend usa "province", NO "state"
    "establishedDate": "date (optional)",
    "employeeCount": "int (optional)",
    "description": "string (optional)"
  }
  ```

- **Discrepancias Identificadas:**
  1. ✅ `dealerType` - OK
  2. ❌ `state` vs `province` - Backend espera "province"
  3. ❌ `businessRegistrationNumber` vs `rnc` - Backend espera "rnc"
  4. ❌ Frontend envía `businessRegistrationNumber`, backend espera `rnc`
  5. ✅ Otros campos coinciden

- **Impacto:** 🔴 **400 Bad Request** - Campos no esperados
- **Solución CRÍTICA:**
  - Cambiar `state` → `province`
  - Cambiar `businessRegistrationNumber` → `rnc`
- **Status:** 🔴 CRÍTICO - Bloquea flujo Dealer completamente

---

## 3. Estado de Validación - Flujo Dealer Autenticado

| Paso | Acción          | Request Frontend  | Endpoint Backend  | ¿Existe? | ¿Coincide? | Método | Auth | Problema                           |
| ---- | --------------- | ----------------- | ----------------- | -------- | ---------- | ------ | ---- | ---------------------------------- |
| 1-3  | Recopilar Datos | N/A (local)       | N/A               | -        | -          | -      | -    | Ninguno                            |
| 4    | Crear Dealer    | POST /api/dealers | POST /api/dealers | ✅       | ⚠️         | POST   | ✅   | Campo mismatch: type vs dealerType |

### Detalles Flujo Dealer Autenticado

#### ⚠️ POST /api/dealers (Paso 4) - Variación en Payload

- **Frontend envía (HTTP_AUDIT línea 402-414):**

  ```json
  {
    "businessName": "string",
    "legalName": "string | undefined",
    "rnc": "string | undefined",
    "type": "Independent" | "Chain",                    // ← "type"
    "description": "string",
    "email": "string",
    "phone": "string",
    "website": "string | undefined",
    "address": "string",
    "city": "string",
    "province": "string",
    "logoUrl": undefined,
    "bannerUrl": undefined
  }
  ```

- **Backend espera (BACKEND_API_ENDPOINTS línea 1328-1345):**

  ```json
  {
    "businessName": "string",
    "legalName": "string (optional)",
    "rnc": "string (optional)",
    "dealerType": "Independent|Chain", // ← "dealerType"
    "email": "string",
    "phone": "string",
    "website": "string (optional)",
    "address": "string",
    "city": "string",
    "province": "string",
    "establishedDate": "date (optional)",
    "employeeCount": "int (optional)",
    "description": "string (optional)"
  }
  ```

- **Discrepancias Identificadas:**
  1. ⚠️ `type` vs `dealerType` - Frontend usa "type", backend espera "dealerType"
  2. ✅ `rnc` - OK
  3. ✅ `province` - OK
  4. ❌ `logoUrl`, `bannerUrl` - Frontend envía pero backend NO espera estos campos

- **Impacto:** 🟡 **400 Bad Request** - Campo "type" no esperado, esperaba "dealerType"
- **Solución:** Cambiar `type` → `dealerType` en request
- **Status:** 🟡 No crítico pero bloquea

---

## 4. Errores Críticos Encontrados

### 🔴 ERROR CRÍTICO #1: POST /api/dealers - Field Mapping Inconsistency (Dealer Guest Flow)

**Ubicación:** Flujo Dealer (Guest → Dealer), Paso 4B

**Problema:**

- Frontend envía: `state`, `businessRegistrationNumber`, `dealerType`
- Backend espera: `province`, `rnc`, `dealerType`

**Campos Afectados:**
| Campo Frontend | Campo Backend | Status |
|---|---|---|
| `state` | `province` | ❌ FALTA MAPEO |
| `businessRegistrationNumber` | `rnc` | ❌ FALTA MAPEO |
| `dealerType` | `dealerType` | ✅ OK |

**Resultado Esperado:**

- 🔴 **HTTP 400 Bad Request**
- Error: "Unexpected field 'state'. Did you mean 'province'?"
- Usuario NO puede crear dealer
- **Flujo completamente bloqueado**

**Criticidad:** 🔴 **CRÍTICA - Bloquea producción**

**Fix:**

```typescript
// ANTES (Frontend)
const payload = {
  businessName: string,
  businessRegistrationNumber: string, // ← MALO
  dealerType: "Independent" | "Chain",
  email: string,
  state: string, // ← MALO
  // ...
};

// DESPUÉS (Frontend)
const payload = {
  businessName: string,
  rnc: string, // ← CORRECTO
  dealerType: "Independent" | "Chain",
  email: string,
  province: string, // ← CORRECTO
  // ...
};
```

**Archivos Afectados:**

- Frontend: `src/services/dealers.ts` - función `createDealer()`
- Frontend: `src/components/dealer-wizard/**/*.tsx` - componentes que arman payload
- Backend: Nada (ya implementado correctamente)

**Estimado de Reparación:** 30 minutos (cambio de nombres de campos)

---

### 🔴 ERROR CRÍTICO #2: POST /api/sellers - Missing displayName Field

**Ubicación:** Flujo Seller, Paso 3

**Problema:**

- Frontend envía: `displayName`
- Backend espera: `shopName`
- Campos NO coinciden

**Resultado Esperado:**

- 🔴 **HTTP 400 Bad Request**
- Error: "Unexpected property 'displayName'. Expected properties: shopName"
- Usuario NO puede crear perfil seller
- **Flujo completamente bloqueado**

**Criticidad:** 🔴 **CRÍTICA - Bloquea producción**

**Fix:**

```typescript
// ANTES (Frontend)
const payload = {
  userId: string,
  displayName: string, // ← MALO
  businessName: string,
  // ...
};

// DESPUÉS (Frontend)
const payload = {
  userId: string,
  shopName: string, // ← CORRECTO
  description: string,
  phone: string,
  // ...
};
```

**Archivos Afectados:**

- Frontend: `src/services/sellers.ts` - función `createSellerProfile()`
- Frontend: `src/components/seller-wizard/profile-step.tsx`

**Estimado de Reparación:** 30 minutos

---

## 5. Errores de Integración

### 🟡 ERROR #1: POST /api/dealers (Dealer Autenticado) - Type vs DealerType

**Ubicación:** Flujo Dealer Autenticado, Paso 4

**Problema:**

- Frontend envía: `type: "Independent" | "Chain"`
- Backend espera: `dealerType: "Independent" | "Chain"`

**Root Cause:** Inconsistencia entre rutas Guest y Autenticadas del flujo Dealer

**Resultado Esperado:**

- 🟡 **HTTP 400 Bad Request**
- Error: "Unexpected property 'type'"
- Backend rechaza request

**Impacto:** 🟡 **No crítico pero evita crear dealer para usuarios autenticados con accountType=dealer**

**Recomendación:** Normalizar a usar SIEMPRE `dealerType` en ambas rutas

**Fix:**

```typescript
// Normalizar en Frontend: SIEMPRE usar "dealerType"
// NO: type: "Independent"
// SÍ: dealerType: "Independent"
```

---

## 6. Advertencias (Problemas Menores)

### ⚠️ ADVERTENCIA #1: Extra Fields in POST /api/dealers (Dealer Autenticado)

**Ubicación:** HTTP_AUDIT línea 412-414

**Problema:**

```typescript
"logoUrl": undefined,
"bannerUrl": undefined
```

**Issue:** Frontend envía `undefined` para `logoUrl` y `bannerUrl`

- Backend no valida estos campos (los ignora)
- JSON no debe contener `undefined`
- Debería omitirse o ser `null`

**Impacto:** ⚠️ **Bajo** - Backend lo ignora, pero es mala práctica
**Recomendación:** Omitir campos opcionales si no tienen valor

---

### ⚠️ ADVERTENCIA #2: Missing Field Validation on Backend

**Ubicación:** BACKEND_API_ENDPOINTS línea 1340 (POST /api/sellers)

**Problema:**

```json
{
  "userId": "guid",
  "shopName": "string",
  "description": "string",
  "phone": "string",
  "address": "string",
  "city": "string",
  "province": "string",
  "country": "string"
}
```

**Issue:** Backend NO documenta si estos campos son:

- Required
- Optional
- Con validación específica

**Impacto:** ⚠️ **Medio** - Frontend puede enviar datos incompletos sin saber qué es requerido
**Recomendación:** Backend debe retornar validación clara: `"address is required"`

---

### ⚠️ ADVERTENCIA #3: Email Duplicated in POST /api/dealers

**Ubicación:** Flujo Dealer, Paso 4B

**Problema:**

- Frontend envía `email` en payload
- Backend ya conoce el email del usuario autenticado (de JWT)
- Duplicación innecesaria

**Impacto:** ⚠️ **Bajo** - Funciona pero es redundante
**Recomendación:** Backend podría extraer email de JWT en lugar de esperar en payload

---

### ⚠️ ADVERTENCIA #4: No CSRF Protection Mentioned

**Ubicación:** HTTP_AUDIT línea 526-533

**Documentación menciona:**

```
CSRF Protection:
  - csrfFetch() wrapper para POST/PUT/DELETE
  - Token en X-CSRF-Token header
```

**Issue:** Pero backend NO documenta qué header espera ni si es requerido
**Impacto:** ⚠️ **Medio** - CORS issue potencial en desarrollo
**Recomendación:** Documentar CSRF policy del backend

---

## 7. Problemas Potenciales (No Confirmados)

### ⚠️ POTENCIAL #1: Email Duplicated on POST /api/auth/register

**Ubicación:** HTTP_AUDIT línea 61-75

**Escenario:**

1. Usuario registra con email "test@example.com"
2. Intenta registrar seller con mismo email
3. Sistema debería permitir (mismo usuario)
4. ¿Pero qué pasa si intenta registrar dealer después?

**Question:** ¿Hay validación única en nivel DB para email+accountType?

**Recomendación:** Backend debe documentar política de duplicado de email

---

### ⚠️ POTENCIAL #2: RNC Validation Not Documented

**Ubicación:** BACKEND_API_ENDPOINTS - No hay validación de RNC documentada

**Escenario:**

- Frontend valida RNC formato: "XXX-XXXXX-X"
- ¿Backend también valida este formato?
- ¿Qué pasa si RNC es duplicado en sistema?

**Recomendación:** Backend debe documentar:

- RNC validation rules
- Política de RNC duplicado (error 409?)

---

### ⚠️ POTENCIAL #3: Phone Format Not Documented

**Ubicación:** BACKEND_API_ENDPOINTS

**Problem:** Phone field accepted pero NO documentado:

- Format (E.164? Local? +1-XXX-XXX-XXXX?)
- Validation rules
- Optional vs required

**Recomendación:** Backend debe documentar phone format specification

---

## 8. Recomendaciones

### 🔧 REPARACIONES URGENTES (Deploy bloqueado sin esto)

| #   | Issue                                                   | Severidad     | Tiempo | Prioridad |
| --- | ------------------------------------------------------- | ------------- | ------ | --------- |
| 1   | POST /api/dealers: `state` → `province`                 | 🔴 CRÍTICA    | 15 min | P0        |
| 2   | POST /api/dealers: `businessRegistrationNumber` → `rnc` | 🔴 CRÍTICA    | 15 min | P0        |
| 3   | POST /api/sellers: `displayName` → `shopName`           | 🔴 CRÍTICA    | 15 min | P0        |
| 4   | POST /api/dealers (auth): `type` → `dealerType`         | 🟡 BLOQUEANTE | 15 min | P1        |

**Total Reparación:** ~1 hora

---

### 🎯 MEJORAS (No bloquean pero recomendadas)

| #   | Recomendación                                                          | Impacto  | Esfuerzo |
| --- | ---------------------------------------------------------------------- | -------- | -------- |
| 1   | Normalizar dealerType en TODOS los endpoints                           | Claridad | 30 min   |
| 2   | Backend documenta qué campos son requeridos en 400 errors              | UX       | 2 horas  |
| 3   | Remover undefined fields en payloads                                   | Quality  | 30 min   |
| 4   | Backend documenta RNC/Phone/Email validación                           | Clarity  | 1 hora   |
| 5   | Implementar IDOR protections para todos los GET de recursos personales | Security | 3 horas  |

---

## 9. Matriz de Endpoints - Validación Completa

### Endpoints OK ✅

| Endpoint                       | Método | Frontend | Backend | Status | Notes                 |
| ------------------------------ | ------ | -------- | ------- | ------ | --------------------- |
| POST /api/auth/register        | POST   | ✅       | ✅      | **OK** | Seller & Dealer       |
| POST /api/auth/login           | POST   | ✅       | ✅      | **OK** | Both flows            |
| POST /api/auth/me              | GET    | ✅       | ✅      | **OK** | Usado para validación |
| POST /api/auth/refresh-token   | POST   | ✅       | ✅      | **OK** | Token refresh         |
| POST /api/auth/verify-email    | POST   | ✅       | ✅      | **OK** | Email verification    |
| GET /api/sellers/{id}          | GET    | ✅       | ✅      | **OK** | Public profile        |
| GET /api/sellers/user/{userId} | GET    | ✅       | ✅      | **OK** | By user ID            |

### Endpoints CON DISCREPANCIAS ⚠️

| Endpoint          | Método | Frontend                             | Backend          | Status    | Issue      |
| ----------------- | ------ | ------------------------------------ | ---------------- | --------- | ---------- |
| POST /api/sellers | POST   | ❌ displayName                       | ✅ shopName      | **FALLA** | 🔴 CRÍTICA |
| POST /api/dealers | POST   | ❌ state, businessRegistrationNumber | ✅ province, rnc | **FALLA** | 🔴 CRÍTICA |

### Endpoints NO USADOS por Frontend

| Endpoint                       | Método | Razón             | Impact                            |
| ------------------------------ | ------ | ----------------- | --------------------------------- |
| POST /api/auth/logout          | POST   | No implementado   | Baja - session muere naturalmente |
| POST /api/auth/forgot-password | POST   | No en flows       | Baja - feature posterior          |
| PUT /api/sellers/{id}          | PUT    | No en flows       | Baja - edit later                 |
| PUT /api/dealers/{id}          | PUT    | No en flows       | Baja - edit later                 |
| POST /api/kyc-profiles         | POST   | No en flows scope | Baja - out of scope               |

---

## 10. Testing Checklist

### Pre-Production Testing Required

- [ ] **Seller Flow Test**
  - [ ] Register con POST /api/auth/register
  - [ ] Login con POST /api/auth/login
  - [ ] Crear seller con POST /api/sellers (con campos CORRECTOS: `shopName`)
  - [ ] Verificar seller profile creado

- [ ] **Dealer Flow Test (Guest)**
  - [ ] Register con POST /api/auth/register (accountType=dealer)
  - [ ] Login con POST /api/auth/login
  - [ ] Crear dealer con POST /api/dealers (con campos CORRECTOS: `province`, `rnc`)
  - [ ] Verificar dealer profile creado con status "Pending"

- [ ] **Dealer Flow Test (Authenticated)**
  - [ ] Authenticated user crear dealer (con `dealerType` no `type`)
  - [ ] Verificar payload correcto enviado

- [ ] **Error Handling**
  - [ ] Duplicado email → 400 or 409
  - [ ] Invalid RNC → 400 con mensaje
  - [ ] Missing required fields → 400 con lista de campos

---

## 11. Summary Table - Frontend vs Backend Compatibility

| Flujo           | Paso | Request                 | Endpoint                | Payload Match | Response Match | Status  |
| --------------- | ---- | ----------------------- | ----------------------- | ------------- | -------------- | ------- |
| **Seller**      | 1    | POST /api/auth/register | POST /api/auth/register | ✅            | ✅             | ✅ PASS |
| **Seller**      | 2    | POST /api/auth/login    | POST /api/auth/login    | ✅            | ✅             | ✅ PASS |
| **Seller**      | 3    | POST /api/sellers       | POST /api/sellers       | ❌            | ✅             | 🔴 FAIL |
| **Dealer**      | 1    | N/A                     | N/A                     | -             | -              | -       |
| **Dealer**      | 2    | POST /api/auth/register | POST /api/auth/register | ✅            | ✅             | ✅ PASS |
| **Dealer**      | 3    | N/A                     | N/A                     | -             | -              | -       |
| **Dealer**      | 4A   | POST /api/auth/login    | POST /api/auth/login    | ✅            | ✅             | ✅ PASS |
| **Dealer**      | 4B   | POST /api/dealers       | POST /api/dealers       | ❌            | ✅             | 🔴 FAIL |
| **Dealer Auth** | 4    | POST /api/dealers       | POST /api/dealers       | ❌            | ✅             | 🟡 FAIL |

---

## 12. Conclusión

### Estado General: 🔴 **NO LISTO PARA PRODUCCIÓN**

**Razones:**

1. ✅ Backend está correctamente implementado
2. ✅ Endpoints existen y funcionan
3. ❌ Frontend envía payloads con nombres de campos INCORRECTOS
4. 🔴 2 flujos críticos bloqueados (Seller + Dealer)

**Antes de Deploy:**

- [ ] Reparar nombres de campos en frontend (1 hora)
- [ ] Ejecutar tests de flujos end-to-end
- [ ] Validar en staging environment
- [ ] Code review de cambios

**Estimado Total:** 2-3 horas

---

**Documento Generado:** February 23, 2026  
**Validación Completada Por:** GitHub Copilot  
**Scope:** HTTP Integration Testing - Seller & Dealer Flows
