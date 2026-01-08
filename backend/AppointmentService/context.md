# AppointmentService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** AppointmentService
- **Puerto en Desarrollo:** 5025
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`appointmentservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de gestión de citas y agendamiento. Permite a usuarios programar:
- Test drives de vehículos
- Tours de propiedades
- Consultas con vendedores/agentes
- Inspecciones y evaluaciones

---

## 🏗️ ARQUITECTURA

```
AppointmentService/
├── AppointmentService.Api/
│   ├── Controllers/
│   │   ├── AppointmentsController.cs
│   │   ├── AvailabilityController.cs
│   │   └── TimeSlotsController.cs
│   └── Program.cs
├── AppointmentService.Application/
├── AppointmentService.Domain/
│   ├── Entities/
│   │   ├── Appointment.cs
│   │   ├── TimeSlot.cs
│   │   └── Availability.cs
│   └── Enums/
│       ├── AppointmentType.cs
│       └── AppointmentStatus.cs
└── AppointmentService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### Appointment
```csharp
public class Appointment
{
    public Guid Id { get; set; }
    public string AppointmentNumber { get; set; }  // APT-2026-001234
    
    // Tipo y contexto
    public AppointmentType Type { get; set; }      // TestDrive, PropertyTour, Consultation, Inspection
    public Guid? RelatedEntityId { get; set; }     // VehicleId o PropertyId
    public string? RelatedEntityDescription { get; set; }
    
    // Participantes
    public Guid ClientId { get; set; }
    public string ClientName { get; set; }
    public string ClientEmail { get; set; }
    public string ClientPhone { get; set; }
    
    public Guid? AgentOrSellerId { get; set; }
    public string? AgentOrSellerName { get; set; }
    
    // Fecha y hora
    public DateTime ScheduledDate { get; set; }
    public DateTime ScheduledTime { get; set; }
    public int DurationMinutes { get; set; } = 30;
    
    // Ubicación
    public string Location { get; set; }
    public string? LocationAddress { get; set; }
    public bool IsVirtualAppointment { get; set; }
    public string? VirtualMeetingLink { get; set; }
    
    // Estado
    public AppointmentStatus Status { get; set; }  // Scheduled, Confirmed, InProgress, Completed, Cancelled, NoShow
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    
    // Notas
    public string? ClientNotes { get; set; }
    public string? InternalNotes { get; set; }
    
    // Recordatorios
    public bool ReminderSent { get; set; }
    public DateTime? ReminderSentAt { get; set; }
}
```

### TimeSlot
```csharp
public class TimeSlot
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }           // AgentId o DealerId
    public string ProviderType { get; set; }       // "Agent", "Dealer", "Location"
    
    // Día y hora
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationMinutes { get; set; } = 30;
    
    // Disponibilidad
    public bool IsAvailable { get; set; }
    public int MaxConcurrentAppointments { get; set; } = 1;
    
    // Validez
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
}
```

### Availability
```csharp
public class Availability
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime Date { get; set; }
    
    // Horarios específicos para este día
    public TimeSpan? CustomStartTime { get; set; }
    public TimeSpan? CustomEndTime { get; set; }
    
    // Override de disponibilidad
    public bool IsAvailable { get; set; } = true;
    public string? UnavailabilityReason { get; set; }  // "Holiday", "Vacation", "Personal"
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Citas
- `POST /api/appointments` - Crear cita
  ```json
  {
    "type": "TestDrive",
    "relatedEntityId": "vehicle-uuid",
    "clientId": "user-uuid",
    "scheduledDate": "2026-02-15",
    "scheduledTime": "14:00:00",
    "location": "Dealer Showroom",
    "clientNotes": "Interested in test driving manual transmission"
  }
  ```
- `GET /api/appointments/{id}` - Detalle de cita
- `PUT /api/appointments/{id}` - Actualizar cita
- `PUT /api/appointments/{id}/confirm` - Confirmar cita
- `PUT /api/appointments/{id}/cancel` - Cancelar cita
- `PUT /api/appointments/{id}/complete` - Marcar como completada
- `GET /api/appointments/my-appointments` - Citas del usuario

### Disponibilidad
- `GET /api/availability/{providerId}` - Ver disponibilidad de agente/dealer
  ```json
  Query: ?date=2026-02-15
  
  Response:
  {
    "date": "2026-02-15",
    "availableSlots": [
      { "time": "09:00", "available": true },
      { "time": "09:30", "available": true },
      { "time": "10:00", "available": false },
      { "time": "10:30", "available": true }
    ]
  }
  ```
- `POST /api/availability` - Definir disponibilidad
- `PUT /api/availability/{id}` - Actualizar disponibilidad

### Time Slots
- `GET /api/timeslots/{providerId}` - Ver horarios configurados
- `POST /api/timeslots` - Crear horario recurrente
- `PUT /api/timeslots/{id}` - Actualizar horario

---

## 💡 FUNCIONALIDADES PLANEADAS

### Sistema de Recordatorios
- Email 24h antes de la cita
- SMS 2h antes de la cita
- Push notification 30min antes

### Calendario Integrado
- Export a Google Calendar / iCal
- Sincronización bidireccional
- Mostrar disponibilidad en tiempo real

### Re-Scheduling Automático
- Cliente puede re-agendar hasta 4h antes
- Sugerir slots alternativos si se cancela

### Check-in Digital
- QR code en email de confirmación
- Cliente hace check-in al llegar
- Notifica a vendedor/agente

### Follow-up Automático
- Email después de cita: "¿Cómo estuvo tu experiencia?"
- Si no compró: agregar a nurture campaign
- Si compró: pedir review

### Analytics
- Tasa de show-up vs no-show
- Tasa de conversión por tipo de cita
- Tiempos promedio de duración

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### VehiclesSaleService / PropertiesSaleService
- Botón "Agendar Test Drive" / "Schedule Tour"
- Pre-llenar información del vehículo/propiedad

### UserService
- Obtener info del cliente
- Historial de citas previas

### NotificationService
- Enviar confirmaciones
- Recordatorios automáticos
- Notificaciones de cambios

### CRMService
- Registrar interacción
- Lead scoring
- Follow-up automation

---

## 🎯 BUSINESS RULES

### Cancelaciones
- Clientes pueden cancelar hasta 2h antes sin penalización
- Cancelar < 2h: marcar como "Late Cancellation"
- 3 no-shows: suspender capacidad de agendar por 30 días

### Duración por Tipo
- Test Drive: 30 min
- Property Tour: 45 min
- Consultation: 30 min
- Inspection: 60 min

### Slots Disponibles
- No permitir agendar < 2h de anticipación
- Máximo 30 días en el futuro
- Respetar horarios de negocio (9AM - 6PM)

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0
