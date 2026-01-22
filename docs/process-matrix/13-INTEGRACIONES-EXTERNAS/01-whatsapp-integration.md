# 💬 WhatsApp Integration - Integración WhatsApp Business - Matriz de Procesos

> **Servicio:** NotificationService / ChatService  
> **Proveedor:** Meta WhatsApp Business API (via Twilio/360dialog)  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 1. Información General

### 1.1 Descripción

Integración con WhatsApp Business API para comunicación bidireccional con usuarios. Permite enviar notificaciones, responder consultas, y facilitar la conexión entre compradores y vendedores.

### 1.2 Casos de Uso

| Caso               | Descripción                                        | Iniciador |
| ------------------ | -------------------------------------------------- | --------- |
| **Notificaciones** | Alertas de sistema (vehículo aprobado, lead nuevo) | Sistema   |
| **Lead Contact**   | Comprador contacta vendedor                        | Usuario   |
| **Support**        | Soporte al cliente                                 | Usuario   |
| **Marketing**      | Campañas opt-in                                    | Sistema   |
| **Chatbot**        | Respuestas automáticas                             | Bot       |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     WhatsApp Integration Architecture                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   User Device                   OKLA Backend                            │
│   ┌──────────┐                 ┌──────────────────────────────────────┐ │
│   │ WhatsApp │                 │                                      │ │
│   │   App    │                 │  ┌────────────────┐                  │ │
│   └────┬─────┘                 │  │ NotificationSvc│                  │ │
│        │                       │  │                │                  │ │
│        │                       │  │  WhatsApp      │                  │ │
│        │                       │  │  Provider      │                  │ │
│        │                       │  └────────┬───────┘                  │ │
│        │                       │           │                          │ │
│        ▼                       │           ▼                          │ │
│   ┌──────────┐                 │  ┌────────────────┐                  │ │
│   │  Meta    │                 │  │    Twilio      │                  │ │
│   │ WhatsApp │ ◀──────────────▶│  │ (or 360dialog) │                  │ │
│   │  Cloud   │    Webhooks     │  │                │                  │ │
│   └──────────┘                 │  └────────────────┘                  │ │
│        │                       │           │                          │ │
│        │                       │           ▼                          │ │
│        │                       │  ┌────────────────┐                  │ │
│        │                       │  │   ChatService  │                  │ │
│        │                       │  │   (Chatbot)    │                  │ │
│        │                       │  └────────────────┘                  │ │
│        │                       │                                      │ │
│        │ Message               └──────────────────────────────────────┘ │
│        │ Delivered                                                      │
│        ▼                                                                │
│   ┌──────────┐                                                         │
│   │  User    │                                                         │
│   │ Receives │                                                         │
│   └──────────┘                                                         │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

### 2.1 Outbound Messages

| Método | Endpoint                      | Descripción      | Auth    |
| ------ | ----------------------------- | ---------------- | ------- |
| `POST` | `/api/whatsapp/send`          | Enviar mensaje   | Service |
| `POST` | `/api/whatsapp/send-template` | Enviar template  | Service |
| `POST` | `/api/whatsapp/send-media`    | Enviar con media | Service |
| `GET`  | `/api/whatsapp/templates`     | Listar templates | Admin   |
| `POST` | `/api/whatsapp/templates`     | Crear template   | Admin   |

### 2.2 Inbound (Webhooks)

| Método | Endpoint                 | Descripción             | Auth              |
| ------ | ------------------------ | ----------------------- | ----------------- |
| `POST` | `/api/webhooks/whatsapp` | Recibir mensajes        | Webhook Signature |
| `GET`  | `/api/webhooks/whatsapp` | Verificación de webhook | Meta Challenge    |

### 2.3 Conversations

| Método | Endpoint                                  | Descripción           | Auth  |
| ------ | ----------------------------------------- | --------------------- | ----- |
| `GET`  | `/api/whatsapp/conversations`             | Listar conversaciones | Admin |
| `GET`  | `/api/whatsapp/conversations/{id}`        | Ver conversación      | Admin |
| `POST` | `/api/whatsapp/conversations/{id}/assign` | Asignar agente        | Admin |

---

## 3. Entidades

### 3.1 WhatsAppMessage

```csharp
public class WhatsAppMessage
{
    public Guid Id { get; set; }
    public string MessageId { get; set; } = string.Empty; // ID de WhatsApp
    public Guid? ConversationId { get; set; }

    public string PhoneNumber { get; set; } = string.Empty; // +18091234567
    public Guid? UserId { get; set; } // Si está registrado

    public MessageDirection Direction { get; set; }
    public MessageType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public string? TemplateName { get; set; }
    public string? TemplateParams { get; set; } // JSON

    public MessageStatus Status { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
}

public enum MessageDirection
{
    Inbound,
    Outbound
}

public enum MessageType
{
    Text,
    Template,
    Image,
    Document,
    Video,
    Audio,
    Location,
    Contact,
    Interactive
}

public enum MessageStatus
{
    Pending,
    Sent,
    Delivered,
    Read,
    Failed
}
```

### 3.2 WhatsAppConversation

```csharp
public class WhatsAppConversation
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid? UserId { get; set; }

    public ConversationType Type { get; set; }
    public ConversationStatus Status { get; set; }
    public Guid? AssignedAgentId { get; set; }
    public Guid? RelatedEntityId { get; set; } // VehicleId, LeadId, etc.
    public string? RelatedEntityType { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? WindowExpiresAt { get; set; } // 24h window

    public ICollection<WhatsAppMessage> Messages { get; set; } = new List<WhatsAppMessage>();
}

public enum ConversationType
{
    Support,
    Lead,
    Notification,
    Marketing,
    Chatbot
}

public enum ConversationStatus
{
    Active,
    Pending,
    Assigned,
    Closed
}
```

### 3.3 WhatsAppTemplate

```csharp
public class WhatsAppTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = "es";
    public string Category { get; set; } = string.Empty; // MARKETING, UTILITY, AUTHENTICATION

    public string HeaderType { get; set; } = "NONE"; // NONE, TEXT, IMAGE, VIDEO, DOCUMENT
    public string? HeaderContent { get; set; }

    public string BodyText { get; set; } = string.Empty;
    public List<string> BodyParameters { get; set; } = new(); // {{1}}, {{2}}, etc.

    public string? FooterText { get; set; }
    public string? ButtonsJson { get; set; } // JSON de botones

    public TemplateStatus Status { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public enum TemplateStatus
{
    Pending,
    Approved,
    Rejected,
    Paused
}
```

---

## 4. Templates de WhatsApp

### 4.1 Templates Aprobados

| Template                | Categoría | Uso                        |
| ----------------------- | --------- | -------------------------- |
| `okla_welcome`          | UTILITY   | Bienvenida a nuevo usuario |
| `okla_vehicle_approved` | UTILITY   | Vehículo aprobado          |
| `okla_new_lead`         | UTILITY   | Nuevo lead para vendedor   |
| `okla_payment_confirm`  | UTILITY   | Confirmación de pago       |
| `okla_vehicle_interest` | MARKETING | Recordatorio de vehículo   |

### 4.2 Ejemplo de Templates

```json
{
  "okla_vehicle_approved": {
    "name": "okla_vehicle_approved",
    "language": "es",
    "category": "UTILITY",
    "components": [
      {
        "type": "HEADER",
        "format": "TEXT",
        "text": "¡Tu vehículo está publicado! 🚗"
      },
      {
        "type": "BODY",
        "text": "Hola {{1}}, tu {{2}} {{3}} {{4}} ya está visible en OKLA.\n\nPrecio: RD${{5}}\n\nPuedes ver tu publicación aquí:"
      },
      {
        "type": "FOOTER",
        "text": "OKLA - El marketplace de vehículos #1 en RD"
      },
      {
        "type": "BUTTONS",
        "buttons": [
          {
            "type": "URL",
            "text": "Ver Publicación",
            "url": "https://okla.com.do/vehiculos/{{6}}"
          }
        ]
      }
    ]
  }
}
```

```json
{
  "okla_new_lead": {
    "name": "okla_new_lead",
    "language": "es",
    "category": "UTILITY",
    "components": [
      {
        "type": "HEADER",
        "format": "TEXT",
        "text": "📬 ¡Nuevo interesado!"
      },
      {
        "type": "BODY",
        "text": "{{1}} está interesado en tu {{2}} {{3}} {{4}}.\n\nMensaje: \"{{5}}\"\n\nResponde rápido para aumentar tus chances de venta."
      },
      {
        "type": "BUTTONS",
        "buttons": [
          {
            "type": "QUICK_REPLY",
            "text": "Responder ahora"
          },
          {
            "type": "QUICK_REPLY",
            "text": "Ver detalles"
          }
        ]
      }
    ]
  }
}
```

---

## 5. Procesos Detallados

### 5.1 WA-001: Enviar Notificación por Template

| Paso | Acción                       | Sistema             | Validación         |
| ---- | ---------------------------- | ------------------- | ------------------ |
| 1    | Evento dispara notificación  | RabbitMQ            | Evento válido      |
| 2    | Verificar opt-in del usuario | NotificationService | Tiene opt-in       |
| 3    | Verificar número de WhatsApp | NotificationService | Número válido      |
| 4    | Obtener template             | NotificationService | Template aprobado  |
| 5    | Construir parámetros         | NotificationService | Params completos   |
| 6    | Llamar API de Twilio         | Twilio SDK          | Request válido     |
| 7    | Guardar mensaje en DB        | NotificationService | Mensaje guardado   |
| 8    | Recibir webhook de entrega   | Twilio Webhook      | Status = delivered |
| 9    | Actualizar status            | NotificationService | Status actualizado |

```csharp
public class WhatsAppService : IWhatsAppService
{
    private readonly TwilioRestClient _twilioClient;
    private readonly IWhatsAppRepository _repository;

    public async Task<SendResult> SendTemplateAsync(
        string phoneNumber,
        string templateName,
        Dictionary<string, string> parameters,
        CancellationToken ct = default)
    {
        // 1. Validate phone number (E.164 format)
        var formattedNumber = FormatE164(phoneNumber);

        // 2. Get template
        var template = await _repository.GetTemplateAsync(templateName, ct);
        if (template == null || template.Status != TemplateStatus.Approved)
            throw new InvalidOperationException($"Template {templateName} not available");

        // 3. Build template components
        var contentSid = await GetContentSidAsync(templateName);
        var variables = BuildVariables(template, parameters);

        // 4. Send via Twilio
        var message = await MessageResource.CreateAsync(
            to: new PhoneNumber($"whatsapp:{formattedNumber}"),
            from: new PhoneNumber($"whatsapp:{_config.WhatsAppNumber}"),
            contentSid: contentSid,
            contentVariables: JsonSerializer.Serialize(variables),
            client: _twilioClient
        );

        // 5. Store message
        var dbMessage = new WhatsAppMessage
        {
            MessageId = message.Sid,
            PhoneNumber = formattedNumber,
            Direction = MessageDirection.Outbound,
            Type = MessageType.Template,
            TemplateName = templateName,
            TemplateParams = JsonSerializer.Serialize(parameters),
            Status = MapStatus(message.Status),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddMessageAsync(dbMessage, ct);

        return new SendResult(message.Sid, MapStatus(message.Status));
    }
}
```

### 5.2 WA-002: Procesar Mensaje Entrante

| Paso | Acción                          | Sistema             | Validación           |
| ---- | ------------------------------- | ------------------- | -------------------- |
| 1    | Webhook recibe mensaje          | API                 | Signature válida     |
| 2    | Parsear payload de Twilio       | NotificationService | Payload válido       |
| 3    | Buscar/crear conversación       | NotificationService | Conversación activa  |
| 4    | Guardar mensaje                 | NotificationService | Mensaje guardado     |
| 5    | Identificar usuario (si existe) | UserService         | Usuario encontrado   |
| 6    | Clasificar intención            | ChatbotService      | Intent identificado  |
| 7    | Si chatbot puede responder      | ChatbotService      | Respuesta automática |
| 8    | Si no, asignar a agente         | SupportService      | Agente asignado      |
| 9    | Notificar agente                | NotificationService | Notificación enviada |

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Inbound Message Processing                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Twilio Webhook                                                        │
│        │                                                                │
│        ▼                                                                │
│   ┌─────────────────┐                                                   │
│   │ Validate Signature│                                                  │
│   └────────┬────────┘                                                   │
│            │                                                            │
│            ▼                                                            │
│   ┌─────────────────┐                                                   │
│   │ Parse Message   │                                                   │
│   └────────┬────────┘                                                   │
│            │                                                            │
│            ▼                                                            │
│   ┌─────────────────┐    No     ┌─────────────────┐                     │
│   │ Conversation    │ ─────────▶│ Create New      │                     │
│   │ Exists?         │           │ Conversation    │                     │
│   └────────┬────────┘           └────────┬────────┘                     │
│            │ Yes                         │                              │
│            └──────────┬──────────────────┘                              │
│                       ▼                                                 │
│              ┌─────────────────┐                                        │
│              │ Save Message    │                                        │
│              └────────┬────────┘                                        │
│                       │                                                 │
│                       ▼                                                 │
│              ┌─────────────────┐                                        │
│              │ Classify Intent │                                        │
│              └────────┬────────┘                                        │
│                       │                                                 │
│       ┌───────────────┼───────────────┐                                │
│       ▼               ▼               ▼                                │
│   ┌────────┐    ┌──────────┐    ┌───────────┐                          │
│   │Greeting│    │ Question │    │  Support  │                          │
│   │ (Bot)  │    │  (Bot)   │    │  (Agent)  │                          │
│   └────────┘    └──────────┘    └───────────┘                          │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.3 WA-003: Contacto Lead via WhatsApp

| Paso | Acción                                          | Sistema             | Validación          |
| ---- | ----------------------------------------------- | ------------------- | ------------------- |
| 1    | Comprador hace clic en "Contactar por WhatsApp" | Frontend            | Vehículo válido     |
| 2    | Crear lead en sistema                           | LeadService         | Lead creado         |
| 3    | Generar link de WhatsApp                        | Frontend            | Link generado       |
| 4    | Usuario abre WhatsApp                           | WhatsApp App        | App abierta         |
| 5    | Usuario envía mensaje                           | WhatsApp            | Mensaje enviado     |
| 6    | Webhook recibe mensaje                          | NotificationService | Mensaje recibido    |
| 7    | Vincular a lead existente                       | LeadService         | Lead vinculado      |
| 8    | Notificar vendedor                              | NotificationService | Vendedor notificado |
| 9    | Vendedor responde                               | WhatsApp            | Respuesta enviada   |
| 10   | Actualizar status del lead                      | LeadService         | Status = Contacted  |

---

## 6. 24-Hour Window Rule

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     WhatsApp 24-Hour Window                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   REGLA: Solo puedes enviar mensajes free-form (no templates)           │
│          dentro de 24 horas después del último mensaje del usuario      │
│                                                                          │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                                                                 │   │
│   │  User sends    Window Opens           Window Closes             │   │
│   │  message       (24h session)          (Templates only)          │   │
│   │     │              │                        │                   │   │
│   │     ▼              │                        ▼                   │   │
│   │  ───●──────────────┼────────────────────────●───────────────▶   │   │
│   │     │              │                        │                   │   │
│   │     │◀────── FREE FORM ALLOWED ──────▶│    │                   │   │
│   │     │     Text, Images, Media         │    │                   │   │
│   │     │                                 │    │                   │   │
│   │     │                                 │    │◀─ TEMPLATES ONLY  │   │
│   │                                                                 │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│   Mensaje dentro de ventana:                                            │
│   ├── Cualquier tipo de mensaje                                        │
│   ├── Sin costo adicional por conversación                             │
│   └── Respuestas inmediatas                                            │
│                                                                          │
│   Mensaje fuera de ventana:                                             │
│   ├── SOLO templates pre-aprobados                                     │
│   ├── Costo por template enviado                                       │
│   └── Útil para re-engagement                                          │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

```csharp
public class WhatsAppWindowService
{
    public async Task<bool> CanSendFreeFormAsync(string phoneNumber, CancellationToken ct)
    {
        var conversation = await _repository.GetActiveConversationAsync(phoneNumber, ct);

        if (conversation == null)
            return false;

        // Last inbound message
        var lastInbound = await _repository.GetLastInboundMessageAsync(phoneNumber, ct);

        if (lastInbound == null)
            return false;

        // Window is 24 hours
        var windowExpiry = lastInbound.CreatedAt.AddHours(24);

        return DateTime.UtcNow < windowExpiry;
    }

    public async Task<SendResult> SendMessageAsync(
        string phoneNumber,
        string content,
        CancellationToken ct)
    {
        if (await CanSendFreeFormAsync(phoneNumber, ct))
        {
            // Send free-form message
            return await SendTextMessageAsync(phoneNumber, content, ct);
        }
        else
        {
            // Must use template - fallback to generic template
            return await SendTemplateAsync(
                phoneNumber,
                "okla_reengagement",
                new { message = content },
                ct);
        }
    }
}
```

---

## 7. Reglas de Negocio

| Código | Regla                             | Validación                 |
| ------ | --------------------------------- | -------------------------- |
| WA-R01 | Solo enviar a usuarios con opt-in | WhatsAppOptIn == true      |
| WA-R02 | Respetar ventana de 24h           | LastInbound + 24h > Now    |
| WA-R03 | Templates deben estar aprobados   | TemplateStatus == Approved |
| WA-R04 | Limitar mensajes marketing        | Max 1 por semana           |
| WA-R05 | Horario de envío: 8am-9pm         | LocalTime entre 8-21       |
| WA-R06 | Opt-out inmediato                 | Palabra "STOP" cancela     |

---

## 8. Códigos de Error

| Código   | HTTP | Mensaje               | Causa                        |
| -------- | ---- | --------------------- | ---------------------------- |
| `WA_001` | 400  | Invalid phone number  | Número no válido             |
| `WA_002` | 400  | User not opted in     | Sin opt-in                   |
| `WA_003` | 400  | Template not approved | Template pendiente/rechazado |
| `WA_004` | 400  | Window expired        | Ventana 24h expirada         |
| `WA_005` | 429  | Rate limit exceeded   | Muchos mensajes              |
| `WA_006` | 500  | Provider error        | Error de Twilio              |

---

## 9. Eventos RabbitMQ

| Evento                             | Exchange          | Descripción           |
| ---------------------------------- | ----------------- | --------------------- |
| `WhatsAppMessageSentEvent`         | `whatsapp.events` | Mensaje enviado       |
| `WhatsAppMessageDeliveredEvent`    | `whatsapp.events` | Mensaje entregado     |
| `WhatsAppMessageReadEvent`         | `whatsapp.events` | Mensaje leído         |
| `WhatsAppMessageReceivedEvent`     | `whatsapp.events` | Mensaje recibido      |
| `WhatsAppConversationStartedEvent` | `whatsapp.events` | Conversación iniciada |

---

## 10. Configuración

```json
{
  "WhatsApp": {
    "Provider": "Twilio",
    "AccountSid": "${TWILIO_ACCOUNT_SID}",
    "AuthToken": "${TWILIO_AUTH_TOKEN}",
    "WhatsAppNumber": "+18091234567",
    "WebhookUrl": "https://api.okla.com.do/api/webhooks/whatsapp",
    "SendingHours": {
      "Start": "08:00",
      "End": "21:00",
      "Timezone": "America/Santo_Domingo"
    },
    "RateLimits": {
      "PerUserPerDay": 10,
      "MarketingPerWeek": 1
    }
  }
}
```

---

## 11. Métricas Prometheus

```
# Mensajes enviados
whatsapp_messages_sent_total{template="...", status="..."}

# Mensajes recibidos
whatsapp_messages_received_total

# Delivery rate
whatsapp_delivery_rate

# Read rate
whatsapp_read_rate

# Response time
whatsapp_response_time_seconds
```

---

## 📚 Referencias

- [Meta WhatsApp Business API](https://developers.facebook.com/docs/whatsapp/cloud-api) - Documentación oficial
- [Twilio WhatsApp](https://www.twilio.com/docs/whatsapp) - Proveedor
- [02-sms-integration.md](02-sms-integration.md) - SMS
