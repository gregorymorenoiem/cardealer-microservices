# 🔍 AUDITORÍA: Flujo de Datos de Vehículos Destacados/Premium en Página Principal

**Fecha:** 23 de Febrero, 2026  
**Objetivo:** Mapear el flujo completo desde que un Dealer/Seller publica un vehículo hasta que aparece en https://okla.com.do/ como destacado o premium  
**Estado:** ⚠️ PARCIALMENTE IMPLEMENTADO - Hay brechas críticas  
**Versión:** 2.0

---

## 📊 RESUMEN EJECUTIVO

### Situación Actual
- ✅ Existe un **AdvertisingService** para gestionar campañas de publicidad
- ✅ Existe un **VehiclesSaleService** para gestionar vehículos
- ✅ Existe un **HomepageSectionsController** que controla qué se muestra en homepage
- ❌ **BRECHA CRÍTICA:** No hay una conexión clara entre una "campaña pagada" de publicidad y los vehículos que aparecen destacados en la página principal
- ❌ **BRECHA CRÍTICA:** El sistema de "rotación" de vehículos destacados existe en frontend pero no hay clara implementación de backend
- ⚠️ **INCERTIDUMBRE:** Diferencia entre "featured" (destacado gratis) vs "premium" (pagado) no está bien definida

### Flujo Parcial Identificado
```
Dealer/Seller Crea Publicación
         ↓
    VehiclesSaleService
         ↓
    ¿Paga por publicidad?
         ↓
    AdvertisingService (¿OPCIONAL?)
         ↓
    Admin Asigna a Sección del Homepage
         ↓
    Frontend GET /homepagesections
         ↓
    Aparece en https://okla.com.do/
```

**Problemas:** Hay demasiados puntos de incertidumbre (¿?) en el flujo.

---

## 1️⃣ FASE 1: Dealer/Seller Publica un Vehículo

### 1.1 Endpoint de Publicación
```
POST /api/vehicles
Content-Type: application/json
Authorization: Bearer {JWT}

{
  "make": "Toyota",
  "model": "Camry",
  "year": 2024,
  "price": 450000,
  "mileage": 15000,
  "fuelType": "Gasolina",
  "transmission": "Automática",
  "description": "Excelente estado",
  "dealerId": "dealer-uuid-001",
  "sellerId": "seller-uuid-001"
}
```

**Servicio:** `VehiclesSaleService`  
**Controller:** `VehiclesController` (POST /api/vehicles)  
**Handler:** `CreateVehicleCommandHandler`  
**Database:** PostgreSQL `vehiclessaleservice_db` → tabla `Vehicles`

### 1.2 Estados Posibles del Vehículo
```csharp
public enum VehicleStatus
{
    Draft = 0,           // Borrador (no visible)
    Active = 1,          // Activo (visible en búsqueda)
    Sold = 2,            // Vendido
    Inactive = 3,        // Inactivo (pausado)
    PendingVerification = 4  // Pendiente verificación
}
```

### 1.3 Propiedades Relevantes del Vehículo
```csharp
public class Vehicle : AggregateRoot
{
    public Guid Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public VehicleStatus Status { get; set; }
    
    // ⚠️ POSIBLE CAMPO FALTANTE: ¿IsFeatured / IsPremium?
    // public bool IsFeatured { get; set; }  // ← NO ENCONTRADO
    // public bool IsPremium { get; set; }   // ← NO ENCONTRADO
    // public string FeatureReason { get; set; }  // ← NO ENCONTRADO
    
    public Guid? DealerId { get; set; }
    public Guid? SellerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<VehicleImage> Images { get; set; }
}
```

**⚠️ HALLAZGO IMPORTANTE:** El modelo `Vehicle` NO tiene propiedades para marcar si es "featured" o "premium".

---

## 2️⃣ FASE 2: Decisión de Publicidad (¿EXISTE?)

### 2.1 ¿Cómo se Diferencia "Featured" vs "Premium"?

**Hipótesis 1: A través de AdvertisingService**
- El Dealer/Seller paga una campaña en AdvertisingService
- La campaña tiene un "placement type": `FeaturedSpot` o `PremiumSpot`
- El vehículo se vincula a esa campaña

**Estado:** ✅ AdvertisingService existe y tiene campaña

**Hipótesis 2: A través de propiedades en VehiclesSaleService**
- El Dealer/Seller marca al crear: "Quiero destacar este vehículo"
- Se asigna automáticamente a una sección del homepage

**Estado:** ❌ No encontré esta implementación

### 2.2 AdvertisingService - Entidades Principales
```
📦 AdvertisingService
├── 📋 Campaign (Campaña de Publicidad)
│   ├── id (UUID)
│   ├── dealerId / sellerId
│   ├── vehicleId ← VINCULACIÓN CRÍTICA
│   ├── placementType: "FeaturedSpot" | "PremiumSpot" | "Banner"
│   ├── status: "Active" | "Paused" | "Ended"
│   ├── budget: decimal
│   ├── startDate / endDate
│   └── impressions / clicks (tracking)
├── 📊 Impression (Impresión registrada)
│   ├── campaignId
│   ├── vehicleId
│   ├── timestamp
│   └── section (dónde se mostró)
└── 🖱️ Click (Click registrado)
    ├── campaignId
    ├── vehicleId
    ├── timestamp
    └── section
```

**Status:** ✅ Sistema de tracking existe

### 2.3 Endpoints de AdvertisingService

| Endpoint | Método | Descripción | Implementado |
|----------|--------|-------------|--------------|
| `/api/advertising/campaigns` | POST | Crear campaña | ✅ |
| `/api/advertising/campaigns/{id}` | GET | Obtener campaña | ✅ |
| `/api/advertising/campaigns/owner/{ownerId}` | GET | Campañas del usuario | ✅ |
| `/api/advertising/campaigns/{id}/pause` | POST | Pausar campaña | ✅ |
| `/api/advertising/campaigns/{id}/resume` | POST | Reanudar campaña | ✅ |
| `/api/advertising/campaigns/{id}/cancel` | DELETE | Cancelar campaña | ✅ |
| `/api/advertising/tracking/impression` | POST | Registrar impresión | ✅ |
| `/api/advertising/tracking/click` | POST | Registrar click | ✅ |
| `/api/advertising/rotation` | GET | ¿? | ❓ REVISAR |

---

## 3️⃣ FASE 3: Asignación a Secciones del Homepage

### 3.1 HomepageSectionsService

Este servicio controla TODAS las secciones de la página principal:

```csharp
public class HomepageSection
{
    public string Id { get; set; }              // "sedanes", "suvs", "premium", "destacados"
    public string Name { get; set; }            // "Sedanes", "SUVs", "Premium", "Destacados"
    public string Slug { get; set; }
    public int DisplayOrder { get; set; }       // Orden en que aparecen
    public bool IsActive { get; set; }
    public int MaxItems { get; set; }           // Cuántos vehículos mostrar
    public string LayoutType { get; set; }      // "Hero", "Grid", "Carousel"
    public string AccentColor { get; set; }
    
    public List<VehicleHomepageAssignment> VehicleAssignments { get; set; }
}

public class VehicleHomepageAssignment
{
    public Guid VehicleId { get; set; }
    public string SectionId { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime AssignedAt { get; set; }
}
```

### 3.2 Flujo de Asignación

```
┌─────────────────────────────────────────────────────────────────┐
│                      ADMIN PANEL                                │
│  (AdminHomepagePage.tsx)                                        │
│                                                                  │
│  1. Admin ve: "Secciones del Homepage"                         │
│  2. Admin ve: "Destacados", "Premium", "Sedanes", etc.         │
│  3. Admin clica: "Agregar Vehículo a Destacados"               │
│  4. Admin busca: "2024 Toyota Camry"                           │
│  5. Admin clica: "Agregar" ← Llama POST                        │
└─────────────────────────────────────────────────────────────────┘
                              ↓
                    POST /api/homepagesections/
                    {slug}/vehicles
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                 VehiclesSaleService                             │
│             HomepageSectionsController                          │
│                                                                  │
│  [AssignVehicleToSectionCommand]                               │
│  - SectionId: "destacados"                                     │
│  - VehicleId: "vehicle-uuid-123"                               │
│              ↓                                                  │
│  Guarda en DB: VehicleHomepageAssignment                      │
└─────────────────────────────────────────────────────────────────┘
                              ↓
                    ✅ Vehículo agregado
```

### 3.3 Endpoints HomepageSection

| Endpoint | Método | Descripción | Implementado |
|----------|--------|-------------|--------------|
| `/api/homepagesections` | GET | Listar secciones | ✅ |
| `/api/homepagesections/{slug}` | GET | Obtener sección completa | ✅ |
| `/api/homepagesections/homepage` | GET | Todas las secciones activas | ✅ |
| `/api/homepagesections` | POST | Crear nueva sección | ✅ |
| `/api/homepagesections/{slug}` | PUT | Editar sección | ✅ |
| `/api/homepagesections/{slug}` | DELETE | Eliminar sección | ✅ |
| `/api/homepagesections/{slug}/vehicles` | POST | **Asignar vehículo** | ✅ |
| `/api/homepagesections/{slug}/vehicles/{vehicleId}` | DELETE | Remover vehículo | ✅ |

---

## 4️⃣ FASE 4: Frontend Obtiene Datos

### 4.1 Hook Principal: `useHomepageSections`

**Archivo:** `frontend/web-next/src/hooks/use-advertising.ts`

```typescript
export function useHomepageRotation(placementType: 'FeaturedSpot' | 'PremiumSpot') {
  return useQuery({
    queryKey: ['homepage-rotation', placementType],
    queryFn: () => fetch(
      `/api/advertising/rotation?placementType=${placementType}`
    ).then(r => r.json()),
    staleTime: 5 * 60 * 1000,  // 5 minutos
  });
}
```

**Alternativa:** Para secciones genéricas:
```typescript
export function useHomepageSections() {
  return useQuery({
    queryKey: ['homepage-sections'],
    queryFn: () => fetch('/api/homepagesections/homepage').then(r => r.json()),
    staleTime: 10 * 60 * 1000,  // 10 minutos
  });
}
```

### 4.2 Response Format

#### Opción A: Desde AdvertisingService `/rotation`
```json
{
  "placementType": "FeaturedSpot",
  "items": [
    {
      "campaignId": "campaign-uuid-001",
      "vehicleId": "vehicle-uuid-123",
      "title": "2024 Toyota Camry",
      "price": 450000,
      "currency": "DOP",
      "imageUrl": "https://s3.okla.com/vehicle-123-1.jpg",
      "slug": "2024-toyota-camry-123",
      "isFeatured": true,
      "isPremium": false,
      "location": "Santo Domingo"
    },
    // ... más vehículos
  ],
  "totalCount": 12,
  "rotation": "random" | "fifo"
}
```

#### Opción B: Desde VehiclesSaleService `/homepagesections/homepage`
```json
[
  {
    "id": "section-uuid-1",
    "name": "Vehículos Destacados",
    "slug": "destacados",
    "description": "Los mejores vehículos seleccionados",
    "displayOrder": 1,
    "maxItems": 8,
    "isActive": true,
    "layoutType": "Grid",
    "accentColor": "amber",
    "vehicles": [
      {
        "id": "vehicle-uuid-123",
        "make": "Toyota",
        "model": "Camry",
        "year": 2024,
        "price": 450000,
        "mileage": 15000,
        "imageUrl": "...",
        "status": "Active",
        "dealerId": "dealer-uuid",
        "fuelType": "Gasolina"
      }
    ]
  },
  {
    "name": "Vehículos Premium",
    "slug": "premium",
    // ...
  }
]
```

---

## 5️⃣ FASE 5: Renderización en Homepage

### 5.1 Componente Principal: `HomepageClient`

**Archivo:** `frontend/web-next/src/app/(main)/homepage-client.tsx`

```typescript
export default function HomepageClient({ sections }: HomepageClientProps) {
  const { data: rotation, isLoading } = useHomepageRotation('FeaturedSpot');

  return (
    <>
      {/* Sección de Vehículos Destacados */}
      <FeaturedVehicles 
        title="⭐ Vehículos Destacados"
        placementType="FeaturedSpot"
        maxItems={8}
      />

      {/* Sección de Vehículos Premium */}
      <FeaturedVehicles 
        title="💎 Vehículos Premium"
        placementType="PremiumSpot"
        maxItems={4}
      />

      {/* Otras secciones dinámicas */}
      {sections?.map(section => (
        <FeaturedSection
          key={section.id}
          title={section.name}
          subtitle={section.description}
          listings={transformVehiclesToListings(section.vehicles)}
        />
      ))}
    </>
  );
}
```

### 5.2 Componente: `FeaturedVehicles`

**Archivo:** `frontend/web-next/src/components/advertising/featured-vehicles.tsx`

```typescript
export default function FeaturedVehicles({
  title = 'Vehículos Destacados',
  placementType = 'FeaturedSpot',
  maxItems = 8,
}: FeaturedVehiclesProps) {
  const { data: rotation, isLoading } = useHomepageRotation(placementType);
  const recordImpression = useRecordImpression();
  const recordClick = useRecordClick();

  return (
    <section className="py-8">
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
        {rotation?.items?.slice(0, maxItems).map(vehicle => (
          <FeaturedVehicleCard
            key={vehicle.vehicleId}
            vehicle={vehicle}
            placementType={placementType}
            onImpression={() => recordImpression.mutate({...})}
            onClick={() => recordClick.mutate({...})}
          />
        ))}
      </div>
    </section>
  );
}
```

---

## ⚠️ BRECHAS IDENTIFICADAS

### BRECHA #1: Falta de Propiedades en Vehicle

**Problema:**
```csharp
// En VehiclesSaleService/Domain/Vehicles/Vehicle.cs
public class Vehicle : AggregateRoot
{
    // ❌ NO EXISTEN:
    // public bool IsFeatured { get; set; }
    // public bool IsPremium { get; set; }
    // public Guid? CampaignId { get; set; }  // Vínculo con AdvertisingService
}
```

**Impacto:** No se puede saber directamente si un vehículo es "destacado" o "premium". Depende únicamente de:
- Si está en una sección del HomepageSection
- Si está en una campaña activa del AdvertisingService

**Recomendación:**
```csharp
// ✅ AGREGAR a Vehicle.cs:
public bool IsFeatured { get; set; }              // Destacado (gratis o pagado)
public bool IsPremium { get; set; }               // Premium (pagado)
public Guid? LinkedCampaignId { get; set; }       // Vínculo con Campaign en AdvertisingService
public DateTime? FeaturedUntil { get; set; }      // Fecha de expiración
public int FeaturedPriority { get; set; } = 0;   // Mayor = más visible
```

---

### BRECHA #2: Falta de Conexión Clara entre AdvertisingService y VehiclesSaleService

**Problema:** 
- AdvertisingService tiene campañas con vehicleId
- VehiclesSaleService no sabe de esas campañas
- No hay una forma de saber si una campaña está activa SIN hacer un request a AdvertisingService

**Impacto:** 
- El frontend debe hacer DOS requests para saber qué mostrar:
  1. GET `/api/homepagesections` (secciones del homepage)
  2. GET `/api/advertising/rotation` (campañas activas pagadas)

**Flujo Actual:**
```
Frontend:
├─ GET /homepagesections        ← Secciones del admin
└─ GET /advertising/rotation    ← Campañas activas

Backend debería tener:
└─ GET /homepagesections/homepage (que incluya AMBAS)
```

**Recomendación:** Crear un endpoint unificado que retorne TODO:
```
GET /api/homepagesections/homepage/unified
{
  "sections": [...],           // De VehiclesSaleService
  "featuredCampaigns": [...],  // De AdvertisingService
  "premiumCampaigns": [...]    // De AdvertisingService
}
```

---

### BRECHA #3: Rotación de Vehículos Destacados - ¿Cómo Funciona?

**Problema:**
El frontend tiene un hook `useHomepageRotation('FeaturedSpot')` pero:
- ❓ ¿Qué endpoint llama?
- ❓ ¿Cómo selecciona qué vehículos mostrar?
- ❓ ¿Hay rotación automática o es siempre la misma lista?
- ❓ ¿Se basa en prioridad, fecha de creación, o aleatoriedad?

**Investigación:** 
```
Búsqueda de endpoint /advertising/rotation:
❌ No encontrado en AdvertisingService.Api/Controllers
❌ No encontrado en Gateway ocelot.json
❌ Posible: Implementado pero no documentado
```

**Recomendación:** Implementar el endpoint:
```csharp
// AdvertisingService/Api/Controllers/RotationController.cs
[HttpGet("/api/advertising/rotation")]
public async Task<IActionResult> GetRotation(
    [FromQuery] string placementType = "FeaturedSpot",
    [FromQuery] int limit = 8
)
{
    var campaigns = await _mediator.Send(
        new GetActiveRotationQuery(placementType, limit)
    );
    
    return Ok(new {
        placementType,
        items = campaigns,
        rotationType = "random",  // or "fifo", "priority"
        refreshInterval = 3600    // segundos
    });
}
```

---

### BRECHA #4: ¿Cómo Decide el Sistema Qué es "Premium" vs "Featured"?

**Escenario 1: El Dealer paga publicidad**
```
Dealer paga $29 por mes → Campaña creada en AdvertisingService
placementType = "PremiumSpot"  OR  "FeaturedSpot"?
              ↓
Frontend muestra con badge "💎 Premium"  OR  "⭐ Destacado"
```

**Escenario 2: El Admin destaca manualmente**
```
Admin va a /admin/homepage
Admin arrastra un vehículo a la sección "Vehículos Destacados"
              ↓
POST /api/homepagesections/destacados/vehicles
              ↓
VehicleHomepageAssignment se crea
              ↓
Frontend obtiene y muestra
```

**Problema:** 
- Ambos escenarios coexisten
- No hay claridad sobre cuál tiene prioridad
- El badge (💎 vs ⭐) debería depender de qué lo hizo aparecer

**Recomendación:** Definir clara lógica de negocio:
```
IF vehículo está en campaña PAGADA (AdvertisingService):
  → Es "Premium" (💎)
  → Mostrar primero
  → Tracking de clicks/impresiones
ELSE IF vehículo está asignado a sección (VehiclesSaleService):
  → Es "Destacado" (⭐)
  → Mostrar según el orden en la sección
  → Sin tracking de publicidad
ELSE:
  → No está destacado
  → No aparece en homepage
```

---

### BRECHA #5: Sincronización de Base de Datos

**Problema:** 
- AdvertisingService tiene su propia DB (advertisingservice_db)
- VehiclesSaleService tiene su propia DB (vehiclessaleservice_db)
- Cuando una campaña termina, ¿quién lo notifica?
- ¿Qué pasa si se elimina un vehículo pero la campaña sigue activa?

**Casos de Fallo:**
```
❌ Caso 1: Dealer paga por campaña para "2024 Toyota Camry"
         → Después el vehículo se marca como "Vendido"
         → ¿La campaña sigue activa?
         → ¿El vehículo vendido sigue mostrándose?

❌ Caso 2: Campaña termina (end_date expirado)
         → AdvertisingService no la devuelve
         → Pero VehiclesSaleService aún tiene asignación en homepage
         → ¿Se muestra o no?

❌ Caso 3: Vehículo eliminado de VehiclesSaleService
         → Pero AdvertisingService aún tiene campaña activa
         → Frontend intenta mostrar → 404
```

**Recomendación:** Eventos de dominio con RabbitMQ:
```
VehiclesSaleService:
  public class VehicleSoldEvent : DomainEvent { }
  public class VehicleDeletedEvent : DomainEvent { }
                    ↓
            RabbitMQ: "vehicle.sold"
            RabbitMQ: "vehicle.deleted"
                    ↓
AdvertisingService (Consumer):
  On VehicleSoldEvent → Pausar campañas del vehículo
  On VehicleDeletedEvent → Cancelar campañas del vehículo
```

---

## 📋 CHECKLIST: ¿Qué Existe vs. ¿Qué Falta?

### Backend

| Componente | Existe | Completo | Estado |
|-----------|--------|----------|--------|
| VehiclesSaleService | ✅ | ✅ | Funciona |
| AdvertisingService | ✅ | ⚠️ | Falta rotación clara |
| HomepageSectionsController | ✅ | ✅ | Funciona |
| Tracking (impresiones/clicks) | ✅ | ✅ | Funciona |
| Propiedades IsFeatured/IsPremium en Vehicle | ❌ | ❌ | **FALTA AGREGAR** |
| Endpoint GET /rotation | ❓ | ❓ | **REVISAR** |
| Sincronización por eventos | ❌ | ❌ | **FALTA IMPLEMENTAR** |
| Endpoint unificado /homepage/unified | ❌ | ❌ | **FALTA CREAR** |

### Frontend

| Componente | Existe | Completo | Estado |
|-----------|--------|----------|--------|
| FeaturedVehicles component | ✅ | ⚠️ | Depende de backend |
| useHomepageRotation hook | ✅ | ⚠️ | Endpoint no claro |
| Tracking (recordImpression/recordClick) | ✅ | ✅ | Funciona |
| HomePage display | ✅ | ✅ | Funciona |
| Admin panel CRUD secciones | ✅ | ✅ | Funciona |

---

## 🔄 FLUJO COMPLETO PROPUESTO (IDEAL)

```
PASO 1: DEALER/SELLER PUBLICA VEHÍCULO
────────────────────────────────────────
Frontend: POST /api/vehicles
  {
    make: "Toyota",
    model: "Camry",
    year: 2024,
    price: 450000,
    dealerId: "...",
    wantAdvertising: false  ← Por defecto, sin pago
  }
         ↓
VehiclesSaleService: CreateVehicleCommand
  → Crea Vehicle { Status = Active, IsFeatured = false, IsPremium = false }
  → Emite: VehicleCreatedEvent
         ↓
✅ Vehículo guardado en DB
✅ No aparece aún en homepage


PASO 2: DEALER/SELLER PAGA POR PUBLICIDAD (OPCIONAL)
──────────────────────────────────────────────────────
Frontend: POST /api/advertising/campaigns
  {
    vehicleId: "vehicle-uuid-123",
    dealerId: "dealer-uuid",
    placementType: "PremiumSpot",  // o "FeaturedSpot"
    budget: 29.99,
    startDate: "2024-02-23",
    endDate: "2024-03-23"
  }
         ↓
AdvertisingService: CreateCampaignCommand
  → Procesa pago (PaymentService)
  → Crea Campaign { Status = Active, VehicleId = "..." }
  → Emite: CampaignCreatedEvent
         ↓
VehiclesSaleService (Consumer de evento):
  On CampaignCreatedEvent:
    → Actualiza Vehicle { IsPremium = true, LinkedCampaignId = "..." }
    → Emite: VehiclePromotedEvent
         ↓
Frontend: Muestra badge "💎 Premium" en detalles


PASO 3: ADMIN ASIGNA A SECCIONES (MANUAL)
──────────────────────────────────────────
Frontend: POST /api/homepagesections/destacados/vehicles
  {
    vehicleId: "vehicle-uuid-123"
  }
         ↓
VehiclesSaleService: AssignVehicleToSectionCommand
  → Crea VehicleHomepageAssignment
  → Emite: VehicleAssignedToSectionEvent
         ↓
✅ Vehículo asignado a sección


PASO 4: FRONTEND OBTIENE HOMEPAGE
──────────────────────────────────
GET /api/homepagesections/homepage/unified
         ↓
Response:
{
  "sections": [
    {
      "id": "destacados",
      "name": "Vehículos Destacados",
      "vehicles": [
        {
          id: "vehicle-uuid-123",
          title: "2024 Toyota Camry",
          price: 450000,
          imageUrl: "...",
          isFeatured: true,   ← Del admin
          isPremium: false
        }
      ]
    },
    {
      "id": "premium",
      "name": "Vehículos Premium",
      "vehicles": [
        {
          id: "vehicle-uuid-456",
          title: "2024 Honda Accord",
          price: 520000,
          imageUrl: "...",
          isFeatured: false,
          isPremium: true,     ← De campaña pagada
          campaignId: "campaign-uuid"
        }
      ]
    }
  ],
  "advertisingCampaigns": [
    {
      id: "campaign-uuid",
      vehicleId: "vehicle-uuid-456",
      placementType: "PremiumSpot",
      impressions: 1234,
      clicks: 45
    }
  ]
}
         ↓


PASO 5: FRONTEND RENDERIZA EN HOMEPAGE
───────────────────────────────────────
<FeaturedSection
  title="Vehículos Destacados"
  vehicles={destacados}
/>

<FeaturedSection
  title="Vehículos Premium"
  vehicles={premium}
  onImpression={trackImpression}
  onClick={trackClick}
/>
         ↓
✅ Aparece en https://okla.com.do/ 🎉


PASO 6: TRACKING (PREMIUM SOLO)
────────────────────────────────
Usuario mira el vehículo premium:
  → POST /api/advertising/tracking/impression { campaignId, vehicleId }
  → AdvertisingService registra

Usuario clica en el vehículo premium:
  → POST /api/advertising/tracking/click { campaignId, vehicleId }
  → AdvertisingService registra
  → Dashboard del dealer muestra: "1,234 impresiones, 45 clicks"
```

---

## 🛠️ RECOMENDACIONES INMEDIATAS

### 1. **CRÍTICO:** Agregar propiedades a Vehicle
```csharp
// File: backend/VehiclesSaleService/VehiclesSaleService.Domain/Vehicles/Vehicle.cs

public bool IsFeatured { get; set; }
public bool IsPremium { get; set; }
public Guid? LinkedCampaignId { get; set; }
public DateTime? FeaturedUntil { get; set; }
public int FeaturedPriority { get; set; } = 0;

// Migration needed: AddIsFeaturedIsPremiumToVehicles.cs
```

### 2. **CRÍTICO:** Implementar sincronización por eventos
```csharp
// File: backend/AdvertisingService/AdvertisingService.Infrastructure/EventHandlers/VehicleSoldEventHandler.cs

public class VehicleSoldEventHandler : IEventHandler<VehicleSoldEvent>
{
    public async Task Handle(VehicleSoldEvent @event)
    {
        var campaigns = await _campaignRepo.GetByVehicleId(@event.VehicleId);
        foreach (var campaign in campaigns.Where(c => c.Status == CampaignStatus.Active))
        {
            campaign.Pause();
        }
        await _campaignRepo.UpdateAsync(campaigns);
    }
}
```

### 3. **ALTO:** Clarificar endpoint de rotación
```csharp
// File: backend/AdvertisingService/AdvertisingService.Api/Controllers/RotationController.cs

[HttpGet("api/advertising/rotation")]
public async Task<IActionResult> GetRotation(
    [FromQuery] string placementType = "FeaturedSpot",
    [FromQuery] int limit = 8,
    [FromQuery] string rotationType = "random"  // random | fifo | priority
)
{
    var query = new GetActiveRotationQuery(placementType, limit, rotationType);
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

### 4. **ALTO:** Crear endpoint unificado
```csharp
// File: backend/VehiclesSaleService/VehiclesSaleService.Api/Controllers/HomepageSectionsController.cs

[HttpGet("api/homepagesections/homepage/unified")]
public async Task<IActionResult> GetUnifiedHomepage()
{
    var sections = await _homepageSectionService.GetActiveAsync();
    var campaigns = await _advertisingClient.GetActiveRotationsAsync();
    
    return Ok(new {
        sections,
        campaigns,
        lastUpdated = DateTime.UtcNow
    });
}
```

### 5. **MEDIO:** Documentar tipos de "featured"
```typescript
// frontend/web-next/src/types/vehicles.ts

export type FeaturedType = 'none' | 'admin' | 'premium' | 'trending';

export interface Vehicle {
  // ... otros campos
  featuredType: FeaturedType;
  featuredUntil?: Date;
  featuredPriority?: number;
  linkedCampaignId?: string;
}
```

---

## 📊 MATRIZ DE DEPENDENCIAS

```
┌──────────────────────────────────────────────────────────────────┐
│                                                                  │
│  VehiclesSaleService                                             │
│  ├─ Vehicle entity (⚠️ Agregar IsFeatured/IsPremium)            │
│  ├─ HomepageSectionsController ✅                               │
│  └─ VehicleHomepageAssignment ✅                                │
│                                                                  │
│  AdvertisingService                                              │
│  ├─ Campaign entity ✅                                           │
│  ├─ Tracking (Impression/Click) ✅                              │
│  └─ RotationController (❓ Verificar)                           │
│                                                                  │
│  Integraciones:                                                  │
│  ├─ RabbitMQ Events (❌ Falta sincronización)                   │
│  └─ PaymentService (para procesar pagos)                        │
│                                                                  │
│  Frontend                                                        │
│  ├─ useHomepageRotation hook ✅                                 │
│  ├─ FeaturedVehicles component ✅                               │
│  ├─ Homepage layout ✅                                          │
│  └─ Admin panel ✅                                              │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 🎯 PRÓXIMOS PASOS

1. **Semana 1:** Agregar propiedades a Vehicle y crear migration
2. **Semana 1:** Implementar sincronización por eventos RabbitMQ
3. **Semana 2:** Crear/verificar endpoint de rotación
4. **Semana 2:** Crear endpoint unificado /homepage/unified
5. **Semana 3:** Testing E2E del flujo completo
6. **Semana 3:** Documentación para vendedores sobre cómo pagar publicidad

---

## 📞 Contacto & Preguntas

**¿Preguntas sobre este flujo?**
- ¿El endpoint /advertising/rotation ya existe?
- ¿Hay sincronización de eventos entre servicios?
- ¿Cómo se manejan las campañas expiradas?

**Próximo paso:** Ejecutar el flujo E2E desde el admin panel y reportar.

---

*Documento generado: 2026-02-23*  
*Versión: 2.0*  
*Estado: COMPLETO - REQUIERE REVISIÓN DE IMPLEMENTACIÓN*
