# 🔀 DATA FLOW AUDIT: Impacto de Cambios en SellerProfile

**Fecha:** 24 de febrero de 2026  
**Status:** ⚠️ CRITICAL - Múltiples puntos de impacto identificados  
**Alcance:** Análisis completo del flujo de datos desde UI hasta BD y viceversa

---

## 📋 Tabla de Contenidos

1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Flujo Actual de Datos](#flujo-actual-de-datos)
3. [Componentes Afectados](#componentes-afectados)
4. [Servicios y Queries Afectados](#servicios-y-queries-afectados)
5. [Impacto por Campo](#impacto-por-campo)
6. [Matriz de Riesgos](#matriz-de-riesgos)
7. [Plan de Implementación Segura](#plan-de-implementación-segura)

---

## 📊 Resumen Ejecutivo

### Cambios Propuestos

1. **Remover teléfono duplicado** - Eliminar `phone` de Profile Step
2. **Persistir especialidades** - Agregar campo `Specialties[]` a SellerProfile
3. **Expandir ubicación** - Cambiar `location` string a `City`, `State`, `Address`, `ZipCode`
4. **Agregar campos opcionales** - DateOfBirth, Nationality, AlternatePhone, WhatsApp, etc.

### Nivel de Impacto

- 🔴 **CRÍTICO**: 5 cambios que afectan múltiples componentes
- 🟠 **ALTO**: 3 áreas de riesgo en datos existentes
- 🟡 **MEDIO**: 8 componentes que requieren actualización
- 🟢 **BAJO**: 2 cambios que son aditivos (no destructivos)

---

## 🔀 Flujo Actual de Datos

### **SENTIDO UP (UI → BD): Registro de Vendedor**

```
┌─────────────────────────────────────────────────────────────────┐
│ FRONTEND: Seller Registration Wizard                            │
├─────────────────────────────────────────────────────────────────┤
│ src/app/(main)/vender/registro/page.tsx                         │
│ ├─ Step 1: Account Creation                                     │
│ │  ├─ firstName, lastName, email, phone ✓                       │
│ │  └─ accountType: 'individual' | 'dealer'                      │
│ │                                                                │
│ ├─ Step 2: Profile Setup (ProfileStep.tsx)                      │
│ │  ├─ displayName ✓                                             │
│ │  ├─ businessName (dealers only)                               │
│ │  ├─ rnc (dealers only)                                        │
│ │  ├─ description ✓                                             │
│ │  ├─ phone ✓ (⚠️ DUPLICADO)                                    │
│ │  ├─ location ✓ (string genérico)                              │
│ │  └─ specialties[] ✓                                           │
│                                                                 │
│ HOOKS: use-seller.ts                                            │
│ ├─ useCreateSellerProfile()                                     │
│ │  └─ mutationFn: createSellerProfile(data)                     │
│                                                                 │
│ SERVICES: services/sellers.ts                                   │
│ ├─ createSellerProfile(data: CreateSellerProfileRequest)        │
│ │  └─ POST /api/sellers → CreateSellerProfileRequest            │
│                                                                 │
│ API REQUEST PAYLOAD:                                            │
│ {                                                               │
│   "userId": string,                                             │
│   "displayName": string,                                        │
│   "businessName": string? (dealers)                             │
│   "rnc": string? (dealers)                                      │
│   "description": string?,                                       │
│   "phone": string?,        ⚠️ PROBLEMA #1                       │
│   "location": string?,     ⚠️ PROBLEMA #2                       │
│   "specialties": string[]? ⚠️ PROBLEMA #3                       │
│ }                                                               │
│                                                                 │
│ BACKEND: SellerProfileController                               │
│ └─ POST /api/sellers/profile                                    │
│    ├─ CreateSellerProfileRequest DTO                            │
│    └─ CreateSellerProfileHandler (CQRS)                         │
│       ├─ Valida datos                                           │
│       ├─ Mapea a SellerProfile entity                           │
│       │  ├─ Phone → Phone ✓                                     │
│       │  ├─ description → Bio ✓                                 │
│       │  ├─ location → City (solo, incompleto) ⚠️               │
│       │  ├─ specialties → ??? (NO EXISTE CAMPO) ❌              │
│       │  └─ displayName → ??? (NO EXISTE MAPEO) ⚠️             │
│       ├─ Persiste en BD                                         │
│       └─ Publica evento: SellerProfileCreated                   │
│          └─ (RabbitMQ)                                          │
│              ├─ UserService subscripción                        │
│              ├─ VehiclesSaleService subscripción                │
│              ├─ ReviewService subscripción                      │
│              └─ NotificationService subscripción                │
│                                                                 │
│ DATABASE: UserServiceDbContext                                  │
│ └─ SellerProfiles table                                         │
│    ├─ Id, UserId ✓                                              │
│    ├─ FullName (?), Phone (?), Bio (de description)             │
│    ├─ Address (vacío), City (parcial), State (vacío) ⚠️        │
│    ├─ Specialties (NO EXISTE) ❌                                │
│    └─ DateOfBirth, Nationality, etc. (no capturados)            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### **SENTIDO DOWN (BD → UI): Lectura de Perfil**

```
┌─────────────────────────────────────────────────────────────────┐
│ BACKEND: GetSellerProfileQuery (CQRS)                          │
├─────────────────────────────────────────────────────────────────┤
│ SellerProfileController                                         │
│ ├─ GET /api/sellers/{sellerId}/profile (público)               │
│ ├─ GET /api/sellers/profile (autenticado)                       │
│ ├─ GET /api/sellers/by-user/{userId} (lookup)                  │
│ └─ GET /api/sellers/search (búsqueda)                           │
│                                                                 │
│ Mapea SellerProfile → SellerProfileDto                          │
│ ├─ Todos los campos físicos se incluyen                         │
│ ├─ Calcula métricas (ratings, listings count, etc)              │
│ └─ Serializa a JSON                                             │
│                                                                 │
│ RESPONSE TYPES:                                                 │
│ ├─ SellerProfileDto (completo)                                  │
│ ├─ SellerPublicProfileDto (público)                             │
│ ├─ SellerProfileSummaryDto (listados)                           │
│ └─ ContactPreferencesDto (preferencias)                         │
│                                                                 │
│ FRONTEND: services/sellers.ts                                   │
│ ├─ getSellerProfile(sellerId)                                   │
│ ├─ getSellerByUserId(userId)                                    │
│ └─ updateSellerProfile(sellerId, data)                          │
│                                                                 │
│ REACT QUERY HOOKS: use-seller.ts                                │
│ ├─ useSellerProfile(sellerId)                                   │
│ ├─ useSellerByUserId(userId)                                    │
│ └─ useUpdateSellerProfile()                                     │
│                                                                 │
│ CONSUMER COMPONENTS:                                            │
│ ├─ /cuenta/perfil/page.tsx                                      │
│ │  └─ Muestra todos los campos del perfil                       │
│ │     Sincroniza form con datos cargados                        │
│ │                                                               │
│ ├─ /publicar/page.tsx                                           │
│ │  └─ Valida que vendedor tenga perfil                          │
│ │     Muestra nombre en formulario                              │
│ │                                                               │
│ ├─ /cuenta/page.tsx                                             │
│ │  └─ Dashboard vendedor                                        │
│ │     Muestra estadísticas y perfil resumen                     │
│ │                                                               │
│ ├─ seller-card.tsx (en detalle de vehículo)                     │
│ │  └─ Muestra:                                                  │
│ │     • Nombre del vendedor (displayName/name)                  │
│ │     • Ciudad (city)                                           │
│ │     • Rating y reviews                                        │
│ │     • Teléfono (mostrado/oculto)                              │
│ │     • Tipo (dealer/seller)                                    │
│ │                                                               │
│ └─ dealer-profile-client.tsx                                    │
│    └─ Perfil público de dealer                                  │
│       Muestra info completa                                     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Componentes Afectados

### **1. FRONTEND - Páginas y Componentes**

#### A. Registration/Wizard (Escritura)

| Componente              | Ubicación                                   | Impacto    | Cambios Necesarios                                                                        |
| ----------------------- | ------------------------------------------- | ---------- | ----------------------------------------------------------------------------------------- |
| ProfileStep             | `components/seller-wizard/profile-step.tsx` | 🔴 CRÍTICO | **Remover `phone`**, agregar `location` expandida, manejar `specialties` con persistencia |
| SellerRegistrationPage  | `app/(main)/vender/registro/page.tsx`       | 🟠 ALTO    | Actualizar payload de CreateSellerProfileRequest, sincronizar con nuevos DTOs             |
| profile-step validation | `lib/validations/seller-onboarding.ts`      | 🟠 ALTO    | Actualizar schemas Zod (remover phone, expandir location)                                 |

#### B. Profile Edit (Lectura/Escritura)

| Componente  | Ubicación                                        | Impacto    | Cambios Necesarios                                                                     |
| ----------- | ------------------------------------------------ | ---------- | -------------------------------------------------------------------------------------- |
| ProfilePage | `app/(main)/cuenta/perfil/page.tsx`              | 🔴 CRÍTICO | Sincronizar form con nuevos campos, manejar `specialties`, mostrar ubicación expandida |
| SellerForm  | `app/(main)/cuenta/perfil/page.tsx` (form state) | 🟠 ALTO    | Agregar campos: `dateOfBirth`, `nationality`, `alternatePhone`, `whatsApp`, etc.       |

#### C. Publishing (Lectura)

| Componente             | Ubicación                         | Impacto  | Cambios Necesarios                                     |
| ---------------------- | --------------------------------- | -------- | ------------------------------------------------------ |
| PublishPage            | `app/(main)/publicar/page.tsx`    | 🟡 MEDIO | Si usa `specialties` para filtrar por tipo, actualizar |
| useSellerByUserId hook | `app/(main)/publicar/page.tsx:26` | 🟡 MEDIO | Verificar que datos retornados incluyan nuevos campos  |

#### D. Seller Card (Lectura)

| Componente          | Ubicación                                   | Impacto  | Cambios Necesarios                                     |
| ------------------- | ------------------------------------------- | -------- | ------------------------------------------------------ |
| SellerCard          | `components/vehicle-detail/seller-card.tsx` | 🟡 MEDIO | Si muestra ciudad o especialidades, validar estructura |
| Vehicle detail page | `app/(main)/vehiculos/[slug]/page.tsx`      | 🟡 MEDIO | Verificar que retorna datos completos del seller       |

#### E. Dashboard (Lectura)

| Componente    | Ubicación                    | Impacto  | Cambios Necesarios                                             |
| ------------- | ---------------------------- | -------- | -------------------------------------------------------------- |
| DashboardPage | `app/(main)/cuenta/page.tsx` | 🟡 MEDIO | Actualizar queries si usan datos de ubicación o especialidades |
| Seller stats  | `hooks/use-seller.ts`        | 🟡 MEDIO | Verificar que useSellerStats() incluye nuevos campos           |

### **2. BACKEND - Controllers, Handlers, DTOs**

#### A. Request/Response DTOs

| Archivo                    | Entidad                          | Impacto    | Cambios Necesarios                                                      |
| -------------------------- | -------------------------------- | ---------- | ----------------------------------------------------------------------- |
| CreateSellerProfileRequest | `Application/DTOs/SellerDtos.cs` | 🔴 CRÍTICO | Remover `phone`, agregar `address`, `state`, `zipCode`, `specialties[]` |
| UpdateSellerProfileRequest | `Application/DTOs/SellerDtos.cs` | 🔴 CRÍTICO | Ídem anterior                                                           |
| SellerProfileDto           | `Application/DTOs/SellerDtos.cs` | 🔴 CRÍTICO | Agregar `specialties[]`, expandir campos de ubicación                   |

#### B. Handlers (CQRS)

| Handler                      | Ubicación                       | Impacto    | Cambios Necesarios                                                                     |
| ---------------------------- | ------------------------------- | ---------- | -------------------------------------------------------------------------------------- |
| CreateSellerProfileHandler   | `Application/UseCases/Sellers/` | 🔴 CRÍTICO | **Mapear `specialties`**, mapear `displayName` a `FullName`, mapear ubicación completa |
| UpdateSellerProfileHandler   | `Application/UseCases/Sellers/` | 🔴 CRÍTICO | Ídem anterior                                                                          |
| GetSellerProfileQueryHandler | `Application/UseCases/Sellers/` | 🟠 ALTO    | Incluir `specialties[]` en DTO de respuesta                                            |

#### C. Entities

| Entidad       | Ubicación                          | Impacto    | Cambios Necesarios                                      |
| ------------- | ---------------------------------- | ---------- | ------------------------------------------------------- |
| SellerProfile | `Domain/Entities/SellerProfile.cs` | 🔴 CRÍTICO | **Agregar `public string[] Specialties { get; set; }`** |

#### D. Migrations

| Migración      | Ubicación                    | Impacto    | Cambios Necesarios                                                                      |
| -------------- | ---------------------------- | ---------- | --------------------------------------------------------------------------------------- |
| AddSpecialties | `Infrastructure/Migrations/` | 🔴 CRÍTICO | **Crear nueva migración** para agregar columna `specialties` (JSON array en PostgreSQL) |

#### E. Controllers

| Endpoint                | Ubicación                                    | Impacto  | Cambios Necesarios                                 |
| ----------------------- | -------------------------------------------- | -------- | -------------------------------------------------- |
| POST /api/sellers       | `Api/Controllers/SellerProfileController.cs` | 🟠 ALTO  | Validar nuevos DTOs                                |
| GET /api/sellers/search | `Api/Controllers/SellerProfileController.cs` | 🟡 MEDIO | Si agrega filtrado por especialidades, implementar |
| PUT /api/sellers/{id}   | `Api/Controllers/SellerProfileController.cs` | 🟠 ALTO  | Actualizar mapeo de request                        |

### **3. SERVICIOS EXTERNOS (Sincronización)**

#### A. Consumidores de SellerProfileCreated Event

| Servicio            | Ubicación | Evento               | Impacto  | Cambios Necesarios                                                         |
| ------------------- | --------- | -------------------- | -------- | -------------------------------------------------------------------------- |
| VehiclesSaleService | RabbitMQ  | SellerProfileCreated | 🟠 ALTO  | Si usa `specialties` para categorizar vehículos, actualizar listener       |
| ReviewService       | RabbitMQ  | SellerProfileCreated | 🟠 ALTO  | Si sincroniza datos de ubicación, validar que recibe `City`, `State`, etc. |
| NotificationService | RabbitMQ  | SellerProfileCreated | 🟡 MEDIO | Si personaliza mensajes con ubicación o especialidades                     |

#### B. Queries que leen SellerProfile

| Servicio | Operación                          | Impacto  | Cambios Necesarios                             |
| -------- | ---------------------------------- | -------- | ---------------------------------------------- |
| Gateway  | Proxies requests a /api/sellers/\* | 🟡 MEDIO | Validar que ocelot.json routes estén correctos |

---

## 📈 Impacto por Campo

### **CAMPO 1: Teléfono (Phone)**

```
CAMBIO: Remover de Profile Step (seguir usando el de Account Step)

IMPACTO:
  Frontend:
  ✓ ProfileStep.tsx: remover input de phone
  ✓ validations/seller-onboarding.ts: remover phone schema
  ✓ page.tsx (registro): no pasar phone en ProfileData
  ✓ /cuenta/perfil: phone sigue siendo editable (usar Account phone)

  Backend:
  ✓ CreateSellerProfileRequest: phone sigue presente (de Account)
  ✓ SellerProfile entity: phone sin cambios

  Riesgo: BAJO si se implementa correctamente

  QUIÉN CONSULTA:
  - seller-card.tsx: muestra phone si showPhone=true
  - SellerPublicProfileDto: incluye phone
  - VehiclesSaleService: puede usar para contacto
```

### **CAMPO 2: Ubicación (Location)**

```
CAMBIO: location (string) → City + State + Address + ZipCode

PROBLEMA ACTUAL:
  Formulario captura: "Santo Domingo, Distrito Nacional"
  BD espera: City=Santo Domingo, State=Distrito Nacional, Address=vacío, ZipCode=vacío

IMPACTO CRÍTICO:

  Frontend:
  ✓ profile-step.tsx: cambiar input genérico a:
    - Dropdown provincia (RD_PROVINCES)
    - Dropdown ciudad (por provincia)
    - Input dirección (opcional)
    - Input código postal (opcional)
  ✓ validations: actualizar schemas
  ✓ /cuenta/perfil: mostrar 4 campos en vez de 1
  ✓ Payload API: cambiar structure completamente

  Backend:
  ✓ CreateSellerProfileRequest: remover "location", agregar "address", "state", "zipCode"
  ✓ Handlers: mapear fields correctamente
  ✓ SellerPublicProfileDto: retornar fields separados
  ✓ Search endpoint: puede filtrar por city/state por separado

  CONSUMIDORES AFECTADOS:
  - seller-card.tsx: muestra city ✓
  - /cuenta/perfil: muestra "location" genérico ⚠️ NECESITA UPDATE
  - Maps/geolocation: puede usar address + coordinates
  - VehiclesSaleService: si indexa por location

  Riesgo: ALTO - Cambio de estructura de datos
```

### **CAMPO 3: Especialidades (Specialties)**

```
CAMBIO: Capturar en formulario → Persistir en BD

PROBLEMA ACTUAL:
  Frontend: captura specialties[] ✓
  Backend: request recibe specialties pero NO PERSISTE
  BD: no existe campo

IMPACTO CRÍTICO:

  Frontend:
  ✓ profile-step.tsx: UI ya existe (multi-select)
  ✓ /cuenta/perfil: UI ya existe
  ✓ Validations: ya está definido
  ✓ No requiere cambios (ya captura correctamente)

  Backend:
  ❌ CreateSellerProfileRequest: agregar "specialties[]?"
  ❌ UpdateSellerProfileRequest: agregar "specialties[]?"
  ❌ SellerProfile entity: AGREGAR "public string[] Specialties"
  ❌ Migration: CREAR para agregar columna
  ❌ Handler: mapear request.Specialties → entity.Specialties
  ❌ SellerProfileDto: incluir Specialties[]

  EVENTO RabbitMQ:
  - SellerProfileCreated debe incluir specialties[]
  - Consumidores deben sincronizar

  CONSUMIDORES POTENCIALES:
  - VehiclesSaleService: puede filtrar vehículos por especialidad del vendedor
  - Recomendación de búsqueda: mostrar sellers especializados
  - Admin dashboard: filtrar sellers por especialidad

  Riesgo: MEDIO - Cambio aditivo, pero requiere migration + mapeo completo
```

### **CAMPO 4: Información Adicional (DateOfBirth, Nationality, etc.)**

```
CAMBIOS:
  - Agregar DateOfBirth (opcional)
  - Agregar Nationality (opcional)
  - Agregar AlternatePhone (opcional)
  - Agregar WhatsApp (opcional)
  - Agregar AvatarUrl (foto de perfil)

IMPACTO: BAJO para cambios iniciales, pero propuestos para settings post-registro

IMPLEMENTACIÓN RECOMENDADA:
  Fase 1 (Inmediata): especidades + ubicación expandida
  Fase 2 (Settings UI): estos campos en página de settings

Riesgo: BAJO - Cambios son opcionales/aditivos
```

---

## ⚠️ Matriz de Riesgos

### **RIESGO #1: Datos huérfanos (Orphan Data)**

```
ESCENARIO:
Si usuarios ya tienen registrados specialties=[] y luego agregamos el campo,
pueden ver que sus especialidades "desaparecieron"

CÓMO OCURRE:
1. Usuario A registra con specialties=["Sedanes", "Jeepetas"] (actualmente se pierden)
2. Agregamos columna Specialties
3. Datos antiguos tienen Specialties=NULL
4. Frontend lee NULL en lugar de valores históricos

MITIGACIÓN:
✓ Mantener registro histórico en tabla separada (especialidades editadas)
✓ Migración debe establecer valores por defecto
✓ UI debe manejar nullables gracefully
✓ Notificar a usuarios para que reconfirmen especialidades
```

### **RIESGO #2: Incompatibilidad de API**

```
ESCENARIO:
Frontend envia location="Santo Domingo, DN"
Backend espera city, state, address por separado

CÓMO OCURRE:
Cambios parciales de frontend sin actualizar backend (o viceversa)
Requests fallan o datos se mapean incorrectamente

MITIGACIÓN:
✓ Cambios simultáneos en frontend + backend
✓ Tests de integración ANTES de deployar
✓ API versioning si cambios son radicales
✓ Validación estricta en API
```

### **RIESGO #3: Sincronización de Eventos**

```
ESCENARIO:
VehiclesSaleService espera specialties[] en evento SellerProfileCreated
Pero backend aún no genera el evento con esos datos

CÓMO OCURRE:
Cambios parciales en diferentes servicios
Services se ponen "out of sync"

MITIGACIÓN:
✓ Tests de RabbitMQ integration
✓ Validar que evento incluye todos los campos nuevos
✓ Consumidores deben ser defensivos (manejar missing fields)
✓ Deprecate old event, publish new version
```

### **RIESGO #4: Performance (Búsqueda/Indexing)**

```
ESCENARIO:
Si antes buscaban por location="Santo Domingo"
Ahora deben buscar por city="Santo Domingo" AND state="Distrito Nacional"

CÓMO OCURRE:
Cambios de estructura afectan queries y indexes

MITIGACIÓN:
✓ Actualizar Database indexes
✓ Optimizar search endpoint queries
✓ Tests de performance ANTES de producción
✓ Migration script para actualizar índices
```

### **RIESGO #5: Componentes Desactualizados**

```
ESCENARIO:
seller-card.tsx u otro componente espera structure de datos antiguo
Recibe nuevos datos y falla

CÓMO OCURRE:
Cambios en backend sin actualizar todos los consumers en frontend

MITIGACIÓN:
✓ Comprehensive grep search para encontrar todos los usos
✓ DTO versioning si necesario
✓ Tests de componentes
✓ Type safety con TypeScript
```

---

## 📋 Plan de Implementación Segura

### **FASE 0: Auditoría (COMPLETADO)**

- [x] Mapear flujo de datos actual
- [x] Identificar todos los componentes afectados
- [x] Documentar riesgos

### **FASE 1: Preparación (Antes de código)**

- [ ] Crear script de backup de datos actuales
- [ ] Crear migration DRY-RUN para validar sintaxis
- [ ] Actualizar documentación
- [ ] Revisar tests existentes

### **FASE 2: Backend - Cambios de Base de Datos**

**Orden IMPORTANTE:**

1. [ ] Crear migration: AddSpecialitiesToSellerProfile

   ```sql
   ALTER TABLE seller_profiles
   ADD COLUMN specialties jsonb DEFAULT '[]'::jsonb;
   ```

2. [ ] Actualizar SellerProfile entity

   ```csharp
   public string[] Specialties { get; set; } = Array.Empty<string>();
   ```

3. [ ] Actualizar DTOs

   ```csharp
   // CreateSellerProfileRequest
   public string[]? Specialties { get; set; }

   // SellerProfileDto
   public string[] Specialties { get; set; } = Array.Empty<string>();
   ```

4. [ ] Actualizar Handlers (CreateSellerProfileHandler, UpdateSellerProfileHandler)
   ```csharp
   sellerProfile.Specialties = request.Specialties ?? Array.Empty<string>();
   ```

### **FASE 3: Backend - DTOs de Ubicación**

1. [ ] Actualizar CreateSellerProfileRequest

   ```csharp
   public string? Address { get; set; }
   public string City { get; set; } = string.Empty;
   public string? State { get; set; }
   public string? ZipCode { get; set; }
   // REMOVER: public string? Location { get; set; }
   ```

2. [ ] Actualizar Handlers - Mapeo de ubicación

   ```csharp
   sellerProfile.City = request.City;
   sellerProfile.Address = request.Address;
   sellerProfile.State = request.State;
   sellerProfile.ZipCode = request.ZipCode;
   ```

3. [ ] Actualizar SellerPublicProfileDto para retornar ubicación expandida

### **FASE 4: Backend - Remover Phone Duplicado**

1. [ ] UpdateSellerProfileRequest

   ```csharp
   // REMOVER: public string? Phone { get; set; }
   // Mantener phone en CreateSellerProfileRequest (viene de Account)
   ```

2. [ ] Validar en handler que phone viene del Account Step, no de Profile

### **FASE 5: Frontend - ProfileStep**

1. [ ] Remover input de teléfono
2. [ ] Expandir input de ubicación (4 campos)
3. [ ] Actualizar validations/seller-onboarding.ts
4. [ ] Tests de ProfileStep

### **FASE 6: Frontend - Registration Page**

1. [ ] Actualizar ProfileData interface

   ```typescript
   interface ProfileData {
     displayName: string;
     businessName: string;
     rnc: string;
     description: string;
     // REMOVER: phone: string;
     // AGREGAR:
     city: string;
     state: string;
     address?: string;
     zipCode?: string;
     specialties: string[];
   }
   ```

2. [ ] Actualizar payload API
   ```typescript
   const payload = {
     userId,
     displayName,
     businessName,
     rnc,
     description,
     city,
     state,
     address,
     zipCode,
     specialties,
     // NO incluir phone aquí
   };
   ```

### **FASE 7: Frontend - Profile Settings Page (/cuenta/perfil)**

1. [ ] Actualizar sync de datos desde query
2. [ ] Mostrar 4 campos de ubicación
3. [ ] Mantener specialties editable
4. [ ] Actualizar submit handler

### **FASE 8: Frontend - Otros Componentes**

1. [ ] Revisar seller-card.tsx
2. [ ] Revisar dashboard pages
3. [ ] Actualizar tipos si necesario

### **FASE 9: Eventos y Sincronización**

1. [ ] Verificar SellerProfileCreated event incluye specialties[]
2. [ ] Actualizar listeners en VehiclesSaleService, ReviewService, NotificationService
3. [ ] Tests de integración RabbitMQ

### **FASE 10: Tests y QA**

1. [ ] Unit tests de handlers
2. [ ] Integration tests de endpoints
3. [ ] Tests de componentes React
4. [ ] Manual QA: registro completo seller
5. [ ] Manual QA: editar perfil
6. [ ] Manual QA: ver perfil público
7. [ ] Validar búsqueda de sellers

### **FASE 11: Deployment**

1. [ ] Deploy migration a BD primero
2. [ ] Deploy backend services (API, handlers)
3. [ ] Deploy frontend cambios
4. [ ] Monitor logs y errores

### **FASE 12: Post-Launch**

1. [ ] Verificar datos migraron correctamente
2. [ ] Ejecutar script de notificación a usuarios
3. [ ] Monitor de performance
4. [ ] Rollback plan si necesario

---

## 🔗 Interdependencias Identificadas

### **BLOQUEADORES ENCONTRADOS:**

1. **¿Dónde se mapea displayName a FullName?**
   - Actualmente ambiguo en el código
   - Necesita clarificación antes de implementar

2. **¿Location se persiste en algún lugar?**
   - Audit inicial sugiere que se pierden datos
   - Necesita verificación en handlers actuales

3. **¿Hay tests de CreateSellerProfileHandler?**
   - Importantes para validar nuevos mapeos
   - Necesita cobertura antes de cambios

4. **¿VehiclesSaleService consumo de especialidades?**
   - No encontrado en código actual
   - Pero se capturan en formulario
   - Necesita investigación

---

## 📝 Checklist de Validación Pre-Implementación

- [ ] ¿Todos los DTOs tienen actualización clara?
- [ ] ¿Migration script está escrito?
- [ ] ¿Tests existen para los nuevos campos?
- [ ] ¿Frontend tipos están sincronizados con backend DTOs?
- [ ] ¿RabbitMQ events incluyen nuevos campos?
- [ ] ¿Documentación actualizada (API docs)?
- [ ] ¿Plan de rollback escrito?
- [ ] ¿Notificación a users si es necesario?
- [ ] ¿Performance tests ejecutados?
- [ ] ¿Security review completed?

---

## 🎯 Conclusión

**RECOMENDACIÓN:**

✅ Implementar cambios en orden:

1. **Primero**: Especialidades (menos riesgoso, aditivo)
2. **Segundo**: Ubicación expandida (requiere coordinación UI/Backend)
3. **Tercero**: Remover phone duplicado (requiere cuidado máximo)

⚠️ **NO implementar todos simultáneamente** - Aumenta riesgo de bugs

📅 **Tiempo estimado**: 2-3 sprints (dependiendo de tests)

🚀 **Próximo paso**: Empezar con FASE 1 (preparación) antes de tocar código
