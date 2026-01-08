# RealEstateService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** RealEstateService
- **Puerto en Desarrollo:** 5023
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`realestateservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio agregador y coordinador para gestión unificada de bienes raíces. Combina funcionalidad de PropertiesSaleService y PropertiesRentService, añadiendo gestión de agentes inmobiliarios, agencias y comisiones.

---

## 🏗️ ARQUITECTURA

```
RealEstateService/
├── RealEstateService.Api/
│   ├── Controllers/
│   │   ├── AgentsController.cs
│   │   ├── AgenciesController.cs
│   │   ├── CommissionsController.cs
│   │   └── TransactionsController.cs
│   └── Program.cs
├── RealEstateService.Application/
├── RealEstateService.Domain/
│   ├── Entities/
│   │   ├── RealEstateAgent.cs
│   │   ├── Agency.cs
│   │   ├── Commission.cs
│   │   └── Transaction.cs
│   └── Enums/
│       └── TransactionType.cs
└── RealEstateService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### RealEstateAgent
```csharp
public class RealEstateAgent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }              // Link a UserService
    
    // Información básica
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string? PhotoUrl { get; set; }
    
    // Licencia
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public bool IsLicensed { get; set; }
    
    // Afiliación
    public Guid? AgencyId { get; set; }
    public Agency? Agency { get; set; }
    
    // Especialización
    public string[] Specializations { get; set; } = Array.Empty<string>();
    // ["Residential", "Commercial", "Luxury", "Rentals"]
    
    // Performance
    public int TotalSales { get; set; }
    public decimal TotalVolume { get; set; }      // Volumen total de ventas
    public decimal? AverageRating { get; set; }
    
    // Status
    public bool IsActive { get; set; }
    public DateTime JoinedAt { get; set; }
}
```

### Agency
```csharp
public class Agency
{
    public Guid Id { get; set; }
    
    // Información
    public string Name { get; set; }
    public string? Logo { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string? Website { get; set; }
    
    // Licencia
    public string? BusinessLicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    
    // Administración
    public Guid OwnerId { get; set; }             // Principal/Owner
    public int AgentCount { get; set; }
    
    // Comisión
    public decimal DefaultCommissionRate { get; set; } = 0.06m;  // 6%
    public decimal AgentSplitRate { get; set; } = 0.50m;        // 50% al agente
    
    public DateTime EstablishedAt { get; set; }
    public ICollection<RealEstateAgent> Agents { get; set; }
}
```

### Transaction
```csharp
public class Transaction
{
    public Guid Id { get; set; }
    public string TransactionNumber { get; set; }
    
    // Tipo y referencia
    public TransactionType Type { get; set; }     // Sale, Rent, Lease
    public Guid PropertyId { get; set; }          // ID en PropertiesSaleService o PropertiesRentService
    public string PropertyAddress { get; set; }
    
    // Partes
    public Guid BuyerOrTenantId { get; set; }
    public Guid SellerOrLandlordId { get; set; }
    public Guid? AgentId { get; set; }
    public Guid? AgencyId { get; set; }
    
    // Montos
    public decimal SalePrice { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal CommissionRate { get; set; }
    
    // Fechas
    public DateTime? OfferDate { get; set; }
    public DateTime? AcceptedDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Estado
    public string Status { get; set; }            // Offered, Accepted, Closed, Cancelled
}
```

### Commission
```csharp
public class Commission
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid AgentId { get; set; }
    public Guid? AgencyId { get; set; }
    
    // Montos
    public decimal TotalCommission { get; set; }
    public decimal AgentShare { get; set; }
    public decimal AgencyShare { get; set; }
    
    // Pago
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public bool IsPaid { get; set; }
    public string? PaymentReference { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Agentes
- `GET /api/agents` - Listar agentes (con filtros por ciudad, especialización)
- `GET /api/agents/{id}` - Perfil de agente
- `POST /api/agents` - Registrar agente
- `PUT /api/agents/{id}` - Actualizar perfil
- `GET /api/agents/{id}/listings` - Propiedades del agente
- `GET /api/agents/{id}/reviews` - Reviews/ratings del agente

### Agencias
- `GET /api/agencies` - Listar agencias
- `GET /api/agencies/{id}` - Detalle de agencia
- `POST /api/agencies` - Crear agencia
- `GET /api/agencies/{id}/agents` - Agentes de la agencia

### Transacciones
- `POST /api/transactions` - Crear transacción (cuando se cierra venta/alquiler)
- `GET /api/transactions/{id}` - Detalle de transacción
- `GET /api/agents/{agentId}/transactions` - Transacciones del agente

### Comisiones
- `GET /api/commissions/pending` - Comisiones pendientes de pago
- `PUT /api/commissions/{id}/mark-paid` - Marcar comisión como pagada
- `GET /api/agents/{agentId}/commissions` - Historial de comisiones

---

## 💡 FUNCIONALIDADES PLANEADAS

### Dashboard para Agentes
- Listings activos
- Leads asignados
- Comisiones ganadas
- Performance metrics

### Sistema CRM Básico
- Gestión de leads
- Follow-ups
- Historial de comunicaciones
- Conversión de lead a cliente

### Calculadora de Comisiones
Calcular comisiones basadas en:
- Precio de venta/alquiler
- Tasa de comisión de agencia
- Split con agente
- Co-listing con otra agencia (50/50)

### Reportes
- Ventas por periodo
- Top performing agents
- Comisiones por cobrar/cobradas
- Market trends por zona

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### PropertiesSaleService
- Agentes pueden listar propiedades en venta
- Vincular transacciones de venta

### PropertiesRentService
- Agentes pueden listar propiedades en renta
- Vincular transacciones de alquiler

### BillingService
- Procesar pagos de comisiones
- Invoicing automático

### NotificationService
- Notificar a agente cuando hay nuevo lead
- Recordatorios de follow-up

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0
