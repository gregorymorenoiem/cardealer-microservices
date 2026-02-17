# 📱 Twilio SMS API - Documentación Técnica

**API Provider:** Twilio Inc.  
**Versión:** v1  
**Tipo:** SMS/Text Messaging Service  
**Status en OKLA:** 🚧 En Configuración  
**Última actualización:** Enero 15, 2026

---

## 📋 Descripción General

**Twilio** es la plataforma utilizada para enviar SMS en OKLA:

- Confirmaciones de código OTP
- Alertas de precio críticas
- Notificaciones urgentes a dealers
- Recordatorios de test drive
- Updates de ofertas

**¿Por qué Twilio?**

- ✅ **Coverage global** (190+ países)
- ✅ **Delivery rate:** ~99.8% en RD
- ✅ **Webhooks** para confirmación de entrega
- ✅ **Inbound SMS** (respuestas automáticas)
- ✅ **Smart Routing** (mejor carrier por número)
- ✅ **Pricing flexible** ($0.0075/SMS en RD)

---

## 🔑 Autenticación

### Account SID & Auth Token

```bash
# Obtener en: https://www.twilio.com/console

TWILIO_ACCOUNT_SID=ACxxxxxxxxxxxxxxxxxxxxxxxxxx
TWILIO_AUTH_TOKEN=your_auth_token_here
TWILIO_PHONE_NUMBER=+18005551234
```

### En appsettings.json

```json
{
  "NotificationSettings": {
    "Twilio": {
      "AccountSid": "${TWILIO_ACCOUNT_SID}",
      "AuthToken": "${TWILIO_AUTH_TOKEN}",
      "FromNumber": "+18005551234"
    }
  }
}
```

### Basic Auth (HTTP)

```
Authorization: Basic {Base64(AccountSid:AuthToken)}
```

---

## 🔌 Endpoints Principales

### Enviar SMS

```
POST https://api.twilio.com/2010-04-01/Accounts/{AccountSid}/Messages
```

**Headers:**

```
Authorization: Basic {Base64(AccountSid:AuthToken)}
Content-Type: application/x-www-form-urlencoded
```

**Body:**

```
To=%2B18005551234
From=%2B14155552671
Body=Your+OTP+code+is%3A+123456
```

**Response (200 OK):**

```json
{
  "sid": "SMxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "date_created": "2026-01-15T10:30:00Z",
  "date_updated": "2026-01-15T10:30:00Z",
  "date_sent": "2026-01-15T10:30:01Z",
  "account_sid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "to": "+1201255501111",
  "from": "+14155552671",
  "messaging_service_sid": null,
  "body": "Your OTP code is: 123456",
  "status": "queued",
  "num_segments": "1",
  "num_media": "0",
  "direction": "outbound-api",
  "api_version": "2010-04-01",
  "price": "-0.00750",
  "price_unit": "USD",
  "error_code": null,
  "error_message": null,
  "uri": "/2010-04-01/Accounts/ACxxxxxxxxxxxxxxxxxxxxxxxxxx/Messages/SMxxxxxxxxxxxxxxxxxxxxxxxxxxx.json"
}
```

### Obtener Mensaje

```
GET https://api.twilio.com/2010-04-01/Accounts/{AccountSid}/Messages/{MessageSid}
```

**Response:**

```json
{
  "sid": "SMxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "status": "delivered",
  "date_sent": "2026-01-15T10:30:01Z"
}
```

---

## 💻 Implementación en C#/.NET

### Instalación del paquete

```bash
dotnet add package Twilio
```

### TwilioSmsService.cs

```csharp
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Services;

public class TwilioSmsService : ISmsService
{
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly string _fromNumber;

    public TwilioSmsService(
        string accountSid,
        string authToken,
        string fromNumber,
        ILogger<TwilioSmsService> logger)
    {
        TwilioClient.Init(accountSid, authToken);
        _fromNumber = fromNumber;
        _logger = logger;
    }

    // ✅ Enviar SMS simple
    public async Task<Result<string>> SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken ct = default)
    {
        try
        {
            // Validar número (básico)
            if (!phoneNumber.StartsWith("+") && !phoneNumber.StartsWith("1"))
            {
                return Result<string>.Failure("Invalid phone number format");
            }

            // Truncar mensaje si es muy largo (SMS = 160 caracteres)
            if (message.Length > 160)
            {
                message = message.Substring(0, 157) + "...";
            }

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromNumber),
                to: new PhoneNumber(phoneNumber));

            _logger.LogInformation(
                $"SMS sent to {phoneNumber}. SID: {messageResource.Sid}, Status: {messageResource.Status}");

            return Result<string>.Success(messageResource.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending SMS to {PhoneNumber}", phoneNumber);
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Enviar SMS con parámetros dinámicos
    public async Task<Result<string>> SendTemplateSmAsync(
        string phoneNumber,
        string template,
        Dictionary<string, string> parameters,
        CancellationToken ct = default)
    {
        try
        {
            var message = BuildMessageFromTemplate(template, parameters);

            if (message.Length > 160)
            {
                return Result<string>.Failure("Message too long after substitution");
            }

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromNumber),
                to: new PhoneNumber(phoneNumber));

            _logger.LogInformation($"Template SMS sent to {phoneNumber}. SID: {messageResource.Sid}");
            return Result<string>.Success(messageResource.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending template SMS");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Obtener estado de SMS
    public async Task<Result<SmsSentStatus>> GetSmsStatusAsync(
        string messageSid,
        CancellationToken ct = default)
    {
        try
        {
            var messageResource = await MessageResource.FetchAsync(messageSid);

            var status = messageResource.Status switch
            {
                MessageResource.StatusEnum.Queued => SmsSentStatus.Queued,
                MessageResource.StatusEnum.Sending => SmsSentStatus.Sending,
                MessageResource.StatusEnum.Sent => SmsSentStatus.Sent,
                MessageResource.StatusEnum.Failed => SmsSentStatus.Failed,
                MessageResource.StatusEnum.Delivered => SmsSentStatus.Delivered,
                MessageResource.StatusEnum.Undelivered => SmsSentStatus.Undelivered,
                MessageResource.StatusEnum.Receiving => SmsSentStatus.Receiving,
                MessageResource.StatusEnum.Received => SmsSentStatus.Received,
                _ => SmsSentStatus.Unknown
            };

            return Result<SmsSentStatus>.Success(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception fetching SMS status");
            return Result<SmsSentStatus>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Generar código OTP
    public string GenerateOtpCode(int length = 6)
    {
        var random = new Random();
        return string.Concat(Enumerable.Range(0, length).Select(_ => random.Next(10)));
    }

    // ✅ Enviar OTP
    public async Task<Result<string>> SendOtpAsync(
        string phoneNumber,
        CancellationToken ct = default)
    {
        try
        {
            var otp = GenerateOtpCode();
            var message = $"Your OTP code is: {otp}. Valid for 10 minutes.";

            var result = await SendSmsAsync(phoneNumber, message, ct);

            if (result.IsSuccess)
            {
                // TODO: Guardar OTP en Redis con expiración de 10 minutos
                await _redis.StringSetAsync(
                    $"otp:{phoneNumber}",
                    otp,
                    TimeSpan.FromMinutes(10));

                _logger.LogInformation($"OTP sent to {phoneNumber}");
                return Result<string>.Success(otp);
            }

            return Result<string>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending OTP");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Verificar OTP
    public async Task<Result<bool>> VerifyOtpAsync(
        string phoneNumber,
        string otp,
        CancellationToken ct = default)
    {
        try
        {
            var storedOtp = await _redis.StringGetAsync($"otp:{phoneNumber}");

            if (!storedOtp.HasValue)
            {
                return Result<bool>.Failure("OTP expired");
            }

            if (storedOtp.ToString() != otp)
            {
                return Result<bool>.Failure("Invalid OTP");
            }

            // Eliminar OTP después de verificar
            await _redis.KeyDeleteAsync($"otp:{phoneNumber}");

            _logger.LogInformation($"OTP verified for {phoneNumber}");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception verifying OTP");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    private string BuildMessageFromTemplate(
        string template,
        Dictionary<string, string> parameters)
    {
        var message = template;
        foreach (var param in parameters)
        {
            message = message.Replace($"{{{param.Key}}}", param.Value);
        }
        return message;
    }
}

// Enums
public enum SmsSentStatus
{
    Queued,
    Sending,
    Sent,
    Failed,
    Delivered,
    Undelivered,
    Receiving,
    Received,
    Unknown
}
```

---

## 🎯 Casos de Uso en OKLA

### 1. OTP para Login

```csharp
var result = await _smsService.SendOtpAsync("+18005551234");
// User recibe SMS con código de 6 dígitos
// Válido por 10 minutos
```

### 2. Alerta de Precio Crítica

```csharp
var vehicle = await _vehicleService.GetAsync(vehicleId);
var buyer = await _userService.GetAsync(buyerId);

await _smsService.SendTemplateSmAsync(
    buyer.PhoneNumber,
    "Price alert: {title} now ${price}. Link: {url}",
    new Dictionary<string, string>
    {
        { "title", vehicle.Title },
        { "price", vehicle.Price.ToString() },
        { "url", $"https://okla.com.do/vehicles/{vehicleId}" }
    });
```

### 3. Recordatorio de Test Drive

```csharp
var testDrive = await _testDriveService.GetAsync(testDriveId);

await _smsService.SendSmsAsync(
    buyer.PhoneNumber,
    $"Reminder: Test drive at {testDrive.DateTime:g} with {testDrive.DealerName}. Reply CONFIRM to confirm.");
```

### 4. Notificación Urgente a Dealer

```csharp
await _smsService.SendSmsAsync(
    dealer.PhoneNumber,
    $"⚠️ NEW LEAD: {buyer.Name} interested in {vehicle.Title}. Check dashboard for details.");
```

---

## 🔄 Webhooks para Status Updates

### Configurar Webhook en Twilio

```
POST https://www.twilio.com/console/sms/webhooks
```

**URL:** `https://okla.com.do/api/webhooks/twilio`

**Body para Status Updates:**

```json
{
  "MessageSid": "SMxxxxxxxxxxxxxxxxx",
  "MessageStatus": "delivered",
  "To": "+18005551234",
  "From": "+14155552671"
}
```

### Handler

```csharp
[HttpPost("webhooks/twilio")]
public async Task<IActionResult> HandleTwilioWebhook(
    [FromForm] string messageSid,
    [FromForm] string messageStatus,
    [FromForm] string to)
{
    switch (messageStatus)
    {
        case "delivered":
            await _notificationService.MarkSmsAsDeliveredAsync(messageSid);
            break;
        case "failed":
        case "undelivered":
            await _notificationService.MarkSmsAsFailedAsync(messageSid);
            break;
        case "received":
            await _smsService.HandleInboundSmsAsync(messageSid, to);
            break;
    }

    return Ok();
}
```

---

## ⚠️ Manejo de Errores

| Código    | Mensaje                        | Solución                                |
| --------- | ------------------------------ | --------------------------------------- |
| **20001** | Invalid parameter              | Verificar formato de número y mensaje   |
| **20003** | Authentication error           | Account SID o Auth Token inválido       |
| **20005** | Invalid phone number           | Número no válido (agregar country code) |
| **20006** | The SMS message was not sent   | No crédito o número bloqueado           |
| **20007** | The message length is too long | Mensaje >1600 caracteres                |
| **30007** | Destination blocked            | Número en lista negra de Twilio         |

---

## 💰 Costos

| Región                 | Costo/SMS   |
| ---------------------- | ----------- |
| **Dominican Republic** | $0.0075     |
| **USA/Canada**         | $0.0075     |
| **International**      | $0.01-$0.20 |

**Costo OKLA (Enero 2026):** ~$10/mes (1,300 SMS)  
**Proyectado Q4 2026:** ~$300/mes (40K SMS)

---

## 🔐 Seguridad y Best Practices

### ✅ Do's

- ✅ **Validar números** antes de enviar
- ✅ **Implementar rate limiting** (max 1 SMS/usuario/minuto)
- ✅ **Usar OTP para auth** (nunca guardar passwords en SMS)
- ✅ **Respetar opt-out** de usuarios
- ✅ **Monitorear bounce rate** (<5%)
- ✅ **Usar short URLs** para messages
- ✅ **Incluir unsubscribe** info

### ❌ Don'ts

- ❌ **NO spam SMS** (solo transaccionales)
- ❌ **NO guardar credenciales** en código
- ❌ **NO enviar sin consent** del usuario
- ❌ **NO ignorar opt-outs**

---

## 📊 Monitoreo y Métricas

```csharp
public class SmsMetrics
{
    public long TotalSent { get; set; }
    public long TotalDelivered { get; set; }
    public long TotalFailed { get; set; }
    public double DeliveryRate => TotalSent > 0 ? (double)TotalDelivered / TotalSent : 0;
    public double ErrorRate => TotalSent > 0 ? (double)TotalFailed / TotalSent : 0;
    public decimal Cost => TotalSent * 0.0075m; // RD pricing
}
```

---

**Mantenido por:** Notification Service Team  
**Última revisión:** Enero 15, 2026
