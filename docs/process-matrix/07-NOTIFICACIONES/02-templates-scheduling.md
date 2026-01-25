# 📋 Templates y Programación de Notificaciones - Matriz de Procesos

> **Servicio:** NotificationService (TemplatesController, ScheduledNotificationsController)  
> **Puerto:** 5010  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente  | Total | Implementado | Pendiente | Estado |
| ----------- | ----- | ------------ | --------- | ------ |
| Controllers | 2     | 2            | 0         | 🟢     |
| TPL-\*      | 6     | 6            | 0         | 🟢     |
| SCHED-\*    | 5     | 5            | 0         | 🟢     |
| TPL-VAR-\*  | 4     | 4            | 0         | 🟢     |
| Tests       | 10    | 10           | 0         | ✅     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 1. Información General

### 1.1 Descripción

Sistema de gestión de templates de notificaciones y programación de envíos. Permite crear, editar y versionar templates, además de programar notificaciones para envío futuro o recurrente.

### 1.2 Componentes

- **TemplatesController**: CRUD de templates de notificación
- **ScheduledNotificationsController**: Programación de envíos

### 1.3 Dependencias

| Servicio            | Propósito                     |
| ------------------- | ----------------------------- |
| SchedulerService    | Ejecución de jobs programados |
| NotificationService | Envío de notificaciones       |
| MediaService        | Imágenes en templates         |

### 1.4 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                Templates & Scheduling Architecture                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   Clients                            NotificationService                     │
│   ┌────────────────┐              ┌─────────────────────────────────────┐    │
│   │ Admin Panel    │──┐           │     Templates & Scheduling Module   │    │
│   │ (Create Templ) │  │           │  ┌───────────────────────────────┐  │    │
│   └────────────────┘  │           │  │ TemplatesController           │  │    │
│   ┌────────────────┐  │           │  │ • CRUD templates              │  │    │
│   │ Marketing Team │──┼──────────▶│  │ • Preview with data           │  │    │
│   │ (Schedule)     │  │           │  │ • Validate Handlebars         │  │    │
│   └────────────────┘  │           │  └───────────────────────────────┘  │    │
│   ┌────────────────┐  │           │  ┌───────────────────────────────┐  │    │
│   │ Other Services │──┘           │  │ ScheduledNotificationsCtrl    │  │    │
│   │ (API calls)    │              │  │ • Create scheduled send       │  │    │
│   └────────────────┘              │  │ • Recurring (cron patterns)   │  │    │
│                                   │  │ • Cancel/reschedule           │  │    │
│   Scheduler                       │  └───────────────────────────────┘  │    │
│   ┌────────────────┐              │  ┌───────────────────────────────┐  │    │
│   │ Hangfire       │─────────────▶│  │ Template Engine (Handlebars)  │  │    │
│   │ (Cron Jobs)    │              │  │ • Variables: {{user.name}}   │  │    │
│   └────────────────┘              │  │ • Conditionals: {{#if}}      │  │    │
│                                   │  │ • Loops: {{#each items}}     │  │    │
│                                   │  └───────────────────────────────┘  │    │
│                                   └─────────────────────────────────────┘    │
│                                                    │                        │
│                                    ┌───────────────┼───────────────┐        │
│                                    ▼               ▼               ▼        │
│                            ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│                            │ PostgreSQL │  │   Redis    │  │  RabbitMQ  │  │
│                            │ (Templates,│  │ (Rendered  │  │ (Schedule  │  │
│                            │  Schedules)│  │  Cache)    │  │  Events)   │  │
│                            └────────────┘  └────────────┘  └────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints API

### 2.1 TemplatesController

| Método   | Endpoint                        | Descripción         | Auth | Roles |
| -------- | ------------------------------- | ------------------- | ---- | ----- |
| `POST`   | `/api/templates`                | Crear template      | ✅   | Admin |
| `GET`    | `/api/templates/{id}`           | Obtener por ID      | ✅   | User  |
| `GET`    | `/api/templates/by-name/{name}` | Obtener por nombre  | ✅   | User  |
| `GET`    | `/api/templates`                | Listar con filtros  | ✅   | User  |
| `PUT`    | `/api/templates/{id}`           | Actualizar template | ✅   | Admin |
| `DELETE` | `/api/templates/{id}`           | Eliminar template   | ✅   | Admin |
| `POST`   | `/api/templates/{id}/duplicate` | Duplicar template   | ✅   | Admin |
| `POST`   | `/api/templates/{id}/preview`   | Preview con datos   | ✅   | User  |
| `POST`   | `/api/templates/{id}/validate`  | Validar sintaxis    | ✅   | Admin |
| `GET`    | `/api/templates/categories`     | Listar categorías   | ✅   | User  |
| `GET`    | `/api/templates/tags`           | Listar tags         | ✅   | User  |

### 2.2 ScheduledNotificationsController

| Método   | Endpoint                                       | Descripción            | Auth | Roles |
| -------- | ---------------------------------------------- | ---------------------- | ---- | ----- |
| `POST`   | `/api/notifications/scheduled`                 | Programar notificación | ✅   | Admin |
| `GET`    | `/api/notifications/scheduled/{id}`            | Obtener programada     | ✅   | Admin |
| `GET`    | `/api/notifications/scheduled`                 | Listar programadas     | ✅   | Admin |
| `PUT`    | `/api/notifications/scheduled/{id}/reschedule` | Reprogramar            | ✅   | Admin |
| `DELETE` | `/api/notifications/scheduled/{id}`            | Cancelar               | ✅   | Admin |
| `POST`   | `/api/notifications/scheduled/{id}/pause`      | Pausar recurrente      | ✅   | Admin |
| `POST`   | `/api/notifications/scheduled/{id}/resume`     | Reanudar               | ✅   | Admin |

---

## 3. Entidades y Enums

### 3.1 NotificationType (Enum)

```csharp
public enum NotificationType
{
    Email = 0,           // Correo electrónico
    Push = 1,            // Push notification
    SMS = 2,             // Mensaje de texto
    InApp = 3,           // Notificación in-app
    WhatsApp = 4         // WhatsApp Business
}
```

### 3.2 ScheduleStatus (Enum)

```csharp
public enum ScheduleStatus
{
    Pending = 0,         // Esperando ejecución
    Completed = 1,       // Ejecutado exitosamente
    Failed = 2,          // Falló en ejecución
    Cancelled = 3,       // Cancelado por usuario
    Paused = 4           // Pausado (recurrente)
}
```

### 3.3 RecurrenceType (Enum)

```csharp
public enum RecurrenceType
{
    None = 0,            // Una sola vez
    Daily = 1,           // Diario
    Weekly = 2,          // Semanal
    Monthly = 3,         // Mensual
    Quarterly = 4,       // Trimestral
    Yearly = 5,          // Anual
    Custom = 6           // Cron expression
}
```

### 3.4 NotificationTemplate (Entidad)

```csharp
public class NotificationTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; }                 // Único
    public string Subject { get; set; }              // Asunto (email)
    public string Body { get; set; }                 // Contenido con placeholders
    public NotificationType Type { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; }             // auth, payment, marketing
    public string Tags { get; set; }                 // Comma-separated
    public bool IsActive { get; set; }

    // Variables del template
    public Dictionary<string, string> Variables { get; set; }
    public string? PreviewData { get; set; }         // JSON para preview

    // Versionamiento
    public int Version { get; set; }
    public Guid? PreviousVersionId { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
```

### 3.5 ScheduledNotification (Entidad)

```csharp
public class ScheduledNotification
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public DateTime ScheduledFor { get; set; }
    public string TimeZone { get; set; }             // America/Santo_Domingo
    public ScheduleStatus Status { get; set; }

    // Recurrencia
    public bool IsRecurring { get; set; }
    public RecurrenceType? RecurrenceType { get; set; }
    public string? CronExpression { get; set; }      // Para Custom
    public DateTime? NextExecution { get; set; }
    public DateTime? LastExecution { get; set; }
    public int ExecutionCount { get; set; }
    public int? MaxExecutions { get; set; }          // null = infinito

    // Error handling
    public int FailureCount { get; set; }
    public string? LastError { get; set; }
    public int MaxRetries { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? CancelledBy { get; set; }
    public string? CancellationReason { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 TEMPL-001: Crear Template de Notificación

| Campo       | Valor               |
| ----------- | ------------------- |
| **ID**      | TEMPL-001           |
| **Nombre**  | Crear Template      |
| **Actor**   | Admin               |
| **Trigger** | POST /api/templates |

#### Flujo del Proceso

| Paso | Acción                      | Sistema             | Validación            |
| ---- | --------------------------- | ------------------- | --------------------- |
| 1    | Admin accede a editor       | Dashboard           | Rol Admin             |
| 2    | Ingresar datos del template | Frontend            | Nombre, Subject, Body |
| 3    | Definir variables           | Frontend            | key-value pairs       |
| 4    | Validar nombre único        | NotificationService | No existe             |
| 5    | Validar sintaxis template   | TemplateEngine      | Handlebars válido     |
| 6    | Guardar template            | Database            | Version = 1           |
| 7    | Publicar evento             | RabbitMQ            | template.created      |

#### Request

```json
{
  "name": "welcome_email",
  "subject": "¡Bienvenido a OKLA, {{userName}}!",
  "body": "<h1>Hola {{userName}}</h1><p>Gracias por unirte...</p>",
  "type": "Email",
  "description": "Email de bienvenida para nuevos usuarios",
  "category": "auth",
  "tags": "welcome,onboarding",
  "variables": {
    "userName": "Nombre del usuario",
    "email": "Email del usuario",
    "verificationLink": "Link de verificación"
  },
  "previewData": "{\"userName\": \"Juan\", \"email\": \"juan@email.com\"}"
}
```

#### Response

```json
{
  "id": "uuid",
  "name": "welcome_email",
  "type": "Email",
  "version": 1,
  "isActive": true,
  "createdAt": "2026-01-21T10:00:00Z",
  "createdBy": "admin@okla.com.do"
}
```

---

### 4.2 TEMPL-002: Preview de Template

| Campo       | Valor                            |
| ----------- | -------------------------------- |
| **ID**      | TEMPL-002                        |
| **Nombre**  | Preview Template con Datos       |
| **Actor**   | Admin                            |
| **Trigger** | POST /api/templates/{id}/preview |

#### Flujo del Proceso

| Paso | Acción                  | Sistema        | Validación  |
| ---- | ----------------------- | -------------- | ----------- |
| 1    | Obtener template        | Database       | Existe      |
| 2    | Recibir datos de prueba | Request        | JSON válido |
| 3    | Renderizar template     | TemplateEngine | Handlebars  |
| 4    | Retornar HTML/texto     | Response       | Preview     |

#### Request

```json
{
  "data": {
    "userName": "Juan Pérez",
    "email": "juan@email.com",
    "verificationLink": "https://okla.com.do/verify/abc123"
  }
}
```

#### Response

```json
{
  "subject": "¡Bienvenido a OKLA, Juan Pérez!",
  "body": "<h1>Hola Juan Pérez</h1><p>Gracias por unirte...</p>",
  "plainText": "Hola Juan Pérez\n\nGracias por unirte..."
}
```

---

### 4.3 SCHED-001: Programar Notificación

| Campo       | Valor                             |
| ----------- | --------------------------------- |
| **ID**      | SCHED-001                         |
| **Nombre**  | Programar Envío de Notificación   |
| **Actor**   | Admin/Sistema                     |
| **Trigger** | POST /api/notifications/scheduled |

#### Flujo del Proceso

| Paso | Acción                      | Sistema             | Validación            |
| ---- | --------------------------- | ------------------- | --------------------- |
| 1    | Crear notificación base     | NotificationService | Template + recipients |
| 2    | Definir fecha/hora de envío | Frontend            | Futuro                |
| 3    | Seleccionar timezone        | Frontend            | IANA timezone         |
| 4    | Configurar recurrencia      | Frontend            | Opcional              |
| 5    | Validar Cron expression     | NotificationService | Si custom             |
| 6    | Calcular NextExecution      | NotificationService | Según timezone        |
| 7    | Crear scheduled             | Database            | Status = Pending      |
| 8    | Registrar en Scheduler      | SchedulerService    | Job programado        |

#### Request (Una vez)

```json
{
  "notificationId": "uuid",
  "scheduledFor": "2026-01-25T09:00:00Z",
  "timeZone": "America/Santo_Domingo"
}
```

#### Request (Recurrente)

```json
{
  "notificationId": "uuid",
  "scheduledFor": "2026-01-25T09:00:00Z",
  "timeZone": "America/Santo_Domingo",
  "isRecurring": true,
  "recurrenceType": "Weekly",
  "maxExecutions": 12
}
```

#### Request (Cron Expression)

```json
{
  "notificationId": "uuid",
  "scheduledFor": "2026-01-25T09:00:00Z",
  "timeZone": "America/Santo_Domingo",
  "cronExpression": "0 9 * * 1",
  "maxExecutions": null
}
```

---

### 4.4 SCHED-002: Ejecutar Notificación Programada

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **ID**      | SCHED-002                  |
| **Nombre**  | Ejecutar Scheduled Job     |
| **Actor**   | Sistema (SchedulerService) |
| **Trigger** | Cron trigger               |

#### Flujo del Proceso

| Paso | Acción                         | Sistema             | Validación                |
| ---- | ------------------------------ | ------------------- | ------------------------- |
| 1    | Job trigger ejecuta            | SchedulerService    | Según NextExecution       |
| 2    | Obtener scheduled notification | Database            | Status = Pending          |
| 3    | Obtener notification base      | Database            | Con template y recipients |
| 4    | Renderizar template            | TemplateEngine      | Con datos actuales        |
| 5    | Enviar notificaciones          | NotificationService | Por canal                 |
| 6    | Si éxito                       | Check               | Todos enviados            |
| 7    | Actualizar status              | Database            | Completed                 |
| 8    | Actualizar ExecutionCount      | Database            | +1                        |
| 9    | Si recurrente                  | Check               | Calcular next             |
| 10   | Si alcanzó max                 | Check               | Status = Completed        |
| 11   | Si error                       | Handle              | Retry o Failed            |

---

### 4.5 SCHED-003: Reprogramar Notificación

| Campo       | Valor                                            |
| ----------- | ------------------------------------------------ |
| **ID**      | SCHED-003                                        |
| **Nombre**  | Reprogramar Envío                                |
| **Actor**   | Admin                                            |
| **Trigger** | PUT /api/notifications/scheduled/{id}/reschedule |

#### Flujo del Proceso

| Paso | Acción                   | Sistema             | Validación       |
| ---- | ------------------------ | ------------------- | ---------------- |
| 1    | Obtener scheduled        | Database            | Status = Pending |
| 2    | Validar nueva fecha      | NotificationService | En el futuro     |
| 3    | Cancelar job actual      | SchedulerService    | Desregistrar     |
| 4    | Actualizar ScheduledFor  | Database            | Nueva fecha      |
| 5    | Recalcular NextExecution | NotificationService | Si recurrente    |
| 6    | Registrar nuevo job      | SchedulerService    | Reprogramar      |

---

## 5. Sintaxis de Templates

### 5.1 Variables (Handlebars)

```handlebars
{{userName}}
<!-- Variable simple -->
{{user.firstName}}
<!-- Propiedad anidada -->
{{formatCurrency price}}
<!-- Helper con formato -->
{{formatDate date "DD/MM/YYYY"}}
<!-- Helper con parámetro -->
```

### 5.2 Condicionales

```handlebars
{{#if isPremium}}
  <p>Gracias por ser miembro premium!</p>
{{else}}
  <p>Considera actualizar a premium</p>
{{/if}}

{{#unless hasVerified}}
  <p>Por favor verifica tu email</p>
{{/unless}}
```

### 5.3 Loops

```handlebars
<ul>
  {{#each vehicles}}
    <li>{{this.title}} - {{formatCurrency this.price}}</li>
  {{/each}}
</ul>
```

### 5.4 Helpers Disponibles

| Helper           | Uso            | Ejemplo                                      |
| ---------------- | -------------- | -------------------------------------------- |
| `formatCurrency` | Formato moneda | `{{formatCurrency 1500000}}` → RD$ 1,500,000 |
| `formatDate`     | Formato fecha  | `{{formatDate date 'DD/MM/YYYY'}}`           |
| `formatNumber`   | Formato número | `{{formatNumber 15000}}` → 15,000            |
| `uppercase`      | Mayúsculas     | `{{uppercase name}}`                         |
| `lowercase`      | Minúsculas     | `{{lowercase email}}`                        |
| `truncate`       | Truncar texto  | `{{truncate description 100}}`               |

---

## 6. Templates Predefinidos

### 6.1 Categorías

| Categoría   | Templates                                             |
| ----------- | ----------------------------------------------------- |
| `auth`      | welcome, verify_email, password_reset, 2fa_code       |
| `payment`   | payment_success, payment_failed, subscription_renewed |
| `vehicle`   | listing_approved, listing_expired, price_changed      |
| `lead`      | new_lead, lead_response, missed_lead                  |
| `dealer`    | kyc_approved, kyc_rejected, plan_upgraded             |
| `marketing` | promo_weekly, newsletter, abandoned_search            |

### 6.2 Template de Ejemplo: payment_success

```json
{
  "name": "payment_success",
  "subject": "✅ Pago confirmado - OKLA",
  "body": "
    <h1>¡Pago exitoso!</h1>
    <p>Hola {{userName}},</p>
    <p>Hemos recibido tu pago correctamente.</p>

    <table>
      <tr><td>Monto:</td><td>{{formatCurrency amount}}</td></tr>
      <tr><td>Referencia:</td><td>{{transactionId}}</td></tr>
      <tr><td>Fecha:</td><td>{{formatDate date 'DD/MM/YYYY HH:mm'}}</td></tr>
      <tr><td>Método:</td><td>{{paymentMethod}}</td></tr>
    </table>

    {{#if isSubscription}}
    <p>Tu suscripción {{planName}} está activa hasta {{formatDate nextBillingDate}}.</p>
    {{/if}}

    <p>Gracias por confiar en OKLA.</p>
  ",
  "type": "Email",
  "category": "payment",
  "variables": {
    "userName": "Nombre del usuario",
    "amount": "Monto pagado",
    "transactionId": "ID de transacción",
    "date": "Fecha del pago",
    "paymentMethod": "Método de pago",
    "isSubscription": "Boolean si es suscripción",
    "planName": "Nombre del plan",
    "nextBillingDate": "Próximo cobro"
  }
}
```

---

## 7. Reglas de Negocio

### 7.1 Templates

| Regla              | Valor           |
| ------------------ | --------------- |
| Nombre único       | Global          |
| Versiones máximas  | 10 por template |
| Tamaño máximo body | 500 KB          |
| Variables máximas  | 50              |
| Tags máximos       | 10              |

### 7.2 Programación

| Regla                         | Valor     |
| ----------------------------- | --------- |
| Anticipación mínima           | 5 minutos |
| Máximo futuro                 | 1 año     |
| Recurrencias por usuario      | 50        |
| Ejecuciones máximas (default) | 100       |
| Reintentos máximos            | 3         |

### 7.3 Cron Expressions Válidas

| Pattern       | Descripción       |
| ------------- | ----------------- |
| `0 9 * * *`   | Diario a las 9 AM |
| `0 9 * * 1`   | Lunes a las 9 AM  |
| `0 9 1 * *`   | Día 1 de cada mes |
| `0 9 1 */3 *` | Trimestral        |
| `0 9 1 1 *`   | Anual (1 enero)   |

---

## 8. Manejo de Errores

| Código | Error            | Mensaje                                | Acción             |
| ------ | ---------------- | -------------------------------------- | ------------------ |
| 400    | TemplateExists   | "Template name already exists"         | Usar otro nombre   |
| 400    | InvalidTemplate  | "Template syntax is invalid"           | Revisar Handlebars |
| 400    | InvalidCron      | "Invalid cron expression"              | Verificar formato  |
| 400    | PastDate         | "Scheduled date must be in the future" | Usar fecha futura  |
| 404    | TemplateNotFound | "Template not found"                   | Verificar ID       |
| 404    | ScheduleNotFound | "Scheduled notification not found"     | Verificar ID       |

---

## 9. Eventos RabbitMQ

| Evento               | Exchange              | Descripción            |
| -------------------- | --------------------- | ---------------------- |
| `template.created`   | `notification.events` | Template creado        |
| `template.updated`   | `notification.events` | Template actualizado   |
| `template.deleted`   | `notification.events` | Template eliminado     |
| `schedule.created`   | `notification.events` | Programación creada    |
| `schedule.executed`  | `notification.events` | Programación ejecutada |
| `schedule.failed`    | `notification.events` | Ejecución fallida      |
| `schedule.cancelled` | `notification.events` | Programación cancelada |

---

## 10. Métricas

### 10.1 Prometheus

```
# Templates
notification_templates_total{category="auth|payment|marketing"}
notification_template_renders_total{template="welcome_email"}

# Scheduled
notification_scheduled_total{status="pending|completed|failed"}
notification_scheduled_executed_total
notification_scheduled_failed_total{reason="..."}

# Timing
notification_schedule_execution_delay_seconds
```

---

## 11. Configuración

```json
{
  "Templates": {
    "MaxVersions": 10,
    "MaxBodySize": 512000,
    "MaxVariables": 50,
    "CacheMinutes": 60
  },
  "Scheduling": {
    "MinAdvanceMinutes": 5,
    "MaxAdvanceDays": 365,
    "MaxRecurrencesPerUser": 50,
    "DefaultMaxExecutions": 100,
    "MaxRetries": 3,
    "RetryDelayMinutes": [5, 15, 60]
  }
}
```

---

## 📚 Referencias

- [01-notification-service.md](01-notification-service.md) - Sistema principal
- [Handlebars.js](https://handlebarsjs.com/) - Sintaxis de templates
- [NCrontab](https://github.com/atifaziz/NCrontab) - Cron expressions
