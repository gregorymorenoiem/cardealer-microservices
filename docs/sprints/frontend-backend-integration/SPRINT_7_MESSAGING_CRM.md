# 💬 SPRINT 7 - Messaging & CRM

**Fecha:** 2 Enero 2026  
**Duración estimada:** 3-4 horas  
**Tokens estimados:** ~20,000  
**Prioridad:** 🟡 Media

---

## 🎯 OBJETIVOS

1. Crear sistema de mensajería entre compradores y dealers
2. Implementar chat en tiempo real
3. Crear página de inbox/mensajes
4. Gestionar leads en CRMService
5. Implementar tracking de contactos
6. Crear dashboard de leads para dealers

---

## 📋 CHECKLIST DE TAREAS

### Fase 1: Backend - Messaging System (1.5 horas)

- [ ] 1.1. Crear entidades Message y Conversation
- [ ] 1.2. Implementar endpoints de mensajería
- [ ] 1.3. Agregar SignalR para real-time
- [ ] 1.4. Implementar notificaciones de mensajes

### Fase 2: Backend - CRM Integration (1 hora)

- [ ] 2.1. Crear entidades Lead y Contact
- [ ] 2.2. Implementar tracking de interacciones
- [ ] 2.3. Crear dashboard de leads
- [ ] 2.4. Agregar analytics de conversión

### Fase 3: Frontend - Chat UI (1.5 horas)

- [ ] 3.1. Crear ChatPage con lista de conversaciones
- [ ] 3.2. Implementar ChatWindow component
- [ ] 3.3. Agregar real-time updates
- [ ] 3.4. Implementar typing indicators

---

## 📝 IMPLEMENTACIÓN DETALLADA

### 1️⃣ Backend - Message Entity

**Archivo:** `backend/MessageService/MessageService.Domain/Entities/Message.cs`

```csharp
namespace MessageService.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public Conversation Conversation { get; set; } = null!;
}

public class Conversation
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string VehicleTitle { get; set; } = string.Empty;
    
    public Guid BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    
    public DateTime LastMessageAt { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    
    public int UnreadCount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public List<Message> Messages { get; set; } = new();
}

public enum MessageType
{
    Text,
    Image,
    PhoneNumber,
    Email
}
```

---

### 2️⃣ Backend - Messages Controller

**Archivo:** `backend/MessageService/MessageService.Api/Controllers/MessagesController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MediatR;

namespace MessageService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<ChatHub> _hubContext;

    public MessagesController(IMediator mediator, IHubContext<ChatHub> hubContext)
    {
        _mediator = mediator;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Get user's conversations
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? "");
        var query = new GetConversationsQuery(userId);
        var result = await _mediator.Send(query);

        return Ok(result.Value);
    }

    /// <summary>
    /// Get conversation by ID with messages
    /// </summary>
    [HttpGet("conversations/{id}")]
    public async Task<IActionResult> GetConversation(Guid id)
    {
        var query = new GetConversationQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Send message
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var senderId = Guid.Parse(User.FindFirst("sub")?.Value ?? "");
        
        var command = new SendMessageCommand
        {
            ConversationId = request.ConversationId,
            SenderId = senderId,
            Content = request.Content,
            Type = request.Type
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        // Send real-time notification
        await _hubContext.Clients
            .Group($"conversation_{request.ConversationId}")
            .SendAsync("ReceiveMessage", result.Value);

        return Ok(result.Value);
    }

    /// <summary>
    /// Mark messages as read
    /// </summary>
    [HttpPut("conversations/{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? "");
        
        var command = new MarkMessagesAsReadCommand
        {
            ConversationId = id,
            UserId = userId
        };

        await _mediator.Send(command);
        return NoContent();
    }
}

public record SendMessageRequest(
    Guid ConversationId,
    string Content,
    MessageType Type = MessageType.Text
);
```

---

### 3️⃣ Backend - SignalR Chat Hub

**Archivo:** `backend/MessageService/MessageService.Api/Hubs/ChatHub.cs`

```csharp
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace MessageService.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    public async Task SendTypingIndicator(string conversationId, string userName)
    {
        await Clients.OthersInGroup($"conversation_{conversationId}")
            .SendAsync("UserTyping", userName);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnConnectedAsync();
    }
}
```

**Configurar SignalR en Program.cs:**

```csharp
// Add SignalR
builder.Services.AddSignalR();

// Map hub
app.MapHub<ChatHub>("/hubs/chat");
```

---

### 4️⃣ Frontend - Chat Page

**Archivo:** `frontend/web/original/src/pages/ChatPage.tsx`

```typescript
import { useState, useEffect, type FC } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Send, Search, Circle } from 'lucide-react';
import { api } from '@/services/api';
import * as signalR from '@microsoft/signalr';

interface Conversation {
  id: string;
  vehicleTitle: string;
  otherUser: {
    id: string;
    name: string;
    avatar?: string;
  };
  lastMessage: string;
  lastMessageAt: string;
  unreadCount: number;
}

interface Message {
  id: string;
  senderId: string;
  senderName: string;
  content: string;
  createdAt: string;
  isRead: boolean;
}

export const ChatPage: FC = () => {
  const [selectedConversation, setSelectedConversation] = useState<string | null>(null);
  const [messageInput, setMessageInput] = useState('');
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);

  // Fetch conversations
  const { data: conversations = [] } = useQuery({
    queryKey: ['conversations'],
    queryFn: async () => {
      const response = await api.get<Conversation[]>('/messages/conversations');
      return response.data;
    },
  });

  // Fetch selected conversation messages
  useEffect(() => {
    if (!selectedConversation) return;

    const fetchMessages = async () => {
      const response = await api.get<{ messages: Message[] }>(
        `/messages/conversations/${selectedConversation}`
      );
      setMessages(response.data.messages);
    };

    fetchMessages();
  }, [selectedConversation]);

  // Setup SignalR connection
  useEffect(() => {
    const token = localStorage.getItem('accessToken');
    
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_API_URL.replace('/api', '')}/hubs/chat`, {
        accessTokenFactory: () => token || ''
      })
      .withAutomaticReconnect()
      .build();

    newConnection.start()
      .then(() => {
        console.log('Connected to chat hub');
        setConnection(newConnection);
      })
      .catch(err => console.error('SignalR connection error:', err));

    return () => {
      newConnection.stop();
    };
  }, []);

  // Listen for new messages
  useEffect(() => {
    if (!connection) return;

    connection.on('ReceiveMessage', (message: Message) => {
      setMessages(prev => [...prev, message]);
    });

    if (selectedConversation) {
      connection.invoke('JoinConversation', selectedConversation);
    }

    return () => {
      if (selectedConversation) {
        connection.invoke('LeaveConversation', selectedConversation);
      }
    };
  }, [connection, selectedConversation]);

  const handleSendMessage = async () => {
    if (!messageInput.trim() || !selectedConversation) return;

    try {
      await api.post('/messages/send', {
        conversationId: selectedConversation,
        content: messageInput,
        type: 'Text'
      });

      setMessageInput('');
    } catch (error) {
      console.error('Error sending message:', error);
    }
  };

  return (
    <div className="h-screen flex">
      {/* Conversations List */}
      <div className="w-80 border-r border-gray-200 flex flex-col">
        <div className="p-4 border-b border-gray-200">
          <h2 className="text-xl font-semibold mb-3">Mensajes</h2>
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 
                             text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="Buscar conversaciones..."
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>
        </div>

        <div className="flex-1 overflow-y-auto">
          {conversations.map((conv) => (
            <button
              key={conv.id}
              onClick={() => setSelectedConversation(conv.id)}
              className={`w-full p-4 flex items-start gap-3 hover:bg-gray-50 
                         transition-colors border-b border-gray-100
                         ${selectedConversation === conv.id ? 'bg-blue-50' : ''}`}
            >
              <div className="w-12 h-12 bg-gray-200 rounded-full flex-shrink-0" />
              
              <div className="flex-1 text-left min-w-0">
                <div className="flex items-center justify-between mb-1">
                  <p className="font-medium text-gray-900 truncate">
                    {conv.otherUser.name}
                  </p>
                  {conv.unreadCount > 0 && (
                    <span className="bg-blue-600 text-white text-xs px-2 py-0.5 rounded-full">
                      {conv.unreadCount}
                    </span>
                  )}
                </div>
                <p className="text-sm text-gray-600 mb-1 truncate">{conv.vehicleTitle}</p>
                <p className="text-sm text-gray-500 truncate">{conv.lastMessage}</p>
              </div>
            </button>
          ))}
        </div>
      </div>

      {/* Chat Area */}
      {selectedConversation ? (
        <div className="flex-1 flex flex-col">
          {/* Messages */}
          <div className="flex-1 overflow-y-auto p-6 space-y-4">
            {messages.map((message) => (
              <div
                key={message.id}
                className={`flex ${message.senderId === 'current-user' ? 'justify-end' : 'justify-start'}`}
              >
                <div
                  className={`max-w-md px-4 py-2 rounded-lg ${
                    message.senderId === 'current-user'
                      ? 'bg-blue-600 text-white'
                      : 'bg-gray-200 text-gray-900'
                  }`}
                >
                  <p>{message.content}</p>
                  <p className="text-xs mt-1 opacity-70">
                    {new Date(message.createdAt).toLocaleTimeString('es-MX', {
                      hour: '2-digit',
                      minute: '2-digit'
                    })}
                  </p>
                </div>
              </div>
            ))}
          </div>

          {/* Input */}
          <div className="p-4 border-t border-gray-200">
            <div className="flex items-center gap-2">
              <input
                type="text"
                value={messageInput}
                onChange={(e) => setMessageInput(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && handleSendMessage()}
                placeholder="Escribe un mensaje..."
                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg 
                         focus:ring-2 focus:ring-blue-600 focus:border-transparent"
              />
              <button
                onClick={handleSendMessage}
                disabled={!messageInput.trim()}
                className="p-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
                         disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <Send className="w-5 h-5" />
              </button>
            </div>
          </div>
        </div>
      ) : (
        <div className="flex-1 flex items-center justify-center text-gray-500">
          <p>Selecciona una conversación para empezar</p>
        </div>
      )}
    </div>
  );
};
```

**Instalar dependencia:**

```bash
npm install @microsoft/signalr
```

---

## ✅ CRITERIOS DE ACEPTACIÓN

1. Usuario puede enviar mensaje sobre un vehículo
2. Dealer recibe notificación de nuevo mensaje
3. Chat se actualiza en tiempo real
4. Mensajes se marcan como leídos automáticamente
5. Conversaciones se ordenan por última actividad

---

## 📊 ESTIMACIÓN DE TOKENS

| Tarea | Tokens |
|-------|--------|
| Message entities | 3,000 |
| Messages controller | 4,000 |
| SignalR hub | 3,000 |
| Frontend chat page | 6,000 |
| CRM integration | 3,000 |
| Testing | 1,000 |
| **TOTAL** | **~20,000** |

---

## ➡️ PRÓXIMO SPRINT

**Sprint 8:** Search & Filters

---

**Estado:** ⚪ Pendiente  
**Última actualización:** 2 Enero 2026
