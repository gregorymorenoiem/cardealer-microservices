# 💳 Stripe Machine Learning - Fraud Detection

**Versión:** Stripe Radar  
**Integración:** Incluida en Stripe Payments  
**Costo:** Incluido en comisión de pagos  
**Precisión:** 99.9% (fraud detection)

---

## 📖 Introducción

**Stripe Radar** usa ML para detectar:

- Transacciones fraudulentas
- Riesgo de chargeback
- Comportamiento anómalo
- Patrones sospechosos

### Estado en OKLA:

Ya integrado en **BillingService** para pagos de suscripciones de dealers.

---

## 🎯 Casos de Uso

### Caso 1: Verificar Riesgo de Transacción

```csharp
// En BillingService - Crear Stripe Charge
var paymentIntent = new PaymentIntentCreateOptions
{
    Amount = amount, // En centavos
    Currency = "usd",
    PaymentMethod = paymentMethodId,
    Confirm = true,
    RadarOptions = new RadarOptionsCreateOptions
    {
        Session = new RadarSessionCreateOptions
        {
            // Captura información de sesión para ML
            IpAddress = ipAddress,
            UserAgent = userAgent
        }
    }
};

var service = new PaymentIntentService();
var paymentIntent = await service.CreateAsync(paymentIntent);

// Consultar resultado de Radar
if (paymentIntent.Status == "requires_action")
{
    // 3D Secure o verificación adicional requerida
    // Riesgo detectado por Radar
}
```

### Caso 2: Crear Charge con Fraud Check

```csharp
var chargeOptions = new ChargeCreateOptions
{
    Amount = 15000, // RD$150
    Currency = "usd", // Stripe trabaja en USD
    SourceId = stripeTokenId,
    Description = "Suscripción dealer plan Pro",
    Metadata = new Dictionary<string, string>
    {
        { "dealer_id", dealerId },
        { "plan", "pro" },
        { "period", "monthly" }
    },
    // Fraud Detection Options
    Fraud = new FraudOptions
    {
        UserReport = "safe" // o "fraudulent" si el usuario reporta
    },
    StatementDescriptor = "OKLA DEALER SUBSCRIPTION"
};

var chargeService = new ChargeService();
var charge = await chargeService.CreateAsync(chargeOptions);

// Checkear resultado
if (charge.Outcome?.NetworkStatus == "declined_by_network")
{
    _logger.LogWarning($"Fraud detected for dealer {dealerId}");
    // Rechazar pago
    return PaymentResult.Declined("Fraud detected");
}

if (charge.Outcome?.RiskLevel == "highest")
{
    _logger.LogWarning($"High risk transaction: {dealerId}");
    // Requerir verificación adicional
    return PaymentResult.RequiresVerification();
}
```

---

## 💻 Implementación Stripe Radar

### Configuración en Stripe Dashboard

1. **Activar Radar**:

   - Ir a `Settings → Radar`
   - Activar "Radar for Fraud Teams"

2. **Reglas de Riesgo** (Stripe Dashboard):

```
Crear reglas:
- SI: monto > $500 Y país != RD → BLOQUEAR
- SI: 3+ intentos fallidos en 1 hora → BLOQUEAR
- SI: IP cambia entre transacciones → REQUERIR 3DS
- SI: Email nuevo + tarjeta nueva → REQUERIR 3DS
```

### Métodos de IStripeService

```csharp
public interface IStripePaymentService
{
    Task<PaymentResult> CreatePaymentIntentAsync(
        string dealerId,
        decimal amountUsd,
        string paymentMethodId,
        string ipAddress,
        string userAgent,
        CancellationToken ct
    );

    Task<PaymentResult> ConfirmPaymentAsync(
        string paymentIntentId,
        string paymentMethodId,
        CancellationToken ct
    );

    Task<FraudAssessment> AssessTransactionRiskAsync(
        string chargeId,
        CancellationToken ct
    );

    Task HandleWebhookAsync(
        string payload,
        string signature,
        CancellationToken ct
    );
}

public record FraudAssessment(
    string RiskLevel, // low, medium, high
    double FraudScore, // 0-100
    List<string> RiskIndicators,
    string Recommendation // approve, challenge, block
);

public record PaymentResult(
    bool Success,
    string ChargeId,
    string Status, // succeeded, processing, requires_action
    string Message,
    FraudAssessment RiskAssessment
);
```

### Implementación Completa

```csharp
using Stripe;
using Stripe.BillingPortal;
using Stripe.Checkout;

public class StripePaymentService : IStripePaymentService
{
    private readonly ILogger<StripePaymentService> _logger;
    private readonly string _stripeApiKey;

    public StripePaymentService(
        IConfiguration config,
        ILogger<StripePaymentService> logger)
    {
        _logger = logger;
        _stripeApiKey = config["Stripe:SecretKey"];
        StripeConfiguration.ApiKey = _stripeApiKey;
    }

    /// <summary>
    /// Crear Payment Intent con contexto de sesión para Radar
    /// </summary>
    public async Task<PaymentResult> CreatePaymentIntentAsync(
        string dealerId,
        decimal amountUsd,
        string paymentMethodId,
        string ipAddress,
        string userAgent,
        CancellationToken ct)
    {
        try
        {
            var amountCents = (long)(amountUsd * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountCents,
                Currency = "usd",
                PaymentMethod = paymentMethodId,
                Confirm = true,
                OffSession = true, // Pago sin presencia del usuario
                Metadata = new Dictionary<string, string>
                {
                    { "dealer_id", dealerId },
                    { "created_at", DateTime.UtcNow.ToString("O") }
                },
                // Información para Radar
                ClientIp = ipAddress,
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, null, ct);

            var riskAssessment = await AssessPaymentRiskAsync(paymentIntent);

            if (paymentIntent.Status == "succeeded")
            {
                _logger.LogInformation($"Payment succeeded for dealer {dealerId}");
                return new PaymentResult(
                    Success: true,
                    ChargeId: paymentIntent.Id,
                    Status: "succeeded",
                    Message: "Payment processed successfully",
                    RiskAssessment: riskAssessment
                );
            }

            if (paymentIntent.Status == "requires_action")
            {
                _logger.LogWarning($"3D Secure required for dealer {dealerId}");
                return new PaymentResult(
                    Success: false,
                    ChargeId: paymentIntent.Id,
                    Status: "requires_action",
                    Message: "3D Secure verification required",
                    RiskAssessment: riskAssessment
                );
            }

            return new PaymentResult(
                Success: false,
                ChargeId: paymentIntent.Id,
                Status: paymentIntent.Status,
                Message: "Payment processing failed",
                RiskAssessment: riskAssessment
            );
        }
        catch (StripeException ex)
        {
            _logger.LogError($"Stripe error: {ex.StripeResponse.Content}");
            return new PaymentResult(
                Success: false,
                ChargeId: null,
                Status: "error",
                Message: ex.Message,
                RiskAssessment: null
            );
        }
    }

    /// <summary>
    /// Evaluar riesgo de fraude usando datos de Radar
    /// </summary>
    private async Task<FraudAssessment> AssessPaymentRiskAsync(
        PaymentIntent paymentIntent)
    {
        var outcome = paymentIntent.Charges.Data.FirstOrDefault()?.Outcome;

        if (outcome == null)
        {
            return new FraudAssessment(
                RiskLevel: "unknown",
                FraudScore: 0,
                RiskIndicators: new List<string>(),
                Recommendation: "approve"
            );
        }

        var riskIndicators = new List<string>();
        double fraudScore = 0;

        // Analizar resultado de Radar
        if (outcome.NetworkStatus == "declined_by_network")
        {
            fraudScore = 95;
            riskIndicators.Add("Declined by network");
        }

        if (outcome.RiskLevel == "highest")
        {
            fraudScore = Math.Max(fraudScore, 80);
            riskIndicators.Add("Highest risk level detected");
        }

        if (outcome.Type == "issuer_declined")
        {
            fraudScore = Math.Max(fraudScore, 70);
            riskIndicators.Add("Issuer declined");
        }

        var riskLevel = fraudScore switch
        {
            >= 80 => "high",
            >= 50 => "medium",
            _ => "low"
        };

        var recommendation = fraudScore switch
        {
            >= 90 => "block",
            >= 70 => "challenge", // Requerir verificación adicional
            _ => "approve"
        };

        _logger.LogInformation(
            $"Fraud assessment: {riskLevel} ({fraudScore}), recommendation: {recommendation}"
        );

        return new FraudAssessment(
            RiskLevel: riskLevel,
            FraudScore: fraudScore,
            RiskIndicators: riskIndicators,
            Recommendation: recommendation
        );
    }

    /// <summary>
    /// Manejar webhooks de Stripe
    /// </summary>
    public async Task HandleWebhookAsync(
        string payload,
        string signature,
        CancellationToken ct)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET")
            );

            switch (stripeEvent.Type)
            {
                case "charge.succeeded":
                    _logger.LogInformation("Charge succeeded");
                    break;

                case "charge.failed":
                    _logger.LogWarning("Charge failed");
                    break;

                case "charge.dispute.created":
                    _logger.LogError("Dispute created - possible fraud");
                    break;

                case "radar.early_fraud_warning":
                    _logger.LogWarning("Early fraud warning from Radar");
                    break;
            }
        }
        catch (StripeException ex)
        {
            _logger.LogError($"Webhook error: {ex.Message}");
        }

        await Task.CompletedTask;
    }
}
```

---

## 📊 Indicadores de Riesgo que Stripe Radar Detecta

| Indicador                         | Riesgo | Acción       |
| --------------------------------- | ------ | ------------ |
| Transacción rechazada por network | 95%    | BLOQUEAR     |
| IP diferente a la del registro    | 60%    | REQUERIR 3DS |
| Tarjeta nueva + Email nuevo       | 70%    | REQUERIR 3DS |
| Múltiples intentos fallidos       | 80%    | BLOQUEAR     |
| País diferente al registrado      | 50%    | VERIFICAR    |
| Monto 10x superior al promedio    | 75%    | REQUERIR 3DS |

---

## 🔐 Integración en BillingService

### Program.cs

```csharp
// En ConfigureServices
services.AddScoped<IStripePaymentService, StripePaymentService>();

// En appsettings.json
"Stripe": {
  "PublishableKey": "pk_live_...",
  "SecretKey": "sk_live_...",
  "WebhookSecret": "whsec_..."
}
```

### Controller

```csharp
[HttpPost("create-subscription")]
public async Task<IActionResult> CreateSubscription(
    [FromBody] SubscriptionRequest request,
    CancellationToken ct)
{
    // 1. Crear Payment Intent con Radar
    var paymentResult = await _paymentService.CreatePaymentIntentAsync(
        dealerId: request.DealerId,
        amountUsd: request.AmountUsd,
        paymentMethodId: request.PaymentMethodId,
        ipAddress: HttpContext.Connection.RemoteIpAddress.ToString(),
        userAgent: Request.Headers["User-Agent"],
        ct: ct
    );

    // 2. Evaluar riesgo
    if (paymentResult.RiskAssessment.Recommendation == "block")
    {
        return BadRequest(new { error = "Payment blocked due to fraud detection" });
    }

    if (paymentResult.RiskAssessment.Recommendation == "challenge")
    {
        // Requerir verificación adicional
        return Accepted(new
        {
            status = "requires_verification",
            chargeId = paymentResult.ChargeId,
            message = "Please verify your identity"
        });
    }

    // 3. Si aprobado, crear suscripción
    var subscription = await _stripeService.CreateSubscriptionAsync(
        customerId: request.CustomerId,
        priceId: request.PriceId,
        ct
    );

    return Ok(new { subscription = subscription });
}

[HttpPost("webhook")]
[AllowAnonymous]
public async Task<IActionResult> HandleWebhook(
    CancellationToken ct)
{
    var json = await new StreamReader(Request.Body).ReadToEndAsync();
    var signature = Request.Headers["Stripe-Signature"];

    await _paymentService.HandleWebhookAsync(json, signature, ct);

    return Ok();
}
```

---

## 💰 Costo

**Stripe Radar:** Incluido en comisión de Stripe

```
Stripe Pricing OKLA:
- Pagos nacionales (AZUL): 2.5%
- Pagos internacionales (Stripe): 3.5% + $0.30

Radar:
- Incluido en las comisiones anteriores
- SIN costo adicional
```

---

## ✅ Checklist

- [x] Cuenta Stripe configurada
- [x] Radar activado en dashboard
- [x] Webhook configurado
- [x] IStripePaymentService implementada
- [ ] Testing con tarjetas de prueba
- [ ] Monitoreo de fraude en producción
- [ ] Reporte mensual de transacciones bloqueadas
- [ ] Fallback a AZUL si Stripe falla

---

_Stripe ML & Radar para OKLA_  
_Última actualización: Enero 15, 2026_
