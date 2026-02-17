# 💰 Calculadora de Financiamiento

> **Código:** FIN-001  
> **Versión:** 1.0  
> **Última actualización:** Enero 25, 2026  
> **Criticidad:** 🔴 CRÍTICA (Diferenciador vs SuperCarros)  
> **Estado de Implementación:** ✅ Backend 100% | 🔴 UI 0%

---

## 🔴 AUDITORÍA DE ACCESO UI (Enero 25, 2026)

> **Estado:** ⚠️ Backend implementado pero SIN UI.

| Proceso                    | Backend | UI Access | Observación                  |
| -------------------------- | ------- | --------- | ---------------------------- |
| Calculadora financiamiento | ✅ 100% | 🔴 0%     | Sin widget en vehicle detail |
| Pre-aprobación             | ✅ 100% | 🔴 0%     | Sin formulario               |
| Tasas bancos RD            | ✅ 100% | 🔴 0%     | Sin comparador visual        |
| Integración bancos         | 🟡 60%  | 🔴 0%     | APIs pendientes              |

### Rutas UI Faltantes 🔴

| Ruta Propuesta              | Funcionalidad                   | Prioridad  |
| --------------------------- | ------------------------------- | ---------- |
| Widget en `/vehicles/:slug` | Calculadora en detalle vehículo | 🔴 CRÍTICA |
| `/financing/calculator`     | Calculadora standalone          | 🔴 ALTA    |
| `/financing/pre-approval`   | Formulario pre-aprobación       | 🔴 ALTA    |
| `/financing/compare`        | Comparador de bancos            | 🟡 MEDIA   |

**Verificación Backend:** FinanceService existe en `/backend/FinanceService/` ✅

---

## 📊 Resumen de Implementación (ACTUALIZADO)

| Componente  | Total | Implementado | Pendiente | Estado  |
| ----------- | ----- | ------------ | --------- | ------- |
| Controllers | 1     | 1            | 0         | ✅ 100% |
| FIN-CALC-\* | 5     | 5            | 0         | ✅ 100% |
| FIN-BANK-\* | 4     | 2            | 2         | 🟡 50%  |
| FIN-PRE-\*  | 4     | 4            | 0         | ✅ 100% |
| FIN-RATE-\* | 3     | 3            | 0         | ✅ 100% |
| Tests       | 10    | 6            | 4         | 🟡 60%  |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## �📋 Información General

| Campo             | Valor                                            |
| ----------------- | ------------------------------------------------ |
| **Servicio**      | FinancingService                                 |
| **Puerto**        | 5080                                             |
| **Base de Datos** | `financingservice`                               |
| **Dependencias**  | BillingService, UserService, VehiclesSaleService |
| **Integraciones** | Banco Popular, BHD León, Banreservas, Scotiabank |

---

## 🎯 Objetivo del Proceso

Permitir a compradores calcular cuotas mensuales de financiamiento para cualquier vehículo, con tasas reales de bancos dominicanos, y facilitar solicitud de pre-aprobación.

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     FinancingService Architecture                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   User Actions                       Core Service                            │
│   ┌────────────────┐              ┌─────────────────────────────────────┐   │
│   │ Calculator     │──┐           │          FinancingService           │   │
│   │ Widget         │  │           │  ┌───────────────────────────────┐  │   │
│   └────────────────┘  │           │  │ Controllers                   │  │   │
│   ┌────────────────┐  │           │  │ • FinancingController         │  │   │
│   │ Vehicle Detail │──┼──────────▶│  │ • PreApprovalController       │  │   │
│   │ (Calculate)    │  │           │  │ • BanksController             │  │   │
│   └────────────────┘  │           │  └───────────────────────────────┘  │   │
│   ┌────────────────┐  │           │  ┌───────────────────────────────┐  │   │
│   │ Pre-Approval   │──┘           │  │ Application (CQRS)            │  │   │
│   │ Form           │              │  │ • CalculatePaymentQuery       │  │   │
│   └────────────────┘              │  │ • SimulateMultipleBanksQuery  │  │   │
│                                   │  │ • RequestPreApprovalCmd       │  │   │
│   Bank Integrations               │  └───────────────────────────────┘  │   │
│   ┌────────────────┐              │  ┌───────────────────────────────┐  │   │
│   │ Banco Popular  │─────────────▶│  │ Domain                        │  │   │
│   │ BHD León       │              │  │ • FinancingCalculation        │  │   │
│   │ Banreservas    │              │  │ • Bank, InterestRate          │  │   │
│   │ Scotiabank     │              │  │ • PreApproval                 │  │   │
│   └────────────────┘              │  └───────────────────────────────┘  │   │
│                                   └─────────────────────────────────────┘   │
│                                                    │                        │
│                                    ┌───────────────┼───────────────┐        │
│                                    ▼               ▼               ▼        │
│                            ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│                            │ PostgreSQL │  │   Redis    │  │  RabbitMQ  │  │
│                            │ (Rates,    │  │  (Cached   │  │ (PreAppr.  │  │
│                            │  PreAppr.) │  │  Rates)    │  │  Events)   │  │
│                            └────────────┘  └────────────┘  └────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Endpoints

| Método | Endpoint                            | Descripción                 | Auth |
| ------ | ----------------------------------- | --------------------------- | ---- |
| `POST` | `/api/financing/calculate`          | Calcular cuota mensual      | ❌   |
| `GET`  | `/api/financing/banks`              | Listar bancos disponibles   | ❌   |
| `GET`  | `/api/financing/rates`              | Tasas actuales por banco    | ❌   |
| `POST` | `/api/financing/pre-approval`       | Solicitar pre-aprobación    | ✅   |
| `GET`  | `/api/financing/pre-approvals`      | Mis pre-aprobaciones        | ✅   |
| `GET`  | `/api/financing/pre-approvals/{id}` | Detalle pre-aprobación      | ✅   |
| `POST` | `/api/financing/simulate-multiple`  | Comparar cuotas multi-banco | ❌   |

---

## 🗃️ Entidades

### FinancingCalculation

```csharp
public class FinancingCalculation
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }              // Null si anónimo
    public Guid? VehicleId { get; set; }           // Opcional

    // Datos del cálculo
    public decimal VehiclePrice { get; set; }       // Precio del vehículo
    public decimal DownPayment { get; set; }        // Inicial
    public decimal DownPaymentPercentage { get; set; } // % de inicial
    public decimal AmountToFinance { get; set; }    // Monto a financiar
    public int TermMonths { get; set; }             // Plazo en meses

    // Resultado
    public Guid BankId { get; set; }
    public decimal InterestRate { get; set; }       // Tasa anual
    public decimal MonthlyPayment { get; set; }     // Cuota mensual
    public decimal TotalInterest { get; set; }      // Total intereses
    public decimal TotalPayment { get; set; }       // Total a pagar

    // Metadata
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Bank

```csharp
public class Bank
{
    public Guid Id { get; set; }
    public string Name { get; set; }                // "Banco Popular"
    public string Code { get; set; }                // "BPOP"
    public string LogoUrl { get; set; }
    public bool IsActive { get; set; }

    // Contacto
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Website { get; set; }

    // Configuración
    public decimal MinDownPaymentPercent { get; set; }  // Mínimo inicial %
    public decimal MaxFinancingPercent { get; set; }    // Máximo a financiar %
    public int MinTermMonths { get; set; }              // Plazo mínimo
    public int MaxTermMonths { get; set; }              // Plazo máximo
    public decimal MinAmount { get; set; }              // Monto mínimo
    public decimal MaxAmount { get; set; }              // Monto máximo
    public int MaxVehicleAge { get; set; }              // Antigüedad máxima
}
```

### InterestRate

```csharp
public class InterestRate
{
    public Guid Id { get; set; }
    public Guid BankId { get; set; }
    public Bank Bank { get; set; }

    public VehicleCondition Condition { get; set; }  // New, Used
    public int MinTermMonths { get; set; }
    public int MaxTermMonths { get; set; }
    public decimal AnnualRate { get; set; }          // Tasa anual

    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveUntil { get; set; }
    public bool IsActive { get; set; }

    public DateTime UpdatedAt { get; set; }
}
```

### PreApprovalRequest

```csharp
public class PreApprovalRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid BankId { get; set; }

    // Datos financieros
    public decimal RequestedAmount { get; set; }
    public int RequestedTermMonths { get; set; }
    public decimal DownPaymentAmount { get; set; }

    // Datos del solicitante
    public decimal MonthlyIncome { get; set; }
    public string EmploymentType { get; set; }       // Employed, SelfEmployed, Business
    public string EmployerName { get; set; }
    public int YearsEmployed { get; set; }

    // Estado
    public PreApprovalStatus Status { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public decimal? ApprovedRate { get; set; }
    public string RejectionReason { get; set; }

    // Tracking
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }          // Pre-aprobación válida por 30 días
}

public enum PreApprovalStatus
{
    Pending,
    UnderReview,
    Approved,
    Rejected,
    Expired
}
```

---

## 📊 Proceso FIN-001: Calcular Cuota de Financiamiento

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: FIN-001 - Calcular Cuota de Financiamiento                    │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-ANON, USR-REG (cualquier visitante)               │
│ Sistemas: FinancingService, VehiclesSaleService                        │
│ Duración: < 1 segundo                                                  │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                          | Sistema          | Actor    | Evidencia             | Código    |
| ---- | ------- | ------------------------------- | ---------------- | -------- | --------------------- | --------- |
| 1    | 1.1     | Usuario ingresa precio vehículo | Frontend         | USR-ANON | Input tracked         | EVD-LOG   |
| 1    | 1.2     | Usuario selecciona inicial %    | Frontend         | USR-ANON | Selection             | EVD-LOG   |
| 1    | 1.3     | Usuario selecciona plazo        | Frontend         | USR-ANON | Term selected         | EVD-LOG   |
| 1    | 1.4     | Usuario selecciona banco        | Frontend         | USR-ANON | Bank selected         | EVD-LOG   |
| 2    | 2.1     | POST /api/financing/calculate   | Gateway          | USR-ANON | **Request log**       | EVD-AUDIT |
| 2    | 2.2     | Validar parámetros              | FinancingService | Sistema  | Validation            | EVD-LOG   |
| 3    | 3.1     | Obtener tasa del banco          | FinancingService | Sistema  | Rate fetched          | EVD-LOG   |
| 3    | 3.2     | Verificar elegibilidad          | FinancingService | Sistema  | Eligibility           | EVD-LOG   |
| 4    | 4.1     | **Calcular cuota mensual**      | FinancingService | Sistema  | **Cálculo**           | EVD-AUDIT |
| 4    | 4.2     | Calcular total intereses        | FinancingService | Sistema  | Interest calc         | EVD-LOG   |
| 4    | 4.3     | Calcular total a pagar          | FinancingService | Sistema  | Total calc            | EVD-LOG   |
| 5    | 5.1     | Guardar cálculo                 | FinancingService | Sistema  | **Calculation saved** | EVD-AUDIT |
| 5    | 5.2     | Retornar resultado              | FinancingService | Sistema  | Response              | EVD-LOG   |
| 6    | 6.1     | Mostrar resultado al usuario    | Frontend         | Sistema  | UI rendered           | EVD-LOG   |
| 6    | 6.2     | Track para analytics            | AnalyticsService | Sistema  | Event tracked         | EVD-EVENT |

### Fórmula de Cálculo (Cuota Fija)

```csharp
// Método de amortización francesa (cuota fija)
public decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int months)
{
    if (annualRate == 0)
        return principal / months;

    decimal monthlyRate = annualRate / 12 / 100;
    decimal factor = (decimal)Math.Pow((double)(1 + monthlyRate), months);

    return principal * (monthlyRate * factor) / (factor - 1);
}
```

### Evidencia de Cálculo

```json
{
  "processCode": "FIN-001",
  "calculation": {
    "id": "calc-12345",
    "input": {
      "vehiclePrice": 1500000,
      "downPaymentPercent": 20,
      "downPaymentAmount": 300000,
      "amountToFinance": 1200000,
      "termMonths": 48,
      "bankCode": "BPOP"
    },
    "rates": {
      "bank": "Banco Popular",
      "annualRate": 12.5,
      "monthlyRate": 1.04167
    },
    "result": {
      "monthlyPayment": 31847.23,
      "totalInterest": 328667.04,
      "totalPayment": 1528667.04
    },
    "context": {
      "vehicleId": "veh-12345",
      "vehicleTitle": "Toyota Corolla 2024",
      "userId": null,
      "ip": "190.52.xx.xx",
      "userAgent": "Mozilla/5.0..."
    },
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

## 📊 Proceso FIN-002: Comparar Multi-Banco

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: FIN-002 - Comparar Cuotas Multi-Banco                         │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-ANON, USR-REG                                     │
│ Sistemas: FinancingService                                             │
│ Duración: < 2 segundos                                                 │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                | Sistema          | Actor    | Evidencia            | Código    |
| ---- | ------- | ------------------------------------- | ---------------- | -------- | -------------------- | --------- |
| 1    | 1.1     | POST /api/financing/simulate-multiple | Gateway          | USR-ANON | Request              | EVD-AUDIT |
| 2    | 2.1     | Obtener todos los bancos activos      | FinancingService | Sistema  | Banks list           | EVD-LOG   |
| 2    | 2.2     | Para cada banco: obtener tasa         | FinancingService | Sistema  | Rates fetched        | EVD-LOG   |
| 3    | 3.1     | Para cada banco: calcular cuota       | FinancingService | Sistema  | **Multi-calc**       | EVD-AUDIT |
| 3    | 3.2     | Ordenar por cuota menor               | FinancingService | Sistema  | Sorted               | EVD-LOG   |
| 4    | 4.1     | **Guardar comparación**               | FinancingService | Sistema  | **Comparison saved** | EVD-AUDIT |
| 4    | 4.2     | Retornar array de resultados          | FinancingService | Sistema  | Response             | EVD-LOG   |

### Evidencia de Comparación

```json
{
  "processCode": "FIN-002",
  "comparison": {
    "id": "comp-12345",
    "input": {
      "vehiclePrice": 1500000,
      "downPayment": 300000,
      "termMonths": 48
    },
    "results": [
      {
        "rank": 1,
        "bank": "Scotiabank",
        "rate": 11.5,
        "monthlyPayment": 30985.45,
        "totalInterest": 287301.6,
        "savings": 41365.44
      },
      {
        "rank": 2,
        "bank": "BHD León",
        "rate": 12.0,
        "monthlyPayment": 31416.78,
        "totalInterest": 308005.44,
        "savings": 20661.6
      },
      {
        "rank": 3,
        "bank": "Banco Popular",
        "rate": 12.5,
        "monthlyPayment": 31847.23,
        "totalInterest": 328667.04,
        "savings": 0
      },
      {
        "rank": 4,
        "bank": "Banreservas",
        "rate": 13.0,
        "monthlyPayment": 32278.12,
        "totalInterest": 349349.76,
        "savings": -20682.72
      }
    ],
    "recommendation": "Scotiabank ofrece la mejor tasa para este monto",
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

## 📊 Proceso FIN-003: Solicitud de Pre-Aprobación

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: FIN-003 - Solicitar Pre-Aprobación                            │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG (requiere autenticación)                      │
│ Sistemas: FinancingService, UserService, NotificationService           │
│ Duración: Instantáneo → 24-48h respuesta del banco                     │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                           | Sistema             | Actor     | Evidencia              | Código    |
| ---- | ------- | -------------------------------- | ------------------- | --------- | ---------------------- | --------- |
| 1    | 1.1     | Usuario completa formulario      | Frontend            | USR-REG   | Form data              | EVD-LOG   |
| 1    | 1.2     | Validación client-side           | Frontend            | Sistema   | Validation             | EVD-LOG   |
| 2    | 2.1     | POST /api/financing/pre-approval | Gateway             | USR-REG   | **Request**            | EVD-AUDIT |
| 2    | 2.2     | Verificar usuario autenticado    | AuthService         | Sistema   | Auth check             | EVD-LOG   |
| 3    | 3.1     | Validar datos financieros        | FinancingService    | Sistema   | Validation             | EVD-LOG   |
| 3    | 3.2     | Verificar capacidad de pago      | FinancingService    | Sistema   | **Debt-to-income**     | EVD-AUDIT |
| 4    | 4.1     | **Crear PreApprovalRequest**     | FinancingService    | Sistema   | **Request created**    | EVD-EVENT |
| 4    | 4.2     | Snapshot de datos                | FinancingService    | Sistema   | Data snapshot          | EVD-SNAP  |
| 5    | 5.1     | **Enviar a banco (API/Email)**   | FinancingService    | Sistema   | **Bank notification**  | EVD-COMM  |
| 5    | 5.2     | Log de envío                     | FinancingService    | Sistema   | Send log               | EVD-LOG   |
| 6    | 6.1     | Notificar al usuario             | NotificationService | SYS-NOTIF | **Email confirmación** | EVD-COMM  |
| 6    | 6.2     | Push notification                | NotificationService | SYS-NOTIF | Push sent              | EVD-COMM  |
| 7    | 7.1     | **Audit trail**                  | AuditService        | Sistema   | Complete audit         | EVD-AUDIT |

### [Asíncrono] Respuesta del Banco

| Paso | Subpaso | Acción                  | Sistema             | Actor     | Evidencia            | Código    |
| ---- | ------- | ----------------------- | ------------------- | --------- | -------------------- | --------- |
| 8    | 8.1     | Webhook de banco        | FinancingService    | SYS-BANK  | **Webhook received** | EVD-AUDIT |
| 8    | 8.2     | Verificar firma         | FinancingService    | Sistema   | Signature check      | EVD-LOG   |
| 9    | 9.1     | Actualizar status       | FinancingService    | Sistema   | **Status updated**   | EVD-AUDIT |
| 9    | 9.2     | Snapshot nuevo estado   | FinancingService    | Sistema   | New state            | EVD-SNAP  |
| 10   | 10.1    | **Notificar resultado** | NotificationService | SYS-NOTIF | **Email resultado**  | EVD-COMM  |
| 10   | 10.2    | SMS si aprobado         | NotificationService | SYS-NOTIF | SMS sent             | EVD-COMM  |

### Evidencia de Solicitud de Pre-Aprobación

```json
{
  "processCode": "FIN-003",
  "preApproval": {
    "id": "pre-12345",
    "status": "PENDING",
    "applicant": {
      "userId": "user-001",
      "name": "Juan Pérez",
      "cedula": "001-1234567-8",
      "phone": "+18095551234",
      "email": "juan@email.com"
    },
    "employment": {
      "type": "EMPLOYED",
      "employer": "Empresa XYZ SRL",
      "position": "Gerente",
      "yearsEmployed": 5,
      "monthlyIncome": 150000
    },
    "request": {
      "vehicleId": "veh-12345",
      "vehicleTitle": "Toyota Corolla 2024",
      "vehiclePrice": 1500000,
      "downPayment": 300000,
      "amountRequested": 1200000,
      "termMonths": 48,
      "bankId": "bank-bpop"
    },
    "analysis": {
      "debtToIncomeRatio": 21.2,
      "maxRecommendedPayment": 52500,
      "requestedPayment": 31847,
      "eligible": true
    },
    "sentToBank": {
      "timestamp": "2026-01-21T10:30:00Z",
      "method": "API",
      "referenceId": "BPOP-REF-12345"
    },
    "timestamps": {
      "created": "2026-01-21T10:30:00Z",
      "expiresAt": "2026-02-20T10:30:00Z"
    }
  }
}
```

---

## 🏦 Configuración de Bancos

### Tasas Actuales (Enero 2026)

```json
{
  "banks": [
    {
      "code": "BPOP",
      "name": "Banco Popular Dominicano",
      "rates": {
        "new": {
          "12-24": 10.5,
          "25-36": 11.5,
          "37-48": 12.0,
          "49-60": 12.5,
          "61-72": 13.0
        },
        "used": {
          "12-24": 12.5,
          "25-36": 13.5,
          "37-48": 14.0,
          "49-60": 14.5
        }
      },
      "requirements": {
        "minDownPayment": 20,
        "maxFinancing": 80,
        "maxVehicleAge": 7,
        "minAmount": 200000,
        "maxAmount": 5000000
      }
    },
    {
      "code": "BHD",
      "name": "BHD León",
      "rates": {
        "new": {
          "12-24": 10.0,
          "25-36": 11.0,
          "37-48": 11.5,
          "49-60": 12.0,
          "61-72": 12.5
        }
      }
    },
    {
      "code": "BANRES",
      "name": "Banreservas",
      "rates": {
        "new": {
          "12-24": 11.0,
          "25-36": 12.0,
          "37-48": 12.5,
          "49-60": 13.0
        }
      }
    },
    {
      "code": "SCOTIA",
      "name": "Scotiabank",
      "rates": {
        "new": {
          "12-24": 9.5,
          "25-36": 10.5,
          "37-48": 11.0,
          "49-60": 11.5,
          "61-72": 12.0
        }
      }
    }
  ]
}
```

---

## 📊 Métricas Prometheus

```yaml
# Calculadoras ejecutadas
financing_calculations_total{bank, term_months, vehicle_condition}

# Pre-aprobaciones
financing_preapprovals_total{bank, status}
financing_preapproval_processing_time_seconds{bank}

# Tasas
financing_current_rate{bank, condition, term_range}

# Conversión
financing_calculator_to_preapproval_rate
financing_preapproval_to_sale_rate
```

---

## 🔗 Referencias

- [05-PAGOS-FACTURACION/01-billing-service.md](../05-PAGOS-FACTURACION/01-billing-service.md)
- [03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md](../03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md)
- [Banco Popular API Docs](https://desarrolladores.bpd.com.do)
- [BHD León API](https://api.bhdleon.com.do)
