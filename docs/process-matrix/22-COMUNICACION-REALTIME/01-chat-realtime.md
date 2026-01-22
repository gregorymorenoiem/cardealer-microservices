# 💬 Chat en Tiempo Real

> **Código:** CHAT-001  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🔴 ALTA (Conversión)  
> **Origen:** Cars.com, WhatsApp Business

---

## 📋 Información General

| Campo             | Valor                                                 |
| ----------------- | ----------------------------------------------------- |
| **Servicio**      | ChatService (NUEVO)                                   |
| **Puerto**        | 5093                                                  |
| **Base de Datos** | `chatservice`                                         |
| **Tecnología**    | SignalR (WebSocket)                                   |
| **Dependencias**  | UserService, VehiclesSaleService, NotificationService |

---

## 🎯 Objetivo del Proceso

1. **Inmediatez:** Respuestas en tiempo real = mayor conversión
2. **Engagement:** Usuario no sale del sitio para comunicarse
3. **Tracking:** Registro de todas las conversaciones
4. **Leads:** Cada chat es un lead calificado

---

## 📡 Endpoints

### REST API

| Método | Endpoint                                | Descripción               | Auth |
| ------ | --------------------------------------- | ------------------------- | ---- |
| `GET`  | `/api/chat/conversations`               | Mis conversaciones        | ✅   |
| `GET`  | `/api/chat/conversations/{id}`          | Detalle de conversación   | ✅   |
| `POST` | `/api/chat/conversations`               | Iniciar conversación      | ✅   |
| `GET`  | `/api/chat/conversations/{id}/messages` | Mensajes (paginado)       | ✅   |
| `POST` | `/api/chat/conversations/{id}/messages` | Enviar mensaje (fallback) | ✅   |
| `PUT`  | `/api/chat/conversations/{id}/read`     | Marcar como leído         | ✅   |
| `POST` | `/api/chat/conversations/{id}/archive`  | Archivar                  | ✅   |
| `GET`  | `/api/chat/unread-count`                | Contador de no leídos     | ✅   |

### SignalR Hub

| Método              | Dirección       | Descripción                |
| ------------------- | --------------- | -------------------------- |
| `JoinConversation`  | Client → Server | Unirse a sala de chat      |
| `LeaveConversation` | Client → Server | Salir de sala              |
| `SendMessage`       | Client → Server | Enviar mensaje             |
| `ReceiveMessage`    | Server → Client | Recibir mensaje            |
| `UserTyping`        | Client → Server | Indicador "escribiendo..." |
| `TypingIndicator`   | Server → Client | Mostrar "escribiendo..."   |
| `MessageRead`       | Server → Client | Confirmación de lectura    |
| `UserOnline`        | Server → Client | Usuario conectado          |
| `UserOffline`       | Server → Client | Usuario desconectado       |

---

## 🗃️ Entidades

### Conversation

```csharp
public class Conversation
{
    public Guid Id { get; set; }

    // Participantes
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }

    // Contexto
    public Guid? VehicleId { get; set; }
    public string VehicleTitle { get; set; }
    public string VehicleImage { get; set; }
    public decimal? VehiclePrice { get; set; }

    // Estado
    public ConversationStatus Status { get; set; }
    public DateTime LastMessageAt { get; set; }
    public int TotalMessages { get; set; }

    // Unread counts
    public int BuyerUnreadCount { get; set; }
    public int SellerUnreadCount { get; set; }

    // Lead info
    public LeadQuality? LeadQuality { get; set; }
    public ConversationOutcome? Outcome { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
}

public enum ConversationStatus
{
    Active,
    Archived,
    Blocked
}

public enum LeadQuality
{
    Hot,      // Respondió rápido, hace preguntas específicas
    Warm,     // Interesado pero no urgente
    Cold      // Solo curiosidad
}

public enum ConversationOutcome
{
    Pending,
    TestDriveScheduled,
    OfferMade,
    Purchased,
    NotInterested,
    VehicleSold
}
```

### ChatMessage

```csharp
public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }

    // Sender
    public Guid SenderId { get; set; }
    public string SenderName { get; set; }
    public string SenderAvatar { get; set; }

    // Content
    public MessageType Type { get; set; }
    public string Content { get; set; }

    // Attachments
    public List<MessageAttachment> Attachments { get; set; }

    // Quick replies (para bots/templates)
    public List<QuickReply> QuickReplies { get; set; }

    // Status
    public MessageStatus Status { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
}

public enum MessageType
{
    Text,
    Image,
    Document,
    VehicleCard,      // Tarjeta de vehículo embebida
    LocationShare,    // Compartir ubicación del dealer
    QuickReplyPrompt, // Respuestas rápidas sugeridas
    SystemMessage     // Mensaje del sistema
}

public enum MessageStatus
{
    Sending,
    Sent,
    Delivered,
    Read,
    Failed
}

public class MessageAttachment
{
    public string Type { get; set; }  // image, pdf, etc.
    public string Url { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public string ThumbnailUrl { get; set; }
}

public class QuickReply
{
    public string Label { get; set; }
    public string Value { get; set; }
    public string Icon { get; set; }
}
```

### UserPresence

```csharp
public class UserPresence
{
    public Guid UserId { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastSeenAt { get; set; }
    public string CurrentConversationId { get; set; }
    public List<string> ActiveConnections { get; set; }
}
```

---

## 📊 Proceso CHAT-001: Iniciar y Mantener Conversación

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: CHAT-001 - Chat en Tiempo Real                                │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG (comprador)                                   │
│ Sistemas: ChatService, SignalR, NotificationService                    │
│ Tecnología: WebSocket                                                  │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                             | Sistema             | Actor      | Evidencia             | Código     |
| ---- | ------- | ---------------------------------- | ------------------- | ---------- | --------------------- | ---------- |
| 1    | 1.1     | Usuario ve listing de vehículo     | Frontend            | USR-REG    | Listing viewed        | EVD-LOG    |
| 1    | 1.2     | Click "Chatear con vendedor"       | Frontend            | USR-REG    | CTA clicked           | EVD-LOG    |
| 2    | 2.1     | **POST /api/chat/conversations**   | Gateway             | USR-REG    | **Create conv**       | EVD-AUDIT  |
| 2    | 2.2     | Verificar usuario autenticado      | ChatService         | Sistema    | Auth check            | EVD-LOG    |
| 2    | 2.3     | **Crear Conversation**             | ChatService         | Sistema    | **Conv created**      | EVD-AUDIT  |
| 2    | 2.4     | Asociar con VehicleId              | ChatService         | Sistema    | Vehicle linked        | EVD-LOG    |
| 3    | 3.1     | **Conectar WebSocket (SignalR)**   | ChatService         | USR-REG    | **WS connected**      | EVD-LOG    |
| 3    | 3.2     | Invoke: JoinConversation           | SignalR             | USR-REG    | Join room             | EVD-LOG    |
| 3    | 3.3     | **Cargar mensajes previos**        | ChatService         | Sistema    | **Messages loaded**   | EVD-LOG    |
| 4    | 4.1     | **Mostrar chat UI**                | Frontend            | Sistema    | **Chat shown**        | EVD-SCREEN |
| 4    | 4.2     | Mostrar tarjeta del vehículo       | Frontend            | Sistema    | Vehicle card          | EVD-LOG    |
| 4    | 4.3     | Quick replies sugeridas            | Frontend            | Sistema    | Quick replies         | EVD-LOG    |
| 5    | 5.1     | **Usuario escribe mensaje**        | Frontend            | USR-REG    | **Typing**            | EVD-LOG    |
| 5    | 5.2     | Invoke: UserTyping                 | SignalR             | USR-REG    | Typing event          | EVD-LOG    |
| 5    | 5.3     | Server broadcast: TypingIndicator  | SignalR             | Sistema    | Typing shown          | EVD-LOG    |
| 6    | 6.1     | **Usuario envía mensaje**          | Frontend            | USR-REG    | **Send clicked**      | EVD-LOG    |
| 6    | 6.2     | **Invoke: SendMessage**            | SignalR             | USR-REG    | **Message sent**      | EVD-AUDIT  |
| 6    | 6.3     | Validar contenido                  | ChatService         | Sistema    | Validation            | EVD-LOG    |
| 6    | 6.4     | **Guardar ChatMessage**            | ChatService         | Sistema    | **Message saved**     | EVD-AUDIT  |
| 6    | 6.5     | **Broadcast: ReceiveMessage**      | SignalR             | Sistema    | **Message delivered** | EVD-LOG    |
| 7    | 7.1     | Si vendedor online: Recibe mensaje | Frontend            | USR-SELLER | Message shown         | EVD-LOG    |
| 7    | 7.2     | Si vendedor offline:               | Sistema             | Sistema    | Offline handling      | EVD-LOG    |
| 7    | 7.3     | **Push notification**              | NotificationService | SYS-NOTIF  | **Push sent**         | EVD-COMM   |
| 7    | 7.4     | **Email notification**             | NotificationService | SYS-NOTIF  | **Email sent**        | EVD-COMM   |
| 8    | 8.1     | Vendedor responde                  | Frontend            | USR-SELLER | Reply                 | EVD-LOG    |
| 8    | 8.2     | Mismo flujo de mensaje             | SignalR             | Sistema    | Message flow          | EVD-LOG    |
| 9    | 9.1     | Mensaje marcado como leído         | SignalR             | Sistema    | Read receipt          | EVD-LOG    |
| 9    | 9.2     | **Broadcast: MessageRead**         | SignalR             | Sistema    | **Read shown**        | EVD-LOG    |
| 10   | 10.1    | **Actualizar contadores unread**   | ChatService         | Sistema    | **Counts updated**    | EVD-LOG    |
| 11   | 11.1    | Usuario cierra chat                | Frontend            | USR-REG    | Chat closed           | EVD-LOG    |
| 11   | 11.2    | Invoke: LeaveConversation          | SignalR             | USR-REG    | Leave room            | EVD-LOG    |
| 11   | 11.3    | **WebSocket desconectado**         | SignalR             | Sistema    | **WS disconnected**   | EVD-LOG    |
| 12   | 12.1    | **Actualizar LastMessageAt**       | ChatService         | Sistema    | **Timestamp updated** | EVD-LOG    |
| 13   | 13.1    | **Audit trail**                    | AuditService        | Sistema    | Complete audit        | EVD-AUDIT  |

### Evidencia de Conversación

```json
{
  "processCode": "CHAT-001",
  "conversation": {
    "id": "conv-12345",
    "participants": {
      "buyer": {
        "id": "user-001",
        "name": "Juan Pérez",
        "avatar": "cdn.okla.com.do/avatars/user-001.jpg",
        "isOnline": true
      },
      "seller": {
        "id": "dealer-001",
        "name": "AutoMax RD",
        "avatar": "cdn.okla.com.do/avatars/dealer-001.jpg",
        "isOnline": false,
        "lastSeenAt": "2026-01-21T09:30:00Z"
      }
    },
    "vehicle": {
      "id": "veh-67890",
      "title": "Toyota Corolla 2023",
      "image": "cdn.okla.com.do/vehicles/veh-67890/main.jpg",
      "price": 1250000
    },
    "stats": {
      "totalMessages": 8,
      "buyerUnread": 0,
      "sellerUnread": 2
    },
    "messages": [
      {
        "id": "msg-001",
        "senderId": "user-001",
        "senderName": "Juan Pérez",
        "type": "Text",
        "content": "Hola, me interesa el Corolla. ¿Está disponible para test drive?",
        "status": "Read",
        "createdAt": "2026-01-21T10:00:00Z",
        "readAt": "2026-01-21T10:02:00Z"
      },
      {
        "id": "msg-002",
        "senderId": "dealer-001",
        "senderName": "AutoMax RD",
        "type": "Text",
        "content": "¡Hola Juan! Sí, está disponible. ¿Cuándo te gustaría venir?",
        "status": "Read",
        "createdAt": "2026-01-21T10:02:30Z"
      },
      {
        "id": "msg-003",
        "senderId": "dealer-001",
        "senderName": "AutoMax RD",
        "type": "QuickReplyPrompt",
        "content": "Selecciona un horario:",
        "quickReplies": [
          { "label": "Mañana 10am", "value": "tomorrow_10am" },
          { "label": "Mañana 2pm", "value": "tomorrow_2pm" },
          { "label": "Otro día", "value": "other" }
        ],
        "status": "Delivered",
        "createdAt": "2026-01-21T10:02:35Z"
      }
    ],
    "status": "Active",
    "leadQuality": "Hot",
    "outcome": "Pending",
    "createdAt": "2026-01-21T10:00:00Z",
    "lastMessageAt": "2026-01-21T10:02:35Z"
  }
}
```

---

## 📱 UI Mockup - Chat Window

```
┌─────────────────────────────────────────────────────────────────────────┐
│ ← Mensajes            AutoMax RD                         🟢 En línea   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  🚗 Toyota Corolla 2023                          RD$ 1,250,000 │   │
│  │  25,000 km · Santo Domingo                        [Ver Listing] │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ─────────────── 21 de Enero, 2026 ───────────────                     │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────┐           │
│  │  Hola, me interesa el Corolla.                         │ 10:00 ✓✓  │
│  │  ¿Está disponible para test drive?                     │           │
│  └─────────────────────────────────────────────────────────┘           │
│                                                                         │
│           ┌─────────────────────────────────────────────────────────┐  │
│  10:02    │  ¡Hola Juan! Sí, está disponible.                      │  │
│           │  ¿Cuándo te gustaría venir?                            │  │
│           └─────────────────────────────────────────────────────────┘  │
│                                                                         │
│           ┌─────────────────────────────────────────────────────────┐  │
│  10:02    │  Selecciona un horario:                                │  │
│           │                                                         │  │
│           │  ┌───────────┐ ┌───────────┐ ┌───────────┐             │  │
│           │  │Mañana 10am│ │Mañana 2pm │ │ Otro día  │             │  │
│           │  └───────────┘ └───────────┘ └───────────┘             │  │
│           └─────────────────────────────────────────────────────────┘  │
│                                                                         │
│                                                                         │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  Escribe un mensaje...                            📎  😊  ➤    │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📱 UI Mockup - Lista de Conversaciones

```
┌─────────────────────────────────────────────────────────────────────────┐
│ ← Perfil                    Mensajes                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  🔍 Buscar conversaciones...                                           │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                                                                 │   │
│  │  [Avatar]  AutoMax RD                              hace 2 min  │   │
│  │            🚗 Toyota Corolla 2023                              │   │
│  │            Selecciona un horario:                    🔵 2     │   │
│  │                                                                 │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                                                                 │   │
│  │  [Avatar]  CarMax Santo Domingo                       hace 1h  │   │
│  │            🚗 Honda Civic 2022                                 │   │
│  │            ✓✓ Perfecto, te espero mañana                       │   │
│  │                                                                 │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                                                                 │   │
│  │  [Avatar]  Vehículos Premium RD                       hace 3d  │   │
│  │            🚗 BMW X5 2021                                      │   │
│  │            ✓ Gracias por tu interés                            │   │
│  │                                                                 │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Métricas Prometheus

```yaml
# WebSocket
websocket_connections_active
websocket_messages_sent_total
websocket_messages_received_total
websocket_connection_duration_seconds

# Conversaciones
conversations_created_total
conversations_active_total
conversations_by_outcome{outcome}

# Mensajes
messages_sent_total
messages_delivery_time_ms
messages_read_time_avg_seconds

# Engagement
response_time_avg_seconds
first_response_time_avg_seconds
messages_per_conversation_avg

# Leads
chat_to_testdrive_rate
chat_to_purchase_rate
lead_quality_distribution{quality}
```

---

## 🔧 Configuración SignalR

```csharp
// Program.cs
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Para escalar: Redis backplane
builder.Services.AddSignalR()
    .AddStackExchangeRedis("redis:6379", options =>
    {
        options.Configuration.ChannelPrefix = "okla-chat";
    });

// Hub
app.MapHub<ChatHub>("/hubs/chat");
```

---

## 🔗 Referencias

- [00-ANALISIS-COMPETITIVO.md](../00-ANALISIS-COMPETITIVO.md)
- [06-CRM-LEADS-CONTACTOS/01-lead-service.md](../06-CRM-LEADS-CONTACTOS/01-lead-service.md)
- [07-NOTIFICACIONES/01-notification-service.md](../07-NOTIFICACIONES/01-notification-service.md)
