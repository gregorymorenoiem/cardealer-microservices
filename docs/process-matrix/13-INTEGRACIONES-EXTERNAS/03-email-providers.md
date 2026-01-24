# 📧 Email Providers - Proveedores de Email - Matriz de Procesos

> **Servicio:** NotificationService  
> **Proveedor Principal:** Resend API ✅ (antes SendGrid)  
> **Última actualización:** Enero 21, 2026  
> **Estado:** ✅ PRODUCCIÓN - Funcionando

---

## 📊 Resumen de Implementación

| Componente      | Total | Implementado | Pendiente | Estado |
| --------------- | ----- | ------------ | --------- | ------ |
| Controllers     | 1     | 1            | 0         | 🟢     |
| EMAIL-SEND-\*   | 4     | 4            | 0         | ✅     |
| EMAIL-TPL-\*    | 5     | 5            | 0         | ✅     |
| EMAIL-TRANS-\*  | 4     | 4            | 0         | ✅     |
| EMAIL-DIGEST-\* | 3     | 0            | 3         | 🔴     |
| Tests           | 10    | 10           | 0         | ✅     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 1. Información General

### 1.1 Descripción

Sistema de envío de emails transaccionales utilizando **Resend**. Soporta templates dinámicos, personalización, y cumplimiento con regulaciones anti-spam.

> ⚠️ **ACTUALIZACIÓN Enero 2026:** Migrado de SendGrid a **Resend** para mejor integración con el stack moderno.

### 1.2 Proveedor Actual ✅

| Configuración          | Valor                         |
| ---------------------- | ----------------------------- |
| **Proveedor**          | Resend                        |
| **API URL**            | https://api.resend.com/emails |
| **Dominio verificado** | okla.com.do                   |
| **From Email**         | noreply@okla.com.do           |
| **API Key**            | re*Bi3rubbH*\*\*\* (secreto)  |

### 1.3 Tipos de Email

| Tipo              | Prioridad | Ejemplos                                 | Estado       |
| ----------------- | --------- | ---------------------------------------- | ------------ |
| **Transaccional** | Crítica   | Confirmación de registro, reset password | ✅ Activo    |
| **Notificación**  | Alta      | Nuevo lead, vehículo aprobado            | ✅ Activo    |
| **Digest**        | Media     | Resumen semanal de actividad             | 🔶 Pendiente |
| **Marketing**     | Baja      | Promociones, newsletters                 | 🔶 Pendiente |

### 1.4 Arquitectura Actual ✅

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Email Integration Architecture (Resend)              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   OKLA Backend                                                          │
│   ┌──────────────────────────────────────────────────────────────────┐  │
│   │                                                                   │  │
│   │  ┌─────────────┐     ┌─────────────┐     ┌──────────────────┐    │  │
│   │  │ AuthService │────▶│  RabbitMQ   │────▶│NotificationService│   │  │
│   │  └─────────────┘     │notification-│     │                  │    │  │
│   │                      │   queue     │     │ • Consumer       │    │  │
│   │  ┌─────────────┐     │             │     │ • Template Render│    │  │
│   │  │LeadService  │────▶│             │     │ • ResendService  │    │  │
│   │  └─────────────┘     │             │     └────────┬─────────┘    │  │
│   │                      │             │              │              │  │
│   │  ┌─────────────┐     │             │              ▼              │  │
│   │  │BillingService│───▶│             │     ┌───────────────────┐   │  │
│   │  └─────────────┘     └─────────────┘     │  Resend API       │   │  │
│   │                                          │  api.resend.com   │   │  │
│   │                                          └─────────┬─────────┘   │  │
│   │                                                    │             │  │
│   └────────────────────────────────────────────────────┼─────────────┘  │
│                                                        │                │
│                                                        ▼                │
│                                              ┌─────────────────┐        │
│                                              │  Usuario Email  │        │
│                                              └─────────────────┘        │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

│ │ │ │ │ │
│ └──────────────────────────┼────────────────┼──────────────────────┘ │
│ │ │ │
│ ▼ │ │
│ ┌──────────────────┐ │ │
│ │ User Inbox │ │ │
│ │ (Gmail, etc.) │ │ │
│ └────────┬─────────┘ │ │
│ │ │ │
│ │ Opens/Clicks │ │
│ └─────────────────┘ │
│ │ │
│ ▼ │
│ Webhook to OKLA │
│ │
└─────────────────────────────────────────────────────────────────────────┘

````

---

## 2. Endpoints

### 2.1 Envío de Emails

| Método | Endpoint                        | Descripción               | Auth    |
| ------ | ------------------------------- | ------------------------- | ------- |
| `POST` | `/api/email/send`               | Enviar email simple       | Service |
| `POST` | `/api/email/send-template`      | Enviar con template       | Service |
| `POST` | `/api/email/send-batch`         | Enviar batch (hasta 1000) | Service |
| `GET`  | `/api/email/status/{messageId}` | Estado del email          | Admin   |

### 2.2 Templates

| Método | Endpoint                       | Descripción       | Auth  |
| ------ | ------------------------------ | ----------------- | ----- |
| `GET`  | `/api/email/templates`         | Listar templates  | Admin |
| `GET`  | `/api/email/templates/{id}`    | Obtener template  | Admin |
| `POST` | `/api/email/templates/preview` | Preview con datos | Admin |

### 2.3 Webhooks

| Método | Endpoint                 | Descripción         | Auth      |
| ------ | ------------------------ | ------------------- | --------- |
| `POST` | `/api/webhooks/sendgrid` | Eventos de SendGrid | Signature |

### 2.4 Analytics

| Método | Endpoint                                 | Descripción            | Auth  |
| ------ | ---------------------------------------- | ---------------------- | ----- |
| `GET`  | `/api/email/stats`                       | Estadísticas generales | Admin |
| `GET`  | `/api/email/stats/template/{templateId}` | Stats por template     | Admin |

---

## 3. Entidades

### 3.1 EmailMessage

```csharp
public class EmailMessage
{
    public Guid Id { get; set; }
    public string MessageId { get; set; } = string.Empty; // SendGrid ID

    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public Guid? UserId { get; set; }

    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string? ReplyTo { get; set; }

    public string Subject { get; set; } = string.Empty;
    public EmailType Type { get; set; }
    public string? TemplateId { get; set; }
    public string? TemplateData { get; set; } // JSON

    public string? HtmlContent { get; set; }
    public string? TextContent { get; set; }

    public EmailStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public DateTime? BouncedAt { get; set; }
    public string? BounceReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; } // JSON array
}

public enum EmailType
{
    Transactional,
    Notification,
    Digest,
    Marketing
}

public enum EmailStatus
{
    Queued,
    Sent,
    Delivered,
    Opened,
    Clicked,
    Bounced,
    SpamReport,
    Unsubscribed,
    Dropped
}
````

### 3.2 EmailTemplate

```csharp
public class EmailTemplate
{
    public Guid Id { get; set; }
    public string SendGridTemplateId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public EmailType Type { get; set; }
    public string? Category { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? PreviewText { get; set; }

    public string RequiredFields { get; set; } = "[]"; // JSON array
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### 3.3 EmailUnsubscribe

```csharp
public class EmailUnsubscribe
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public Guid? UserId { get; set; }

    public UnsubscribeScope Scope { get; set; }
    public string? Category { get; set; } // Si scope = Category

    public string? Reason { get; set; }
    public DateTime UnsubscribedAt { get; set; }
}

public enum UnsubscribeScope
{
    All,           // No recibe ningún email
    Marketing,     // Solo no recibe marketing
    Category       // No recibe categoría específica
}
```

---

## 4. Templates de SendGrid

### 4.1 Templates Configurados

| Template Slug          | SendGrid ID | Tipo          | Uso                |
| ---------------------- | ----------- | ------------- | ------------------ |
| `welcome`              | d-abc123    | Transactional | Bienvenida         |
| `verify-email`         | d-def456    | Transactional | Verificar email    |
| `reset-password`       | d-ghi789    | Transactional | Reset password     |
| `vehicle-approved`     | d-jkl012    | Notification  | Vehículo aprobado  |
| `vehicle-rejected`     | d-mno345    | Notification  | Vehículo rechazado |
| `new-lead`             | d-pqr678    | Notification  | Nuevo lead         |
| `payment-receipt`      | d-stu901    | Transactional | Recibo de pago     |
| `subscription-renewed` | d-vwx234    | Transactional | Renovación         |
| `weekly-digest`        | d-yza567    | Digest        | Resumen semanal    |

### 4.2 Ejemplo de Template Data

```json
{
  "welcome": {
    "templateId": "d-abc123",
    "requiredFields": ["firstName", "verifyUrl"],
    "example": {
      "firstName": "Juan",
      "verifyUrl": "https://okla.com.do/verify?token=xxx"
    }
  },

  "new-lead": {
    "templateId": "d-pqr678",
    "requiredFields": [
      "sellerName",
      "buyerName",
      "buyerEmail",
      "buyerPhone",
      "vehicleTitle",
      "vehicleYear",
      "vehiclePrice",
      "vehicleUrl",
      "message"
    ],
    "example": {
      "sellerName": "María García",
      "buyerName": "Carlos Pérez",
      "buyerEmail": "carlos@email.com",
      "buyerPhone": "+1809-555-1234",
      "vehicleTitle": "Toyota Corolla",
      "vehicleYear": 2020,
      "vehiclePrice": "RD$1,250,000",
      "vehicleUrl": "https://okla.com.do/vehiculos/toyota-corolla-2020-abc123",
      "message": "Me interesa el vehículo. ¿Está disponible para ver mañana?"
    }
  },

  "payment-receipt": {
    "templateId": "d-stu901",
    "requiredFields": [
      "userName",
      "amount",
      "currency",
      "paymentMethod",
      "transactionId",
      "description",
      "date",
      "receiptUrl"
    ],
    "example": {
      "userName": "Juan Rodríguez",
      "amount": "2,499.00",
      "currency": "DOP",
      "paymentMethod": "Visa ****1234",
      "transactionId": "PAY-2026011500001",
      "description": "Publicación de vehículo - Individual",
      "date": "15 de enero, 2026",
      "receiptUrl": "https://okla.com.do/receipts/PAY-2026011500001"
    }
  }
}
```

### 4.3 Template HTML (SendGrid Dynamic Template)

```html
<!-- welcome template -->
<!DOCTYPE html>
<html>
  <head>
    <meta charset="utf-8" />
    <style>
      .container {
        max-width: 600px;
        margin: 0 auto;
        font-family: Arial, sans-serif;
      }
      .header {
        background: #3b82f6;
        color: white;
        padding: 20px;
        text-align: center;
      }
      .content {
        padding: 30px;
        background: #f9fafb;
      }
      .button {
        display: inline-block;
        background: #3b82f6;
        color: white;
        padding: 12px 24px;
        text-decoration: none;
        border-radius: 6px;
      }
      .footer {
        padding: 20px;
        text-align: center;
        font-size: 12px;
        color: #6b7280;
      }
    </style>
  </head>
  <body>
    <div class="container">
      <div class="header">
        <img src="https://okla.com.do/logo-white.png" alt="OKLA" width="120" />
      </div>
      <div class="content">
        <h1>¡Bienvenido a OKLA, {{firstName}}!</h1>
        <p>
          Gracias por unirte al marketplace de vehículos #1 en República
          Dominicana.
        </p>
        <p>Para completar tu registro, verifica tu correo electrónico:</p>
        <p style="text-align: center;">
          <a href="{{verifyUrl}}" class="button">Verificar Email</a>
        </p>
        <p>Si no creaste esta cuenta, puedes ignorar este email.</p>
      </div>
      <div class="footer">
        <p>© 2026 OKLA. Todos los derechos reservados.</p>
        <p>Santo Domingo, República Dominicana</p>
        <p><a href="{{unsubscribeUrl}}">Cancelar suscripción</a></p>
      </div>
    </div>
  </body>
</html>
```

---

## 5. Procesos Detallados

### 5.1 EMAIL-001: Enviar Email Transaccional

| Paso | Acción                           | Sistema             | Validación         |
| ---- | -------------------------------- | ------------------- | ------------------ |
| 1    | Evento dispara notificación      | RabbitMQ            | Evento válido      |
| 2    | Obtener datos del usuario        | UserService         | Usuario existe     |
| 3    | Verificar preferencias           | NotificationService | Email habilitado   |
| 4    | Verificar no está en unsubscribe | NotificationService | No bloqueado       |
| 5    | Obtener template                 | NotificationService | Template activo    |
| 6    | Validar campos requeridos        | NotificationService | Campos completos   |
| 7    | Llamar SendGrid API              | SendGrid            | Request válido     |
| 8    | Guardar mensaje en DB            | NotificationService | Mensaje guardado   |
| 9    | SendGrid procesa y envía         | SendGrid            | Email enviado      |
| 10   | Webhook reporta delivery         | SendGrid Webhook    | Status actualizado |

```csharp
public class EmailService : IEmailService
{
    private readonly SendGridClient _client;
    private readonly IEmailRepository _repository;
    private readonly ITemplateRepository _templateRepository;

    public async Task<SendEmailResult> SendTemplateEmailAsync(
        string templateSlug,
        string toEmail,
        string? toName,
        Dictionary<string, object> templateData,
        Guid? userId = null,
        CancellationToken ct = default)
    {
        // 1. Check unsubscribe
        if (await IsUnsubscribedAsync(toEmail, ct))
        {
            _logger.LogInformation("Email {Email} is unsubscribed", toEmail);
            return SendEmailResult.Skipped("User unsubscribed");
        }

        // 2. Get template
        var template = await _templateRepository.GetBySlugAsync(templateSlug, ct);
        if (template == null || !template.IsActive)
            throw new InvalidOperationException($"Template {templateSlug} not found or inactive");

        // 3. Validate required fields
        var requiredFields = JsonSerializer.Deserialize<List<string>>(template.RequiredFields);
        foreach (var field in requiredFields!)
        {
            if (!templateData.ContainsKey(field))
                throw new ArgumentException($"Missing required field: {field}");
        }

        // 4. Build message
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_config.FromEmail, _config.FromName),
            Subject = template.Subject,
            TemplateId = template.SendGridTemplateId
        };

        msg.AddTo(new EmailAddress(toEmail, toName));
        msg.SetTemplateData(templateData);

        // Add tracking
        msg.SetClickTracking(true, true);
        msg.SetOpenTracking(true);

        // Add unsubscribe
        var unsubscribeUrl = $"https://okla.com.do/unsubscribe?email={Uri.EscapeDataString(toEmail)}";
        msg.AddHeader("List-Unsubscribe", $"<{unsubscribeUrl}>");

        // 5. Send via SendGrid
        var response = await _client.SendEmailAsync(msg, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync(ct);
            throw new EmailSendException($"SendGrid error: {response.StatusCode} - {body}");
        }

        // 6. Get message ID from headers
        var messageId = response.Headers
            .GetValues("X-Message-Id")
            .FirstOrDefault() ?? Guid.NewGuid().ToString();

        // 7. Store message
        var emailMessage = new EmailMessage
        {
            MessageId = messageId,
            ToEmail = toEmail,
            ToName = toName,
            UserId = userId,
            FromEmail = _config.FromEmail,
            FromName = _config.FromName,
            Subject = template.Subject,
            Type = template.Type,
            TemplateId = template.SendGridTemplateId,
            TemplateData = JsonSerializer.Serialize(templateData),
            Status = EmailStatus.Sent,
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Category = template.Category
        };

        await _repository.AddAsync(emailMessage, ct);

        return SendEmailResult.Success(messageId);
    }
}
```

### 5.2 EMAIL-002: Procesar Webhook de SendGrid

| Paso | Acción                    | Sistema             | Validación         |
| ---- | ------------------------- | ------------------- | ------------------ |
| 1    | SendGrid envía webhook    | SendGrid            | Event fired        |
| 2    | Verificar firma           | NotificationService | Signature válida   |
| 3    | Parsear eventos           | NotificationService | JSON válido        |
| 4    | Para cada evento          | Loop                | -                  |
| 5    | Buscar mensaje por ID     | NotificationService | Mensaje existe     |
| 6    | Actualizar status         | NotificationService | Status actualizado |
| 7    | Si bounce, procesar       | NotificationService | Bounce handling    |
| 8    | Si unsubscribe, registrar | NotificationService | Unsubscribe creado |

```csharp
[HttpPost("webhooks/sendgrid")]
public async Task<IActionResult> HandleSendGridWebhook(
    [FromBody] List<SendGridEvent> events)
{
    // Verify signature (middleware)

    foreach (var evt in events)
    {
        var message = await _repository.GetByMessageIdAsync(evt.SgMessageId);
        if (message == null) continue;

        switch (evt.Event)
        {
            case "delivered":
                message.Status = EmailStatus.Delivered;
                message.DeliveredAt = DateTimeOffset.FromUnixTimeSeconds(evt.Timestamp).UtcDateTime;
                break;

            case "open":
                message.Status = EmailStatus.Opened;
                message.OpenedAt ??= DateTimeOffset.FromUnixTimeSeconds(evt.Timestamp).UtcDateTime;
                break;

            case "click":
                message.Status = EmailStatus.Clicked;
                message.ClickedAt ??= DateTimeOffset.FromUnixTimeSeconds(evt.Timestamp).UtcDateTime;
                break;

            case "bounce":
                message.Status = EmailStatus.Bounced;
                message.BouncedAt = DateTimeOffset.FromUnixTimeSeconds(evt.Timestamp).UtcDateTime;
                message.BounceReason = evt.Reason;

                // Handle bounce - add to suppression list if hard bounce
                if (evt.BounceType == "hard")
                {
                    await AddToSuppressionListAsync(message.ToEmail);
                }
                break;

            case "spamreport":
                message.Status = EmailStatus.SpamReport;
                await AddToUnsubscribeAsync(message.ToEmail, UnsubscribeScope.All, "spam_report");
                break;

            case "unsubscribe":
                message.Status = EmailStatus.Unsubscribed;
                await AddToUnsubscribeAsync(message.ToEmail, UnsubscribeScope.Marketing, "user_request");
                break;
        }

        await _repository.UpdateAsync(message);
    }

    return Ok();
}
```

### 5.3 EMAIL-003: Enviar Email Batch (Marketing)

| Paso | Acción                    | Sistema             | Validación        |
| ---- | ------------------------- | ------------------- | ----------------- |
| 1    | Admin crea campaña        | Frontend Admin      | Campaña válida    |
| 2    | Selecciona audiencia      | Frontend Admin      | Segmento definido |
| 3    | Obtener lista de usuarios | UserService         | Lista obtenida    |
| 4    | Filtrar unsubscribes      | NotificationService | Lista filtrada    |
| 5    | Dividir en batches (1000) | NotificationService | Batches creados   |
| 6    | Encolar cada batch        | RabbitMQ            | Jobs encolados    |
| 7    | Worker procesa batch      | Worker              | Emails enviados   |
| 8    | Reportar progreso         | NotificationService | Progress updated  |

---

## 6. Reglas de Negocio

| Código    | Regla                             | Validación                               |
| --------- | --------------------------------- | ---------------------------------------- |
| EMAIL-R01 | Transaccionales siempre se envían | Type == Transactional → skip unsub check |
| EMAIL-R02 | Marketing respeta unsubscribe     | Check UnsubscribeScope                   |
| EMAIL-R03 | Hard bounce → suppression list    | BounceType == hard                       |
| EMAIL-R04 | Spam report → global unsub        | Immediate unsubscribe                    |
| EMAIL-R05 | Rate limit: 100 emails/segundo    | SendGrid limit                           |
| EMAIL-R06 | Max 1 marketing/semana            | Check last marketing email               |

---

## 7. Códigos de Error

| Código      | HTTP | Mensaje                   | Causa               |
| ----------- | ---- | ------------------------- | ------------------- |
| `EMAIL_001` | 400  | Invalid email address     | Email no válido     |
| `EMAIL_002` | 404  | Template not found        | Template no existe  |
| `EMAIL_003` | 400  | Missing required fields   | Campos faltantes    |
| `EMAIL_004` | 400  | User unsubscribed         | Usuario canceló     |
| `EMAIL_005` | 400  | Email in suppression list | Hard bounce previo  |
| `EMAIL_006` | 500  | SendGrid error            | Error del proveedor |
| `EMAIL_007` | 429  | Rate limit exceeded       | Muchos emails       |

---

## 8. Eventos RabbitMQ

| Evento                   | Exchange       | Descripción     |
| ------------------------ | -------------- | --------------- |
| `EmailSentEvent`         | `email.events` | Email enviado   |
| `EmailDeliveredEvent`    | `email.events` | Email entregado |
| `EmailOpenedEvent`       | `email.events` | Email abierto   |
| `EmailClickedEvent`      | `email.events` | Link clickeado  |
| `EmailBouncedEvent`      | `email.events` | Email rebotado  |
| `EmailUnsubscribedEvent` | `email.events` | Usuario canceló |

---

## 9. Configuración

```json
{
  "Email": {
    "Provider": "SendGrid",
    "ApiKey": "${SENDGRID_API_KEY}",
    "FromEmail": "hola@okla.com.do",
    "FromName": "OKLA",
    "ReplyTo": "soporte@okla.com.do",
    "WebhookSigningKey": "${SENDGRID_WEBHOOK_KEY}",
    "Templates": {
      "BaseUrl": "https://okla.com.do"
    },
    "RateLimits": {
      "PerSecond": 100,
      "MarketingPerWeekPerUser": 1
    }
  }
}
```

---

## 10. Métricas Prometheus

```
# Emails enviados
emails_sent_total{type="...", template="...", status="..."}

# Delivery rate
email_delivery_rate

# Open rate
email_open_rate

# Click rate
email_click_rate

# Bounce rate
email_bounce_rate{type="hard|soft"}

# Unsubscribe rate
email_unsubscribe_rate
```

---

## 📚 Referencias

- [SendGrid API](https://docs.sendgrid.com/api-reference/how-to-use-the-sendgrid-v3-api) - API Documentation
- [SendGrid Dynamic Templates](https://docs.sendgrid.com/ui/sending-email/how-to-send-an-email-with-dynamic-templates) - Templates
- [01-whatsapp-integration.md](01-whatsapp-integration.md) - WhatsApp
- [02-sms-integration.md](02-sms-integration.md) - SMS
