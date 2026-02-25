# 🔍 AUDIT: Trazabilidad Completa de Consumidores de SellerProfile

**Fecha:** 24 de febrero de 2026  
**Propósito:** Mapear EXACTAMENTE dónde se usan datos de SellerProfile en todo el sistema

---

## 📊 Tabla Maestra: Quién Consume Qué

### **1. PÁGINAS FRONTEND (UI Consumers)**

#### A. `/vender/registro` - Registro de Vendedor

```typescript
Archivo: src/app/(main)/vender/registro/page.tsx

CONSUME:
├─ useSellerByUserId(user?.id) [línea 171]
│  └─ Propósito: Evitar duplicados - si ya tiene perfil, redirect a /cuenta
│  └─ Campos usados: id (para verificación de existencia)
│  └─ IMPACTO: Si cambios en respuesta, puede romper guard logic
│
├─ useCreateSellerProfile() [línea 168]
│  └─ Propósito: Crear perfil en step 2
│  └─ Campos enviados: displayName, businessName, rnc, description, phone, location, specialties
│  └─ IMPACTO: ⚠️ MÁXIMO - datos que se envían deben coincidir con DTOs
│
└─ ProfileStep component [línea 106-126]
   └─ Captura: displayName, businessName, rnc, description, phone, location, specialties
   └─ IMPACTO: 🔴 CRÍTICO - remover phone, expandir location

CAMBIOS NECESARIOS:
  ✓ RemoverPhone: ProfileStep no debe enviar phone
  ✓ ExpandLocation: ProfileData debe tener city, state, address, zipCode
  ✓ KeepSpecialties: Mantener captura (ya está)
  ✓ ValidateSync: Validar que ProfileData coincide con CreateSellerProfileRequest DTO
```

#### B. `/cuenta/perfil` - Editar Perfil

```typescript
Archivo: src/app/(main)/cuenta/perfil/page.tsx

CONSUME:
├─ useSellerByUserId(accountType === 'seller' ? authUser?.id : undefined) [línea 177]
│  └─ Propósito: Cargar datos actuales para editar
│  └─ Campos usados: TODOS (sincroniza a form en línea 217)
│  └─ IMPACTO: 🔴 CRÍTICO - cualquier cambio de estructura rompe syncronización
│
├─ useUpdateSellerProfile() [línea 178]
│  └─ Propósito: Guardar cambios
│  └─ Campos enviados: dependiendo de form state
│  └─ IMPACTO: 🟠 ALTO - cambios en DTOs rompen update
│
├─ sellerForm state [línea 182-189]
│  ├─ businessName
│  ├─ displayName
│  ├─ description
│  ├─ phone
│  ├─ location
│  └─ specialties
│  └─ IMPACTO: ⚠️ NO coincide con SellerProfile entity (location vs city/state/address)
│
└─ handleSellerSubmit() [línea ??]
   └─ Construye payload para updateSellerProfile
   └─ IMPACTO: 🔴 CRÍTICO - payload estructura debe ser válida

CAMBIOS NECESARIOS:
  ✓ UpdateForms: Mostrar city, state, address, zipCode por separado
  ✓ SyncLogic: Mapeo de location string → 4 campos
  ✓ RemovePhone: No incluir phone en form (ya viene de Account)
  ✓ AddSpecialties: UI para editar specialties (ya existe)
```

#### C. `/publicar` - Publicar Vehículo

```typescript
Archivo: src/app/(main)/publicar/page.tsx

CONSUME:
├─ useSellerByUserId(isSeller ? user?.id : undefined) [línea 26]
│  └─ Propósito: Validar que vendedor tiene perfil antes de permitir publicación
│  └─ Campos usados: id (solo para verificación)
│  └─ IMPACTO: 🟡 BAJO - no necesita cambios
│
└─ Propósito general: Guardar seller ID en listing para link

CAMBIOS NECESARIOS:
  ✓ Ninguno (solo verificación de existencia)
```

#### D. `/cuenta` - Dashboard Vendedor

```typescript
Archivo: src/app/(main)/cuenta/page.tsx

CONSUME:
├─ useSellerByUserId(canSell ? user?.id : undefined) [línea 1072]
│  └─ Propósito: Mostrar resumen de perfil en dashboard
│  └─ Campos potencialmente usados: displayName, stats (ratings, listings, etc)
│  └─ IMPACTO: 🟡 MEDIO - si displayName cambia estructura, puede afectar UI
│
├─ useSellerStats(user?.id) [línea 56 - import, línea ?? - uso]
│  └─ Propósito: Mostrar estadísticas de vendedor
│  └─ Campos usados: totalListings, activeListings, ratings, reviews
│  └─ IMPACTO: 🟢 BAJO - datos de stats, no de perfil
│
└─ Resumen cards
   └─ Muestra info como "Eres vendedor", total listings, rating
   └─ IMPACTO: 🟡 MEDIO - si estructura de datos cambia

CAMBIOS NECESARIOS:
  ✓ Validar sincronización de datos
  ✓ Tests si usa displayName o datos de ubicación
```

#### E. `vehicle-detail/seller-card` - Card en Detalle de Vehículo

```typescript
Archivo: src/components/vehicle-detail/seller-card.tsx

CONSUME:
├─ Vehicle.seller?: {
│  ├─ id: string
│  ├─ name: string (¿es displayName?)
│  ├─ type: 'seller' | 'dealer'
│  ├─ avatar?: string
│  ├─ phone?: string (⚠️ cuidado con cambios)
│  ├─ email?: string
│  ├─ city?: string (¿de dónde viene esto?)
│  ├─ rating?: number
│  ├─ reviewCount?: number
│  ├─ responseRate?: number
│  ├─ responseTime?: string
│  ├─ isVerified?: boolean
│  ├─ memberSince?: string
│  └─ listingsCount?: number
│
└─ PROBLEMA: ¿De dónde viene vehicle.seller data?
   ¿Se popula en Vehicle query?
   ¿O es nested object?

CAMBIOS NECESARIOS:
  ✓ Investigar de dónde viene vehicle.seller
  ✓ Si viene de SellerProfile, validar que city viene de nuevo structure
  ✓ Si viene de Vehicle entity, puede que no se afecte
```

#### F. `dealer-profile-client` - Perfil Público de Dealer

```typescript
Archivo: src/app/(main)/dealers/[slug]/dealer-profile-client.tsx

CONSUME:
├─ useDealer(slug) - probablemente DealerProfile, no SellerProfile
│
└─ PROBABLEMENTE NO AFECTADO (es dealer, no seller)
```

### **2. HOOKS REACT QUERY (Query Definitions)**

#### A. `use-seller.ts` - Hook Central

```typescript
Archivo: src/hooks/use-seller.ts

DEFINE:
├─ useSellerProfile(sellerId)
│  └─ Queries: GET /api/sellers/{sellerId}
│  └─ Return: SellerProfile (completo)
│  └─ Usado por: seller-card.tsx, vendor dashboard, search results
│  └─ IMPACTO: 🔴 CRÍTICO si estructura cambia
│
├─ useSellerByUserId(userId)
│  └─ Queries: GET /api/sellers/by-user/{userId}
│  └─ Return: SellerProfile (completo)
│  └─ Usado por: registro page, perfil page, dashboard, publish page
│  └─ IMPACTO: 🔴 CRÍTICO
│
├─ useSellerStats(sellerId)
│  └─ Queries: GET /api/sellers/{sellerId}/stats
│  └─ Return: SellerStats
│  └─ Usado por: dashboard, vendor profile
│  └─ IMPACTO: 🟡 BAJO (datos diferentes)
│
├─ useConvertToSeller()
│  └─ Mutation: POST /api/users/convert-to-seller
│  └─ Return: SellerConversionResult
│  └─ IMPACTO: 🟢 BAJO (conversión, no profile data)
│
├─ useCreateSellerProfile()
│  └─ Mutation: POST /api/sellers
│  └─ Input: CreateSellerProfileRequest
│  └─ Return: SellerProfile
│  └─ IMPACTO: 🔴 CRÍTICO - DTOs deben coincidir
│
└─ useUpdateSellerProfile()
   └─ Mutation: PUT /api/sellers/{sellerId}
   └─ Input: UpdateSellerProfileRequest
   └─ Return: SellerProfile
   └─ IMPACTO: 🔴 CRÍTICO - DTOs deben coincidir

CAMBIOS NECESARIOS:
  ✓ Los hooks en sí no cambian (son genéricos)
  ✓ Pero los DTOs que importan sí deben actualizarse
  ✓ Types: SellerProfile, CreateSellerProfileRequest, UpdateSellerProfileRequest
```

### **3. SERVICIOS API (services/sellers.ts)**

```typescript
Archivo: src/services/sellers.ts

OPERACIONES:
├─ getSellerProfile(sellerId): Promise<SellerProfile>
│  └─ HTTP: GET /api/sellers/{sellerId}
│  └─ Return type DEBE incluir: specialties[], city, state, address, zipCode
│  └─ IMPACTO: 🔴 CRÍTICO
│
├─ getSellerByUserId(userId): Promise<SellerProfile>
│  └─ HTTP: GET /api/sellers/by-user/{userId}
│  └─ Return type DEBE incluir: specialties[], city, state, address, zipCode
│  └─ IMPACTO: 🔴 CRÍTICO
│
├─ createSellerProfile(data): Promise<SellerProfile>
│  └─ HTTP: POST /api/sellers
│  └─ Input type DEBE tener: city, state, address?, zipCode? (NO location string)
│  └─ IMPACTO: 🔴 CRÍTICO
│
├─ updateSellerProfile(sellerId, data): Promise<SellerProfile>
│  └─ HTTP: PUT /api/sellers/{sellerId}
│  └─ Input type DEBE tener: city?, state?, address?, zipCode?, specialties?
│  └─ IMPACTO: 🔴 CRÍTICO
│
└─ getSellerStats(sellerId): Promise<SellerStats>
   └─ HTTP: GET /api/sellers/{sellerId}/stats
   └─ IMPACTO: 🟢 BAJO (stats diferentes)

TIPOS DEFINIDOS:
├─ SellerProfile interface
│  └─ DEBE INCLUIR:
│     ├─ id, userId, displayName, phone, email
│     ├─ city, state, address, zipCode (⚠️ actualmente location?)
│     ├─ specialties[] (⚠️ actualmente ausente?)
│     ├─ dateOfBirth?, nationality?, avatar?, bio?
│     └─ stats: ratings, reviews, totalListings, etc.
│
├─ CreateSellerProfileRequest interface
│  └─ DEBE INCLUIR:
│     ├─ userId, displayName, businessName?, rnc?
│     ├─ description?, city, state, address?, zipCode?
│     ├─ specialties?[]
│     └─ NO: phone (viene de Account Step)
│
└─ UpdateSellerProfileRequest interface
   └─ SIMILAR a CreateSellerProfileRequest pero todos opcional

CAMBIOS NECESARIOS:
  ✓ Actualizar SellerProfile interface: agregar specialties[], expandir location
  ✓ Actualizar CreateSellerProfileRequest: remover location, agregar city/state/address/zipCode
  ✓ Actualizar UpdateSellerProfileRequest: ídem
  ✓ Validación TypeScript garantiza sincronización
```

### **4. BACKEND ENDPOINTS (SellerProfileController)**

```csharp
Archivo: Api/Controllers/SellerProfileController.cs

ENDPOINTS PÚBLICOS:
├─ GET /api/sellers/{sellerId}/profile
│  └─ Returns: SellerPublicProfileDto
│  └─ IMPACTO: 🟠 ALTO si DTO cambia
│
├─ GET /api/sellers/search?q=&city=&type=
│  └─ Returns: PaginatedSellersResponse
│  └─ QUERY PARAMS: ?city= (¿usar esto para nueva estructura city?)
│  └─ IMPACTO: 🟠 ALTO si query logic cambia
│
└─ GET /api/sellers/top?count=&city=
   └─ Returns: List<SellerProfileSummaryDto>
   └─ IMPACTO: 🟡 MEDIO

ENDPOINTS AUTENTICADOS:
├─ GET /api/sellers/profile (mi perfil)
│  └─ Returns: SellerProfileDto
│  └─ IMPACTO: 🔴 CRÍTICO
│
├─ PUT /api/sellers/{id} (actualizar)
│  └─ Input: UpdateSellerProfileRequest
│  └─ Returns: SellerProfileDto
│  └─ IMPACTO: 🔴 CRÍTICO
│
├─ POST /api/sellers/profile (crear)
│  └─ Input: CreateSellerProfileRequest
│  └─ Returns: SellerProfileDto
│  └─ IMPACTO: 🔴 CRÍTICO
│
└─ GET /api/sellers/{sellerId}/contact-preferences
   └─ Returns: ContactPreferencesDto
   └─ IMPACTO: 🟡 BAJO (datos diferentes)

CAMBIOS NECESARIOS:
  ✓ Validar DTOs que retorna cada endpoint
  ✓ Si cambios, necesita DTO versioning o migration
  ✓ Tests que validen estructura de respuesta
```

### **5. HANDLERS CQRS (Application Layer)**

```csharp
Archivo: Application/UseCases/Sellers/

HANDLERS AFECTADOS:

├─ CreateSellerProfileHandler
│  └─ Input: CreateSellerProfileCommand
│  └─ Output: SellerProfileDto
│  └─ CRITICAL LOGIC:
│     ├─ Mapea CreateSellerProfileRequest → SellerProfile entity
│     ├─ MAPEOS ACTUALES:
│     │  ├─ request.Phone → profile.Phone ✓
│     │  ├─ request.Description → profile.Bio ✓
│     │  ├─ request.Location → ??? (¿dónde va?)
│     │  └─ request.Specialties → ??? (NO EXISTE CAMPO)
│     └─ CAMBIOS NECESARIOS:
│        ├─ Agregar: request.City → profile.City
│        ├─ Agregar: request.State → profile.State
│        ├─ Agregar: request.Address → profile.Address
│        ├─ Agregar: request.ZipCode → profile.ZipCode
│        ├─ Agregar: request.Specialties → profile.Specialties
│        └─ REMOVER o ignorar: request.Phone (viene de Account)
│  └─ IMPACTO: 🔴 CRÍTICO
│
├─ UpdateSellerProfileHandler
│  └─ CAMBIOS: Ídem anterior (para campos opcionales)
│  └─ IMPACTO: 🔴 CRÍTICO
│
├─ GetSellerProfileQueryHandler
│  └─ Output: SellerProfileDto
│  └─ Mapea: SellerProfile entity → DTO
│  └─ CAMBIOS: Agregar specialties a mapeo
│  └─ IMPACTO: 🟠 ALTO
│
└─ GetSellerProfileByUserQueryHandler
   └─ CAMBIOS: Ídem anterior
   └─ IMPACTO: 🟠 ALTO

CAMBIOS NECESARIOS:
  ✓ Mapeos completos en handlers
  ✓ Tests de mapping logic
  ✓ Validación de datos mapeados
```

### **6. DTOs (Data Transfer Objects)**

```csharp
Archivo: Application/DTOs/SellerProfileDtos.cs

TIPOS DEFINIDOS:

├─ SellerProfileDto (Respuesta completa)
│  └─ PROPIEDADES ACTUALES:
│     ├─ Id, UserId, FullName, DateOfBirth, Nationality, Bio, AvatarUrl
│     ├─ Phone, AlternatePhone, WhatsApp, Email
│     ├─ Address, City, State, ZipCode, Country, Latitude, Longitude
│     ├─ VerificationStatus, VerifiedAt, RejectionReason
│     ├─ TotalListings, ActiveListings, TotalSales, AverageRating, TotalReviews
│     ├─ ResponseTimeMinutes, IsActive, AcceptsOffers, ShowPhone, ShowLocation
│     └─ FALTA: Specialties[]
│
│  └─ CAMBIOS NECESARIOS:
│     └─ Agregar: public string[] Specialties { get; set; } = Array.Empty<string>();
│
├─ SellerPublicProfileDto (Respuesta pública - vista de comprador)
│  └─ PROPIEDADES:
│     ├─ Id, UserId, DisplayName, Type, Bio, ProfilePhotoUrl, CoverPhotoUrl
│     ├─ City, Province, MemberSince, IsVerified, Badges, Stats, Dealer
│     └─ FALTA: Specialties[]
│
│  └─ CAMBIOS NECESARIOS:
│     ├─ Agregar: public string[] Specialties { get; set; } = new();
│     ├─ Cambiar: Province (¿es State en entity?)
│     └─ Validar: City se llena correctamente de nuevo structure
│
├─ CreateSellerProfileRequest (Request para crear)
│  └─ PROPIEDADES ACTUALES:
│     ├─ UserId, FullName, DateOfBirth, Nationality, Phone, AlternatePhone, WhatsApp
│     ├─ Email, Address, City, State, ZipCode, Country, Latitude, Longitude
│     ├─ AcceptsOffers, ShowPhone, ShowLocation, PreferredContactMethod
│     └─ FALTA: Specialties[], tal vez Businessname?, displayName?
│
│  └─ PROBLEMA: Frontend envía CreateSellerProfileRequest pero con estructura diferente:
│     ├─ Frontend: displayName, businessName, description, phone, location, specialties
│     ├─ Backend espera: userId, fullName, phone, address, city, state, etc.
│     └─ ¡NO HAY MAPEO CLARO!
│
│  └─ CAMBIOS NECESARIOS:
│     ├─ Agregar: public string[]? Specialties { get; set; }
│     ├─ Revisar: ¿dónde va businessName en entity?
│     ├─ Revisar: ¿dónde va displayName en entity?
│     ├─ Revisar: ¿location string cómo se parseaba? ⚠️ CRÍTICO
│     └─ Actualizar: Estructura para recibir city, state, address, zipCode separados
│
├─ UpdateSellerProfileRequest (Request para actualizar)
│  └─ SIMILAR A CreateSellerProfileRequest pero todos optional
│
└─ Otros DTOs:
   ├─ SellerProfileSummaryDto (para listados/search)
   ├─ SellerPublicStatsDto (estadísticas públicas)
   ├─ ContactPreferencesDto (preferencias de contacto)
   └─ IMPACTO: 🟡 MEDIO

CAMBIOS NECESARIOS:
  ✓ Clarificar mapeos: frontend fields → backend fields
  ✓ Agregar Specialties a todos los DTOs
  ✓ Expandir location a city/state/address/zipCode
  ✓ Verificar displayName/businessName/fullName
```

### **7. ENTIDAD (Domain Model)**

```csharp
Archivo: Domain/Entities/SellerProfile.cs

PROPIEDADES ACTUALES:
├─ Id, UserId
├─ FullName, DateOfBirth, Nationality, Bio, AvatarUrl
├─ Phone, AlternatePhone, WhatsApp, Email
├─ Address, City, State, ZipCode, Country, Latitude, Longitude
├─ VerificationStatus, VerifiedAt, VerifiedByUserId, VerificationNotes, RejectionReason
├─ TotalListings, ActiveListings, TotalSales, AverageRating, TotalReviews, ResponseTimeMinutes
├─ IsActive, AcceptsOffers, ShowPhone, ShowLocation
└─ CreatedAt, UpdatedAt, DeletedAt (audit)

PROPIEDADES FALTANTES:
├─ Specialties[] ⚠️ CRÍTICO
├─ DisplayName (¿se mapea a FullName?)
├─ BusinessName (¿dónde se guarda?)
└─ PreferredContactMethod (existe?)

CAMBIOS NECESARIOS:
  ✓ Agregar: public string[] Specialties { get; set; } = Array.Empty<string>();
  ✓ Clarificar: ¿FullName vs DisplayName?
  ✓ Clarificar: ¿Dónde va BusinessName?
  ✓ Validar: Todos los campos de ubicación existen
```

### **8. EVENTOS RABBITMQ (Domain Events)**

```csharp
Evento: SellerProfileCreated
Publicado por: CreateSellerProfileHandler
Consumido por: VehiclesSaleService, ReviewService, NotificationService

PAYLOAD ACTUAL:
├─ Id, UserId, FullName, Email, Phone
├─ City, Country
└─ Timestamp

CAMPOS FALTANTES:
├─ Specialties[] ⚠️ CRÍTICO si VehiclesSaleService los necesita
├─ DisplayName/BusinessName
├─ Address, State, ZipCode
└─ VerificationStatus

CAMBIOS NECESARIOS:
  ✓ Agregar Specialties[] al evento
  ✓ Agregar fields de ubicación expandida
  ✓ Consumidores deben validar y usar nuevos campos
  ✓ BACKWARD COMPATIBILITY: versionar evento si es breaking change
```

### **9. SERVICIOS EXTERNOS (Consumidores de Eventos)**

#### A. VehiclesSaleService

```
Escucha: SellerProfileCreated
Acción: ¿Qué hace con los datos?
  - ¿Copia a Vehicle tabla?
  - ¿Índices de búsqueda?
  - ¿Usa especialidades para categorización?

IMPACTO: 🟠 ALTO si usa specialties
CAMBIOS NECESARIOS:
  ✓ Revisar consumer de SellerProfileCreated
  ✓ Validar que maneja nuevos campos
  ✓ Tests de integración
```

#### B. ReviewService

```
Escucha: SellerProfileCreated
Acción: ¿Sincroniza datos de seller?

IMPACTO: 🟡 MEDIO
CAMBIOS NECESARIOS:
  ✓ Revisar consumer
  ✓ Validar compatible con nuevos datos
```

#### C. NotificationService

```
Escucha: SellerProfileCreated (posiblemente)
Acción: ¿Envía emails de bienvenida?

IMPACTO: 🟡 BAJO
CAMBIOS NECESARIOS:
  ✓ Revisar si templates usan datos de specialties/ubicación
```

---

## 📋 Matriz de Riesgos por Consumidor

| Componente                   | Criticidad | Riesgo   | Cambios                                          | Tests            |
| ---------------------------- | ---------- | -------- | ------------------------------------------------ | ---------------- |
| ProfileStep (registro)       | 🔴 CRÍTICO | Alto     | Remover phone, expandir location                 | ✅ Necesarios    |
| ProfilePage (edit)           | 🔴 CRÍTICO | Alto     | Sync form data, expandir location                | ✅ Necesarios    |
| useSellerProfile hook        | 🔴 CRÍTICO | Alto     | Types deben sincronizarse                        | ✅ Necesarios    |
| CreateSellerProfileHandler   | 🔴 CRÍTICO | Alto     | Mapeos completamente nuevos                      | ✅ Necesarios    |
| UpdateSellerProfileHandler   | 🔴 CRÍTICO | Alto     | Mapeos completamente nuevos                      | ✅ Necesarios    |
| SellerProfileDto             | 🔴 CRÍTICO | Alto     | Agregar specialties[], expandir location         | ✅ Necesarios    |
| SellerProfile entity         | 🔴 CRÍTICO | Alto     | Agregar Specialties[]                            | ✅ Necesarios    |
| seller-card component        | 🟠 ALTO    | Medio    | Validar que recibe city correctly                | ✅ Necesarios    |
| Dashboard page               | 🟠 ALTO    | Medio    | Validar que muestra displayName correctly        | ✅ Necesarios    |
| VehiclesSaleService consumer | 🟠 ALTO    | Medio    | Manejar specialties[] si los usa                 | ✅ Necesarios    |
| SellerPublicProfileDto       | 🟠 ALTO    | Medio    | Agregar specialties[], validar Province vs State | ✅ Necesarios    |
| Publish page                 | 🟡 MEDIO   | Bajo     | Cambios mínimos (solo validación)                | ✅ Mínimos       |
| Dealers components           | 🟢 BAJO    | Muy bajo | Probablemente no afectados                       | ❌ No necesarios |

---

## 📝 Checklist de Validación por Componente

### **Antes de cambios:**

- [ ] Backup de datos actuales
- [ ] Documentación de flujos actuales
- [ ] Tests baseline (cobertura actual)

### **Cambios en ProfileStep:**

- [ ] Remover input phone ✓
- [ ] Agregar inputs: city (dropdown), state (dropdown), address, zipCode
- [ ] Actualizar validaciones Zod
- [ ] Tests de ProfileStep
- [ ] Manual test: registro completo

### **Cambios en ProfilePage:**

- [ ] Actualizar sync logic (location string → 4 campos)
- [ ] Mostrar 4 inputs en lugar de 1
- [ ] Tests de form sync
- [ ] Tests de submit
- [ ] Manual test: editar perfil

### **Cambios en Backend DTOs:**

- [ ] CreateSellerProfileRequest: agregar city, state, address?, zipCode?, specialties?
- [ ] UpdateSellerProfileRequest: mismo
- [ ] SellerProfileDto: agregar specialties[]
- [ ] SellerPublicProfileDto: agregar specialties[], validar Province
- [ ] Tests de DTOs

### **Cambios en Handlers:**

- [ ] CreateSellerProfileHandler: nuevo mapeo
- [ ] UpdateSellerProfileHandler: nuevo mapeo
- [ ] GetSellerProfileQueryHandler: incluir specialties
- [ ] Tests de handlers (unit tests)

### **Cambios en Entity:**

- [ ] Agregar Specialties property
- [ ] Migration script
- [ ] Tests de entity

### **Cambios en Componentes:**

- [ ] seller-card: validar city
- [ ] Dashboard: validar displayName
- [ ] Tests de componentes

### **Integration Tests:**

- [ ] E2E: registro → perfil → publicar
- [ ] API: crear → leer → actualizar
- [ ] RabbitMQ: evento incluye especialidades

### **Manual QA:**

- [ ] Registrar vendedor (nuevo)
- [ ] Editar perfil (existente)
- [ ] Ver perfil público
- [ ] Publicar vehículo
- [ ] Buscar vendedores (si existe)

---

## 🎯 Conclusión

**Total de componentes afectados:** 20+
**Criticidad promedio:** 🔴 ALTA
**Tiempo estimado:** 2-3 sprints
**Riesgo de datos:** MEDIO (con buena planificación, bajo)

**RECOMENDACIÓN:** Implementar en fases pequeñas, NO todo de una vez.
