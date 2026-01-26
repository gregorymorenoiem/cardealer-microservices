# 💬 Teams Integration - Integración con Microsoft Teams - Matriz de Procesos

> **Servicio:** NotificationService  
> **Provider:** Microsoft Graph API  
> **Última actualización:** Enero 25, 2026  
> **Estado:** 🟡 PLANIFICADO  
> **Estado de Implementación:** 🔴 0% Backend | UI: N/A (Integración interna)

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                 | Backend      | UI Access | Observación     |
| ----------------------- | ------------ | --------- | --------------- |
| TEAMS-HOOK-001 Webhooks | 🔴 Pendiente | N/A       | Sin integración |
| TEAMS-MSG-001 Mensajes  | 🔴 Pendiente | N/A       | Sin integración |
| TEAMS-ALERT-001 Alertas | 🔴 Pendiente | N/A       | Sin integración |

### Rutas UI Existentes ✅

- N/A - Integración interna sin UI pública

### Rutas UI Faltantes 🔴

- `/admin/integrations/teams` → Configuración de webhooks Teams (opcional)

**Verificación Backend:** Integración Teams **NO** implementada ⚠️

> ⚠️ **NOTA:** Integración interna para equipo OKLA. Prioridad baja.

---

## 📊 Resumen de Implementación

| Componente                     | Total | Implementado | Pendiente | Estado         |
| ------------------------------ | ----- | ------------ | --------- | -------------- |
| **TEAMS-HOOK-\*** (Webhooks)   | 3     | 0            | 3         | 🔴 Pendiente   |
| **TEAMS-MSG-\*** (Mensajes)    | 4     | 0            | 4         | 🔴 Pendiente   |
| **TEAMS-CARD-\*** (Cards)      | 4     | 0            | 4         | 🔴 Pendiente   |
| **TEAMS-ALERT-\*** (Alertas)   | 4     | 0            | 4         | 🔴 Pendiente   |
| **TEAMS-REPORT-\*** (Reportes) | 3     | 0            | 3         | 🔴 Pendiente   |
| **Tests**                      | 0     | 0            | 15        | 🔴 Pendiente   |
| **TOTAL**                      | 18    | 0            | 18        | 🔴 0% Completo |

---

## 1. Información General

### 1.1 Descripción

Integración con Microsoft Teams para notificaciones internas del equipo OKLA, alertas del sistema, reportes automáticos y comunicación entre departamentos. Utiliza Microsoft Graph API y Webhooks de Teams.

### 1.2 Casos de Uso

| Tipo                | Canal               | Descripción                         |
| ------------------- | ------------------- | ----------------------------------- |
| Alertas del Sistema | #alerts-okla        | Alertas críticas de infraestructura |
| Nuevos Dealers      | #dealers-ventas     | Notificación de dealer registrado   |
| Ventas Grandes      | #ventas-highlight   | Transacciones > $50K                |
| Soporte             | #soporte-tier2      | Tickets escalados                   |
| Compliance          | #compliance-alertas | Alertas de compliance               |
| Reportes Diarios    | #reportes           | Métricas automáticas                |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      Teams Integration Architecture                      │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   OKLA Services                            Microsoft Teams              │
│   ┌─────────────────┐                     ┌─────────────────┐           │
│   │ NotificationSvc │                     │                 │           │
│   │                 │                     │   #alerts-okla  │           │
│   │ ┌─────────────┐ │    Webhook POST     │   #ventas       │           │
│   │ │ Teams       │ │─────────────────────▶│   #soporte      │           │
│   │ │ Notifier    │ │                     │   #reportes     │           │
│   │ └─────────────┘ │                     │                 │           │
│   └────────┬────────┘                     └─────────────────┘           │
│            │                                      │                     │
│            │                                      │                     │
│   ┌────────▼────────┐                     ┌───────▼───────┐            │
│   │   RabbitMQ      │                     │ MS Graph API  │            │
│   │   (Events)      │                     │               │            │
│   └────────┬────────┘                     │ - User lookup │            │
│            │                              │ - Channels    │            │
│            │                              │ - Messages    │            │
│   ┌────────▼────────┐                     └───────────────┘            │
│   │ Event           │                                                   │
│   │ Handlers        │                                                   │
│   │                 │                                                   │
│   │ - System alerts │                                                   │
│   │ - New dealers   │                                                   │
│   │ - Big sales     │                                                   │
│   │ - Escalations   │                                                   │
│   └─────────────────┘                                                   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

| Método | Endpoint                            | Descripción                 | Auth  |
| ------ | ----------------------------------- | --------------------------- | ----- |
| `POST` | `/api/notifications/teams/send`     | Enviar mensaje a Teams      | Admin |
| `POST` | `/api/notifications/teams/card`     | Enviar Adaptive Card        | Admin |
| `GET`  | `/api/notifications/teams/channels` | Listar canales configurados | Admin |
| `POST` | `/api/notifications/teams/test`     | Enviar mensaje de prueba    | Admin |

---

## 3. Entidades

### 3.1 TeamsChannel

```csharp
public class TeamsChannel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public TeamsChannelType Type { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public enum TeamsChannelType
{
    Alerts,
    Sales,
    Support,
    Compliance,
    Reports,
    General
}
```

### 3.2 TeamsMessage

```csharp
public class TeamsMessage
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public TeamsMessageType Type { get; set; }
    public string? CardJson { get; set; }
    public DateTime SentAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum TeamsMessageType
{
    Text,
    AdaptiveCard,
    HeroCard,
    Thumbnail
}
```

---

## 4. Tipos de Mensajes

### 4.1 Mensaje Simple

```json
{
  "@type": "MessageCard",
  "@context": "http://schema.org/extensions",
  "themeColor": "0076D7",
  "summary": "Nuevo dealer registrado",
  "sections": [
    {
      "activityTitle": "🏢 Nuevo Dealer: Auto San Juan",
      "facts": [
        {
          "name": "RNC",
          "value": "123456789"
        },
        {
          "name": "Tipo",
          "value": "Independent"
        },
        {
          "name": "Plan",
          "value": "Pro ($103/mes)"
        },
        {
          "name": "Registrado",
          "value": "2026-01-21 10:30 AM"
        }
      ],
      "markdown": true
    }
  ]
}
```

### 4.2 Adaptive Card (Alerta)

```json
{
  "type": "message",
  "attachments": [
    {
      "contentType": "application/vnd.microsoft.card.adaptive",
      "content": {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": [
          {
            "type": "TextBlock",
            "size": "Large",
            "weight": "Bolder",
            "text": "⚠️ Alerta de Sistema",
            "color": "Attention"
          },
          {
            "type": "FactSet",
            "facts": [
              { "title": "Servicio:", "value": "VehiclesSaleService" },
              { "title": "Tipo:", "value": "High CPU Usage" },
              { "title": "Valor:", "value": "95%" },
              { "title": "Hora:", "value": "2026-01-21T15:30:00Z" }
            ]
          }
        ],
        "actions": [
          {
            "type": "Action.OpenUrl",
            "title": "Ver Dashboard",
            "url": "https://admin.okla.com.do/monitoring"
          },
          {
            "type": "Action.OpenUrl",
            "title": "Ver Runbook",
            "url": "https://docs.okla.com/runbooks/high-cpu"
          }
        ]
      }
    }
  ]
}
```

### 4.3 Reporte Diario

```json
{
  "type": "message",
  "attachments": [
    {
      "contentType": "application/vnd.microsoft.card.adaptive",
      "content": {
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": [
          {
            "type": "TextBlock",
            "size": "Large",
            "weight": "Bolder",
            "text": "📊 Reporte Diario OKLA - 21 Ene 2026"
          },
          {
            "type": "ColumnSet",
            "columns": [
              {
                "type": "Column",
                "items": [
                  {
                    "type": "TextBlock",
                    "text": "Vehículos Nuevos",
                    "weight": "Bolder"
                  },
                  { "type": "TextBlock", "text": "45", "size": "ExtraLarge" }
                ]
              },
              {
                "type": "Column",
                "items": [
                  { "type": "TextBlock", "text": "Leads", "weight": "Bolder" },
                  { "type": "TextBlock", "text": "127", "size": "ExtraLarge" }
                ]
              },
              {
                "type": "Column",
                "items": [
                  { "type": "TextBlock", "text": "Ventas", "weight": "Bolder" },
                  { "type": "TextBlock", "text": "8", "size": "ExtraLarge" }
                ]
              }
            ]
          },
          {
            "type": "FactSet",
            "facts": [
              { "title": "Revenue:", "value": "$245,000 DOP" },
              { "title": "Dealers Activos:", "value": "125" },
              { "title": "Usuarios Nuevos:", "value": "89" }
            ]
          }
        ]
      }
    }
  ]
}
```

---

## 5. Procesos Detallados

### 5.1 TEAMS-001: Notificar Nuevo Dealer

| Paso | Acción                         | Sistema             | Validación         |
| ---- | ------------------------------ | ------------------- | ------------------ |
| 1    | Dealer completa registro       | DealerManagement    | Dealer created     |
| 2    | Publicar DealerRegisteredEvent | RabbitMQ            | Event published    |
| 3    | Handler consume evento         | NotificationService | Event received     |
| 4    | Construir Adaptive Card        | NotificationService | Card JSON          |
| 5    | Obtener webhook de canal       | TeamsChannel        | Webhook URL        |
| 6    | POST al webhook                | MS Teams            | 200 OK             |
| 7    | Guardar log del mensaje        | NotificationService | TeamsMessage saved |

```csharp
public class DealerRegisteredTeamsHandler : IConsumer<DealerRegisteredEvent>
{
    private readonly ITeamsNotifier _teamsNotifier;

    public async Task Consume(ConsumeContext<DealerRegisteredEvent> context)
    {
        var dealer = context.Message;

        var card = new AdaptiveCard("1.4")
        {
            Body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock
                {
                    Text = $"🏢 Nuevo Dealer Registrado: {dealer.BusinessName}",
                    Size = AdaptiveTextSize.Large,
                    Weight = AdaptiveTextWeight.Bolder
                },
                new AdaptiveFactSet
                {
                    Facts = new List<AdaptiveFact>
                    {
                        new("RNC", dealer.RNC),
                        new("Tipo", dealer.DealerType.ToString()),
                        new("Plan", $"{dealer.Plan} (${dealer.PlanPrice}/mes)"),
                        new("Ciudad", dealer.City),
                        new("Early Bird", dealer.IsEarlyBird ? "✅ Sí" : "❌ No"),
                        new("Registrado", dealer.CreatedAt.ToString("yyyy-MM-dd HH:mm"))
                    }
                }
            },
            Actions = new List<AdaptiveAction>
            {
                new AdaptiveOpenUrlAction
                {
                    Title = "Ver en Admin",
                    Url = new Uri($"https://admin.okla.com.do/dealers/{dealer.Id}")
                }
            }
        };

        await _teamsNotifier.SendCardAsync(
            TeamsChannelType.Sales,
            card);
    }
}
```

### 5.2 TEAMS-002: Alerta de Sistema Crítica

| Paso | Acción                        | Sistema             | Validación       |
| ---- | ----------------------------- | ------------------- | ---------------- |
| 1    | Alertmanager dispara alerta   | Prometheus          | Alert triggered  |
| 2    | Webhook a NotificationService | Alertmanager        | Webhook received |
| 3    | Parsear alerta                | NotificationService | Alert parsed     |
| 4    | Determinar severidad          | NotificationService | Critical/Warning |
| 5    | Construir card con color      | NotificationService | Theme color      |
| 6    | Mencionar on-call si crítico  | NotificationService | @mention         |
| 7    | POST a Teams                  | MS Teams            | Sent             |

```csharp
public class AlertManagerWebhookController : ControllerBase
{
    [HttpPost("alertmanager")]
    public async Task<IActionResult> HandleAlert([FromBody] AlertManagerPayload payload)
    {
        foreach (var alert in payload.Alerts)
        {
            var severity = alert.Labels.GetValueOrDefault("severity", "warning");
            var themeColor = severity switch
            {
                "critical" => "FF0000", // Red
                "warning" => "FFA500",  // Orange
                _ => "0076D7"           // Blue
            };

            var card = BuildAlertCard(alert, themeColor);

            // Add @mention for critical alerts
            if (severity == "critical")
            {
                await _teamsNotifier.SendCardWithMentionAsync(
                    TeamsChannelType.Alerts,
                    card,
                    GetOnCallUser());
            }
            else
            {
                await _teamsNotifier.SendCardAsync(
                    TeamsChannelType.Alerts,
                    card);
            }
        }

        return Ok();
    }
}
```

### 5.3 TEAMS-003: Reporte Automático Diario

| Paso | Acción                            | Sistema             | Validación      |
| ---- | --------------------------------- | ------------------- | --------------- |
| 1    | Job scheduler (8 AM)              | Scheduler           | Cron triggered  |
| 2    | Obtener métricas del día anterior | ReportsService      | Aggregated data |
| 3    | Construir reporte card            | NotificationService | Card JSON       |
| 4    | Agregar gráficos (imágenes)       | NotificationService | Image URLs      |
| 5    | POST a canal #reportes            | MS Teams            | Sent            |
| 6    | Guardar log                       | NotificationService | Logged          |

---

## 6. Configuración de Webhooks

### 6.1 Crear Incoming Webhook en Teams

```
1. Abrir Teams → Canal deseado
2. Configuración → Connectors
3. Incoming Webhook → Configure
4. Nombrar: "OKLA Notifications"
5. Copiar URL del webhook
6. Guardar en configuración
```

### 6.2 Configuración en OKLA

```json
{
  "Teams": {
    "Enabled": true,
    "Channels": {
      "Alerts": {
        "WebhookUrl": "${TEAMS_ALERTS_WEBHOOK}",
        "MentionOnCritical": true
      },
      "Sales": {
        "WebhookUrl": "${TEAMS_SALES_WEBHOOK}",
        "MentionOnCritical": false
      },
      "Support": {
        "WebhookUrl": "${TEAMS_SUPPORT_WEBHOOK}",
        "MentionOnCritical": true
      },
      "Reports": {
        "WebhookUrl": "${TEAMS_REPORTS_WEBHOOK}",
        "DailyReportTime": "08:00",
        "WeeklyReportDay": "Monday"
      },
      "Compliance": {
        "WebhookUrl": "${TEAMS_COMPLIANCE_WEBHOOK}",
        "MentionOnCritical": true
      }
    },
    "OnCallRotation": {
      "Enabled": true,
      "PagerDutyIntegration": true
    }
  }
}
```

---

## 7. Reglas de Negocio

| Código    | Regla                                   | Validación          |
| --------- | --------------------------------------- | ------------------- |
| TEAMS-R01 | Solo alertas críticas mencionan on-call | severity = critical |
| TEAMS-R02 | Máximo 1 mensaje por minuto por tipo    | Rate limiting       |
| TEAMS-R03 | Reportes solo días hábiles              | Skip weekends       |
| TEAMS-R04 | Webhooks deben ser HTTPS                | URL validation      |
| TEAMS-R05 | Retry 3 veces con backoff               | Resilience          |

---

## 8. Eventos RabbitMQ Monitoreados

| Evento                  | Fuente     | Canal Teams         |
| ----------------------- | ---------- | ------------------- |
| `DealerRegisteredEvent` | DealerMgmt | #ventas             |
| `HighValueSaleEvent`    | Billing    | #ventas-highlight   |
| `TicketEscalatedEvent`  | Support    | #soporte-tier2      |
| `ComplianceAlertEvent`  | Compliance | #compliance-alertas |
| `SystemAlertEvent`      | Monitoring | #alerts-okla        |
| `DailyReportEvent`      | Reports    | #reportes           |

---

## 9. Métricas Prometheus

```
# Teams messages
teams_messages_sent_total{channel="...", type="..."}
teams_messages_failed_total{channel="...", error="..."}

# Webhook latency
teams_webhook_latency_ms{channel="..."}

# Rate limiting
teams_rate_limited_total{channel="..."}
```

---

## 📚 Referencias

- [Microsoft Teams Webhooks](https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/) - Documentación
- [Adaptive Cards](https://adaptivecards.io/) - Designer y ejemplos
- [01-notification-service.md](01-notification-service.md) - NotificationService principal
