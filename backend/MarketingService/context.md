# MarketingService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** MarketingService
- **Puerto en Desarrollo:** 5034
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`marketingservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de marketing y campañas. Gestión de email marketing, landing pages, A/B testing, promociones, cupones y analytics de marketing.

---

## 🏗️ ARQUITECTURA

```
MarketingService/
├── MarketingService.Api/
│   ├── Controllers/
│   │   ├── CampaignsController.cs
│   │   ├── CouponsController.cs
│   │   ├── LandingPagesController.cs
│   │   └── ABTestsController.cs
│   └── Program.cs
├── MarketingService.Application/
├── MarketingService.Domain/
│   ├── Entities/
│   │   ├── EmailCampaign.cs
│   │   ├── Coupon.cs
│   │   ├── LandingPage.cs
│   │   └── ABTest.cs
│   └── Enums/
│       ├── CampaignStatus.cs
│       └── DiscountType.cs
└── MarketingService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### EmailCampaign
```csharp
public class EmailCampaign
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    
    // Contenido
    public string Subject { get; set; }
    public string HtmlBody { get; set; }
    public string? PlainTextBody { get; set; }
    
    // Segmentación (audience)
    public string TargetSegment { get; set; }      // "AllUsers", "NewUsers", "InactiveUsers", "Custom"
    public string? CustomSegmentQuery { get; set; } // JSON criteria
    
    // Scheduling
    public CampaignStatus Status { get; set; }     // Draft, Scheduled, Sending, Sent, Cancelled
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    
    // Resultados
    public int TotalRecipients { get; set; }
    public int Sent { get; set; }
    public int Delivered { get; set; }
    public int Opened { get; set; }
    public int Clicked { get; set; }
    public int Bounced { get; set; }
    public int Unsubscribed { get; set; }
    
    // Métricas calculadas
    public decimal OpenRate => TotalRecipients > 0 ? (decimal)Opened / TotalRecipients * 100 : 0;
    public decimal ClickRate => Delivered > 0 ? (decimal)Clicked / Delivered * 100 : 0;
    
    // Metadata
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Coupon
```csharp
public class Coupon
{
    public Guid Id { get; set; }
    public string Code { get; set; }               // "WELCOME20", "SUMMER50"
    
    // Descuento
    public DiscountType Type { get; set; }         // Percentage, FixedAmount
    public decimal Value { get; set; }             // 20% o $50
    public decimal? MaxDiscount { get; set; }      // Máximo descuento si es porcentaje
    
    // Restricciones
    public decimal? MinPurchaseAmount { get; set; }
    public int? MaxUsesTotal { get; set; }         // Límite total de usos
    public int? MaxUsesPerUser { get; set; }       // Límite por usuario
    
    // Aplicabilidad
    public string? ApplicableToEntityType { get; set; } // "Vehicle", "Property", "Subscription"
    public List<Guid>? ApplicableToEntityIds { get; set; }
    
    // Validez
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public bool IsActive { get; set; }
    
    // Uso
    public int TimesUsed { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

### LandingPage
```csharp
public class LandingPage
{
    public Guid Id { get; set; }
    public string Slug { get; set; }               // URL: /promo/summer-sale
    public string Title { get; set; }
    
    // Contenido (JSON - page builder)
    public string ContentJson { get; set; }
    // {
    //   "sections": [
    //     { "type": "hero", "heading": "Summer Sale", "cta": "Shop Now" },
    //     { "type": "features", "items": [...] }
    //   ]
    // }
    
    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    
    // Tracking
    public string? UtmCampaign { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    
    // Analytics
    public int Views { get; set; }
    public int UniqueVisitors { get; set; }
    public int Conversions { get; set; }
    public decimal ConversionRate => Views > 0 ? (decimal)Conversions / Views * 100 : 0;
    
    // Estado
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
```

### ABTest
```csharp
public class ABTest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Tipo de test
    public string TestType { get; set; }           // "EmailSubject", "CTAButton", "PriceDisplay"
    
    // Variantes
    public string VariantA { get; set; }           // JSON con config de A
    public string VariantB { get; set; }           // JSON con config de B
    public int TrafficSplit { get; set; } = 50;    // % de tráfico a B (50/50 default)
    
    // Métricas
    public string SuccessMetric { get; set; }      // "Clicks", "Conversions", "SignUps"
    
    // Resultados
    public int VariantAViews { get; set; }
    public int VariantAConversions { get; set; }
    public int VariantBViews { get; set; }
    public int VariantBConversions { get; set; }
    
    // Calculados
    public decimal VariantAConversionRate => VariantAViews > 0 
        ? (decimal)VariantAConversions / VariantAViews * 100 : 0;
    public decimal VariantBConversionRate => VariantBViews > 0 
        ? (decimal)VariantBConversions / VariantBViews * 100 : 0;
    
    // Estado
    public ABTestStatus Status { get; set; }       // Running, Paused, Completed
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? WinningVariant { get; set; }    // "A" o "B"
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Email Campaigns
- `POST /api/campaigns/email` - Crear campaña
- `GET /api/campaigns/email` - Listar campañas
- `GET /api/campaigns/email/{id}` - Detalle con métricas
- `POST /api/campaigns/email/{id}/send` - Enviar campaña
- `POST /api/campaigns/email/{id}/test` - Enviar email de prueba

### Coupons
- `POST /api/coupons` - Crear cupón
- `GET /api/coupons` - Listar cupones
- `POST /api/coupons/validate` - Validar cupón
  ```json
  {
    "code": "WELCOME20",
    "userId": "uuid",
    "amount": 15000
  }
  
  Response:
  {
    "valid": true,
    "discount": 3000,
    "finalAmount": 12000
  }
  ```
- `POST /api/coupons/{id}/apply` - Aplicar cupón (registrar uso)
- `PUT /api/coupons/{id}/deactivate` - Desactivar cupón

### Landing Pages
- `POST /api/landing-pages` - Crear landing page
- `GET /api/landing-pages` - Listar
- `GET /api/landing-pages/{slug}` - Ver por slug (público)
- `PUT /api/landing-pages/{id}` - Actualizar
- `POST /api/landing-pages/{id}/track-view` - Registrar vista
- `POST /api/landing-pages/{id}/track-conversion` - Registrar conversión

### A/B Tests
- `POST /api/ab-tests` - Crear test
- `GET /api/ab-tests` - Listar tests
- `GET /api/ab-tests/{id}/results` - Ver resultados
- `POST /api/ab-tests/{id}/assign-variant` - Asignar variante a usuario
- `PUT /api/ab-tests/{id}/stop` - Detener test

---

## 💡 FUNCIONALIDADES PLANEADAS

### Email Template Builder
Drag-and-drop builder con componentes:
- Header con logo
- Hero image
- Text blocks
- CTA buttons
- Product grids
- Footer con unsubscribe link

### Segmentation Engine
Crear audiencias personalizadas:
```json
{
  "name": "High-Value Leads",
  "criteria": {
    "and": [
      { "field": "leadScore", "operator": "gte", "value": 70 },
      { "field": "budget", "operator": "gte", "value": 25000 },
      { "field": "lastContactedAt", "operator": "lte", "value": "7 days ago" }
    ]
  }
}
```

### Dynamic Coupons
Auto-generar cupones únicos:
```csharp
public string GenerateUniqueCoupon(string prefix = "OKLA")
{
    var random = new Random();
    var code = $"{prefix}{random.Next(1000, 9999)}";
    return code;
}
```

### Automated Campaigns (Drip Campaigns)
Secuencias automatizadas:
1. Usuario registra → Enviar "Welcome" inmediato
2. +3 días sin actividad → "Here's what you can do"
3. +7 días sin compra → "Special offer: 10% off"

### UTM Tracking
Generar URLs con parámetros UTM automáticamente:
```
https://okla.com.do/vehicles?utm_source=email&utm_medium=campaign&utm_campaign=summer_sale&utm_content=cta_button
```

### Referral Program
Sistema de referidos:
- Usuario comparte link único
- Amigo registra usando link → ambos reciben cupón
- Tracking de conversiones por referido

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### NotificationService
- Enviar emails de campañas
- Programar envíos masivos

### UserService
- Segmentación de usuarios
- Historial de cupones usados

### BillingService
- Aplicar descuentos de cupones
- Registrar transacciones con cupón

### CRMService
- Importar leads de campañas
- Sync de métricas

### AnalyticsService
- Tracking de conversiones
- Attribution modeling

---

## 📊 MÉTRICAS CLAVE

### Email Campaign Metrics
- **Open Rate:** % emails abiertos
- **Click-Through Rate:** % clics en links
- **Bounce Rate:** % emails rechazados
- **Unsubscribe Rate:** % usuarios que se desuscriben
- **Conversion Rate:** % que completaron acción deseada

### Coupon Metrics
- **Redemption Rate:** % cupones usados vs distribuidos
- **Average Discount:** Descuento promedio aplicado
- **ROI:** Revenue generado vs costo de descuentos

### Landing Page Metrics
- **Traffic Sources:** De dónde vienen visitantes
- **Bounce Rate:** % que salen sin interactuar
- **Conversion Rate:** % que completan goal
- **Time on Page:** Tiempo promedio en página

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0
