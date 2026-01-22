# 🛠️ Admin Service - Panel de Administración - Matriz de Procesos

> **Servicio:** AdminService  
> **Puerto:** 5011  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 1. Información General

### 1.1 Descripción

Panel de administración centralizado para la plataforma OKLA. Proporciona funcionalidades de gestión de usuarios, dealers, vehículos, moderación de contenido, configuración del sistema, y monitoreo en tiempo real.

### 1.2 Funcionalidades Principales

| Módulo              | Descripción                          |
| ------------------- | ------------------------------------ |
| Dashboard           | Métricas en tiempo real, KPIs        |
| User Management     | Gestión de usuarios y permisos       |
| Dealer Management   | Aprobación, verificación, suspensión |
| Listings Moderation | Moderación de vehículos              |
| Support Tickets     | Gestión de tickets                   |
| System Config       | Configuración centralizada           |
| Reports             | Reportes y exportaciones             |
| Audit Log           | Historial de acciones                |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                       Admin Panel Architecture                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                      Admin Frontend (React)                      │   │
│   │   https://admin.okla.com.do                                      │   │
│   │                                                                   │   │
│   │   ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐           │   │
│   │   │Dashboard │ │ Users   │ │ Dealers  │ │Moderation│           │   │
│   │   └──────────┘ └──────────┘ └──────────┘ └──────────┘           │   │
│   │   ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐           │   │
│   │   │ Support  │ │ Config  │ │ Reports  │ │  Audit   │           │   │
│   │   └──────────┘ └──────────┘ └──────────┘ └──────────┘           │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│                                   ▼                                      │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                        API Gateway                               │   │
│   │                    /admin/* routes                               │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│                                   ▼                                      │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                       AdminService API                           │   │
│   │                                                                   │   │
│   │   ┌────────────────┐  ┌────────────────┐  ┌────────────────┐    │   │
│   │   │ Dashboard      │  │ User Mgmt      │  │ Dealer Mgmt    │    │   │
│   │   │ Controller     │  │ Controller     │  │ Controller     │    │   │
│   │   └────────────────┘  └────────────────┘  └────────────────┘    │   │
│   │   ┌────────────────┐  ┌────────────────┐  ┌────────────────┐    │   │
│   │   │ Moderation     │  │ Config         │  │ Reports        │    │   │
│   │   │ Controller     │  │ Controller     │  │ Controller     │    │   │
│   │   └────────────────┘  └────────────────┘  └────────────────┘    │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│                                   ▼                                      │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                     Other Microservices                          │   │
│   │   UserSvc, DealerSvc, VehiclesSvc, BillingSvc, NotificationSvc  │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

### 2.1 Dashboard

| Método | Endpoint                             | Descripción          | Auth  |
| ------ | ------------------------------------ | -------------------- | ----- |
| `GET`  | `/api/admin/dashboard/overview`      | Resumen general      | Admin |
| `GET`  | `/api/admin/dashboard/realtime`      | Métricas tiempo real | Admin |
| `GET`  | `/api/admin/dashboard/kpis`          | KPIs principales     | Admin |
| `GET`  | `/api/admin/dashboard/charts/{type}` | Datos para gráficos  | Admin |

### 2.2 User Management

| Método   | Endpoint                               | Descripción        | Auth       |
| -------- | -------------------------------------- | ------------------ | ---------- |
| `GET`    | `/api/admin/users`                     | Listar usuarios    | Admin      |
| `GET`    | `/api/admin/users/{id}`                | Detalle de usuario | Admin      |
| `PUT`    | `/api/admin/users/{id}`                | Actualizar usuario | Admin      |
| `POST`   | `/api/admin/users/{id}/suspend`        | Suspender usuario  | Admin      |
| `POST`   | `/api/admin/users/{id}/activate`       | Activar usuario    | Admin      |
| `POST`   | `/api/admin/users/{id}/reset-password` | Reset password     | Admin      |
| `DELETE` | `/api/admin/users/{id}`                | Eliminar usuario   | SuperAdmin |

### 2.3 Dealer Management

| Método | Endpoint                                            | Descripción                     | Auth  |
| ------ | --------------------------------------------------- | ------------------------------- | ----- |
| `GET`  | `/api/admin/dealers`                                | Listar dealers                  | Admin |
| `GET`  | `/api/admin/dealers/{id}`                           | Detalle de dealer               | Admin |
| `GET`  | `/api/admin/dealers/pending`                        | Dealers pendientes verificación | Admin |
| `POST` | `/api/admin/dealers/{id}/verify`                    | Verificar dealer                | Admin |
| `POST` | `/api/admin/dealers/{id}/reject`                    | Rechazar dealer                 | Admin |
| `POST` | `/api/admin/dealers/{id}/suspend`                   | Suspender dealer                | Admin |
| `GET`  | `/api/admin/dealers/{id}/documents`                 | Documentos del dealer           | Admin |
| `POST` | `/api/admin/dealers/{id}/documents/{docId}/approve` | Aprobar documento               | Admin |

### 2.4 Moderation

| Método | Endpoint                                      | Descripción           | Auth      |
| ------ | --------------------------------------------- | --------------------- | --------- |
| `GET`  | `/api/admin/moderation/queue`                 | Cola de moderación    | Moderator |
| `GET`  | `/api/admin/moderation/listings`              | Listings pendientes   | Moderator |
| `POST` | `/api/admin/moderation/listings/{id}/approve` | Aprobar listing       | Moderator |
| `POST` | `/api/admin/moderation/listings/{id}/reject`  | Rechazar listing      | Moderator |
| `GET`  | `/api/admin/moderation/reports`               | Reportes de contenido | Moderator |
| `POST` | `/api/admin/moderation/reports/{id}/action`   | Tomar acción          | Moderator |

### 2.5 Support

| Método | Endpoint                                   | Descripción       | Auth    |
| ------ | ------------------------------------------ | ----------------- | ------- |
| `GET`  | `/api/admin/support/tickets`               | Listar tickets    | Support |
| `GET`  | `/api/admin/support/tickets/{id}`          | Detalle de ticket | Support |
| `PUT`  | `/api/admin/support/tickets/{id}`          | Actualizar ticket | Support |
| `POST` | `/api/admin/support/tickets/{id}/assign`   | Asignar ticket    | Support |
| `POST` | `/api/admin/support/tickets/{id}/escalate` | Escalar ticket    | Support |
| `POST` | `/api/admin/support/tickets/{id}/close`    | Cerrar ticket     | Support |

### 2.6 Reports & Export

| Método | Endpoint                        | Descripción         | Auth              |
| ------ | ------------------------------- | ------------------- | ----------------- |
| `GET`  | `/api/admin/reports/users`      | Reporte de usuarios | Admin             |
| `GET`  | `/api/admin/reports/dealers`    | Reporte de dealers  | Admin             |
| `GET`  | `/api/admin/reports/sales`      | Reporte de ventas   | Admin             |
| `GET`  | `/api/admin/reports/compliance` | Reporte compliance  | ComplianceOfficer |
| `POST` | `/api/admin/reports/export`     | Exportar reporte    | Admin             |

---

## 3. Entidades

### 3.1 AdminDashboardMetrics

```csharp
public class AdminDashboardMetrics
{
    public DateTime Timestamp { get; set; }

    // Users
    public int TotalUsers { get; set; }
    public int ActiveUsers24h { get; set; }
    public int NewUsersToday { get; set; }

    // Dealers
    public int TotalDealers { get; set; }
    public int ActiveDealers { get; set; }
    public int PendingVerification { get; set; }

    // Vehicles
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int PendingModeration { get; set; }
    public int SoldToday { get; set; }

    // Revenue
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal MRR { get; set; } // Monthly Recurring Revenue

    // Support
    public int OpenTickets { get; set; }
    public int EscalatedTickets { get; set; }
    public double AvgResponseTime { get; set; }
}
```

### 3.2 ModerationItem

```csharp
public class ModerationItem
{
    public Guid Id { get; set; }
    public ModerationItemType Type { get; set; }
    public Guid TargetId { get; set; }
    public string TargetName { get; set; } = string.Empty;
    public ModerationStatus Status { get; set; }
    public ModerationPriority Priority { get; set; }

    public string? Reason { get; set; }
    public Guid? ReportedByUserId { get; set; }
    public int ReportCount { get; set; }

    public Guid? AssignedToId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedById { get; set; }
    public string? Resolution { get; set; }
}

public enum ModerationItemType
{
    NewListing,
    ReportedListing,
    ReportedUser,
    ReportedReview,
    FlaggedMessage
}

public enum ModerationStatus
{
    Pending,
    InReview,
    Approved,
    Rejected,
    Escalated
}

public enum ModerationPriority
{
    Low,
    Medium,
    High,
    Critical
}
```

### 3.3 SupportTicket

```csharp
public class SupportTicket
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty; // OKLA-00001

    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;

    public TicketCategory Category { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid? AssignedToId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public List<TicketMessage> Messages { get; set; } = new();
}

public enum TicketCategory
{
    Account,
    Billing,
    Listing,
    Technical,
    Complaint,
    Other
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum TicketStatus
{
    Open,
    InProgress,
    WaitingOnCustomer,
    Escalated,
    Resolved,
    Closed
}
```

---

## 4. Procesos Detallados

### 4.1 ADMIN-001: Verificar Dealer

| Paso | Acción                                 | Sistema             | Validación        |
| ---- | -------------------------------------- | ------------------- | ----------------- |
| 1    | Admin abre lista de dealers pendientes | Frontend            | Admin auth        |
| 2    | Selecciona dealer para verificar       | Frontend            | Dealer exists     |
| 3    | Revisa documentos subidos              | Frontend            | Docs available    |
| 4    | Verifica RNC con DGII                  | AdminService        | RNC válido        |
| 5    | Aprueba/Rechaza documentos             | AdminService        | All docs reviewed |
| 6    | Si aprobado: cambiar status a Active   | DealerManagement    | Status updated    |
| 7    | Si rechazado: indicar razón            | AdminService        | Reason required   |
| 8    | Notificar al dealer                    | NotificationService | Email/SMS sent    |
| 9    | Registrar en audit log                 | AuditService        | Action logged     |

```csharp
public class VerifyDealerCommandHandler : IRequestHandler<VerifyDealerCommand, Result<DealerDto>>
{
    public async Task<Result<DealerDto>> Handle(VerifyDealerCommand request, CancellationToken ct)
    {
        var dealer = await _dealerRepository.GetByIdAsync(request.DealerId, ct);
        if (dealer == null)
            return Result.Failure<DealerDto>("Dealer not found");

        // 1. Validate all documents are reviewed
        var documents = await _documentRepository.GetByDealerAsync(dealer.Id, ct);
        var pendingDocs = documents.Where(d => d.Status == DocumentStatus.Pending);

        if (pendingDocs.Any())
            return Result.Failure<DealerDto>("All documents must be reviewed first");

        // 2. Check for rejected documents
        var rejectedDocs = documents.Where(d => d.Status == DocumentStatus.Rejected);
        if (rejectedDocs.Any())
        {
            dealer.VerificationStatus = VerificationStatus.Rejected;
            dealer.RejectionReason = "Documents rejected: " +
                string.Join(", ", rejectedDocs.Select(d => d.Type));
        }
        else
        {
            dealer.VerificationStatus = VerificationStatus.Verified;
            dealer.Status = DealerStatus.Active;
            dealer.VerifiedAt = DateTime.UtcNow;
            dealer.VerifiedById = request.AdminId;
        }

        await _dealerRepository.UpdateAsync(dealer, ct);

        // 3. Send notification
        await _notificationService.SendDealerVerificationResultAsync(
            dealer.Id,
            dealer.VerificationStatus == VerificationStatus.Verified);

        // 4. Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            Action = "DealerVerified",
            EntityType = "Dealer",
            EntityId = dealer.Id,
            ActorId = request.AdminId,
            Details = $"Status: {dealer.VerificationStatus}"
        }, ct);

        // 5. Publish event
        await _eventBus.PublishAsync(new DealerVerifiedEvent
        {
            DealerId = dealer.Id,
            Verified = dealer.VerificationStatus == VerificationStatus.Verified,
            VerifiedById = request.AdminId
        }, ct);

        return Result.Success(_mapper.Map<DealerDto>(dealer));
    }
}
```

### 4.2 ADMIN-002: Moderar Listing

| Paso | Acción                                | Sistema             | Validación        |
| ---- | ------------------------------------- | ------------------- | ----------------- |
| 1    | Moderador abre cola de moderación     | Frontend            | Moderator auth    |
| 2    | Sistema muestra listado por prioridad | AdminService        | Priority order    |
| 3    | Moderador revisa fotos y descripción  | Frontend            | Content loaded    |
| 4    | Verificar con checklist               | Frontend            | All checks done   |
| 5    | Aprobar o rechazar                    | AdminService        | Decision made     |
| 6    | Si rechazado: seleccionar razón       | AdminService        | Reason selected   |
| 7    | Actualizar status del vehículo        | VehiclesSaleService | Status updated    |
| 8    | Notificar al vendedor                 | NotificationService | Notification sent |
| 9    | Actualizar métricas del moderador     | AdminService        | Stats updated     |

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Moderation Checklist                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   ☑ Fotos                                                               │
│   ├─ [ ] Mínimo 6 fotos                                                 │
│   ├─ [ ] Fotos claras y bien iluminadas                                 │
│   ├─ [ ] Vehículo completo visible                                      │
│   ├─ [ ] Interior visible                                               │
│   ├─ [ ] Sin logos de otros sitios                                      │
│   └─ [ ] Sin información personal visible                               │
│                                                                          │
│   ☑ Descripción                                                         │
│   ├─ [ ] Información precisa                                            │
│   ├─ [ ] Sin lenguaje inapropiado                                       │
│   ├─ [ ] Sin información de contacto externa                            │
│   └─ [ ] Precio razonable para el mercado                               │
│                                                                          │
│   ☑ Documentos                                                          │
│   ├─ [ ] Matrícula visible (si aplica)                                  │
│   └─ [ ] Título de propiedad (si premium)                               │
│                                                                          │
│   ☑ Compliance                                                          │
│   ├─ [ ] No es vehículo robado (check automático)                       │
│   └─ [ ] Seller verificado o en proceso                                 │
│                                                                          │
│   Decisión: ○ Aprobar  ○ Rechazar  ○ Solicitar cambios                 │
│                                                                          │
│   Razón de rechazo (si aplica):                                         │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │ ○ Fotos insuficientes                                            │   │
│   │ ○ Fotos de baja calidad                                          │   │
│   │ ○ Descripción inadecuada                                         │   │
│   │ ○ Precio sospechoso                                              │   │
│   │ ○ Posible fraude                                                 │   │
│   │ ○ Violación de términos                                          │   │
│   │ ○ Otro: ________________                                         │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.3 ADMIN-003: Gestionar Ticket de Soporte

| Paso | Acción                          | Sistema             | Validación        |
| ---- | ------------------------------- | ------------------- | ----------------- |
| 1    | Ticket asignado al agente       | AdminService        | Auto or manual    |
| 2    | Agente abre ticket              | Frontend            | Ticket loaded     |
| 3    | Revisar historial del usuario   | AdminService        | User context      |
| 4    | Responder al usuario            | AdminService        | Response saved    |
| 5    | Si requiere escalación          | AdminService        | Escalate flag     |
| 6    | Si resuelto: cerrar ticket      | AdminService        | Status = Resolved |
| 7    | Enviar encuesta de satisfacción | NotificationService | Survey sent       |
| 8    | Actualizar métricas SLA         | AdminService        | SLA tracked       |

---

## 5. Roles y Permisos

### 5.1 Matriz de Permisos

| Acción             | SuperAdmin | Admin | Moderator | Support | ComplianceOfficer |
| ------------------ | ---------- | ----- | --------- | ------- | ----------------- |
| View Dashboard     | ✅         | ✅    | ✅        | ✅      | ✅                |
| Manage Users       | ✅         | ✅    | ❌        | ❌      | ❌                |
| Delete Users       | ✅         | ❌    | ❌        | ❌      | ❌                |
| Verify Dealers     | ✅         | ✅    | ❌        | ❌      | ✅                |
| Moderate Listings  | ✅         | ✅    | ✅        | ❌      | ❌                |
| Handle Tickets     | ✅         | ✅    | ❌        | ✅      | ❌                |
| View Audit Log     | ✅         | ✅    | ❌        | ❌      | ✅                |
| System Config      | ✅         | ❌    | ❌        | ❌      | ❌                |
| Compliance Reports | ✅         | ❌    | ❌        | ❌      | ✅                |

---

## 6. Reglas de Negocio

| Código  | Regla                                               | Validación            |
| ------- | --------------------------------------------------- | --------------------- |
| ADM-R01 | SuperAdmin no puede eliminarse a sí mismo           | ActorId != TargetId   |
| ADM-R02 | Dealer rechazado debe tener razón                   | Reason required       |
| ADM-R03 | Ticket cerrado no puede reabrirse después de 7 días | ClosedAt < 7 days     |
| ADM-R04 | Acciones críticas requieren 2FA                     | 2FA verified          |
| ADM-R05 | Todas las acciones se registran en audit            | AuditLog entry        |
| ADM-R06 | SLA de primera respuesta: 4 horas                   | FirstResponseAt check |

---

## 7. Códigos de Error

| Código      | HTTP | Mensaje                  | Causa                  |
| ----------- | ---- | ------------------------ | ---------------------- |
| `ADMIN_001` | 403  | Insufficient permissions | No tiene permiso       |
| `ADMIN_002` | 404  | User not found           | Usuario no existe      |
| `ADMIN_003` | 404  | Dealer not found         | Dealer no existe       |
| `ADMIN_004` | 400  | Cannot delete yourself   | Auto-eliminación       |
| `ADMIN_005` | 400  | Reason required          | Falta razón de rechazo |
| `ADMIN_006` | 400  | 2FA required             | Acción requiere 2FA    |

---

## 8. Eventos RabbitMQ

| Evento                  | Exchange       | Descripción        |
| ----------------------- | -------------- | ------------------ |
| `UserSuspendedEvent`    | `admin.events` | Usuario suspendido |
| `DealerVerifiedEvent`   | `admin.events` | Dealer verificado  |
| `ListingModeratedEvent` | `admin.events` | Listing moderado   |
| `TicketEscalatedEvent`  | `admin.events` | Ticket escalado    |
| `AdminActionEvent`      | `audit.events` | Acción de admin    |

---

## 9. Configuración

```json
{
  "AdminService": {
    "AllowedIPs": ["10.0.0.0/8", "172.16.0.0/12"],
    "SessionTimeout": 480,
    "RequireTwoFactor": true,
    "ModerationSLA": {
      "NewListingHours": 24,
      "ReportedContentHours": 4
    },
    "SupportSLA": {
      "FirstResponseHours": 4,
      "ResolutionHours": 48
    }
  }
}
```

---

## 10. Métricas Prometheus

```
# Admin actions
admin_actions_total{action="...", role="..."}

# Moderation
moderation_queue_size{priority="..."}
moderation_resolution_time_seconds{decision="..."}

# Support
support_tickets_open{priority="...", category="..."}
support_first_response_seconds
support_resolution_seconds
```

---

## 📚 Referencias

- [04-audit-service.md](04-audit-service.md) - Servicio de auditoría
- [02-admin-users.md](02-admin-users.md) - Gestión de usuarios admin
- [03-system-config.md](03-system-config.md) - Configuración del sistema
