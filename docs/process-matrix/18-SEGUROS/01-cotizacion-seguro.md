# 🛡️ Cotización y Compra de Seguro Vehicular

> **Código:** SEG-001, SEG-002, SEG-003  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🟡 ALTA (Revenue adicional + UX completa)

---

## 📋 Información General

| Campo             | Valor                                                           |
| ----------------- | --------------------------------------------------------------- |
| **Servicio**      | InsuranceService                                                |
| **Puerto**        | 5086                                                            |
| **Base de Datos** | `insuranceservice`                                              |
| **Dependencias**  | VehiclesSaleService, UserService, BillingService                |
| **Integraciones** | Seguros Universal, Mapfre BHD, La Colonial, Seguros Banreservas |

---

## 🎯 Objetivo del Proceso

1. **Cotización Instantánea:** Comparar precios de múltiples aseguradoras
2. **Compra de Póliza:** Adquirir seguro directamente en la plataforma
3. **Gestión de Pólizas:** Ver y renovar pólizas existentes
4. **Comisión para OKLA:** Revenue share con aseguradoras (8-12%)

---

## 📡 Endpoints

| Método | Endpoint                             | Descripción              | Auth |
| ------ | ------------------------------------ | ------------------------ | ---- |
| `POST` | `/api/insurance/quote`               | Obtener cotizaciones     | ❌   |
| `GET`  | `/api/insurance/quotes/{quoteId}`    | Detalle de cotización    | ❌   |
| `POST` | `/api/insurance/purchase`            | Comprar póliza           | ✅   |
| `GET`  | `/api/insurance/policies`            | Mis pólizas              | ✅   |
| `GET`  | `/api/insurance/policies/{id}`       | Detalle de póliza        | ✅   |
| `POST` | `/api/insurance/policies/{id}/renew` | Renovar póliza           | ✅   |
| `GET`  | `/api/insurance/providers`           | Aseguradoras disponibles | ❌   |

---

## 🗃️ Entidades

### InsuranceQuote

```csharp
public class InsuranceQuote
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? VehicleId { get; set; }

    // Datos del vehículo (si no está en plataforma)
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public decimal VehicleValue { get; set; }
    public string LicensePlate { get; set; }
    public string VIN { get; set; }

    // Datos del conductor
    public string DriverName { get; set; }
    public string DriverCedula { get; set; }
    public DateTime DriverDOB { get; set; }
    public int DriverAge { get; set; }
    public string DriverGender { get; set; }
    public int YearsLicensed { get; set; }
    public bool HasAccidents { get; set; }
    public int AccidentsLast3Years { get; set; }

    // Cobertura solicitada
    public CoverageType RequestedCoverage { get; set; }
    public decimal? RequestedDeductible { get; set; }

    // Resultados
    public List<InsuranceQuoteResult> Results { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }          // Válido 48 horas
}

public enum CoverageType
{
    Liability,           // Solo responsabilidad civil
    ThirdParty,          // Terceros
    Comprehensive,       // Todo riesgo
    ComprehensivePlus    // Todo riesgo + extras
}
```

### InsuranceQuoteResult

```csharp
public class InsuranceQuoteResult
{
    public Guid Id { get; set; }
    public Guid QuoteId { get; set; }
    public Guid ProviderId { get; set; }

    public string ProviderName { get; set; }
    public string ProviderLogo { get; set; }
    public string PlanName { get; set; }

    // Precios
    public decimal AnnualPremium { get; set; }
    public decimal MonthlyPremium { get; set; }
    public decimal Deductible { get; set; }

    // Coberturas incluidas
    public List<CoverageItem> Coverages { get; set; }

    // Beneficios adicionales
    public List<string> Benefits { get; set; }

    // Ranking
    public int Ranking { get; set; }
    public bool IsRecommended { get; set; }
    public string RecommendationReason { get; set; }

    // Comisión OKLA
    public decimal CommissionPercent { get; set; }
    public decimal CommissionAmount { get; set; }
}

public class CoverageItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal? Limit { get; set; }
    public bool IsIncluded { get; set; }
}
```

### InsurancePolicy

```csharp
public class InsurancePolicy
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid QuoteResultId { get; set; }

    // Póliza
    public string PolicyNumber { get; set; }
    public string ProviderPolicyNumber { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; }

    // Vigencia
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PolicyStatus Status { get; set; }

    // Financiero
    public decimal AnnualPremium { get; set; }
    public PaymentFrequency PaymentFrequency { get; set; }
    public decimal PaymentAmount { get; set; }
    public DateTime NextPaymentDate { get; set; }

    // Documentos
    public string PolicyDocumentUrl { get; set; }
    public string IdCardUrl { get; set; }

    // Datos del vehículo
    public string VehicleMake { get; set; }
    public string VehicleModel { get; set; }
    public int VehicleYear { get; set; }
    public string LicensePlate { get; set; }

    // Coberturas
    public CoverageType CoverageType { get; set; }
    public decimal Deductible { get; set; }
    public List<CoverageItem> Coverages { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? RenewedAt { get; set; }
}

public enum PolicyStatus
{
    Pending,        // Esperando pago
    Active,         // Vigente
    Expired,        // Vencida
    Cancelled,      // Cancelada
    Renewed         // Renovada (link a nueva)
}

public enum PaymentFrequency
{
    Annual,
    SemiAnnual,
    Quarterly,
    Monthly
}
```

### InsuranceProvider

```csharp
public class InsuranceProvider
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string LogoUrl { get; set; }
    public string Website { get; set; }

    // Integración
    public string ApiEndpoint { get; set; }
    public string ApiKey { get; set; }
    public bool IsActive { get; set; }

    // Comisiones
    public decimal DefaultCommissionPercent { get; set; }

    // Rating
    public decimal Rating { get; set; }
    public int TotalReviews { get; set; }
    public int ClaimResolutionDays { get; set; }

    // Productos disponibles
    public List<InsuranceProduct> Products { get; set; }
}
```

---

## 📊 Proceso SEG-001: Cotizar Seguro

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: SEG-001 - Cotizar Seguro Vehicular                            │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-ANON, USR-REG                                     │
│ Sistemas: InsuranceService, External Insurance APIs                    │
│ Duración: 5-15 segundos                                                │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                | Sistema          | Actor   | Evidencia         | Código     |
| ---- | ------- | ------------------------------------- | ---------------- | ------- | ----------------- | ---------- |
| 1    | 1.1     | Usuario ve listing o accede a seguros | Frontend         | USR-REG | Page accessed     | EVD-LOG    |
| 1    | 1.2     | Click "Cotizar Seguro"                | Frontend         | USR-REG | CTA clicked       | EVD-LOG    |
| 2    | 2.1     | Formulario de cotización              | Frontend         | USR-REG | Form displayed    | EVD-SCREEN |
| 2    | 2.2     | Ingresar datos del vehículo           | Frontend         | USR-REG | Vehicle data      | EVD-LOG    |
| 2    | 2.3     | Ingresar datos del conductor          | Frontend         | USR-REG | Driver data       | EVD-LOG    |
| 2    | 2.4     | Seleccionar tipo de cobertura         | Frontend         | USR-REG | Coverage selected | EVD-LOG    |
| 3    | 3.1     | POST /api/insurance/quote             | Gateway          | USR-REG | **Request**       | EVD-AUDIT  |
| 3    | 3.2     | Validar datos                         | InsuranceService | Sistema | Validation        | EVD-LOG    |
| 4    | 4.1     | **Llamar API Seguros Universal**      | InsuranceService | Sistema | **API call**      | EVD-AUDIT  |
| 4    | 4.2     | **Llamar API Mapfre BHD**             | InsuranceService | Sistema | **API call**      | EVD-AUDIT  |
| 4    | 4.3     | **Llamar API La Colonial**            | InsuranceService | Sistema | **API call**      | EVD-AUDIT  |
| 4    | 4.4     | **Llamar API Banreservas**            | InsuranceService | Sistema | **API call**      | EVD-AUDIT  |
| 5    | 5.1     | Agregar resultados                    | InsuranceService | Sistema | Results compiled  | EVD-LOG    |
| 5    | 5.2     | Calcular comisiones OKLA              | InsuranceService | Sistema | Commission calc   | EVD-LOG    |
| 5    | 5.3     | Ordenar por precio                    | InsuranceService | Sistema | Sorted            | EVD-LOG    |
| 5    | 5.4     | Marcar recomendado                    | InsuranceService | Sistema | Recommendation    | EVD-LOG    |
| 6    | 6.1     | **Crear InsuranceQuote**              | InsuranceService | Sistema | **Quote created** | EVD-AUDIT  |
| 7    | 7.1     | Mostrar resultados comparativos       | Frontend         | USR-REG | Results displayed | EVD-SCREEN |
| 8    | 8.1     | **Audit trail**                       | AuditService     | Sistema | Complete audit    | EVD-AUDIT  |

### Evidencia de Cotización

```json
{
  "processCode": "SEG-001",
  "quote": {
    "id": "quote-12345",
    "userId": "user-001",
    "vehicleId": "veh-67890",
    "vehicle": {
      "make": "Toyota",
      "model": "Corolla",
      "year": 2023,
      "value": 1250000,
      "plate": "A123456"
    },
    "driver": {
      "name": "Juan Pérez",
      "cedula": "001-*****-8",
      "age": 35,
      "yearsLicensed": 12,
      "accidentsLast3Years": 0
    },
    "requestedCoverage": "COMPREHENSIVE",
    "results": [
      {
        "provider": "Seguros Universal",
        "logo": "cdn.okla.com.do/logos/universal.png",
        "plan": "Todo Riesgo Premium",
        "annualPremium": 45000,
        "monthlyPremium": 4125,
        "deductible": 25000,
        "isRecommended": true,
        "reason": "Mejor relación precio-cobertura",
        "coverages": [
          {"name": "Responsabilidad Civil", "limit": 2000000, "included": true},
          {"name": "Daños Propios", "limit": 1250000, "included": true},
          {"name": "Robo Total", "limit": 1250000, "included": true},
          {"name": "Asistencia Vial", "limit": null, "included": true}
        ],
        "benefits": ["Grúa 24/7", "Vehículo sustituto 7 días", "App móvil"],
        "oklaCommission": {
          "percent": 10,
          "amount": 4500
        }
      },
      {
        "provider": "Mapfre BHD",
        "logo": "cdn.okla.com.do/logos/mapfre.png",
        "plan": "Óptimo Plus",
        "annualPremium": 48500,
        "monthlyPremium": 4454,
        "deductible": 20000,
        "isRecommended": false,
        "coverages": [...]
      },
      {
        "provider": "La Colonial",
        "logo": "cdn.okla.com.do/logos/colonial.png",
        "plan": "Total Cover",
        "annualPremium": 42000,
        "monthlyPremium": 3850,
        "deductible": 35000,
        "isRecommended": false,
        "coverages": [...]
      }
    ],
    "expiresAt": "2026-01-23T10:30:00Z",
    "createdAt": "2026-01-21T10:30:00Z"
  }
}
```

---

## 📊 Proceso SEG-002: Comprar Póliza

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: SEG-002 - Comprar Póliza de Seguro                            │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG                                               │
│ Sistemas: InsuranceService, BillingService, Insurance Provider API     │
│ Duración: 1-5 minutos                                                  │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                          | Sistema             | Actor     | Evidencia          | Código    |
| ---- | ------- | ------------------------------- | ------------------- | --------- | ------------------ | --------- |
| 1    | 1.1     | Usuario ve cotizaciones         | Frontend            | USR-REG   | Quotes viewed      | EVD-LOG   |
| 1    | 1.2     | Click "Comprar" en plan elegido | Frontend            | USR-REG   | Plan selected      | EVD-LOG   |
| 2    | 2.1     | Verificar cotización vigente    | InsuranceService    | Sistema   | Quote check        | EVD-LOG   |
| 2    | 2.2     | Confirmar datos del conductor   | Frontend            | USR-REG   | Data confirmed     | EVD-LOG   |
| 3    | 3.1     | Seleccionar frecuencia de pago  | Frontend            | USR-REG   | Frequency selected | EVD-LOG   |
| 3    | 3.2     | Calcular monto a pagar          | InsuranceService    | Sistema   | Amount calculated  | EVD-LOG   |
| 4    | 4.1     | POST /api/insurance/purchase    | Gateway             | USR-REG   | **Request**        | EVD-AUDIT |
| 4    | 4.2     | Crear payment intent            | BillingService      | Sistema   | Payment intent     | EVD-LOG   |
| 5    | 5.1     | Usuario realiza pago            | Stripe/Azul         | USR-REG   | **Payment**        | EVD-AUDIT |
| 5    | 5.2     | Confirmar pago                  | BillingService      | Sistema   | Payment confirmed  | EVD-EVENT |
| 6    | 6.1     | **Enviar a aseguradora**        | InsuranceService    | Sistema   | **Provider API**   | EVD-AUDIT |
| 6    | 6.2     | Recibir número de póliza        | InsuranceService    | Sistema   | Policy number      | EVD-LOG   |
| 7    | 7.1     | **Crear InsurancePolicy**       | InsuranceService    | Sistema   | **Policy created** | EVD-AUDIT |
| 7    | 7.2     | Generar documento de póliza     | InsuranceService    | Sistema   | **Policy doc**     | EVD-DOC   |
| 7    | 7.3     | Generar tarjeta de seguro       | InsuranceService    | Sistema   | **ID card**        | EVD-DOC   |
| 8    | 8.1     | **Registrar comisión OKLA**     | BillingService      | Sistema   | **Commission**     | EVD-AUDIT |
| 9    | 9.1     | **Enviar póliza por email**     | NotificationService | SYS-NOTIF | **Policy sent**    | EVD-COMM  |
| 9    | 9.2     | Notificación push               | NotificationService | SYS-NOTIF | Push sent          | EVD-COMM  |
| 10   | 10.1    | **Audit trail completo**        | AuditService        | Sistema   | Complete audit     | EVD-AUDIT |

### Evidencia de Compra de Póliza

```json
{
  "processCode": "SEG-002",
  "policy": {
    "id": "policy-12345",
    "policyNumber": "OKLA-SEG-2026-00001",
    "providerPolicyNumber": "SU-2026-789012",
    "userId": "user-001",
    "vehicleId": "veh-67890",
    "provider": {
      "id": "provider-001",
      "name": "Seguros Universal",
      "logo": "cdn.okla.com.do/logos/universal.png"
    },
    "vehicle": {
      "make": "Toyota",
      "model": "Corolla",
      "year": 2023,
      "plate": "A123456",
      "vin": "1HGBH41JXMN109186"
    },
    "coverage": {
      "type": "COMPREHENSIVE",
      "deductible": 25000,
      "items": [
        { "name": "Responsabilidad Civil", "limit": 2000000 },
        { "name": "Daños Propios", "limit": 1250000 },
        { "name": "Robo Total", "limit": 1250000 },
        { "name": "Asistencia Vial", "limit": null }
      ]
    },
    "financial": {
      "annualPremium": 45000,
      "paymentFrequency": "ANNUAL",
      "paymentAmount": 45000,
      "paymentId": "pay-67890"
    },
    "dates": {
      "startDate": "2026-01-21",
      "endDate": "2027-01-21",
      "nextPaymentDate": "2027-01-21"
    },
    "documents": {
      "policyPdf": "s3://insurance/policy-12345/policy.pdf",
      "idCard": "s3://insurance/policy-12345/id-card.pdf"
    },
    "oklaCommission": {
      "percent": 10,
      "amount": 4500,
      "status": "PENDING",
      "payoutDate": "2026-02-15"
    },
    "status": "ACTIVE",
    "createdAt": "2026-01-21T10:45:00Z"
  }
}
```

---

## 📊 Proceso SEG-003: Renovar Póliza

| Paso | Subpaso | Acción                                  | Sistema             | Actor     | Evidencia        | Código     |
| ---- | ------- | --------------------------------------- | ------------------- | --------- | ---------------- | ---------- |
| 1    | 1.1     | Scheduler detecta pólizas por vencer    | SYS-SCHEDULER       | Sistema   | Expiring check   | EVD-LOG    |
| 1    | 1.2     | 30 días antes: notificar usuario        | NotificationService | SYS-NOTIF | **Reminder**     | EVD-COMM   |
| 2    | 2.1     | Usuario accede a "Mis Pólizas"          | Frontend            | USR-REG   | Access           | EVD-LOG    |
| 2    | 2.2     | Click "Renovar"                         | Frontend            | USR-REG   | CTA clicked      | EVD-LOG    |
| 3    | 3.1     | Obtener nueva cotización                | InsuranceService    | Sistema   | New quote        | EVD-LOG    |
| 3    | 3.2     | Mostrar comparación vs anterior         | Frontend            | USR-REG   | Comparison shown | EVD-SCREEN |
| 4    | 4.1     | POST /api/insurance/policies/{id}/renew | Gateway             | USR-REG   | **Request**      | EVD-AUDIT  |
| 4    | 4.2     | Procesar pago                           | BillingService      | Sistema   | **Payment**      | EVD-AUDIT  |
| 5    | 5.1     | **Renovar con aseguradora**             | InsuranceService    | Sistema   | **Provider API** | EVD-AUDIT  |
| 5    | 5.2     | Crear nueva póliza                      | InsuranceService    | Sistema   | **New policy**   | EVD-AUDIT  |
| 5    | 5.3     | Marcar anterior como RENEWED            | InsuranceService    | Sistema   | Status updated   | EVD-LOG    |
| 6    | 6.1     | **Enviar nueva póliza**                 | NotificationService | SYS-NOTIF | **Policy sent**  | EVD-COMM   |

---

## 💰 Modelo de Comisiones

| Aseguradora         | Comisión OKLA | Pago    |
| ------------------- | ------------- | ------- |
| Seguros Universal   | 10%           | Mensual |
| Mapfre BHD          | 8%            | Mensual |
| La Colonial         | 12%           | Mensual |
| Seguros Banreservas | 9%            | Mensual |

### Ejemplo de Revenue

```
Póliza Anual: RD$ 45,000
Comisión OKLA (10%): RD$ 4,500

Si OKLA vende 100 pólizas/mes @ promedio RD$45,000:
- Revenue mensual: RD$ 450,000
- Revenue anual: RD$ 5,400,000
```

---

## 📊 Métricas Prometheus

```yaml
# Cotizaciones
insurance_quotes_total{provider}
insurance_quote_response_time_seconds{provider}
insurance_quote_conversion_rate

# Pólizas
insurance_policies_sold_total{provider, coverage_type}
insurance_policies_active_count
insurance_policies_renewed_total
insurance_policies_cancelled_total

# Revenue
insurance_commission_revenue_total{provider}
insurance_premium_volume_total

# Performance
insurance_api_latency_seconds{provider, endpoint}
insurance_api_error_rate{provider}
```

---

## 🔗 Referencias

- [03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md](../03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md)
- [05-PAGOS-FACTURACION/01-billing-service.md](../05-PAGOS-FACTURACION/01-billing-service.md)
- [Seguros Universal API](https://api.segurosuniversal.com.do)
