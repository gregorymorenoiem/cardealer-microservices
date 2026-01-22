# 🚀 Boost y Promoción de Listings

> **Código:** PROMO-001, PROMO-002  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🟡 MEDIA (Monetización y visibilidad)

---

## 📋 Información General

| Campo             | Valor                                                    |
| ----------------- | -------------------------------------------------------- |
| **Servicio**      | PromotionService                                         |
| **Puerto**        | 5085                                                     |
| **Base de Datos** | `promotionservice`                                       |
| **Dependencias**  | VehiclesSaleService, BillingService, NotificationService |

---

## 🎯 Objetivo del Proceso

1. **Boost de Listing:** Aumentar visibilidad de un vehículo por tiempo limitado
2. **Listing Destacado:** Aparecer en secciones premium del homepage
3. **Renovación Automática:** Mantener listings activos y visibles
4. **Analytics de Promoción:** Medir ROI de las promociones

---

## 📡 Endpoints

| Método   | Endpoint                                | Descripción             | Auth |
| -------- | --------------------------------------- | ----------------------- | ---- |
| `GET`    | `/api/promotions/options`               | Opciones de promoción   | ❌   |
| `POST`   | `/api/promotions/boost`                 | Aplicar boost a listing | ✅   |
| `POST`   | `/api/promotions/featured`              | Solicitar destacado     | ✅   |
| `GET`    | `/api/promotions/my-promotions`         | Mis promociones activas | ✅   |
| `GET`    | `/api/promotions/analytics/{vehicleId}` | Analytics de promoción  | ✅   |
| `DELETE` | `/api/promotions/{id}`                  | Cancelar promoción      | ✅   |

---

## 🗃️ Entidades

### Promotion

```csharp
public class Promotion
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid UserId { get; set; }
    public Guid? DealerId { get; set; }

    // Tipo y duración
    public PromotionType Type { get; set; }
    public PromotionPlan Plan { get; set; }
    public int DurationDays { get; set; }

    // Fechas
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Estado
    public PromotionStatus Status { get; set; }
    public bool AutoRenew { get; set; }

    // Pago
    public decimal Price { get; set; }
    public Guid PaymentId { get; set; }

    // Analytics
    public PromotionAnalytics Analytics { get; set; }

    public DateTime CreatedAt { get; set; }
}

public enum PromotionType
{
    Boost,              // Aumentar posición en búsqueda
    Featured,           // Aparecer en homepage
    Spotlight,          // Banner especial
    UrgentSale,         // Badge "URGENTE"
    PriceDropAlert,     // Notificar a interesados sobre baja
    Bundle              // Combinación
}

public enum PromotionPlan
{
    // Boost
    Boost3Days,
    Boost7Days,
    Boost14Days,
    Boost30Days,

    // Featured
    Featured1Day,
    Featured3Days,
    Featured7Days,

    // Spotlight
    SpotlightWeekend,
    SpotlightWeek,

    // Bundles
    StarterPack,        // Boost 7 + Featured 1
    ProPack,            // Boost 14 + Featured 3 + Spotlight Weekend
    MaxVisibility       // Boost 30 + Featured 7 + Spotlight Week
}

public enum PromotionStatus
{
    Pending,        // Pago pendiente
    Active,         // Activa
    Paused,         // Pausada por usuario
    Expired,        // Expirada
    Cancelled,      // Cancelada
    Completed       // Completada
}
```

### PromotionAnalytics

```csharp
public class PromotionAnalytics
{
    public Guid PromotionId { get; set; }

    // Antes de promoción (7 días previos)
    public int ViewsBeforePromotion { get; set; }
    public int ContactsBeforePromotion { get; set; }
    public int FavoritesBeforePromotion { get; set; }

    // Durante promoción
    public int ViewsDuringPromotion { get; set; }
    public int ContactsDuringPromotion { get; set; }
    public int FavoritesDuringPromotion { get; set; }

    // Incrementos
    public decimal ViewsIncreasePercent { get; set; }
    public decimal ContactsIncreasePercent { get; set; }
    public decimal FavoritesIncreasePercent { get; set; }

    // Posición en búsqueda
    public int AveragePositionBefore { get; set; }
    public int AveragePositionDuring { get; set; }

    // ROI
    public decimal CostPerView { get; set; }
    public decimal CostPerContact { get; set; }

    public DateTime LastCalculated { get; set; }
}
```

### FeaturedPlacement

```csharp
public class FeaturedPlacement
{
    public Guid Id { get; set; }
    public Guid PromotionId { get; set; }
    public Guid VehicleId { get; set; }

    public PlacementLocation Location { get; set; }
    public int Position { get; set; }           // 1-10 en carrusel

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public bool IsActive { get; set; }
}

public enum PlacementLocation
{
    HomepageCarousel,       // Carrusel principal
    HomepageGrid,           // Grid de destacados
    CategoryTop,            // Top de categoría (SUVs, Sedanes)
    SearchTop,              // Top de resultados de búsqueda
    SimilarVehicles,        // "También te puede interesar"
    EmailNewsletter         // Email semanal
}
```

---

## 📊 Proceso PROMO-001: Boost de Listing

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: PROMO-001 - Boost de Listing                                  │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-SELLER, DLR-ADMIN                                 │
│ Sistemas: PromotionService, BillingService, VehiclesSaleService        │
│ Duración: Instantáneo                                                  │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                        | Sistema             | Actor      | Evidencia             | Código     |
| ---- | ------- | ----------------------------- | ------------------- | ---------- | --------------------- | ---------- |
| 1    | 1.1     | Usuario accede a su listing   | Frontend            | USR-SELLER | Listing accessed      | EVD-LOG    |
| 1    | 1.2     | Click "Promocionar"           | Frontend            | USR-SELLER | CTA clicked           | EVD-LOG    |
| 2    | 2.1     | GET /api/promotions/options   | Gateway             | USR-SELLER | Options loaded        | EVD-LOG    |
| 2    | 2.2     | Mostrar opciones de boost     | Frontend            | USR-SELLER | Options displayed     | EVD-SCREEN |
| 3    | 3.1     | Usuario selecciona plan       | Frontend            | USR-SELLER | Plan selected         | EVD-LOG    |
| 3    | 3.2     | Usuario activa auto-renovar   | Frontend            | USR-SELLER | Auto-renew toggled    | EVD-LOG    |
| 4    | 4.1     | POST /api/promotions/boost    | Gateway             | USR-SELLER | **Request**           | EVD-AUDIT  |
| 4    | 4.2     | Validar listing activo        | PromotionService    | Sistema    | Validation            | EVD-LOG    |
| 4    | 4.3     | Verificar no duplicado        | PromotionService    | Sistema    | Duplicate check       | EVD-LOG    |
| 5    | 5.1     | Calcular precio               | PromotionService    | Sistema    | Price calculated      | EVD-LOG    |
| 5    | 5.2     | Crear payment intent          | BillingService      | Sistema    | Payment intent        | EVD-LOG    |
| 6    | 6.1     | Usuario paga                  | Stripe/Azul         | USR-SELLER | **Payment**           | EVD-AUDIT  |
| 6    | 6.2     | Confirmar pago                | BillingService      | Sistema    | Payment confirmed     | EVD-EVENT  |
| 7    | 7.1     | **Crear Promotion**           | PromotionService    | Sistema    | **Promotion created** | EVD-AUDIT  |
| 7    | 7.2     | Capturar baseline analytics   | PromotionService    | Sistema    | Baseline captured     | EVD-SNAP   |
| 8    | 8.1     | **Aplicar boost al vehículo** | VehiclesSaleService | Sistema    | **Boost applied**     | EVD-EVENT  |
| 8    | 8.2     | Actualizar índice de búsqueda | VehiclesSaleService | Sistema    | Index updated         | EVD-LOG    |
| 9    | 9.1     | Agregar badge "PROMOCIONADO"  | VehiclesSaleService | Sistema    | Badge added           | EVD-LOG    |
| 10   | 10.1    | **Notificar confirmación**    | NotificationService | SYS-NOTIF  | **Confirmation**      | EVD-COMM   |
| 11   | 11.1    | **Audit trail**               | AuditService        | Sistema    | Complete audit        | EVD-AUDIT  |

### [Expiración del Boost]

| Paso | Subpaso | Acción                          | Sistema             | Actor     | Evidencia             | Código    |
| ---- | ------- | ------------------------------- | ------------------- | --------- | --------------------- | --------- |
| 12   | 12.1    | Scheduler detecta expiración    | SYS-SCHEDULER       | Sistema   | Expiration check      | EVD-LOG   |
| 12   | 12.2    | Si auto-renew: crear nuevo pago | BillingService      | Sistema   | Renewal payment       | EVD-AUDIT |
| 12   | 12.3    | Si no auto-renew: desactivar    | PromotionService    | Sistema   | **Promotion expired** | EVD-EVENT |
| 13   | 13.1    | Calcular analytics finales      | PromotionService    | Sistema   | **Final analytics**   | EVD-AUDIT |
| 13   | 13.2    | Remover badge                   | VehiclesSaleService | Sistema   | Badge removed         | EVD-LOG   |
| 14   | 14.1    | Notificar resultados            | NotificationService | SYS-NOTIF | **Results sent**      | EVD-COMM  |

### Evidencia de Boost

```json
{
  "processCode": "PROMO-001",
  "promotion": {
    "id": "promo-12345",
    "vehicle": {
      "id": "veh-67890",
      "title": "Toyota Corolla 2023",
      "currentPrice": 1250000
    },
    "user": {
      "userId": "user-001",
      "type": "INDIVIDUAL"
    },
    "boost": {
      "type": "BOOST",
      "plan": "BOOST_7_DAYS",
      "duration": 7,
      "startDate": "2026-01-21T10:30:00Z",
      "endDate": "2026-01-28T10:30:00Z",
      "autoRenew": true
    },
    "payment": {
      "price": 1500,
      "currency": "DOP",
      "transactionId": "pay-12345"
    },
    "baseline": {
      "capturedAt": "2026-01-21T10:30:00Z",
      "last7Days": {
        "views": 45,
        "contacts": 3,
        "favorites": 8,
        "averagePosition": 25
      }
    },
    "effects": {
      "boostMultiplier": 2.5,
      "targetPosition": "TOP_10",
      "badge": "PROMOTED"
    }
  }
}
```

---

## 📊 Proceso PROMO-002: Listing Destacado

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: PROMO-002 - Listing Destacado                                 │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-SELLER, DLR-ADMIN                                 │
│ Sistemas: PromotionService, BillingService, HomepageService            │
│ Duración: Instantáneo → Duración del featured                          │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                   | Sistema             | Actor      | Evidencia             | Código     |
| ---- | ------- | ---------------------------------------- | ------------------- | ---------- | --------------------- | ---------- |
| 1    | 1.1     | Usuario accede a promociones             | Frontend            | USR-SELLER | Access                | EVD-LOG    |
| 1    | 1.2     | Selecciona "Destacar en Homepage"        | Frontend            | USR-SELLER | Feature selected      | EVD-LOG    |
| 2    | 2.1     | Verificar disponibilidad de slots        | PromotionService    | Sistema    | Slot check            | EVD-LOG    |
| 2    | 2.2     | Mostrar posiciones disponibles           | Frontend            | USR-SELLER | Positions shown       | EVD-SCREEN |
| 3    | 3.1     | Seleccionar ubicación                    | Frontend            | USR-SELLER | Location selected     | EVD-LOG    |
| 3    | 3.2     | Seleccionar fechas                       | Frontend            | USR-SELLER | Dates selected        | EVD-LOG    |
| 4    | 4.1     | POST /api/promotions/featured            | Gateway             | USR-SELLER | **Request**           | EVD-AUDIT  |
| 4    | 4.2     | Validar calidad del listing              | PromotionService    | Sistema    | **Quality check**     | EVD-AUDIT  |
| 4    | 4.3     | Requiere: 5+ fotos, descripción completa | PromotionService    | Sistema    | Requirements check    | EVD-LOG    |
| 5    | 5.1     | Reservar slot                            | PromotionService    | Sistema    | Slot reserved         | EVD-LOG    |
| 5    | 5.2     | Calcular precio (oferta/demanda)         | PromotionService    | Sistema    | Dynamic price         | EVD-LOG    |
| 6    | 6.1     | Crear payment intent                     | BillingService      | Sistema    | Payment intent        | EVD-LOG    |
| 6    | 6.2     | Usuario paga                             | Stripe/Azul         | USR-SELLER | **Payment**           | EVD-AUDIT  |
| 7    | 7.1     | **Crear Promotion**                      | PromotionService    | Sistema    | **Promotion created** | EVD-AUDIT  |
| 7    | 7.2     | **Crear FeaturedPlacement**              | PromotionService    | Sistema    | **Placement created** | EVD-AUDIT  |
| 8    | 8.1     | Agregar badge "DESTACADO"                | VehiclesSaleService | Sistema    | Badge added           | EVD-LOG    |
| 8    | 8.2     | Agregar a sección homepage               | HomepageService     | Sistema    | **Homepage updated**  | EVD-EVENT  |
| 9    | 9.1     | **Notificar confirmación**               | NotificationService | SYS-NOTIF  | **Confirmation**      | EVD-COMM   |
| 10   | 10.1    | **Audit trail**                          | AuditService        | Sistema    | Complete audit        | EVD-AUDIT  |

### Evidencia de Featured

```json
{
  "processCode": "PROMO-002",
  "featured": {
    "id": "featured-12345",
    "promotion": {
      "id": "promo-67890",
      "type": "FEATURED",
      "plan": "FEATURED_3_DAYS"
    },
    "vehicle": {
      "id": "veh-11111",
      "title": "BMW X5 2024",
      "price": 3500000,
      "qualityScore": 92,
      "imageCount": 12
    },
    "placement": {
      "location": "HOMEPAGE_CAROUSEL",
      "position": 3,
      "slot": "SLOT_3_JAN21_JAN24"
    },
    "schedule": {
      "startDate": "2026-01-21T00:00:00Z",
      "endDate": "2026-01-24T00:00:00Z",
      "timeZone": "America/Santo_Domingo"
    },
    "pricing": {
      "basePrice": 2500,
      "demandMultiplier": 1.2,
      "finalPrice": 3000,
      "currency": "DOP"
    },
    "requirements": {
      "minImages": 5,
      "actualImages": 12,
      "minDescLength": 100,
      "actualDescLength": 450,
      "passed": true
    }
  }
}
```

---

## 💰 Precios de Promociones

### Boost

| Plan           | Duración | Precio (DOP) | Efecto           |
| -------------- | -------- | ------------ | ---------------- |
| Boost Express  | 3 días   | RD$ 800      | 2x visibilidad   |
| Boost Standard | 7 días   | RD$ 1,500    | 2.5x visibilidad |
| Boost Pro      | 14 días  | RD$ 2,500    | 3x visibilidad   |
| Boost Max      | 30 días  | RD$ 4,000    | 3x + prioridad   |

### Featured (Homepage)

| Plan             | Duración | Precio Base | Ubicación          |
| ---------------- | -------- | ----------- | ------------------ |
| Featured Day     | 24 horas | RD$ 2,500   | Carrusel principal |
| Featured Weekend | 3 días   | RD$ 5,500   | Carrusel principal |
| Featured Week    | 7 días   | RD$ 10,000  | Carrusel principal |

_Precios varían según demanda (1.0x - 1.5x)_

### Spotlight

| Plan             | Duración | Precio    | Descripción      |
| ---------------- | -------- | --------- | ---------------- |
| Spotlight Banner | 24 horas | RD$ 7,500 | Banner hero top  |
| Category Top     | 7 días   | RD$ 3,500 | Top de categoría |

### Bundles

| Bundle             | Incluye                           | Precio     | Ahorro |
| ------------------ | --------------------------------- | ---------- | ------ |
| **Starter**        | Boost 7 + Featured 1 día          | RD$ 3,500  | 12%    |
| **Pro**            | Boost 14 + Featured 3 + Urgent    | RD$ 8,500  | 18%    |
| **Max Visibility** | Boost 30 + Featured 7 + Spotlight | RD$ 18,000 | 25%    |

---

## 📊 Algoritmo de Boost

```csharp
public class BoostAlgorithm
{
    public decimal CalculateBoostScore(Vehicle vehicle, Promotion promotion)
    {
        // Base score del vehículo
        var baseScore = vehicle.QualityScore;

        // Multiplicador según plan
        var boostMultiplier = promotion.Plan switch
        {
            PromotionPlan.Boost3Days => 2.0m,
            PromotionPlan.Boost7Days => 2.5m,
            PromotionPlan.Boost14Days => 3.0m,
            PromotionPlan.Boost30Days => 3.0m,
            _ => 1.0m
        };

        // Factores adicionales
        var recencyFactor = CalculateRecencyFactor(vehicle.CreatedAt);
        var priceFactor = CalculatePriceFactor(vehicle.Price);

        // Score final
        var boostedScore = baseScore * boostMultiplier * recencyFactor * priceFactor;

        // Decay suave hacia el final del boost
        if (promotion.Plan == PromotionPlan.Boost30Days)
        {
            var daysRemaining = (promotion.EndDate - DateTime.UtcNow).Days;
            if (daysRemaining < 7)
            {
                var decayFactor = 0.8m + (0.2m * daysRemaining / 7);
                boostedScore *= decayFactor;
            }
        }

        return boostedScore;
    }

    private decimal CalculateRecencyFactor(DateTime createdAt)
    {
        var daysOld = (DateTime.UtcNow - createdAt).Days;
        return daysOld switch
        {
            <= 7 => 1.2m,      // Nuevo = bonus
            <= 30 => 1.0m,     // Normal
            <= 60 => 0.9m,     // Algo viejo
            _ => 0.8m          // Viejo
        };
    }
}
```

---

## 📈 Analytics de Promoción

```json
{
  "promotionId": "promo-12345",
  "period": {
    "start": "2026-01-21",
    "end": "2026-01-28",
    "days": 7
  },
  "comparison": {
    "before": {
      "period": "2026-01-14 to 2026-01-20",
      "views": 45,
      "uniqueViewers": 38,
      "contacts": 3,
      "favorites": 8,
      "avgSearchPosition": 25,
      "impressions": 120
    },
    "during": {
      "period": "2026-01-21 to 2026-01-28",
      "views": 187,
      "uniqueViewers": 156,
      "contacts": 14,
      "favorites": 28,
      "avgSearchPosition": 4,
      "impressions": 890
    }
  },
  "increases": {
    "views": "+315%",
    "contacts": "+367%",
    "favorites": "+250%",
    "positionImprovement": "21 positions"
  },
  "roi": {
    "promotionCost": 1500,
    "costPerView": 8.02,
    "costPerContact": 107.14,
    "estimatedValuePerContact": 5000,
    "estimatedROI": "367%"
  },
  "insights": [
    "Tu vehículo recibió 4x más vistas que el promedio de la categoría",
    "Los contactos aumentaron principalmente los fines de semana",
    "Considera renovar el boost para mantener el momentum"
  ]
}
```

---

## 📊 Métricas Prometheus

```yaml
# Promociones
promotions_created_total{type, plan}
promotions_revenue_total{type}
promotions_active_count{type}

# Efectividad
promotions_views_increase_percent
promotions_contacts_increase_percent
promotions_roi_average

# Featured
featured_slots_available{location}
featured_occupancy_rate{location}

# Expiración
promotions_expired_total
promotions_renewed_total
promotions_renewal_rate
```

---

## 🔗 Referencias

- [03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md](../03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md)
- [05-PAGOS-FACTURACION/01-billing-service.md](../05-PAGOS-FACTURACION/01-billing-service.md)
- [10-ANALYTICS-REPORTES/01-analytics-service.md](../10-ANALYTICS-REPORTES/01-analytics-service.md)
