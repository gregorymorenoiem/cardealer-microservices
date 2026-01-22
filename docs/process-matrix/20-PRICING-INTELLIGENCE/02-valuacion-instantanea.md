# 💰 Valuación Instantánea (Instant Market Value)

> **Código:** VALUE-001  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🔴 ALTA (Captación de vendedores)  
> **Origen:** CarGurus, Kavak

---

## 📋 Información General

| Campo             | Valor                            |
| ----------------- | -------------------------------- |
| **Servicio**      | PricingIntelligenceService       |
| **Puerto**        | 5090                             |
| **Base de Datos** | `pricingintelligenceservice`     |
| **Dependencias**  | VehiclesSaleService, UserService |

---

## 🎯 Objetivo del Proceso

1. **Captación de vendedores:** "¿Cuánto vale mi carro?" atrae tráfico
2. **Lead generation:** Capturar email/teléfono para contactar
3. **Confianza:** Usuario ve que OKLA sabe del mercado
4. **Conversión:** De valuación a publicación

---

## 📡 Endpoints

| Método | Endpoint                                     | Descripción                   | Auth |
| ------ | -------------------------------------------- | ----------------------------- | ---- |
| `POST` | `/api/valuation/instant`                     | Obtener valuación instantánea | ❌   |
| `GET`  | `/api/valuation/{id}`                        | Recuperar valuación guardada  | ❌   |
| `POST` | `/api/valuation/{id}/claim`                  | Reclamar con email            | ❌   |
| `GET`  | `/api/valuation/makes`                       | Marcas disponibles            | ❌   |
| `GET`  | `/api/valuation/models/{make}`               | Modelos por marca             | ❌   |
| `GET`  | `/api/valuation/trims/{make}/{model}/{year}` | Versiones                     | ❌   |

---

## 🗃️ Entidades

### ValuationRequest

```csharp
public class ValuationRequest
{
    public Guid Id { get; set; }

    // Vehículo
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Trim { get; set; }
    public int Mileage { get; set; }
    public string Condition { get; set; }  // Excellent, Good, Fair, Poor
    public string ExteriorColor { get; set; }
    public string InteriorColor { get; set; }
    public string Transmission { get; set; }
    public string FuelType { get; set; }
    public List<string> Features { get; set; }

    // Historial
    public bool HasAccidentHistory { get; set; }
    public int NumberOfOwners { get; set; }
    public bool HasServiceRecords { get; set; }

    // Ubicación
    public string City { get; set; }
    public string Province { get; set; }

    // Resultado
    public ValuationResult Result { get; set; }

    // Lead capture
    public string Email { get; set; }
    public string Phone { get; set; }
    public bool ConsentToContact { get; set; }
    public bool WantsToSell { get; set; }

    // Tracking
    public string SessionId { get; set; }
    public string Source { get; set; }  // homepage, google, facebook
    public string UtmCampaign { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ClaimedAt { get; set; }
}

public class ValuationResult
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }

    // Valores
    public decimal TradeInValue { get; set; }      // Valor trade-in (bajo)
    public decimal PrivatePartyValue { get; set; } // Venta entre particulares
    public decimal DealerRetailValue { get; set; } // Precio en dealer (alto)

    // Rangos
    public decimal ValueRangeLow { get; set; }
    public decimal ValueRangeHigh { get; set; }

    // Contexto
    public int ComparablesCount { get; set; }
    public List<ValuationComparable> Comparables { get; set; }

    // Factores
    public List<ValuationFactor> PositiveFactors { get; set; }
    public List<ValuationFactor> NegativeFactors { get; set; }

    // Tendencia
    public decimal PriceChange30Days { get; set; }
    public string TrendDirection { get; set; }  // up, down, stable

    // Confidence
    public int ConfidenceScore { get; set; }
    public string ConfidenceLevel { get; set; }

    public DateTime CalculatedAt { get; set; }
    public DateTime ValidUntil { get; set; }  // 7 días
}

public class ValuationFactor
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Impact { get; set; }
    public FactorType Type { get; set; }  // Positive, Negative
}
```

---

## 📊 Proceso VALUE-001: Valuación Instantánea

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: VALUE-001 - Calcular Valuación Instantánea                    │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-ANON o USR-REG                                    │
│ Sistemas: PricingIntelligenceService, VehiclesSaleService              │
│ Duración: 30-60 segundos                                               │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                           | Sistema                    | Actor    | Evidencia              | Código     |
| ---- | ------- | ------------------------------------------------ | -------------------------- | -------- | ---------------------- | ---------- |
| 1    | 1.1     | Usuario accede a /vender o /cuanto-vale-mi-carro | Frontend                   | USR-ANON | Page view              | EVD-LOG    |
| 1    | 1.2     | Mostrar formulario de valuación                  | Frontend                   | Sistema  | Form shown             | EVD-SCREEN |
| 2    | 2.1     | **Seleccionar marca**                            | Frontend                   | Usuario  | **Make selected**      | EVD-LOG    |
| 2    | 2.2     | GET /api/valuation/models/{make}                 | Gateway                    | Usuario  | Models loaded          | EVD-LOG    |
| 2    | 2.3     | **Seleccionar modelo**                           | Frontend                   | Usuario  | **Model selected**     | EVD-LOG    |
| 2    | 2.4     | **Seleccionar año**                              | Frontend                   | Usuario  | **Year selected**      | EVD-LOG    |
| 2    | 2.5     | GET /api/valuation/trims                         | Gateway                    | Usuario  | Trims loaded           | EVD-LOG    |
| 2    | 2.6     | **Seleccionar versión**                          | Frontend                   | Usuario  | **Trim selected**      | EVD-LOG    |
| 3    | 3.1     | **Ingresar kilometraje**                         | Frontend                   | Usuario  | **Mileage entered**    | EVD-LOG    |
| 3    | 3.2     | **Seleccionar condición**                        | Frontend                   | Usuario  | **Condition selected** | EVD-LOG    |
| 3    | 3.3     | Responder preguntas adicionales                  | Frontend                   | Usuario  | Answers                | EVD-LOG    |
| 4    | 4.1     | Click "Ver mi valuación"                         | Frontend                   | Usuario  | CTA clicked            | EVD-LOG    |
| 4    | 4.2     | **POST /api/valuation/instant**                  | Gateway                    | Usuario  | **Request**            | EVD-AUDIT  |
| 5    | 5.1     | **Crear ValuationRequest**                       | PricingIntelligenceService | Sistema  | **Request created**    | EVD-AUDIT  |
| 5    | 5.2     | **Buscar comparables**                           | PricingIntelligenceService | Sistema  | **Comparables found**  | EVD-LOG    |
| 5    | 5.3     | Filtrar por criterios                            | Sistema                    | Sistema  | Filtering              | EVD-LOG    |
| 6    | 6.1     | **Calcular valores**                             | PricingIntelligenceService | Sistema  | **Values calculated**  | EVD-AUDIT  |
| 6    | 6.2     | Trade-in value (más bajo)                        | Sistema                    | Sistema  | Trade-in calc          | EVD-LOG    |
| 6    | 6.3     | Private party value (medio)                      | Sistema                    | Sistema  | Private calc           | EVD-LOG    |
| 6    | 6.4     | Dealer retail value (más alto)                   | Sistema                    | Sistema  | Dealer calc            | EVD-LOG    |
| 7    | 7.1     | **Aplicar ajustes por factores**                 | PricingIntelligenceService | Sistema  | **Adjustments**        | EVD-LOG    |
| 7    | 7.2     | Condición, km, historial, features               | Sistema                    | Sistema  | Factors applied        | EVD-LOG    |
| 8    | 8.1     | **Calcular rango**                               | PricingIntelligenceService | Sistema  | **Range calc**         | EVD-LOG    |
| 8    | 8.2     | Low = TradeIn - 10%, High = Dealer + 10%         | Sistema                    | Sistema  | Range set              | EVD-LOG    |
| 9    | 9.1     | **Guardar ValuationResult**                      | PricingIntelligenceService | Sistema  | **Result saved**       | EVD-AUDIT  |
| 10   | 10.1    | **Mostrar resultado**                            | Frontend                   | Sistema  | **Result shown**       | EVD-SCREEN |
| 10   | 10.2    | Mostrar gráfico de rango                         | Frontend                   | Sistema  | Chart shown            | EVD-LOG    |
| 10   | 10.3    | Mostrar factores positivos/negativos             | Frontend                   | Sistema  | Factors shown          | EVD-LOG    |
| 11   | 11.1    | **Solicitar email para guardar**                 | Frontend                   | Sistema  | **Email prompt**       | EVD-SCREEN |
| 11   | 11.2    | POST /api/valuation/{id}/claim                   | Gateway                    | Usuario  | Claim request          | EVD-LOG    |
| 11   | 11.3    | **Lead capturado**                               | PricingIntelligenceService | Sistema  | **Lead captured**      | EVD-AUDIT  |
| 12   | 12.1    | CTA "Publica tu vehículo ahora"                  | Frontend                   | Sistema  | CTA shown              | EVD-LOG    |
| 12   | 12.2    | Redirigir a crear listing                        | Frontend                   | Usuario  | Redirect               | EVD-LOG    |
| 13   | 13.1    | **Audit trail**                                  | AuditService               | Sistema  | Complete audit         | EVD-AUDIT  |

### Evidencia de Valuación

```json
{
  "processCode": "VALUE-001",
  "valuation": {
    "requestId": "val-12345",
    "vehicle": {
      "make": "Toyota",
      "model": "Corolla",
      "year": 2021,
      "trim": "LE",
      "mileage": 45000,
      "condition": "Good",
      "transmission": "Automatic",
      "fuelType": "Gasoline",
      "hasAccidentHistory": false,
      "numberOfOwners": 1
    },
    "location": {
      "city": "Santo Domingo",
      "province": "Distrito Nacional"
    },
    "result": {
      "tradeInValue": 980000,
      "privatePartyValue": 1150000,
      "dealerRetailValue": 1320000,
      "valueRangeLow": 882000,
      "valueRangeHigh": 1452000,
      "recommendedListPrice": 1200000
    },
    "factors": {
      "positive": [
        {
          "name": "Un solo dueño",
          "description": "Vehículos con un solo dueño tienen más demanda",
          "impact": 40000
        },
        {
          "name": "Sin historial de accidentes",
          "description": "Historial limpio aumenta el valor",
          "impact": 60000
        },
        {
          "name": "Transmisión automática",
          "description": "Mayor demanda en el mercado RD",
          "impact": 25000
        }
      ],
      "negative": [
        {
          "name": "Kilometraje moderado",
          "description": "45,000 km está en el rango normal para un 2021",
          "impact": -15000
        }
      ]
    },
    "comparables": {
      "count": 18,
      "samples": [
        {
          "id": "veh-111",
          "title": "Toyota Corolla LE 2021",
          "price": 1180000,
          "mileage": 42000,
          "daysOnMarket": 12
        }
      ]
    },
    "market": {
      "trend": "stable",
      "priceChange30Days": -1.2,
      "demandLevel": "High"
    },
    "confidence": {
      "score": 85,
      "level": "High",
      "reason": "18 vehículos similares encontrados"
    },
    "calculatedAt": "2026-01-21T10:00:00Z",
    "validUntil": "2026-01-28T10:00:00Z"
  },
  "lead": {
    "captured": true,
    "email": "usuario@email.com",
    "wantsToSell": true,
    "source": "homepage",
    "capturedAt": "2026-01-21T10:02:00Z"
  }
}
```

---

## 📱 UI Mockup - Formulario

```
┌─────────────────────────────────────────────────────────────────────────┐
│                                                                         │
│  💰 ¿CUÁNTO VALE TU VEHÍCULO?                                          │
│  ════════════════════════════════════════════════════════════════════  │
│                                                                         │
│  Obtén una valuación instantánea basada en datos reales del mercado    │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                                                                 │   │
│  │  Marca *                        Modelo *                        │   │
│  │  ┌─────────────────────┐       ┌─────────────────────┐         │   │
│  │  │ Toyota           ▼  │       │ Corolla          ▼  │         │   │
│  │  └─────────────────────┘       └─────────────────────┘         │   │
│  │                                                                 │   │
│  │  Año *                          Versión                         │   │
│  │  ┌─────────────────────┐       ┌─────────────────────┐         │   │
│  │  │ 2021             ▼  │       │ LE               ▼  │         │   │
│  │  └─────────────────────┘       └─────────────────────┘         │   │
│  │                                                                 │   │
│  │  Kilometraje *                  Condición *                     │   │
│  │  ┌─────────────────────┐       ┌─────────────────────┐         │   │
│  │  │ 45,000 km           │       │ Buena            ▼  │         │   │
│  │  └─────────────────────┘       └─────────────────────┘         │   │
│  │                                                                 │   │
│  │  ¿Ha tenido accidentes?                                        │   │
│  │  ○ Sí   ● No                                                   │   │
│  │                                                                 │   │
│  │  ¿Cuántos dueños ha tenido?                                    │   │
│  │  ● 1   ○ 2   ○ 3+                                              │   │
│  │                                                                 │   │
│  │  Ciudad                                                         │   │
│  │  ┌─────────────────────┐                                       │   │
│  │  │ Santo Domingo    ▼  │                                       │   │
│  │  └─────────────────────┘                                       │   │
│  │                                                                 │   │
│  │          ┌─────────────────────────────────────┐               │   │
│  │          │    💰 VER MI VALUACIÓN              │               │   │
│  │          └─────────────────────────────────────┘               │   │
│  │                                                                 │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  🔒 Tu información es privada. No compartimos tus datos.               │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📱 UI Mockup - Resultado

```
┌─────────────────────────────────────────────────────────────────────────┐
│                                                                         │
│  ✅ ¡VALUACIÓN LISTA!                                                  │
│  ════════════════════════════════════════════════════════════════════  │
│                                                                         │
│  Tu Toyota Corolla LE 2021 con 45,000 km                               │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                                                                 │   │
│  │   VALOR ESTIMADO DE VENTA PARTICULAR                           │   │
│  │                                                                 │   │
│  │         RD$ 1,150,000                                          │   │
│  │         ═══════════════                                        │   │
│  │                                                                 │   │
│  │   Rango: RD$980,000 - RD$1,320,000                             │   │
│  │                                                                 │   │
│  │   ├─────────●────────────────────────────────┤                 │   │
│  │   Trade-In    Particular    Dealer                              │   │
│  │   $980K       $1.15M        $1.32M                              │   │
│  │                                                                 │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  📈 FACTORES QUE AFECTAN TU PRECIO                                     │
│                                                                         │
│  ✅ Un solo dueño                           +RD$40,000                 │
│  ✅ Sin historial de accidentes             +RD$60,000                 │
│  ✅ Transmisión automática                  +RD$25,000                 │
│  ⚠️ Kilometraje moderado                    -RD$15,000                 │
│                                                                         │
│  💡 RECOMENDACIÓN                                                      │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  Precio sugerido de publicación: RD$ 1,200,000                 │   │
│  │  Este precio te ayudará a vender rápido y obtener buen valor   │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│          ┌─────────────────────────────────────┐                       │
│          │    🚗 PUBLICAR MI VEHÍCULO AHORA    │                       │
│          └─────────────────────────────────────┘                       │
│                                                                         │
│  📧 ¿Quieres guardar esta valuación?                                   │
│  ┌─────────────────────────────┐  ┌───────────────┐                   │
│  │ tu@email.com                │  │ GUARDAR       │                   │
│  └─────────────────────────────┘  └───────────────┘                   │
│                                                                         │
│  ℹ️ Basado en 18 vehículos similares · Válido por 7 días              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Métricas Prometheus

```yaml
# Valuaciones
valuation_requests_total
valuation_completed_total
valuation_abandoned_total{step}
valuation_duration_seconds

# Leads
valuation_leads_captured_total
valuation_lead_to_listing_rate
valuation_lead_contacted_total

# Conversión
valuation_to_listing_rate
valuation_to_sale_rate

# Datos
comparables_found_avg
confidence_score_avg
```

---

## 🔗 Referencias

- [00-ANALISIS-COMPETITIVO.md](../00-ANALISIS-COMPETITIVO.md)
- [20-PRICING-INTELLIGENCE/01-deal-rating.md](01-deal-rating.md)
- [14-FINANCIAMIENTO-TRADEIN/02-trade-in-estimador.md](../14-FINANCIAMIENTO-TRADEIN/02-trade-in-estimador.md)
