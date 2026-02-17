# 📱 SMS Gateways RD (Claro, Altice, Viva)

**Proveedores:** Claro RD, Altice Dominicana, Viva  
**Uso:** SMS masivos, OTP, notificaciones  
**Prioridad:** ⭐⭐⭐⭐ ALTA (Respaldo de WhatsApp)

---

## 📋 Información General

| Proveedor | API | Costo/SMS | Mejor Para |
|-----------|-----|-----------|------------|
| **Claro Business** | REST | RD$0.75 | Volumen alto |
| **Altice Business** | REST | RD$0.80 | Entregas rápidas |
| **Viva** | SMPP/REST | RD$0.70 | Mejor precio |
| **Twilio** | REST | $0.0785 USD | OTPs internacionales |

---

## 🌐 API Endpoints

### Claro Business SMS

```http
# Base URL
https://api.clarobusiness.do/sms/v1

# Enviar SMS Individual
POST /send
Authorization: Bearer {API_KEY}
Content-Type: application/json

{
  "from": "OKLA",
  "to": "8091234567",
  "message": "Tu código de verificación OKLA es: 123456. Válido por 5 minutos."
}

# Response
{
  "messageId": "msg_abc123",
  "status": "queued",
  "to": "8091234567",
  "credits": 1,
  "timestamp": "2026-01-09T10:30:00Z"
}

# Enviar SMS Masivo (Batch)
POST /send/batch
{
  "from": "OKLA",
  "messages": [
    { "to": "8091234567", "message": "¡Nuevo vehículo que te puede interesar!" },
    { "to": "8291234567", "message": "¡Nuevo vehículo que te puede interesar!" },
    { "to": "8491234567", "message": "¡Nuevo vehículo que te puede interesar!" }
  ]
}

# Response
{
  "batchId": "batch_xyz789",
  "totalMessages": 3,
  "queued": 3,
  "failed": 0,
  "totalCredits": 3
}

# Verificar Estado
GET /status/{messageId}

# Response
{
  "messageId": "msg_abc123",
  "status": "delivered",
  "deliveredAt": "2026-01-09T10:30:05Z",
  "errorCode": null
}

# Webhook de Entrega
POST /your-webhook-url
{
  "event": "delivery",
  "messageId": "msg_abc123",
  "status": "delivered",
  "timestamp": "2026-01-09T10:30:05Z"
}
```

### Altice Business SMS

```http
# Base URL
https://sms.altice.com.do/api/v2

# Enviar SMS
POST /messages
X-API-Key: {API_KEY}
Content-Type: application/json

{
  "sender_id": "OKLA",
  "recipient": "+18091234567",
  "content": "Tu código de verificación es: 654321",
  "type": "transactional"
}

# Enviar Campaña
POST /campaigns
{
  "name": "Alerta Precio Toyota",
  "sender_id": "OKLA",
  "content": "¡El Toyota Corolla que guardaste bajó de precio! Ahora RD$1,400,000. Ver: https://okla.do/v/123",
  "recipients": [
    { "phone": "+18091234567", "variables": { "name": "Juan" } },
    { "phone": "+18291234567", "variables": { "name": "María" } }
  ],
  "schedule": null
}
```

### Twilio (Internacional)

```http
# Base URL
https://api.twilio.com/2010-04-01/Accounts/{ACCOUNT_SID}

# Enviar SMS
POST /Messages.json
Authorization: Basic {base64(account_sid:auth_token)}
Content-Type: application/x-www-form-urlencoded

From=+18001234567&To=+18091234567&Body=Tu código es: 123456

# Response (JSON)
{
  "sid": "SMxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "status": "queued",
  "to": "+18091234567",
  "from": "+18001234567",
  "body": "Tu código es: 123456"
}
```

---

## 💻 Modelos C#

```csharp
namespace CommunicationService.Domain.Entities;

/// <summary>
/// Mensaje SMS
/// </summary>
public record SmsMessage(
    string MessageId,
    string To,
    string From,
    string Body,
    SmsProvider Provider,
    SmsStatus Status,
    int Segments,
    decimal Cost,
    DateTime SentAt,
    DateTime? DeliveredAt,
    string? ErrorCode,
    string? ErrorMessage
);

public enum SmsProvider
{
    Claro,
    Altice,
    Viva,
    Twilio
}

public enum SmsStatus
{
    Pending,
    Queued,
    Sent,
    Delivered,
    Failed,
    Undeliverable
}

/// <summary>
/// Campaña SMS masiva
/// </summary>
public record SmsCampaign(
    Guid CampaignId,
    string Name,
    string MessageTemplate,
    List<SmsRecipient> Recipients,
    SmsProvider Provider,
    CampaignStatus Status,
    int TotalMessages,
    int DeliveredCount,
    int FailedCount,
    decimal TotalCost,
    DateTime? ScheduledAt,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

public record SmsRecipient(
    string PhoneNumber,
    Dictionary<string, string> Variables,
    SmsStatus Status,
    string? MessageId
);

public enum CampaignStatus
{
    Draft,
    Scheduled,
    Running,
    Completed,
    Paused,
    Cancelled
}

/// <summary>
/// Código OTP
/// </summary>
public record OtpVerification(
    string PhoneNumber,
    string Code,
    OtpPurpose Purpose,
    bool IsVerified,
    int AttemptCount,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? VerifiedAt
);

public enum OtpPurpose
{
    Registration,
    Login,
    PasswordReset,
    PhoneVerification,
    TransactionConfirmation
}

/// <summary>
/// Balance de SMS
/// </summary>
public record SmsBalance(
    SmsProvider Provider,
    decimal Credits,
    decimal CostPerSms,
    DateTime LastUpdated
);
```

---

## 🔧 Service Interface

```csharp
namespace CommunicationService.Domain.Interfaces;

public interface ISmsService
{
    /// <summary>
    /// Envía SMS individual
    /// </summary>
    Task<SmsMessage> SendAsync(
        string to, 
        string message, 
        SmsProvider? preferredProvider = null);

    /// <summary>
    /// Envía SMS masivo (campaña)
    /// </summary>
    Task<SmsCampaign> SendBatchAsync(
        string messageTemplate,
        List<SmsRecipient> recipients,
        string? campaignName = null,
        DateTime? scheduleAt = null);

    /// <summary>
    /// Envía código OTP
    /// </summary>
    Task<OtpVerification> SendOtpAsync(
        string phoneNumber, 
        OtpPurpose purpose);

    /// <summary>
    /// Verifica código OTP
    /// </summary>
    Task<bool> VerifyOtpAsync(
        string phoneNumber, 
        string code, 
        OtpPurpose purpose);

    /// <summary>
    /// Obtiene estado del mensaje
    /// </summary>
    Task<SmsMessage?> GetMessageStatusAsync(string messageId);

    /// <summary>
    /// Obtiene balance de créditos
    /// </summary>
    Task<List<SmsBalance>> GetBalancesAsync();

    /// <summary>
    /// Procesa webhook de delivery
    /// </summary>
    Task ProcessDeliveryWebhookAsync(
        SmsProvider provider, 
        string payload);
}
```

---

## 🏗️ Service Implementation

```csharp
namespace CommunicationService.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly ISmsProviderFactory _providerFactory;
    private readonly ISmsRepository _smsRepo;
    private readonly IOtpRepository _otpRepo;
    private readonly ILogger<SmsService> _logger;
    private readonly SmsSettings _settings;

    public SmsService(
        ISmsProviderFactory providerFactory,
        ISmsRepository smsRepo,
        IOtpRepository otpRepo,
        ILogger<SmsService> logger,
        IOptions<SmsSettings> settings)
    {
        _providerFactory = providerFactory;
        _smsRepo = smsRepo;
        _otpRepo = otpRepo;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<SmsMessage> SendAsync(
        string to, 
        string message, 
        SmsProvider? preferredProvider = null)
    {
        var phone = NormalizePhoneNumber(to);
        var provider = preferredProvider ?? SelectOptimalProvider(phone);
        var smsProvider = _providerFactory.GetProvider(provider);

        _logger.LogInformation(
            "Sending SMS via {Provider} to {To}", 
            provider, MaskPhone(phone));

        try
        {
            var result = await smsProvider.SendAsync(phone, message);
            
            var smsMessage = new SmsMessage(
                MessageId: result.MessageId,
                To: phone,
                From: _settings.SenderId,
                Body: message,
                Provider: provider,
                Status: SmsStatus.Queued,
                Segments: CalculateSegments(message),
                Cost: smsProvider.CostPerSms * CalculateSegments(message),
                SentAt: DateTime.UtcNow,
                DeliveredAt: null,
                ErrorCode: null,
                ErrorMessage: null
            );

            await _smsRepo.SaveAsync(smsMessage);
            return smsMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS via {Provider}", provider);
            
            // Fallback a otro proveedor
            if (preferredProvider == null)
            {
                var fallbackProvider = GetFallbackProvider(provider);
                return await SendAsync(to, message, fallbackProvider);
            }
            
            throw;
        }
    }

    public async Task<SmsCampaign> SendBatchAsync(
        string messageTemplate,
        List<SmsRecipient> recipients,
        string? campaignName = null,
        DateTime? scheduleAt = null)
    {
        var campaign = new SmsCampaign(
            CampaignId: Guid.NewGuid(),
            Name: campaignName ?? $"Campaign_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            MessageTemplate: messageTemplate,
            Recipients: recipients,
            Provider: SmsProvider.Claro, // Mejor para volumen
            Status: scheduleAt.HasValue ? CampaignStatus.Scheduled : CampaignStatus.Running,
            TotalMessages: recipients.Count,
            DeliveredCount: 0,
            FailedCount: 0,
            TotalCost: 0,
            ScheduledAt: scheduleAt,
            CreatedAt: DateTime.UtcNow,
            CompletedAt: null
        );

        await _smsRepo.SaveCampaignAsync(campaign);

        if (!scheduleAt.HasValue)
        {
            // Enviar inmediatamente en batches
            await ProcessCampaignAsync(campaign);
        }

        return campaign;
    }

    private async Task ProcessCampaignAsync(SmsCampaign campaign)
    {
        var provider = _providerFactory.GetProvider(campaign.Provider);
        var batchSize = 100;
        var batches = campaign.Recipients
            .Select((r, i) => new { Recipient = r, Index = i })
            .GroupBy(x => x.Index / batchSize)
            .Select(g => g.Select(x => x.Recipient).ToList());

        foreach (var batch in batches)
        {
            var messages = batch.Select(r => new
            {
                r.PhoneNumber,
                Message = ApplyTemplate(campaign.MessageTemplate, r.Variables)
            }).ToList();

            try
            {
                var results = await provider.SendBatchAsync(messages);
                
                foreach (var result in results)
                {
                    await _smsRepo.UpdateRecipientStatusAsync(
                        campaign.CampaignId,
                        result.PhoneNumber,
                        result.Success ? SmsStatus.Queued : SmsStatus.Failed,
                        result.MessageId
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch send failed for campaign {Id}", campaign.CampaignId);
            }

            // Rate limiting
            await Task.Delay(1000);
        }

        await _smsRepo.UpdateCampaignStatusAsync(
            campaign.CampaignId, 
            CampaignStatus.Completed
        );
    }

    public async Task<OtpVerification> SendOtpAsync(
        string phoneNumber, 
        OtpPurpose purpose)
    {
        var phone = NormalizePhoneNumber(phoneNumber);
        var code = GenerateOtpCode();
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.OtpExpirationMinutes);

        var otp = new OtpVerification(
            PhoneNumber: phone,
            Code: HashCode(code), // Guardar hasheado
            Purpose: purpose,
            IsVerified: false,
            AttemptCount: 0,
            CreatedAt: DateTime.UtcNow,
            ExpiresAt: expiresAt,
            VerifiedAt: null
        );

        await _otpRepo.SaveAsync(otp);

        // Enviar SMS con código
        var message = purpose switch
        {
            OtpPurpose.Registration => $"Tu código de registro OKLA es: {code}. Válido por {_settings.OtpExpirationMinutes} minutos.",
            OtpPurpose.Login => $"Tu código de acceso OKLA es: {code}. No lo compartas con nadie.",
            OtpPurpose.PasswordReset => $"Tu código para restablecer contraseña es: {code}. Válido por {_settings.OtpExpirationMinutes} minutos.",
            OtpPurpose.PhoneVerification => $"Verifica tu número OKLA con el código: {code}",
            OtpPurpose.TransactionConfirmation => $"Confirma tu transacción con el código: {code}",
            _ => $"Tu código OKLA es: {code}"
        };

        await SendAsync(phone, message, SmsProvider.Twilio); // OTPs por Twilio (más rápido)

        return otp with { Code = "******" }; // No devolver código
    }

    public async Task<bool> VerifyOtpAsync(
        string phoneNumber, 
        string code, 
        OtpPurpose purpose)
    {
        var phone = NormalizePhoneNumber(phoneNumber);
        var otp = await _otpRepo.GetLatestAsync(phone, purpose);

        if (otp == null)
        {
            _logger.LogWarning("OTP not found for {Phone}", MaskPhone(phone));
            return false;
        }

        if (otp.IsVerified)
        {
            _logger.LogWarning("OTP already used for {Phone}", MaskPhone(phone));
            return false;
        }

        if (DateTime.UtcNow > otp.ExpiresAt)
        {
            _logger.LogWarning("OTP expired for {Phone}", MaskPhone(phone));
            return false;
        }

        if (otp.AttemptCount >= _settings.MaxOtpAttempts)
        {
            _logger.LogWarning("Too many OTP attempts for {Phone}", MaskPhone(phone));
            return false;
        }

        // Incrementar intentos
        await _otpRepo.IncrementAttemptsAsync(phone, purpose);

        // Verificar código
        if (HashCode(code) != otp.Code)
        {
            return false;
        }

        // Marcar como verificado
        await _otpRepo.MarkVerifiedAsync(phone, purpose);
        return true;
    }

    public async Task<SmsMessage?> GetMessageStatusAsync(string messageId)
    {
        return await _smsRepo.GetByMessageIdAsync(messageId);
    }

    public async Task<List<SmsBalance>> GetBalancesAsync()
    {
        var balances = new List<SmsBalance>();

        foreach (SmsProvider provider in Enum.GetValues<SmsProvider>())
        {
            try
            {
                var smsProvider = _providerFactory.GetProvider(provider);
                var balance = await smsProvider.GetBalanceAsync();
                balances.Add(new SmsBalance(
                    Provider: provider,
                    Credits: balance,
                    CostPerSms: smsProvider.CostPerSms,
                    LastUpdated: DateTime.UtcNow
                ));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get balance for {Provider}", provider);
            }
        }

        return balances;
    }

    public async Task ProcessDeliveryWebhookAsync(SmsProvider provider, string payload)
    {
        var smsProvider = _providerFactory.GetProvider(provider);
        var update = smsProvider.ParseDeliveryWebhook(payload);

        await _smsRepo.UpdateStatusAsync(
            update.MessageId,
            update.Status,
            update.DeliveredAt,
            update.ErrorCode,
            update.ErrorMessage
        );
    }

    private SmsProvider SelectOptimalProvider(string phoneNumber)
    {
        // Seleccionar según carrier del número
        var prefix = phoneNumber.Substring(0, 3);
        
        return prefix switch
        {
            // Claro: 809, 829 (algunos)
            "809" => SmsProvider.Claro,
            // Altice: 829, 849 (algunos)
            "829" or "849" => SmsProvider.Altice,
            // Default: mejor precio
            _ => SmsProvider.Viva
        };
    }

    private SmsProvider GetFallbackProvider(SmsProvider current)
    {
        return current switch
        {
            SmsProvider.Claro => SmsProvider.Altice,
            SmsProvider.Altice => SmsProvider.Claro,
            SmsProvider.Viva => SmsProvider.Claro,
            SmsProvider.Twilio => SmsProvider.Claro,
            _ => SmsProvider.Twilio
        };
    }

    private static int CalculateSegments(string message)
    {
        // GSM-7: 160 chars, UCS-2: 70 chars
        var isUnicode = message.Any(c => c > 127);
        var maxLength = isUnicode ? 70 : 160;
        return (int)Math.Ceiling((double)message.Length / maxLength);
    }

    private static string NormalizePhoneNumber(string phone)
    {
        var cleaned = new string(phone.Where(char.IsDigit).ToArray());
        if (cleaned.Length == 10 && (cleaned.StartsWith("809") || 
            cleaned.StartsWith("829") || cleaned.StartsWith("849")))
        {
            return "1" + cleaned;
        }
        return cleaned;
    }

    private static string MaskPhone(string phone)
    {
        if (phone.Length < 4) return "****";
        return phone.Substring(0, 3) + "****" + phone.Substring(phone.Length - 4);
    }

    private static string GenerateOtpCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var code = BitConverter.ToUInt32(bytes, 0) % 1000000;
        return code.ToString("D6");
    }

    private static string HashCode(string code)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(bytes);
    }

    private static string ApplyTemplate(string template, Dictionary<string, string> variables)
    {
        var result = template;
        foreach (var (key, value) in variables)
        {
            result = result.Replace($"{{{key}}}", value);
        }
        return result;
    }
}

// Provider Factory
public interface ISmsProviderFactory
{
    ISmsProvider GetProvider(SmsProvider provider);
}

public interface ISmsProvider
{
    decimal CostPerSms { get; }
    Task<SendResult> SendAsync(string to, string message);
    Task<List<BatchSendResult>> SendBatchAsync(IEnumerable<object> messages);
    Task<decimal> GetBalanceAsync();
    DeliveryUpdate ParseDeliveryWebhook(string payload);
}

public record SendResult(string MessageId, bool Success, string? Error);
public record BatchSendResult(string PhoneNumber, bool Success, string? MessageId);
public record DeliveryUpdate(string MessageId, SmsStatus Status, DateTime? DeliveredAt, string? ErrorCode, string? ErrorMessage);
```

---

## 🔌 Provider Implementation (Claro)

```csharp
namespace CommunicationService.Infrastructure.Providers;

public class ClaroSmsProvider : ISmsProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClaroSmsProvider> _logger;
    private readonly string _senderId;

    public decimal CostPerSms => 0.75m; // RD$

    public ClaroSmsProvider(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<ClaroSmsProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _senderId = config["Sms:Claro:SenderId"]!;

        _httpClient.BaseAddress = new Uri("https://api.clarobusiness.do/sms/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", config["Sms:Claro:ApiKey"]);
    }

    public async Task<SendResult> SendAsync(string to, string message)
    {
        var request = new { from = _senderId, to, message };
        
        var response = await _httpClient.PostAsJsonAsync("send", request);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Claro SMS failed: {Error}", error);
            return new SendResult("", false, error);
        }

        var result = await response.Content.ReadFromJsonAsync<ClaroSendResponse>();
        return new SendResult(result!.MessageId, true, null);
    }

    public async Task<List<BatchSendResult>> SendBatchAsync(IEnumerable<object> messages)
    {
        var request = new
        {
            from = _senderId,
            messages = messages.Cast<dynamic>().Select(m => new
            {
                to = m.PhoneNumber,
                message = m.Message
            })
        };

        var response = await _httpClient.PostAsJsonAsync("send/batch", request);
        
        if (!response.IsSuccessStatusCode)
        {
            return messages.Cast<dynamic>().Select(m => 
                new BatchSendResult(m.PhoneNumber, false, null)).ToList();
        }

        var result = await response.Content.ReadFromJsonAsync<ClaroBatchResponse>();
        
        return result!.Results.Select(r => 
            new BatchSendResult(r.To, r.Status == "queued", r.MessageId)).ToList();
    }

    public async Task<decimal> GetBalanceAsync()
    {
        var response = await _httpClient.GetAsync("balance");
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<ClaroBalanceResponse>();
        return result!.Credits;
    }

    public DeliveryUpdate ParseDeliveryWebhook(string payload)
    {
        var json = JsonSerializer.Deserialize<ClaroWebhookPayload>(payload)!;
        
        return new DeliveryUpdate(
            MessageId: json.MessageId,
            Status: json.Status switch
            {
                "delivered" => SmsStatus.Delivered,
                "failed" => SmsStatus.Failed,
                "undeliverable" => SmsStatus.Undeliverable,
                _ => SmsStatus.Sent
            },
            DeliveredAt: json.DeliveredAt,
            ErrorCode: json.ErrorCode,
            ErrorMessage: json.ErrorMessage
        );
    }
}

internal record ClaroSendResponse(string MessageId, string Status);
internal record ClaroBatchResponse(string BatchId, List<ClaroBatchResult> Results);
internal record ClaroBatchResult(string To, string Status, string? MessageId);
internal record ClaroBalanceResponse(decimal Credits);
internal record ClaroWebhookPayload(
    string MessageId, 
    string Status, 
    DateTime? DeliveredAt, 
    string? ErrorCode, 
    string? ErrorMessage);
```

---

## ⚛️ React Component

```tsx
// components/OtpVerification.tsx
import { useState, useRef, useEffect } from 'react';
import { useMutation } from '@tanstack/react-query';
import { smsService } from '@/services/smsService';
import { Loader2, CheckCircle, XCircle } from 'lucide-react';

interface Props {
  phoneNumber: string;
  purpose: 'registration' | 'login' | 'password_reset' | 'phone_verification';
  onVerified: () => void;
  onCancel: () => void;
}

export function OtpVerification({ phoneNumber, purpose, onVerified, onCancel }: Props) {
  const [code, setCode] = useState(['', '', '', '', '', '']);
  const [timeLeft, setTimeLeft] = useState(300); // 5 minutos
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  const sendOtpMutation = useMutation({
    mutationFn: () => smsService.sendOtp(phoneNumber, purpose),
  });

  const verifyMutation = useMutation({
    mutationFn: (otpCode: string) => smsService.verifyOtp(phoneNumber, otpCode, purpose),
    onSuccess: (isValid) => {
      if (isValid) onVerified();
    },
  });

  useEffect(() => {
    sendOtpMutation.mutate();
  }, []);

  useEffect(() => {
    if (timeLeft > 0) {
      const timer = setTimeout(() => setTimeLeft(timeLeft - 1), 1000);
      return () => clearTimeout(timer);
    }
  }, [timeLeft]);

  const handleChange = (index: number, value: string) => {
    if (!/^\d*$/.test(value)) return;

    const newCode = [...code];
    newCode[index] = value.slice(-1);
    setCode(newCode);

    // Auto-focus siguiente input
    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }

    // Auto-verificar cuando se complete
    if (index === 5 && value) {
      const fullCode = newCode.join('');
      if (fullCode.length === 6) {
        verifyMutation.mutate(fullCode);
      }
    }
  };

  const handleKeyDown = (index: number, e: React.KeyboardEvent) => {
    if (e.key === 'Backspace' && !code[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handlePaste = (e: React.ClipboardEvent) => {
    e.preventDefault();
    const pasted = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, 6);
    const newCode = [...code];
    pasted.split('').forEach((char, i) => {
      newCode[i] = char;
    });
    setCode(newCode);
    
    if (pasted.length === 6) {
      verifyMutation.mutate(pasted);
    }
  };

  const handleResend = () => {
    setTimeLeft(300);
    setCode(['', '', '', '', '', '']);
    sendOtpMutation.mutate();
  };

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  return (
    <div className="max-w-md mx-auto p-6 bg-white rounded-xl shadow-lg">
      <h2 className="text-2xl font-bold text-center mb-2">
        Verificar Número
      </h2>
      <p className="text-gray-600 text-center mb-6">
        Enviamos un código de 6 dígitos a<br />
        <span className="font-medium">{phoneNumber}</span>
      </p>

      {/* Code Inputs */}
      <div className="flex justify-center gap-2 mb-6" onPaste={handlePaste}>
        {code.map((digit, index) => (
          <input
            key={index}
            ref={(el) => (inputRefs.current[index] = el)}
            type="text"
            inputMode="numeric"
            maxLength={1}
            value={digit}
            onChange={(e) => handleChange(index, e.target.value)}
            onKeyDown={(e) => handleKeyDown(index, e)}
            disabled={verifyMutation.isPending}
            className={`w-12 h-14 text-center text-2xl font-bold border-2 rounded-lg
              ${verifyMutation.isError ? 'border-red-500' : 'border-gray-300'}
              focus:border-blue-500 focus:outline-none
              disabled:bg-gray-100`}
          />
        ))}
      </div>

      {/* Status */}
      {verifyMutation.isPending && (
        <div className="flex items-center justify-center gap-2 text-blue-600 mb-4">
          <Loader2 className="w-5 h-5 animate-spin" />
          <span>Verificando...</span>
        </div>
      )}

      {verifyMutation.isSuccess && verifyMutation.data && (
        <div className="flex items-center justify-center gap-2 text-green-600 mb-4">
          <CheckCircle className="w-5 h-5" />
          <span>¡Verificado correctamente!</span>
        </div>
      )}

      {verifyMutation.isError && (
        <div className="flex items-center justify-center gap-2 text-red-600 mb-4">
          <XCircle className="w-5 h-5" />
          <span>Código incorrecto. Intenta de nuevo.</span>
        </div>
      )}

      {/* Timer */}
      <div className="text-center mb-4">
        {timeLeft > 0 ? (
          <p className="text-gray-500">
            El código expira en{' '}
            <span className="font-medium">{formatTime(timeLeft)}</span>
          </p>
        ) : (
          <p className="text-red-500">El código ha expirado</p>
        )}
      </div>

      {/* Actions */}
      <div className="flex gap-3">
        <button
          onClick={onCancel}
          className="flex-1 px-4 py-2 border rounded-lg hover:bg-gray-50"
        >
          Cancelar
        </button>
        <button
          onClick={handleResend}
          disabled={timeLeft > 240 || sendOtpMutation.isPending}
          className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg 
            hover:bg-blue-700 disabled:opacity-50"
        >
          {sendOtpMutation.isPending ? (
            <Loader2 className="w-5 h-5 animate-spin mx-auto" />
          ) : (
            'Reenviar Código'
          )}
        </button>
      </div>
    </div>
  );
}
```

---

## 🎯 Controller

```csharp
namespace CommunicationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SmsController : ControllerBase
{
    private readonly ISmsService _smsService;
    private readonly ILogger<SmsController> _logger;

    public SmsController(
        ISmsService smsService,
        ILogger<SmsController> logger)
    {
        _smsService = smsService;
        _logger = logger;
    }

    /// <summary>
    /// Envía código OTP
    /// </summary>
    [HttpPost("otp/send")]
    public async Task<ActionResult<OtpResponse>> SendOtp([FromBody] SendOtpRequest request)
    {
        var otp = await _smsService.SendOtpAsync(request.PhoneNumber, request.Purpose);
        
        return Ok(new OtpResponse(
            Success: true,
            ExpiresAt: otp.ExpiresAt,
            Message: "Código enviado exitosamente"
        ));
    }

    /// <summary>
    /// Verifica código OTP
    /// </summary>
    [HttpPost("otp/verify")]
    public async Task<ActionResult<VerifyOtpResponse>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var isValid = await _smsService.VerifyOtpAsync(
            request.PhoneNumber, 
            request.Code, 
            request.Purpose);

        return Ok(new VerifyOtpResponse(
            IsValid: isValid,
            Message: isValid ? "Verificación exitosa" : "Código inválido o expirado"
        ));
    }

    /// <summary>
    /// Webhook Claro
    /// </summary>
    [HttpPost("webhook/claro")]
    public async Task<IActionResult> ClaroWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();
        
        await _smsService.ProcessDeliveryWebhookAsync(SmsProvider.Claro, payload);
        
        return Ok();
    }

    /// <summary>
    /// Webhook Altice
    /// </summary>
    [HttpPost("webhook/altice")]
    public async Task<IActionResult> AlticeWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();
        
        await _smsService.ProcessDeliveryWebhookAsync(SmsProvider.Altice, payload);
        
        return Ok();
    }

    /// <summary>
    /// Obtener balance de créditos (Admin)
    /// </summary>
    [HttpGet("balance")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<SmsBalance>>> GetBalances()
    {
        var balances = await _smsService.GetBalancesAsync();
        return Ok(balances);
    }
}

public record SendOtpRequest(string PhoneNumber, OtpPurpose Purpose);
public record VerifyOtpRequest(string PhoneNumber, string Code, OtpPurpose Purpose);
public record OtpResponse(bool Success, DateTime ExpiresAt, string Message);
public record VerifyOtpResponse(bool IsValid, string Message);
```

---

## ⚙️ Configuración

```json
{
  "Sms": {
    "SenderId": "OKLA",
    "OtpExpirationMinutes": 5,
    "MaxOtpAttempts": 3,
    "Claro": {
      "ApiKey": "claro_api_key_here",
      "SenderId": "OKLA"
    },
    "Altice": {
      "ApiKey": "altice_api_key_here",
      "SenderId": "OKLA"
    },
    "Viva": {
      "ApiKey": "viva_api_key_here",
      "SenderId": "OKLA"
    },
    "Twilio": {
      "AccountSid": "ACxxxxxxxx",
      "AuthToken": "xxxxxxxx",
      "FromNumber": "+18001234567"
    }
  }
}
```

---

## 📞 Contactos Comerciales

| Proveedor | Departamento | Teléfono | Email |
|-----------|-------------|----------|-------|
| Claro Business | Ventas Corporativas | 809-220-1111 | business@claro.com.do |
| Altice Business | Soluciones Empresariales | 809-200-1010 | corporativo@altice.com.do |
| Viva | Ventas API | 809-999-8484 | api@viva.com.do |

---

**Anterior:** [WHATSAPP_BUSINESS_API.md](./WHATSAPP_BUSINESS_API.md)  
**Siguiente:** [GOOGLE_MAPS_API.md](./GOOGLE_MAPS_API.md)
