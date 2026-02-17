# 📢 Marketing Service - Matriz de Procesos

> **Servicio:** MarketingService  
> **Puerto:** 5045  
> **Última actualización:** Enero 25, 2026  
> **Estado:** 🟡 PLANIFICADO  
> **Estado de Implementación:** 🔴 0% Backend | 🔴 0% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                | Backend      | UI Access | Observación              |
| ---------------------- | ------------ | --------- | ------------------------ |
| MKT-CAMP-001 Campañas  | 🔴 Pendiente | 🔴 Falta  | Servicio no implementado |
| MKT-AUD-001 Audiencias | 🔴 Pendiente | 🔴 Falta  | Servicio no implementado |
| MKT-ANAL-001 Analytics | 🔴 Pendiente | 🔴 Falta  | Servicio no implementado |

### Rutas UI Existentes ✅

- Ninguna - Servicio no implementado

### Rutas UI Faltantes 🔴

- `/dealer/marketing` → Dashboard de campañas
- `/dealer/marketing/campaigns/new` → Crear campaña
- `/dealer/marketing/audiences` → Gestión de audiencias
- `/admin/marketing` → Campañas a nivel plataforma

**Verificación Backend:** MarketingService **NO** existe en `/backend/` ⚠️

> ⚠️ **NOTA:** Este servicio está planificado para Q2 2026.

---

## 📊 Resumen de Implementación

| Componente  | Total | Implementado | Pendiente | Estado |
| ----------- | ----- | ------------ | --------- | ------ |
| Controllers | 3     | 0            | 3         | 🔴     |
| MKT-CAMP-\* | 6     | 0            | 6         | 🔴     |
| MKT-AUD-\*  | 4     | 0            | 4         | 🔴     |
| MKT-TPL-\*  | 5     | 0            | 5         | 🔴     |
| MKT-ANAL-\* | 4     | 0            | 4         | 🔴     |
| Tests       | 0     | 0            | 12        | 🔴     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 1. Información General

### 1.1 Descripción

Sistema de gestión de campañas de marketing para OKLA. Permite a dealers y administradores crear campañas de email marketing, gestionar audiencias segmentadas y analizar resultados.

### 1.2 Dependencias

| Servicio             | Propósito                |
| -------------------- | ------------------------ |
| NotificationService  | Envío de emails          |
| UserService          | Datos de usuarios        |
| EventTrackingService | Tracking de opens/clicks |
| BillingService       | Cobro por campañas       |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      MarketingService Architecture                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   Campaign Managers                  Core Service                            │
│   ┌────────────────┐                ┌────────────────────────────────┐      │
│   │ Dealers        │──┐             │        MarketingService          │      │
│   │ (Campaigns)    │  │             │  ┌──────────────────────────┐   │      │
│   └────────────────┘  │             │  │ Controllers              │   │      │
│   ┌────────────────┐  │             │  │ • CampaignsController    │   │      │
│   │ Admin          │──┼────────────▶│  │ • AudiencesController    │   │      │
│   │ (Platform-wide)│  │             │  │ • TemplatesController    │   │      │
│   └────────────────┘  │             │  └──────────────────────────┘   │      │
│   ┌────────────────┐  │             │  ┌──────────────────────────┐   │      │
│   │ Analytics View │──┘             │  │ Application (CQRS)       │   │      │
│   │ (Reports)      │               │  │ • CreateCampaignCmd      │   │      │
│   └────────────────┘               │  │ • ScheduleCampaignCmd    │   │      │
│                                    │  │ • BuildAudienceQuery     │   │      │
│   Integrations                     │  │ • GetCampaignStatsQuery  │   │      │
│   ┌────────────────┐               │  └──────────────────────────┘   │      │
│   │ Notification   │◀─────────────│  ┌──────────────────────────┐   │      │
│   │ Service        │               │  │ Domain                   │   │      │
│   └────────────────┘               │  │ • Campaign, Audience     │   │      │
│   ┌────────────────┐               │  │ • EmailTemplate          │   │      │
│   │ EventTracking  │◀─────────────│  │ • CampaignStats          │   │      │
│   │ (Opens/Clicks) │               │  └──────────────────────────┘   │      │
│   └────────────────┘               └────────────────────────────────┘      │
│                                                    │                        │
│                                    ┌───────────────┼───────────────┐        │
│                                    ▼               ▼               ▼        │
│                            ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│                            │ PostgreSQL │  │   Redis    │  │  RabbitMQ  │  │
│                            │ (Campaigns,│  │  (Queue,   │  │ (Campaign  │  │
│                            │  Audiences)│  │  Stats)    │  │  Events)   │  │
│                            └────────────┘  └────────────┘  └────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.4 Componentes

- **CampaignsController**: Gestión de campañas
- **AudiencesController**: Segmentos de usuarios
- **EmailTemplatesController**: Templates de marketing

---

## 2. Endpoints API

### 2.1 CampaignsController

| Método   | Endpoint                         | Descripción        | Auth | Roles                 |
| -------- | -------------------------------- | ------------------ | ---- | --------------------- |
| `GET`    | `/api/campaigns`                 | Listar campañas    | ✅   | Dealer, Admin         |
| `GET`    | `/api/campaigns/{id}`            | Obtener campaña    | ✅   | Dealer (owner), Admin |
| `GET`    | `/api/campaigns/status/{status}` | Filtrar por status | ✅   | Dealer, Admin         |
| `POST`   | `/api/campaigns`                 | Crear campaña      | ✅   | Dealer, Admin         |
| `PUT`    | `/api/campaigns/{id}`            | Actualizar campaña | ✅   | Dealer (owner), Admin |
| `POST`   | `/api/campaigns/{id}/schedule`   | Programar envío    | ✅   | Dealer (owner), Admin |
| `POST`   | `/api/campaigns/{id}/start`      | Iniciar campaña    | ✅   | Dealer (owner), Admin |
| `POST`   | `/api/campaigns/{id}/pause`      | Pausar campaña     | ✅   | Dealer (owner), Admin |
| `POST`   | `/api/campaigns/{id}/resume`     | Reanudar campaña   | ✅   | Dealer (owner), Admin |
| `POST`   | `/api/campaigns/{id}/complete`   | Completar campaña  | ✅   | Dealer (owner), Admin |
| `POST`   | `/api/campaigns/{id}/cancel`     | Cancelar campaña   | ✅   | Dealer (owner), Admin |
| `DELETE` | `/api/campaigns/{id}`            | Eliminar campaña   | ✅   | Dealer (owner), Admin |
| `GET`    | `/api/campaigns/{id}/stats`      | Estadísticas       | ✅   | Dealer (owner), Admin |

### 2.2 AudiencesController

| Método   | Endpoint                      | Descripción          | Auth | Roles          |
| -------- | ----------------------------- | -------------------- | ---- | -------------- |
| `GET`    | `/api/audiences`              | Listar audiencias    | ✅   | Dealer, Admin  |
| `GET`    | `/api/audiences/{id}`         | Obtener audiencia    | ✅   | Dealer (owner) |
| `POST`   | `/api/audiences`              | Crear audiencia      | ✅   | Dealer         |
| `PUT`    | `/api/audiences/{id}`         | Actualizar audiencia | ✅   | Dealer (owner) |
| `DELETE` | `/api/audiences/{id}`         | Eliminar audiencia   | ✅   | Dealer (owner) |
| `GET`    | `/api/audiences/{id}/members` | Listar miembros      | ✅   | Dealer (owner) |
| `POST`   | `/api/audiences/{id}/refresh` | Refrescar segmento   | ✅   | Dealer (owner) |

### 2.3 EmailTemplatesController

| Método   | Endpoint                           | Descripción      | Auth | Roles          |
| -------- | ---------------------------------- | ---------------- | ---- | -------------- |
| `GET`    | `/api/emailtemplates`              | Listar templates | ✅   | Dealer         |
| `GET`    | `/api/emailtemplates/{id}`         | Obtener template | ✅   | Dealer (owner) |
| `POST`   | `/api/emailtemplates`              | Crear template   | ✅   | Dealer         |
| `PUT`    | `/api/emailtemplates/{id}`         | Actualizar       | ✅   | Dealer (owner) |
| `DELETE` | `/api/emailtemplates/{id}`         | Eliminar         | ✅   | Dealer (owner) |
| `POST`   | `/api/emailtemplates/{id}/preview` | Preview          | ✅   | Dealer (owner) |

---

## 3. Entidades y Enums

### 3.1 CampaignStatus (Enum)

```csharp
public enum CampaignStatus
{
    Draft = 0,           // Borrador
    Scheduled = 1,       // Programada para envío
    Running = 2,         // En ejecución
    Paused = 3,          // Pausada
    Completed = 4,       // Completada
    Cancelled = 5        // Cancelada
}
```

### 3.2 CampaignType (Enum)

```csharp
public enum CampaignType
{
    Newsletter = 0,      // Newsletter general
    Promotional = 1,     // Promoción/Descuento
    Announcement = 2,    // Anuncio
    Drip = 3,            // Secuencia automatizada
    Transactional = 4,   // Transaccional
    Remarketing = 5      // Remarketing
}
```

### 3.3 Campaign (Entidad)

```csharp
public class Campaign
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public string Name { get; set; }
    public CampaignType Type { get; set; }
    public CampaignStatus Status { get; set; }
    public string? Description { get; set; }

    // Configuración
    public Guid? AudienceId { get; set; }
    public Guid? TemplateId { get; set; }
    public decimal Budget { get; set; }

    // Programación
    public DateTime? ScheduledDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Métricas
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int DeliveredCount { get; set; }
    public int OpenedCount { get; set; }
    public int ClickedCount { get; set; }
    public int UnsubscribedCount { get; set; }
    public int BouncedCount { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### 3.4 Audience (Entidad)

```csharp
public class Audience
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public AudienceType Type { get; set; }

    // Criterios de segmentación (JSON)
    public string SegmentCriteria { get; set; }

    // Métricas
    public int MemberCount { get; set; }
    public DateTime? LastRefreshedAt { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### 3.5 SegmentCriteria (JSON Schema)

```json
{
  "rules": [
    {
      "field": "lastActivityDate",
      "operator": "greaterThan",
      "value": "2026-01-01"
    },
    {
      "field": "totalPurchases",
      "operator": "equals",
      "value": 0
    },
    {
      "field": "favoriteCategories",
      "operator": "contains",
      "value": ["SUV", "Sedan"]
    }
  ],
  "combination": "AND"
}
```

---

## 4. Procesos Detallados

### 4.1 MKT-CAMP-001: Crear Campaña

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **ID**      | MKT-CAMP-001               |
| **Nombre**  | Crear Campaña de Marketing |
| **Actor**   | Dealer/Admin               |
| **Trigger** | POST /api/campaigns        |

#### Flujo del Proceso

| Paso | Acción                    | Sistema          | Validación         |
| ---- | ------------------------- | ---------------- | ------------------ |
| 1    | Dealer accede a Marketing | Dashboard        | Plan con Marketing |
| 2    | Click "Nueva Campaña"     | Frontend         | Formulario         |
| 3    | Ingresar nombre y tipo    | Frontend         | Obligatorios       |
| 4    | Seleccionar audiencia     | Frontend         | Opcional (o todos) |
| 5    | Seleccionar template      | Frontend         | Opcional (o crear) |
| 6    | Definir presupuesto       | Frontend         | Si aplica          |
| 7    | Crear campaña             | MarketingService | Status = Draft     |
| 8    | Publicar evento           | RabbitMQ         | campaign.created   |

#### Request

```json
{
  "name": "Promoción Año Nuevo 2026",
  "type": "Promotional",
  "description": "Descuentos especiales en inventario",
  "audienceId": "uuid",
  "templateId": "uuid",
  "budget": 5000.0
}
```

---

### 4.2 MKT-CAMP-002: Programar Campaña

| Campo       | Valor                             |
| ----------- | --------------------------------- |
| **ID**      | MKT-CAMP-002                      |
| **Nombre**  | Programar Envío de Campaña        |
| **Actor**   | Dealer/Admin                      |
| **Trigger** | POST /api/campaigns/{id}/schedule |

#### Flujo del Proceso

| Paso | Acción                  | Sistema          | Validación         |
| ---- | ----------------------- | ---------------- | ------------------ |
| 1    | Obtener campaña         | Database         | Status = Draft     |
| 2    | Validar tiene audiencia | MarketingService | AudienceId != null |
| 3    | Validar tiene template  | MarketingService | TemplateId != null |
| 4    | Validar fecha futuro    | MarketingService | > Now              |
| 5    | Contar recipientes      | AudienceService  | MemberCount        |
| 6    | Verificar presupuesto   | BillingService   | Si aplica          |
| 7    | Actualizar status       | Database         | Scheduled          |
| 8    | Crear scheduled job     | SchedulerService | Para ScheduledDate |
| 9    | Publicar evento         | RabbitMQ         | campaign.scheduled |

#### Request

```json
{
  "scheduledDate": "2026-01-25T10:00:00Z"
}
```

---

### 4.3 MKT-CAMP-003: Ejecutar Campaña

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **ID**      | MKT-CAMP-003               |
| **Nombre**  | Ejecutar Envío de Campaña  |
| **Actor**   | Sistema (SchedulerService) |
| **Trigger** | Cron trigger o POST /start |

#### Flujo del Proceso

| Paso | Acción                         | Sistema          | Validación         |
| ---- | ------------------------------ | ---------------- | ------------------ |
| 1    | Trigger de ejecución           | SchedulerService | Hora programada    |
| 2    | Obtener campaña                | Database         | Status = Scheduled |
| 3    | Actualizar status              | Database         | Running            |
| 4    | Obtener miembros audiencia     | AudienceService  | Lista de emails    |
| 5    | Obtener template               | MarketingService | Con contenido      |
| 6    | Por cada batch (100)           | Loop             | Procesamiento      |
| 7    | Personalizar email             | TemplateEngine   | Con datos usuario  |
| 8    | Enviar via NotificationService | HTTP             | Email              |
| 9    | Actualizar SentCount           | Database         | Incrementar        |
| 10   | Si error                       | Handle           | Log y continuar    |
| 11   | Al completar todos             | Check            | Status = Completed |
| 12   | Publicar evento                | RabbitMQ         | campaign.completed |

---

### 4.4 MKT-AUD-001: Crear Audiencia

| Campo       | Valor                       |
| ----------- | --------------------------- |
| **ID**      | MKT-AUD-001                 |
| **Nombre**  | Crear Segmento de Audiencia |
| **Actor**   | Dealer                      |
| **Trigger** | POST /api/audiences         |

#### Flujo del Proceso

| Paso | Acción                     | Sistema          | Validación             |
| ---- | -------------------------- | ---------------- | ---------------------- |
| 1    | Dealer accede a Audiencias | Dashboard        | Marketing habilitado   |
| 2    | Click "Nueva Audiencia"    | Frontend         | Builder de reglas      |
| 3    | Definir nombre             | Frontend         | Único para dealer      |
| 4    | Agregar reglas de segmento | Frontend         | Field, operator, value |
| 5    | Definir combinación        | Frontend         | AND/OR                 |
| 6    | Validar reglas             | MarketingService | Sintaxis correcta      |
| 7    | Ejecutar query preview     | MarketingService | Contar matches         |
| 8    | Crear audiencia            | Database         | Con MemberCount        |
| 9    | Publicar evento            | RabbitMQ         | audience.created       |

#### Request

```json
{
  "name": "Leads sin compra últimos 30 días",
  "description": "Usuarios que mostraron interés pero no compraron",
  "segmentCriteria": {
    "rules": [
      {
        "field": "hasContactedLast30Days",
        "operator": "equals",
        "value": true
      },
      {
        "field": "hasPurchased",
        "operator": "equals",
        "value": false
      }
    ],
    "combination": "AND"
  }
}
```

---

### 4.5 MKT-TRACK-001: Tracking de Email

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **ID**      | MKT-TRACK-001              |
| **Nombre**  | Tracking de Opens y Clicks |
| **Actor**   | Sistema                    |
| **Trigger** | Pixel/Link click           |

#### Flujo del Proceso (Open)

| Paso | Acción                   | Sistema              | Validación            |
| ---- | ------------------------ | -------------------- | --------------------- |
| 1    | Email incluye pixel 1x1  | MarketingService     | En footer             |
| 2    | Usuario abre email       | Email Client         | Carga pixel           |
| 3    | Request a tracking pixel | TrackingAPI          | Con campaign/user ID  |
| 4    | Registrar open           | EventTrackingService | Timestamp, IP, device |
| 5    | Incrementar OpenedCount  | Database             | En campaña            |
| 6    | Publicar evento          | RabbitMQ             | email.opened          |

#### Flujo del Proceso (Click)

| Paso | Acción                   | Sistema              | Validación      |
| ---- | ------------------------ | -------------------- | --------------- |
| 1    | Links en email wrapeados | MarketingService     | Redirect URL    |
| 2    | Usuario click link       | Email                | Redirect        |
| 3    | Registrar click          | EventTrackingService | Link, timestamp |
| 4    | Incrementar ClickedCount | Database             | En campaña      |
| 5    | Redirect a destino final | 302 Redirect         | URL original    |
| 6    | Publicar evento          | RabbitMQ             | email.clicked   |

---

## 5. Reglas de Negocio

### 5.1 Límites por Plan de Dealer

| Plan       | Campañas/mes | Emails/mes | Audiencias |
| ---------- | ------------ | ---------- | ---------- |
| Starter    | 2            | 1,000      | 3          |
| Pro        | 10           | 10,000     | 10         |
| Enterprise | Ilimitado    | 50,000     | Ilimitado  |

### 5.2 Reglas de Envío

| Regla                   | Valor                 |
| ----------------------- | --------------------- |
| Rate limit              | 100 emails/segundo    |
| Cooldown entre campañas | 24 horas              |
| Unsubscribe obligatorio | Footer de email       |
| Max retries por email   | 3                     |
| Bounce threshold        | 5% (pausa automática) |

### 5.3 Segmentos Predefinidos

| Segmento          | Descripción            |
| ----------------- | ---------------------- |
| `all_subscribers` | Todos los suscriptores |
| `recent_leads`    | Leads últimos 7 días   |
| `hot_leads`       | Leads con score > 70   |
| `inactive_30d`    | Sin actividad 30 días  |
| `high_value`      | Compras > $100,000     |

---

## 6. Métricas de Campaña

### 6.1 KPIs Calculados

```csharp
public class CampaignStats
{
    public double DeliveryRate => (double)DeliveredCount / SentCount * 100;
    public double OpenRate => (double)OpenedCount / DeliveredCount * 100;
    public double ClickRate => (double)ClickedCount / OpenedCount * 100;
    public double CTR => (double)ClickedCount / DeliveredCount * 100;
    public double UnsubscribeRate => (double)UnsubscribedCount / DeliveredCount * 100;
    public double BounceRate => (double)BouncedCount / SentCount * 100;
}
```

### 6.2 Benchmarks

| Métrica          | Bueno  | Excelente |
| ---------------- | ------ | --------- |
| Open Rate        | > 20%  | > 35%     |
| Click Rate       | > 2.5% | > 5%      |
| Unsubscribe Rate | < 0.5% | < 0.2%    |
| Bounce Rate      | < 2%   | < 0.5%    |

---

## 7. Eventos RabbitMQ

| Evento               | Exchange           | Payload                             |
| -------------------- | ------------------ | ----------------------------------- |
| `campaign.created`   | `marketing.events` | `{ campaignId, dealerId, type }`    |
| `campaign.scheduled` | `marketing.events` | `{ campaignId, scheduledDate }`     |
| `campaign.started`   | `marketing.events` | `{ campaignId, recipientCount }`    |
| `campaign.completed` | `marketing.events` | `{ campaignId, stats }`             |
| `campaign.cancelled` | `marketing.events` | `{ campaignId, reason }`            |
| `email.sent`         | `marketing.events` | `{ campaignId, recipientId }`       |
| `email.opened`       | `marketing.events` | `{ campaignId, recipientId }`       |
| `email.clicked`      | `marketing.events` | `{ campaignId, recipientId, link }` |
| `audience.created`   | `marketing.events` | `{ audienceId, memberCount }`       |
| `audience.refreshed` | `marketing.events` | `{ audienceId, newCount }`          |

---

## 8. Configuración

```json
{
  "Marketing": {
    "Campaigns": {
      "BatchSize": 100,
      "RateLimit": 100,
      "MaxRetries": 3,
      "CooldownHours": 24,
      "BounceThreshold": 5.0
    },
    "Tracking": {
      "PixelEnabled": true,
      "LinkWrapping": true,
      "TrackingDomain": "track.okla.com.do"
    },
    "Templates": {
      "MaxSize": 512000,
      "RequireUnsubscribe": true
    }
  }
}
```

---

## 📚 Referencias

- [01-notification-service.md](01-notification-service.md) - Sistema de notificaciones
- [02-templates-scheduling.md](02-templates-scheduling.md) - Templates
- [04-event-tracking.md](../13-INTEGRACIONES-EXTERNAS/04-event-tracking.md) - Tracking de eventos
