# 💬 WhatsApp Business API

**Proveedor:** Meta (Facebook)  
**Website:** [business.whatsapp.com](https://business.whatsapp.com)  
**Uso:** Comunicación con compradores/vendedores  
**Prioridad:** ⭐⭐⭐⭐⭐ CRÍTICA (Canal #1 en RD)

---

## 📋 Información General

| Campo | Valor |
|-------|-------|
| **API** | WhatsApp Business Cloud API |
| **Proveedor** | Meta for Developers |
| **Costo Base** | $0.0549/mensaje (RD conversaciones) |
| **Primeras 1000** | GRATIS (por mes) |

---

## 📊 Tipos de Mensajes

| Tipo | Descripción | Costo | Ventana |
|------|-------------|-------|---------|
| **Marketing** | Promociones, ofertas | $0.0549 | 24h después de opt-in |
| **Utility** | Confirmaciones, recordatorios | $0.0376 | 24h |
| **Authentication** | OTPs, verificación | $0.0255 | N/A |
| **Service** | Respuestas al usuario | GRATIS | 24h desde mensaje |

---

## 🌐 API Endpoints

```http
# Base URL
https://graph.facebook.com/v18.0/{PHONE_NUMBER_ID}/messages

# Enviar Mensaje de Texto
POST /messages
Authorization: Bearer {ACCESS_TOKEN}
Content-Type: application/json

{
  "messaging_product": "whatsapp",
  "recipient_type": "individual",
  "to": "18091234567",
  "type": "text",
  "text": {
    "preview_url": false,
    "body": "¡Hola! Gracias por tu interés en el Toyota Corolla 2024. ¿Tienes alguna pregunta?"
  }
}

# Response
{
  "messaging_product": "whatsapp",
  "contacts": [{
    "input": "18091234567",
    "wa_id": "18091234567"
  }],
  "messages": [{
    "id": "wamid.HBgLMTgwOTEyMzQ1NjcVAgARGBI5QUNDQU..."
  }]
}

# Enviar Template (Para iniciar conversación)
POST /messages
{
  "messaging_product": "whatsapp",
  "to": "18091234567",
  "type": "template",
  "template": {
    "name": "vehicle_inquiry_response",
    "language": { "code": "es" },
    "components": [
      {
        "type": "header",
        "parameters": [
          {
            "type": "image",
            "image": {
              "link": "https://okla.com.do/images/vehicle-123.jpg"
            }
          }
        ]
      },
      {
        "type": "body",
        "parameters": [
          { "type": "text", "text": "Juan" },
          { "type": "text", "text": "Toyota Corolla 2024" },
          { "type": "text", "text": "RD$ 1,500,000" }
        ]
      },
      {
        "type": "button",
        "sub_type": "url",
        "index": "0",
        "parameters": [
          { "type": "text", "text": "vehicle-123" }
        ]
      }
    ]
  }
}

# Enviar Mensaje con Botones Interactivos
POST /messages
{
  "messaging_product": "whatsapp",
  "to": "18091234567",
  "type": "interactive",
  "interactive": {
    "type": "button",
    "header": {
      "type": "image",
      "image": {
        "link": "https://okla.com.do/images/vehicle-123.jpg"
      }
    },
    "body": {
      "text": "¡Hola! El Toyota Corolla 2024 está disponible.\n\n💰 Precio: RD$ 1,500,000\n📍 Ubicación: Santiago\n\n¿Qué te gustaría hacer?"
    },
    "action": {
      "buttons": [
        { "type": "reply", "reply": { "id": "schedule_visit", "title": "📅 Agendar Visita" } },
        { "type": "reply", "reply": { "id": "request_financing", "title": "💳 Ver Financiamiento" } },
        { "type": "reply", "reply": { "id": "more_info", "title": "ℹ️ Más Información" } }
      ]
    }
  }
}

# Enviar Lista Interactiva
POST /messages
{
  "messaging_product": "whatsapp",
  "to": "18091234567",
  "type": "interactive",
  "interactive": {
    "type": "list",
    "body": {
      "text": "Tenemos varias opciones que pueden interesarte:"
    },
    "action": {
      "button": "Ver Opciones",
      "sections": [
        {
          "title": "Vehículos Similares",
          "rows": [
            { "id": "v-001", "title": "Toyota Corolla 2024", "description": "RD$ 1,500,000 - Blanco" },
            { "id": "v-002", "title": "Honda Civic 2024", "description": "RD$ 1,450,000 - Gris" },
            { "id": "v-003", "title": "Hyundai Elantra 2024", "description": "RD$ 1,200,000 - Azul" }
          ]
        }
      ]
    }
  }
}

# Webhook para recibir mensajes
POST /webhook
{
  "object": "whatsapp_business_account",
  "entry": [{
    "id": "WHATSAPP_BUSINESS_ACCOUNT_ID",
    "changes": [{
      "value": {
        "messaging_product": "whatsapp",
        "messages": [{
          "from": "18091234567",
          "id": "wamid.xxx",
          "timestamp": "1706100000",
          "type": "text",
          "text": {
            "body": "Hola, me interesa el Toyota Corolla"
          }
        }]
      },
      "field": "messages"
    }]
  }]
}
```

---

## 💻 Modelos C#

```csharp
namespace CommunicationService.Domain.Entities;

/// <summary>
/// Mensaje de WhatsApp
/// </summary>
public record WhatsAppMessage(
    string MessageId,
    string To,
    string From,
    MessageType Type,
    string? TextBody,
    MessageTemplate? Template,
    InteractiveMessage? Interactive,
    MediaAttachment? Media,
    MessageStatus Status,
    DateTime SentAt,
    DateTime? DeliveredAt,
    DateTime? ReadAt
);

public enum MessageType
{
    Text,
    Template,
    Interactive,
    Image,
    Document,
    Audio,
    Video
}

public enum MessageStatus
{
    Pending,
    Sent,
    Delivered,
    Read,
    Failed
}

/// <summary>
/// Template de mensaje
/// </summary>
public record MessageTemplate(
    string Name,
    string LanguageCode,
    List<TemplateComponent> Components
);

public record TemplateComponent(
    ComponentType Type,
    List<TemplateParameter> Parameters
);

public enum ComponentType
{
    Header,
    Body,
    Button
}

public record TemplateParameter(
    ParameterType Type,
    string? Text,
    string? ImageUrl,
    string? DocumentUrl
);

public enum ParameterType
{
    Text,
    Image,
    Document,
    Currency,
    DateTime
}

/// <summary>
/// Mensaje interactivo
/// </summary>
public record InteractiveMessage(
    InteractiveType Type,
    MessageHeader? Header,
    string BodyText,
    List<InteractiveButton>? Buttons,
    List<ListSection>? Sections
);

public enum InteractiveType
{
    Button,
    List,
    Product,
    ProductList
}

public record InteractiveButton(
    string Id,
    string Title
);

public record ListSection(
    string Title,
    List<ListRow> Rows
);

public record ListRow(
    string Id,
    string Title,
    string? Description
);

/// <summary>
/// Conversación con contexto
/// </summary>
public record WhatsAppConversation(
    string ConversationId,
    string PhoneNumber,
    string? CustomerName,
    Guid? UserId,
    List<WhatsAppMessage> Messages,
    ConversationContext Context,
    DateTime StartedAt,
    DateTime LastMessageAt,
    bool IsActive
);

public record ConversationContext(
    Guid? VehicleId,
    string? VehicleTitle,
    Guid? DealerId,
    string? DealerName,
    ConversationIntent? Intent
);

public enum ConversationIntent
{
    VehicleInquiry,
    ScheduleVisit,
    FinancingQuestion,
    PriceNegotiation,
    GeneralSupport
}
```

---

## 🔧 Service Interface

```csharp
namespace CommunicationService.Domain.Interfaces;

public interface IWhatsAppService
{
    /// <summary>
    /// Envía mensaje de texto simple
    /// </summary>
    Task<WhatsAppMessage> SendTextMessageAsync(
        string to, 
        string text);

    /// <summary>
    /// Envía mensaje con template (para iniciar conversación)
    /// </summary>
    Task<WhatsAppMessage> SendTemplateMessageAsync(
        string to,
        string templateName,
        Dictionary<string, string> parameters,
        string? imageUrl = null);

    /// <summary>
    /// Envía mensaje con botones interactivos
    /// </summary>
    Task<WhatsAppMessage> SendInteractiveButtonsAsync(
        string to,
        string bodyText,
        List<InteractiveButton> buttons,
        string? imageUrl = null);

    /// <summary>
    /// Envía lista interactiva
    /// </summary>
    Task<WhatsAppMessage> SendInteractiveListAsync(
        string to,
        string bodyText,
        string buttonText,
        List<ListSection> sections);

    /// <summary>
    /// Procesa webhook de mensaje entrante
    /// </summary>
    Task ProcessIncomingWebhookAsync(WhatsAppWebhookPayload payload);

    /// <summary>
    /// Obtiene historial de conversación
    /// </summary>
    Task<WhatsAppConversation?> GetConversationAsync(string phoneNumber);

    /// <summary>
    /// Marca mensajes como leídos
    /// </summary>
    Task MarkAsReadAsync(string messageId);
}
```

---

## 🏗️ Service Implementation

```csharp
namespace CommunicationService.Infrastructure.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly IConversationRepository _conversationRepo;
    private readonly IMediator _mediator;

    private readonly string _phoneNumberId;

    public WhatsAppService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<WhatsAppService> logger,
        IConversationRepository conversationRepo,
        IMediator mediator)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _conversationRepo = conversationRepo;
        _mediator = mediator;

        _phoneNumberId = config["WhatsApp:PhoneNumberId"]!;
        _httpClient.BaseAddress = new Uri("https://graph.facebook.com/v18.0/");
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", config["WhatsApp:AccessToken"]);
    }

    public async Task<WhatsAppMessage> SendTextMessageAsync(
        string to, 
        string text)
    {
        var request = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = NormalizePhoneNumber(to),
            type = "text",
            text = new { preview_url = true, body = text }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{_phoneNumberId}/messages", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<WhatsAppApiResponse>();

        var message = new WhatsAppMessage(
            MessageId: result!.Messages[0].Id,
            To: to,
            From: _config["WhatsApp:BusinessNumber"]!,
            Type: MessageType.Text,
            TextBody: text,
            Template: null,
            Interactive: null,
            Media: null,
            Status: MessageStatus.Sent,
            SentAt: DateTime.UtcNow,
            DeliveredAt: null,
            ReadAt: null
        );

        await SaveMessageAsync(message);
        return message;
    }

    public async Task<WhatsAppMessage> SendTemplateMessageAsync(
        string to,
        string templateName,
        Dictionary<string, string> parameters,
        string? imageUrl = null)
    {
        var components = new List<object>();

        // Header con imagen si existe
        if (!string.IsNullOrEmpty(imageUrl))
        {
            components.Add(new
            {
                type = "header",
                parameters = new[] { new { type = "image", image = new { link = imageUrl } } }
            });
        }

        // Body con parámetros
        if (parameters.Any())
        {
            components.Add(new
            {
                type = "body",
                parameters = parameters.Select(p => new { type = "text", text = p.Value })
            });
        }

        var request = new
        {
            messaging_product = "whatsapp",
            to = NormalizePhoneNumber(to),
            type = "template",
            template = new
            {
                name = templateName,
                language = new { code = "es" },
                components
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{_phoneNumberId}/messages", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<WhatsAppApiResponse>();

        var message = new WhatsAppMessage(
            MessageId: result!.Messages[0].Id,
            To: to,
            From: _config["WhatsApp:BusinessNumber"]!,
            Type: MessageType.Template,
            TextBody: null,
            Template: new MessageTemplate(templateName, "es", new()),
            Interactive: null,
            Media: null,
            Status: MessageStatus.Sent,
            SentAt: DateTime.UtcNow,
            DeliveredAt: null,
            ReadAt: null
        );

        await SaveMessageAsync(message);
        return message;
    }

    public async Task<WhatsAppMessage> SendInteractiveButtonsAsync(
        string to,
        string bodyText,
        List<InteractiveButton> buttons,
        string? imageUrl = null)
    {
        var interactive = new
        {
            type = "button",
            header = imageUrl != null ? new { type = "image", image = new { link = imageUrl } } : null,
            body = new { text = bodyText },
            action = new
            {
                buttons = buttons.Take(3).Select(b => new
                {
                    type = "reply",
                    reply = new { id = b.Id, title = b.Title }
                })
            }
        };

        var request = new
        {
            messaging_product = "whatsapp",
            to = NormalizePhoneNumber(to),
            type = "interactive",
            interactive
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{_phoneNumberId}/messages", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<WhatsAppApiResponse>();

        var message = new WhatsAppMessage(
            MessageId: result!.Messages[0].Id,
            To: to,
            From: _config["WhatsApp:BusinessNumber"]!,
            Type: MessageType.Interactive,
            TextBody: bodyText,
            Template: null,
            Interactive: new InteractiveMessage(
                InteractiveType.Button, null, bodyText, buttons, null),
            Media: null,
            Status: MessageStatus.Sent,
            SentAt: DateTime.UtcNow,
            DeliveredAt: null,
            ReadAt: null
        );

        await SaveMessageAsync(message);
        return message;
    }

    public async Task<WhatsAppMessage> SendInteractiveListAsync(
        string to,
        string bodyText,
        string buttonText,
        List<ListSection> sections)
    {
        var request = new
        {
            messaging_product = "whatsapp",
            to = NormalizePhoneNumber(to),
            type = "interactive",
            interactive = new
            {
                type = "list",
                body = new { text = bodyText },
                action = new
                {
                    button = buttonText,
                    sections = sections.Select(s => new
                    {
                        title = s.Title,
                        rows = s.Rows.Select(r => new
                        {
                            id = r.Id,
                            title = r.Title,
                            description = r.Description
                        })
                    })
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{_phoneNumberId}/messages", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<WhatsAppApiResponse>();

        return new WhatsAppMessage(
            MessageId: result!.Messages[0].Id,
            To: to,
            From: _config["WhatsApp:BusinessNumber"]!,
            Type: MessageType.Interactive,
            TextBody: bodyText,
            Template: null,
            Interactive: new InteractiveMessage(
                InteractiveType.List, null, bodyText, null, sections),
            Media: null,
            Status: MessageStatus.Sent,
            SentAt: DateTime.UtcNow,
            DeliveredAt: null,
            ReadAt: null
        );
    }

    public async Task ProcessIncomingWebhookAsync(WhatsAppWebhookPayload payload)
    {
        foreach (var entry in payload.Entry)
        {
            foreach (var change in entry.Changes)
            {
                if (change.Value.Messages != null)
                {
                    foreach (var msg in change.Value.Messages)
                    {
                        await HandleIncomingMessageAsync(msg, change.Value.Contacts?.FirstOrDefault());
                    }
                }

                if (change.Value.Statuses != null)
                {
                    foreach (var status in change.Value.Statuses)
                    {
                        await UpdateMessageStatusAsync(status);
                    }
                }
            }
        }
    }

    private async Task HandleIncomingMessageAsync(
        IncomingMessage msg, 
        ContactInfo? contact)
    {
        _logger.LogInformation(
            "Received message from {From}: {Type}", 
            msg.From, msg.Type);

        // Guardar mensaje entrante
        var message = new WhatsAppMessage(
            MessageId: msg.Id,
            To: _config["WhatsApp:BusinessNumber"]!,
            From: msg.From,
            Type: ParseMessageType(msg.Type),
            TextBody: msg.Text?.Body ?? msg.Interactive?.ButtonReply?.Title,
            Template: null,
            Interactive: null,
            Media: null,
            Status: MessageStatus.Delivered,
            SentAt: DateTimeOffset.FromUnixTimeSeconds(long.Parse(msg.Timestamp)).UtcDateTime,
            DeliveredAt: DateTime.UtcNow,
            ReadAt: null
        );

        await SaveMessageAsync(message);

        // Marcar como leído
        await MarkAsReadAsync(msg.Id);

        // Publicar evento para procesamiento
        await _mediator.Publish(new WhatsAppMessageReceived(
            MessageId: msg.Id,
            From: msg.From,
            CustomerName: contact?.Profile?.Name,
            MessageText: msg.Text?.Body,
            ButtonReplyId: msg.Interactive?.ButtonReply?.Id,
            ListReplyId: msg.Interactive?.ListReply?.Id,
            ReceivedAt: DateTime.UtcNow
        ));
    }

    private async Task UpdateMessageStatusAsync(MessageStatusUpdate status)
    {
        // Actualizar estado del mensaje en DB
        await _conversationRepo.UpdateMessageStatusAsync(
            status.Id,
            ParseStatus(status.Status),
            DateTime.UtcNow
        );
    }

    public async Task<WhatsAppConversation?> GetConversationAsync(string phoneNumber)
    {
        return await _conversationRepo.GetByPhoneNumberAsync(
            NormalizePhoneNumber(phoneNumber));
    }

    public async Task MarkAsReadAsync(string messageId)
    {
        var request = new
        {
            messaging_product = "whatsapp",
            status = "read",
            message_id = messageId
        };

        await _httpClient.PostAsJsonAsync(
            $"{_phoneNumberId}/messages", request);
    }

    private async Task SaveMessageAsync(WhatsAppMessage message)
    {
        await _conversationRepo.AddMessageAsync(message);
    }

    private static string NormalizePhoneNumber(string phone)
    {
        // Asegurar formato internacional sin +
        var cleaned = new string(phone.Where(char.IsDigit).ToArray());
        
        // Si empieza con 809, 829 o 849 sin código país
        if (cleaned.Length == 10 && (cleaned.StartsWith("809") || 
            cleaned.StartsWith("829") || cleaned.StartsWith("849")))
        {
            return "1" + cleaned; // Agregar código país USA/RD
        }

        return cleaned;
    }

    private static MessageType ParseMessageType(string type)
    {
        return type switch
        {
            "text" => MessageType.Text,
            "interactive" => MessageType.Interactive,
            "image" => MessageType.Image,
            "document" => MessageType.Document,
            _ => MessageType.Text
        };
    }

    private static MessageStatus ParseStatus(string status)
    {
        return status switch
        {
            "sent" => MessageStatus.Sent,
            "delivered" => MessageStatus.Delivered,
            "read" => MessageStatus.Read,
            "failed" => MessageStatus.Failed,
            _ => MessageStatus.Pending
        };
    }
}

// DTOs para API
internal record WhatsAppApiResponse(
    List<MessageResponse> Messages
);

internal record MessageResponse(string Id);

// Payload de webhook
public record WhatsAppWebhookPayload(
    string Object,
    List<WebhookEntry> Entry
);

public record WebhookEntry(
    string Id,
    List<WebhookChange> Changes
);

public record WebhookChange(
    WebhookValue Value,
    string Field
);

public record WebhookValue(
    string MessagingProduct,
    List<IncomingMessage>? Messages,
    List<ContactInfo>? Contacts,
    List<MessageStatusUpdate>? Statuses
);

public record IncomingMessage(
    string From,
    string Id,
    string Timestamp,
    string Type,
    TextContent? Text,
    InteractiveContent? Interactive
);

public record TextContent(string Body);

public record InteractiveContent(
    string Type,
    ButtonReply? ButtonReply,
    ListReply? ListReply
);

public record ButtonReply(string Id, string Title);
public record ListReply(string Id, string Title, string? Description);
public record ContactInfo(ProfileInfo? Profile);
public record ProfileInfo(string Name);
public record MessageStatusUpdate(string Id, string Status, string Timestamp);

// Evento MediatR
public record WhatsAppMessageReceived(
    string MessageId,
    string From,
    string? CustomerName,
    string? MessageText,
    string? ButtonReplyId,
    string? ListReplyId,
    DateTime ReceivedAt
) : INotification;
```

---

## ⚛️ React Component

```tsx
// components/WhatsAppChat.tsx
import { useState, useEffect, useRef } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { whatsappService } from '@/services/whatsappService';
import { Send, Phone, Clock, Check, CheckCheck } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import { es } from 'date-fns/locale';

interface Props {
  phoneNumber: string;
  customerName?: string;
  vehicleId?: string;
}

export function WhatsAppChat({ phoneNumber, customerName, vehicleId }: Props) {
  const [message, setMessage] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const queryClient = useQueryClient();

  const conversationQuery = useQuery({
    queryKey: ['whatsapp-conversation', phoneNumber],
    queryFn: () => whatsappService.getConversation(phoneNumber),
    refetchInterval: 5000, // Poll cada 5 segundos
  });

  const sendMutation = useMutation({
    mutationFn: (text: string) => whatsappService.sendMessage(phoneNumber, text),
    onSuccess: () => {
      setMessage('');
      queryClient.invalidateQueries({ queryKey: ['whatsapp-conversation', phoneNumber] });
    },
  });

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [conversationQuery.data?.messages]);

  const handleSend = () => {
    if (message.trim()) {
      sendMutation.mutate(message);
    }
  };

  return (
    <div className="flex flex-col h-[500px] bg-white rounded-xl shadow-lg overflow-hidden">
      {/* Header */}
      <div className="bg-[#075E54] text-white p-4 flex items-center gap-3">
        <div className="w-10 h-10 bg-gray-300 rounded-full flex items-center justify-center">
          <span className="text-gray-600 font-bold">
            {customerName?.[0] || phoneNumber[0]}
          </span>
        </div>
        <div className="flex-1">
          <h3 className="font-medium">{customerName || phoneNumber}</h3>
          <p className="text-xs text-green-200">
            {conversationQuery.data?.isActive ? 'En línea' : 'Última vez hace mucho'}
          </p>
        </div>
        <a href={`tel:${phoneNumber}`} className="p-2">
          <Phone className="w-5 h-5" />
        </a>
      </div>

      {/* Messages */}
      <div 
        className="flex-1 overflow-y-auto p-4 space-y-3"
        style={{ backgroundImage: 'url(/whatsapp-bg.png)' }}
      >
        {conversationQuery.data?.messages.map((msg) => (
          <MessageBubble key={msg.messageId} message={msg} />
        ))}
        <div ref={messagesEndRef} />
      </div>

      {/* Input */}
      <div className="p-4 bg-gray-100 flex gap-2">
        <input
          type="text"
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          onKeyPress={(e) => e.key === 'Enter' && handleSend()}
          placeholder="Escribe un mensaje..."
          className="flex-1 px-4 py-2 rounded-full border"
        />
        <button
          onClick={handleSend}
          disabled={!message.trim() || sendMutation.isPending}
          className="p-2 bg-[#075E54] text-white rounded-full disabled:opacity-50"
        >
          <Send className="w-5 h-5" />
        </button>
      </div>
    </div>
  );
}

function MessageBubble({ message }: { message: WhatsAppMessage }) {
  const isOutgoing = message.from !== message.to;

  return (
    <div className={`flex ${isOutgoing ? 'justify-end' : 'justify-start'}`}>
      <div 
        className={`max-w-[75%] rounded-lg px-3 py-2 ${
          isOutgoing 
            ? 'bg-[#DCF8C6] rounded-tr-none' 
            : 'bg-white rounded-tl-none shadow'
        }`}
      >
        <p className="text-sm">{message.textBody}</p>
        <div className="flex items-center justify-end gap-1 mt-1">
          <span className="text-xs text-gray-500">
            {formatDistanceToNow(new Date(message.sentAt), { 
              addSuffix: true, 
              locale: es 
            })}
          </span>
          {isOutgoing && (
            <MessageStatus status={message.status} />
          )}
        </div>
      </div>
    </div>
  );
}

function MessageStatus({ status }: { status: MessageStatus }) {
  switch (status) {
    case 'sent':
      return <Check className="w-4 h-4 text-gray-400" />;
    case 'delivered':
      return <CheckCheck className="w-4 h-4 text-gray-400" />;
    case 'read':
      return <CheckCheck className="w-4 h-4 text-blue-500" />;
    default:
      return <Clock className="w-4 h-4 text-gray-400" />;
  }
}
```

---

## 📱 Templates Sugeridos

### 1. Respuesta a Consulta de Vehículo

```
Nombre: vehicle_inquiry_response
Idioma: es

Hola {{1}}, gracias por tu interés en el {{2}}.

📍 Precio: {{3}}
📆 Año: {{4}}
🚗 Condición: {{5}}

¿Te gustaría agendar una visita o tienes alguna pregunta?

[Ver Vehículo] [Agendar Visita]
```

### 2. Confirmación de Cita

```
Nombre: appointment_confirmation
Idioma: es

¡Cita confirmada! ✅

📅 Fecha: {{1}}
🕐 Hora: {{2}}
📍 Lugar: {{3}}

Vehículo: {{4}}

Te esperamos. Si necesitas cambiar la cita, responde a este mensaje.
```

### 3. Seguimiento Post-Visita

```
Nombre: post_visit_followup
Idioma: es

Hola {{1}}, esperamos que tu visita haya sido de tu agrado.

¿Qué te pareció el {{2}}? ¿Tienes alguna pregunta adicional?

Estamos aquí para ayudarte en tu decisión.

[Tengo preguntas] [Hacer oferta] [No me interesa]
```

---

## ⚙️ Configuración

```json
{
  "WhatsApp": {
    "PhoneNumberId": "123456789",
    "BusinessAccountId": "987654321",
    "AccessToken": "EAAxxxxxx",
    "BusinessNumber": "18091234567",
    "WebhookVerifyToken": "okla_webhook_2026",
    "TemplateNamespace": "okla_marketplace"
  }
}
```

---

## 📞 Recursos

| Recurso | URL |
|---------|-----|
| Meta for Developers | [developers.facebook.com](https://developers.facebook.com) |
| WhatsApp Business API Docs | [developers.facebook.com/docs/whatsapp](https://developers.facebook.com/docs/whatsapp) |
| Precios | [business.whatsapp.com/pricing](https://business.whatsapp.com/pricing) |
| Soporte | [business.facebook.com/help](https://business.facebook.com/help) |

---

**Anterior:** [ASEGURADORAS_API.md](./ASEGURADORAS_API.md)  
**Siguiente:** [SMS_GATEWAYS_API.md](./SMS_GATEWAYS_API.md)
