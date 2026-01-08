# IntegrationService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** IntegrationService
- **Puerto en Desarrollo:** 5037
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`integrationservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de integraciones externas y APIs de terceros. Gestiona conexiones con servicios externos como proveedores de datos de vehículos (Carfax, AutoCheck), pasarelas de pago adicionales, CRM externos, plataformas de publicidad y más.

---

## 🏗️ ARQUITECTURA

```
IntegrationService/
├── IntegrationService.Api/
│   ├── Controllers/
│   │   ├── IntegrationsController.cs
│   │   ├── WebhooksController.cs
│   │   └── SyncController.cs
│   └── Program.cs
├── IntegrationService.Application/
│   └── Integrations/
│       ├── CarfaxIntegration.cs
│       ├── FacebookAdsIntegration.cs
│       ├── GoogleMapsIntegration.cs
│       └── ZapierIntegration.cs
├── IntegrationService.Domain/
│   ├── Entities/
│   │   ├── Integration.cs
│   │   ├── IntegrationLog.cs
│   │   ├── Webhook.cs
│   │   └── SyncJob.cs
│   └── Enums/
│       ├── IntegrationType.cs
│       └── SyncStatus.cs
└── IntegrationService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### Integration
```csharp
public class Integration
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public IntegrationType Type { get; set; }      // VehicleData, Payment, Marketing, CRM, Analytics
    public string Provider { get; set; }           // "Carfax", "Stripe", "Facebook", "HubSpot"
    
    // Configuración (JSON encriptado)
    public string ConfigurationJson { get; set; }
    // {
    //   "apiKey": "encrypted_key",
    //   "apiSecret": "encrypted_secret",
    //   "environment": "production"
    // }
    
    // Endpoints
    public string? BaseUrl { get; set; }
    public string? WebhookUrl { get; set; }        // Para recibir eventos del proveedor
    
    // Estado
    public bool IsEnabled { get; set; }
    public bool IsConnected { get; set; }
    public DateTime? LastConnectionTest { get; set; }
    public string? LastError { get; set; }
    
    // Límites
    public int? RateLimitPerHour { get; set; }
    public int? MonthlyQuota { get; set; }
    public int CurrentMonthUsage { get; set; }
    
    // Propietario
    public Guid? OwnerId { get; set; }             // null = global, o específico por usuario
    
    public DateTime CreatedAt { get; set; }
    public DateTime LastUsedAt { get; set; }
}
```

### IntegrationLog
```csharp
public class IntegrationLog
{
    public Guid Id { get; set; }
    public Guid IntegrationId { get; set; }
    public Integration Integration { get; set; }
    
    // Request
    public string Method { get; set; }             // GET, POST, etc.
    public string Endpoint { get; set; }
    public string? RequestBody { get; set; }
    
    // Response
    public int HttpStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Timing
    public DateTime RequestedAt { get; set; }
    public TimeSpan Duration { get; set; }
    
    // Context
    public Guid? UserId { get; set; }
    public string? Purpose { get; set; }           // "VehicleHistoryCheck", "PaymentProcessing"
}
```

### Webhook
```csharp
public class Webhook
{
    public Guid Id { get; set; }
    public Guid IntegrationId { get; set; }
    
    // Configuración
    public string EventType { get; set; }          // "payment.succeeded", "vehicle.updated"
    public string TargetUrl { get; set; }          // URL para enviar el webhook
    public string Secret { get; set; }             // Para verificar firma
    
    // Headers personalizados
    public Dictionary<string, string>? CustomHeaders { get; set; }
    
    // Retry policy
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 60;
    
    // Estado
    public bool IsActive { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public int TotalDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

### SyncJob
```csharp
public class SyncJob
{
    public Guid Id { get; set; }
    public Guid IntegrationId { get; set; }
    public Integration Integration { get; set; }
    
    // Tipo de sincronización
    public string SyncType { get; set; }           // "ImportVehicles", "ExportLeads", "SyncContacts"
    public SyncDirection Direction { get; set; }   // Import, Export, Bidirectional
    
    // Schedule
    public string? CronExpression { get; set; }    // "0 */6 * * *" (cada 6 horas)
    public bool IsAutomatic { get; set; }
    
    // Última ejecución
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public SyncStatus LastStatus { get; set; }
    public int? RecordsSynced { get; set; }
    public string? LastError { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Integrations
- `GET /api/integrations` - Listar integraciones disponibles
- `GET /api/integrations/{id}` - Detalle de integración
- `POST /api/integrations` - Configurar nueva integración
- `PUT /api/integrations/{id}` - Actualizar configuración
- `POST /api/integrations/{id}/test` - Probar conexión
- `DELETE /api/integrations/{id}` - Eliminar integración

### Specific Integrations
- `POST /api/integrations/carfax/check` - Consultar historial de vehículo
  ```json
  {
    "vin": "1HGCM82633A123456"
  }
  
  Response:
  {
    "vin": "1HGCM82633A123456",
    "accidents": 0,
    "owners": 2,
    "serviceRecords": 8,
    "mileageHistory": [...],
    "reportUrl": "https://carfax.com/report/..."
  }
  ```
- `POST /api/integrations/facebook/publish-listing` - Publicar en Facebook Marketplace
- `POST /api/integrations/google-maps/geocode` - Convertir dirección a coordenadas

### Webhooks
- `POST /api/integrations/webhooks` - Registrar webhook
- `GET /api/integrations/webhooks` - Listar webhooks
- `POST /api/integrations/webhooks/{id}/test` - Enviar test webhook
- `GET /api/integrations/webhooks/{id}/deliveries` - Historial de entregas

### Sync Jobs
- `POST /api/integrations/sync-jobs` - Crear job de sincronización
- `GET /api/integrations/sync-jobs` - Listar jobs
- `POST /api/integrations/sync-jobs/{id}/run` - Ejecutar manualmente

### Webhook Receiver (para recibir de terceros)
- `POST /api/webhooks/stripe` - Recibir eventos de Stripe
- `POST /api/webhooks/facebook` - Recibir eventos de Facebook
- `POST /api/webhooks/{provider}` - Endpoint genérico

---

## 💡 INTEGRACIONES PLANEADAS

### 1. Carfax / AutoCheck (Vehicle History)
```csharp
public class CarfaxIntegration
{
    public async Task<VehicleHistoryReport> GetReportAsync(string vin)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.carfax.com/v1/reports")
        {
            Headers = { { "Authorization", $"Bearer {_apiKey}" } },
            Content = JsonContent.Create(new { vin })
        };
        
        var response = await _httpClient.SendAsync(request);
        var report = await response.Content.ReadFromJsonAsync<VehicleHistoryReport>();
        
        // Registrar en log
        await LogIntegrationCallAsync("Carfax", "GetReport", vin, response.IsSuccessStatusCode);
        
        return report;
    }
}
```

### 2. Facebook Marketplace / Ads
Publicar listings automáticamente:
```csharp
public async Task PublishToFacebookAsync(Vehicle vehicle)
{
    var listing = new
    {
        title = $"{vehicle.Year} {vehicle.Make} {vehicle.Model}",
        description = vehicle.Description,
        price = vehicle.Price,
        currency = "DOP",
        availability = "in stock",
        condition = vehicle.Condition,
        images = vehicle.Images.Select(i => i.Url).ToArray()
    };
    
    await _facebookClient.PostAsync($"/marketplace_listings", listing);
}
```

### 3. Google Maps API
Geocoding y reverse geocoding:
```csharp
public async Task<Coordinates> GeocodeAddressAsync(string address)
{
    var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";
    var response = await _httpClient.GetFromJsonAsync<GoogleMapsResponse>(url);
    
    return new Coordinates
    {
        Latitude = response.Results[0].Geometry.Location.Lat,
        Longitude = response.Results[0].Geometry.Location.Lng
    };
}
```

### 4. Zapier / Make (Automation)
Enviar eventos a Zapier para automatizaciones:
```csharp
public async Task TriggerZapAsync(string zapUrl, object data)
{
    await _httpClient.PostAsJsonAsync(zapUrl, data);
}
```

### 5. HubSpot CRM
Sincronizar leads:
```csharp
public async Task SyncLeadToHubSpotAsync(Lead lead)
{
    var contact = new
    {
        properties = new
        {
            email = lead.Email,
            firstname = lead.FirstName,
            lastname = lead.LastName,
            phone = lead.Phone,
            leadstatus = lead.Status
        }
    };
    
    await _hubSpotClient.PostAsync("/crm/v3/objects/contacts", contact);
}
```

### 6. Twilio (SMS adicionales)
Complementar NotificationService:
```csharp
public async Task SendSMSAsync(string to, string message)
{
    var twilioMessage = new
    {
        To = to,
        From = _twilioNumber,
        Body = message
    };
    
    await _twilioClient.PostAsync("/Messages.json", twilioMessage);
}
```

### 7. Mailchimp (Email Marketing)
Sincronizar subscribers:
```csharp
public async Task AddSubscriberAsync(string email, Dictionary<string, string> mergeFields)
{
    var subscriber = new
    {
        email_address = email,
        status = "subscribed",
        merge_fields = mergeFields
    };
    
    await _mailchimpClient.PostAsync($"/lists/{_listId}/members", subscriber);
}
```

### 8. Google Analytics / Facebook Pixel
Server-side tracking:
```csharp
public async Task TrackEventAsync(string eventName, Dictionary<string, object> properties)
{
    // Google Analytics Measurement Protocol
    await _gaClient.PostAsync("/collect", new
    {
        v = 1,
        tid = _trackingId,
        cid = properties["userId"],
        t = "event",
        ec = properties["category"],
        ea = eventName
    });
    
    // Facebook Conversions API
    await _fbClient.PostAsync($"/{_pixelId}/events", new
    {
        data = new[]
        {
            new
            {
                event_name = eventName,
                event_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                user_data = properties
            }
        }
    });
}
```

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### VehiclesSaleService
- Consultar Carfax cuando se publica vehículo
- Publicar a Facebook Marketplace

### CRMService
- Sincronizar leads con HubSpot
- Enviar a Zapier para workflows

### BillingService
- Recibir webhooks de Stripe
- Procesar eventos de pago

### NotificationService
- Usar Twilio como provider adicional

---

## 🔐 SEGURIDAD

### API Keys Encryption
```csharp
public class ConfigurationEncryption
{
    public string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();
        
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var encrypted = encryptor.TransformFinalBlock(
            Encoding.UTF8.GetBytes(plaintext), 0, plaintext.Length);
        
        return Convert.ToBase64String(aes.IV.Concat(encrypted).ToArray());
    }
}
```

### Webhook Signature Verification
```csharp
public bool VerifyWebhookSignature(string payload, string signature, string secret)
{
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
    var expectedSignature = Convert.ToBase64String(hash);
    
    return signature == expectedSignature;
}
```

---

## 📊 MONITORING

### Métricas por Integración
- Total API calls
- Success rate
- Average response time
- Quota usage
- Error rate

### Alertas
- Integration down (connection test fails)
- Rate limit approaching (90% quota)
- High error rate (> 10% in 1 hour)
- Webhook delivery failures

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0  
**Integraciones Planeadas:** 8+
