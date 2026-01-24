# 🧮 Calculadora de Costos Totales

> **Código:** FIN-004, FIN-005  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🟢 ALTA (Diferenciador competitivo)

---

## � Resumen de Implementación

| Componente   | Total | Implementado | Pendiente | Estado |
| ------------ | ----- | ------------ | --------- | ------ |
| Controllers  | 1     | 0            | 1         | 🔴     |
| COST-CALC-\* | 5     | 0            | 5         | 🔴     |
| COST-TAX-\*  | 4     | 0            | 4         | 🔴     |
| COST-FEE-\*  | 3     | 0            | 3         | 🔴     |
| COST-INS-\*  | 3     | 0            | 3         | 🔴     |
| Tests        | 0     | 0            | 10        | 🔴     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## �📋 Información General

| Campo             | Valor                                      |
| ----------------- | ------------------------------------------ |
| **Servicio**      | FinancingService                           |
| **Puerto**        | 5080                                       |
| **Base de Datos** | `financingservice`                         |
| **Dependencias**  | VehiclesSaleService, DGII API, INTRANT API |

---

## 🎯 Objetivo del Proceso

1. **Transparencia total:** Mostrar TODOS los costos antes de comprar
2. **Evitar sorpresas:** Impuestos, transferencia, marbete, seguro
3. **Comparación justa:** Mismo criterio para todos los vehículos
4. **Diferenciación:** SuperCarros NO tiene esto

---

## 💰 Estructura de Costos RD (2026)

| Concepto                      | Cálculo                     | Quién Paga                    |
| ----------------------------- | --------------------------- | ----------------------------- |
| **Precio del Vehículo**       | Precio publicado            | Comprador                     |
| **ITBIS (si vehículo nuevo)** | 18% del precio              | Comprador                     |
| **Impuesto Primera Placa**    | 17% del valor CIF           | Comprador (nuevos importados) |
| **Transferencia DGII**        | 2% del precio               | Comprador                     |
| **Marbete Anual**             | RD$ 3,000 - 15,000 según CC | Comprador                     |
| **INTRANT (Traspaso)**        | RD$ 2,500                   | Comprador                     |
| **Peritaje/Inspección**       | RD$ 3,000 - 5,000           | Comprador                     |
| **Gestión Legal**             | RD$ 5,000 - 15,000          | Comprador                     |
| **Seguro Obligatorio**        | RD$ 1,500/año               | Comprador                     |
| **Seguro Full (opcional)**    | ~3-4% del valor             | Comprador                     |

---

## 📡 Endpoints

| Método | Endpoint                       | Descripción                 | Auth |
| ------ | ------------------------------ | --------------------------- | ---- |
| `POST` | `/api/financing/total-cost`    | Calcular costo total        | ❌   |
| `GET`  | `/api/financing/tax-rates`     | Tasas de impuestos vigentes | ❌   |
| `GET`  | `/api/financing/marbete-rates` | Tarifas de marbete por CC   | ❌   |

---

## 🗃️ Entidades

### TotalCostCalculation

```csharp
public class TotalCostCalculation
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid VehicleId { get; set; }

    // Datos del vehículo
    public string VehicleMake { get; set; }
    public string VehicleModel { get; set; }
    public int VehicleYear { get; set; }
    public decimal VehiclePrice { get; set; }
    public VehicleCondition Condition { get; set; }
    public int EngineCC { get; set; }
    public FuelType FuelType { get; set; }

    // Desglose de costos
    public CostBreakdown Breakdown { get; set; }

    // Totales
    public decimal TotalCost { get; set; }
    public decimal TotalTaxes { get; set; }
    public decimal TotalFees { get; set; }
    public decimal OptionalCosts { get; set; }

    // Con financiamiento
    public bool IncludeFinancing { get; set; }
    public decimal? MonthlyPayment { get; set; }
    public int? FinancingMonths { get; set; }
    public decimal? FinancingTotalCost { get; set; }

    public DateTime CalculatedAt { get; set; }
    public DateTime ValidUntil { get; set; }         // Válido 24 horas
}

public class CostBreakdown
{
    // Obligatorios
    public CostItem VehiclePrice { get; set; }
    public CostItem TransferTax { get; set; }        // 2% DGII
    public CostItem Marbete { get; set; }            // Según CC
    public CostItem IntrantFee { get; set; }         // RD$ 2,500
    public CostItem ObligatorySafety { get; set; }   // RD$ 1,500

    // Según condición
    public CostItem? ITBIS { get; set; }             // Solo nuevos
    public CostItem? FirstPlateTax { get; set; }     // Solo importados nuevos

    // Opcionales pero recomendados
    public CostItem? Inspection { get; set; }        // Peritaje
    public CostItem? LegalFees { get; set; }         // Gestión legal
    public CostItem? Insurance { get; set; }         // Seguro full

    // Lista para UI
    public List<CostItem> AllItems { get; set; }
}

public class CostItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Calculation { get; set; }          // "2% del precio"
    public CostCategory Category { get; set; }
    public bool IsRequired { get; set; }
    public bool IsIncluded { get; set; }
    public string LearnMoreUrl { get; set; }
}

public enum CostCategory
{
    Price,
    Tax,
    GovernmentFee,
    Insurance,
    Service,
    Optional
}

public enum VehicleCondition
{
    New,
    Used,
    Certified
}
```

### TaxRates

```csharp
public class TaxRates
{
    public Guid Id { get; set; }

    // Tasas vigentes
    public decimal ITBIS { get; set; }               // 18%
    public decimal TransferTax { get; set; }         // 2%
    public decimal FirstPlateTax { get; set; }       // 17%
    public decimal IntrantFee { get; set; }          // 2,500
    public decimal ObligatoryInsurance { get; set; } // 1,500

    // Marbete por CC
    public List<MarbeteRate> MarbeteRates { get; set; }

    // Validez
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveUntil { get; set; }
    public string Source { get; set; }               // "DGII Resolución XXX"

    public DateTime LastUpdated { get; set; }
}

public class MarbeteRate
{
    public int MinCC { get; set; }
    public int MaxCC { get; set; }
    public decimal Amount { get; set; }
}
```

---

## 📊 Proceso FIN-004: Calcular Costo Total

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: FIN-004 - Calcular Costo Total de Adquisición                 │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-ANON, USR-REG                                     │
│ Sistemas: FinancingService, VehiclesSaleService                        │
│ Duración: Instantáneo                                                  │
│ Criticidad: BAJA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                               | Sistema          | Actor    | Evidencia           | Código     |
| ---- | ------- | ---------------------------------------------------- | ---------------- | -------- | ------------------- | ---------- |
| 1    | 1.1     | Usuario ve listing de vehículo                       | Frontend         | USR-ANON | Listing viewed      | EVD-LOG    |
| 1    | 1.2     | Click "Ver Costo Total"                              | Frontend         | USR-ANON | CTA clicked         | EVD-LOG    |
| 2    | 2.1     | Modal/Page de calculadora                            | Frontend         | USR-ANON | Calculator opened   | EVD-SCREEN |
| 2    | 2.2     | Pre-llenar datos del vehículo                        | Frontend         | Sistema  | Data prefilled      | EVD-LOG    |
| 3    | 3.1     | POST /api/financing/total-cost                       | Gateway          | USR-ANON | **Request**         | EVD-LOG    |
| 3    | 3.2     | Obtener tasas vigentes                               | FinancingService | Sistema  | Rates fetched       | EVD-LOG    |
| 4    | 4.1     | Calcular Transferencia (2%)                          | FinancingService | Sistema  | Transfer calc       | EVD-LOG    |
| 4    | 4.2     | Calcular Marbete según CC                            | FinancingService | Sistema  | Marbete calc        | EVD-LOG    |
| 4    | 4.3     | Si nuevo: calcular ITBIS (18%)                       | FinancingService | Sistema  | ITBIS calc          | EVD-LOG    |
| 4    | 4.4     | Agregar INTRANT (RD$ 2,500)                          | FinancingService | Sistema  | INTRANT added       | EVD-LOG    |
| 4    | 4.5     | Agregar Seguro Obligatorio                           | FinancingService | Sistema  | Insurance added     | EVD-LOG    |
| 5    | 5.1     | Calcular opcionales (inspección, legal, seguro full) | FinancingService | Sistema  | Optionals calc      | EVD-LOG    |
| 5    | 5.2     | Sumar totales                                        | FinancingService | Sistema  | Totals calc         | EVD-LOG    |
| 6    | 6.1     | Retornar TotalCostCalculation                        | FinancingService | Sistema  | Response sent       | EVD-LOG    |
| 7    | 7.1     | Mostrar desglose visual                              | Frontend         | USR-ANON | Breakdown displayed | EVD-SCREEN |
| 7    | 7.2     | Toggles para opcionales                              | Frontend         | USR-ANON | Options interactive | EVD-SCREEN |

### Evidencia de Cálculo

```json
{
  "processCode": "FIN-004",
  "calculation": {
    "id": "calc-12345",
    "vehicle": {
      "id": "veh-67890",
      "make": "Toyota",
      "model": "Corolla",
      "year": 2023,
      "condition": "USED",
      "engineCC": 1800,
      "price": 1250000
    },
    "breakdown": {
      "required": [
        {
          "name": "Precio del Vehículo",
          "amount": 1250000,
          "category": "PRICE",
          "isRequired": true
        },
        {
          "name": "Transferencia DGII",
          "amount": 25000,
          "calculation": "2% de RD$ 1,250,000",
          "category": "TAX",
          "isRequired": true,
          "learnMore": "https://dgii.gov.do/vehiculos"
        },
        {
          "name": "Marbete 2026",
          "amount": 6000,
          "calculation": "Motor 1501-2000cc",
          "category": "GOVERNMENT_FEE",
          "isRequired": true
        },
        {
          "name": "Traspaso INTRANT",
          "amount": 2500,
          "category": "GOVERNMENT_FEE",
          "isRequired": true
        },
        {
          "name": "Seguro Obligatorio",
          "amount": 1500,
          "calculation": "Anual",
          "category": "INSURANCE",
          "isRequired": true
        }
      ],
      "optional": [
        {
          "name": "Inspección/Peritaje",
          "amount": 3500,
          "category": "SERVICE",
          "isRequired": false,
          "isIncluded": true,
          "description": "Recomendado para vehículos usados"
        },
        {
          "name": "Gestión Legal",
          "amount": 8000,
          "category": "SERVICE",
          "isRequired": false,
          "isIncluded": true,
          "description": "Abogado para contrato y transferencia"
        },
        {
          "name": "Seguro Todo Riesgo (Anual)",
          "amount": 45000,
          "calculation": "~3.6% del valor",
          "category": "INSURANCE",
          "isRequired": false,
          "isIncluded": false
        }
      ]
    },
    "totals": {
      "vehiclePrice": 1250000,
      "requiredCosts": 35000,
      "includedOptionalCosts": 11500,
      "excludedOptionalCosts": 45000,
      "subtotalWithoutOptionalInsurance": 1296500,
      "grandTotalWithEverything": 1341500
    },
    "summary": {
      "headline": "RD$ 1,296,500",
      "subheadline": "Costo total para circular (sin seguro full)",
      "savings": "Ahorra RD$ 8,000 con OKLA Legal Services"
    },
    "validUntil": "2026-01-22T10:30:00Z",
    "disclaimer": "Cálculo estimado basado en tasas vigentes. Puede variar según municipio."
  }
}
```

---

## 📊 Proceso FIN-005: Calcular con Financiamiento

| Paso | Subpaso | Acción                                 | Sistema          | Actor    | Evidencia       | Código     |
| ---- | ------- | -------------------------------------- | ---------------- | -------- | --------------- | ---------- |
| 1    | 1.1     | Usuario tiene cálculo base             | Frontend         | USR-ANON | Base calc       | EVD-LOG    |
| 1    | 1.2     | Toggle "Incluir Financiamiento"        | Frontend         | USR-ANON | Financing on    | EVD-LOG    |
| 2    | 2.1     | Ingresar inicial (%)                   | Frontend         | USR-ANON | Down payment    | EVD-LOG    |
| 2    | 2.2     | Seleccionar plazo                      | Frontend         | USR-ANON | Term selected   | EVD-LOG    |
| 3    | 3.1     | POST /api/financing/calculate          | Gateway          | USR-ANON | **Request**     | EVD-LOG    |
| 3    | 3.2     | Calcular monto a financiar             | FinancingService | Sistema  | Amount calc     | EVD-LOG    |
| 3    | 3.3     | Agregar costos al préstamo             | FinancingService | Sistema  | Costs added     | EVD-LOG    |
| 4    | 4.1     | Calcular cuota mensual                 | FinancingService | Sistema  | Monthly calc    | EVD-LOG    |
| 4    | 4.2     | Calcular costo total del crédito       | FinancingService | Sistema  | Total credit    | EVD-LOG    |
| 5    | 5.1     | Mostrar tabla de amortización resumida | Frontend         | USR-ANON | Schedule shown  | EVD-SCREEN |
| 5    | 5.2     | Mostrar costo total con financiamiento | Frontend         | USR-ANON | Total displayed | EVD-SCREEN |

### Ejemplo con Financiamiento

```json
{
  "withFinancing": {
    "vehiclePrice": 1250000,
    "totalCostsToFinance": 1296500,
    "downPayment": {
      "percent": 20,
      "amount": 259300
    },
    "amountFinanced": 1037200,
    "term": 48,
    "interestRate": 12.5,
    "monthlyPayment": 27545,
    "totalInterest": 285960,
    "totalCost": {
      "downPayment": 259300,
      "totalPayments": 1322160,
      "grandTotal": 1581460
    },
    "comparison": {
      "cashPrice": 1296500,
      "financedPrice": 1581460,
      "difference": 284960,
      "percentMore": "21.9% más con financiamiento"
    }
  }
}
```

---

## 📊 UI Components

### Vista en Listing

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Toyota Corolla 2023 - RD$ 1,250,000                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   💰 Costo Total para Circular: RD$ 1,296,500                          │
│   ─────────────────────────────────────────────                        │
│   Precio vehículo           RD$ 1,250,000                              │
│   + Transferencia DGII         RD$ 25,000                              │
│   + Marbete 2026               RD$  6,000                              │
│   + INTRANT                    RD$  2,500                              │
│   + Seguro obligatorio         RD$  1,500                              │
│   + Inspección (recomendado)   RD$  3,500                              │
│   + Gestión legal              RD$  8,000                              │
│   ─────────────────────────────────────────────                        │
│   TOTAL                     RD$ 1,296,500                              │
│                                                                         │
│   [+ Agregar Seguro Full: RD$ 45,000/año]                              │
│   [📊 Calcular con Financiamiento]                                      │
│   [🔗 Compartir este cálculo]                                           │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📈 Tarifas de Marbete 2026

| Cilindraje    | Tarifa Anual              |
| ------------- | ------------------------- |
| Hasta 1000cc  | RD$ 3,000                 |
| 1001 - 1500cc | RD$ 4,500                 |
| 1501 - 2000cc | RD$ 6,000                 |
| 2001 - 2500cc | RD$ 8,000                 |
| 2501 - 3000cc | RD$ 10,000                |
| 3001 - 4000cc | RD$ 12,000                |
| Más de 4000cc | RD$ 15,000                |
| Eléctricos    | RD$ 1,500 (50% descuento) |
| Híbridos      | RD$ 3,000 (50% descuento) |

---

## 📊 Métricas Prometheus

```yaml
# Uso
total_cost_calculations_total{vehicle_condition}
total_cost_with_financing_total
total_cost_shared_total

# Engagement
total_cost_calculator_time_seconds
total_cost_to_contact_conversion_rate

# Revenue potencial
total_cost_legal_service_interest
total_cost_insurance_interest
```

---

## 🏆 Diferenciador vs Competencia

| Feature                        | OKLA                 | SuperCarros | Corotos     |
| ------------------------------ | -------------------- | ----------- | ----------- |
| Calculadora de costo total     | ✅ Completa          | ❌ No tiene | ❌ No tiene |
| Desglose de impuestos          | ✅ Detallado         | ❌          | ❌          |
| Cálculo de marbete             | ✅ Automático por CC | ❌          | ❌          |
| Integración con financiamiento | ✅                   | ❌          | ❌          |
| Compartir cálculo              | ✅                   | ❌          | ❌          |

---

## 🔗 Referencias

- [14-FINANCIAMIENTO-TRADEIN/01-calculadora-financiamiento.md](01-calculadora-financiamiento.md)
- [DGII - Impuestos Vehiculares](https://dgii.gov.do/vehiculos)
- [INTRANT - Tarifas](https://intrant.gob.do)
