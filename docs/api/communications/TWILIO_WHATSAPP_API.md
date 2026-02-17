# 💬 Twilio WhatsApp API

**API Provider:** Twilio  
**Endpoint:** https://api.twilio.com/2010-04-01/Accounts/{AccountSID}/Messages  
**Método:** POST  
**Autenticación:** HTTP Basic (AccountSID:AuthToken)  
**Rate Limit:** 1,000 messages/day (configurable)  
**Latencia:** 5-15 segundos

---

## 📝 Descripción

Twilio WhatsApp permite enviar mensajes directos a usuarios de WhatsApp desde OKLA. Casos de uso:

- ✅ Contacto directo buyer-seller (negociación)
- ✅ Notificaciones de nuevo interés en vehículo
- ✅ Confirmación de citas para test drive
- ✅ Recordatorio de publicación expirada
- ✅ Ofertas personalizadas del dealer

---

## 🔧 Especificaciones Técnicas

### Request

```bash
POST /2010-04-01/Accounts/ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxx/Messages
Host: api.twilio.com
Authorization: Basic base64(AccountSID:AuthToken)
Content-Type: application/x-www-form-urlencoded

From=whatsapp:%2B1234567890
To=whatsapp:%2B1234567890
Body=Hola! Tu vehículo tiene un nuevo interés...
MediaUrl=https://okla.com.do/images/car.jpg (opcional)
```

### Response

```json
{
  "sid": "SMxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "account_sid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "to": "whatsapp:+1234567890",
  "from": "whatsapp:+1234567890",
  "body": "Hola! Tu vehículo tiene un nuevo interés...",
  "status": "queued",
  "num_media": "0",
  "num_segments": "1",
  "direction": "outbound-api",
  "api_version": "2010-04-01",
  "price": "-0.00750",
  "price_unit": "USD",
  "error_code": null,
  "error_message": null,
  "uri": "/2010-04-01/Accounts/.../Messages/SMxxxxx.json",
  "date_created": "Thu, 15 Jan 2026 10:30:00 +0000",
  "date_sent": "Thu, 15 Jan 2026 10:30:05 +0000",
  "date_updated": "Thu, 15 Jan 2026 10:30:05 +0000"
}
```

---

## 💻 Implementación Backend (.NET 8)

### 1. Instalación de paquetes

```bash
dotnet add package Twilio
dotnet add package Microsoft.Extensions.Http
```

### 2. Configuración (appsettings.json)

```json
{
  "Twilio": {
    "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AuthToken": "your_auth_token_here",
    "WhatsAppNumber": "+1234567890",
    "DailyLimit": 1000,
    "RateLimitPerUser": 100
  }
}
```

### 3. Domain Model

```csharp
namespace NotificationService.Domain.Entities;

public class WhatsAppMessage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ToPhoneNumber { get; set; } // E.164 format: +1234567890
    public string MessageBody { get; set; }
    public string? ImageUrl { get; set; }
    public string TwilioMessageSid { get; set; }
    public MessageStatus Status { get; set; } // queued, sent, delivered, failed, etc.
    public string? ErrorMessage { get; set; }
    public decimal CostUSD { get; set; }
    public int AttemptCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

public enum MessageStatus
{
    Queued,
    Sent,
    Delivered,
    Failed,
    Undelivered,
    Read
}
```

### 4. Application Service (CQRS)

```csharp
namespace NotificationService.Application.Features.WhatsApp.Commands;

public record SendWhatsAppMessageCommand(
    Guid UserId,
    string ToPhoneNumber,
    string MessageBody,
    string? ImageUrl = null
) : IRequest<SendWhatsAppMessageResult>;

public record SendWhatsAppMessageResult(
    bool Success,
    string? TwilioMessageSid,
    string? ErrorMessage
);

public class SendWhatsAppMessageHandler : IRequestHandler<SendWhatsAppMessageCommand, SendWhatsAppMessageResult>
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly IWhatsAppRepository _repository;
    private readonly ILogger<SendWhatsAppMessageHandler> _logger;

    public SendWhatsAppMessageHandler(
        IWhatsAppService whatsAppService,
        IWhatsAppRepository repository,
        ILogger<SendWhatsAppMessageHandler> logger)
    {
        _whatsAppService = whatsAppService;
        _repository = repository;
        _logger = logger;
    }

    public async Task<SendWhatsAppMessageResult> Handle(
        SendWhatsAppMessageCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validar número de teléfono
            if (!PhoneNumberValidator.IsValid(request.ToPhoneNumber))
                return new(false, null, "Invalid phone number format");

            // 2. Validar rate limit
            var recentMessages = await _repository
                .GetRecentMessagesAsync(request.UserId, timeWindowMinutes: 60);

            if (recentMessages.Count >= 100)
                return new(false, null, "Rate limit exceeded");

            // 3. Enviar mensaje
            var result = await _whatsAppService.SendMessageAsync(
                request.ToPhoneNumber,
                request.MessageBody,
                request.ImageUrl,
                cancellationToken);

            if (!result.Success)
                return result;

            // 4. Guardar en BD
            var message = new WhatsAppMessage
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ToPhoneNumber = request.ToPhoneNumber,
                MessageBody = request.MessageBody,
                ImageUrl = request.ImageUrl,
                TwilioMessageSid = result.TwilioMessageSid!,
                Status = MessageStatus.Sent,
                CostUSD = 0.015m,
                AttemptCount = 1,
                CreatedAt = DateTime.UtcNow,
                SentAt = DateTime.UtcNow
            };

            await _repository.AddAsync(message, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "WhatsApp message sent. UserId: {UserId}, To: {To}, Sid: {Sid}",
                request.UserId, request.ToPhoneNumber, result.TwilioMessageSid);

            return new(true, result.TwilioMessageSid, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp message");
            return new(false, null, ex.Message);
        }
    }
}
```

### 5. Twilio Service

```csharp
namespace NotificationService.Infrastructure.Services;

public interface IWhatsAppService
{
    Task<SendWhatsAppMessageResult> SendMessageAsync(
        string toPhoneNumber,
        string messageBody,
        string? imageUrl,
        CancellationToken cancellationToken);
}

public class TwilioWhatsAppService : IWhatsAppService
{
    private readonly TwilioClient _twilioClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioWhatsAppService> _logger;

    public TwilioWhatsAppService(
        IConfiguration configuration,
        ILogger<TwilioWhatsAppService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Inicializar Twilio
        var accountSid = configuration["Twilio:AccountSid"];
        var authToken = configuration["Twilio:AuthToken"];
        TwilioClient.Init(accountSid, authToken);
    }

    public async Task<SendWhatsAppMessageResult> SendMessageAsync(
        string toPhoneNumber,
        string messageBody,
        string? imageUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            var fromNumber = _configuration["Twilio:WhatsAppNumber"];

            var messageResource = await MessageResource.CreateAsync(
                body: messageBody,
                from: new Twilio.Types.PhoneNumber($"whatsapp:{fromNumber}"),
                to: new Twilio.Types.PhoneNumber($"whatsapp:{toPhoneNumber}"),
                mediaUrl: imageUrl != null ? new List<Uri> { new Uri(imageUrl) } : null
            );

            _logger.LogInformation(
                "WhatsApp message sent: {Sid}, Status: {Status}",
                messageResource.Sid, messageResource.Status);

            return new(
                Success: true,
                TwilioMessageSid: messageResource.Sid,
                ErrorMessage: null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp message via Twilio");
            return new(
                Success: false,
                TwilioMessageSid: null,
                ErrorMessage: ex.Message
            );
        }
    }
}
```

### 6. Controller

```csharp
namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WhatsAppController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage(
        [FromBody] SendWhatsAppDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var command = new SendWhatsAppMessageCommand(
            Guid.Parse(userId),
            request.ToPhoneNumber,
            request.MessageBody,
            request.ImageUrl
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(new {
            success = true,
            messageId = result.TwilioMessageSid,
            cost = 0.015m
        });
    }

    [HttpGet("status/{messageId}")]
    public async Task<IActionResult> GetMessageStatus(string messageId)
    {
        // Consultar Twilio para estado actual
        // Actualizar en BD
        // Retornar estado
        return Ok();
    }
}

public class SendWhatsAppDto
{
    [Required]
    [RegularExpression(@"^\+?[1-9]\d{1,14}$")] // E.164 format
    public string ToPhoneNumber { get; set; }

    [Required]
    [StringLength(4096)]
    public string MessageBody { get; set; }

    [Url]
    public string? ImageUrl { get; set; }
}
```

---

## 🎨 Implementación Frontend (React 19 + TypeScript)

### 1. Hook personalizado

```typescript
// hooks/useWhatsApp.ts
import { useMutation, useQuery } from "@tanstack/react-query";
import { api } from "@/services/api";

interface SendWhatsAppRequest {
  toPhoneNumber: string;
  messageBody: string;
  imageUrl?: string;
}

interface WhatsAppMessage {
  messageId: string;
  status: "queued" | "sent" | "delivered" | "failed";
  cost: number;
}

export const useWhatsApp = () => {
  const sendMessage = useMutation<WhatsAppMessage, Error, SendWhatsAppRequest>({
    mutationFn: async (data) => {
      const response = await api.post<WhatsAppMessage>(
        "/api/whatsapp/send",
        data
      );
      return response.data;
    },
  });

  return { sendMessage };
};
```

### 2. Componente para enviar mensajes

```typescript
// components/WhatsAppModal.tsx
import React, { useState } from "react";
import { useWhatsApp } from "@/hooks/useWhatsApp";

interface WhatsAppModalProps {
  isOpen: boolean;
  recipientPhone: string;
  onClose: () => void;
}

export const WhatsAppModal: React.FC<WhatsAppModalProps> = ({
  isOpen,
  recipientPhone,
  onClose,
}) => {
  const { sendMessage } = useWhatsApp();
  const [message, setMessage] = useState("");
  const [imageUrl, setImageUrl] = useState<string>();
  const [loading, setLoading] = useState(false);

  const handleSend = async () => {
    setLoading(true);
    try {
      const result = await sendMessage.mutateAsync({
        toPhoneNumber: recipientPhone,
        messageBody: message,
        imageUrl,
      });

      console.log("Message sent:", result);
      setMessage("");
      setImageUrl(undefined);
      onClose();
    } catch (error) {
      console.error("Error sending message:", error);
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal">
      <div className="modal-content">
        <h2>Enviar por WhatsApp</h2>

        <input
          type="text"
          disabled
          value={recipientPhone}
          placeholder="Número de teléfono"
        />

        <textarea
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          placeholder="Escribe tu mensaje..."
          maxLength={4096}
        />

        <input
          type="url"
          value={imageUrl || ""}
          onChange={(e) => setImageUrl(e.target.value)}
          placeholder="URL de imagen (opcional)"
        />

        <div className="modal-actions">
          <button onClick={onClose}>Cancelar</button>
          <button onClick={handleSend} disabled={!message || loading}>
            {loading ? "Enviando..." : "Enviar"}
          </button>
        </div>
      </div>
    </div>
  );
};
```

### 3. Integración en página de vehículo

```typescript
// pages/VehicleDetailPage.tsx
import { WhatsAppModal } from "@/components/WhatsAppModal";

export const VehicleDetailPage = () => {
  const [showWhatsApp, setShowWhatsApp] = useState(false);
  const [sellerPhone, setSellerPhone] = useState<string>();

  const handleContactSeller = (phone: string) => {
    setSellerPhone(phone);
    setShowWhatsApp(true);
  };

  return (
    <div>
      {/* Detalles del vehículo */}
      <button onClick={() => handleContactSeller("+1234567890")}>
        📱 Contactar por WhatsApp
      </button>

      <WhatsAppModal
        isOpen={showWhatsApp}
        recipientPhone={sellerPhone || ""}
        onClose={() => setShowWhatsApp(false)}
      />
    </div>
  );
};
```

---

## 🧪 Testing

### Unit Tests (xUnit)

```csharp
public class SendWhatsAppMessageHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_SendsMessage()
    {
        // Arrange
        var command = new SendWhatsAppMessageCommand(
            Guid.NewGuid(),
            "+1234567890",
            "Test message"
        );

        var whatsAppServiceMock = new Mock<IWhatsAppService>();
        whatsAppServiceMock
            .Setup(s => s.SendMessageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendWhatsAppMessageResult(
                true,
                "SM1234567890",
                null
            ));

        var handler = new SendWhatsAppMessageHandler(
            whatsAppServiceMock.Object,
            new Mock<IWhatsAppRepository>().Object,
            new Mock<ILogger<SendWhatsAppMessageHandler>>().Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.TwilioMessageSid);
        whatsAppServiceMock.Verify(s => s.SendMessageAsync(
            "+1234567890",
            "Test message",
            null,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidPhoneNumber_ReturnsFalse()
    {
        // Arrange
        var command = new SendWhatsAppMessageCommand(
            Guid.NewGuid(),
            "invalid",
            "Test message"
        );

        // Act & Assert
        var handler = new SendWhatsAppMessageHandler(
            new Mock<IWhatsAppService>().Object,
            new Mock<IWhatsAppRepository>().Object,
            new Mock<ILogger<SendWhatsAppMessageHandler>>().Object
        );

        var result = await handler.Handle(command, CancellationToken.None);
        Assert.False(result.Success);
    }
}
```

### Integration Tests

```typescript
// __tests__/whatsapp.integration.test.ts
describe("WhatsApp API Integration", () => {
  it("should send message successfully", async () => {
    const response = await fetch("/api/whatsapp/send", {
      method: "POST",
      body: JSON.stringify({
        toPhoneNumber: "+1234567890",
        messageBody: "Test message",
      }),
      headers: {
        Authorization: `Bearer ${authToken}`,
        "Content-Type": "application/json",
      },
    });

    expect(response.status).toBe(200);
    const data = await response.json();
    expect(data.success).toBe(true);
    expect(data.messageId).toBeDefined();
  });
});
```

---

## 🐛 Troubleshooting

| Problema              | Causa                    | Solución                         |
| --------------------- | ------------------------ | -------------------------------- |
| 400 Bad Request       | Número teléfono inválido | Usar formato E.164: +1234567890  |
| 401 Unauthorized      | AuthToken inválido       | Verificar credenciales en Twilio |
| 429 Too Many Requests | Rate limit excedido      | Esperar antes de reintentarlo    |
| Message failed        | Número no existe         | Validar que existe en WhatsApp   |
| Timeout (>15s)        | Problema de red          | Reintentar después de 5 seg      |

---

## 💰 Costos

- **Costo por mensaje:** $0.015
- **Límite diario:** 1,000 mensajes (configurable)
- **Límite por dealer:** 100 mensajes/hora

**Estimado mensual (1,000 dealers):**

- 500,000 mensajes × $0.015 = **$7,500/mes**

---

## 🔗 Referencias

- **Docs Twilio:** https://www.twilio.com/docs/whatsapp/api
- **Webhook Status:** https://www.twilio.com/docs/whatsapp/api/messages-api#status-callbacks
- **E.164 Format:** https://www.twilio.com/docs/glossary/what-e164

---

**Versión:** 1.0  
**Estado:** ✅ Documentación Completa  
**Última actualización:** Enero 15, 2026
