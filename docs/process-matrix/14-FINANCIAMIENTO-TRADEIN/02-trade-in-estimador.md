# 🔄 Trade-In y Estimador de Valor

> **Código:** TRADE-001, TRADE-002  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🔴 CRÍTICA (Diferenciador - Nadie en RD lo tiene)

---

## � Resumen de Implementación

| Componente    | Total | Implementado | Pendiente | Estado |
| ------------- | ----- | ------------ | --------- | ------ |
| Controllers   | 1     | 0            | 1         | 🔴     |
| TRADE-EST-\*  | 5     | 0            | 5         | 🔴     |
| TRADE-REQ-\*  | 4     | 0            | 4         | 🔴     |
| TRADE-VAL-\*  | 4     | 0            | 4         | 🔴     |
| TRADE-DEAL-\* | 3     | 0            | 3         | 🔴     |
| Tests         | 0     | 0            | 10        | 🔴     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## �📋 Información General

| Campo             | Valor                                       |
| ----------------- | ------------------------------------------- |
| **Servicio**      | TradeInService                              |
| **Puerto**        | 5081                                        |
| **Base de Datos** | `tradeinservice`                            |
| **Dependencias**  | VehiclesSaleService, MLService, UserService |
| **Integraciones** | INTRANT (verificación placa), Libro Azul RD |

---

## 🎯 Objetivo del Proceso

Permitir a los usuarios:

1. Obtener estimación del valor de su vehículo actual
2. Usar ese valor como parte de pago para otro vehículo
3. Conectar con dealers interesados en el trade-in

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       TradeInService Architecture                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   User Flow                          Core Service                            │
│   ┌────────────────┐              ┌─────────────────────────────────────┐   │
│   │ Estimate Form  │──┐           │           TradeInService             │   │
│   │ (Enter Details)│  │           │  ┌───────────────────────────────┐  │   │
│   └────────────────┘  │           │  │ Controllers                   │  │   │
│   ┌────────────────┐  │           │  │ • TradeInController           │  │   │
│   │ Scan Plate     │──┼──────────▶│  │ • ValuationController         │  │   │
│   │ (Mobile OCR)   │  │           │  │ • OffersController            │  │   │
│   └────────────────┘  │           │  └───────────────────────────────┘  │   │
│   ┌────────────────┐  │           │  ┌───────────────────────────────┐  │   │
│   │ Offers from    │──┘           │  │ ML Valuation Engine           │  │   │
│   │ Dealers        │              │  │ • Market data analysis        │  │   │
│   └────────────────┘              │  │ • Condition scoring           │  │   │
│                                   │  │ • Depreciation model          │  │   │
│   External APIs                   │  └───────────────────────────────┘  │   │
│   ┌────────────────┐              │  ┌───────────────────────────────┐  │   │
│   │ INTRANT (Placa)│─────────────▶│  │ Domain                        │  │   │
│   │ Libro Azul RD  │              │  │ • VehicleValuation            │  │   │
│   │ (Market Value) │              │  │ • TradeInOffer                │  │   │
│   └────────────────┘              │  │ • MarketData                  │  │   │
│                                   │  └───────────────────────────────┘  │   │
│                                   └─────────────────────────────────────┘   │
│   Dealer Flow                                      │                        │
│   ┌────────────────┐               ┌───────────────┼───────────────┐        │
│   │ Receive Offers │◀─────────    ▼               ▼               ▼        │
│   │ (Dashboard)    │       ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│   └────────────────┘       │ PostgreSQL │  │   Redis    │  │  RabbitMQ  │  │
│                            │ (Valuations│  │ (Market    │  │ (Offer    │  │
│                            │  Offers)   │  │  Cache)    │  │  Events)   │  │
│                            └────────────┘  └────────────┘  └────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Endpoints

| Método | Endpoint                          | Descripción                 | Auth |
| ------ | --------------------------------- | --------------------------- | ---- |
| `POST` | `/api/tradein/estimate`           | Estimar valor de vehículo   | ❌   |
| `GET`  | `/api/tradein/estimate/{id}`      | Ver estimación guardada     | ✅   |
| `POST` | `/api/tradein/offers`             | Crear oferta de trade-in    | ✅   |
| `GET`  | `/api/tradein/offers`             | Mis ofertas de trade-in     | ✅   |
| `POST` | `/api/tradein/offers/{id}/accept` | Aceptar oferta de dealer    | ✅   |
| `GET`  | `/api/tradein/market-value`       | Valor de mercado por modelo | ❌   |
| `POST` | `/api/tradein/scan-plate`         | Escanear placa (móvil)      | ❌   |

---

## 🗃️ Entidades

### VehicleValuation

```csharp
public class VehicleValuation
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }

    // Datos del vehículo a valorar
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Trim { get; set; }
    public int Mileage { get; set; }                 // Kilometraje
    public VehicleConditionScore Condition { get; set; }
    public FuelType FuelType { get; set; }
    public TransmissionType Transmission { get; set; }
    public string Color { get; set; }

    // Datos adicionales
    public bool HasAccidentHistory { get; set; }
    public int NumberOfOwners { get; set; }
    public bool ServiceHistoryAvailable { get; set; }
    public List<string> AdditionalFeatures { get; set; }

    // Resultado de valoración
    public decimal EstimatedValueLow { get; set; }   // Valor mínimo
    public decimal EstimatedValueMid { get; set; }   // Valor medio (recomendado)
    public decimal EstimatedValueHigh { get; set; }  // Valor máximo
    public decimal TradeInValue { get; set; }        // Valor para trade-in (85% del mid)

    // Desglose
    public decimal BaseValue { get; set; }           // Valor base del modelo
    public decimal MileageAdjustment { get; set; }   // Ajuste por km
    public decimal ConditionAdjustment { get; set; } // Ajuste por condición
    public decimal FeatureAdjustment { get; set; }   // Ajuste por extras

    // Metadata
    public ValuationSource Source { get; set; }      // ML, MarketData, Manual
    public decimal ConfidenceScore { get; set; }     // 0-100
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }          // Válido por 7 días
}

public enum VehicleConditionScore
{
    Excellent = 5,    // Como nuevo, sin detalles
    Good = 4,         // Buen estado, detalles menores
    Fair = 3,         // Estado regular, necesita reparaciones menores
    Poor = 2,         // Mal estado, necesita reparaciones
    Salvage = 1       // Rescate, daño significativo
}

public enum ValuationSource
{
    MLModel,          // Modelo de ML entrenado
    MarketData,       // Datos de mercado (ventas recientes)
    ManualAppraisal   // Tasación manual de experto
}
```

### TradeInOffer

```csharp
public class TradeInOffer
{
    public Guid Id { get; set; }
    public Guid ValuationId { get; set; }
    public VehicleValuation Valuation { get; set; }

    public Guid UserId { get; set; }
    public Guid TargetVehicleId { get; set; }        // Vehículo que quiere comprar
    public Guid DealerId { get; set; }               // Dealer del vehículo objetivo

    // Valores
    public decimal TradeInValueOffered { get; set; } // Lo que ofrece el usuario
    public decimal TargetVehiclePrice { get; set; }  // Precio del vehículo objetivo
    public decimal DifferenceAmount { get; set; }    // Diferencia a pagar

    // Respuesta del dealer
    public TradeInOfferStatus Status { get; set; }
    public decimal? DealerCounterOffer { get; set; } // Contraoferta del dealer
    public string DealerNotes { get; set; }
    public DateTime? DealerResponseAt { get; set; }

    // Tracking
    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime ExpiresAt { get; set; }          // Oferta válida por 48h
}

public enum TradeInOfferStatus
{
    Pending,          // Esperando respuesta del dealer
    CounterOffered,   // Dealer hizo contraoferta
    Accepted,         // Ambas partes aceptaron
    Rejected,         // Dealer rechazó
    Expired,          // Expiró sin respuesta
    Cancelled         // Usuario canceló
}
```

### MarketPriceData

```csharp
public class MarketPriceData
{
    public Guid Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Trim { get; set; }

    // Precios de mercado
    public decimal AverageListingPrice { get; set; }  // Precio promedio en listings
    public decimal AverageSoldPrice { get; set; }     // Precio promedio vendido
    public decimal LowestPrice { get; set; }
    public decimal HighestPrice { get; set; }

    // Estadísticas
    public int ActiveListings { get; set; }           // Cuántos hay en venta
    public int SoldLast30Days { get; set; }           // Vendidos último mes
    public int AverageDaysOnMarket { get; set; }      // Días promedio para vender

    // Tendencia
    public decimal PriceChangePercent { get; set; }   // Cambio % último mes
    public PriceTrend Trend { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public enum PriceTrend
{
    Rising,
    Stable,
    Declining
}
```

---

## 📊 Proceso TRADE-001: Estimar Valor de Vehículo

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: TRADE-001 - Estimar Valor de Vehículo                         │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-ANON, USR-REG                                     │
│ Sistemas: TradeInService, MLService, VehiclesSaleService               │
│ Duración: 2-5 segundos                                                 │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                        | Sistema          | Actor    | Evidencia           | Código    |
| ---- | ------- | ----------------------------- | ---------------- | -------- | ------------------- | --------- |
| 1    | 1.1     | Usuario selecciona marca      | Frontend         | USR-ANON | Selection           | EVD-LOG   |
| 1    | 1.2     | Usuario selecciona modelo     | Frontend         | USR-ANON | Selection           | EVD-LOG   |
| 1    | 1.3     | Usuario selecciona año        | Frontend         | USR-ANON | Selection           | EVD-LOG   |
| 1    | 1.4     | Usuario ingresa kilometraje   | Frontend         | USR-ANON | Input               | EVD-LOG   |
| 1    | 1.5     | Usuario selecciona condición  | Frontend         | USR-ANON | Condition           | EVD-LOG   |
| 2    | 2.1     | POST /api/tradein/estimate    | Gateway          | USR-ANON | **Request**         | EVD-AUDIT |
| 2    | 2.2     | Validar datos                 | TradeInService   | Sistema  | Validation          | EVD-LOG   |
| 3    | 3.1     | Obtener precio base           | TradeInService   | Sistema  | Base price          | EVD-LOG   |
| 3    | 3.2     | Obtener datos de mercado      | TradeInService   | Sistema  | Market data         | EVD-LOG   |
| 4    | 4.1     | **Invocar modelo ML**         | MLService        | Sistema  | **ML prediction**   | EVD-AUDIT |
| 4    | 4.2     | Calcular ajuste por km        | MLService        | Sistema  | Mileage adj         | EVD-LOG   |
| 4    | 4.3     | Calcular ajuste por condición | MLService        | Sistema  | Condition adj       | EVD-LOG   |
| 4    | 4.4     | Calcular ajuste por extras    | MLService        | Sistema  | Feature adj         | EVD-LOG   |
| 5    | 5.1     | **Calcular rango de valores** | TradeInService   | Sistema  | **Value range**     | EVD-AUDIT |
| 5    | 5.2     | Calcular valor trade-in       | TradeInService   | Sistema  | Trade-in value      | EVD-LOG   |
| 6    | 6.1     | **Guardar valuación**         | TradeInService   | Sistema  | **Valuation saved** | EVD-AUDIT |
| 6    | 6.2     | Si usuario logueado: asociar  | TradeInService   | Sistema  | User linked         | EVD-LOG   |
| 7    | 7.1     | Retornar resultado            | TradeInService   | Sistema  | Response            | EVD-LOG   |
| 7    | 7.2     | Track para analytics          | AnalyticsService | Sistema  | Event               | EVD-EVENT |

### Algoritmo de Valoración

```csharp
public class ValuationAlgorithm
{
    public VehicleValuation Calculate(ValuationRequest request)
    {
        // 1. Obtener precio base del modelo
        var basePrice = _marketData.GetBasePrice(
            request.Make, request.Model, request.Year, request.Trim);

        // 2. Ajuste por kilometraje
        // Promedio: 15,000 km/año
        var expectedMileage = (DateTime.Now.Year - request.Year) * 15000;
        var mileageDiff = request.Mileage - expectedMileage;
        var mileageAdjustment = CalculateMileageAdjustment(mileageDiff, basePrice);

        // 3. Ajuste por condición
        var conditionMultiplier = request.Condition switch
        {
            VehicleConditionScore.Excellent => 1.05m,
            VehicleConditionScore.Good => 1.00m,
            VehicleConditionScore.Fair => 0.90m,
            VehicleConditionScore.Poor => 0.75m,
            VehicleConditionScore.Salvage => 0.50m,
            _ => 1.00m
        };

        // 4. Ajuste por historial de accidentes
        var accidentAdjustment = request.HasAccidentHistory ? -0.10m : 0m;

        // 5. Ajuste por features adicionales
        var featureBonus = CalculateFeatureBonus(request.AdditionalFeatures);

        // 6. Calcular valor final
        var adjustedValue = basePrice + mileageAdjustment;
        adjustedValue *= conditionMultiplier;
        adjustedValue *= (1 + accidentAdjustment);
        adjustedValue += featureBonus;

        // 7. Crear rango (±10%)
        return new VehicleValuation
        {
            BaseValue = basePrice,
            MileageAdjustment = mileageAdjustment,
            ConditionAdjustment = (conditionMultiplier - 1) * basePrice,
            FeatureAdjustment = featureBonus,
            EstimatedValueLow = adjustedValue * 0.90m,
            EstimatedValueMid = adjustedValue,
            EstimatedValueHigh = adjustedValue * 1.10m,
            TradeInValue = adjustedValue * 0.85m,  // Trade-in es 85% del valor
            ConfidenceScore = CalculateConfidence(request)
        };
    }

    private decimal CalculateMileageAdjustment(int mileageDiff, decimal basePrice)
    {
        // Cada 10,000 km extra = -3% del valor
        // Cada 10,000 km menos = +2% del valor
        var adjustment = mileageDiff switch
        {
            < -30000 => 0.06m,   // Muy bajo km
            < -15000 => 0.04m,   // Bajo km
            < 0 => 0.02m,        // Ligeramente bajo
            < 15000 => 0m,       // Normal
            < 30000 => -0.03m,   // Ligeramente alto
            < 50000 => -0.06m,   // Alto
            _ => -0.10m          // Muy alto
        };

        return basePrice * adjustment;
    }
}
```

### Evidencia de Valoración

```json
{
  "processCode": "TRADE-001",
  "valuation": {
    "id": "val-12345",
    "vehicle": {
      "make": "Honda",
      "model": "Accord",
      "year": 2021,
      "trim": "Sport",
      "mileage": 45000,
      "condition": "Good",
      "fuelType": "Gasoline",
      "transmission": "Automatic",
      "hasAccidentHistory": false,
      "additionalFeatures": ["Sunroof", "Leather", "Navigation"]
    },
    "calculation": {
      "baseValue": 1350000,
      "adjustments": {
        "mileage": -40500,
        "condition": 0,
        "features": 45000,
        "accident": 0
      },
      "totalAdjustment": 4500
    },
    "result": {
      "estimatedValueLow": 1219050,
      "estimatedValueMid": 1354500,
      "estimatedValueHigh": 1489950,
      "tradeInValue": 1151325
    },
    "marketContext": {
      "activeListings": 23,
      "averageListingPrice": 1420000,
      "averageDaysOnMarket": 18,
      "pricetrend": "STABLE"
    },
    "confidence": {
      "score": 87,
      "factors": [
        { "factor": "Market data availability", "score": 95 },
        { "factor": "Model popularity", "score": 90 },
        { "factor": "Condition accuracy", "score": 75 }
      ]
    },
    "validity": {
      "createdAt": "2026-01-21T10:30:00Z",
      "expiresAt": "2026-01-28T10:30:00Z"
    }
  }
}
```

---

## 📊 Proceso TRADE-002: Crear Oferta de Trade-In

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: TRADE-002 - Crear Oferta de Trade-In                          │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG (requiere cuenta)                             │
│ Sistemas: TradeInService, VehiclesSaleService, NotificationService     │
│ Duración: Instantáneo → 24-48h respuesta dealer                        │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                          | Sistema             | Actor     | Evidencia             | Código    |
| ---- | ------- | ------------------------------- | ------------------- | --------- | --------------------- | --------- |
| 1    | 1.1     | Usuario tiene valoración        | Frontend            | USR-REG   | Valuation exists      | EVD-LOG   |
| 1    | 1.2     | Usuario ve vehículo objetivo    | Frontend            | USR-REG   | Vehicle viewed        | EVD-LOG   |
| 1    | 1.3     | Click "Usar como parte de pago" | Frontend            | USR-REG   | CTA clicked           | EVD-LOG   |
| 2    | 2.1     | POST /api/tradein/offers        | Gateway             | USR-REG   | **Request**           | EVD-AUDIT |
| 2    | 2.2     | Verificar valuación válida      | TradeInService      | Sistema   | Valuation check       | EVD-LOG   |
| 2    | 2.3     | Verificar vehículo disponible   | VehiclesSaleService | Sistema   | Vehicle check         | EVD-LOG   |
| 3    | 3.1     | Calcular diferencia a pagar     | TradeInService      | Sistema   | Difference calc       | EVD-LOG   |
| 3    | 3.2     | **Crear TradeInOffer**          | TradeInService      | Sistema   | **Offer created**     | EVD-EVENT |
| 3    | 3.3     | Snapshot de términos            | TradeInService      | Sistema   | Terms snapshot        | EVD-SNAP  |
| 4    | 4.1     | **Notificar al dealer**         | NotificationService | SYS-NOTIF | **Dealer notified**   | EVD-COMM  |
| 4    | 4.2     | Email con detalles              | NotificationService | SYS-NOTIF | Email sent            | EVD-COMM  |
| 4    | 4.3     | Push/SMS al dealer              | NotificationService | SYS-NOTIF | Push/SMS sent         | EVD-COMM  |
| 5    | 5.1     | Confirmar al usuario            | NotificationService | SYS-NOTIF | **User confirmation** | EVD-COMM  |
| 6    | 6.1     | **Audit trail**                 | AuditService        | Sistema   | Complete audit        | EVD-AUDIT |

### [Dealer Response Flow]

| Paso | Subpaso | Acción                                    | Sistema             | Actor     | Evidencia               | Código    |
| ---- | ------- | ----------------------------------------- | ------------------- | --------- | ----------------------- | --------- |
| 7    | 7.1     | Dealer accede a oferta                    | Frontend            | DLR-STAFF | Access log              | EVD-LOG   |
| 7    | 7.2     | Dealer revisa valuación                   | Frontend            | DLR-STAFF | Review log              | EVD-AUDIT |
| 8    | 8.1     | Dealer decide                             | Frontend            | DLR-STAFF | Decision                | EVD-LOG   |
| 8    | 8.2     | Si acepta: PUT /offers/{id}/accept        | TradeInService      | DLR-STAFF | **Acceptance**          | EVD-AUDIT |
| 8    | 8.3     | Si contraoferta: PUT /offers/{id}/counter | TradeInService      | DLR-STAFF | **Counter offer**       | EVD-AUDIT |
| 8    | 8.4     | Si rechaza: PUT /offers/{id}/reject       | TradeInService      | DLR-STAFF | **Rejection**           | EVD-AUDIT |
| 9    | 9.1     | **Notificar resultado**                   | NotificationService | SYS-NOTIF | **Result notification** | EVD-COMM  |
| 10   | 10.1    | Si aceptado: crear lead                   | CRMService          | Sistema   | **Lead created**        | EVD-EVENT |
| 10   | 10.2    | Agendar inspección                        | AppointmentService  | Sistema   | Appointment             | EVD-EVENT |

### Evidencia de Oferta de Trade-In

```json
{
  "processCode": "TRADE-002",
  "offer": {
    "id": "offer-12345",
    "status": "PENDING",
    "user": {
      "id": "user-001",
      "name": "María García",
      "phone": "+18095559876"
    },
    "tradeInVehicle": {
      "valuationId": "val-12345",
      "make": "Honda",
      "model": "Accord",
      "year": 2021,
      "tradeInValue": 1151325
    },
    "targetVehicle": {
      "id": "veh-67890",
      "make": "Toyota",
      "model": "Camry",
      "year": 2024,
      "price": 2100000,
      "dealer": {
        "id": "dlr-001",
        "name": "Toyota Santo Domingo"
      }
    },
    "calculation": {
      "targetPrice": 2100000,
      "tradeInCredit": 1151325,
      "differenceToPay": 948675
    },
    "timeline": {
      "createdAt": "2026-01-21T10:30:00Z",
      "expiresAt": "2026-01-23T10:30:00Z",
      "dealerNotifiedAt": "2026-01-21T10:30:01Z"
    },
    "notifications": {
      "dealer": {
        "email": true,
        "push": true,
        "sms": true
      },
      "user": {
        "email": true,
        "push": true
      }
    }
  }
}
```

---

## 📊 Proceso TRADE-003: Escaneo de Placa (Móvil)

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: TRADE-003 - Escaneo de Placa                                  │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-ANON, USR-REG (desde app móvil)                   │
│ Sistemas: TradeInService, MLService (OCR), INTRANT API                 │
│ Duración: 3-5 segundos                                                 │
│ Criticidad: MEDIA (Diferenciador UX)                                   │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                        | Sistema        | Actor   | Evidencia         | Código    |
| ---- | ------- | ----------------------------- | -------------- | ------- | ----------------- | --------- |
| 1    | 1.1     | Usuario abre cámara           | Mobile App     | USR-REG | Camera opened     | EVD-LOG   |
| 1    | 1.2     | Apunta a placa del vehículo   | Mobile App     | USR-REG | Frame captured    | EVD-LOG   |
| 2    | 2.1     | Capturar imagen               | Mobile App     | Sistema | Image captured    | EVD-FILE  |
| 2    | 2.2     | POST /api/tradein/scan-plate  | Gateway        | USR-REG | **Request**       | EVD-AUDIT |
| 3    | 3.1     | **OCR para extraer placa**    | MLService      | Sistema | **OCR result**    | EVD-AUDIT |
| 3    | 3.2     | Validar formato placa RD      | TradeInService | Sistema | Format validation | EVD-LOG   |
| 4    | 4.1     | **Consultar INTRANT**         | TradeInService | Sistema | **INTRANT query** | EVD-AUDIT |
| 4    | 4.2     | Obtener datos del vehículo    | TradeInService | Sistema | Vehicle data      | EVD-LOG   |
| 5    | 5.1     | Mapear a modelo interno       | TradeInService | Sistema | Data mapped       | EVD-LOG   |
| 5    | 5.2     | Retornar datos prellenados    | TradeInService | Sistema | Response          | EVD-LOG   |
| 6    | 6.1     | Usuario confirma/ajusta datos | Mobile App     | USR-REG | User confirmation | EVD-LOG   |
| 6    | 6.2     | Continuar con valoración      | TradeInService | USR-REG | → TRADE-001       | EVD-LOG   |

### Evidencia de Escaneo de Placa

```json
{
  "processCode": "TRADE-003",
  "plateScan": {
    "id": "scan-12345",
    "ocr": {
      "rawImage": "s3://media/scans/scan-12345.jpg",
      "extractedPlate": "A123456",
      "confidence": 0.95,
      "processingTime": "0.8s"
    },
    "intrantData": {
      "plate": "A123456",
      "vin": "1HGBH41JXMN109186",
      "make": "Honda",
      "model": "Civic",
      "year": 2020,
      "color": "Blanco",
      "fuelType": "Gasolina",
      "cylinderCapacity": 2000,
      "registrationStatus": "VIGENTE",
      "lastInspection": "2025-08-15",
      "owner": "[REDACTED]"
    },
    "mappedData": {
      "make": "Honda",
      "model": "Civic",
      "year": 2020,
      "trim": "LX",
      "suggestedCondition": "Good"
    },
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

## 🔗 Integración con INTRANT

```csharp
public class IntrantService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public async Task<IntrantVehicleData?> GetVehicleByPlate(string plate)
    {
        // Llamada a API de INTRANT (ficticia - adaptar al API real)
        var response = await _httpClient.GetAsync(
            $"https://api.intrant.gob.do/v1/vehicles/{plate}?apiKey={_apiKey}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<IntrantVehicleData>();
    }
}

public class IntrantVehicleData
{
    public string Plate { get; set; }
    public string Vin { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
    public string FuelType { get; set; }
    public int CylinderCapacity { get; set; }
    public string RegistrationStatus { get; set; }
    public DateTime? LastInspection { get; set; }
}
```

---

## 📊 Métricas Prometheus

```yaml
# Valuaciones
tradein_valuations_total{source, confidence_bucket}
tradein_valuation_processing_time_seconds

# Ofertas
tradein_offers_total{status}
tradein_offer_response_time_hours{dealer}
tradein_offer_acceptance_rate{dealer}

# Conversión
tradein_valuation_to_offer_rate
tradein_offer_to_sale_rate

# Escaneo de placas
tradein_plate_scans_total{success}
tradein_ocr_accuracy
```

---

## 🔗 Referencias

- [14-FINANCIAMIENTO-TRADEIN/01-calculadora-financiamiento.md](01-calculadora-financiamiento.md)
- [03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md](../03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md)
- [06-CRM-LEADS-CONTACTOS/01-crm-service.md](../06-CRM-LEADS-CONTACTOS/01-crm-service.md)
- [INTRANT Portal](https://www.intrant.gob.do)
