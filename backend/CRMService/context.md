# CRMService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** CRMService
- **Puerto en Desarrollo:** 5030
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`crmservice`)
- **Imagen Docker:** Local only

### Propósito
Customer Relationship Management. Gestión de leads, pipeline de ventas, seguimiento de interacciones, campañas de marketing y automatización de follow-ups.

---

## 🏗️ ARQUITECTURA

```
CRMService/
├── CRMService.Api/
│   ├── Controllers/
│   │   ├── LeadsController.cs
│   │   ├── OpportunitiesController.cs
│   │   ├── ActivitiesController.cs
│   │   └── CampaignsController.cs
│   └── Program.cs
├── CRMService.Application/
├── CRMService.Domain/
│   ├── Entities/
│   │   ├── Lead.cs
│   │   ├── Opportunity.cs
│   │   ├── Activity.cs
│   │   └── Campaign.cs
│   └── Enums/
│       ├── LeadStatus.cs
│       └── OpportunityStage.cs
└── CRMService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### Lead
```csharp
public class Lead
{
    public Guid Id { get; set; }
    public string LeadNumber { get; set; }         // LEAD-2026-001234
    
    // Información de contacto
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string? Company { get; set; }
    
    // Interés
    public string? InterestedIn { get; set; }      // "Vehicle", "Property"
    public Guid? InterestedEntityId { get; set; }
    public string? Budget { get; set; }
    
    // Origen
    public LeadSource Source { get; set; }         // Website, Facebook, Referral, Walk-in
    public string? SourceDetails { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    
    // Asignación
    public Guid? AssignedToUserId { get; set; }    // Sales agent
    public string? AssignedToName { get; set; }
    public DateTime? AssignedAt { get; set; }
    
    // Calificación
    public LeadStatus Status { get; set; }         // New, Contacted, Qualified, Unqualified, Converted
    public int LeadScore { get; set; }             // 0-100
    public string? QualificationNotes { get; set; }
    
    // Conversión
    public bool IsConverted { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public Guid? OpportunityId { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public DateTime? NextFollowUpAt { get; set; }
}
```

### Opportunity (Lead calificado → venta potencial)
```csharp
public class Opportunity
{
    public Guid Id { get; set; }
    public string OpportunityNumber { get; set; }
    
    // Lead origen
    public Guid LeadId { get; set; }
    public Lead Lead { get; set; }
    
    // Deal
    public string Name { get; set; }               // "Toyota Corolla sale - Juan Pérez"
    public decimal EstimatedValue { get; set; }
    public string Currency { get; set; } = "DOP";
    
    // Pipeline stage
    public OpportunityStage Stage { get; set; }    // Qualification, Proposal, Negotiation, Closed-Won, Closed-Lost
    public int Probability { get; set; }           // % chance de cerrar (0-100)
    public DateTime? ExpectedCloseDate { get; set; }
    
    // Asignación
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; }
    
    // Resultado
    public bool IsClosed { get; set; }
    public bool IsWon { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? LossReason { get; set; }        // "Price too high", "Bought from competitor"
    
    // Tracking
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
```

### Activity (Interacciones con leads/customers)
```csharp
public class Activity
{
    public Guid Id { get; set; }
    
    // Tipo
    public ActivityType Type { get; set; }         // Call, Email, Meeting, Task, Note
    public string Subject { get; set; }
    public string? Description { get; set; }
    
    // Relacionado a
    public Guid? LeadId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? CustomerId { get; set; }
    
    // Responsable
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    
    // Scheduling (para llamadas/reuniones futuras)
    public DateTime? ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    
    // Resultado
    public string? Outcome { get; set; }           // "Left voicemail", "Scheduled meeting", "No answer"
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

### Campaign
```csharp
public class Campaign
{
    public Guid Id { get; set; }
    
    // Información
    public string Name { get; set; }
    public string? Description { get; set; }
    public CampaignType Type { get; set; }         // Email, SMS, Social, PPC
    public CampaignStatus Status { get; set; }     // Draft, Active, Paused, Completed
    
    // Timing
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Budget
    public decimal? Budget { get; set; }
    public decimal? ActualCost { get; set; }
    
    // Targeting
    public string? TargetAudience { get; set; }    // JSON criteria
    
    // Resultados
    public int LeadsGenerated { get; set; }
    public int Conversions { get; set; }
    public decimal ROI { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Leads
- `POST /api/leads` - Crear lead (desde formulario web)
- `GET /api/leads` - Listar leads (filtros: status, assigned, source)
- `GET /api/leads/{id}` - Detalle de lead
- `PUT /api/leads/{id}` - Actualizar lead
- `PUT /api/leads/{id}/assign` - Asignar lead a agente
- `PUT /api/leads/{id}/qualify` - Calificar lead
- `POST /api/leads/{id}/convert` - Convertir lead a opportunity
- `GET /api/leads/my-leads` - Leads asignados al usuario

### Opportunities
- `GET /api/opportunities` - Listar opportunities (pipeline view)
- `GET /api/opportunities/{id}` - Detalle
- `PUT /api/opportunities/{id}/stage` - Mover a siguiente stage
- `PUT /api/opportunities/{id}/close-won` - Marcar como ganada
- `PUT /api/opportunities/{id}/close-lost` - Marcar como perdida

### Activities
- `POST /api/activities` - Registrar actividad
- `GET /api/activities` - Listar actividades
- `GET /api/activities/upcoming` - Actividades pendientes
- `PUT /api/activities/{id}/complete` - Marcar como completada

### Campaigns
- `POST /api/campaigns` - Crear campaña
- `GET /api/campaigns` - Listar campañas
- `GET /api/campaigns/{id}/performance` - Métricas de campaña

---

## 💡 FUNCIONALIDADES PLANEADAS

### Lead Scoring Automático
```csharp
public class LeadScoringEngine
{
    public int CalculateScore(Lead lead)
    {
        int score = 0;
        
        // Budget (30 puntos)
        if (lead.Budget == "50K+") score += 30;
        else if (lead.Budget == "25K-50K") score += 20;
        
        // Engagement (25 puntos)
        if (lead.LastContactedAt != null && 
            (DateTime.UtcNow - lead.LastContactedAt.Value).Days < 7)
            score += 25;
        
        // Source quality (20 puntos)
        if (lead.Source == LeadSource.Referral) score += 20;
        else if (lead.Source == LeadSource.Website) score += 15;
        
        // Completeness (25 puntos)
        if (!string.IsNullOrEmpty(lead.Phone)) score += 10;
        if (!string.IsNullOrEmpty(lead.Email)) score += 10;
        if (lead.InterestedEntityId != null) score += 5;
        
        return Math.Min(score, 100);
    }
}
```

### Pipeline Kanban View
Visualización del pipeline de ventas:
```
[New] → [Contacted] → [Qualified] → [Proposal] → [Negotiation] → [Closed-Won]
  12        45           23           15            8               5
```

### Automated Follow-ups
- Si lead no contactado en 24h → asignar tarea automática
- Si opportunity en "Negotiation" > 7 días → recordatorio
- Si lead calificado no convertido en 30 días → re-engagement campaign

### Email Templates
Templates predefinidos para:
- Primer contacto
- Follow-up después de test drive
- Propuesta de precio
- Thank you después de venta

### Activity Timeline
Vista cronológica de todas las interacciones con un lead:
```
Jan 7, 10:30 AM - Email sent: "Initial inquiry"
Jan 7, 2:45 PM  - Call completed: "Discussed budget"
Jan 8, 11:00 AM - Meeting scheduled: "Test drive"
Jan 8, 3:00 PM  - Note added: "Customer loved the car"
```

### Reports & Analytics
- Leads by source (¿qué canal genera más leads?)
- Conversion funnel (lead → qualified → opportunity → won)
- Average time to close
- Win rate por agente
- Revenue forecast

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### ContactService
- Inquiry recibido → crear Lead automáticamente

### AppointmentService
- Appointment agendado → crear Activity

### VehiclesSaleService
- Usuario muestra interés → crear Lead con InterestedEntityId

### NotificationService
- Recordatorios de follow-up
- Alertas de leads calientes no atendidos

### UserService
- Asignar leads a sales agents

### BillingService
- Opportunity cerrada → crear invoice

---

## 🔄 EVENTOS CONSUMIDOS (RabbitMQ)

### InquiryCreated (ContactService)
```json
{
  "inquiryId": "uuid",
  "senderName": "Juan Pérez",
  "senderEmail": "juan@example.com",
  "interestedIn": "Vehicle",
  "entityId": "vehicle-uuid"
}
```
→ Crear Lead automáticamente

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0
