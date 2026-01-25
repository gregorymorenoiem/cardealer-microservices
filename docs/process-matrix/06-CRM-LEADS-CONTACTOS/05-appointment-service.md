# 📅 Appointment Service - Test Drives - Matriz de Procesos

> **Servicio:** AppointmentService  
> **Puerto:** 5064  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente                         | Total | Implementado | Pendiente | Estado         |
| ---------------------------------- | ----- | ------------ | --------- | -------------- |
| **Controllers**                    | 2     | 0            | 2         | 🔴 Pendiente   |
| **APPT-BOOK-\*** (Reservar)        | 4     | 0            | 4         | 🔴 Pendiente   |
| **APPT-AVAIL-\*** (Disponibilidad) | 4     | 0            | 4         | 🔴 Pendiente   |
| **APPT-REMIND-\*** (Recordatorios) | 3     | 0            | 3         | 🔴 Pendiente   |
| **APPT-CANCEL-\*** (Cancelación)   | 3     | 0            | 3         | 🔴 Pendiente   |
| **APPT-SYNC-\*** (Calendario)      | 3     | 0            | 3         | 🔴 Pendiente   |
| **Tests**                          | 0     | 0            | 18        | 🔴 Pendiente   |
| **TOTAL**                          | 19    | 0            | 19        | 🔴 0% Completo |

---

## 1. Información General

### 1.1 Descripción

Sistema de agendamiento de citas para test drives de vehículos. Gestiona la disponibilidad de vendedores, sincronización con calendarios, recordatorios automáticos y confirmaciones.

### 1.2 Dependencias

| Servicio            | Propósito               |
| ------------------- | ----------------------- |
| VehiclesSaleService | Datos del vehículo      |
| UserService         | Datos del comprador     |
| DealerService       | Horarios y ubicación    |
| NotificationService | Recordatorios           |
| LeadService         | Creación de leads       |
| CalendarService     | Sync con Google/Outlook |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     AppointmentService Architecture                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   User Actions                       Core Service                            │
│   ┌────────────────┐                ┌────────────────────────────────┐      │
│   │ Buyers         │──┐             │       AppointmentService         │      │
│   │ (Book Test Dr.)│  │             │  ┌──────────────────────────┐   │      │
│   └────────────────┘  │             │  │ Controllers              │   │      │
│   ┌────────────────┐  │             │  │ • AppointmentsController │   │      │
│   │ Dealers        │──┼────────────▶│  │ • AvailabilityController │   │      │
│   │ (Manage Slots) │  │             │  │ • DealerApptController   │   │      │
│   └────────────────┘  │             │  └──────────────────────────┘   │      │
│   ┌────────────────┐  │             │  ┌──────────────────────────┐   │      │
│   │ Vehicle Detail │──┘             │  │ Application (CQRS)       │   │      │
│   │ (Schedule Btn) │               │  │ • CreateAppointmentCmd   │   │      │
│   └────────────────┘               │  │ • CheckAvailabilityQuery │   │      │
│                                    │  │ • SendReminderCommand    │   │      │
│   External                         │  │ • SyncCalendarCommand    │   │      │
│   ┌────────────────┐               │  └──────────────────────────┘   │      │
│   │ Google Cal.    │◀─────────────│  ┌──────────────────────────┐   │      │
│   │ Outlook        │               │  │ Domain                   │   │      │
│   └────────────────┘               │  │ • Appointment            │   │      │
│   ┌────────────────┐               │  │ • AvailabilitySlot       │   │      │
│   │ Notification   │◀─────────────│  │ • Reminder, BlockedTime  │   │      │
│   │ Service        │               │  └──────────────────────────┘   │      │
│   └────────────────┘               └────────────────────────────────┘      │
│                                                    │                        │
│                                    ┌───────────────┼───────────────┐        │
│                                    ▼               ▼               ▼        │
│                            ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│                            │ PostgreSQL │  │   Redis    │  │  RabbitMQ  │  │
│                            │ (Appts,    │  │  (Slots,   │  │ (Reminder  │  │
│                            │  Slots)    │  │  Avail.)   │  │  Events)   │  │
│                            └────────────┘  └────────────┘  └────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints API

### 2.1 AppointmentsController

| Método | Endpoint                            | Descripción           | Auth | Roles  |
| ------ | ----------------------------------- | --------------------- | ---- | ------ |
| `GET`  | `/api/appointments/availability`    | Slots disponibles     | ❌   | Public |
| `POST` | `/api/appointments`                 | Crear cita            | ❌   | Public |
| `GET`  | `/api/appointments/{id}`            | Obtener cita          | ✅   | Owner  |
| `PUT`  | `/api/appointments/{id}`            | Modificar cita        | ✅   | Owner  |
| `POST` | `/api/appointments/{id}/confirm`    | Confirmar asistencia  | ✅   | Owner  |
| `POST` | `/api/appointments/{id}/cancel`     | Cancelar cita         | ✅   | Owner  |
| `POST` | `/api/appointments/{id}/reschedule` | Reprogramar           | ✅   | Owner  |
| `POST` | `/api/appointments/{id}/complete`   | Marcar completada     | ✅   | Dealer |
| `POST` | `/api/appointments/{id}/no-show`    | Marcar no-show        | ✅   | Dealer |
| `GET`  | `/api/appointments/my`              | Mis citas (comprador) | ✅   | User   |

### 2.2 DealerAppointmentsController

| Método   | Endpoint                              | Descripción           | Auth | Roles  |
| -------- | ------------------------------------- | --------------------- | ---- | ------ |
| `GET`    | `/api/dealer/appointments`            | Citas del dealer      | ✅   | Dealer |
| `GET`    | `/api/dealer/appointments/today`      | Citas de hoy          | ✅   | Dealer |
| `GET`    | `/api/dealer/appointments/upcoming`   | Próximas citas        | ✅   | Dealer |
| `PUT`    | `/api/dealer/availability`            | Config disponibilidad | ✅   | Dealer |
| `GET`    | `/api/dealer/availability`            | Ver disponibilidad    | ✅   | Dealer |
| `POST`   | `/api/dealer/availability/block`      | Bloquear horario      | ✅   | Dealer |
| `DELETE` | `/api/dealer/availability/block/{id}` | Desbloquear           | ✅   | Dealer |

---

## 3. Entidades y Enums

### 3.1 AppointmentStatus (Enum)

```csharp
public enum AppointmentStatus
{
    Pending = 0,          // Creada, esperando confirmación
    Confirmed = 1,        // Confirmada por usuario
    Completed = 2,        // Test drive realizado
    Cancelled = 3,        // Cancelada
    NoShow = 4,           // No se presentó
    Rescheduled = 5       // Reprogramada
}
```

### 3.2 AppointmentType (Enum)

```csharp
public enum AppointmentType
{
    TestDrive = 0,        // Prueba de manejo
    Inspection = 1,       // Inspección del vehículo
    Consultation = 2,     // Consulta general
    Delivery = 3          // Entrega de vehículo
}
```

### 3.3 Appointment (Entidad)

```csharp
public class Appointment
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid DealerId { get; set; }
    public Guid? UserId { get; set; }              // Si está registrado
    public Guid? LeadId { get; set; }

    // Tipo y estado
    public AppointmentType Type { get; set; }
    public AppointmentStatus Status { get; set; }

    // Fecha y hora
    public DateTime ScheduledDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public string TimeZone { get; set; }

    // Contacto (si no registrado)
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public string CustomerEmail { get; set; }

    // Ubicación
    public Guid? LocationId { get; set; }          // Sucursal
    public string? CustomLocation { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Notas
    public string? CustomerNotes { get; set; }
    public string? DealerNotes { get; set; }

    // Recordatorios
    public bool ReminderSent24h { get; set; }
    public bool ReminderSent1h { get; set; }
    public bool ConfirmationSent { get; set; }

    // Feedback
    public int? SatisfactionRating { get; set; }   // 1-5
    public string? FeedbackNotes { get; set; }

    // Resultado
    public bool? InterestedAfterTestDrive { get; set; }
    public string? Outcome { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}
```

### 3.4 DealerAvailability (Entidad)

```csharp
public class DealerAvailability
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }

    // Horario regular
    public Dictionary<DayOfWeek, DaySchedule> WeeklySchedule { get; set; }

    // Configuración
    public int SlotDurationMinutes { get; set; }   // 30, 45, 60
    public int BufferBetweenMinutes { get; set; }  // 15
    public int MaxConcurrentAppointments { get; set; }
    public int AdvanceBookingDays { get; set; }    // Max días adelante
    public int MinAdvanceHours { get; set; }       // Min horas adelante

    // Fechas bloqueadas
    public List<DateBlock> BlockedDates { get; set; }
}

public class DaySchedule
{
    public bool IsOpen { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public List<TimeSpan>? BreakStart { get; set; }
    public List<TimeSpan>? BreakEnd { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 APPT-001: Obtener Disponibilidad

| Campo       | Valor                              |
| ----------- | ---------------------------------- |
| **ID**      | APPT-001                           |
| **Nombre**  | Obtener Slots Disponibles          |
| **Actor**   | Usuario                            |
| **Trigger** | GET /api/appointments/availability |

#### Flujo del Proceso

| Paso | Acción                     | Sistema            | Validación           |
| ---- | -------------------------- | ------------------ | -------------------- |
| 1    | Usuario ve vehículo        | Frontend           | VehicleDetail        |
| 2    | Click "Agendar Test Drive" | Frontend           | Abre modal           |
| 3    | Seleccionar fecha          | Frontend           | Calendar picker      |
| 4    | Request disponibilidad     | AppointmentService | Por fecha y dealerId |
| 5    | Obtener horario regular    | Database           | DealerAvailability   |
| 6    | Obtener citas existentes   | Database           | Por fecha            |
| 7    | Obtener bloqueos           | Database           | BlockedDates         |
| 8    | Calcular slots libres      | AppointmentService | Algoritmo            |
| 9    | Filtrar por min advance    | AppointmentService | Si es hoy            |
| 10   | Retornar slots             | Response           | Array de tiempos     |

#### Request

```json
{
  "dealerId": "uuid",
  "vehicleId": "uuid",
  "date": "2026-01-22",
  "type": "TestDrive"
}
```

#### Response

```json
{
  "date": "2026-01-22",
  "dealerName": "Autos del Caribe",
  "location": "Av. Churchill #75, Santo Domingo",
  "slots": [
    { "startTime": "09:00", "endTime": "10:00", "available": true },
    { "startTime": "10:00", "endTime": "11:00", "available": false },
    { "startTime": "11:00", "endTime": "12:00", "available": true },
    { "startTime": "14:00", "endTime": "15:00", "available": true },
    { "startTime": "15:00", "endTime": "16:00", "available": true },
    { "startTime": "16:00", "endTime": "17:00", "available": true }
  ],
  "nextAvailableDate": "2026-01-23"
}
```

---

### 4.2 APPT-002: Crear Cita de Test Drive

| Campo       | Valor                  |
| ----------- | ---------------------- |
| **ID**      | APPT-002               |
| **Nombre**  | Agendar Test Drive     |
| **Actor**   | Usuario                |
| **Trigger** | POST /api/appointments |

#### Flujo del Proceso

| Paso | Acción                        | Sistema             | Validación              |
| ---- | ----------------------------- | ------------------- | ----------------------- |
| 1    | Usuario selecciona slot       | Frontend            | Slot disponible         |
| 2    | Ingresar datos contacto       | Frontend            | Nombre, teléfono, email |
| 3    | Notas adicionales             | Frontend            | Opcional                |
| 4    | Submit solicitud              | API                 | POST                    |
| 5    | Verificar slot aún disponible | AppointmentService  | Race condition check    |
| 6    | Si ocupado                    | Response            | 409 Conflict            |
| 7    | Crear cita                    | Database            | Status = Pending        |
| 8    | Crear/actualizar lead         | LeadService         | +20 puntos              |
| 9    | Enviar confirmación           | NotificationService | Email + WhatsApp        |
| 10   | Notificar dealer              | NotificationService | Push + email            |
| 11   | Agregar a calendario          | CalendarService     | ICS + Google/Outlook    |
| 12   | Publicar evento               | RabbitMQ            | appointment.created     |

#### Request

```json
{
  "vehicleId": "uuid",
  "dealerId": "uuid",
  "scheduledDate": "2026-01-22",
  "startTime": "14:00",
  "type": "TestDrive",
  "customerName": "Juan Pérez",
  "customerPhone": "+18295550100",
  "customerEmail": "juan@email.com",
  "notes": "Prefiero probar en autopista si es posible"
}
```

#### Response

```json
{
  "id": "uuid",
  "confirmationCode": "TD-2026-00123",
  "status": "Pending",
  "vehicle": {
    "title": "Toyota RAV4 XLE 2024",
    "image": "https://..."
  },
  "dealer": {
    "name": "Autos del Caribe",
    "address": "Av. Churchill #75",
    "phone": "809-555-0100"
  },
  "scheduledDate": "2026-01-22",
  "startTime": "14:00",
  "endTime": "15:00",
  "message": "Tu test drive está agendado. Te enviaremos un recordatorio."
}
```

---

### 4.3 APPT-003: Enviar Recordatorios

| Campo       | Valor                     |
| ----------- | ------------------------- |
| **ID**      | APPT-003                  |
| **Nombre**  | Recordatorios Automáticos |
| **Actor**   | Sistema (Job)             |
| **Trigger** | Cron cada 15 minutos      |

#### Flujo del Proceso (24 horas antes)

| Paso | Acción                    | Sistema             | Validación                 |
| ---- | ------------------------- | ------------------- | -------------------------- |
| 1    | Job ejecuta               | SchedulerService    | Cada 15 min                |
| 2    | Buscar citas mañana       | Database            | Status = Pending/Confirmed |
| 3    | Filtrar sin reminder 24h  | Database            | ReminderSent24h = false    |
| 4    | Por cada cita             | Loop                | Procesar                   |
| 5    | Enviar email recordatorio | NotificationService | Template reminder_24h      |
| 6    | Enviar WhatsApp           | WhatsApp API        | Con botones                |
| 7    | Marcar enviado            | Database            | ReminderSent24h = true     |
| 8    | Log envío                 | Database            | Para auditoría             |

#### Flujo del Proceso (1 hora antes)

| Paso | Acción                    | Sistema             | Validación             |
| ---- | ------------------------- | ------------------- | ---------------------- |
| 1    | Buscar citas próxima hora | Database            | Status = Confirmed     |
| 2    | Filtrar sin reminder 1h   | Database            | ReminderSent1h = false |
| 3    | Enviar SMS/WhatsApp       | NotificationService | Mensaje corto          |
| 4    | Incluir mapa              | Message             | Link a Google Maps     |
| 5    | Marcar enviado            | Database            | ReminderSent1h = true  |

#### Template WhatsApp 24h

```
📅 Recordatorio de Test Drive - OKLA

Hola {{customerName}},

Te recordamos tu cita de test drive:

🚗 {{vehicleTitle}}
📅 Mañana, {{date}} a las {{time}}
📍 {{dealerName}}
    {{dealerAddress}}

[Ver en mapa] [Confirmar asistencia] [Reprogramar]

¿Tienes alguna pregunta? Responde a este mensaje.
```

---

### 4.4 APPT-004: Completar Test Drive

| Campo       | Valor                                |
| ----------- | ------------------------------------ |
| **ID**      | APPT-004                             |
| **Nombre**  | Registrar Resultado de Test Drive    |
| **Actor**   | Dealer                               |
| **Trigger** | POST /api/appointments/{id}/complete |

#### Flujo del Proceso

| Paso | Acción               | Sistema             | Validación            |
| ---- | -------------------- | ------------------- | --------------------- |
| 1    | Test drive realizado | Presencial          | Cliente llegó         |
| 2    | Dealer accede a app  | Mobile/Dashboard    | Autenticado           |
| 3    | Buscar cita de hoy   | AppointmentService  | Lista del día         |
| 4    | Click "Completar"    | Frontend            | Modal                 |
| 5    | Registrar resultado  | Frontend            | Interesado? Notas     |
| 6    | Submit               | API                 | POST /complete        |
| 7    | Actualizar status    | Database            | Completed             |
| 8    | Actualizar lead      | LeadService         | Notas + score         |
| 9    | Enviar encuesta      | NotificationService | Después de 2h         |
| 10   | Publicar evento      | RabbitMQ            | appointment.completed |

#### Request

```json
{
  "interestedAfterTestDrive": true,
  "outcome": "Cliente muy interesado, pidió cotización de financiamiento",
  "dealerNotes": "Probó SUV y sedán, prefirió el RAV4. Presupuesto ~$2M. Seguimiento en 2 días.",
  "nextSteps": "FinancingQuote"
}
```

---

### 4.5 APPT-005: Cancelar Cita

| Campo       | Valor                              |
| ----------- | ---------------------------------- |
| **ID**      | APPT-005                           |
| **Nombre**  | Cancelar Cita                      |
| **Actor**   | Usuario/Dealer                     |
| **Trigger** | POST /api/appointments/{id}/cancel |

#### Flujo del Proceso

| Paso | Acción                 | Sistema             | Validación            |
| ---- | ---------------------- | ------------------- | --------------------- |
| 1    | Usuario/dealer cancela | Frontend            | Botón cancelar        |
| 2    | Seleccionar razón      | Frontend            | Dropdown              |
| 3    | Confirmar cancelación  | Frontend            | Modal                 |
| 4    | Validar política       | AppointmentService  | Min 2h antes          |
| 5    | Actualizar status      | Database            | Cancelled             |
| 6    | Liberar slot           | AppointmentService  | Disponible de nuevo   |
| 7    | Notificar contraparte  | NotificationService | Email/WhatsApp        |
| 8    | Actualizar lead        | LeadService         | Si aplica             |
| 9    | Sugerir reprogramar    | Response            | Próximos slots        |
| 10   | Publicar evento        | RabbitMQ            | appointment.cancelled |

---

## 5. Reglas de Negocio

### 5.1 Políticas de Tiempo

| Regla                            | Valor         |
| -------------------------------- | ------------- |
| Anticipación mínima para agendar | 2 horas       |
| Anticipación máxima              | 30 días       |
| Cancelación sin penalidad        | 2 horas antes |
| Duración test drive default      | 60 minutos    |
| Buffer entre citas               | 15 minutos    |

### 5.2 Límites

| Regla                                  | Valor  |
| -------------------------------------- | ------ |
| Max citas por día por dealer           | 10     |
| Max citas activas por usuario          | 3      |
| Max no-shows antes de bloqueo          | 2      |
| Días de anticipación para recordatorio | 1 + 1h |

### 5.3 Horarios Default

| Día             | Horario       |
| --------------- | ------------- |
| Lunes - Viernes | 9:00 - 18:00  |
| Sábado          | 9:00 - 14:00  |
| Domingo         | Cerrado       |
| Almuerzo        | 12:00 - 14:00 |

---

## 6. Eventos RabbitMQ

| Evento                      | Exchange             | Payload                       |
| --------------------------- | -------------------- | ----------------------------- |
| `appointment.created`       | `appointment.events` | `{ id, vehicleId, dealerId }` |
| `appointment.confirmed`     | `appointment.events` | `{ id }`                      |
| `appointment.cancelled`     | `appointment.events` | `{ id, reason }`              |
| `appointment.rescheduled`   | `appointment.events` | `{ id, oldDate, newDate }`    |
| `appointment.completed`     | `appointment.events` | `{ id, interested }`          |
| `appointment.noshow`        | `appointment.events` | `{ id, userId }`              |
| `appointment.reminder_sent` | `appointment.events` | `{ id, type }`                |

---

## 7. Métricas

```
# Citas
appointments_created_total{type="testdrive|inspection"}
appointments_completed_total
appointments_cancelled_total{reason="..."}
appointments_noshow_total

# Conversión
appointments_to_lead_conversion_rate
appointments_to_sale_conversion_rate

# Timing
appointment_booking_lead_time_hours
appointment_completion_rate

# Satisfaction
appointment_satisfaction_score{rating="1|2|3|4|5"}
```

---

## 8. Configuración

```json
{
  "Appointments": {
    "DefaultDuration": 60,
    "BufferMinutes": 15,
    "MinAdvanceHours": 2,
    "MaxAdvanceDays": 30,
    "MaxPerDayPerDealer": 10,
    "MaxActivePerUser": 3,
    "MaxNoShowsBeforeBlock": 2,
    "ReminderHoursBefore": [24, 1]
  }
}
```

---

## 📚 Referencias

- [02-lead-service.md](02-lead-service.md) - Gestión de leads
- [04-chatbot-service.md](04-chatbot-service.md) - Agendamiento via chatbot
- [01-notification-service.md](../07-NOTIFICACIONES/01-notification-service.md) - Recordatorios
