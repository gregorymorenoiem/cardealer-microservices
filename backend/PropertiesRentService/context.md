# PropertiesRentService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** PropertiesRentService
- **Puerto en Desarrollo:** 5022
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`propertiesrentservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de alquiler de propiedades inmobiliarias (casas, apartamentos, locales comerciales). Gestión de listings de renta, contratos de arrendamiento, depósitos y pagos mensuales.

---

## 🏗️ ARQUITECTURA

```
PropertiesRentService/
├── PropertiesRentService.Api/
│   ├── Controllers/
│   │   ├── RentalPropertiesController.cs
│   │   ├── LeaseApplicationsController.cs
│   │   └── LeaseContractsController.cs
│   └── Program.cs
├── PropertiesRentService.Application/
├── PropertiesRentService.Domain/
│   ├── Entities/
│   │   ├── RentalProperty.cs
│   │   ├── LeaseApplication.cs
│   │   ├── LeaseContract.cs
│   │   └── MonthlyPayment.cs
│   └── Enums/
│       ├── ApplicationStatus.cs
│       └── LeaseStatus.cs
└── PropertiesRentService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### RentalProperty
```csharp
public class RentalProperty
{
    public Guid Id { get; set; }
    
    // Información básica (similar a PropertiesSaleService)
    public string Title { get; set; }
    public PropertyType Type { get; set; }
    public string Address { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal SquareMeters { get; set; }
    
    // Precio de alquiler
    public decimal MonthlyRent { get; set; }
    public decimal SecurityDeposit { get; set; }  // Típicamente 1-2 meses de renta
    public bool UtilitiesIncluded { get; set; }
    
    // Disponibilidad
    public DateTime? AvailableFrom { get; set; }
    public bool IsAvailable { get; set; }
    
    // Requisitos
    public bool PetsAllowed { get; set; }
    public bool FurnishedOption { get; set; }
    public int MinLeaseMonths { get; set; }       // Mínimo contrato (6, 12 meses)
    
    // Propietario
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; }
    public string? OwnerPhone { get; set; }
}
```

### LeaseApplication
```csharp
public class LeaseApplication
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public Guid ApplicantId { get; set; }
    
    // Información del aplicante
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string CurrentAddress { get; set; }
    
    // Verificación
    public string EmploymentStatus { get; set; }
    public decimal MonthlyIncome { get; set; }
    public string? EmployerName { get; set; }
    public bool HasPets { get; set; }
    public string? PetDescription { get; set; }
    
    // Referencias
    public string? References { get; set; }       // JSON array
    
    // Estado
    public ApplicationStatus Status { get; set; }  // Submitted, UnderReview, Approved, Rejected
    public DateTime SubmittedAt { get; set; }
    public string? RejectionReason { get; set; }
}
```

### LeaseContract
```csharp
public class LeaseContract
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; }
    public Guid PropertyId { get; set; }
    public Guid TenantId { get; set; }
    public Guid LandlordId { get; set; }
    
    // Términos
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal SecurityDeposit { get; set; }
    public int PaymentDayOfMonth { get; set; }    // Día de pago (ej: 1, 15)
    
    // Estado
    public LeaseStatus Status { get; set; }        // Active, Expired, Terminated
    public DateTime? TerminatedAt { get; set; }
    public string? TerminationReason { get; set; }
    
    // Documentos
    public string? ContractPdfUrl { get; set; }
    public DateTime? SignedByTenantAt { get; set; }
    public DateTime? SignedByLandlordAt { get; set; }
}
```

### MonthlyPayment
```csharp
public class MonthlyPayment
{
    public Guid Id { get; set; }
    public Guid LeaseContractId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public bool IsLate { get; set; }
    public decimal? LateFee { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Propiedades en Alquiler
- `GET /api/rental-properties` - Listar propiedades disponibles
- `POST /api/rental-properties` - Publicar propiedad para alquiler
- `GET /api/rental-properties/{id}` - Detalle de propiedad

### Aplicaciones
- `POST /api/applications` - Aplicar para alquilar propiedad
- `GET /api/applications/{id}` - Ver aplicación
- `PUT /api/applications/{id}/approve` - Aprobar aplicación (propietario)
- `PUT /api/applications/{id}/reject` - Rechazar aplicación

### Contratos
- `POST /api/contracts` - Crear contrato de arrendamiento
- `GET /api/contracts/{id}` - Ver contrato
- `POST /api/contracts/{id}/sign` - Firmar contrato (tenant/landlord)
- `PUT /api/contracts/{id}/terminate` - Terminar contrato anticipadamente

### Pagos
- `GET /api/contracts/{contractId}/payments` - Historial de pagos
- `POST /api/payments` - Registrar pago de renta
- `GET /api/payments/pending` - Pagos pendientes del tenant

---

## 💡 FUNCIONALIDADES PLANEADAS

### Verificación de Inquilinos
- Credit check
- Background check
- Income verification
- References check

### Pagos Automatizados
- ACH recurring payments
- Recordatorios automáticos antes de fecha de pago
- Cargos automáticos por mora

### Portal del Inquilino
- Ver contrato
- Historial de pagos
- Reportar problemas de mantenimiento
- Comunicación con propietario

### Portal del Propietario
- Dashboard de propiedades
- Gestión de aplicaciones
- Tracking de pagos
- Reportes financieros

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0
