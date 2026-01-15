# 📧 Resend API - Email Marketing & Transaccional

**Versión:** Resend API v1  
**Costo:** $0.20 por 1,000 emails (100 gratis/día)  
**Latencia:** <100ms  
**Tasa de entrega:** 99.9%  
**Soporte:** SMTP, API REST, React Components

---

## 📖 Introducción

**Resend** es la plataforma moderna para enviar emails transaccionales y marketing:

- ✅ Emails transaccionales (confirmación, notificaciones)
- ✅ Emails marketing (newsletters, campañas)
- ✅ Emails de bienvenida (onboarding)
- ✅ Alertas y notificaciones
- ✅ Reportes completos y analytics

### Ventajas vs Alternativas

| Aspecto       | Resend     | SendGrid  | Mailgun  |
| ------------- | ---------- | --------- | -------- |
| **Precio**    | $0.20/1K   | $14.95/mo | $35/mo   |
| **API**       | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐  | ⭐⭐⭐⭐ |
| **React**     | ⭐⭐⭐⭐⭐ | ❌        | ❌       |
| **Templates** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐  | ⭐⭐⭐   |
| **Soporte**   | Rápido     | Lento     | Medio    |

### Uso en OKLA:

1. **NotificationService**: Confirmaciones, alertas
2. **BillingService**: Recibos, facturas
3. **ReviewService**: Notificaciones de reviews
4. **DealerManagementService**: Onboarding dealers
5. **AuthService**: Password reset, 2FA

---

## 🎯 Caso Principal: Email de Bienvenida para Dealers

### Flujo

```
Dealer se registra
    ↓
POST /api/dealers
    ↓
Event: DealerCreated
    ↓
NotificationService
    ↓
Resend API (Welcome Email)
    ↓
Email recibido en <100ms
```

---

## 💻 Implementación C#

### NuGet Packages

```bash
dotnet add package resend
```

### IEmailService.cs

```csharp
public interface IEmailService
{
    Task<EmailResult> SendWelcomeEmailAsync(
        string dealerName,
        string dealerEmail,
        string activationLink,
        CancellationToken ct
    );

    Task<EmailResult> SendPasswordResetEmailAsync(
        string email,
        string resetLink,
        CancellationToken ct
    );

    Task<EmailResult> SendOrderConfirmationAsync(
        string customerEmail,
        string orderId,
        decimal amount,
        CancellationToken ct
    );

    Task<EmailResult> SendBulkEmailAsync(
        List<string> emails,
        string subject,
        string htmlContent,
        CancellationToken ct
    );
}

public record EmailResult(
    bool Success,
    string MessageId,
    string Error = null
);
```

### ResendEmailService.cs

```csharp
using Resend;
using Resend.EmailAddress;

public class ResendEmailService : IEmailService
{
    private readonly ResendClient _client;
    private readonly ILogger<ResendEmailService> _logger;
    private const string FromEmail = "noreply@okla.com.do";

    public ResendEmailService(
        IConfiguration config,
        ILogger<ResendEmailService> logger)
    {
        _logger = logger;
        var apiKey = config["Resend:ApiKey"];
        _client = new ResendClient(apiKey);
    }

    /// <summary>
    /// Enviar email de bienvenida a dealer
    /// </summary>
    public async Task<EmailResult> SendWelcomeEmailAsync(
        string dealerName,
        string dealerEmail,
        string activationLink,
        CancellationToken ct)
    {
        try
        {
            var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h1>¡Bienvenido a OKLA, {dealerName}!</h1>

                <p>Nos alegra que te unas a nuestra plataforma de venta de vehículos.</p>

                <h2>Próximos Pasos:</h2>
                <ol>
                    <li>Activa tu cuenta haciendo clic en el botón de abajo</li>
                    <li>Completa tu perfil de dealer</li>
                    <li>Sube los documentos de verificación</li>
                    <li>¡Comienza a publicar vehículos!</li>
                </ol>

                <p style='margin: 30px 0;'>
                    <a href='{activationLink}'
                       style='background-color: #3b82f6; color: white; padding: 12px 24px;
                              text-decoration: none; border-radius: 4px; display: inline-block;'>
                        Activar Cuenta
                    </a>
                </p>

                <p>Si tienes preguntas, contáctanos en <a href='mailto:support@okla.com.do'>support@okla.com.do</a></p>

                <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 40px 0;'>
                <p style='color: #6b7280; font-size: 12px;'>
                    Este es un email automatizado. Por favor no respondas directamente.
                </p>
            </div>";

            var request = new EmailMessage
            {
                From = FromEmail,
                To = new List<string> { dealerEmail },
                Subject = $"Bienvenido a OKLA, {dealerName}",
                HtmlBody = htmlContent
            };

            var response = await _client.EmailAsync(request);

            if (response.Id == null)
            {
                _logger.LogError($"Resend error: {response.Error}");
                return new EmailResult(
                    Success: false,
                    MessageId: null,
                    Error: response.Error
                );
            }

            _logger.LogInformation($"Welcome email sent to {dealerEmail} (ID: {response.Id})");

            return new EmailResult(
                Success: true,
                MessageId: response.Id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError($"Email service error: {ex.Message}");
            return new EmailResult(
                Success: false,
                MessageId: null,
                Error: ex.Message
            );
        }
    }

    /// <summary>
    /// Enviar email de reset de password
    /// </summary>
    public async Task<EmailResult> SendPasswordResetEmailAsync(
        string email,
        string resetLink,
        CancellationToken ct)
    {
        try
        {
            var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h1>Restablece tu contraseña</h1>

                <p>Recibimos una solicitud para restablecer tu contraseña.</p>

                <p style='margin: 30px 0;'>
                    <a href='{resetLink}'
                       style='background-color: #ef4444; color: white; padding: 12px 24px;
                              text-decoration: none; border-radius: 4px; display: inline-block;'>
                        Restablecer Contraseña
                    </a>
                </p>

                <p style='color: #6b7280; font-size: 12px;'>
                    Este link expira en 24 horas.
                    Si no solicitaste esto, ignora este email.
                </p>
            </div>";

            var request = new EmailMessage
            {
                From = FromEmail,
                To = new List<string> { email },
                Subject = "Restablece tu contraseña en OKLA",
                HtmlBody = htmlContent
            };

            var response = await _client.EmailAsync(request);

            return response.Id != null
                ? new EmailResult(Success: true, MessageId: response.Id)
                : new EmailResult(Success: false, MessageId: null, Error: response.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Password reset email error: {ex.Message}");
            return new EmailResult(Success: false, MessageId: null, Error: ex.Message);
        }
    }

    /// <summary>
    /// Enviar confirmación de orden/pago
    /// </summary>
    public async Task<EmailResult> SendOrderConfirmationAsync(
        string customerEmail,
        string orderId,
        decimal amount,
        CancellationToken ct)
    {
        try
        {
            var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h1>¡Orden Confirmada!</h1>

                <p>Tu transacción ha sido procesada exitosamente.</p>

                <div style='background-color: #f3f4f6; padding: 20px; border-radius: 4px; margin: 20px 0;'>
                    <p><strong>ID de Orden:</strong> {orderId}</p>
                    <p><strong>Monto:</strong> RD${amount:N2}</p>
                    <p><strong>Fecha:</strong> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</p>
                </div>

                <p>En breve recibirás más información sobre tu pedido.</p>

                <p>
                    <a href='https://okla.com.do/orders/{orderId}'>Ver Detalles de la Orden</a>
                </p>
            </div>";

            var request = new EmailMessage
            {
                From = FromEmail,
                To = new List<string> { customerEmail },
                Subject = $"Confirmación de Orden: {orderId}",
                HtmlBody = htmlContent
            };

            var response = await _client.EmailAsync(request);

            return response.Id != null
                ? new EmailResult(Success: true, MessageId: response.Id)
                : new EmailResult(Success: false, MessageId: null, Error: response.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Order confirmation error: {ex.Message}");
            return new EmailResult(Success: false, MessageId: null, Error: ex.Message);
        }
    }

    /// <summary>
    /// Enviar emails en bulk (newsletters, campañas)
    /// </summary>
    public async Task<EmailResult> SendBulkEmailAsync(
        List<string> emails,
        string subject,
        string htmlContent,
        CancellationToken ct)
    {
        try
        {
            var request = new EmailMessage
            {
                From = FromEmail,
                To = emails,
                Subject = subject,
                HtmlBody = htmlContent
            };

            var response = await _client.EmailAsync(request);

            _logger.LogInformation($"Bulk email sent to {emails.Count} recipients");

            return response.Id != null
                ? new EmailResult(Success: true, MessageId: response.Id)
                : new EmailResult(Success: false, MessageId: null, Error: response.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Bulk email error: {ex.Message}");
            return new EmailResult(Success: false, MessageId: null, Error: ex.Message);
        }
    }
}
```

### Program.cs

```csharp
// En ConfigureServices
services.AddScoped<IEmailService, ResendEmailService>();

// En appsettings.json
"Resend": {
  "ApiKey": "re_xxxxxxxxxxxxx"
}

// En Kubernetes Secret
kubectl create secret generic email-secrets \
  --from-literal=RESEND_API_KEY=re_xxx \
  -n okla
```

---

## 🎨 React Components

### useResendEmail.ts (Custom Hook)

```typescript
import { useMutation } from "@tanstack/react-query";
import { emailService } from "@/services/emailService";

interface EmailParams {
  email: string;
  subject: string;
  htmlContent: string;
}

export const useResendEmail = () => {
  return useMutation({
    mutationFn: async (params: EmailParams) => {
      const response = await emailService.sendEmail(params);
      if (!response.success) {
        throw new Error(response.error || "Email failed to send");
      }
      return response;
    },
  });
};
```

### WelcomeEmailTemplate.tsx

```typescript
import React from "react";
import {
  Html,
  Body,
  Container,
  Heading,
  Text,
  Link,
  Button,
} from "@react-email/components";

interface Props {
  dealerName: string;
  activationLink: string;
}

export const WelcomeEmailTemplate = ({ dealerName, activationLink }: Props) => (
  <Html>
    <Body style={main}>
      <Container style={container}>
        <Heading style={heading}>¡Bienvenido a OKLA, {dealerName}!</Heading>

        <Text style={text}>
          Nos alegra que te unas a nuestra plataforma de venta de vehículos.
        </Text>

        <Heading style={subheading}>Próximos Pasos:</Heading>
        <ol style={list}>
          <li>Activa tu cuenta</li>
          <li>Completa tu perfil</li>
          <li>Sube documentos</li>
          <li>¡Publica vehículos!</li>
        </ol>

        <Button href={activationLink} style={button}>
          Activar Cuenta
        </Button>
      </Container>
    </Body>
  </Html>
);

const main = { backgroundColor: "#f3f4f6" };
const container = { margin: "0 auto", padding: "20px" };
const heading = { color: "#1f2937" };
const subheading = { color: "#374151" };
const text = { color: "#6b7280" };
const button = {
  backgroundColor: "#3b82f6",
  color: "white",
  padding: "12px 24px",
  borderRadius: "4px",
  textDecoration: "none",
  display: "inline-block",
};
const list = { marginLeft: "20px" };
```

---

## 📡 Casos de Uso en OKLA

### 1. Bienvenida a Dealers

```csharp
[HttpPost("register")]
public async Task<IActionResult> RegisterDealer(
    [FromBody] DealerRegistrationRequest request,
    CancellationToken ct)
{
    var dealer = await _dealerService.CreateAsync(request, ct);

    // Enviar email de bienvenida
    var emailResult = await _emailService.SendWelcomeEmailAsync(
        dealerName: dealer.BusinessName,
        dealerEmail: dealer.Email,
        activationLink: $"https://okla.com.do/dealer/activate/{dealer.Id}",
        ct: ct
    );

    return Ok(new { dealer = dealer, email = emailResult });
}
```

### 2. Reset de Contraseña

```csharp
[HttpPost("password-reset")]
[AllowAnonymous]
public async Task<IActionResult> RequestPasswordReset(
    [FromBody] string email,
    CancellationToken ct)
{
    var resetToken = _tokenService.GenerateResetToken();
    await _userService.SetPasswordResetTokenAsync(email, resetToken, ct);

    var resetLink = $"https://okla.com.do/auth/reset-password?token={resetToken}";

    var emailResult = await _emailService.SendPasswordResetEmailAsync(
        email: email,
        resetLink: resetLink,
        ct: ct
    );

    return Ok(new { success = emailResult.Success });
}
```

### 3. Confirmación de Pago

```csharp
[HttpPost("subscribe")]
public async Task<IActionResult> SubscribeDealer(
    [FromBody] SubscriptionRequest request,
    CancellationToken ct)
{
    var payment = await _billingService.ProcessPaymentAsync(request, ct);

    // Enviar confirmación
    var emailResult = await _emailService.SendOrderConfirmationAsync(
        customerEmail: request.Email,
        orderId: payment.Id,
        amount: request.Amount,
        ct: ct
    );

    return Ok(new { payment = payment, confirmation = emailResult });
}
```

---

## 💰 Pricing

```
Free Tier:
  • 100 emails/día
  • Soporte por email
  • Sin costo

Pay-as-you-go:
  • $0.20 por 1,000 emails
  • Soporte prioritario
  • Analytics completo

Ejemplo OKLA (1,000 usuarios, 10 emails/mes por usuario):
  • 10,000 emails/mes
  • Costo: $0.20 * 10 = $2/mes

Scaling (100,000 usuarios):
  • 1,000,000 emails/mes
  • Costo: $0.20 * 1,000 = $200/mes
```

---

## ✅ Checklist

- [ ] Crear cuenta Resend
- [ ] Generar API key
- [ ] Implementar IEmailService
- [ ] Crear templates (bienvenida, reset, confirmación)
- [ ] Testing con emails de prueba
- [ ] Integrar en NotificationService
- [ ] Integrar en AuthService
- [ ] Integrar en BillingService
- [ ] Validar deliverability (tasa de entrega)
- [ ] Deployment a Kubernetes

---

_Documentación Resend para OKLA_  
_Última actualización: Enero 15, 2026_
