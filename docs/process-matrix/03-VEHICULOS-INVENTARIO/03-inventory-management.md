# 📦 InventoryManagementService - Matriz de Procesos

> **Servicio:** InventoryManagementService  
> **Puerto:** 5040 (docker: 15149)  
> **Última actualización:** Enero 28, 2026  
> **Estado de Implementación:** ✅ Backend 100% | ✅ UI 100% | ✅ Gateway 100%

---

## ✅ AUDITORÍA DE ACCESO UI (Enero 28, 2026) - COMPLETADA

> **Estado:** ✅ SERVICIO 100% OPERATIVO - Todas las rutas configuradas.

### ✅ PROBLEMA RESUELTO: Gateway ahora tiene todas las rutas de InventoryManagementService

**Se agregaron las siguientes rutas en `ocelot.prod.json` y `ocelot.dev.json`:**

- `/api/inventory` - Listar inventario
- `/api/inventory/stats` - Estadísticas de inventario
- `/api/inventory/featured` - Items destacados
- `/api/inventory/hot` - Items calientes (alta actividad)
- `/api/inventory/overdue` - Items vencidos (90+ días)
- `/api/inventory/bulk/status` - Actualización masiva de estados
- `/api/inventory/bulkimport/upload` - Carga de archivos CSV
- `/api/inventory/bulkimport/{everything}` - Gestión de jobs de importación
- `/api/inventory/{id}` - CRUD individual de items

### Estado Detallado por Componente

| Componente           | Estado  | Descripción                                                           |
| -------------------- | ------- | --------------------------------------------------------------------- |
| **Backend Service**  | ✅ 100% | InventoryController + BulkImportController implementados              |
| **Domain Entities**  | ✅ 100% | InventoryItem, BulkImportJob con métodos completos                    |
| **CQRS Commands**    | ✅ 100% | 5 Commands: Create, Update, Delete, BulkUpdate, StartBulkImport       |
| **CQRS Queries**     | ✅ 100% | 7 Queries: GetItems, GetById, Stats, Featured, Hot, Overdue, BulkJobs |
| **DTOs**             | ✅ 100% | InventoryItemDto, BulkImportJobDto, ImportErrorDto, etc.              |
| **Repositories**     | ✅ 100% | InventoryItemRepository, BulkImportJobRepository                      |
| **Tests**            | ✅ 100% | 15+ tests unitarios pasando                                           |
| **Docker Compose**   | ✅ 100% | Servicio en puerto 15149, configuración completa                      |
| **Gateway (Ocelot)** | ✅ 100% | **RUTAS AGREGADAS - ocelot.prod.json y ocelot.dev.json**              |
| **Frontend Service** | ✅ 100% | inventoryManagementService.ts con todos los métodos                   |
| **UI Pages**         | ✅ 100% | Todas las páginas conectadas correctamente                            |

### Procesos por Estado de Implementación

| Proceso            | Backend | Gateway | UI Access | Ruta UI                      | Observación                           |
| ------------------ | ------- | ------- | --------- | ---------------------------- | ------------------------------------- |
| Lista inventario   | ✅ 100% | ✅ 100% | ✅ 100%   | `/dealer/inventory`          | Conectado a InventoryManagement       |
| Crear vehículo     | ✅ 100% | ✅ 100% | ✅ 100%   | `/dealer/inventory/new`      | Conectado a VehiclesSaleService       |
| Editar vehículo    | ✅ 100% | ✅ 100% | ✅ 100%   | `/dealer/inventory/:id/edit` | Conectado a VehiclesSaleService       |
| Import CSV         | ✅ 100% | ✅ 100% | ✅ 100%   | `/dealer/import`             | Conectado a inventoryManagement       |
| Bulk Status Update | ✅ 100% | ✅ 100% | ✅ 100%   | N/A                          | Endpoint `/api/inventory/bulk/status` |
| Stats inventario   | ✅ 100% | ✅ 100% | ✅ 100%   | `/dealer/analytics`          | Conectado a InventoryManagement       |
| Featured items     | ✅ 100% | ✅ 100% | ✅ 100%   | N/A                          | Endpoint configurado                  |
| Hot items          | ✅ 100% | ✅ 100% | ✅ 100%   | N/A                          | Endpoint configurado                  |
| Overdue items      | ✅ 100% | ✅ 100% | ✅ 100%   | N/A                          | Endpoint configurado                  |
| Ubicaciones        | ✅ 100% | ✅ 100% | ✅ 100%   | `/dealer/locations`          | Backend + UI + Gateway completos      |

### Rutas UI Existentes - TODAS FUNCIONALES

- ✅ `/dealer/inventory` → `DealerInventoryPage.tsx`
- ✅ `/dealer/inventory/new` → `DealerAddVehiclePage.tsx`
- ✅ `/dealer/inventory/:id/edit` → `DealerVehicleEditPage.tsx`
- ✅ `/dealer/analytics` → `AnalyticsPage.tsx`
- ✅ `/dealer/import` → `CSVImportPage.tsx` (usa inventoryManagementService.ts)
- ✅ `/dealer/locations` → `LocationsPage.tsx` (conectado a DealerManagementService)

---

## 📊 Resumen de Implementación (ACTUALIZADO Enero 28, 2026)

| Componente         | Total | Implementado | Pendiente | Estado  |
| ------------------ | ----- | ------------ | --------- | ------- |
| Controllers        | 2     | 2            | 0         | ✅ 100% |
| INV-ITEM-\*        | 6     | 6            | 0         | ✅ 100% |
| INV-BULK-\*        | 4     | 4            | 0         | ✅ 100% |
| INV-STAT-\*        | 5     | 5            | 0         | ✅ 100% |
| INV-LOC-\*         | 3     | 3            | 0         | ✅ 100% |
| Tests              | 15    | 15           | 0         | ✅ 100% |
| **Gateway Routes** | 12    | 12           | 0         | ✅ 100% |
| **UI Integration** | 6     | 6            | 0         | ✅ 100% |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 📋 Información General

| Aspecto           | Detalle                                                                                                                                               |
| ----------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Servicio**      | InventoryManagementService                                                                                                                            |
| **Puerto Docker** | 15149 (interno: 80)                                                                                                                                   |
| **Puerto K8s**    | 8080                                                                                                                                                  |
| **Base de Datos** | PostgreSQL (inventorymanagementservice)                                                                                                               |
| **Tecnología**    | .NET 8, MediatR, Entity Framework Core                                                                                                                |
| **Mensajería**    | RabbitMQ                                                                                                                                              |
| **Descripción**   | Gestión del inventario de vehículos para dealers: precios de costo, ubicación física, días en mercado, métricas de rendimiento y operaciones en batch |

### Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                 InventoryManagementService Architecture                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   Dealer Dashboard                   Core Service                            │
│   ┌────────────────┐                ┌────────────────────────────────┐      │
│   │ Inventory List │──┐             │    InventoryManagementService    │      │
│   │ (Grid View)    │  │             │  ┌──────────────────────────┐   │      │
│   └────────────────┘  │             │  │ Controllers              │   │      │
│   ┌────────────────┐  │             │  │ • InventoryController     │   │      │
│   │ Bulk Actions   │──┼────────────▶│  │ • BulkOperationsCtrl      │   │      │
│   │ (CSV Import)   │  │             │  └──────────────────────────┘   │      │
│   └────────────────┘  │             │  ┌──────────────────────────┐   │      │
│   ┌────────────────┐  │             │  │ Application (CQRS)       │   │      │
│   │ Stats Panel    │──┘             │  │ • CreateInventoryItemCmd  │   │      │
│   │ (KPIs)         │               │  │ • BulkUpdateStatusCmd     │   │      │
│   └────────────────┘               │  │ • GetInventoryStatsQuery  │   │      │
│                                    │  │ • GetOverdueItemsQuery    │   │      │
│   Data Sources                     │  └──────────────────────────┘   │      │
│   ┌────────────────┐               │  ┌──────────────────────────┐   │      │
│   │ VehiclesSale   │──────────────▶│  │ Domain                   │   │      │
│   │ Service        │               │  │ • InventoryItem          │   │      │
│   └────────────────┘               │  │ • InventoryStatus        │   │      │
│   ┌────────────────┐               │  │ • Location, CostInfo     │   │      │
│   │ DealerService  │               │  └──────────────────────────┘   │      │
│   │ (Limits)       │               └────────────────────────────────┘      │
│   └────────────────┘                           │                        │
│                                    ┌───────────┼───────────┐                │
│                                    ▼           ▼           ▼                │
│                            ┌────────────┐ ┌────────────┐ ┌────────────┐   │
│                            │ PostgreSQL │ │   Redis    │ │  RabbitMQ  │   │
│                            │ (Inventory │ │  (Stats,   │ │ (Inventory │   │
│                            │  Items)    │ │  Metrics)  │ │  Events)   │   │
│                            └────────────┘ └────────────┘ └────────────┘   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Endpoints del Servicio

### InventoryController

| Método   | Endpoint                     | Descripción                             | Auth | Roles   |
| -------- | ---------------------------- | --------------------------------------- | ---- | ------- |
| `GET`    | `/api/inventory`             | Listar items del inventario con filtros | ✅   | Dealer  |
| `GET`    | `/api/inventory/stats`       | Estadísticas del inventario             | ✅   | Dealer  |
| `GET`    | `/api/inventory/{id}`        | Obtener item por ID                     | ✅   | Dealer  |
| `POST`   | `/api/inventory`             | Crear nuevo item de inventario          | ✅   | Dealer  |
| `PUT`    | `/api/inventory/{id}`        | Actualizar item                         | ✅   | Dealer  |
| `DELETE` | `/api/inventory/{id}`        | Eliminar item                           | ✅   | Dealer  |
| `POST`   | `/api/inventory/bulk/status` | Actualizar estado en batch              | ✅   | Dealer  |
| `GET`    | `/api/inventory/featured`    | Items destacados del dealer             | ❌   | Público |
| `GET`    | `/api/inventory/hot`         | Items con alta actividad                | ✅   | Dealer  |
| `GET`    | `/api/inventory/overdue`     | Items con más de 90 días                | ✅   | Dealer  |

---

## 📊 Entidades del Dominio

### InventoryItem (Entidad Principal)

```csharp
public class InventoryItem
{
    public Guid Id { get; set; }

    // Identificación
    public Guid DealerId { get; set; }              // Dealer propietario
    public Guid VehicleId { get; set; }             // FK a VehiclesSaleService
    public int? StockNumber { get; set; }           // Número de stock interno
    public string? VIN { get; set; }                // VIN cacheado

    // Estado
    public InventoryStatus Status { get; set; }     // Draft, Active, Paused, Sold, etc.
    public InventoryVisibility Visibility { get; set; }  // Public, Unlisted, Private

    // Notas Internas (Privadas)
    public string? InternalNotes { get; set; }      // Notas del dealer
    public string? Location { get; set; }           // Ubicación física

    // Pricing (Gestión de Precios)
    public decimal? CostPrice { get; set; }         // Precio de costo (PRIVADO)
    public decimal ListPrice { get; set; }          // Precio de lista público
    public decimal? TargetPrice { get; set; }       // Precio objetivo de venta
    public decimal? MinAcceptablePrice { get; set; } // Precio mínimo aceptable
    public bool IsNegotiable { get; set; } = true;

    // Adquisición
    public DateTime? AcquiredDate { get; set; }
    public AcquisitionSource? AcquisitionSource { get; set; }
    public string? AcquisitionDetails { get; set; }

    // Métricas de Rendimiento
    public int ViewCount { get; set; }              // Vistas totales
    public int InquiryCount { get; set; }           // Consultas recibidas
    public int TestDriveCount { get; set; }         // Test drives agendados
    public int OfferCount { get; set; }             // Ofertas recibidas
    public decimal? HighestOffer { get; set; }      // Oferta más alta
    public DateTime? LastViewedAt { get; set; }
    public DateTime? LastInquiryAt { get; set; }

    // Featured y Prioridad
    public bool IsFeatured { get; set; }
    public DateTime? FeaturedUntil { get; set; }
    public int Priority { get; set; }               // 0 = normal

    // Tags
    public List<string> Tags { get; set; } = new();

    // Timestamps
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? SoldAt { get; set; }

    // Información de Venta
    public decimal? SoldPrice { get; set; }
    public string? SoldTo { get; set; }             // Nombre del comprador

    // Propiedades Calculadas
    public int DaysOnMarket { get; }                // Días desde creación
    public decimal? PotentialProfit { get; }        // SoldPrice - CostPrice
    public decimal? ExpectedProfit { get; }         // ListPrice - CostPrice
    public bool IsOverdue { get; }                  // DaysOnMarket > 90
    public bool IsHot { get; }                      // Alta actividad
}
```

### Enumeraciones

```csharp
public enum InventoryStatus
{
    Draft,      // No publicado aún
    Active,     // Activo en marketplace
    Paused,     // Pausado temporalmente
    Pending,    // Pendiente de completar venta
    Sold,       // Vendido
    Archived    // Archivado
}

public enum InventoryVisibility
{
    Public,     // Visible para todos
    Unlisted,   // Solo por link directo
    Private     // Solo visible para el dealer
}

public enum AcquisitionSource
{
    TradeIn,        // Intercambio (trade-in)
    Auction,        // Subasta
    Wholesale,      // Compra al por mayor
    DirectPurchase, // Compra directa
    Consignment,    // Consignación
    Other           // Otro
}
```

---

## 🔄 Procesos Detallados

### PROCESO 1: Listar Inventario del Dealer

#### Endpoint: `GET /api/inventory`

| Paso | Actor      | Acción                       | Sistema                | Resultado        |
| ---- | ---------- | ---------------------------- | ---------------------- | ---------------- |
| 1    | Dealer     | Solicita lista de inventario | HTTP GET con filtros   | Request recibido |
| 2    | API        | Valida token JWT             | Authorization          | Autenticado      |
| 3    | API        | Valida dealerId del query    | Ownership check        | Autorizado       |
| 4    | Handler    | Construye query con filtros  | LINQ builder           | Query preparado  |
| 5    | Handler    | Aplica filtro de status      | WHERE Status = @status | Filtrado         |
| 6    | Handler    | Aplica búsqueda por término  | LIKE searchTerm        | Filtrado         |
| 7    | Handler    | Aplica ordenamiento          | ORDER BY sortBy        | Ordenado         |
| 8    | Handler    | Aplica paginación            | Skip/Take              | Paginado         |
| 9    | Repository | Ejecuta query                | SELECT inventory_items | Datos obtenidos  |
| 10   | API        | Retorna lista paginada       | HTTP 200               | PagedResultDto   |

#### Query Parameters

| Parámetro        | Tipo   | Default     | Descripción                               |
| ---------------- | ------ | ----------- | ----------------------------------------- |
| `dealerId`       | Guid   | required    | ID del dealer                             |
| `page`           | int    | 1           | Número de página                          |
| `pageSize`       | int    | 20          | Tamaño de página                          |
| `status`         | string | null        | Filtrar por status (Active, Paused, etc.) |
| `searchTerm`     | string | null        | Búsqueda por VIN, stockNumber             |
| `sortBy`         | string | "CreatedAt" | Campo para ordenar                        |
| `sortDescending` | bool   | false       | Orden descendente                         |

#### Response (200 OK)

```json
{
  "items": [
    {
      "id": "inventory-item-uuid",
      "vehicleId": "vehicle-uuid",
      "dealerId": "dealer-uuid",
      "stockNumber": 12345,
      "vin": "1HGCM82633A123456",
      "status": "Active",
      "visibility": "Public",
      "location": "Showroom A",
      "costPrice": 850000,
      "listPrice": 1250000,
      "targetPrice": 1150000,
      "minAcceptablePrice": 1050000,
      "isNegotiable": true,
      "daysOnMarket": 15,
      "viewCount": 125,
      "inquiryCount": 8,
      "testDriveCount": 2,
      "offerCount": 1,
      "highestOffer": 1100000,
      "isFeatured": true,
      "featuredUntil": "2026-01-20T00:00:00Z",
      "priority": 1,
      "tags": ["hot", "trade-in"],
      "isOverdue": false,
      "isHot": true,
      "expectedProfit": 400000,
      "acquiredDate": "2025-12-25T00:00:00Z",
      "acquisitionSource": "TradeIn",
      "createdAt": "2025-12-25T10:00:00Z",
      "publishedAt": "2025-12-26T08:00:00Z"
    }
  ],
  "totalCount": 45,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

---

### PROCESO 2: Obtener Estadísticas del Inventario

#### Endpoint: `GET /api/inventory/stats`

| Paso | Actor   | Acción                           | Sistema                        | Resultado           |
| ---- | ------- | -------------------------------- | ------------------------------ | ------------------- |
| 1    | Dealer  | Solicita estadísticas            | HTTP GET                       | Request recibido    |
| 2    | API     | Valida autenticación             | JWT check                      | Autorizado          |
| 3    | Handler | Cuenta items por status          | GROUP BY Status                | Conteos por status  |
| 4    | Handler | Calcula totales                  | SUM, AVG queries               | Métricas totales    |
| 5    | Handler | Calcula días promedio en mercado | AVG(DaysOnMarket)              | Promedio            |
| 6    | Handler | Identifica items overdue         | DaysOnMarket > 90              | Items vencidos      |
| 7    | Handler | Identifica hot items             | Criteria check                 | Items calientes     |
| 8    | Handler | Calcula inventario total         | SUM(CostPrice), SUM(ListPrice) | Valor de inventario |
| 9    | API     | Retorna estadísticas             | HTTP 200                       | InventoryStatsDto   |

#### Response (200 OK)

```json
{
  "dealerId": "dealer-uuid",
  "totalItems": 45,
  "byStatus": {
    "draft": 5,
    "active": 32,
    "paused": 3,
    "pending": 2,
    "sold": 0,
    "archived": 3
  },
  "byVisibility": {
    "public": 35,
    "unlisted": 5,
    "private": 5
  },
  "metrics": {
    "totalViews": 4520,
    "totalInquiries": 245,
    "totalTestDrives": 67,
    "totalOffers": 34,
    "conversionRate": 4.2
  },
  "daysOnMarket": {
    "average": 28,
    "min": 1,
    "max": 120,
    "overdueCount": 8
  },
  "pricing": {
    "totalCostValue": 38250000,
    "totalListValue": 56250000,
    "expectedTotalProfit": 18000000,
    "averageListPrice": 1250000,
    "averageCostPrice": 850000,
    "averageMarkup": 47.1
  },
  "performance": {
    "hotItemsCount": 5,
    "featuredCount": 8,
    "avgViewsPerItem": 100.4,
    "avgInquiriesPerItem": 5.4,
    "avgOffersPerItem": 0.76
  },
  "recentActivity": {
    "viewsLast7Days": 890,
    "inquiriesLast7Days": 45,
    "offersLast7Days": 12
  },
  "alerts": [
    {
      "type": "overdue",
      "message": "8 vehículos tienen más de 90 días sin venderse",
      "itemIds": ["uuid1", "uuid2", "..."]
    },
    {
      "type": "low_stock",
      "message": "Solo quedan 32 vehículos activos",
      "severity": "warning"
    }
  ]
}
```

---

### PROCESO 3: Crear Item de Inventario

#### Endpoint: `POST /api/inventory`

| Paso | Actor                   | Acción                            | Sistema                       | Resultado            |
| ---- | ----------------------- | --------------------------------- | ----------------------------- | -------------------- |
| 1    | Dealer                  | Envía datos del item              | HTTP POST                     | Request recibido     |
| 2    | API                     | Valida autenticación              | JWT check                     | Autorizado           |
| 3    | Validador               | Valida campos requeridos          | FluentValidation              | Validación OK        |
| 4    | Handler                 | Verifica que VehicleId exista     | VehiclesSaleService call      | Vehículo existe      |
| 5    | Handler                 | Verifica límites del plan         | DealerManagementService check | Dentro del límite    |
| 6    | Handler                 | Crea entidad InventoryItem        | new InventoryItem()           | Item creado          |
| 7    | Handler                 | Asigna Status = Draft             | InventoryStatus.Draft         | Estado inicial       |
| 8    | Repository              | Persiste en base de datos         | INSERT inventory_items        | Item guardado        |
| 9    | EventBus                | Publica InventoryItemCreatedEvent | RabbitMQ                      | Evento emitido       |
| 10   | DealerManagementService | Incrementa contador de listings   | HTTP POST                     | Contador actualizado |
| 11   | API                     | Retorna 201 Created               | HTTP Response                 | InventoryItemDto     |

#### Request Body

```json
{
  "dealerId": "dealer-uuid",
  "vehicleId": "vehicle-uuid",
  "internalNotes": "Trade-in del cliente José Pérez. Buen estado general.",
  "location": "Showroom A - Estante 3",
  "stockNumber": 12345,
  "vin": "1HGCM82633A123456",
  "costPrice": 850000,
  "listPrice": 1250000,
  "targetPrice": 1150000,
  "minAcceptablePrice": 1050000,
  "isNegotiable": true,
  "acquiredDate": "2026-01-05",
  "acquisitionSource": "TradeIn",
  "acquisitionDetails": "Trade-in por Toyota Camry 2024 nuevo"
}
```

#### Response (201 Created)

```json
{
  "id": "new-inventory-item-uuid",
  "dealerId": "dealer-uuid",
  "vehicleId": "vehicle-uuid",
  "stockNumber": 12345,
  "vin": "1HGCM82633A123456",
  "status": "Draft",
  "visibility": "Private",
  "location": "Showroom A - Estante 3",
  "costPrice": 850000,
  "listPrice": 1250000,
  "targetPrice": 1150000,
  "minAcceptablePrice": 1050000,
  "isNegotiable": true,
  "daysOnMarket": 0,
  "expectedProfit": 400000,
  "acquiredDate": "2026-01-05T00:00:00Z",
  "acquisitionSource": "TradeIn",
  "createdAt": "2026-01-09T10:30:00Z"
}
```

---

### PROCESO 4: Actualizar Item de Inventario

#### Endpoint: `PUT /api/inventory/{id}`

| Paso | Actor      | Acción                            | Sistema                   | Resultado             |
| ---- | ---------- | --------------------------------- | ------------------------- | --------------------- |
| 1    | Dealer     | Envía actualización               | HTTP PUT                  | Request recibido      |
| 2    | API        | Valida autenticación              | JWT check                 | Autorizado            |
| 3    | Handler    | Busca item por ID                 | Repository.GetByIdAsync() | Item encontrado       |
| 4    | Handler    | Valida ownership                  | item.DealerId == dealerId | Autorizado            |
| 5    | Validador  | Valida datos                      | FluentValidation          | Datos válidos         |
| 6    | Handler    | Actualiza campos permitidos       | Mapper update             | Campos actualizados   |
| 7    | Handler    | Registra UpdatedAt                | DateTime.UtcNow           | Timestamp actualizado |
| 8    | Repository | Persiste cambios                  | UPDATE inventory_items    | Cambios guardados     |
| 9    | EventBus   | Publica InventoryItemUpdatedEvent | RabbitMQ                  | Evento emitido        |
| 10   | API        | Retorna item actualizado          | HTTP 200                  | InventoryItemDto      |

#### Request Body

```json
{
  "internalNotes": "Actualización: Se reparó el aire acondicionado",
  "location": "Showroom B - Entrada principal",
  "listPrice": 1200000,
  "targetPrice": 1100000,
  "minAcceptablePrice": 1000000,
  "isNegotiable": true,
  "isFeatured": true,
  "priority": 2
}
```

---

### PROCESO 5: Actualización en Batch (Bulk Status Update)

#### Endpoint: `POST /api/inventory/bulk/status`

| Paso | Actor      | Acción                            | Sistema                    | Resultado          |
| ---- | ---------- | --------------------------------- | -------------------------- | ------------------ |
| 1    | Dealer     | Envía lista de IDs y nuevo status | HTTP POST                  | Request recibido   |
| 2    | API        | Valida autenticación              | JWT check                  | Autorizado         |
| 3    | Validador  | Valida lista de IDs               | FluentValidation           | IDs válidos        |
| 4    | Validador  | Valida status permitido           | Enum check                 | Status válido      |
| 5    | Handler    | Obtiene todos los items           | Repository.GetByIdsAsync() | Items obtenidos    |
| 6    | Handler    | Valida ownership de todos         | item.DealerId check        | Todos autorizados  |
| 7    | Handler    | Actualiza status en batch         | Bulk update                | Status actualizado |
| 8    | Repository | Persiste cambios                  | Batch UPDATE               | Cambios guardados  |
| 9    | EventBus   | Publica BulkStatusUpdatedEvent    | RabbitMQ                   | Evento emitido     |
| 10   | API        | Retorna 204 No Content            | HTTP Response              | Sin contenido      |

#### Request Body

```json
{
  "itemIds": ["inventory-uuid-1", "inventory-uuid-2", "inventory-uuid-3"],
  "status": "Paused"
}
```

#### Operaciones de Bulk Disponibles

| Operación | Status Resultante | Descripción              |
| --------- | ----------------- | ------------------------ |
| Activar   | `Active`          | Publicar múltiples items |
| Pausar    | `Paused`          | Pausar temporalmente     |
| Archivar  | `Archived`        | Mover a archivo          |
| Borrador  | `Draft`           | Volver a borrador        |

---

### PROCESO 6: Obtener Items Destacados (Featured)

#### Endpoint: `GET /api/inventory/featured`

| Paso | Actor     | Acción                       | Sistema                                         | Resultado              |
| ---- | --------- | ---------------------------- | ----------------------------------------------- | ---------------------- |
| 1    | Visitante | Solicita featured del dealer | HTTP GET (sin auth)                             | Request recibido       |
| 2    | Handler   | Filtra por dealerId          | WHERE DealerId = @dealerId                      | Filtrado               |
| 3    | Handler   | Filtra featured activos      | WHERE IsFeatured = true AND FeaturedUntil > now | Filtrado               |
| 4    | Handler   | Filtra solo públicos         | WHERE Visibility = Public AND Status = Active   | Filtrado               |
| 5    | Handler   | Ordena por priority DESC     | ORDER BY Priority DESC                          | Ordenado               |
| 6    | Handler   | Limita resultados            | TAKE 10                                         | Limitado               |
| 7    | API       | Retorna lista                | HTTP 200                                        | List<InventoryItemDto> |

---

### PROCESO 7: Obtener Items Hot (Alta Actividad)

#### Endpoint: `GET /api/inventory/hot`

| Paso | Actor   | Acción                | Sistema                                                   | Resultado              |
| ---- | ------- | --------------------- | --------------------------------------------------------- | ---------------------- |
| 1    | Dealer  | Solicita hot items    | HTTP GET                                                  | Request recibido       |
| 2    | API     | Valida autenticación  | JWT check                                                 | Autorizado             |
| 3    | Handler | Filtra por criteria   | ViewCount > 50 AND InquiryCount > 5 AND DaysOnMarket < 30 | Hot items              |
| 4    | Handler | Ordena por engagement | ORDER BY (Views + Inquiries\*10) DESC                     | Ordenado               |
| 5    | API     | Retorna lista         | HTTP 200                                                  | List<InventoryItemDto> |

#### Criteria de "Hot Item"

| Métrica      | Umbral |
| ------------ | ------ |
| ViewCount    | > 50   |
| InquiryCount | > 5    |
| DaysOnMarket | < 30   |

---

### PROCESO 8: Obtener Items Overdue (Más de 90 Días)

#### Endpoint: `GET /api/inventory/overdue`

| Paso | Actor   | Acción                            | Sistema                                     | Resultado          |
| ---- | ------- | --------------------------------- | ------------------------------------------- | ------------------ |
| 1    | Dealer  | Solicita overdue items            | HTTP GET                                    | Request recibido   |
| 2    | API     | Valida autenticación              | JWT check                                   | Autorizado         |
| 3    | Handler | Calcula DaysOnMarket              | DATEDIFF(now, CreatedAt)                    | Días calculados    |
| 4    | Handler | Filtra overdue                    | WHERE DaysOnMarket > 90 AND Status = Active | Overdue items      |
| 5    | Handler | Ordena por días DESC              | ORDER BY DaysOnMarket DESC                  | Más viejos primero |
| 6    | API     | Retorna lista con recomendaciones | HTTP 200                                    | OverdueItemsDto    |

#### Response (200 OK)

```json
{
  "items": [
    {
      "id": "inventory-uuid",
      "stockNumber": 10234,
      "vin": "1HGCM82633A123456",
      "daysOnMarket": 120,
      "listPrice": 1500000,
      "viewCount": 45,
      "inquiryCount": 2,
      "offerCount": 0,
      "recommendations": [
        "Considere reducir el precio un 10%",
        "Este vehículo tiene pocas consultas",
        "Agregue más fotos para aumentar interés"
      ]
    }
  ],
  "totalOverdue": 8,
  "averageDaysOverdue": 105,
  "potentialLoss": 2500000
}
```

---

## 🔔 Eventos de Dominio (RabbitMQ)

### Eventos Publicados

| Evento                        | Exchange           | Routing Key                | Payload                      |
| ----------------------------- | ------------------ | -------------------------- | ---------------------------- |
| `InventoryItemCreatedEvent`   | `inventory.events` | `inventory.created`        | ItemId, DealerId, VehicleId  |
| `InventoryItemUpdatedEvent`   | `inventory.events` | `inventory.updated`        | ItemId, ChangedFields        |
| `InventoryItemDeletedEvent`   | `inventory.events` | `inventory.deleted`        | ItemId, DealerId             |
| `InventoryStatusChangedEvent` | `inventory.events` | `inventory.status_changed` | ItemId, OldStatus, NewStatus |
| `InventoryItemFeaturedEvent`  | `inventory.events` | `inventory.featured`       | ItemId, FeaturedUntil        |
| `InventorySoldEvent`          | `inventory.events` | `inventory.sold`           | ItemId, SoldPrice, Profit    |
| `InventoryOverdueAlertEvent`  | `inventory.events` | `inventory.overdue_alert`  | DealerId, OverdueItems       |
| `BulkStatusUpdatedEvent`      | `inventory.events` | `inventory.bulk_updated`   | ItemIds, NewStatus           |

### Eventos Consumidos

| Evento                    | Origen              | Acción                     |
| ------------------------- | ------------------- | -------------------------- |
| `VehicleViewedEvent`      | VehiclesSaleService | Incrementar ViewCount      |
| `InquiryReceivedEvent`    | ContactService      | Incrementar InquiryCount   |
| `TestDriveScheduledEvent` | AppointmentService  | Incrementar TestDriveCount |
| `OfferReceivedEvent`      | OffersService       | Registrar oferta           |
| `VehicleSoldEvent`        | SalesService        | Marcar como vendido        |

---

## ⚠️ Reglas de Negocio

### Gestión de Inventario

| #   | Regla                                         | Validación                                |
| --- | --------------------------------------------- | ----------------------------------------- |
| 1   | VehicleId debe existir en VehiclesSaleService | Service call verification                 |
| 2   | StockNumber único por dealer                  | Unique constraint (DealerId, StockNumber) |
| 3   | VIN debe ser válido (17 caracteres)           | Regex pattern                             |
| 4   | ListPrice debe ser mayor que 0                | > 0                                       |
| 5   | MinAcceptablePrice <= ListPrice               | Business rule                             |
| 6   | CostPrice < ListPrice (idealmente)            | Warning si viola                          |

### Límites por Plan

| Plan       | Max Items | Max Featured |
| ---------- | --------- | ------------ |
| Free       | 3         | 0            |
| Basic      | 50        | 3            |
| Pro        | 200       | 10           |
| Enterprise | ∞         | 25           |

### Overdue Alerts

| Días | Nivel    | Acción               |
| ---- | -------- | -------------------- |
| 60   | Warning  | Notificación interna |
| 90   | Alert    | Email al dealer      |
| 120  | Critical | Dashboard highlight  |

---

## ❌ Códigos de Error

| Código    | HTTP Status | Mensaje                                | Causa                                 |
| --------- | ----------- | -------------------------------------- | ------------------------------------- |
| `INV_001` | 400         | VehicleId no existe                    | Vehículo no encontrado                |
| `INV_002` | 400         | StockNumber duplicado                  | Stock number ya existe para el dealer |
| `INV_003` | 400         | VIN inválido                           | Formato incorrecto                    |
| `INV_004` | 404         | Item de inventario no encontrado       | ID inexistente                        |
| `INV_005` | 403         | No autorizado para este inventario     | DealerId no coincide                  |
| `INV_006` | 400         | Límite de inventario alcanzado         | Plan no permite más items             |
| `INV_007` | 400         | Límite de featured alcanzado           | Plan no permite más featured          |
| `INV_008` | 400         | MinAcceptablePrice mayor que ListPrice | Regla de negocio violada              |
| `INV_009` | 400         | IDs inválidos en bulk operation        | Algún ID no existe                    |

---

## ⚙️ Configuración del Servicio

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=inventory_db;Username=postgres;Password=xxx"
  },
  "InventorySettings": {
    "OverdueThresholdDays": 90,
    "HotItemViewThreshold": 50,
    "HotItemInquiryThreshold": 5,
    "HotItemMaxDays": 30,
    "DefaultPageSize": 20,
    "MaxPageSize": 100
  },
  "ServiceUrls": {
    "VehiclesSaleService": "http://vehiclessaleservice:8080",
    "DealerManagementService": "http://dealermanagementservice:8080"
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchange": "inventory.events"
  }
}
```

---

## 🔒 Seguridad

### Autenticación y Autorización

| Endpoint                    | Auth | Acceso                  |
| --------------------------- | ---- | ----------------------- |
| GET /api/inventory          | ✅   | Solo dealer propietario |
| GET /api/inventory/stats    | ✅   | Solo dealer propietario |
| GET /api/inventory/{id}     | ✅   | Solo dealer propietario |
| POST /api/inventory         | ✅   | Dealers verificados     |
| PUT /api/inventory/{id}     | ✅   | Solo dealer propietario |
| DELETE /api/inventory/{id}  | ✅   | Solo dealer propietario |
| POST /api/inventory/bulk/\* | ✅   | Solo dealer propietario |
| GET /api/inventory/featured | ❌   | Público                 |

### Datos Privados

Los siguientes campos **NUNCA** se exponen en endpoints públicos:

- `CostPrice` - Precio de costo
- `MinAcceptablePrice` - Precio mínimo
- `InternalNotes` - Notas internas
- `AcquisitionDetails` - Detalles de adquisición
- `SoldTo` - Información del comprador

---

## 📈 Métricas y Observabilidad

### Métricas Prometheus

| Métrica                        | Tipo    | Labels            | Descripción               |
| ------------------------------ | ------- | ----------------- | ------------------------- |
| `inventory_items_total`        | Gauge   | dealer_id, status | Items por dealer y status |
| `inventory_value_total`        | Gauge   | dealer_id, type   | Valor total (cost/list)   |
| `inventory_days_on_market_avg` | Gauge   | dealer_id         | Promedio días en mercado  |
| `inventory_overdue_count`      | Gauge   | dealer_id         | Items con más de 90 días  |
| `inventory_views_total`        | Counter | dealer_id         | Vistas totales            |
| `inventory_inquiries_total`    | Counter | dealer_id         | Consultas totales         |

---

## 📚 Referencias

- [InventoryController](../../backend/InventoryManagementService/InventoryManagementService.Api/Controllers/InventoryController.cs)
- [InventoryItem Entity](../../backend/InventoryManagementService/InventoryManagementService.Domain/Entities/InventoryItem.cs)
- [DealerManagementService Integration](02-USUARIOS-DEALERS/02-dealer-management.md)

---

**Última actualización:** Enero 9, 2026  
**Autor:** Sistema de Documentación Automatizado  
**Versión:** 1.0.0
