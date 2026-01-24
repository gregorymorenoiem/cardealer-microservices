# 🤖 Chatbot Service - Matriz de Procesos

> **Servicio:** ChatbotService  
> **Puerto:** 5060  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente                       | Total | Implementado | Pendiente | Estado         |
| -------------------------------- | ----- | ------------ | --------- | -------------- |
| **Controllers**                  | 2     | 0            | 2         | 🔴 Pendiente   |
| **CHAT-NLU-\*** (Procesamiento)  | 5     | 0            | 5         | 🔴 Pendiente   |
| **CHAT-INTENT-\*** (Intenciones) | 4     | 0            | 4         | 🔴 Pendiente   |
| **CHAT-RESP-\*** (Respuestas)    | 4     | 0            | 4         | 🔴 Pendiente   |
| **CHAT-WA-\*** (WhatsApp)        | 4     | 0            | 4         | 🔴 Pendiente   |
| **CHAT-ESCAL-\*** (Escalamiento) | 3     | 0            | 3         | 🔴 Pendiente   |
| **Tests**                        | 0     | 0            | 20        | 🔴 Pendiente   |
| **TOTAL**                        | 22    | 0            | 22        | 🔴 0% Completo |

---

## 1. Información General

### 1.1 Descripción

Chatbot inteligente basado en IA para atención al cliente en OKLA. Maneja consultas sobre vehículos, agenda test drives, califica leads automáticamente y escala a agentes humanos cuando es necesario. Integrado con WhatsApp Business y chat in-app.

### 1.2 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      ARQUITECTURA CHATBOT                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  CANALES                     PROCESAMIENTO              ACCIONES         │
│  ────────                    ─────────────              ────────         │
│                                                                          │
│  ┌─────────────┐            ┌─────────────┐            ┌─────────────┐  │
│  │  WhatsApp   │────────>   │   NLU       │────────>   │  Responder  │  │
│  │  Business   │            │  (Intent +  │            │  (Templates)│  │
│  └─────────────┘            │   Entities) │            └─────────────┘  │
│                             └──────┬──────┘                              │
│  ┌─────────────┐                   │                   ┌─────────────┐  │
│  │  In-App     │────────>   ┌──────┴──────┐   ───────> │ Agendar     │  │
│  │  Chat       │            │   Dialog    │            │ Test Drive  │  │
│  └─────────────┘            │   Manager   │            └─────────────┘  │
│                             └──────┬──────┘                              │
│  ┌─────────────┐                   │                   ┌─────────────┐  │
│  │  Web Widget │────────>   ┌──────┴──────┐   ───────> │ Calificar   │  │
│  │             │            │   ML Model  │            │    Lead     │  │
│  └─────────────┘            │   (GPT-4)   │            └─────────────┘  │
│                             └─────────────┘                              │
│                                                        ┌─────────────┐  │
│                                                ───────>│  Escalar a  │  │
│                                                        │   Humano    │  │
│                                                        └─────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.3 Dependencias

| Servicio            | Propósito             |
| ------------------- | --------------------- |
| VehiclesSaleService | Búsqueda de vehículos |
| LeadService         | Crear/calificar leads |
| AppointmentService  | Agendar test drives   |
| UserService         | Datos del cliente     |
| NotificationService | Escalamiento          |
| OpenAI API          | GPT-4 para NLU        |
| WhatsApp Cloud API  | Mensajería            |

---

## 2. Endpoints API

### 2.1 ChatController

| Método | Endpoint                                       | Descripción           | Auth | Roles    |
| ------ | ---------------------------------------------- | --------------------- | ---- | -------- |
| `POST` | `/api/chat/message`                            | Enviar mensaje        | ❌   | Public   |
| `POST` | `/api/chat/webhook/whatsapp`                   | Webhook WhatsApp      | ❌\* | WhatsApp |
| `GET`  | `/api/chat/conversations/{sessionId}`          | Historial             | ✅   | User     |
| `POST` | `/api/chat/conversations/{sessionId}/escalate` | Escalar a humano      | ✅   | User     |
| `POST` | `/api/chat/feedback`                           | Feedback de respuesta | ✅   | User     |

### 2.2 ChatAdminController

| Método | Endpoint                                      | Descripción              | Auth | Roles         |
| ------ | --------------------------------------------- | ------------------------ | ---- | ------------- |
| `GET`  | `/api/chat/admin/conversations`               | Todas las conversaciones | ✅   | Dealer, Admin |
| `GET`  | `/api/chat/admin/conversations/escalated`     | Pendientes de humano     | ✅   | Dealer        |
| `POST` | `/api/chat/admin/conversations/{id}/takeover` | Tomar conversación       | ✅   | Dealer        |
| `POST` | `/api/chat/admin/conversations/{id}/resolve`  | Resolver                 | ✅   | Dealer        |
| `GET`  | `/api/chat/admin/statistics`                  | Estadísticas             | ✅   | Dealer, Admin |

---

## 3. Entidades y Enums

### 3.1 ConversationStatus (Enum)

```csharp
public enum ConversationStatus
{
    Active = 0,           // Conversación activa con bot
    Escalated = 1,        // Esperando agente humano
    WithHuman = 2,        // Siendo atendida por humano
    Resolved = 3,         // Resuelta
    Abandoned = 4         // Abandonada (timeout)
}
```

### 3.2 ChatChannel (Enum)

```csharp
public enum ChatChannel
{
    InApp = 0,            // Chat integrado en app/web
    WhatsApp = 1,         // WhatsApp Business
    WebWidget = 2,        // Widget en website externo
    Messenger = 3         // Facebook Messenger
}
```

### 3.3 IntentType (Enum)

```csharp
public enum IntentType
{
    Greeting = 0,                // Saludo
    VehicleSearch = 1,           // Buscar vehículo
    VehicleDetails = 2,          // Detalles de vehículo
    PriceInquiry = 3,            // Consulta de precio
    FinancingInquiry = 4,        // Consulta de financiamiento
    TestDriveRequest = 5,        // Agendar test drive
    ContactRequest = 6,          // Contactar vendedor
    DocumentsInquiry = 7,        // Consulta de documentos
    OperatingHours = 8,          // Horarios
    LocationInquiry = 9,         // Ubicación
    ComplaintFeedback = 10,      // Queja/feedback
    HumanRequest = 11,           // Quiere humano
    Unknown = 99                 // No identificado
}
```

### 3.4 Conversation (Entidad)

```csharp
public class Conversation
{
    public Guid Id { get; set; }
    public string SessionId { get; set; }          // Único por sesión
    public Guid? UserId { get; set; }              // Si está autenticado
    public Guid? DealerId { get; set; }            // Dealer al que pertenece
    public ChatChannel Channel { get; set; }
    public ConversationStatus Status { get; set; }

    // WhatsApp específico
    public string? WhatsAppPhoneNumber { get; set; }
    public string? WhatsAppProfileName { get; set; }

    // Contexto
    public Guid? CurrentVehicleId { get; set; }
    public string? CurrentIntent { get; set; }
    public string? ConversationContext { get; set; } // JSON

    // Lead
    public Guid? LeadId { get; set; }
    public int LeadScore { get; set; }

    // Agente humano
    public Guid? AssignedAgentId { get; set; }
    public DateTime? EscalatedAt { get; set; }

    // Métricas
    public int MessageCount { get; set; }
    public int BotResponseCount { get; set; }
    public int HumanResponseCount { get; set; }
    public double? SatisfactionRating { get; set; }

    // Timestamps
    public DateTime StartedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
```

### 3.5 ChatMessage (Entidad)

```csharp
public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }

    // Mensaje
    public string Content { get; set; }
    public MessageSender Sender { get; set; }      // User, Bot, Human
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }    // image, document, audio

    // NLU
    public IntentType? DetectedIntent { get; set; }
    public double? IntentConfidence { get; set; }
    public string? ExtractedEntities { get; set; } // JSON

    // Respuesta
    public string? ResponseTemplate { get; set; }
    public bool WasHelpful { get; set; }

    public DateTime SentAt { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 CHAT-001: Procesar Mensaje de Usuario

| Campo       | Valor                     |
| ----------- | ------------------------- |
| **ID**      | CHAT-001                  |
| **Nombre**  | Procesar Mensaje Entrante |
| **Actor**   | Usuario                   |
| **Trigger** | POST /api/chat/message    |

#### Flujo del Proceso

| Paso | Acción                | Sistema            | Validación             |
| ---- | --------------------- | ------------------ | ---------------------- |
| 1    | Usuario envía mensaje | Frontend/WhatsApp  | Texto/media            |
| 2    | Obtener/crear sesión  | ChatbotService     | Por sessionId          |
| 3    | Guardar mensaje       | Database           | ChatMessage            |
| 4    | Enviar a NLU          | OpenAI GPT-4       | Intent + Entities      |
| 5    | Actualizar contexto   | ChatbotService     | Conversation           |
| 6    | Determinar acción     | Dialog Manager     | Según intent           |
| 7    | Ejecutar acción       | ChatbotService     | Si aplica              |
| 8    | Generar respuesta     | ChatbotService     | Template + datos       |
| 9    | Guardar respuesta     | Database           | ChatMessage            |
| 10   | Actualizar lead score | LeadScoringService | +puntos                |
| 11   | Enviar respuesta      | Channel            | Al usuario             |
| 12   | Publicar evento       | RabbitMQ           | chat.message_processed |

#### Request

```json
{
  "sessionId": "session_abc123",
  "channel": "WhatsApp",
  "message": "Hola, busco un Toyota RAV4 2024",
  "phoneNumber": "+18295550100",
  "userId": null
}
```

#### NLU Response (GPT-4)

```json
{
  "intent": "VehicleSearch",
  "confidence": 0.95,
  "entities": {
    "make": "Toyota",
    "model": "RAV4",
    "year": 2024
  },
  "sentiment": "positive"
}
```

#### Bot Response

```json
{
  "sessionId": "session_abc123",
  "messages": [
    {
      "type": "text",
      "content": "¡Hola! 👋 Con gusto te ayudo a encontrar un Toyota RAV4 2024."
    },
    {
      "type": "text",
      "content": "Encontré 3 opciones disponibles:"
    },
    {
      "type": "carousel",
      "vehicles": [
        {
          "id": "uuid",
          "title": "Toyota RAV4 LE 2024",
          "price": 1850000,
          "image": "..."
        },
        {
          "id": "uuid",
          "title": "Toyota RAV4 XLE 2024",
          "price": 2100000,
          "image": "..."
        },
        {
          "id": "uuid",
          "title": "Toyota RAV4 Limited 2024",
          "price": 2400000,
          "image": "..."
        }
      ]
    },
    {
      "type": "quick_replies",
      "options": [
        "Ver más detalles",
        "Agendar test drive",
        "Hablar con vendedor"
      ]
    }
  ]
}
```

---

### 4.2 CHAT-002: Agendar Test Drive via Chat

| Campo       | Valor                    |
| ----------- | ------------------------ |
| **ID**      | CHAT-002                 |
| **Nombre**  | Agendar Test Drive       |
| **Actor**   | Usuario                  |
| **Trigger** | Intent: TestDriveRequest |

#### Flujo del Proceso

| Paso | Acción                         | Sistema             | Validación       |
| ---- | ------------------------------ | ------------------- | ---------------- |
| 1    | Detectar intent test drive     | NLU                 | TestDriveRequest |
| 2    | Verificar vehículo en contexto | ChatbotService      | CurrentVehicleId |
| 3    | Si no hay vehículo             | Ask                 | ¿Cuál vehículo?  |
| 4    | Preguntar fecha preferida      | Bot                 | Quick replies    |
| 5    | Usuario selecciona             | Frontend            | Fecha/hora       |
| 6    | Verificar disponibilidad       | AppointmentService  | Check slots      |
| 7    | Si no disponible               | Suggest             | Alternativas     |
| 8    | Solicitar datos contacto       | Bot                 | Nombre, teléfono |
| 9    | Crear cita                     | AppointmentService  | POST             |
| 10   | Crear/actualizar lead          | LeadService         | +20 puntos       |
| 11   | Confirmar por WhatsApp         | NotificationService | SMS/WhatsApp     |
| 12   | Notificar al dealer            | NotificationService | Push + email     |

#### Conversación de Ejemplo

```
Usuario: "Quiero probar el Toyota RAV4 que me mostraste"

Bot: "¡Excelente elección! 🚗 Agendemos tu test drive del Toyota RAV4 XLE 2024.

¿Qué día te queda mejor?"

[Mañana] [Próxima semana] [Ver calendario]

Usuario: [Mañana]

Bot: "Perfecto. Tenemos estos horarios disponibles para mañana:

• 10:00 AM
• 11:30 AM
• 2:00 PM
• 4:30 PM

¿Cuál prefieres?"

Usuario: "2:00 PM"

Bot: "Listo. Para confirmar tu cita, necesito tu nombre completo y teléfono de contacto."

Usuario: "Juan Pérez, 829-555-0100"

Bot: "✅ ¡Tu test drive está confirmado!

📅 Mañana, 22 de enero a las 2:00 PM
🚗 Toyota RAV4 XLE 2024
📍 Autos del Caribe - Av. Churchill #75

Te enviaremos un recordatorio por WhatsApp. ¡Nos vemos pronto!"
```

---

### 4.3 CHAT-003: Escalar a Agente Humano

| Campo       | Valor                                |
| ----------- | ------------------------------------ |
| **ID**      | CHAT-003                             |
| **Nombre**  | Escalar Conversación a Humano        |
| **Actor**   | Sistema/Usuario                      |
| **Trigger** | Intent HumanRequest o baja confianza |

#### Condiciones de Escalamiento

| Condición                       | Umbral                             |
| ------------------------------- | ---------------------------------- |
| Usuario pide humano             | Intent: HumanRequest               |
| Confianza muy baja              | < 0.4 por 3 mensajes               |
| Queja/Reclamo                   | Intent: ComplaintFeedback          |
| Negociación de precio           | Intent: PriceInquiry + negotiation |
| Sin resolver en 10 intercambios | MessageCount > 10                  |

#### Flujo del Proceso

| Paso | Acción                             | Sistema             | Validación          |
| ---- | ---------------------------------- | ------------------- | ------------------- |
| 1    | Detectar condición de escalamiento | ChatbotService      | Reglas              |
| 2    | Actualizar status                  | Database            | Escalated           |
| 3    | Seleccionar agente disponible      | ChatbotService      | Round-robin         |
| 4    | Notificar al agente                | NotificationService | Push + sound        |
| 5    | Informar al usuario                | Bot                 | "Te conecto con..." |
| 6    | Preparar contexto                  | ChatbotService      | Resumen para agente |
| 7    | Transferir conversación            | ChatbotService      | Status = WithHuman  |
| 8    | Registrar tiempo de espera         | Metrics             | Para SLA            |

#### Mensaje de Escalamiento

```
Bot: "Entiendo que necesitas hablar con uno de nuestros asesores.
Te conecto ahora mismo con un especialista.

⏳ Tiempo de espera estimado: 2 minutos

Mientras esperas, tu asesor ya tiene el contexto de nuestra conversación
para ayudarte mejor."

---

[2 minutos después]

Agente: "Hola Juan, soy María de Autos del Caribe.
Vi que te interesa el Toyota RAV4 2024. ¿En qué puedo ayudarte?"
```

---

### 4.4 CHAT-004: Calificar Lead Automáticamente

| Campo       | Valor                           |
| ----------- | ------------------------------- |
| **ID**      | CHAT-004                        |
| **Nombre**  | Calificación Automática de Lead |
| **Actor**   | Sistema                         |
| **Trigger** | Durante conversación            |

#### Señales de Calificación

| Señal                              | Puntos | Detección                             |
| ---------------------------------- | ------ | ------------------------------------- |
| Pregunta sobre vehículo específico | +5     | Entity: vehicle_id                    |
| Pregunta sobre precio              | +8     | Intent: PriceInquiry                  |
| Pregunta sobre financiamiento      | +10    | Intent: FinancingInquiry              |
| Solicita test drive                | +20    | Intent: TestDriveRequest              |
| Proporciona teléfono               | +15    | Entity: phone                         |
| Menciona urgencia                  | +10    | Entity: urgency (pronto, esta semana) |
| Compara con otro vehículo          | +5     | Intent: comparison                    |
| Pregunta documentos/traspaso       | +12    | Intent: DocumentsInquiry              |

#### Flujo del Proceso

| Paso | Acción                  | Sistema            | Validación        |
| ---- | ----------------------- | ------------------ | ----------------- |
| 1    | Por cada mensaje        | ChatbotService     | Analizar          |
| 2    | Detectar señales        | NLU                | Entidades/intents |
| 3    | Calcular puntos         | ChatbotService     | Por señal         |
| 4    | Actualizar lead score   | LeadScoringService | Agregar puntos    |
| 5    | Si no existe lead       | Check              | Crear lead        |
| 6    | Si Hot Lead             | Check              | Notificar dealer  |
| 7    | Guardar en conversación | Database           | LeadScore         |

---

## 5. Integración WhatsApp Business

### 5.1 Configuración

```json
{
  "WhatsApp": {
    "BusinessAccountId": "${WHATSAPP_BUSINESS_ID}",
    "PhoneNumberId": "${WHATSAPP_PHONE_ID}",
    "AccessToken": "${WHATSAPP_ACCESS_TOKEN}",
    "WebhookVerifyToken": "${WHATSAPP_WEBHOOK_TOKEN}",
    "TemplateNamespace": "okla_templates"
  }
}
```

### 5.2 Templates Aprobados

| Template                  | Uso                 |
| ------------------------- | ------------------- |
| `welcome_message`         | Primer contacto     |
| `test_drive_confirmation` | Confirmar cita      |
| `test_drive_reminder`     | Recordatorio 24h    |
| `vehicle_available`       | Vehículo disponible |
| `price_update`            | Cambio de precio    |

### 5.3 Webhook Handler

```csharp
[HttpPost("webhook/whatsapp")]
public async Task<IActionResult> HandleWhatsAppWebhook([FromBody] WhatsAppWebhookPayload payload)
{
    // Verificar firma
    if (!VerifyWebhookSignature(Request.Headers["X-Hub-Signature-256"], payload))
        return Unauthorized();

    foreach (var entry in payload.Entry)
    {
        foreach (var change in entry.Changes)
        {
            if (change.Value.Messages != null)
            {
                foreach (var message in change.Value.Messages)
                {
                    await ProcessIncomingMessage(new ChatMessage
                    {
                        Channel = ChatChannel.WhatsApp,
                        PhoneNumber = message.From,
                        Content = message.Text?.Body,
                        AttachmentUrl = message.Image?.Id
                    });
                }
            }
        }
    }

    return Ok();
}
```

---

## 6. Prompt de Sistema (GPT-4)

```
Eres un asistente virtual de OKLA, el marketplace de vehículos #1 en República Dominicana.

CONTEXTO:
- Ayudas a compradores a encontrar vehículos
- Puedes buscar en el inventario, mostrar detalles, agendar test drives
- Siempre eres amable, profesional y eficiente
- Respondes en español dominicano pero profesional
- Usas emojis con moderación para ser amigable

REGLAS:
1. Si no entiendes algo, pide clarificación amablemente
2. Nunca inventes información sobre vehículos
3. Si el cliente quiere negociar precio, escala a humano
4. Si detectas frustración, ofrece hablar con un asesor
5. Siempre ofrece el siguiente paso (test drive, más info, contactar)

DATOS DISPONIBLES:
- Inventario de vehículos con precios, especificaciones, fotos
- Horarios de dealers (8AM-6PM L-S)
- Ubicaciones de dealers
- Disponibilidad para test drives

FORMATO DE RESPUESTA:
Responde en JSON con la siguiente estructura:
{
  "intent": "intent_detectado",
  "confidence": 0.0-1.0,
  "entities": {...},
  "response": "texto de respuesta",
  "actions": ["acción1", "acción2"],
  "quickReplies": ["opción1", "opción2"]
}
```

---

## 7. Métricas

### 7.1 Prometheus

```
# Conversaciones
chatbot_conversations_total{channel="whatsapp|inapp", status="..."}
chatbot_messages_total{sender="user|bot|human"}
chatbot_escalations_total{reason="..."}

# NLU
chatbot_intent_detections_total{intent="..."}
chatbot_intent_confidence_histogram{intent="...", le="0.4|0.6|0.8|1.0"}

# Performance
chatbot_response_time_seconds
chatbot_openai_latency_seconds

# Leads
chatbot_leads_created_total
chatbot_leads_qualified_total{category="cold|warm|hot"}

# Satisfaction
chatbot_satisfaction_score{rating="1|2|3|4|5"}
```

---

## 8. Configuración

```json
{
  "Chatbot": {
    "DefaultChannel": "InApp",
    "SessionTimeoutMinutes": 30,
    "MaxMessagesBeforeEscalation": 10,
    "MinConfidenceThreshold": 0.4,
    "MaxResponseTimeSeconds": 5
  },
  "OpenAI": {
    "Model": "gpt-4-turbo-preview",
    "Temperature": 0.7,
    "MaxTokens": 500
  },
  "LeadScoring": {
    "VehicleInquiry": 5,
    "PriceInquiry": 8,
    "FinancingInquiry": 10,
    "TestDriveRequest": 20,
    "PhoneProvided": 15
  }
}
```

---

## 📚 Referencias

- [02-lead-service.md](02-lead-service.md) - Gestión de leads
- [03-lead-scoring.md](03-lead-scoring.md) - Scoring de leads
- [05-appointment-service.md](05-appointment-service.md) - Test drives
- [WhatsApp Business API](https://developers.facebook.com/docs/whatsapp/cloud-api)
