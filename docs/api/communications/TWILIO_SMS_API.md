# 📱 Twilio SMS API

**API Provider:** Twilio  
**Endpoint:** https://api.twilio.com/2010-04-01/Accounts/{AccountSID}/Messages  
**Método:** POST  
**Autenticación:** HTTP Basic  
**Rate Limit:** 10,000 messages/day  
**Latencia:** 1-5 segundos  
**Costo:** $0.05/mensaje

---

## 📖 Descripción

Envío de SMS para notificaciones rápidas, códigos OTP, recordatorios y alertas.

### Casos de Uso en OKLA

✅ Códigos OTP (One-Time Password)  
✅ Recordatorios de test drive  
✅ Alertas de precio bajó  
✅ Confirmaciones de venta  
✅ Recordatorios de documentos faltantes

---

## 🔧 Especificaciones Técnicas

### Request

```bash
POST /2010-04-01/Accounts/ACxxxxx/Messages
From=%2B1234567890
To=%2B18005551212
Body=Tu+código+OTP+es%3A+123456
```

### Response

```json
{
  "sid": "SMxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "status": "queued",
  "to": "+18005551212",
  "from": "+1234567890",
  "body": "Tu código OTP es: 123456",
  "price": "-0.00500",
  "price_unit": "USD"
}
```

---

## 💻 Implementación Backend

### Domain Model

```csharp
public class SmsMessage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ToPhoneNumber { get; set; }
    public string MessageBody { get; set; }
    public string TwilioSid { get; set; }
    public MessageStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal CostUSD { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
```

### CQRS Command

```csharp
public record SendSmsCommand(
    Guid UserId,
    string ToPhoneNumber,
    string MessageBody
) : IRequest<SendSmsResult>;

public record SendSmsResult(
    bool Success,
    string? TwilioSid,
    string? ErrorMessage
);

public class SendSmsHandler : IRequestHandler<SendSmsCommand, SendSmsResult>
{
    private readonly ITwilioService _twilioService;
    private readonly ISmsRepository _repository;

    public async Task<SendSmsResult> Handle(
        SendSmsCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _twilioService.SendSmsAsync(
            request.ToPhoneNumber,
            request.MessageBody,
            cancellationToken);

        if (result.Success)
        {
            var smsMessage = new SmsMessage
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ToPhoneNumber = request.ToPhoneNumber,
                MessageBody = request.MessageBody,
                TwilioSid = result.TwilioSid!,
                Status = MessageStatus.Sent,
                CostUSD = 0.05m,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(smsMessage, cancellationToken);
        }

        return result;
    }
}
```

### Service Integration

```csharp
public interface ISmsService
{
    Task<SendSmsResult> SendAsync(
        string toPhoneNumber,
        string messageBody,
        CancellationToken cancellationToken);
}

public class TwilioSmsService : ISmsService
{
    public async Task<SendSmsResult> SendAsync(
        string toPhoneNumber,
        string messageBody,
        CancellationToken cancellationToken)
    {
        try
        {
            var message = await MessageResource.CreateAsync(
                body: messageBody,
                from: new PhoneNumber(_fromNumber),
                to: new PhoneNumber(toPhoneNumber)
            );

            return new SendSmsResult(
                true,
                message.Sid,
                null
            );
        }
        catch (Exception ex)
        {
            return new SendSmsResult(false, null, ex.Message);
        }
    }
}
```

---

## 🎨 Frontend (React)

```typescript
// useOTP.ts
export const useOTP = () => {
  const sendOtp = useMutation({
    mutationFn: async (phoneNumber: string) => {
      return api.post("/api/sms/send-otp", { phoneNumber });
    },
  });

  return { sendOtp };
};

// VerifyPhoneModal.tsx
export const VerifyPhoneModal = () => {
  const [phone, setPhone] = useState("");
  const [otp, setOtp] = useState("");
  const { sendOtp } = useOTP();

  return (
    <div>
      <input
        value={phone}
        onChange={(e) => setPhone(e.target.value)}
        placeholder="+1234567890"
      />
      <button onClick={() => sendOtp.mutate(phone)}>Enviar Código</button>

      <input
        value={otp}
        onChange={(e) => setOtp(e.target.value)}
        placeholder="Código OTP"
      />
      <button>Verificar</button>
    </div>
  );
};
```

---

## 🧪 Testing

```csharp
[Fact]
public async Task SendSms_WithValidNumber_Success()
{
    var command = new SendSmsCommand(
        Guid.NewGuid(),
        "+1234567890",
        "Tu código OTP es: 123456"
    );

    var result = await _handler.Handle(command, CancellationToken.None);

    Assert.True(result.Success);
    Assert.NotNull(result.TwilioSid);
}
```

---

## 💰 Costos

- **Costo:** $0.05/mensaje
- **Límite:** 10,000 msg/día
- **Estimado:** 100,000 SMS/mes = $5,000/mes

---

**Versión:** 1.0 | **Estado:** ✅ Documentado | **Actualizado:** Enero 15, 2026
