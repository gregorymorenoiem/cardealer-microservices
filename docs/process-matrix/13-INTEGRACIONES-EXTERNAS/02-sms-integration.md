# 📱 SMS Integration - Integración SMS - Matriz de Procesos

> **Servicio:** NotificationService  
> **Proveedor:** Twilio SMS API  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente  | Total | Implementado | Pendiente | Estado |
| ----------- | ----- | ------------ | --------- | ------ |
| Controllers | 1     | 1            | 0         | 🟢     |
| SMS-SEND-\* | 4     | 4            | 0         | 🟢     |
| SMS-OTP-\*  | 3     | 3            | 0         | 🟢     |
| SMS-TPL-\*  | 3     | 3            | 0         | 🟢     |
| SMS-WH-\*   | 2     | 2            | 0         | 🟢     |
| Tests       | 8     | 8            | 0         | ✅     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 1. Información General

### 1.1 Descripción

Sistema de envío de mensajes SMS para notificaciones críticas, verificación de teléfono, OTP (One-Time Password), y alertas urgentes. Complementa las notificaciones por email y push.

### 1.2 Casos de Uso

| Caso                   | Prioridad | Ejemplo                         |
| ---------------------- | --------- | ------------------------------- |
| **OTP/2FA**            | Crítica   | Código de verificación          |
| **Phone Verification** | Crítica   | Verificar número al registrarse |
| **Transaction Alerts** | Alta      | Pago procesado, refund          |
| **Security Alerts**    | Alta      | Login sospechoso                |
| **Lead Notifications** | Media     | Nuevo lead (si no WhatsApp)     |
| **Reminders**          | Baja      | Recordatorio de cita            |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     SMS Integration Architecture                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   OKLA Backend                                                          │
│   ┌──────────────────────────────────────────────────────────────────┐  │
│   │                                                                   │  │
│   │  ┌─────────────┐     ┌─────────────────┐     ┌───────────────┐   │  │
│   │  │ AuthService │ ──▶ │NotificationService│ ──▶│ SMS Provider  │   │  │
│   │  │   (OTP)     │     │                 │     │   (Twilio)    │   │  │
│   │  └─────────────┘     └─────────────────┘     └───────┬───────┘   │  │
│   │                                                       │           │  │
│   │  ┌─────────────┐                                     │           │  │
│   │  │BillingService│ ──────────────────────────────────▶│           │  │
│   │  │ (Alerts)    │                                     │           │  │
│   │  └─────────────┘                                     │           │  │
│   │                                                       │           │  │
│   └──────────────────────────────────────────────────────┼───────────┘  │
│                                                           │              │
│                                                           ▼              │
│                                                    ┌─────────────┐      │
│                                                    │   Twilio    │      │
│                                                    │   API       │      │
│                                                    └──────┬──────┘      │
│                                                           │              │
│                                              ┌────────────┼────────────┐│
│                                              ▼            ▼            ▼│
│                                         ┌────────┐  ┌────────┐  ┌──────┘│
│                                         │Claro RD│  │Altice  │  │Viva  ││
│                                         └────────┘  └────────┘  └──────┘│
│                                                           │              │
│                                                           ▼              │
│                                                    ┌─────────────┐      │
│                                                    │  User Phone │      │
│                                                    └─────────────┘      │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

| Método | Endpoint                        | Descripción                      | Auth    |
| ------ | ------------------------------- | -------------------------------- | ------- |
| `POST` | `/api/sms/send`                 | Enviar SMS                       | Service |
| `POST` | `/api/sms/send-otp`             | Enviar OTP                       | Service |
| `POST` | `/api/sms/verify-otp`           | Verificar OTP                    | Service |
| `POST` | `/api/sms/verify-phone`         | Iniciar verificación de teléfono | User    |
| `POST` | `/api/sms/verify-phone/confirm` | Confirmar código                 | User    |
| `GET`  | `/api/sms/status/{messageId}`   | Estado del mensaje               | Admin   |
| `GET`  | `/api/sms/stats`                | Estadísticas de envío            | Admin   |
| `POST` | `/api/webhooks/sms`             | Webhook de delivery              | Twilio  |

---

## 3. Entidades

### 3.1 SmsMessage

```csharp
public class SmsMessage
{
    public Guid Id { get; set; }
    public string MessageSid { get; set; } = string.Empty; // Twilio SID

    public string PhoneNumber { get; set; } = string.Empty; // E.164: +18091234567
    public Guid? UserId { get; set; }

    public SmsType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Segments { get; set; } // Número de segmentos SMS

    public SmsStatus Status { get; set; }
    public decimal? Price { get; set; }
    public string? PriceUnit { get; set; }

    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

public enum SmsType
{
    Otp,
    PhoneVerification,
    TransactionAlert,
    SecurityAlert,
    LeadNotification,
    Reminder,
    Marketing
}

public enum SmsStatus
{
    Queued,
    Sending,
    Sent,
    Delivered,
    Undelivered,
    Failed
}
```

### 3.2 OtpVerification

```csharp
public class OtpVerification
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid? UserId { get; set; }

    public string Code { get; set; } = string.Empty; // 6 dígitos
    public string CodeHash { get; set; } = string.Empty; // SHA256

    public OtpPurpose Purpose { get; set; }
    public int Attempts { get; set; }
    public int MaxAttempts { get; set; } = 3;

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; } // +5 minutos
    public DateTime? VerifiedAt { get; set; }
    public bool IsUsed { get; set; }
}

public enum OtpPurpose
{
    Login,
    PhoneVerification,
    PasswordReset,
    TransactionConfirmation,
    TwoFactorAuth
}
```

### 3.3 PhoneVerification

```csharp
public class PhoneVerification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;

    public string VerificationSid { get; set; } = string.Empty; // Twilio Verify SID
    public VerificationStatus Status { get; set; }

    public int Attempts { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public enum VerificationStatus
{
    Pending,
    Approved,
    Canceled,
    MaxAttemptsReached
}
```

---

## 4. Procesos Detallados

### 4.1 SMS-001: Enviar OTP para Login

| Paso | Acción                         | Sistema             | Validación       |
| ---- | ------------------------------ | ------------------- | ---------------- |
| 1    | Usuario solicita login con OTP | Frontend            | Número válido    |
| 2    | Verificar rate limit           | AuthService         | No bloqueado     |
| 3    | Generar código 6 dígitos       | AuthService         | Código aleatorio |
| 4    | Hash del código                | AuthService         | SHA256           |
| 5    | Guardar OTP en DB              | AuthService         | TTL 5 min        |
| 6    | Enviar SMS                     | NotificationService | Twilio API       |
| 7    | Guardar mensaje                | NotificationService | Mensaje guardado |
| 8    | Usuario recibe SMS             | Carrier             | Entregado        |
| 9    | Usuario ingresa código         | Frontend            | Código ingresado |
| 10   | Verificar código               | AuthService         | Hash match       |
| 11   | Marcar como usado              | AuthService         | IsUsed = true    |
| 12   | Generar JWT                    | AuthService         | Token emitido    |

```csharp
public class OtpService : IOtpService
{
    public async Task<SendOtpResult> SendOtpAsync(
        string phoneNumber,
        OtpPurpose purpose,
        Guid? userId = null,
        CancellationToken ct = default)
    {
        // 1. Validate phone number
        var formattedNumber = PhoneNumberValidator.FormatE164(phoneNumber);

        // 2. Check rate limit (max 5 OTPs per hour)
        var recentCount = await _repository.CountRecentOtpsAsync(formattedNumber, TimeSpan.FromHours(1), ct);
        if (recentCount >= 5)
            throw new RateLimitException("Too many OTP requests. Try again later.");

        // 3. Invalidate any existing OTPs
        await _repository.InvalidateExistingOtpsAsync(formattedNumber, purpose, ct);

        // 4. Generate 6-digit code
        var code = GenerateSecureCode(6);
        var codeHash = HashCode(code);

        // 5. Store OTP
        var otp = new OtpVerification
        {
            PhoneNumber = formattedNumber,
            UserId = userId,
            Code = code, // Solo para debug en dev
            CodeHash = codeHash,
            Purpose = purpose,
            MaxAttempts = 3,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        await _repository.AddAsync(otp, ct);

        // 6. Send SMS
        var message = purpose switch
        {
            OtpPurpose.Login => $"Tu código de acceso OKLA es: {code}. Válido por 5 minutos.",
            OtpPurpose.PhoneVerification => $"Verifica tu teléfono en OKLA: {code}",
            OtpPurpose.PasswordReset => $"Código para restablecer contraseña OKLA: {code}",
            OtpPurpose.TransactionConfirmation => $"Confirma tu transacción OKLA: {code}",
            _ => $"Tu código OKLA: {code}"
        };

        var result = await _smsService.SendAsync(formattedNumber, message, SmsType.Otp, ct);

        return new SendOtpResult(otp.Id, result.MessageId);
    }

    public async Task<VerifyOtpResult> VerifyOtpAsync(
        string phoneNumber,
        string code,
        OtpPurpose purpose,
        CancellationToken ct = default)
    {
        var formattedNumber = PhoneNumberValidator.FormatE164(phoneNumber);

        // Get active OTP
        var otp = await _repository.GetActiveOtpAsync(formattedNumber, purpose, ct);

        if (otp == null)
            return VerifyOtpResult.Failed("No active OTP found");

        if (otp.ExpiresAt < DateTime.UtcNow)
            return VerifyOtpResult.Failed("OTP expired");

        if (otp.Attempts >= otp.MaxAttempts)
            return VerifyOtpResult.Failed("Max attempts reached");

        // Increment attempts
        otp.Attempts++;

        // Verify code
        var codeHash = HashCode(code);
        if (codeHash != otp.CodeHash)
        {
            await _repository.UpdateAsync(otp, ct);
            return VerifyOtpResult.Failed("Invalid code", otp.MaxAttempts - otp.Attempts);
        }

        // Mark as used
        otp.IsUsed = true;
        otp.VerifiedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(otp, ct);

        return VerifyOtpResult.Success(otp.UserId);
    }

    private string GenerateSecureCode(int length)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var number = BitConverter.ToUInt32(bytes, 0) % (uint)Math.Pow(10, length);
        return number.ToString().PadLeft(length, '0');
    }
}
```

### 4.2 SMS-002: Verificación de Teléfono (Twilio Verify)

| Paso | Acción                      | Sistema             | Validación           |
| ---- | --------------------------- | ------------------- | -------------------- |
| 1    | Usuario inicia verificación | Frontend            | Número ingresado     |
| 2    | Llamar Twilio Verify API    | NotificationService | Verification created |
| 3    | Twilio envía código         | Twilio              | SMS enviado          |
| 4    | Usuario recibe código       | Carrier             | SMS entregado        |
| 5    | Usuario ingresa código      | Frontend            | Código ingresado     |
| 6    | Verificar con Twilio        | NotificationService | Status = approved    |
| 7    | Actualizar usuario          | UserService         | PhoneVerified = true |
| 8    | Publicar evento             | RabbitMQ            | PhoneVerifiedEvent   |

```csharp
public class PhoneVerificationService : IPhoneVerificationService
{
    private readonly TwilioRestClient _client;
    private readonly string _verifySid;

    public async Task<StartVerificationResult> StartVerificationAsync(
        Guid userId,
        string phoneNumber,
        CancellationToken ct = default)
    {
        var formattedNumber = PhoneNumberValidator.FormatE164(phoneNumber);

        // Create verification via Twilio Verify
        var verification = await VerificationResource.CreateAsync(
            to: formattedNumber,
            channel: "sms",
            pathServiceSid: _verifySid,
            client: _client
        );

        // Store verification record
        var record = new PhoneVerification
        {
            UserId = userId,
            PhoneNumber = formattedNumber,
            VerificationSid = verification.Sid,
            Status = VerificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(record, ct);

        return new StartVerificationResult(record.Id, verification.Status);
    }

    public async Task<ConfirmVerificationResult> ConfirmVerificationAsync(
        Guid userId,
        string phoneNumber,
        string code,
        CancellationToken ct = default)
    {
        var formattedNumber = PhoneNumberValidator.FormatE164(phoneNumber);

        // Check verification with Twilio
        var check = await VerificationCheckResource.CreateAsync(
            to: formattedNumber,
            code: code,
            pathServiceSid: _verifySid,
            client: _client
        );

        if (check.Status != "approved")
        {
            return ConfirmVerificationResult.Failed("Invalid code");
        }

        // Update verification record
        var record = await _repository.GetByUserAndPhoneAsync(userId, formattedNumber, ct);
        if (record != null)
        {
            record.Status = VerificationStatus.Approved;
            record.VerifiedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(record, ct);
        }

        // Publish event
        await _eventBus.PublishAsync(new PhoneVerifiedEvent
        {
            UserId = userId,
            PhoneNumber = formattedNumber,
            VerifiedAt = DateTime.UtcNow
        }, ct);

        return ConfirmVerificationResult.Success();
    }
}
```

### 4.3 SMS-003: Alerta de Transacción

| Paso | Acción                            | Sistema             | Validación       |
| ---- | --------------------------------- | ------------------- | ---------------- |
| 1    | Pago procesado exitosamente       | BillingService      | Payment complete |
| 2    | Publicar PaymentSuccessEvent      | RabbitMQ            | Evento publicado |
| 3    | Consumer recibe evento            | NotificationService | Evento recibido  |
| 4    | Verificar preferencias de usuario | NotificationService | SMS habilitado   |
| 5    | Formatear mensaje                 | NotificationService | Mensaje creado   |
| 6    | Enviar SMS                        | Twilio              | SMS enviado      |
| 7    | Guardar mensaje                   | NotificationService | Mensaje guardado |

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Transaction Alert Flow                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   BillingService         RabbitMQ         NotificationService           │
│        │                    │                    │                       │
│        │ PaymentSuccessEvent│                    │                       │
│        │ ──────────────────▶│                    │                       │
│        │                    │                    │                       │
│        │                    │  Event consumed    │                       │
│        │                    │ ──────────────────▶│                       │
│        │                    │                    │                       │
│        │                    │                    │ Check prefs           │
│        │                    │                    │ ──────────┐           │
│        │                    │                    │           │           │
│        │                    │                    │ ◀─────────┘           │
│        │                    │                    │                       │
│        │                    │                    │ Send SMS             │
│        │                    │                    │ ─────────────▶ Twilio │
│        │                    │                    │                       │
│        │                    │                    │ Message delivered    │
│        │                    │                    │ ◀─────────────        │
│        │                    │                    │                       │
│                                                                          │
│   SMS Content:                                                          │
│   "OKLA: Pago de RD$2,499.00 procesado exitosamente.                   │
│    Referencia: PAY-123456. Gracias por tu compra."                      │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Templates de SMS

### 5.1 Mensajes Predefinidos

| Template          | Caracteres | Ejemplo                                                                  |
| ----------------- | ---------- | ------------------------------------------------------------------------ |
| `otp_login`       | 68         | "Tu código de acceso OKLA es: 123456. Válido por 5 minutos."             |
| `otp_verify`      | 45         | "Verifica tu teléfono en OKLA: 123456"                                   |
| `payment_success` | 95         | "OKLA: Pago de RD$2,499.00 procesado. Ref: PAY-123456"                   |
| `payment_failed`  | 78         | "OKLA: Tu pago de RD$2,499.00 no fue procesado. Intenta de nuevo."       |
| `new_lead`        | 120        | "OKLA: Juan está interesado en tu Toyota Corolla 2020. Responde pronto!" |
| `security_alert`  | 85         | "OKLA: Nuevo inicio de sesión desde iPhone, Santiago. ¿Fuiste tú?"       |

### 5.2 Segmentos SMS

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     SMS Segmentation                                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   GSM-7 (caracteres estándar):                                          │
│   ├── 1 segmento: 160 caracteres                                        │
│   ├── 2+ segmentos: 153 caracteres cada uno (header de concatenación)  │
│   └── Caracteres especiales (ñ, á, é) usan GSM-7                        │
│                                                                          │
│   UCS-2 (emojis, caracteres especiales):                                │
│   ├── 1 segmento: 70 caracteres                                         │
│   └── 2+ segmentos: 67 caracteres cada uno                             │
│                                                                          │
│   Recomendación OKLA:                                                   │
│   ├── Evitar emojis en SMS                                              │
│   ├── Mantener mensajes < 160 caracteres                                │
│   └── Usar ñ y acentos normalmente (GSM-7)                              │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Rate Limiting

| Contexto                | Límite | Período  |
| ----------------------- | ------ | -------- |
| OTP por número          | 5      | 1 hora   |
| OTP por IP              | 10     | 1 hora   |
| SMS por usuario         | 20     | 24 horas |
| Verificación por número | 3      | 24 horas |
| Global (OKLA)           | 10,000 | 1 hora   |

---

## 7. Reglas de Negocio

| Código  | Regla                                  | Validación                   |
| ------- | -------------------------------------- | ---------------------------- |
| SMS-R01 | OTP expira en 5 minutos                | ExpiresAt = CreatedAt + 5min |
| SMS-R02 | Máximo 3 intentos por OTP              | Attempts < MaxAttempts       |
| SMS-R03 | Solo números dominicanos +1809/829/849 | Prefix válido                |
| SMS-R04 | SMS transaccionales 24/7               | Sin restricción horaria      |
| SMS-R05 | SMS marketing 8am-8pm                  | LocalTime check              |
| SMS-R06 | Opt-out respetado                      | SmsOptOut == false           |

---

## 8. Códigos de Error

| Código    | HTTP | Mensaje              | Causa                    |
| --------- | ---- | -------------------- | ------------------------ |
| `SMS_001` | 400  | Invalid phone number | Número no válido         |
| `SMS_002` | 429  | Rate limit exceeded  | Muchos intentos          |
| `SMS_003` | 400  | OTP expired          | Código expirado          |
| `SMS_004` | 400  | Invalid OTP          | Código incorrecto        |
| `SMS_005` | 400  | Max attempts reached | Muchos intentos fallidos |
| `SMS_006` | 500  | Provider error       | Error de Twilio          |
| `SMS_007` | 400  | User opted out       | Usuario canceló SMS      |

---

## 9. Eventos RabbitMQ

| Evento               | Exchange      | Descripción         |
| -------------------- | ------------- | ------------------- |
| `SmsSentEvent`       | `sms.events`  | SMS enviado         |
| `SmsDeliveredEvent`  | `sms.events`  | SMS entregado       |
| `SmsFailedEvent`     | `sms.events`  | SMS falló           |
| `OtpVerifiedEvent`   | `auth.events` | OTP verificado      |
| `PhoneVerifiedEvent` | `user.events` | Teléfono verificado |

---

## 10. Configuración

```json
{
  "Sms": {
    "Provider": "Twilio",
    "AccountSid": "${TWILIO_ACCOUNT_SID}",
    "AuthToken": "${TWILIO_AUTH_TOKEN}",
    "FromNumber": "+18091234567",
    "VerifyServiceSid": "${TWILIO_VERIFY_SID}",
    "Otp": {
      "Length": 6,
      "ExpirationMinutes": 5,
      "MaxAttempts": 3
    },
    "RateLimits": {
      "OtpPerPhonePerHour": 5,
      "SmsPerUserPerDay": 20
    },
    "AllowedPrefixes": ["+1809", "+1829", "+1849"]
  }
}
```

---

## 11. Métricas Prometheus

```
# SMS enviados
sms_sent_total{type="...", status="..."}

# Costo de SMS
sms_cost_total{currency="USD"}

# OTP verification rate
otp_verification_success_rate

# Delivery rate
sms_delivery_rate

# Latencia de envío
sms_send_duration_seconds
```

---

## 📚 Referencias

- [Twilio SMS API](https://www.twilio.com/docs/sms) - Documentación
- [Twilio Verify](https://www.twilio.com/docs/verify) - Phone verification
- [01-whatsapp-integration.md](01-whatsapp-integration.md) - WhatsApp
- [03-email-providers.md](03-email-providers.md) - Email
