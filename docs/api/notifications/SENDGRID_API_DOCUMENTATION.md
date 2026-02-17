# 📧 SendGrid Email API - Documentación Técnica

**API Provider:** SendGrid (Twilio Inc.)  
**Versión:** v3  
**Tipo:** Transactional Email Service  
**Status en OKLA:** ✅ En Producción  
**Última actualización:** Enero 15, 2026

---

## 📋 Descripción General

**SendGrid** es la plataforma de email transaccional utilizada en OKLA para:

- Confirmaciones de registro
- Reset de contraseña
- Notificaciones de vehículos
- Invoices de pagos
- Alertas de precio
- Comunicaciones con dealers

**¿Por qué SendGrid?**

- ✅ **Delivered Rate:** ~99.5% (mejor en RD)
- ✅ **Templating avanzado** (Handlebars, variables dinámicas)
- ✅ **A/B testing** integrado
- ✅ **Webhooks** para tracking de opens/clicks
- ✅ **API simple y bien documentada**
- ✅ **Plan gratuito:** 100 emails/día

---

## 🔑 Autenticación

### API Key

```bash
# Obtener en: https://app.sendgrid.com/settings/api_keys

SENDGRID_API_KEY=SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

### En appsettings.json

```json
{
  "NotificationSettings": {
    "SendGrid": {
      "ApiKey": "${SENDGRID_API_KEY}",
      "FromEmail": "noreply@okla.com.do",
      "FromName": "OKLA Marketplace"
    }
  }
}
```

### En Kubernetes Secrets

```bash
kubectl create secret generic sendgrid-api-key \
  --from-literal=api-key="SG.xxxxx" \
  -n okla
```

---

## 🔌 Endpoints Principales

### Enviar Email Simple

```
POST https://api.sendgrid.com/v3/mail/send
```

**Headers:**

```
Authorization: Bearer {SENDGRID_API_KEY}
Content-Type: application/json
```

**Body:**

```json
{
  "personalizations": [
    {
      "to": [
        {
          "email": "recipient@example.com",
          "name": "John Doe"
        }
      ],
      "subject": "Confirmación de Registro en OKLA",
      "substitutions": {
        "-username-": "johndoe",
        "-confirmUrl-": "https://okla.com.do/confirm/abc123"
      }
    }
  ],
  "from": {
    "email": "noreply@okla.com.do",
    "name": "OKLA Marketplace"
  },
  "reply_to": {
    "email": "support@okla.com.do"
  },
  "template_id": "d-1234567890abcdef",
  "tracking_settings": {
    "open": {
      "enable": true
    },
    "click": {
      "enable": true
    }
  }
}
```

**Response (200 OK):**

```json
{}
```

---

### Enviar Email con Template

```
POST https://api.sendgrid.com/v3/mail/send
```

**Body:**

```json
{
  "personalizations": [
    {
      "to": [
        {
          "email": "dealer@example.com",
          "name": "Juan Dealer"
        }
      ],
      "dynamic_template_data": {
        "dealerName": "Juan Dealer",
        "planName": "Pro",
        "monthlyPrice": "$103",
        "billingDate": "2026-02-15",
        "dashboardUrl": "https://okla.com.do/dealer/dashboard"
      }
    }
  ],
  "from": {
    "email": "billing@okla.com.do",
    "name": "OKLA Billing"
  },
  "template_id": "d-invoice-template-123",
  "reply_to": {
    "email": "billing@okla.com.do"
  }
}
```

### Obtener Estadísticas

```
GET https://api.sendgrid.com/v3/stats?start_date=2026-01-01&end_date=2026-01-15&aggregated_by=day
```

**Response:**

```json
[
  {
    "date": "2026-01-15",
    "stats": [
      {
        "metrics": {
          "blocks": 0,
          "bounce": 0,
          "bounces": 0,
          "clicks": 45,
          "deferred": 0,
          "delivered": 1250,
          "drops": 0,
          "invalid_emails": 0,
          "opens": 523,
          "processed": 1250,
          "requests": 1250,
          "spam_reports": 2,
          "unique_clicks": 38,
          "unique_opens": 287,
          "unsubscribe_drops": 0,
          "unsubscribes": 3
        }
      }
    ]
  }
]
```

---

## 💻 Implementación en C#/.NET

### Instalación del paquete

```bash
dotnet add package SendGrid
```

### SendGridEmailService.cs

```csharp
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Services;

public class SendGridEmailService : IEmailService
{
    private readonly SendGridClient _client;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(
        string apiKey,
        string fromEmail,
        string fromName,
        ILogger<SendGridEmailService> logger)
    {
        _client = new SendGridClient(apiKey);
        _fromEmail = fromEmail;
        _fromName = fromName;
        _logger = logger;
    }

    // ✅ Enviar email simple
    public async Task<Result<string>> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        CancellationToken ct = default)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail, toName);
            var msg = new SendGridMessage()
            {
                From = from,
                Subject = subject,
                HtmlContent = htmlContent,
                ReplyToList = new List<EmailAddress>
                {
                    new EmailAddress("support@okla.com.do", "Support OKLA")
                }
            };

            msg.AddTo(to);

            // Tracking
            msg.TrackingSettings = new TrackingSettings
            {
                OpenTracking = new OpenTracking { Enable = true },
                ClickTracking = new ClickTracking { Enable = true }
            };

            var response = await _client.SendEmailAsync(msg, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                var messageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault();
                _logger.LogInformation($"Email sent to {toEmail}. MessageId: {messageId}");
                return Result<string>.Success(messageId ?? Guid.NewGuid().ToString());
            }

            var errorBody = await response.Body.Content.ReadAsStringAsync(ct);
            _logger.LogError($"SendGrid error: {response.StatusCode} - {errorBody}");
            return Result<string>.Failure($"Failed to send email: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending email to {Email}", toEmail);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Enviar con template dinámico
    public async Task<Result<string>> SendTemplateEmailAsync(
        string toEmail,
        string toName,
        string templateId,
        Dictionary<string, string> templateData,
        CancellationToken ct = default)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail, toName);

            var msg = new SendGridMessage()
            {
                From = from,
                TemplateId = templateId
            };

            msg.AddTo(to);

            // Agregar datos dinámicos al template
            foreach (var kvp in templateData)
            {
                msg.AddSubstitution($"-{kvp.Key}-", kvp.Value);
            }

            // Tracking
            msg.TrackingSettings = new TrackingSettings
            {
                OpenTracking = new OpenTracking { Enable = true },
                ClickTracking = new ClickTracking { Enable = true }
            };

            var response = await _client.SendEmailAsync(msg, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                var messageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault();
                _logger.LogInformation($"Template email sent to {toEmail}. MessageId: {messageId}");
                return Result<string>.Success(messageId ?? Guid.NewGuid().ToString());
            }

            return Result<string>.Failure($"Failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending template email");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Enviar email con adjuntos
    public async Task<Result<string>> SendEmailWithAttachmentAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        Stream fileStream,
        string fileName,
        CancellationToken ct = default)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail, toName);
            var msg = new SendGridMessage()
            {
                From = from,
                Subject = subject,
                HtmlContent = htmlContent
            };

            msg.AddTo(to);

            // Leer archivo como base64
            var bytes = new byte[fileStream.Length];
            await fileStream.ReadAsync(bytes, 0, (int)fileStream.Length, ct);
            var base64 = Convert.ToBase64String(bytes);

            // Agregar adjunto
            var mimeType = GetMimeType(fileName);
            msg.AddAttachment(fileName, base64, mimeType);

            var response = await _client.SendEmailAsync(msg, ct);

            return response.StatusCode == System.Net.HttpStatusCode.Accepted
                ? Result<string>.Success("Email sent")
                : Result<string>.Failure($"Failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending email with attachment");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Obtener estadísticas
    public async Task<Result<EmailStats>> GetStatsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetStatsAsync(
                startDate,
                endDate,
                null,
                null,
                ct);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await response.Body.Content.ReadAsStringAsync(ct);
                var stats = JsonSerializer.Deserialize<List<StatsData>>(content);

                // Agregar estadísticas
                var totalStats = new EmailStats
                {
                    TotalRequests = stats?.Sum(s => s.Stats?[0]?.Metrics?.Requests ?? 0) ?? 0,
                    TotalDelivered = stats?.Sum(s => s.Stats?[0]?.Metrics?.Delivered ?? 0) ?? 0,
                    TotalBounces = stats?.Sum(s => s.Stats?[0]?.Metrics?.Bounces ?? 0) ?? 0,
                    TotalOpens = stats?.Sum(s => s.Stats?[0]?.Metrics?.UniqueOpens ?? 0) ?? 0,
                    TotalClicks = stats?.Sum(s => s.Stats?[0]?.Metrics?.UniqueClicks ?? 0) ?? 0
                };

                return Result<EmailStats>.Success(totalStats);
            }

            return Result<EmailStats>.Failure("Failed to fetch stats");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception fetching email stats");
            return Result<EmailStats>.Failure($"Error: {ex.Message}");
        }
    }

    private string GetMimeType(string fileName)
    {
        return fileName.EndsWith(".pdf") ? "application/pdf"
            : fileName.EndsWith(".xlsx") ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            : fileName.EndsWith(".docx") ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            : "application/octet-stream";
    }
}

// DTOs
public class EmailStats
{
    public long TotalRequests { get; set; }
    public long TotalDelivered { get; set; }
    public long TotalBounces { get; set; }
    public long TotalOpens { get; set; }
    public long TotalClicks { get; set; }
    public double OpenRate => TotalRequests > 0 ? (double)TotalOpens / TotalRequests : 0;
    public double ClickRate => TotalRequests > 0 ? (double)TotalClicks / TotalRequests : 0;
}
```

---

## 🎯 Casos de Uso en OKLA

### 1. Confirmación de Registro

```csharp
var templateData = new Dictionary<string, string>
{
    { "username", "johndoe" },
    { "confirmUrl", "https://okla.com.do/confirm/abc123" },
    { "expirationHours", "24" }
};

await _emailService.SendTemplateEmailAsync(
    "john@example.com",
    "John Doe",
    "d-registration-confirm",
    templateData);
```

### 2. Reset de Contraseña

```csharp
await _emailService.SendTemplateEmailAsync(
    user.Email,
    user.FullName,
    "d-password-reset",
    new Dictionary<string, string>
    {
        { "resetUrl", resetUrl },
        { "expirationMinutes", "30" }
    });
```

### 3. Notificación de Nuevo Vehículo Similar

```csharp
await _emailService.SendTemplateEmailAsync(
    buyer.Email,
    buyer.FullName,
    "d-new-vehicle-alert",
    new Dictionary<string, string>
    {
        { "vehicleTitle", "Toyota Corolla 2020" },
        { "vehicleUrl", "https://okla.com.do/vehicles/123" },
        { "price", "$15,000" },
        { "sellerName", "Juan's Cars" }
    });
```

### 4. Invoice de Suscripción

```csharp
await _emailService.SendTemplateEmailAsync(
    dealer.Email,
    dealer.BusinessName,
    "d-dealer-invoice",
    new Dictionary<string, string>
    {
        { "dealerName", dealer.BusinessName },
        { "planName", "Pro" },
        { "amount", "$103.00" },
        { "billingDate", "2026-02-15" },
        { "invoiceUrl", "https://okla.com.do/invoices/inv-123" }
    });
```

---

## ⚠️ Manejo de Errores

### Errores Comunes

| Código  | Mensaje       | Solución                                                              |
| ------- | ------------- | --------------------------------------------------------------------- |
| **401** | Unauthorized  | API key inválido o expirado. Regenerar en SendGrid dashboard          |
| **403** | Forbidden     | IP no autorizada. Agregar IP a whitelist en SendGrid                  |
| **400** | Invalid email | Email destinatario inválido. Validar formato                          |
| **429** | Rate limited  | Demasiadas requests. Esperar y reintentar                             |
| **500** | Server error  | Error temporal de SendGrid. Implementar retry con exponential backoff |

### Implementación con Polly Retry

```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .OrResult<Response<T>>(r =>
        (int)r.StatusCode >= 500 ||
        (int)r.StatusCode == 429)
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt =>
            TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            _logger.LogWarning(
                $"Retry {retryCount} after {timespan.TotalSeconds}s");
        });

var response = await retryPolicy.ExecuteAsync(
    () => _client.SendEmailAsync(message));
```

---

## 🔐 Seguridad y Best Practices

### ✅ Do's

- ✅ **Usar variables de entorno** para API keys
- ✅ **Validar emails** antes de enviar
- ✅ **Implementar rate limiting** (max 100 emails/segundo)
- ✅ **Usar templates** en lugar de HTML hardcodeado
- ✅ **Monitorear bounce rates** (meta: <0.5%)
- ✅ **Respetar preferencias** de unsubscribe
- ✅ **Incluir link de unsubscribe** en todos los emails

### ❌ Don'ts

- ❌ **NO guardar API keys en código**
- ❌ **NO enviar mass emails** sin template
- ❌ **NO ignorar bounces** y unsubscribes
- ❌ **NO usar generic senders** (no-reply@xxx)
- ❌ **NO enviar sin tracking** habilitado

---

## 📊 Monitoreo y Métricas

### KPIs en OKLA

```csharp
public class EmailMetrics
{
    public double DeliveryRate { get; set; }      // Meta: >99%
    public double OpenRate { get; set; }          // Meta: >25%
    public double ClickRate { get; set; }         // Meta: >5%
    public double UnsubscribeRate { get; set; }   // Meta: <0.5%
    public double BounceRate { get; set; }        // Meta: <0.5%
    public double SpamComplaintRate { get; set; } // Meta: <0.1%
}
```

### Dashboard en SendGrid

```
https://app.sendgrid.com/statistics
```

**Monitorear:**

- Delivered vs Dropped
- Opens por template
- Clicks vs campaign
- Bounce reasons
- Spam reports

---

## 🚀 Webhook Events

### Configurar Webhooks

```
POST https://api.sendgrid.com/v3/user/webhooks/event/settings
```

**Body:**

```json
{
  "enabled": true,
  "url": "https://okla.com.do/api/webhooks/sendgrid",
  "group_resubscribe": true,
  "delivered": true,
  "group_unsubscribe": true,
  "spamreport": true,
  "bounce": true,
  "deferred": true,
  "unsubscribe": true,
  "processed": true,
  "open": true,
  "click": true,
  "dropped": false
}
```

### Handler del Webhook

```csharp
[HttpPost("webhooks/sendgrid")]
public async Task<IActionResult> HandleSendGridWebhook(
    [FromBody] List<SendGridEvent> events)
{
    foreach (var evt in events)
    {
        switch (evt.EventType)
        {
            case "delivered":
                await _notificationService.MarkAsDeliveredAsync(
                    evt.MessageId, evt.Timestamp);
                break;
            case "opened":
                await _notificationService.LogOpenAsync(
                    evt.MessageId, evt.Timestamp);
                break;
            case "click":
                await _notificationService.LogClickAsync(
                    evt.MessageId, evt.Url, evt.Timestamp);
                break;
            case "bounce":
                await _notificationService.MarkAsBounceAsync(
                    evt.Email, evt.BounceType);
                break;
            case "unsubscribe":
                await _userService.UnsubscribeAsync(evt.Email);
                break;
        }
    }

    return Ok();
}
```

---

## 💰 Costos

| Plan           | Emails/Mes       | Costo   | Características             |
| -------------- | ---------------- | ------- | --------------------------- |
| **Free**       | 100/día (3K/mes) | $0      | Basic API, 1 sender         |
| **Pro**        | Ilimitados       | $95/mes | Advanced features, webhooks |
| **Enterprise** | Ilimitados       | Custom  | Dedicated IP, 24/7 support  |

**Costo OKLA (Enero 2026):** $0 (plan free, bajísimo volumen)  
**Costo proyectado Q4 2026:** $95/mes (volumen de 100K+ emails)

---

## 📖 Documentación Externa

- [SendGrid API Docs](https://docs.sendgrid.com/api-reference/mail-send/mail-send)
- [SendGrid C# Library](https://github.com/sendgrid/sendgrid-csharp)
- [Email Templates Best Practices](https://docs.sendgrid.com/ui/sending-email/how-to-send-an-email-with-dynamic-templates)

---

**Mantenido por:** Equipo de Notificaciones OKLA  
**Última revisión:** Enero 15, 2026
