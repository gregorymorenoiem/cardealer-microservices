# 📚 Centro de Ayuda y Soporte

> **Código:** HELP-001, HELP-002, HELP-003  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🟡 MEDIA (Satisfacción del cliente)

---

## 📋 Información General

| Campo             | Valor                                            |
| ----------------- | ------------------------------------------------ |
| **Servicio**      | SupportService                                   |
| **Puerto**        | 5087                                             |
| **Base de Datos** | `supportservice`                                 |
| **Dependencias**  | UserService, NotificationService, ChatbotService |

---

## 🎯 Objetivo del Proceso

1. **Centro de Ayuda (FAQ):** Base de conocimiento searchable
2. **Tickets de Soporte:** Sistema de tickets para problemas
3. **Chat en Vivo:** Soporte en tiempo real
4. **Tutoriales/Guías:** Contenido educativo

---

## 📡 Endpoints

| Método | Endpoint                             | Descripción               | Auth |
| ------ | ------------------------------------ | ------------------------- | ---- |
| `GET`  | `/api/support/articles`              | Listar artículos de ayuda | ❌   |
| `GET`  | `/api/support/articles/{slug}`       | Ver artículo              | ❌   |
| `GET`  | `/api/support/articles/search`       | Buscar artículos          | ❌   |
| `GET`  | `/api/support/categories`            | Categorías de ayuda       | ❌   |
| `POST` | `/api/support/tickets`               | Crear ticket              | ✅   |
| `GET`  | `/api/support/tickets`               | Mis tickets               | ✅   |
| `GET`  | `/api/support/tickets/{id}`          | Detalle de ticket         | ✅   |
| `POST` | `/api/support/tickets/{id}/messages` | Agregar mensaje           | ✅   |
| `POST` | `/api/support/feedback`              | Enviar feedback           | ✅   |

---

## 🗃️ Entidades

### HelpArticle

```csharp
public class HelpArticle
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }

    // Contenido
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Summary { get; set; }
    public string Content { get; set; }              // Markdown
    public string ContentHtml { get; set; }          // Rendered

    // SEO
    public string MetaTitle { get; set; }
    public string MetaDescription { get; set; }
    public List<string> Keywords { get; set; }

    // Organización
    public ArticleType Type { get; set; }
    public int SortOrder { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPinned { get; set; }

    // Multimedia
    public List<string> ImageUrls { get; set; }
    public string VideoUrl { get; set; }

    // Visibilidad
    public ArticleAudience Audience { get; set; }
    public bool IsPublished { get; set; }

    // Métricas
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public decimal HelpfulPercent { get; set; }

    // Relacionados
    public List<Guid> RelatedArticleIds { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}

public enum ArticleType
{
    FAQ,
    Tutorial,
    Guide,
    Troubleshooting,
    Policy,
    Announcement
}

public enum ArticleAudience
{
    All,
    Buyers,
    Sellers,
    Dealers,
    Admin
}
```

### HelpCategory

```csharp
public class HelpCategory
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }

    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public string IconName { get; set; }
    public string Color { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; }

    public int ArticleCount { get; set; }
    public List<HelpCategory> SubCategories { get; set; }
}
```

### SupportTicket

```csharp
public class SupportTicket
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; }         // OKLA-T-2026-00001
    public Guid UserId { get; set; }

    // Clasificación
    public TicketCategory Category { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }

    // Contenido
    public string Subject { get; set; }
    public string Description { get; set; }
    public List<string> AttachmentUrls { get; set; }

    // Contexto
    public Guid? RelatedVehicleId { get; set; }
    public Guid? RelatedOrderId { get; set; }
    public Guid? RelatedDealerId { get; set; }

    // Asignación
    public Guid? AssignedTo { get; set; }
    public string AssignedToName { get; set; }
    public Guid? TeamId { get; set; }

    // Mensajes
    public List<TicketMessage> Messages { get; set; }

    // Tiempos
    public DateTime CreatedAt { get; set; }
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }

    // Satisfacción
    public int? SatisfactionRating { get; set; }     // 1-5
    public string SatisfactionComment { get; set; }
}

public enum TicketCategory
{
    AccountIssue,
    PaymentProblem,
    ListingHelp,
    TechnicalSupport,
    FraudReport,
    RefundRequest,
    VerificationHelp,
    GeneralQuestion,
    FeatureRequest,
    Bug
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
    New,
    Open,
    InProgress,
    WaitingOnCustomer,
    WaitingOnThirdParty,
    Resolved,
    Closed,
    Reopened
}
```

### TicketMessage

```csharp
public class TicketMessage
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }

    public MessageSender SenderType { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; }
    public string SenderAvatar { get; set; }

    public string Content { get; set; }
    public List<string> AttachmentUrls { get; set; }

    public bool IsInternal { get; set; }             // Solo visible para staff
    public bool IsAutomated { get; set; }            // Mensaje automático

    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public enum MessageSender
{
    Customer,
    Agent,
    System,
    Bot
}
```

---

## 📊 Proceso HELP-001: Buscar en Centro de Ayuda

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: HELP-001 - Buscar en Centro de Ayuda                          │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-ANON, USR-REG                                     │
│ Sistemas: SupportService, Elasticsearch                                │
│ Duración: Instantáneo                                                  │
│ Criticidad: BAJA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                | Sistema        | Actor    | Evidencia         | Código     |
| ---- | ------- | ------------------------------------- | -------------- | -------- | ----------------- | ---------- |
| 1    | 1.1     | Usuario accede a Centro de Ayuda      | Frontend       | USR-ANON | Page accessed     | EVD-LOG    |
| 1    | 1.2     | Ver categorías y artículos destacados | Frontend       | USR-ANON | Content viewed    | EVD-LOG    |
| 2    | 2.1     | Usuario escribe búsqueda              | Frontend       | USR-ANON | Search input      | EVD-LOG    |
| 2    | 2.2     | GET /api/support/articles/search      | Gateway        | USR-ANON | **Request**       | EVD-LOG    |
| 3    | 3.1     | Buscar en Elasticsearch               | SupportService | Sistema  | ES query          | EVD-LOG    |
| 3    | 3.2     | Ordenar por relevancia                | SupportService | Sistema  | Ranking           | EVD-LOG    |
| 4    | 4.1     | Mostrar resultados                    | Frontend       | USR-ANON | Results displayed | EVD-SCREEN |
| 5    | 5.1     | Usuario hace clic en artículo         | Frontend       | USR-ANON | Article clicked   | EVD-LOG    |
| 5    | 5.2     | Incrementar ViewCount                 | SupportService | Sistema  | View tracked      | EVD-LOG    |
| 6    | 6.1     | Mostrar contenido del artículo        | Frontend       | USR-ANON | Article displayed | EVD-SCREEN |
| 6    | 6.2     | Mostrar artículos relacionados        | Frontend       | USR-ANON | Related shown     | EVD-LOG    |
| 7    | 7.1     | Usuario vota "¿Fue útil?"             | Frontend       | USR-ANON | Feedback          | EVD-LOG    |
| 7    | 7.2     | Actualizar HelpfulCount               | SupportService | Sistema  | Metrics updated   | EVD-LOG    |

---

## 📊 Proceso HELP-002: Crear Ticket de Soporte

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: HELP-002 - Crear Ticket de Soporte                            │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG                                               │
│ Sistemas: SupportService, NotificationService                          │
│ Duración: Instantáneo                                                  │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                | Sistema             | Actor     | Evidencia          | Código     |
| ---- | ------- | ------------------------------------- | ------------------- | --------- | ------------------ | ---------- |
| 1    | 1.1     | Usuario no encuentra respuesta en FAQ | Frontend            | USR-REG   | Search failed      | EVD-LOG    |
| 1    | 1.2     | Click "Contactar Soporte"             | Frontend            | USR-REG   | CTA clicked        | EVD-LOG    |
| 2    | 2.1     | Formulario de nuevo ticket            | Frontend            | USR-REG   | Form displayed     | EVD-SCREEN |
| 2    | 2.2     | Seleccionar categoría                 | Frontend            | USR-REG   | Category selected  | EVD-LOG    |
| 2    | 2.3     | Escribir asunto                       | Frontend            | USR-REG   | Subject input      | EVD-LOG    |
| 2    | 2.4     | Describir problema                    | Frontend            | USR-REG   | Description input  | EVD-LOG    |
| 2    | 2.5     | Adjuntar archivos (opcional)          | MediaService        | USR-REG   | **Files uploaded** | EVD-FILE   |
| 3    | 3.1     | POST /api/support/tickets             | Gateway             | USR-REG   | **Request**        | EVD-AUDIT  |
| 3    | 3.2     | Validar datos                         | SupportService      | Sistema   | Validation         | EVD-LOG    |
| 4    | 4.1     | **Crear SupportTicket**               | SupportService      | Sistema   | **Ticket created** | EVD-AUDIT  |
| 4    | 4.2     | Generar TicketNumber                  | SupportService      | Sistema   | Number generated   | EVD-LOG    |
| 4    | 4.3     | Determinar prioridad automática       | SupportService      | Sistema   | Priority set       | EVD-LOG    |
| 5    | 5.1     | Auto-asignar a equipo                 | SupportService      | Sistema   | Team assigned      | EVD-LOG    |
| 5    | 5.2     | Si urgente: asignar a agente          | SupportService      | Sistema   | Agent assigned     | EVD-LOG    |
| 6    | 6.1     | **Confirmar al usuario**              | NotificationService | SYS-NOTIF | **Confirmation**   | EVD-COMM   |
| 6    | 6.2     | **Notificar a equipo de soporte**     | NotificationService | SYS-NOTIF | **Team notified**  | EVD-COMM   |
| 7    | 7.1     | Mostrar ticket creado                 | Frontend            | USR-REG   | Ticket displayed   | EVD-SCREEN |
| 8    | 8.1     | **Audit trail**                       | AuditService        | Sistema   | Complete audit     | EVD-AUDIT  |

### Evidencia de Ticket

```json
{
  "processCode": "HELP-002",
  "ticket": {
    "id": "ticket-12345",
    "ticketNumber": "OKLA-T-2026-00001",
    "user": {
      "id": "user-001",
      "name": "María Cliente",
      "email": "maria@email.com"
    },
    "classification": {
      "category": "PAYMENT_PROBLEM",
      "priority": "HIGH",
      "status": "NEW"
    },
    "content": {
      "subject": "No puedo completar el pago de mi suscripción",
      "description": "Intenté pagar con mi tarjeta de crédito pero me da error 'Transacción rechazada'. Ya verifiqué que tengo fondos.",
      "attachments": ["s3://support/ticket-12345/screenshot.png"]
    },
    "context": {
      "relatedOrderId": null,
      "relatedDealerId": "dealer-001",
      "userPlan": "PRO",
      "browser": "Chrome 120",
      "os": "Windows 11"
    },
    "assignment": {
      "team": "BILLING_SUPPORT",
      "assignedTo": null,
      "autoEscalateAt": "2026-01-21T14:30:00Z"
    },
    "sla": {
      "firstResponseDue": "2026-01-21T12:30:00Z",
      "resolutionDue": "2026-01-22T10:30:00Z"
    },
    "createdAt": "2026-01-21T10:30:00Z"
  }
}
```

---

## 📊 Proceso HELP-003: Responder Ticket (Agente)

| Paso | Subpaso | Acción                                  | Sistema             | Actor       | Evidencia           | Código    |
| ---- | ------- | --------------------------------------- | ------------------- | ----------- | ------------------- | --------- |
| 1    | 1.1     | Agente ve tickets asignados             | Dashboard           | ADM-SUPPORT | Queue viewed        | EVD-LOG   |
| 1    | 1.2     | Abre ticket                             | Dashboard           | ADM-SUPPORT | Ticket opened       | EVD-AUDIT |
| 2    | 2.1     | Revisa historial del usuario            | Dashboard           | ADM-SUPPORT | History viewed      | EVD-LOG   |
| 2    | 2.2     | Revisa tickets previos                  | Dashboard           | ADM-SUPPORT | Prior tickets       | EVD-LOG   |
| 3    | 3.1     | Escribe respuesta                       | Dashboard           | ADM-SUPPORT | Response input      | EVD-LOG   |
| 3    | 3.2     | Adjunta archivos si necesario           | MediaService        | ADM-SUPPORT | Files attached      | EVD-FILE  |
| 4    | 4.1     | POST /api/support/tickets/{id}/messages | Gateway             | ADM-SUPPORT | **Request**         | EVD-AUDIT |
| 4    | 4.2     | **Crear TicketMessage**                 | SupportService      | Sistema     | **Message created** | EVD-AUDIT |
| 4    | 4.3     | Actualizar Status                       | SupportService      | Sistema     | Status updated      | EVD-LOG   |
| 4    | 4.4     | Registrar FirstResponseAt               | SupportService      | Sistema     | SLA tracked         | EVD-LOG   |
| 5    | 5.1     | **Notificar al cliente**                | NotificationService | SYS-NOTIF   | **Response sent**   | EVD-COMM  |
| 6    | 6.1     | Si resuelto: pedir rating               | SupportService      | Sistema     | Rating request      | EVD-LOG   |

---

## 📋 Estructura de Categorías FAQ

```
Centro de Ayuda OKLA
├── 🚗 Comprar un Vehículo
│   ├── Cómo buscar vehículos
│   ├── Filtros de búsqueda
│   ├── Contactar al vendedor
│   ├── Agendar test drive
│   └── Proceso de compra
├── 💰 Vender tu Vehículo
│   ├── Cómo publicar tu vehículo
│   ├── Consejos para mejores fotos
│   ├── Fijar el precio correcto
│   ├── Responder a compradores
│   └── Completar una venta
├── 🏢 Para Dealers
│   ├── Registro de dealer
│   ├── Planes y precios
│   ├── Gestión de inventario
│   ├── Importar vehículos CSV
│   └── Dashboard de analytics
├── 💳 Pagos y Facturación
│   ├── Métodos de pago aceptados
│   ├── Problemas con pagos
│   ├── Facturación y NCF
│   ├── Reembolsos
│   └── Suscripciones
├── 🔒 Cuenta y Seguridad
│   ├── Crear cuenta
│   ├── Verificar identidad
│   ├── Cambiar contraseña
│   ├── Two-factor authentication
│   └── Eliminar cuenta
├── 🛡️ Confianza y Seguridad
│   ├── Consejos para evitar fraudes
│   ├── Reportar un problema
│   ├── Garantía OKLA
│   ├── Inspección de vehículos
│   └── Vendedores verificados
└── ⚙️ Problemas Técnicos
    ├── La app no carga
    ├── Error al subir fotos
    ├── Problemas de login
    └── Contactar soporte técnico
```

---

## 📊 Métricas Prometheus

```yaml
# Centro de Ayuda
support_articles_views_total{category, article}
support_articles_helpful_rate{category}
support_search_queries_total
support_search_no_results_total

# Tickets
support_tickets_created_total{category, priority}
support_tickets_open_count{status}
support_tickets_resolved_total
support_ticket_first_response_time_minutes
support_ticket_resolution_time_minutes

# Satisfacción
support_satisfaction_rating_average
support_satisfaction_responses_total{rating}

# Agentes
support_agent_tickets_handled_total{agent}
support_agent_avg_response_time_minutes{agent}
```

---

## 🔗 Referencias

- [07-NOTIFICACIONES/01-notification-service.md](../07-NOTIFICACIONES/01-notification-service.md)
- [06-CRM-LEADS-CONTACTOS/04-chatbot-service.md](../06-CRM-LEADS-CONTACTOS/04-chatbot-service.md)
