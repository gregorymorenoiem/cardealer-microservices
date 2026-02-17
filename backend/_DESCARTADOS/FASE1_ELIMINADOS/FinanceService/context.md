# FinanceService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** FinanceService
- **Puerto en Desarrollo:** 5024
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`financeservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de financiamiento para vehículos y propiedades. Gestiona solicitudes de préstamos, cálculo de cuotas, pre-aprobaciones y coordinación con entidades financieras.

---

## 🏗️ ARQUITECTURA

```
FinanceService/
├── FinanceService.Api/
│   ├── Controllers/
│   │   ├── LoanApplicationsController.cs
│   │   ├── CalculatorController.cs
│   │   └── LendersController.cs
│   └── Program.cs
├── FinanceService.Application/
│   └── Services/
│       └── LoanCalculatorService.cs
├── FinanceService.Domain/
│   ├── Entities/
│   │   ├── LoanApplication.cs
│   │   ├── Lender.cs
│   │   └── LoanOffer.cs
│   └── Enums/
│       ├── LoanType.cs
│       └── ApplicationStatus.cs
└── FinanceService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### LoanApplication
```csharp
public class LoanApplication
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; }
    
    // Tipo
    public LoanType Type { get; set; }            // Auto, Property
    public Guid AssetId { get; set; }             // VehicleId o PropertyId
    public string AssetDescription { get; set; }
    public decimal AssetPrice { get; set; }
    
    // Solicitante
    public Guid ApplicantId { get; set; }
    public string ApplicantName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    
    // Detalles financieros
    public decimal DownPayment { get; set; }
    public decimal LoanAmount { get; set; }
    public int TermMonths { get; set; }           // 24, 36, 48, 60 meses
    public decimal? RequestedInterestRate { get; set; }
    
    // Información del solicitante
    public decimal MonthlyIncome { get; set; }
    public string EmploymentStatus { get; set; }
    public string? EmployerName { get; set; }
    public int? CreditScore { get; set; }
    public bool HasExistingLoans { get; set; }
    public decimal? ExistingLoanPayments { get; set; }
    
    // Estado
    public ApplicationStatus Status { get; set; }  // Submitted, UnderReview, Approved, Rejected
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    
    // Documentos
    public string[] DocumentUrls { get; set; } = Array.Empty<string>();
}
```

### Lender
```csharp
public class Lender
{
    public Guid Id { get; set; }
    
    // Información
    public string Name { get; set; }
    public string? Logo { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string? Website { get; set; }
    
    // Tipos de préstamos
    public bool OffersAutoLoans { get; set; }
    public bool OffersPropertyLoans { get; set; }
    
    // Tasas (rangos)
    public decimal MinInterestRate { get; set; }
    public decimal MaxInterestRate { get; set; }
    
    // Requisitos
    public int MinCreditScore { get; set; }
    public decimal MinDownPaymentPercent { get; set; }
    public int MinLoanTermMonths { get; set; }
    public int MaxLoanTermMonths { get; set; }
    
    // API Integration
    public bool HasApiIntegration { get; set; }
    public string? ApiEndpoint { get; set; }
    public string? ApiKey { get; set; }
    
    public bool IsActive { get; set; }
}
```

### LoanOffer
```csharp
public class LoanOffer
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid LenderId { get; set; }
    
    // Términos ofrecidos
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalPayment { get; set; }
    
    // Condiciones
    public decimal? RequiredDownPayment { get; set; }
    public bool RequiresCoSigner { get; set; }
    public string? SpecialConditions { get; set; }
    
    // Validez
    public DateTime OfferedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime? AcceptedAt { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Calculadora
- `POST /api/calculator/estimate` - Calcular cuota mensual
  ```json
  Request:
  {
    "loanAmount": 15000,
    "interestRate": 9.5,
    "termMonths": 48,
    "downPayment": 3000
  }
  
  Response:
  {
    "monthlyPayment": 375.83,
    "totalInterest": 3039.84,
    "totalPayment": 18039.84,
    "estimatedAPR": 10.2
  }
  ```

### Aplicaciones
- `POST /api/applications` - Crear solicitud de préstamo
- `GET /api/applications/{id}` - Ver solicitud
- `PUT /api/applications/{id}` - Actualizar información
- `POST /api/applications/{id}/submit` - Enviar solicitud a prestamistas

### Ofertas
- `GET /api/applications/{id}/offers` - Ofertas recibidas
- `POST /api/offers/{id}/accept` - Aceptar oferta
- `GET /api/offers/{id}` - Detalle de oferta

### Prestamistas
- `GET /api/lenders` - Listar prestamistas disponibles
- `GET /api/lenders/{id}` - Detalle de prestamista

---

## 💡 FUNCIONALIDADES PLANEADAS

### Calculadora Avanzada
```csharp
public class LoanCalculation
{
    public decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int months)
    {
        if (annualRate == 0) return principal / months;
        
        var monthlyRate = annualRate / 100 / 12;
        var payment = principal * (monthlyRate * Math.Pow(1 + monthlyRate, months)) /
                      (Math.Pow(1 + monthlyRate, months) - 1);
        return Math.Round(payment, 2);
    }
    
    public decimal CalculateTotalInterest(decimal monthlyPayment, int months, decimal principal)
    {
        return (monthlyPayment * months) - principal;
    }
}
```

### Pre-Aprobación Automática
Evaluar solicitud basada en:
- Credit score mínimo
- Debt-to-income ratio (DTI)
- Down payment mínimo
- Historial de crédito

### Integración con Bureaus de Crédito
- Consulta de credit score
- Verificación de historial
- Validación de identidad

### Comparador de Ofertas
Mostrar múltiples ofertas lado a lado:
- Mejor tasa de interés
- Cuota mensual más baja
- Términos más flexibles

### Notificaciones
- Actualización de estado de solicitud
- Nueva oferta recibida
- Recordatorio de documentos faltantes

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### VehiclesSaleService / PropertiesSaleService
- Botón "Financiar" en listing
- Pre-llenar datos del activo

### UserService
- Obtener información del usuario
- Credit check authorization

### BillingService
- Procesar down payment
- Gestionar pagos mensuales

### NotificationService
- Alertas de estado de aplicación
- Recordatorios de pago

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0
