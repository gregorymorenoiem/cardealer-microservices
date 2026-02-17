# 💬 WhatsApp Business API - Documentación Técnica

**API Provider:** Meta (Facebook)  
**Versión:** v18.0  
**Tipo:** Messaging Platform  
**Status en OKLA:** 🚧 Planificado (Q2 2026)  
**Última actualización:** Enero 15, 2026

---

## 📋 Descripción General

**WhatsApp Business API** se utiliza para:

- Confirmaciones de orden
- Notificaciones de entrega
- Consultas de soporte
- Alertas de vehículos disponibles
- Mensajes de marketing (templates aprobados)
- Chat directo con dealers

**¿Por qué WhatsApp?**

- ✅ **Cobertura muy alta en RD** (90%+ tienen WhatsApp)
- ✅ **Open rate:** 98%+ (vs 25% email)
- ✅ **Click rate:** 15%+ (vs 5% email)
- ✅ **Official API** con compliance
- ✅ **Templates pre-aprobados** (sin spam)
- ✅ **Webhook para respuestas**

---

## 🔑 Autenticación

### Crear Business Account en WhatsApp

1. Ir a [Meta Business Manager](https://business.facebook.com/)
2. Crear **WhatsApp Business Account**
3. Verificar **Business Phone Number** (+1-555-xxx-xxxx)
4. Obtener **Phone Number ID** y **Access Token**

### En appsettings.json

```json
{
  "WhatsApp": {
    "PhoneNumberId": "1234567890",
    "AccessToken": "${WHATSAPP_ACCESS_TOKEN}",
    "WhatsAppBusinessAccountId": "123456789",
    "WebhookVerifyToken": "${WHATSAPP_WEBHOOK_TOKEN}"
  }
}
```

---

## 🔌 Endpoints Principales

### Enviar Mensaje Texto

```
POST https://graph.instagram.com/v18.0/{phone_number_id}/messages
```

**Headers:**

```
Authorization: Bearer {ACCESS_TOKEN}
Content-Type: application/json
```

**Body:**

```json
{
  "messaging_product": "whatsapp",
  "recipient_type": "individual",
  "to": "5218001234567",
  "type": "text",
  "text": {
    "preview_url": false,
    "body": "Hola! Tu vehículo favorito está disponible. Haz clic aquí: https://okla.com.do/vehicles/123"
  }
}
```

**Response (200 OK):**

```json
{
  "messaging_product": "whatsapp",
  "contacts": [
    {
      "input": "5218001234567",
      "wa_id": "5218001234567"
    }
  ],
  "messages": [
    {
      "id": "wamid.xxxxxxxxxxxxx",
      "message_status": "accepted"
    }
  ]
}
```

### Enviar Template (Mensaje Aprobado)

```
POST https://graph.instagram.com/v18.0/{phone_number_id}/messages
```

**Body:**

```json
{
  "messaging_product": "whatsapp",
  "to": "5218001234567",
  "type": "template",
  "template": {
    "name": "vehicle_alert",
    "language": {
      "code": "es"
    },
    "components": [
      {
        "type": "body",
        "parameters": [
          {
            "type": "text",
            "text": "Toyota Corolla 2020"
          },
          {
            "type": "text",
            "text": "$15,000"
          }
        ]
      }
    ]
  }
}
```

### Enviar Imagen

```json
{
  "messaging_product": "whatsapp",
  "recipient_type": "individual",
  "to": "5218001234567",
  "type": "image",
  "image": {
    "link": "https://okla-media.nyc3.cdn.digitaloceanspaces.com/vehicles/123/image.jpg"
  }
}
```

### Webhook para Respuestas Entrantes

```
POST {webhook_url}
```

**Body (Message Received):**

```json
{
  "object": "whatsapp_business_account",
  "entry": [
    {
      "id": "123456789",
      "changes": [
        {
          "value": {
            "messaging_product": "whatsapp",
            "metadata": {
              "display_phone_number": "5218001234567",
              "phone_number_id": "1234567890"
            },
            "messages": [
              {
                "from": "5218001234567",
                "id": "wamid.xxxxxxxxxxxxx",
                "timestamp": "1672531200",
                "text": {
                  "body": "Sí, estoy interesado en el vehículo"
                },
                "type": "text"
              }
            ]
          },
          "field": "messages"
        }
      ]
    }
  ]
}
```

---

## 💻 Implementación en C#/.NET

### Instalación del paquete

```bash
dotnet add package RestSharp
```

### WhatsAppService.cs

```csharp
using RestSharp;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace NotificationService.Infrastructure.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly RestClient _client;
    private readonly string _phoneNumberId;
    private readonly string _accessToken;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(
        string phoneNumberId,
        string accessToken,
        ILogger<WhatsAppService> logger)
    {
        _phoneNumberId = phoneNumberId;
        _accessToken = accessToken;
        _logger = logger;
        _client = new RestClient("https://graph.instagram.com/v18.0");
    }

    // ✅ Enviar mensaje de texto
    public async Task<Result<string>> SendTextMessageAsync(
        string phoneNumber,
        string message,
        bool includePreview = false,
        CancellationToken ct = default)
    {
        try
        {
            // Validar número (debe incluir country code)
            if (!phoneNumber.StartsWith("+"))
            {
                phoneNumber = "+" + phoneNumber;
            }

            var request = new RestRequest($"/{_phoneNumberId}/messages", Method.Post);
            request.AddHeader("Authorization", $"Bearer {_accessToken}");

            var body = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = phoneNumber.TrimStart('+'),
                type = "text",
                text = new
                {
                    preview_url = includePreview,
                    body = message
                }
            };

            request.AddJsonBody(body);

            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                _logger.LogError($"WhatsApp error: {response.StatusCode} - {response.Content}");
                return Result<string>.Failure($"Failed to send: {response.StatusCode}");
            }

            var content = JsonSerializer.Deserialize<WhatsAppResponse>(response.Content);
            var messageId = content?.Messages?.FirstOrDefault()?.Id ?? Guid.NewGuid().ToString();

            _logger.LogInformation($"WhatsApp message sent to {phoneNumber}. MessageId: {messageId}");
            return Result<string>.Success(messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending WhatsApp message");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Enviar template (mensaje aprobado)
    public async Task<Result<string>> SendTemplateAsync(
        string phoneNumber,
        string templateName,
        List<string> parameters,
        string languageCode = "es",
        CancellationToken ct = default)
    {
        try
        {
            if (!phoneNumber.StartsWith("+"))
            {
                phoneNumber = "+" + phoneNumber;
            }

            var request = new RestRequest($"/{_phoneNumberId}/messages", Method.Post);
            request.AddHeader("Authorization", $"Bearer {_accessToken}");

            // Construir parámetros del template
            var templateParams = parameters.Select(p => new { type = "text", text = p }).ToList();

            var body = new
            {
                messaging_product = "whatsapp",
                to = phoneNumber.TrimStart('+'),
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new { code = languageCode },
                    components = new[]
                    {
                        new
                        {
                            type = "body",
                            parameters = templateParams
                        }
                    }
                }
            };

            request.AddJsonBody(body);
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                _logger.LogError($"WhatsApp template error: {response.StatusCode}");
                return Result<string>.Failure($"Failed: {response.StatusCode}");
            }

            _logger.LogInformation($"Template sent to {phoneNumber}");
            return Result<string>.Success("Sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending template");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Enviar imagen
    public async Task<Result<string>> SendImageAsync(
        string phoneNumber,
        string imageUrl,
        string caption = null,
        CancellationToken ct = default)
    {
        try
        {
            if (!phoneNumber.StartsWith("+"))
            {
                phoneNumber = "+" + phoneNumber;
            }

            var request = new RestRequest($"/{_phoneNumberId}/messages", Method.Post);
            request.AddHeader("Authorization", $"Bearer {_accessToken}");

            var imageBody = new { link = imageUrl };
            if (!string.IsNullOrEmpty(caption))
            {
                imageBody = new { link = imageUrl, caption };
            }

            var body = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = phoneNumber.TrimStart('+'),
                type = "image",
                image = imageBody
            };

            request.AddJsonBody(body);
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                return Result<string>.Failure($"Failed: {response.StatusCode}");
            }

            return Result<string>.Success("Image sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending image");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Manejar webhook entrante
    public async Task<Result<bool>> ProcessWebhookAsync(
        WhatsAppWebhookPayload payload,
        CancellationToken ct = default)
    {
        try
        {
            var messages = payload.Entry?[0]?.Changes?[0]?.Value?.Messages;

            if (messages == null || !messages.Any())
            {
                return Result<bool>.Success(true);
            }

            foreach (var message in messages)
            {
                _logger.LogInformation($"Incoming message from {message.From}: {message.Text?.Body}");

                // Guardar en BD y procesar
                await _notificationService.LogIncomingWhatsAppAsync(
                    message.From,
                    message.Text?.Body,
                    message.Timestamp);

                // TODO: Enviar respuesta automática si es necesario
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception processing webhook");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }
}

// DTOs
public class WhatsAppResponse
{
    [JsonPropertyName("messages")]
    public List<WhatsAppMessage> Messages { get; set; }
}

public class WhatsAppMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("message_status")]
    public string Status { get; set; }
}

public class WhatsAppWebhookPayload
{
    [JsonPropertyName("entry")]
    public List<WhatsAppEntry> Entry { get; set; }
}

public class WhatsAppEntry
{
    [JsonPropertyName("changes")]
    public List<WhatsAppChange> Changes { get; set; }
}

public class WhatsAppChange
{
    [JsonPropertyName("value")]
    public WhatsAppValue Value { get; set; }
}

public class WhatsAppValue
{
    [JsonPropertyName("messages")]
    public List<WhatsAppIncomingMessage> Messages { get; set; }
}

public class WhatsAppIncomingMessage
{
    [JsonPropertyName("from")]
    public string From { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("text")]
    public WhatsAppText Text { get; set; }
}

public class WhatsAppText
{
    [JsonPropertyName("body")]
    public string Body { get; set; }
}
```

---

## 🎯 Casos de Uso en OKLA

### 1. Alerta de Vehículo Disponible

```csharp
await _whatsappService.SendTemplateAsync(
    buyer.PhoneNumber,
    "vehicle_alert",
    new List<string>
    {
        vehicle.Title,
        $"${vehicle.Price}",
        $"https://okla.com.do/vehicles/{vehicle.Id}"
    });
```

### 2. Confirmación de Orden

```csharp
await _whatsappService.SendTemplateAsync(
    buyer.PhoneNumber,
    "order_confirmation",
    new List<string>
    {
        order.Id.ToString(),
        vehicle.Title,
        dealer.Name
    });
```

### 3. Soporte al Cliente

```csharp
// Recibir mensaje del usuario
// Procesar en webhook
// Responder automáticamente

await _whatsappService.SendTextMessageAsync(
    buyer.PhoneNumber,
    "¡Hola! Gracias por tu mensaje. Un agente te responderá en los próximos 5 minutos.");
```

---

## 🔐 Seguridad y Best Practices

### ✅ Do's

- ✅ **Usar templates aprobados** (evita spam)
- ✅ **Respetar horarios** (no enviar 23:00-07:00)
- ✅ **Obtener consent** antes de enviar marketing
- ✅ **Validar números** antes de enviar

### ❌ Don'ts

- ❌ **NO enviar spam** (cuenta será suspendida)
- ❌ **NO usar mensajes generados** (usar templates)
- ❌ **NO compartir access token**

---

## 💰 Costos

| Tipo de Mensaje              | Costo   |
| ---------------------------- | ------- |
| **Categoría Marketing**      | $0.0050 |
| **Categoría Utility**        | $0.0018 |
| **Categoría Authentication** | $0.0082 |
| **Inbound**                  | Free    |

**Costo OKLA (Enero 2026):** $0 (sin volumen aún)

---

**Mantenido por:** Notification Team  
**Última revisión:** Enero 15, 2026  
**Próxima implementación:** Q2 2026
