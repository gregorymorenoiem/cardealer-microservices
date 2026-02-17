# VehiclesRentService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** VehiclesRentService
- **Puerto en Desarrollo:** 5020
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`vehiclesrentservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de alquiler/renta de vehículos. Gestión de inventario de vehículos para renta, reservas, disponibilidad, precios por día/semana/mes y contratos de alquiler.

---

## 🏗️ ARQUITECTURA

```
VehiclesRentService/
├── VehiclesRentService.Api/
│   ├── Controllers/
│   │   ├── RentalVehiclesController.cs
│   │   ├── ReservationsController.cs
│   │   └── RentalContractsController.cs
│   └── Program.cs
├── VehiclesRentService.Application/
├── VehiclesRentService.Domain/
│   ├── Entities/
│   │   ├── RentalVehicle.cs
│   │   ├── Reservation.cs
│   │   ├── RentalContract.cs
│   │   └── RentalPricing.cs
│   └── Enums/
│       ├── ReservationStatus.cs
│       └── VehicleAvailability.cs
└── VehiclesRentService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### RentalVehicle
```csharp
public class RentalVehicle
{
    public Guid Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string LicensePlate { get; set; }
    public VehicleCategory Category { get; set; }   // Economy, Compact, SUV, Luxury
    public VehicleAvailability Availability { get; set; }
    public decimal DailyRate { get; set; }
    public decimal WeeklyRate { get; set; }
    public decimal MonthlyRate { get; set; }
    public int Mileage { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
}
```

### Reservation
```csharp
public class Reservation
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid UserId { get; set; }
    public DateTime PickupDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public string PickupLocation { get; set; }
    public string ReturnLocation { get; set; }
    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; }   // Pending, Confirmed, Active, Completed, Cancelled
    public DateTime CreatedAt { get; set; }
}
```

### RentalContract
```csharp
public class RentalContract
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public string ContractNumber { get; set; }
    public DateTime SignedAt { get; set; }
    public int MileageLimit { get; set; }
    public decimal SecurityDeposit { get; set; }
    public decimal InsuranceFee { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? DigitalSignature { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

- `GET /api/rental-vehicles` - Listar vehículos disponibles
- `GET /api/rental-vehicles/{id}` - Detalle de vehículo
- `GET /api/rental-vehicles/availability` - Check disponibilidad por fechas
- `POST /api/reservations` - Crear reserva
- `GET /api/reservations/{id}` - Detalle de reserva
- `PUT /api/reservations/{id}/confirm` - Confirmar reserva
- `PUT /api/reservations/{id}/cancel` - Cancelar reserva
- `POST /api/rental-contracts` - Crear contrato
- `GET /api/rental-contracts/{id}` - Ver contrato

---

## 💡 FUNCIONALIDADES PLANEADAS

### Check de Disponibilidad
Sistema de calendario para verificar que vehículo no esté reservado en fechas solicitadas.

### Precios Dinámicos
- Temporada alta/baja
- Descuentos por duración (7+ días, 30+ días)
- Extras: GPS, asiento bebé, conductor adicional

### Gestión de Flota
- Mantenimiento programado
- Tracking de kilometraje
- Inspecciones pre/post renta

### Integración con Pagos
- Depósito de seguridad (hold en tarjeta)
- Cargo al devolver (por daños, combustible, etc.)

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0
