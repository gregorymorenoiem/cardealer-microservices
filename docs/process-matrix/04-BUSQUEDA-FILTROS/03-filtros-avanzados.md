# 🔍 Filtros Avanzados de Búsqueda

> **Código:** SEARCH-001  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🔴 ALTA (UX y Engagement)  
> **Origen:** CarGurus

---

## 📋 Información General

| Campo             | Valor                                     |
| ----------------- | ----------------------------------------- |
| **Servicio**      | VehiclesSaleService (extender)            |
| **Puerto**        | 5005                                      |
| **Base de Datos** | `vehiclessaleservice`                     |
| **Dependencias**  | PricingIntelligenceService, Elasticsearch |

---

## 🎯 Objetivo del Proceso

1. **Descubrimiento:** Usuarios encuentran exactamente lo que buscan
2. **Engagement:** Filtros útiles = más tiempo en sitio
3. **Diferenciación:** Filtros que SuperCarros NO tiene
4. **Conversión:** Búsqueda precisa = mayor intención de compra

---

## 📡 Nuevos Filtros a Implementar

### Filtros Existentes (Base)

```yaml
Básicos:
  - Marca (make)
  - Modelo (model)
  - Año (yearMin, yearMax)
  - Precio (priceMin, priceMax)
  - Kilometraje (mileageMin, mileageMax)
  - Ubicación (city, province)
  - Tipo de carrocería (bodyType)
  - Transmisión (transmission)
  - Combustible (fuelType)
  - Color (exteriorColor)
```

### 🆕 Filtros Avanzados (Nuevos)

```yaml
Deal Rating:
  - dealRating: GreatDeal, GoodDeal, FairDeal, HighPrice, Overpriced
  - Descripción: "Filtrar por calificación de precio"

Days on Market:
  - daysOnMarketMax: 7, 14, 30, 60, 90
  - Descripción: "Nuevos listados" (recién publicados)

Price Drops:
  - hasPriceDrop: true/false
  - priceDropMin: porcentaje mínimo de reducción
  - Descripción: "Vehículos con precio reducido"

New Listings:
  - newListings: true (últimos 7 días)
  - Descripción: "Ver los más nuevos primero"

OKLA Certified:
  - isCertified: true/false
  - Descripción: "Solo vehículos certificados OKLA"

Seller Type:
  - sellerType: Dealer, Individual, Certified
  - Descripción: "Filtrar por tipo de vendedor"

Features/Equipment:
  - features[]: SunRoof, LeatherSeats, Navigation, Bluetooth, BackupCamera, etc.
  - Descripción: "Equipamiento específico"

Number of Owners:
  - maxOwners: 1, 2, 3+
  - Descripción: "Número de dueños anteriores"

Accident History:
  - noAccidents: true
  - Descripción: "Sin historial de accidentes"

Financing Available:
  - hasFinancing: true
  - Descripción: "Con opciones de financiamiento"

Photos Count:
  - minPhotos: 5, 10, 15, 20
  - Descripción: "Listings con muchas fotos"

Video Available:
  - hasVideo: true
  - Descripción: "Con video del vehículo"

Verified Seller:
  - verifiedSeller: true
  - Descripción: "Vendedor verificado"

Warranty Included:
  - hasWarranty: true
  - Descripción: "Incluye garantía"
```

---

## 📡 Endpoint Actualizado

### GET /api/vehicles/search

```http
GET /api/vehicles/search
  ?make=Toyota
  &model=Corolla
  &yearMin=2020
  &yearMax=2024
  &priceMin=800000
  &priceMax=1500000
  &dealRating=GreatDeal,GoodDeal
  &daysOnMarketMax=30
  &hasPriceDrop=true
  &isCertified=true
  &sellerType=Dealer
  &features=LeatherSeats,Navigation
  &noAccidents=true
  &verifiedSeller=true
  &sortBy=dealRating
  &sortOrder=asc
  &page=1
  &pageSize=20
```

---

## 🗃️ Entidades

### VehicleSearchFilters

```csharp
public class VehicleSearchFilters
{
    // Básicos
    public string Make { get; set; }
    public string Model { get; set; }
    public int? YearMin { get; set; }
    public int? YearMax { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public int? MileageMin { get; set; }
    public int? MileageMax { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string BodyType { get; set; }
    public string Transmission { get; set; }
    public string FuelType { get; set; }
    public string ExteriorColor { get; set; }

    // 🆕 Filtros Avanzados
    public List<DealRatingLevel> DealRating { get; set; }
    public int? DaysOnMarketMax { get; set; }
    public bool? HasPriceDrop { get; set; }
    public decimal? PriceDropMin { get; set; }
    public bool? NewListings { get; set; }
    public bool? IsCertified { get; set; }
    public SellerType? SellerType { get; set; }
    public List<string> Features { get; set; }
    public int? MaxOwners { get; set; }
    public bool? NoAccidents { get; set; }
    public bool? HasFinancing { get; set; }
    public int? MinPhotos { get; set; }
    public bool? HasVideo { get; set; }
    public bool? VerifiedSeller { get; set; }
    public bool? HasWarranty { get; set; }

    // Sorting
    public string SortBy { get; set; }
    public string SortOrder { get; set; }

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public enum SellerType
{
    All,
    Dealer,
    Individual,
    Certified
}
```

### SearchResult with Facets

```csharp
public class VehicleSearchResult
{
    public List<VehicleListingDto> Vehicles { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    // 🆕 Facets para filtros dinámicos
    public SearchFacets Facets { get; set; }
}

public class SearchFacets
{
    public List<FacetItem> Makes { get; set; }
    public List<FacetItem> Models { get; set; }
    public List<FacetItem> Years { get; set; }
    public List<FacetItem> BodyTypes { get; set; }
    public List<FacetItem> Transmissions { get; set; }
    public List<FacetItem> FuelTypes { get; set; }
    public List<FacetItem> Cities { get; set; }

    // 🆕 Nuevos facets
    public List<FacetItem> DealRatings { get; set; }
    public List<FacetItem> SellerTypes { get; set; }
    public List<FacetItem> Features { get; set; }

    // Rangos
    public RangeFacet PriceRange { get; set; }
    public RangeFacet MileageRange { get; set; }
    public RangeFacet YearRange { get; set; }

    // Counts especiales
    public int CertifiedCount { get; set; }
    public int PriceDropCount { get; set; }
    public int NewListingsCount { get; set; }
    public int GreatDealsCount { get; set; }
}

public class FacetItem
{
    public string Value { get; set; }
    public string Label { get; set; }
    public int Count { get; set; }
}

public class RangeFacet
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Avg { get; set; }
}
```

---

## 📊 Proceso SEARCH-001: Búsqueda con Filtros Avanzados

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: SEARCH-001 - Búsqueda con Filtros Avanzados                   │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-ANON o USR-REG                                    │
│ Sistemas: VehiclesSaleService, PricingIntelligenceService, Elasticsearch│
│ Latencia objetivo: < 200ms                                             │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                        | Sistema                    | Actor    | Evidencia             | Código     |
| ---- | ------- | --------------------------------------------- | -------------------------- | -------- | --------------------- | ---------- |
| 1    | 1.1     | Usuario accede a /buscar                      | Frontend                   | USR-ANON | Page view             | EVD-LOG    |
| 1    | 1.2     | **GET /api/vehicles/search** (inicial)        | Gateway                    | USR-ANON | **Request**           | EVD-LOG    |
| 2    | 2.1     | **Parsear filtros**                           | VehiclesSaleService        | Sistema  | **Filters parsed**    | EVD-LOG    |
| 2    | 2.2     | Validar parámetros                            | VehiclesSaleService        | Sistema  | Validation            | EVD-LOG    |
| 3    | 3.1     | **Construir query Elasticsearch**             | VehiclesSaleService        | Sistema  | **Query built**       | EVD-LOG    |
| 3    | 3.2     | Aplicar filtros básicos                       | Sistema                    | Sistema  | Basic filters         | EVD-LOG    |
| 3    | 3.3     | **Aplicar filtros avanzados**                 | Sistema                    | Sistema  | **Advanced filters**  | EVD-LOG    |
| 4    | 4.1     | Si dealRating filter: Join con PricingService | VehiclesSaleService        | Sistema  | Join query            | EVD-LOG    |
| 4    | 4.2     | Si isCertified: Join con CertificationService | VehiclesSaleService        | Sistema  | Join query            | EVD-LOG    |
| 5    | 5.1     | **Ejecutar búsqueda**                         | Elasticsearch              | Sistema  | **Search executed**   | EVD-LOG    |
| 5    | 5.2     | Obtener resultados paginados                  | Elasticsearch              | Sistema  | Results               | EVD-LOG    |
| 5    | 5.3     | **Calcular facets**                           | Elasticsearch              | Sistema  | **Facets calc**       | EVD-LOG    |
| 6    | 6.1     | Enriquecer resultados                         | VehiclesSaleService        | Sistema  | Enrichment            | EVD-LOG    |
| 6    | 6.2     | Agregar Deal Rating a cada resultado          | PricingIntelligenceService | Sistema  | Ratings added         | EVD-LOG    |
| 6    | 6.3     | Agregar badges (Certified, Verified, etc.)    | VehiclesSaleService        | Sistema  | Badges added          | EVD-LOG    |
| 7    | 7.1     | **Aplicar sorting**                           | VehiclesSaleService        | Sistema  | **Sorting applied**   | EVD-LOG    |
| 8    | 8.1     | **Retornar VehicleSearchResult**              | VehiclesSaleService        | Sistema  | **Response**          | EVD-LOG    |
| 9    | 9.1     | **Mostrar resultados**                        | Frontend                   | Sistema  | **Results shown**     | EVD-SCREEN |
| 9    | 9.2     | Mostrar filtros con counts                    | Frontend                   | Sistema  | Filters shown         | EVD-LOG    |
| 10   | 10.1    | Usuario aplica filtro adicional               | Frontend                   | USR-ANON | Filter applied        | EVD-LOG    |
| 10   | 10.2    | **Actualizar URL**                            | Frontend                   | Sistema  | **URL updated**       | EVD-LOG    |
| 10   | 10.3    | **Nueva búsqueda**                            | Gateway                    | USR-ANON | **New search**        | EVD-LOG    |
| 11   | 11.1    | **Track search analytics**                    | AnalyticsService           | Sistema  | **Analytics tracked** | EVD-LOG    |
| 12   | 12.1    | **Audit trail**                               | AuditService               | Sistema  | Complete audit        | EVD-AUDIT  |

---

## 📱 UI Mockup - Filtros Avanzados

```
┌─────────────────────────────────────────────────────────────────────────┐
│                                                                         │
│  🔍 FILTROS                                          [Limpiar todo]    │
│  ═══════════════════════════════════════════════════════════════════   │
│                                                                         │
│  📊 DEAL RATING                                                        │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  ☑️ 🟢 Excelente Precio (45)                                    │   │
│  │  ☑️ 🟢 Buen Precio (128)                                        │   │
│  │  ☐  🟡 Precio Justo (256)                                       │   │
│  │  ☐  🟠 Precio Alto (89)                                         │   │
│  │  ☐  🔴 Sobrepreciado (23)                                       │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ⏱️ TIEMPO EN MERCADO                                                  │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  ○ Cualquiera                                                   │   │
│  │  ● Nuevos (últimos 7 días) (34)                                 │   │
│  │  ○ Últimos 14 días (67)                                         │   │
│  │  ○ Últimos 30 días (145)                                        │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  📉 CAMBIOS DE PRECIO                                                  │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  ☑️ Con precio reducido (23)                                    │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ✅ CERTIFICACIONES                                                    │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  ☑️ OKLA Certified (56)                                         │   │
│  │  ☑️ Vendedor Verificado (234)                                   │   │
│  │  ☐  Con Garantía (145)                                          │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  🏪 TIPO DE VENDEDOR                                                   │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  ● Todos                                                        │   │
│  │  ○ Solo Dealers (345)                                           │   │
│  │  ○ Solo Particulares (196)                                      │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  📋 HISTORIAL                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  ☑️ Sin accidentes reportados (412)                             │   │
│  │  ☐  Un solo dueño (189)                                         │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  🎬 MULTIMEDIA                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  ☐  Con video (45)                                              │   │
│  │  ☐  10+ fotos (234)                                             │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ⚙️ EQUIPAMIENTO                                                       │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  [Buscar equipamiento...]                                       │   │
│  │                                                                 │   │
│  │  ☑️ Cámara de reversa (312)                                     │   │
│  │  ☑️ Bluetooth (456)                                             │   │
│  │  ☐  Techo solar (89)                                            │   │
│  │  ☐  Asientos de cuero (134)                                     │   │
│  │  ☐  Navegación GPS (78)                                         │   │
│  │  [+ Ver más equipamiento]                                       │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│          ┌─────────────────────────────────────┐                       │
│          │    🔍 APLICAR FILTROS (173)         │                       │
│          └─────────────────────────────────────┘                       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📱 UI Mockup - Chips de Filtros Activos

```
┌─────────────────────────────────────────────────────────────────────────┐
│                                                                         │
│  Filtros activos:                                                      │
│                                                                         │
│  ┌──────────────┐ ┌──────────────┐ ┌────────────────────┐              │
│  │🟢 Buen Precio ×│ │Últimos 7 días ×│ │✅ OKLA Certified ×│             │
│  └──────────────┘ └──────────────┘ └────────────────────┘              │
│  ┌─────────────────────┐ ┌──────────────────┐                          │
│  │📉 Con precio reducido ×│ │🚫 Sin accidentes ×│                         │
│  └─────────────────────┘ └──────────────────┘                          │
│                                                                         │
│  [Limpiar todos los filtros]                                           │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Sorting Options

```yaml
Opciones de Ordenamiento:
  - bestMatch: "Mejor coincidencia" (default)
  - dealRating: "Mejor precio" (Great → Overpriced)
  - priceLowHigh: "Precio: menor a mayor"
  - priceHighLow: "Precio: mayor a menor"
  - newestFirst: "Más nuevos primero"
  - mileageLowHigh: "Menor kilometraje"
  - yearNewest: "Año más reciente"
  - recentlyListed: "Recién publicados"
  - priceDropRecent: "Reducciones recientes"
```

---

## 📊 Métricas Prometheus

```yaml
# Búsquedas
search_requests_total
search_latency_ms
search_results_count_avg

# Filtros
filter_usage_total{filter}
filter_combination_popular
filters_per_search_avg

# Engagement
search_to_click_rate
search_to_contact_rate
search_refinement_rate

# Performance
elasticsearch_query_time_ms
facet_calculation_time_ms
cache_hit_rate
```

---

## 🔗 Referencias

- [00-ANALISIS-COMPETITIVO.md](../00-ANALISIS-COMPETITIVO.md)
- [20-PRICING-INTELLIGENCE/01-deal-rating.md](../20-PRICING-INTELLIGENCE/01-deal-rating.md)
- [15-CONFIANZA-SEGURIDAD/05-okla-certified.md](../15-CONFIANZA-SEGURIDAD/05-okla-certified.md)
