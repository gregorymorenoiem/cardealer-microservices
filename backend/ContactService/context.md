# ContactService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** ContactService
- **Puerto en Desarrollo:** 5026
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`contactservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de gestión de contactos y mensajería entre compradores/inquilinos y vendedores/agentes. Facilita comunicación sin exponer información de contacto directa hasta que sea necesario.

---

## 🏗️ ARQUITECTURA

```
ContactService/
├── ContactService.Api/
│   ├── Controllers/
│   │   ├── MessagesController.cs
│   │   ├── ConversationsController.cs
│   │   └── InquiriesController.cs
│   └── Program.cs
├── ContactService.Application/
├── ContactService.Domain/
│   ├── Entities/
│   │   ├── Inquiry.cs
│   │   ├── Conversation.cs
│   │   └── Message.cs
│   └── Enums/
│       ├── InquiryType.cs
│       └── MessageStatus.cs
└── ContactService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### Inquiry
```csharp
public class Inquiry
{
    public Guid Id { get; set; }
    public string InquiryNumber { get; set; }      // INQ-2026-001234
    
    // Contexto
    public InquiryType Type { get; set; }          // GeneralQuestion, PriceNegotiation, MoreInfo, ScheduleViewing
    public Guid? RelatedEntityId { get; set; }     // VehicleId o PropertyId
    public string? RelatedEntityType { get; set; } // "Vehicle", "Property"
    public string? RelatedEntityTitle { get; set; }
    
    // Remitente (potencial comprador)
    public Guid? SenderId { get; set; }            // null si no autenticado
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string SenderPhone { get; set; }
    
    // Destinatario (vendedor/agente)
    public Guid RecipientId { get; set; }
    public string RecipientName { get; set; }
    
    // Contenido
    public string Subject { get; set; }
    public string Message { get; set; }
    
    // Estado
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool HasResponse { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Lead tracking
    public string? Source { get; set; }            // "Website", "Mobile App", "Email"
    public bool IsConverted { get; set; }          // ¿Se convirtió en venta?
}
```

### Conversation
```csharp
public class Conversation
{
    public Guid Id { get; set; }
    public string ConversationNumber { get; set; }
    
    // Participantes
    public Guid User1Id { get; set; }
    public string User1Name { get; set; }
    public Guid User2Id { get; set; }
    public string User2Name { get; set; }
    
    // Contexto
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityTitle { get; set; }
    
    // Estado
    public bool IsActive { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public Guid? LastMessageById { get; set; }
    public int UnreadCountUser1 { get; set; }
    public int UnreadCountUser2 { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    
    // Navegación
    public ICollection<Message> Messages { get; set; }
}
```

### Message
```csharp
public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    
    // Remitente
    public Guid SenderId { get; set; }
    public string SenderName { get; set; }
    
    // Contenido
    public string Content { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    
    // Estado
    public MessageStatus Status { get; set; }      // Sent, Delivered, Read
    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    
    // Tipo
    public bool IsSystemMessage { get; set; }      // Mensajes automáticos del sistema
    
    // Navegación
    public Conversation Conversation { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Inquiries (Formulario de contacto inicial)
- `POST /api/inquiries` - Enviar consulta
  ```json
  {
    "type": "MoreInfo",
    "relatedEntityId": "vehicle-uuid",
    "relatedEntityType": "Vehicle",
    "senderName": "Juan Pérez",
    "senderEmail": "juan@example.com",
    "senderPhone": "809-555-1234",
    "subject": "Consulta sobre Toyota Corolla 2020",
    "message": "Hola, me interesa este vehículo. ¿Está disponible para prueba de manejo?"
  }
  ```
- `GET /api/inquiries/{id}` - Ver inquiry
- `GET /api/inquiries/received` - Inquiries recibidos (para vendedores)
- `PUT /api/inquiries/{id}/mark-read` - Marcar como leído

### Conversations (Chat continuo)
- `GET /api/conversations` - Listar conversaciones del usuario
- `GET /api/conversations/{id}` - Detalle de conversación con mensajes
- `POST /api/conversations` - Crear nueva conversación
- `PUT /api/conversations/{id}/archive` - Archivar conversación

### Messages
- `POST /api/messages` - Enviar mensaje
  ```json
  {
    "conversationId": "conversation-uuid",
    "content": "Sí, está disponible. ¿Cuándo te gustaría verlo?",
    "attachmentUrl": null
  }
  ```
- `GET /api/conversations/{conversationId}/messages` - Listar mensajes
- `PUT /api/messages/{id}/mark-read` - Marcar mensaje como leído
- `GET /api/messages/unread-count` - Contador de mensajes no leídos

---

## 💡 FUNCIONALIDADES PLANEADAS

### Mensajería en Tiempo Real
- WebSocket / SignalR para chat en vivo
- Notificación push cuando llega mensaje nuevo
- Indicador de "escribiendo..."

### Protección de Privacidad
- No exponer email/teléfono hasta que vendedor responda
- Números de teléfono enmascarados inicialmente
- Sistema de reportar spam/abuso

### Templates de Respuesta Rápida
Vendedores pueden crear respuestas predefinidas:
- "Gracias por tu interés, el vehículo sigue disponible"
- "El precio es negociable, ¿cuál es tu oferta?"
- "Puedo programar una cita para mañana a las 2pm"

### Adjuntos
- Permitir enviar fotos adicionales
- PDFs (documentos del vehículo, carfax, etc.)
- Máximo 5MB por archivo

### Auto-respuestas
- Cuando vendedor está offline: "Gracias por contactar. Responderé en las próximas 24h"
- Fuera de horario: "Recibiré tu mensaje mañana a las 9am"

### Moderación
- Filtro de palabras inapropiadas
- Detección de números de teléfono en mensajes iniciales
- Bloqueo de spam (múltiples inquiries idénticas)

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### VehiclesSaleService / PropertiesSaleService
- Botón "Contactar Vendedor" en listing
- Pre-llenar contexto del vehículo/propiedad

### UserService
- Obtener info del usuario autenticado
- Historial de conversaciones

### NotificationService
- Email cuando llega inquiry nuevo
- Push notification cuando llega mensaje
- Resumen diario de inquiries pendientes

### CRMService
- Registrar lead desde inquiry
- Tracking de conversión (inquiry → appointment → sale)

### AuditService
- Log de todos los mensajes (compliance)
- Reportes de spam/abuso

---

## 🎯 BUSINESS RULES

### Rate Limiting
- Máximo 10 inquiries por usuario por día
- Máximo 50 mensajes por conversación por hora
- Cooldown de 5 min entre inquiries idénticos

### Archivado Automático
- Conversaciones sin actividad por 30 días → archivar
- Inquiries sin respuesta por 7 días → marcar como "Expired"

### Spam Detection
- Si usuario envía mismo mensaje a > 5 vendedores en < 1h → flag como spam
- Si contiene múltiples URLs → requiere revisión

---

## 🔄 EVENTOS PUBLICADOS (RabbitMQ)

### InquiryCreated
```json
{
  "inquiryId": "uuid",
  "recipientId": "uuid",
  "relatedEntityType": "Vehicle",
  "relatedEntityId": "uuid",
  "timestamp": "2026-01-07T10:30:00Z"
}
```
→ Dispara notificación al vendedor

### MessageSent
```json
{
  "messageId": "uuid",
  "conversationId": "uuid",
  "senderId": "uuid",
  "recipientId": "uuid",
  "timestamp": "2026-01-07T10:31:00Z"
}
```
→ Dispara notificación push/email

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0
