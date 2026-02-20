# 🎯 OKLA — Sistema de Publicidad de Homepage (Advertising System)

## Prompt de Implementación Profesional Completo

**Versión:** 1.0  
**Fecha:** Febrero 20, 2026  
**Archivo:** `prompts/OKLA_HOMEPAGE_ADVERTISING_SYSTEM.md`

---

## 👥 ROLES PROFESIONALES ASUMIDOS

Este prompt fue diseñado desde la perspectiva simultánea de:

- **Product Manager**: Define las reglas de negocio, flujos de monetización y criterios de aceptación
- **UI/UX Designer**: Especifica los componentes, layout responsivo y experiencia de usuario
- **Frontend Engineer (Next.js 16)**: Define exactamente qué archivos crear/modificar y su implementación
- **Backend Engineer (.NET 8)**: Define la arquitectura limpia, entidades, handlers y contratos de API
- **Database Architect (PostgreSQL)**: Define esquema completo, índices y consultas críticas
- **Business Analyst**: Define el algoritmo de rotación, métricas y sistema de reportes
- **DevOps Engineer**: Define deployment en K8s, CI/CD, caching con Redis
- **Marketing & Monetization Specialist**: Define los planes de precios y estrategia de publicidad

---

## 🚫 RESTRICCIONES CRÍTICAS (LEER ANTES DE IMPLEMENTAR)

1. **Package manager:** SIEMPRE `pnpm` — NUNCA `npm install` ni `yarn add`
2. **Testing frontend:** SIEMPRE Vitest — NUNCA Jest
3. **OpenTelemetry:** versión `1.9.0` máximo (1.10.0 requiere .NET 9 y rompe el build)
4. **Serilog:** NUNCA usar `CreateBootstrapLogger()` junto con `UseStandardSerilog()`
5. **Health checks:** el endpoint `/health` DEBE excluir checks con tag `"external"`
6. **DI:** Registrar `IDeadLetterQueue` ANTES de cualquier `AddHostedService<T>()`
7. **Program.cs:** SIEMPRE terminar con `public partial class Program { }` al final del archivo
8. **Puertos en K8s:** TODOS los servicios usan puerto `8080` (nunca 80)
9. **Nombre de imagen Docker:** `ghcr.io/gregorymorenoiem/advertisingservice:latest`
10. **SecurityValidators.cs:** aplicar `.NoSqlInjection()` y `.NoXss()` en TODOS los validators de strings
11. **No generalizar:** cada paso de este prompt es concreto y debe implementarse exactamente como se especifica

---

## 📋 CONTEXTO DEL ESTADO ACTUAL

### Lo que YA existe (no recrear):

- `frontend/web-next/src/app/(main)/page.tsx` — Homepage Server Component que usa `HomepageSections` de `VehiclesSaleService`
- `frontend/web-next/src/app/(main)/homepage-client.tsx` — Client Component con secciones: carousel, destacados, sedanes, suvs, etc.
- `frontend/web-next/src/components/homepage/brand-slider.tsx` — Carrusel de marcas con `DEFAULT_BRANDS` **hardcodeados**
- `frontend/web-next/src/components/homepage/category-cards.tsx` — Grid de categorías con `DEFAULT_CATEGORIES` **hardcodeados**
- `frontend/web-next/src/components/homepage/featured-section.tsx` — Sección featured genérica
- `frontend/web-next/src/app/(main)/mis-vehiculos/[id]/boost/page.tsx` — Página de boost **ya existe** (UI skeleton sin backend real)
- `frontend/web-next/src/app/(main)/dealer/inventario/page.tsx` — Inventario del dealer

### Lo que NO existe y se debe crear:

- `AdvertisingService` (nuevo microservicio .NET 8 completo)
- Sistema real de campañas conectado al `boost/page.tsx` existente
- Datos de marcas y categorías dinámicos desde backend (reemplazar hardcoded)
- Tracking de impresiones y clicks en vehículos destacados
- Panel admin para: algoritmo de rotación, imágenes de categorías, logos de marcas
- Dashboard de publicidad en el portal dealer
- Job de reportes diarios automáticos

---

## 🏗️ PARTE 1: NUEVO MICROSERVICIO — `AdvertisingService`

### Puerto local de desarrollo: `15145`

### Estructura de carpetas EXACTA (Clean Architecture):

```
backend/AdvertisingService/
├── AdvertisingService.sln
├── Dockerfile
├── AdvertisingService.Api/
│   ├── Controllers/
│   │   ├── AdCampaignsController.cs
│   │   ├── AdTrackingController.cs
│   │   ├── AdRotationController.cs
│   │   ├── HomepageConfigController.cs
│   │   └── AdReportsController.cs
│   ├── Middleware/
│   │   └── AdRateLimitMiddleware.cs
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── AdvertisingService.Api.csproj
├── AdvertisingService.Application/
│   ├── Features/
│   │   ├── Campaigns/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateCampaign/
│   │   │   │   │   ├── CreateCampaignCommand.cs
│   │   │   │   │   ├── CreateCampaignCommandHandler.cs
│   │   │   │   │   └── CreateCampaignCommandValidator.cs
│   │   │   │   ├── PauseCampaign/
│   │   │   │   │   ├── PauseCampaignCommand.cs
│   │   │   │   │   └── PauseCampaignCommandHandler.cs
│   │   │   │   ├── ResumeCampaign/
│   │   │   │   │   ├── ResumeCampaignCommand.cs
│   │   │   │   │   └── ResumeCampaignCommandHandler.cs
│   │   │   │   └── CancelCampaign/
│   │   │   │       ├── CancelCampaignCommand.cs
│   │   │   │       └── CancelCampaignCommandHandler.cs
│   │   │   └── Queries/
│   │   │       ├── GetCampaignById/
│   │   │       │   ├── GetCampaignByIdQuery.cs
│   │   │       │   └── GetCampaignByIdQueryHandler.cs
│   │   │       ├── GetCampaignsByOwner/
│   │   │       │   ├── GetCampaignsByOwnerQuery.cs
│   │   │       │   └── GetCampaignsByOwnerQueryHandler.cs
│   │   │       └── GetCampaignsByVehicle/
│   │   │           ├── GetCampaignsByVehicleQuery.cs
│   │   │           └── GetCampaignsByVehicleQueryHandler.cs
│   │   ├── Tracking/
│   │   │   └── Commands/
│   │   │       ├── TrackImpression/
│   │   │       │   ├── TrackImpressionCommand.cs
│   │   │       │   ├── TrackImpressionCommandHandler.cs
│   │   │       │   └── TrackImpressionCommandValidator.cs
│   │   │       └── TrackClick/
│   │   │           ├── TrackClickCommand.cs
│   │   │           ├── TrackClickCommandHandler.cs
│   │   │           └── TrackClickCommandValidator.cs
│   │   ├── Rotation/
│   │   │   ├── Queries/
│   │   │   │   └── GetHomepageRotation/
│   │   │   │       ├── GetHomepageRotationQuery.cs
│   │   │   │       ├── GetHomepageRotationQueryHandler.cs
│   │   │   │       └── HomepageRotationDto.cs
│   │   │   └── Commands/
│   │   │       ├── UpdateRotationConfig/
│   │   │       │   ├── UpdateRotationConfigCommand.cs
│   │   │       │   ├── UpdateRotationConfigCommandHandler.cs
│   │   │       │   └── UpdateRotationConfigCommandValidator.cs
│   │   │       └── ForceRotationRefresh/
│   │   │           ├── ForceRotationRefreshCommand.cs
│   │   │           └── ForceRotationRefreshCommandHandler.cs
│   │   ├── HomepageConfig/
│   │   │   ├── Commands/
│   │   │   │   ├── UpdateCategoryConfig/
│   │   │   │   │   ├── UpdateCategoryConfigCommand.cs
│   │   │   │   │   ├── UpdateCategoryConfigCommandHandler.cs
│   │   │   │   │   └── UpdateCategoryConfigCommandValidator.cs
│   │   │   │   ├── UploadCategoryImage/
│   │   │   │   │   ├── UploadCategoryImageCommand.cs
│   │   │   │   │   └── UploadCategoryImageCommandHandler.cs
│   │   │   │   ├── UpdateBrandConfig/
│   │   │   │   │   ├── UpdateBrandConfigCommand.cs
│   │   │   │   │   ├── UpdateBrandConfigCommandHandler.cs
│   │   │   │   │   └── UpdateBrandConfigCommandValidator.cs
│   │   │   │   └── UploadBrandLogo/
│   │   │   │       ├── UploadBrandLogoCommand.cs
│   │   │   │       └── UploadBrandLogoCommandHandler.cs
│   │   │   └── Queries/
│   │   │       ├── GetCategoryConfigs/
│   │   │       │   ├── GetCategoryConfigsQuery.cs
│   │   │       │   └── GetCategoryConfigsQueryHandler.cs
│   │   │       └── GetBrandConfigs/
│   │   │           ├── GetBrandConfigsQuery.cs
│   │   │           └── GetBrandConfigsQueryHandler.cs
│   │   └── Reports/
│   │       └── Queries/
│   │           ├── GetOwnerReport/
│   │           │   ├── GetOwnerReportQuery.cs
│   │           │   └── GetOwnerReportQueryHandler.cs
│   │           └── GetPlatformReport/
│   │               ├── GetPlatformReportQuery.cs
│   │               └── GetPlatformReportQueryHandler.cs
│   ├── DTOs/
│   │   ├── AdCampaignDto.cs
│   │   ├── HomepageVehicleAdDto.cs
│   │   ├── BrandConfigDto.cs
│   │   ├── CategoryConfigDto.cs
│   │   ├── RotationConfigDto.cs
│   │   └── OwnerAdReportDto.cs
│   ├── Validators/
│   │   └── SecurityValidators.cs   ← COPIAR EXACTAMENTE desde backend/AuthService/AuthService.Application/Validators/SecurityValidators.cs
│   └── Clients/
│       ├── VehiclesServiceClient.cs
│       ├── BillingServiceClient.cs
│       ├── MediaServiceClient.cs
│       └── NotificationServiceClient.cs
├── AdvertisingService.Domain/
│   ├── Entities/
│   │   ├── AdCampaign.cs
│   │   ├── AdImpression.cs
│   │   ├── AdClick.cs
│   │   ├── RotationConfig.cs
│   │   ├── CategoryImageConfig.cs
│   │   └── BrandConfig.cs
│   ├── Interfaces/
│   │   ├── IAdCampaignRepository.cs
│   │   ├── IAdImpressionRepository.cs
│   │   ├── IAdClickRepository.cs
│   │   ├── IRotationConfigRepository.cs
│   │   ├── ICategoryConfigRepository.cs
│   │   ├── IBrandConfigRepository.cs
│   │   └── IAdRotationEngine.cs
│   ├── Enums/
│   │   ├── AdPlacementType.cs
│   │   ├── CampaignStatus.cs
│   │   ├── CampaignPricingModel.cs
│   │   └── RotationAlgorithmType.cs
│   └── Events/
│       ├── CampaignCreatedEvent.cs
│       ├── CampaignActivatedEvent.cs
│       ├── CampaignBudgetDepletedEvent.cs
│       └── CampaignCompletedEvent.cs
└── AdvertisingService.Infrastructure/
    ├── Persistence/
    │   ├── AdvertisingDbContext.cs
    │   ├── Repositories/
    │   │   ├── AdCampaignRepository.cs
    │   │   ├── AdImpressionRepository.cs
    │   │   ├── AdClickRepository.cs
    │   │   ├── RotationConfigRepository.cs
    │   │   ├── CategoryConfigRepository.cs
    │   │   └── BrandConfigRepository.cs
    │   ├── Configurations/
    │   │   ├── AdCampaignConfiguration.cs
    │   │   ├── AdImpressionConfiguration.cs
    │   │   ├── RotationConfigConfiguration.cs
    │   │   ├── CategoryImageConfigConfiguration.cs
    │   │   └── BrandConfigConfiguration.cs
    │   └── Migrations/
    ├── Services/
    │   ├── AdRotationEngine.cs
    │   ├── HomepageRotationCacheService.cs
    │   ├── QualityScoreCalculator.cs
    │   └── AdReportingService.cs
    ├── BackgroundJobs/
    │   ├── RotationRefreshJob.cs
    │   └── DailyAdReportJob.cs
    ├── Messaging/
    │   └── Consumers/
    │       └── BillingPaymentCompletedConsumer.cs
    └── DependencyInjection.cs
```

---

## 🗄️ PARTE 2: ESQUEMA DE BASE DE DATOS (`advertising_db`)

### 2.1 Entidad `AdCampaign`

```csharp
// AdvertisingService.Domain/Entities/AdCampaign.cs
public class AdCampaign : ISoftDeletable
{
    public Guid Id { get; private set; }
    public Guid VehicleId { get; private set; }       // FK a VehiclesSaleService (cross-service)
    public Guid OwnerId { get; private set; }          // UserId del vendedor O DealerId
    public string OwnerType { get; private set; }      // "Individual" | "Dealer"
    public AdPlacementType PlacementType { get; private set; } // FeaturedSpot | PremiumSpot
    public CampaignPricingModel PricingModel { get; private set; }
    public decimal? PricePerView { get; private set; } // Solo si PricingModel = PerView
    public decimal? FixedPrice { get; private set; }   // Solo si PricingModel = Fixed*
    public decimal TotalBudget { get; private set; }
    public decimal SpentBudget { get; private set; }
    public int? TotalViewsPurchased { get; private set; } // Solo si PricingModel = PerView
    public int ViewsConsumed { get; private set; }
    public int Clicks { get; private set; }
    public CampaignStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public Guid? BillingReferenceId { get; private set; }
    public decimal QualityScore { get; private set; }  // 0.0 - 1.0
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Métodos de dominio
    public void Activate(Guid billingReferenceId) { ... }
    public void RecordView() { ... }   // Descuenta si PerView, marca Completed si agotado
    public void RecordClick() { ... }
    public void Pause() { ... }
    public void Resume() { ... }
    public void Cancel() { ... }
    public bool IsActive() => Status == CampaignStatus.Active && !IsExpired();
    public bool IsExpired() => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;
    public decimal GetCtr() => ViewsConsumed > 0 ? (decimal)Clicks / ViewsConsumed : 0m;
    public decimal GetRemainingBudgetRatio() => TotalBudget > 0 ? 1 - (SpentBudget / TotalBudget) : 0m;
}
```

### 2.2 Enums EXACTOS

```csharp
// AdvertisingService.Domain/Enums/AdPlacementType.cs
public enum AdPlacementType
{
    FeaturedSpot = 0,   // Sección "⭐ Destacados" — max 8 vehículos
    PremiumSpot = 1     // Sub-sección premium dentro de destacados — max 4 vehículos
}

// CampaignStatus.cs
public enum CampaignStatus
{
    PendingPayment = 0,
    Active = 1,
    Paused = 2,
    Completed = 3,   // Se agotó presupuesto o vistas
    Cancelled = 4,   // Cancelado manualmente
    Expired = 5      // Expiró por fecha de fin
}

// CampaignPricingModel.cs
public enum CampaignPricingModel
{
    PerView = 0,        // Paga por cada vista (views_consumed <= total_views_purchased)
    FixedDaily = 1,
    FixedWeekly = 2,
    FixedMonthly = 3
}

// RotationAlgorithmType.cs
public enum RotationAlgorithmType
{
    WeightedRandom = 0,   // Default — combinación ponderada de factores
    RoundRobin = 1,       // Rotación equitativa entre todas las campañas activas
    CTROptimized = 2,     // Prioriza campañas con mayor CTR
    BudgetPriority = 3    // Prioriza campañas con mayor presupuesto restante
}
```

### 2.3 Tablas SQL (EF Core migrations generarán esto automáticamente)

```sql
-- ad_campaigns
CREATE TABLE ad_campaigns (
    id UUID PRIMARY KEY,
    vehicle_id UUID NOT NULL,
    owner_id UUID NOT NULL,
    owner_type VARCHAR(20) NOT NULL CHECK (owner_type IN ('Individual','Dealer')),
    placement_type INT NOT NULL,
    pricing_model INT NOT NULL,
    price_per_view DECIMAL(10,4),
    fixed_price DECIMAL(18,2),
    total_budget DECIMAL(18,2) NOT NULL,
    spent_budget DECIMAL(18,2) NOT NULL DEFAULT 0,
    total_views_purchased INT,
    views_consumed INT NOT NULL DEFAULT 0,
    clicks INT NOT NULL DEFAULT 0,
    status INT NOT NULL DEFAULT 0,
    start_date TIMESTAMPTZ NOT NULL,
    end_date TIMESTAMPTZ,
    billing_reference_id UUID,
    quality_score DECIMAL(3,2) NOT NULL DEFAULT 0.50,
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    deleted_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_ad_campaigns_placement_status ON ad_campaigns(placement_type, status);
CREATE INDEX idx_ad_campaigns_owner ON ad_campaigns(owner_id, owner_type);
CREATE INDEX idx_ad_campaigns_vehicle ON ad_campaigns(vehicle_id);

-- ad_impressions (tabla de solo-escritura — alta frecuencia)
CREATE TABLE ad_impressions (
    id UUID PRIMARY KEY,
    campaign_id UUID NOT NULL REFERENCES ad_campaigns(id),
    session_id VARCHAR(100),
    user_id UUID,
    ip_hash VARCHAR(64),
    section INT NOT NULL,             -- 0=FeaturedSpot, 1=PremiumSpot
    position INT NOT NULL,
    recorded_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_ad_impressions_campaign ON ad_impressions(campaign_id, recorded_at);

-- ad_clicks
CREATE TABLE ad_clicks (
    id UUID PRIMARY KEY,
    campaign_id UUID NOT NULL REFERENCES ad_campaigns(id),
    impression_id UUID,
    user_id UUID,
    recorded_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_ad_clicks_campaign ON ad_clicks(campaign_id, recorded_at);

-- rotation_configs
CREATE TABLE rotation_configs (
    id UUID PRIMARY KEY,
    section INT NOT NULL UNIQUE,          -- 0=FeaturedSpot, 1=PremiumSpot
    algorithm_type INT NOT NULL DEFAULT 0,
    refresh_interval_minutes INT NOT NULL DEFAULT 30,
    max_vehicles_shown INT NOT NULL DEFAULT 8,
    weight_remaining_budget DECIMAL(3,2) NOT NULL DEFAULT 0.30,
    weight_ctr DECIMAL(3,2) NOT NULL DEFAULT 0.25,
    weight_quality_score DECIMAL(3,2) NOT NULL DEFAULT 0.25,
    weight_recency DECIMAL(3,2) NOT NULL DEFAULT 0.20,
    is_active BOOLEAN NOT NULL DEFAULT true,
    updated_by UUID,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- category_image_configs
CREATE TABLE category_image_configs (
    id UUID PRIMARY KEY,
    category_key VARCHAR(50) NOT NULL UNIQUE,  -- 'suv' | 'sedan' | 'camioneta' | 'deportivo' | 'electrico' | 'hibrido'
    display_name VARCHAR(100) NOT NULL,
    description VARCHAR(300),
    image_url VARCHAR(500) NOT NULL,
    icon_url VARCHAR(500),
    gradient VARCHAR(100) NOT NULL DEFAULT 'from-blue-600 to-blue-800',
    vehicle_count INT NOT NULL DEFAULT 0,
    is_trending BOOLEAN NOT NULL DEFAULT false,
    display_order INT NOT NULL DEFAULT 0,
    is_visible BOOLEAN NOT NULL DEFAULT true,
    route VARCHAR(200) NOT NULL,
    updated_by UUID,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- brand_configs
CREATE TABLE brand_configs (
    id UUID PRIMARY KEY,
    brand_key VARCHAR(100) NOT NULL UNIQUE,   -- 'toyota' | 'honda' — lowercase, sin espacios
    display_name VARCHAR(100) NOT NULL,
    logo_url VARCHAR(500),
    logo_initials VARCHAR(3) NOT NULL,
    vehicle_count INT NOT NULL DEFAULT 0,
    display_order INT NOT NULL DEFAULT 0,
    is_visible BOOLEAN NOT NULL DEFAULT true,
    route VARCHAR(200) NOT NULL,
    updated_by UUID,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

---

## ⚙️ PARTE 3: ALGORITMO DE ROTACIÓN

### Archivo: `AdvertisingService.Infrastructure/Services/AdRotationEngine.cs`

Implementa `IAdRotationEngine`. Inyecta: `IAdCampaignRepository`, `IRotationConfigRepository`, `IDistributedCache` (Redis), `VehiclesServiceClient`.

### Método principal: `Task<HomepageRotationResultDto> ComputeRotationAsync(AdPlacementType section, CancellationToken ct)`

#### Algoritmo 0 — `WeightedRandom` (default para FeaturedSpot)

Paso a paso:

1. Cargar todas las campañas donde `status = Active` y `placement_type = section`
2. Filtrar: excluir campañas donde `end_date < UTC_NOW`
3. Filtrar: excluir campañas donde `pricing_model = PerView` Y `views_consumed >= total_views_purchased`
4. Si quedan 0 campañas activas → activar **FALLBACK** (ver sección 3.1)
5. Para cada campaña calcular `Score` con la fórmula:

```
Score(c) =
    (c.GetRemainingBudgetRatio() * config.WeightRemainingBudget) +
    (c.GetCtr()                  * config.WeightCtr) +
    (c.QualityScore              * config.WeightQualityScore) +
    (RecencyScore(c)             * config.WeightRecency)

RecencyScore(c) = 1.0 / (1.0 + (UtcNow - c.StartDate).TotalDays)
// Si c.GetCtr() == 0 (campaña nueva), usar CTR base = 0.02 para evitar score 0
```

6. Seleccionar `config.MaxVehiclesShown` campañas usando **weighted random sampling sin reemplazo**:
   - Normalizar scores: `weight_i = Score_i / sum(all_Scores)`
   - Usar alias method o selección proporcional acumulada
   - Asignar `position` 0..N-1 en el array de resultado

#### Algoritmo 1 — `RoundRobin`

1. Cargar campañas activas ordenadas por `created_at ASC`
2. Leer de Redis key `adrotation:roundrobin:{section}:last_index` (default 0 si no existe)
3. Tomar las siguientes `max_vehicles_shown` campañas a partir del índice, con wrap-around circular
4. Guardar nuevo índice en Redis con TTL 24 horas

#### Algoritmo 2 — `CTROptimized`

1. Cargar campañas activas
2. Para campañas con `views_consumed < 100` (nuevas), asignar CTR base = 0.02 (garantía de aparición)
3. Ordenar por CTR descendente
4. Tomar las primeras `max_vehicles_shown`

#### Algoritmo 3 — `BudgetPriority` (default para PremiumSpot)

1. Cargar campañas activas
2. Ordenar por `(total_budget - spent_budget)` descendente (mayor presupuesto restante primero)
3. Tomar las primeras `max_vehicles_shown`

### 3.1 FALLBACK cuando no hay campañas activas

Cuando no hay ninguna campaña activa para una sección, llamar a `VehiclesServiceClient.GetRecentVehiclesAsync(count: maxVehiclesShown)` y construir `HomepageVehicleAdDto` con `IsPaidAd = false` y `CampaignId = null`. El frontend NO mostrará el badge "Destacado" ni hará tracking de impresiones para estos vehículos fallback.

### 3.2 Cache en Redis

```
Key: "adrotation:homepage:featured"   TTL: (config.RefreshIntervalMinutes + 5) minutos
Key: "adrotation:homepage:premium"    TTL: (config.RefreshIntervalMinutes + 5) minutos
Key: "adrotation:brands"              TTL: 60 minutos (actualizado por DailyAdReportJob)
Key: "adrotation:categories"          TTL: 60 minutos (actualizado por DailyAdReportJob)
```

El `GetHomepageRotationQueryHandler` PRIMERO consulta Redis. Si existe y no expiró → retorna directamente. Si no existe → ejecuta `AdRotationEngine.ComputeRotationAsync()` y guarda en Redis.

### 3.3 Background Job: `RotationRefreshJob.cs`

```csharp
// IHostedService que ejecuta cada config.RefreshIntervalMinutes
// Implementación:
// 1. Para FeaturedSpot: computar rotación y guardar en Redis
// 2. Para PremiumSpot: computar rotación y guardar en Redis
// 3. Actualizar vehicle_count en brand_configs llamando VehiclesServiceClient.GetVehicleCountByBrandAsync()
// 4. Actualizar vehicle_count en category_image_configs llamando VehiclesServiceClient.GetVehicleCountByBodyTypeAsync()
// Timer: usar PeriodicTimer(.NET 6+), leer intervalo de la config de FeaturedSpot en BD
```

### 3.4 Background Job: `DailyAdReportJob.cs`

```csharp
// Se ejecuta a las 12:00 UTC (= 8:00 AM hora República Dominicana, UTC-4)
// Implementación con PeriodicTimer configurado para próximo 12:00 UTC
// 1. Consultar todos los owners con campañas activas o completadas en últimas 24h
// 2. Para cada owner: generar OwnerAdReportDto con métricas del día
// 3. Llamar NotificationServiceClient.SendEmailAsync(template: "advertising-daily-report", data: reportDto)
// 4. Para campañas con presupuesto < 10% restante: enviar alerta adicional de presupuesto bajo
```

---

## 📊 PARTE 4: QUALITY SCORE

### Archivo: `AdvertisingService.Infrastructure/Services/QualityScoreCalculator.cs`

El `quality_score` (0.0 - 1.0) se calcula al crear la campaña y se recalcula por `DailyAdReportJob`:

```csharp
// Parámetros obtenidos de VehiclesServiceClient.GetVehicleQualityDataAsync(vehicleId)
public decimal Calculate(VehicleQualityDataDto vehicle, bool ownerKycVerified)
{
    decimal score = 0m;
    int photoCount = Math.Min(vehicle.PhotoCount, 5);
    score += photoCount * 0.06m;                                          // Max 0.30
    score += !string.IsNullOrWhiteSpace(vehicle.Description) ? 0.20m : 0m;
    score += vehicle.Price > 0 ? 0.15m : 0m;
    score += vehicle.Mileage >= 0 ? 0.10m : 0m;
    score += vehicle.IsVerified ? 0.15m : 0m;
    score += ownerKycVerified ? 0.10m : 0m;
    return Math.Clamp(score, 0m, 1.00m);
}
```

---

## 🔌 PARTE 5: ENDPOINTS DE API (AdvertisingService)

### AdCampaignsController

```
POST   /api/advertising/campaigns
       Body: CreateCampaignRequest
       Auth: [Authorize(Roles = "Seller,DealerAdmin,Admin")]
       → Crea campaña en estado PendingPayment
       → Llama BillingServiceClient para generar factura
       → Retorna: ApiResponse<CreateCampaignResponse> { CampaignId, PaymentUrl }

GET    /api/advertising/campaigns/{id}
       Auth: [Authorize] + verificar que sea el owner o Admin
       → Retorna: ApiResponse<AdCampaignDto>

GET    /api/advertising/campaigns/vehicle/{vehicleId}
       Auth: [Authorize] + verificar owner o Admin
       → Retorna: ApiResponse<List<AdCampaignDto>>

GET    /api/advertising/campaigns/owner/{ownerId}?ownerType=Individual|Dealer&status=Active&page=1&pageSize=20
       Auth: [Authorize] + verificar Self o Admin
       → Retorna: ApiResponse<PagedResult<AdCampaignDto>>

PATCH  /api/advertising/campaigns/{id}/pause
       Auth: [Authorize] + verificar owner o Admin
       → Retorna: ApiResponse<bool>

PATCH  /api/advertising/campaigns/{id}/resume
       Auth: [Authorize] + verificar owner o Admin
       → Retorna: ApiResponse<bool>

PATCH  /api/advertising/campaigns/{id}/cancel
       Auth: [Authorize] + verificar owner o Admin
       → Retorna: ApiResponse<bool>
```

### AdTrackingController (SIN autenticación — fire and forget)

```
POST   /api/advertising/tracking/impression
       Body: { campaignId, section, position, sessionId, userId? }
       Auth: [AllowAnonymous]
       Rate limit: Max 100 req/min por IP (AdRateLimitMiddleware)
       → Deduplicar en Redis: key "adtrack:imp:{campaignId}:{sessionId}" TTL 24h
       → Si ya existe en Redis → 204 NoContent (ignorar silenciosamente)
       → Si es nueva → guardar en ad_impressions Y llamar campaign.RecordView()
       → Retorna: 204 NoContent (NUNCA retornar error — fire and forget)

POST   /api/advertising/tracking/click
       Body: { campaignId, impressionId? }
       Auth: [AllowAnonymous]
       → Guardar en ad_clicks Y campaign.RecordClick()
       → Retorna: 204 NoContent
```

### AdRotationController

```
GET    /api/advertising/rotation/homepage
       Auth: [AllowAnonymous]
       Cache: Redis (ver sección 3.2)
       → Retorna: ApiResponse<HomepageRotationDto>
       → HomepageRotationDto = {
           featuredVehicles: HomepageVehicleAdDto[],  // max 8
           premiumVehicles: HomepageVehicleAdDto[],   // max 4
           generatedAt: string,
           nextRefreshAt: string
         }

POST   /api/advertising/rotation/refresh
       Auth: [Authorize(Roles = "Admin")]
       → Fuerza recálculo de rotación para FeaturedSpot y PremiumSpot
       → Invalida Redis keys y recomputa
       → Retorna: ApiResponse<HomepageRotationDto>

GET    /api/advertising/rotation/config/{section}
       Auth: [Authorize(Roles = "Admin")]
       → section: "featured" | "premium"
       → Retorna: ApiResponse<RotationConfigDto>

PUT    /api/advertising/rotation/config/{section}
       Body: UpdateRotationConfigRequest
       Auth: [Authorize(Roles = "Admin")]
       → Valida que WeightRemainingBudget + WeightCtr + WeightQualityScore + WeightRecency == 1.00
       → Actualiza BD y fuerza refresh de Redis
       → Retorna: ApiResponse<RotationConfigDto>
```

### HomepageConfigController

```
GET    /api/advertising/config/categories
       Auth: [AllowAnonymous]
       Cache: Redis key "adrotation:categories" TTL 60min
       → Retorna: ApiResponse<List<CategoryConfigDto>>  (solo is_visible = true, ordenadas por display_order)

PUT    /api/advertising/config/categories/{categoryKey}
       Body: UpdateCategoryConfigRequest { displayName?, description?, isTrending?, isVisible?, displayOrder?, gradient? }
       Auth: [Authorize(Roles = "Admin")]
       → Retorna: ApiResponse<CategoryConfigDto>

POST   /api/advertising/config/categories/{categoryKey}/image
       Body: multipart/form-data con file
       Auth: [Authorize(Roles = "Admin")]
       → Llama MediaServiceClient.UploadImageAsync(file, folder: "categories")
       → Actualiza image_url en BD
       → Invalida Redis key "adrotation:categories"
       → Retorna: ApiResponse<string> (la nueva imageUrl)

GET    /api/advertising/config/brands
       Auth: [AllowAnonymous]
       Cache: Redis key "adrotation:brands" TTL 60min
       → Retorna: ApiResponse<List<BrandConfigDto>>  (solo is_visible = true, ordenadas por display_order)

PUT    /api/advertising/config/brands/{brandKey}
       Body: UpdateBrandConfigRequest { displayName?, logoInitials?, isVisible?, displayOrder? }
       Auth: [Authorize(Roles = "Admin")]
       → Retorna: ApiResponse<BrandConfigDto>

POST   /api/advertising/config/brands/{brandKey}/logo
       Body: multipart/form-data con file
       Auth: [Authorize(Roles = "Admin")]
       → Llama MediaServiceClient.UploadImageAsync(file, folder: "brands")
       → Actualiza logo_url en BD
       → Invalida Redis key "adrotation:brands"
       → Retorna: ApiResponse<string> (la nueva logoUrl)
```

### AdReportsController

```
GET    /api/advertising/reports/owner/{ownerId}?period=7d|30d|90d
       Auth: [Authorize] + verificar Self o Admin
       → Retorna: ApiResponse<OwnerAdReportDto>

GET    /api/advertising/reports/campaign/{campaignId}?period=7d|30d|90d
       Auth: [Authorize] + verificar owner o Admin
       → Retorna: ApiResponse<CampaignDetailReportDto>

GET    /api/advertising/reports/platform?period=7d|30d|90d
       Auth: [Authorize(Roles = "Admin")]
       → Retorna: ApiResponse<PlatformAdReportDto>
```

---

## 💳 PARTE 6: INTEGRACIÓN CON BILLINGSERVICE

### 6.1 Nuevo endpoint en BillingService

Agregar en `backend/BillingService/BillingService.Api/Controllers/`:

```
POST   /api/billing/advertising-invoices
       Body: CreateAdvertisingInvoiceRequest { campaignId, ownerId, ownerType, amount, currency, description }
       Auth: [Authorize] o inter-service
       → Crea factura de tipo AdvertisingCampaign
       → Retorna: { invoiceId, paymentUrl } — paymentUrl lleva al checkout de Azul/PixelPay
```

Agregar `AdvertisingCampaign = 5` al enum `BillingItemType` existente en BillingService.

### 6.2 Evento que activa la campaña

`AdvertisingService.Infrastructure/Messaging/Consumers/BillingPaymentCompletedConsumer.cs` consume el evento `billing.payment.completed` de RabbitMQ (exchange: `cardealer.events`, routing: `billing.payment.completed`):

```csharp
// Si event.ItemType == "AdvertisingCampaign":
//   1. Buscar campaña por event.ReferenceId (= campaignId)
//   2. Llamar campaign.Activate(billingReferenceId: event.PaymentId)
//   3. Guardar cambios
//   4. Forzar refresh de rotación via Redis (invalidar cache)
//   5. Publicar CampaignActivatedEvent
```

---

## 🖥️ PARTE 7: FRONTEND — Homepage (reemplazar datos hardcodeados)

### 7.1 Nuevo archivo: `frontend/web-next/src/services/advertising.ts`

```typescript
import { apiClient } from "@/lib/api-client";
import type {
  HomepageRotationDto,
  HomepageVehicleAdDto,
  BrandConfigDto,
  CategoryConfigDto,
  AdCampaignDto,
  CreateCampaignRequest,
  CreateCampaignResponse,
  TrackImpressionRequest,
  TrackClickRequest,
  RotationConfigDto,
  UpdateRotationConfigRequest,
  OwnerAdReportDto,
  PlatformAdReportDto,
} from "@/types/advertising";

export const advertisingService = {
  // ── Homepage (público) ──
  getHomepageRotation: () =>
    apiClient.get<HomepageRotationDto>("/api/advertising/rotation/homepage"),

  getCategories: () =>
    apiClient.get<CategoryConfigDto[]>("/api/advertising/config/categories"),

  getBrands: () =>
    apiClient.get<BrandConfigDto[]>("/api/advertising/config/brands"),

  // ── Tracking (fire and forget — NO usar TanStack Query) ──
  trackImpression: async (data: TrackImpressionRequest): Promise<void> => {
    try {
      await fetch("/api/advertising/tracking/impression", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
        // keepalive: true — para que se envíe incluso si el usuario navega
        keepalive: true,
      });
    } catch {
      // silencioso — tracking no debe interrumpir la experiencia
    }
  },

  trackClick: async (data: TrackClickRequest): Promise<void> => {
    try {
      await fetch("/api/advertising/tracking/click", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
        keepalive: true,
      });
    } catch {
      // silencioso
    }
  },

  // ── Campañas (Seller / Dealer) ──
  createCampaign: (data: CreateCampaignRequest) =>
    apiClient.post<CreateCampaignResponse>("/api/advertising/campaigns", data),

  getCampaignsByOwner: (
    ownerId: string,
    ownerType: "Individual" | "Dealer",
    params?: { status?: string; page?: number; pageSize?: number },
  ) =>
    apiClient.get<{ items: AdCampaignDto[]; totalCount: number }>(
      `/api/advertising/campaigns/owner/${ownerId}`,
      { params: { ownerType, ...params } },
    ),

  pauseCampaign: (id: string) =>
    apiClient.patch<void>(`/api/advertising/campaigns/${id}/pause`, {}),

  resumeCampaign: (id: string) =>
    apiClient.patch<void>(`/api/advertising/campaigns/${id}/resume`, {}),

  cancelCampaign: (id: string) =>
    apiClient.patch<void>(`/api/advertising/campaigns/${id}/cancel`, {}),

  // ── Reportes (Owner) ──
  getOwnerReport: (ownerId: string, period: "7d" | "30d" | "90d") =>
    apiClient.get<OwnerAdReportDto>(
      `/api/advertising/reports/owner/${ownerId}?period=${period}`,
    ),

  getCampaignReport: (campaignId: string, period: "7d" | "30d" | "90d") =>
    apiClient.get<unknown>(
      `/api/advertising/reports/campaign/${campaignId}?period=${period}`,
    ),

  // ── Admin ──
  getRotationConfig: (section: "featured" | "premium") =>
    apiClient.get<RotationConfigDto>(
      `/api/advertising/rotation/config/${section}`,
    ),

  updateRotationConfig: (
    section: "featured" | "premium",
    data: UpdateRotationConfigRequest,
  ) =>
    apiClient.put<RotationConfigDto>(
      `/api/advertising/rotation/config/${section}`,
      data,
    ),

  forceRotationRefresh: () =>
    apiClient.post<HomepageRotationDto>(
      "/api/advertising/rotation/refresh",
      {},
    ),

  updateCategoryConfig: (key: string, data: Partial<CategoryConfigDto>) =>
    apiClient.put<CategoryConfigDto>(
      `/api/advertising/config/categories/${key}`,
      data,
    ),

  uploadCategoryImage: (key: string, file: File) => {
    const form = new FormData();
    form.append("file", file);
    return apiClient.post<string>(
      `/api/advertising/config/categories/${key}/image`,
      form,
    );
  },

  updateBrandConfig: (key: string, data: Partial<BrandConfigDto>) =>
    apiClient.put<BrandConfigDto>(
      `/api/advertising/config/brands/${key}`,
      data,
    ),

  uploadBrandLogo: (key: string, file: File) => {
    const form = new FormData();
    form.append("file", file);
    return apiClient.post<string>(
      `/api/advertising/config/brands/${key}/logo`,
      form,
    );
  },

  getPlatformReport: (period: "7d" | "30d" | "90d") =>
    apiClient.get<PlatformAdReportDto>(
      `/api/advertising/reports/platform?period=${period}`,
    ),
};
```

### 7.2 Nuevo archivo: `frontend/web-next/src/types/advertising.ts`

```typescript
export type AdPlacementType = "FeaturedSpot" | "PremiumSpot";
export type CampaignStatus =
  | "PendingPayment"
  | "Active"
  | "Paused"
  | "Completed"
  | "Cancelled"
  | "Expired";
export type CampaignPricingModel =
  | "PerView"
  | "FixedDaily"
  | "FixedWeekly"
  | "FixedMonthly";
export type RotationAlgorithmType =
  | "WeightedRandom"
  | "RoundRobin"
  | "CTROptimized"
  | "BudgetPriority";
export type AdReportPeriod = "7d" | "30d" | "90d";

export interface HomepageVehicleAdDto {
  vehicleId: string;
  campaignId: string | null; // null si es fallback (no es ad pagada)
  isPaidAd: boolean; // false si es fallback
  title: string;
  make: string;
  model: string;
  year: number;
  price: number;
  currency: string;
  mainImageUrl: string;
  mileage: number;
  condition: string;
  slug: string;
  ownerKycVerified: boolean;
  placementType: AdPlacementType;
  position: number;
}

export interface HomepageRotationDto {
  featuredVehicles: HomepageVehicleAdDto[];
  premiumVehicles: HomepageVehicleAdDto[];
  generatedAt: string;
  nextRefreshAt: string;
}

export interface BrandConfigDto {
  brandKey: string;
  displayName: string;
  logoUrl: string | null;
  logoInitials: string;
  vehicleCount: number;
  displayOrder: number;
  isVisible: boolean;
  route: string;
}

export interface CategoryConfigDto {
  categoryKey: string;
  displayName: string;
  description: string;
  imageUrl: string;
  gradient: string;
  vehicleCount: number;
  isTrending: boolean;
  displayOrder: number;
  isVisible: boolean;
  route: string;
}

export interface AdCampaignDto {
  id: string;
  vehicleId: string;
  vehicleTitle: string;
  vehicleImageUrl: string;
  vehicleSlug: string;
  placementType: AdPlacementType;
  pricingModel: CampaignPricingModel;
  totalBudget: number;
  spentBudget: number;
  remainingBudgetPercent: number;
  totalViewsPurchased: number | null;
  viewsConsumed: number;
  viewsRemaining: number | null;
  clicks: number;
  ctr: number;
  status: CampaignStatus;
  startDate: string;
  endDate: string | null;
  qualityScore: number;
}

export interface CreateCampaignRequest {
  vehicleId: string;
  ownerId: string;
  ownerType: "Individual" | "Dealer";
  placementType: AdPlacementType;
  planId: string; // 'basic' | 'pro' | 'premium' | 'dealer-esencial' | etc.
}

export interface CreateCampaignResponse {
  campaignId: string;
  status: CampaignStatus;
  paymentUrl: string; // URL de checkout de Azul/PixelPay
}

export interface TrackImpressionRequest {
  campaignId: string;
  section: string;
  position: number;
  sessionId: string;
}

export interface TrackClickRequest {
  campaignId: string;
  impressionId?: string;
}

export interface RotationConfigDto {
  section: string;
  algorithmType: RotationAlgorithmType;
  refreshIntervalMinutes: number;
  maxVehiclesShown: number;
  weightRemainingBudget: number;
  weightCtr: number;
  weightQualityScore: number;
  weightRecency: number;
}

export interface UpdateRotationConfigRequest {
  algorithmType: RotationAlgorithmType;
  refreshIntervalMinutes: number;
  maxVehiclesShown: number;
  weightRemainingBudget: number;
  weightCtr: number;
  weightQualityScore: number;
  weightRecency: number;
}

export interface OwnerAdReportDto {
  ownerId: string;
  ownerType: string;
  period: string;
  totalSpend: number;
  totalViews: number;
  totalClicks: number;
  averageCtr: number;
  activeCampaigns: number;
  campaignBreakdown: CampaignReportItemDto[];
  viewsByDay: DailyMetricDto[];
}

export interface CampaignReportItemDto {
  campaignId: string;
  vehicleTitle: string;
  vehicleImageUrl: string;
  views: number;
  clicks: number;
  ctr: number;
  spend: number;
  remainingBudgetPercent: number;
  status: CampaignStatus;
}

export interface DailyMetricDto {
  date: string;
  views: number;
  clicks: number;
}

export interface PlatformAdReportDto {
  period: string;
  totalRevenue: number;
  totalActiveCampaigns: number;
  totalViews: number;
  totalClicks: number;
  averageCtr: number;
  revenueByDay: DailyMetricDto[];
  topCampaigns: CampaignReportItemDto[];
}
```

### 7.3 Modificar: `frontend/web-next/src/components/homepage/brand-slider.tsx`

**OBJETIVO:** Reemplazar `DEFAULT_BRANDS` hardcodeados por datos de `AdvertisingService`.

1. Eliminar la constante `DEFAULT_BRANDS`
2. Agregar prop `brands?: BrandConfigDto[]` al componente `BrandSliderProps`
3. Usar la prop `brands` en lugar del array hardcodeado
4. Mantener el scroll infinito CSS animation actual (no cambiar la lógica de animación)
5. Actualizar el `BrandLogoPlaceholder` para usar `brand.logoInitials` en lugar de `name.substring(0, 2)`
6. El link de cada marca cambia de `/vehiculos?make=${brand.slug}` a `brand.route`
7. Si `brand.logoUrl` existe → usar `<Image>` con esa URL. Si no → usar `BrandLogoPlaceholder` con `brand.logoInitials`

### 7.4 Modificar: `frontend/web-next/src/components/homepage/category-cards.tsx`

**OBJETIVO:** Reemplazar `DEFAULT_CATEGORIES` hardcodeados por datos de `AdvertisingService`.

1. Cambiar el tipo de la prop `categories` de `CategoryCard[]` a `CategoryConfigDto[]`
2. El mapping interno adapta `CategoryConfigDto` al render existente:
   - `isTrending` → badge "Trending"
   - `imageUrl` → background image (si existe). Si `imageUrl` es vacío → usar `gradient` del CSS
   - `route` → `href` del link "Explorar"
   - `vehicleCount` → el número de vehículos
3. Eliminar `DEFAULT_CATEGORIES` hardcodeado — si el prop `categories` está vacío, mostrar skeleton de 6 cards

### 7.5 Nuevo componente: `frontend/web-next/src/components/homepage/featured-ad-card.tsx`

Este componente reemplaza o complementa el card de vehículo existente, agregando:

- Badge "⭐ Destacado" (dorado) si `vehicle.isPaidAd === true`
- Badge "Verificado" (verde con ícono shield) si `vehicle.ownerKycVerified === true`
- Tracking de impresiones con `IntersectionObserver` (ver hook 7.6)
- Click handler que llama `advertisingService.trackClick()` antes de navegar

```tsx
"use client";

interface FeaturedAdCardProps {
  vehicle: HomepageVehicleAdDto;
  sessionId: string;
}

// Usa IntersectionObserver para detectar visibilidad (threshold: 0.5)
// Al entrar en viewport: llama advertisingService.trackImpression() UNA SOLA VEZ por sesión
// Deduplicación: sessionStorage key "ad_imp_{vehicle.campaignId}"
// Al hacer click: llama advertisingService.trackClick() y LUEGO navega a /vehiculos/{vehicle.slug}
```

### 7.6 Nuevo hook: `frontend/web-next/src/hooks/use-ad-session.ts`

```typescript
// Genera y persiste un sessionId anónimo en sessionStorage
// Key: "okla_ad_session"
// Valor: crypto.randomUUID() generado una sola vez por sesión
export function useAdSession(): string;
```

### 7.7 Modificar: `frontend/web-next/src/app/(main)/homepage-client.tsx`

1. Agregar en los imports: `FeaturedAdCard`, `useAdSession`
2. Agregar state/query para la rotación de anuncios:
   ```tsx
   const { data: adRotation } = useQuery({
     queryKey: ["homepage-ad-rotation"],
     queryFn: () => advertisingService.getHomepageRotation(),
     staleTime: 5 * 60 * 1000, // 5 minutos (sincronizar con TTL Redis)
   });
   ```
3. Agregar queries para categorías y marcas dinámicas:

   ```tsx
   const { data: dynamicBrands } = useQuery({
     queryKey: ["homepage-brands"],
     queryFn: () => advertisingService.getBrands(),
     staleTime: 60 * 60 * 1000, // 60 minutos
   });

   const { data: dynamicCategories } = useQuery({
     queryKey: ["homepage-categories"],
     queryFn: () => advertisingService.getCategories(),
     staleTime: 60 * 60 * 1000,
   });
   ```

4. Pasar `adRotation?.featuredVehicles` al componente de Destacados
5. Pasar `dynamicBrands` al componente `BrandSlider` (fallback al array interno si `dynamicBrands` es undefined)
6. Pasar `dynamicCategories` al componente `CategoryCards` (fallback al array interno si undefined)

### 7.8 Modificar: `frontend/web-next/src/app/(main)/page.tsx`

En el Server Component, hacer fetch en paralelo al iniciar:

```tsx
// Fetch en paralelo (Promise.allSettled para que un fallo no rompa toda la página)
const [sectionsResult, adRotationResult, brandsResult, categoriesResult] =
  await Promise.allSettled([
    getHomepageSections(),
    getAdRotation(), // GET /api/advertising/rotation/homepage
    getBrandConfigs(), // GET /api/advertising/config/brands
    getCategoryConfigs(), // GET /api/advertising/config/categories
  ]);

// Pasar datos precargados a HomepageClient via props para evitar waterfall
```

---

## 🏪 PARTE 8: FRONTEND — Portal Vendedor Individual (Actualizar boost page existente)

### 8.1 Modificar: `frontend/web-next/src/app/(main)/mis-vehiculos/[id]/boost/page.tsx`

**ESTADO ACTUAL:** La página ya existe como UI skeleton con `usePlatformPricing`. Necesita conectarse al backend real.

**CAMBIOS EXACTOS:**

1. Importar `advertisingService` desde `@/services/advertising`
2. Importar `useMutation` de `@tanstack/react-query`
3. Importar `useAuthStore` de `@/stores/auth-store` para obtener `userId`
4. Reemplazar el `onClick` del botón de cada plan por la llamada real:

```tsx
const { mutate: createCampaign, isPending } = useMutation({
  mutationFn: (planId: string) =>
    advertisingService.createCampaign({
      vehicleId: params.id,
      ownerId: user!.id,
      ownerType: "Individual",
      placementType: "FeaturedSpot",
      planId,
    }),
  onSuccess: (data) => {
    // Redirigir al paymentUrl devuelto por BillingService
    window.location.href = data.paymentUrl;
  },
  onError: (error) => {
    toast.error("No se pudo iniciar la promoción. Intenta de nuevo.");
  },
});
```

5. Los planes de precios para vendedores individuales son:

| planId    | Nombre        | Precio DOP | Vistas | Duración |
| --------- | ------------- | ---------- | ------ | -------- |
| `basic`   | Boost Básico  | RD$499     | 500    | 15 días  |
| `pro`     | Boost Pro     | RD$999     | 1,200  | 30 días  |
| `premium` | Boost Premium | RD$1,999   | 3,000  | 60 días  |

> **NOTA:** Estos precios REEMPLAZAN los valores de `usePlatformPricing()` — el backend `AdvertisingService` maneja los precios. Simplificar eliminando `usePlatformPricing` para los boost plans y usar los precios hardcodeados en el array de planes (son precios de negocio, no configuración dinámica por ahora).

### 8.2 Nueva ruta: `frontend/web-next/src/app/(main)/mis-vehiculos/publicidad/page.tsx`

**Dashboard de publicidad del vendedor individual:**

```
URL: /mis-vehiculos/publicidad
Auth: Requiere [Authorize] — verificar en middleware

Secciones:
1. Header: "Mi Publicidad" + botón "Destacar Vehículo" → navega a /mis-vehiculos para elegir vehículo

2. KPI Cards (3 tarjetas en row):
   - Total Vistas este mes (ícono Eye)
   - Total Clicks este mes (ícono MousePointer)
   - CTR promedio % (ícono TrendingUp)
   Datos: useQuery → advertisingService.getOwnerReport(userId, '30d')

3. Gráfica de rendimiento:
   - Recharts LineChart con 2 líneas: "Vistas" y "Clicks"
   - Eje X: últimos 30 días
   - Datos: reportData.viewsByDay
   - Selector de período: 7d | 30d | 90d (cambia el query)

4. Tabla de campañas activas:
   - Columnas: Vehículo (foto+nombre), Tipo, Vistas, Clicks, CTR%, Estado, Acciones
   - Estado como Badge con colores: Active=verde, Paused=amarillo, Completed=gris, Cancelled=rojo
   - Acciones por fila: "Pausar" (si Active), "Reactivar" (si Paused), "Ver reporte"
   - useQuery → advertisingService.getCampaignsByOwner(userId, 'Individual')
   - useMutation para pause/resume/cancel

5. Si NO hay campañas activas: EmptyState con ícono Sparkles y CTA "Destaca tu primer vehículo"
```

---

## 🏢 PARTE 9: FRONTEND — Portal Dealer (nuevo dashboard de publicidad)

### 9.1 Nueva ruta: `frontend/web-next/src/app/(main)/dealer/publicidad/page.tsx`

**Dashboard de publicidad del dealer:**

```
URL: /dealer/publicidad
Auth: Requiere role DealerAdmin

Layout: Igual al layout del dealer (usa el layout.tsx existente en /dealer/)

Secciones:
1. Header: "Publicidad" + botón "Destacar Vehículo" (abre Drawer)

2. KPI Cards (4 tarjetas):
   - Inversión este mes (RD$) (ícono DollarSign)
   - Total Vistas (ícono Eye)
   - Total Clicks (ícono MousePointer)
   - CTR promedio % (ícono TrendingUp)
   Datos: advertisingService.getOwnerReport(dealerId, '30d')

3. Gráfica de rendimiento mensual:
   - Recharts AreaChart (área con gradiente)
   - Eje X: últimos 30 días
   - Series: "Vistas" y "Clicks"
   - Selector: 7d | 30d | 90d

4. Tabla de campañas:
   - Columnas: Vehículo, Placement (FeaturedSpot/PremiumSpot), Vistas, Clicks, CTR%, Gasto, Estado, Acciones
   - Filtro por estado (All | Active | Paused | Completed)
   - Paginación: 20 items por página
   - Acciones: Pausar/Reactivar, Ver reporte detallado
   - Botón "Exportar CSV" en el header de la tabla

5. Drawer lateral "Destacar Vehículo":
   - Paso 1: Buscador de vehículos del inventario del dealer (useVehiclesByDealer)
   - Paso 2: Selección de plan (planes dealer — ver tabla abajo)
   - Paso 3: Confirmación → llama advertisingService.createCampaign()
   - Cierra el drawer y refresca la tabla al completar
```

**Planes de precios para dealers:**

| planId              | Nombre      | Precio DOP | Vistas | Duración | Extra                                |
| ------------------- | ----------- | ---------- | ------ | -------- | ------------------------------------ |
| `dealer-esencial`   | Esencial    | RD$2,499   | 3,000  | 30 días  | 1 vehículo                           |
| `dealer-pro`        | Profesional | RD$5,999   | 8,000  | 30 días  | Hasta 3 vehículos simultáneos        |
| `dealer-enterprise` | Enterprise  | RD$14,999  | 25,000 | 30 días  | Hasta 10 vehículos + reporte semanal |

### 9.2 Modificar: `frontend/web-next/src/app/(main)/dealer/inventario/page.tsx`

Agregar columna "Publicidad" a la tabla de inventario (DESPUÉS de la columna de Estado):

1. Para cada vehículo llamar a `advertisingService.getCampaignsByVehicle(vehicle.id)` — en batch si es posible
2. Si el vehículo tiene campaña activa → mostrar Badge estrella dorada con `viewsRemaining` restantes
3. Si NO tiene campaña activa → mostrar botón pequeño "Destacar" con ícono `Sparkles`
4. Click en "Destacar" → navega a `/dealer/publicidad` con param `?vehicleId={vehicle.id}` que preselecciona el vehículo en el drawer

Agregar en el menú de dropdown de cada vehículo (`DropdownMenuItem`):

```tsx
<DropdownMenuItem
  onClick={() => router.push(`/dealer/publicidad?vehicleId=${vehicle.id}`)}
>
  <Sparkles className="mr-2 h-4 w-4 text-amber-500" />
  Destacar vehículo
</DropdownMenuItem>
```

Agregar en el navbar del dealer (en `frontend/web-next/src/app/(main)/dealer/layout.tsx`):

- Nuevo ítem de navegación: "Publicidad" con ícono `Megaphone` → `/dealer/publicidad`
- Posición: entre "Analytics" y "Facturación"

---

## 🔧 PARTE 10: FRONTEND — Panel de Administración

### 10.1 Nueva ruta: `frontend/web-next/src/app/(main)/dashboard/publicidad/page.tsx`

> **NOTA:** El panel admin usa la ruta `/dashboard/` según la estructura actual del proyecto (no `/admin/`).

**Dashboard de publicidad — Vista plataforma:**

```
URL: /dashboard/publicidad
Auth: Requiere role Admin (usar AdminAuthGuard existente en src/components/admin/admin-auth-guard.tsx)

Secciones:
1. Header: "Publicidad & Monetización"

2. KPI Cards (4 tarjetas):
   - Ingresos del mes (RD$) (ícono TrendingUp)
   - Campañas activas (#) (ícono Megaphone)
   - Total Vistas plataforma (ícono Eye)
   - CTR promedio plataforma (ícono MousePointer)
   Datos: advertisingService.getPlatformReport('30d')

3. Gráfica de ingresos:
   - Recharts BarChart (barras por día)
   - Datos: platformReport.revenueByDay
   - Selector: 7d | 30d | 90d

4. Top 10 campañas de mejor rendimiento:
   - Tabla con: Vehículo, Owner, Tipo, Vistas, Clicks, CTR%, Gasto

5. AlertaS de campañas con presupuesto < 10%:
   - Lista de campañas cercanas a agotarse con badge "⚠️ Presupuesto bajo"
   - CTA: enviar notificación al owner

6. Botón "Ir a Configurar Algoritmo" → /dashboard/publicidad/algoritmo
```

### 10.2 Nueva ruta: `frontend/web-next/src/app/(main)/dashboard/publicidad/algoritmo/page.tsx`

**Configuración del algoritmo de rotación:**

```tsx
// Datos: 2 tabs — "Sección Destacados (FeaturedSpot)" y "Sección Premium (PremiumSpot)"
// Para cada tab, un formulario con react-hook-form + Zod:

const rotationConfigSchema = z
  .object({
    algorithmType: z.enum([
      "WeightedRandom",
      "RoundRobin",
      "CTROptimized",
      "BudgetPriority",
    ]),
    refreshIntervalMinutes: z.number().int().min(5).max(1440),
    maxVehiclesShown: z.number().int().min(1).max(20),
    weightRemainingBudget: z.number().min(0).max(1),
    weightCtr: z.number().min(0).max(1),
    weightQualityScore: z.number().min(0).max(1),
    weightRecency: z.number().min(0).max(1),
  })
  .refine(
    (data) => {
      if (data.algorithmType !== "WeightedRandom") return true;
      const total =
        data.weightRemainingBudget +
        data.weightCtr +
        data.weightQualityScore +
        data.weightRecency;
      return Math.abs(total - 1.0) < 0.001; // tolerancia de floating point
    },
    {
      message: "La suma de los pesos debe ser exactamente 100%",
      path: ["weightRemainingBudget"],
    },
  );

// Campos del formulario:
// 1. RadioGroup "Algoritmo de rotación" con 4 opciones, cada una con descripción:
//    - WeightedRandom: "Mezcla ponderada de múltiples factores (recomendado)"
//    - RoundRobin: "Rotación equitativa entre todas las campañas activas"
//    - CTROptimized: "Prioriza campañas con mayor tasa de clicks"
//    - BudgetPriority: "Prioriza campañas con mayor presupuesto restante"

// 2. Select "Intervalo de actualización":
//    - Opciones: 5 min | 15 min | 30 min | 60 min | 120 min

// 3. Input numérico "Máximo de vehículos a mostrar" (1-20)

// 4. Sección "Pesos del algoritmo" — visible SOLO si algorithmType === 'WeightedRandom':
//    Cuatro Sliders (0-100 con step 5):
//    - "Presupuesto restante" (weight_remaining_budget × 100)
//    - "Tasa de clicks (CTR)" (weight_ctr × 100)
//    - "Calidad del vehículo" (weight_quality_score × 100)
//    - "Recencia de inicio" (weight_recency × 100)
//    Indicador en tiempo real: "Suma: {total}% {total === 100 ? '✓' : '⚠️ Debe ser 100%'}"

// 5. Botones:
//    - "Guardar configuración" → PUT /api/advertising/rotation/config/{section}
//    - "Forzar rotación ahora" → POST /api/advertising/rotation/refresh
//    Ambos con toast de confirmación (sonner)
```

### 10.3 Nueva ruta: `frontend/web-next/src/app/(main)/dashboard/contenido/categorias/page.tsx`

**Gestión de imágenes de categorías:**

```
URL: /dashboard/contenido/categorias

Layout:
- Header: "Imágenes de Categorías" + descripción
- Grid de 6 cards (3×2 en desktop), una por categoría: suv, sedan, camioneta, deportivo, electrico, hibrido

Cada card contiene:
- Preview de imagen actual (200×120px con object-cover)
- Nombre de la categoría
- Switch "Visible en homepage" → toggle is_visible
- Toggle "Trending" → toggle is_trending
- Badge con vehicle_count
- Botón "Cambiar imagen" → abre ImageUploadModal

ImageUploadModal (componente compartido en src/components/admin/image-upload-modal.tsx):
- Título: "Cambiar imagen de {categoryName}"
- Zona drag & drop (usar input[type=file] con drag events en lugar de react-dropzone)
  - Accept: image/jpeg, image/png, image/webp
  - Max size: 5MB
  - Preview inmediata con URL.createObjectURL
- Botón "Subir imagen"
  → llama advertisingService.uploadCategoryImage(categoryKey, file)
  → Toast "Imagen actualizada correctamente"
  → Cierra modal y refresca la card

- Drag & drop para reordenar las 6 cards (usar @dnd-kit/sortable ya instalado)
  → Al reordenar, llama advertisingService.updateCategoryConfig(key, { displayOrder: newIndex })
  → Optimistic update: reordenar visualmente ANTES de que responda la API
```

### 10.4 Nueva ruta: `frontend/web-next/src/app/(main)/dashboard/contenido/marcas/page.tsx`

**Gestión de logos de marcas:**

```
URL: /dashboard/contenido/marcas

Layout:
- Header: "Logos de Marcas" + botón "Agregar marca"
- Grid de cards (5 columnas en desktop, 3 en tablet, 2 en mobile)
- Buscador/filtro por nombre de marca

Cada card de marca:
- Logo circular (64×64px) — si logo_url existe: <Image src={logo_url} /> — si no: iniciales con fondo gris
- Nombre de la marca (display_name)
- Número de vehículos (vehicle_count) con badge gris
- Switch "Visible" → toggle is_visible
- Botón "Cambiar logo" → abre mismo ImageUploadModal pero llama uploadBrandLogo()
- Input inline editable para Iniciales (máx 3 chars) con botón guardar
  → llama advertisingService.updateBrandConfig(key, { logoInitials: value })

Botón "Agregar marca":
- Modal con formulario: displayName, brandKey (auto-generado desde displayName), logoInitials
- Al guardar → llama POST /api/advertising/config/brands (nuevo endpoint)
- Las marcas nuevas tienen display_order máximo + 1

Drag & drop para reordenar (igual que categorías)
```

### 10.5 Agregar ítems en el menú del dashboard admin

Modificar el componente de navegación del admin (buscar en `frontend/web-next/src/components/navigation/` o `src/app/(main)/dashboard/layout.tsx`) para agregar:

```tsx
// Sección "Monetización":
{ label: 'Publicidad', href: '/dashboard/publicidad', icon: Megaphone }

// Sección "Contenido":
{ label: 'Categorías', href: '/dashboard/contenido/categorias', icon: LayoutGrid }
{ label: 'Marcas', href: '/dashboard/contenido/marcas', icon: Car }
```

---

## 🔗 PARTE 11: CAMBIOS EN SERVICIOS EXISTENTES

### 11.1 VehiclesSaleService — Nuevo endpoint interno

Agregar en `backend/VehiclesSaleService/VehiclesSaleService.Api/Controllers/VehiclesController.cs`:

```csharp
// GET /api/vehicles/advertising-info?vehicleIds=id1,id2,id3
// [AllowAnonymous] — endpoint interno, protegido por red K8s
// Query: GetVehiclesAdvertisingInfoQuery { VehicleIds: List<Guid> }
// Response: ApiResponse<List<VehicleAdvertisingInfoDto>>
// VehicleAdvertisingInfoDto: { Id, Title, Make, Model, Year, Price, Currency, MainImageUrl, Mileage, Condition, Slug, OwnerKycVerified, PhotoCount, HasDescription, IsVerified }
```

### 11.2 Gateway — Nuevas rutas

Agregar en `backend/Gateway/Gateway.Api/ocelot.Development.json`:

```json
{
  "UpstreamPathTemplate": "/api/advertising/{everything}",
  "UpstreamHttpMethod": ["GET", "POST", "PUT", "PATCH", "DELETE"],
  "DownstreamPathTemplate": "/api/advertising/{everything}",
  "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 15145 }],
  "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
},
{
  "UpstreamPathTemplate": "/api/advertising/rotation/homepage",
  "UpstreamHttpMethod": ["GET"],
  "DownstreamPathTemplate": "/api/advertising/rotation/homepage",
  "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 15145 }]
},
{
  "UpstreamPathTemplate": "/api/advertising/config/categories",
  "UpstreamHttpMethod": ["GET"],
  "DownstreamPathTemplate": "/api/advertising/config/categories",
  "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 15145 }]
},
{
  "UpstreamPathTemplate": "/api/advertising/config/brands",
  "UpstreamHttpMethod": ["GET"],
  "DownstreamPathTemplate": "/api/advertising/config/brands",
  "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 15145 }]
},
{
  "UpstreamPathTemplate": "/api/advertising/tracking/{everything}",
  "UpstreamHttpMethod": ["POST"],
  "DownstreamPathTemplate": "/api/advertising/tracking/{everything}",
  "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 15145 }]
}
```

En `backend/Gateway/Gateway.Api/ocelot.prod.json`, las mismas rutas pero con `"Port": 8080` y `"Host": "advertisingservice"`.

---

## 🏗️ PARTE 12: `Program.cs` COMPLETO DE `AdvertisingService`

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Secrets
builder.Configuration.AddMicroserviceSecrets();

// 2. Serilog (NO CreateBootstrapLogger — NUNCA)
builder.UseStandardSerilog();

// 3. Database (PostgreSQL + EF Core + retry + auto-migrate)
builder.Services.AddStandardDatabase<AdvertisingDbContext>(builder.Configuration);

// 4. RabbitMQ
builder.Services.AddStandardRabbitMq(builder.Configuration);

// 5. Dead Letter Queue — DEBE ir ANTES de los HostedServices
builder.Services.AddPostgreSqlDeadLetterQueue(builder.Configuration);
builder.Services.AddSingleton<IDeadLetterQueue, InMemoryDeadLetterQueue>(); // ← CRÍTICO

// 6. Observability
builder.Services.AddStandardObservability(builder.Configuration, "AdvertisingService");

// 7. Error handling
builder.Services.AddGlobalErrorHandling(builder.Configuration);

// 8. Redis (IDistributedCache)
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "advertising:";
});

// 9. HttpClients para servicios externos
builder.Services.AddHttpClient<VehiclesServiceClient>(client => {
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:VehiclesService"]!);
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHttpClient<BillingServiceClient>(client => {
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:BillingService"]!);
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHttpClient<MediaServiceClient>(client => {
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:MediaService"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpClient<NotificationServiceClient>(client => {
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:NotificationService"]!);
    client.Timeout = TimeSpan.FromSeconds(10);
});

// 10. Repositorios
builder.Services.AddScoped<IAdCampaignRepository, AdCampaignRepository>();
builder.Services.AddScoped<IAdImpressionRepository, AdImpressionRepository>();
builder.Services.AddScoped<IAdClickRepository, AdClickRepository>();
builder.Services.AddScoped<IRotationConfigRepository, RotationConfigRepository>();
builder.Services.AddScoped<ICategoryConfigRepository, CategoryConfigRepository>();
builder.Services.AddScoped<IBrandConfigRepository, BrandConfigRepository>();

// 11. Application services
builder.Services.AddScoped<IAdRotationEngine, AdRotationEngine>();
builder.Services.AddScoped<HomepageRotationCacheService>();
builder.Services.AddScoped<QualityScoreCalculator>();
builder.Services.AddScoped<AdReportingService>();

// 12. Message consumers
builder.Services.AddScoped<BillingPaymentCompletedConsumer>();

// 13. Background Jobs — DESPUÉS de registrar todas las dependencias
builder.Services.AddHostedService<RotationRefreshJob>();
builder.Services.AddHostedService<DailyAdReportJob>();

// 14. MediatR + ValidationBehavior
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateCampaignCommand).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// 15. FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateCampaignCommandValidator).Assembly);

// 16. Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new() { Title = "AdvertisingService API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });
});

// 17. CORS
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "https://okla.com.do" };
        policy.WithOrigins(origins)
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization", "X-Idempotency-Key")
              .AllowCredentials();
    });
});

// 18. JWT
var (jwtKey, jwtIssuer, jwtAudience) = MicroserviceSecretsConfiguration.GetJwtConfig(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true, ValidIssuer = jwtIssuer,
            ValidateAudience = true, ValidAudience = jwtAudience,
            ValidateLifetime = true, ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// 19. Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, tags: new[] { "ready" })
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, tags: new[] { "ready" })
    .AddRabbitMQ(tags: new[] { "ready" });

var app = builder.Build();

// ── Middleware pipeline — orden canónico CRÍTICO ──
app.UseGlobalErrorHandling();
app.UseRequestLogging();
app.UseApiSecurityHeaders(!app.Environment.IsDevelopment());
if (!app.Environment.IsProduction()) app.UseHttpsRedirection();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseAuditMiddleware();
app.MapControllers();

// Health checks — SIEMPRE excluir "external" del endpoint /health
app.MapHealthChecks("/health", new HealthCheckOptions {
    Predicate = check => !check.Tags.Contains("external")
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions {
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions {
    Predicate = _ => false
});
app.UsePrometheusScrapingEndpoint();

app.Run();

public partial class Program { } // ← OBLIGATORIO al final
```

---

## 🌱 PARTE 13: SEEDING DE DATOS INICIALES

En `AdvertisingService.Infrastructure/Persistence/AdvertisingDbContext.cs`, método `OnModelCreating`:

### RotationConfig seeding:

```csharp
modelBuilder.Entity<RotationConfig>().HasData(
    new RotationConfig {
        Id = Guid.Parse("11111111-0000-0000-0000-000000000001"),
        Section = AdPlacementType.FeaturedSpot,
        AlgorithmType = RotationAlgorithmType.WeightedRandom,
        RefreshIntervalMinutes = 30,
        MaxVehiclesShown = 8,
        WeightRemainingBudget = 0.30m,
        WeightCtr = 0.25m,
        WeightQualityScore = 0.25m,
        WeightRecency = 0.20m,
        IsActive = true
    },
    new RotationConfig {
        Id = Guid.Parse("11111111-0000-0000-0000-000000000002"),
        Section = AdPlacementType.PremiumSpot,
        AlgorithmType = RotationAlgorithmType.BudgetPriority,
        RefreshIntervalMinutes = 60,
        MaxVehiclesShown = 4,
        WeightRemainingBudget = 0.40m,
        WeightCtr = 0.20m,
        WeightQualityScore = 0.20m,
        WeightRecency = 0.20m,
        IsActive = true
    }
);
```

### BrandConfig seeding (12 marcas — DATOS EXACTOS del diseño):

```csharp
// Orden y datos EXACTOS del carrusel del homepage actual
modelBuilder.Entity<BrandConfig>().HasData(
    new BrandConfig { Id = Guid.NewGuid(), BrandKey = "toyota",        DisplayName = "Toyota",        LogoInitials = "TO", VehicleCount = 0, DisplayOrder = 1, IsVisible = true, Route = "/buscar?marca=toyota" },
    new BrandConfig { Id = Guid.NewGuid(), BrandKey = "honda",         DisplayName = "Honda",         LogoInitials = "HO", VehicleCount = 0, DisplayOrder = 2, IsVisible = true, Route = "/buscar?marca=honda" },
    new BrandConfig { Id = Guid.NewGuid(), BrandKey = "hyundai",       DisplayName = "Hyundai",       LogoInitials = "HY", VehicleCount = 0, DisplayOrder = 3, IsVisible = true, Route = "/buscar?marca=hyundai" },
    new BrandConfig { Id = Guid.NewGuid(), BrandKey = "kia",           DisplayName = "Kia",           LogoInitials = "KI", VehicleCount = 0, DisplayOrder = 4, IsVisible = true, Route = "/buscar?marca=kia" },
    new BrandConfig { Id = Guid.NewGuid(), BrandKey = "nissan",        DisplayName = "Nissan",        LogoInitials = "NI", VehicleCount = 0, DisplayOrder = 5, IsVisible = true, Route = "/buscar?marca=nissan" },
    new BrandConfig { Id = Guid.NewGuid(), BrandKey = "mazda",         DisplayName = "Mazda",         LogoInitials = "MA", VehicleCount = 0, DisplayOrder = 6, IsVisible = true, Route = "/buscar?marca=mazda" },
    new BrandConfig { Id = Guid.NewGuid(), BrandKey = "ford",          DisplayName = "Ford",          LogoInitials = "FO", VehicleCount = 0, DisplayOrder = 7, IsVisible = true, Route = "/buscar?marca=ford" },
    new BrandConfig { Id = Guid.NewGuid(), BrandKey = "chevrolet",     DisplayName = "Chevrolet",     LogoInitials = "CH", VehicleCount = 0, DisplayOrder = 8, IsVisible = true, Route = "/buscar?marca=chevrolet" },
    new BrandConfig { Id = Guid.NewGuid(), BrandKey = "bmw",           DisplayName = "BMW",           LogoInitials = "BM", VehicleCount = 0, DisplayOrder = 9, IsVisible = true, Route = "/buscar?marca=bmw" },
    new BrandConfig { Id = Guid.NewGuid(), BrandKey = "mercedes-benz", DisplayName = "Mercedes-Benz", LogoInitials = "ME", VehicleCount = 0, DisplayOrder = 10, IsVisible = true, Route = "/buscar?marca=mercedes-benz" },
    new BrandConfig { Id = Guid.NewGuid(), BrandKey = "audi",          DisplayName = "Audi",          LogoInitials = "AU", VehicleCount = 0, DisplayOrder = 11, IsVisible = true, Route = "/buscar?marca=audi" },
    new BrandConfig { Id = Guid.NewGuid(), BrandKey = "volkswagen",    DisplayName = "Volkswagen",    LogoInitials = "VO", VehicleCount = 0, DisplayOrder = 12, IsVisible = true, Route = "/buscar?marca=volkswagen" }
);
```

### CategoryImageConfig seeding (6 categorías — DATOS EXACTOS del diseño):

```csharp
modelBuilder.Entity<CategoryImageConfig>().HasData(
    new CategoryImageConfig { Id = Guid.NewGuid(), CategoryKey = "suv",       DisplayName = "SUV",       Description = "Versatilidad y espacio para toda la familia",     ImageUrl = "",  Gradient = "from-blue-600 to-blue-800",   VehicleCount = 0, IsTrending = true,  DisplayOrder = 1, IsVisible = true, Route = "/buscar?tipo=suv" },
    new CategoryImageConfig { Id = Guid.NewGuid(), CategoryKey = "sedan",     DisplayName = "Sedán",     Description = "Elegancia y eficiencia para el día a día",        ImageUrl = "",  Gradient = "from-primary to-primary/90",  VehicleCount = 0, IsTrending = false, DisplayOrder = 2, IsVisible = true, Route = "/buscar?tipo=sedan" },
    new CategoryImageConfig { Id = Guid.NewGuid(), CategoryKey = "camioneta", DisplayName = "Camioneta", Description = "Potencia y capacidad de carga",                   ImageUrl = "",  Gradient = "from-amber-600 to-amber-800", VehicleCount = 0, IsTrending = false, DisplayOrder = 3, IsVisible = true, Route = "/buscar?tipo=camioneta" },
    new CategoryImageConfig { Id = Guid.NewGuid(), CategoryKey = "deportivo", DisplayName = "Deportivo", Description = "Rendimiento y adrenalina pura",                  ImageUrl = "",  Gradient = "from-red-600 to-red-800",     VehicleCount = 0, IsTrending = false, DisplayOrder = 4, IsVisible = true, Route = "/buscar?tipo=deportivo" },
    new CategoryImageConfig { Id = Guid.NewGuid(), CategoryKey = "electrico", DisplayName = "Eléctrico", Description = "El futuro de la movilidad sostenible",           ImageUrl = "",  Gradient = "from-green-600 to-green-800", VehicleCount = 0, IsTrending = true,  DisplayOrder = 5, IsVisible = true, Route = "/buscar?tipo=electrico" },
    new CategoryImageConfig { Id = Guid.NewGuid(), CategoryKey = "hibrido",   DisplayName = "Híbrido",   Description = "Lo mejor de dos mundos",                         ImageUrl = "",  Gradient = "from-teal-600 to-teal-800",   VehicleCount = 0, IsTrending = false, DisplayOrder = 6, IsVisible = true, Route = "/buscar?tipo=hibrido" }
);
```

---

## 📧 PARTE 14: EMAIL TEMPLATE — Reporte Diario de Publicidad

### Archivo: `backend/NotificationService/NotificationService.Infrastructure/Templates/advertising-daily-report.html`

```html
<!-- Template HTML para el reporte diario de publicidad -->
<!-- Variables disponibles (Liquid/Handlebars): -->
<!-- {{ ownerName }} {{ period }} {{ totalViews }} {{ totalClicks }} {{ averageCtr }} -->
<!-- {{ campaigns }} (array de campañas) -->
<!-- {{ portalUrl }} -->

<!-- Estructura del email: -->
<!-- 1. Header OKLA con logo -->
<!-- 2. "¡Hola {ownerName}! Tu reporte de publicidad del {date}" -->
<!-- 3. 3 métricas en boxes: Vistas | Clicks | CTR% -->
<!-- 4. Tabla de campañas con: Foto del vehículo | Nombre | Vistas | Clicks | Presupuesto restante -->
<!-- 5. CTA: "Ver reporte completo en OKLA" → {portalUrl}/mis-vehiculos/publicidad -->
<!-- 6. Footer estándar OKLA -->
```

---

## 🚀 PARTE 15: KUBERNETES DEPLOYMENT

### Agregar en `k8s/deployments.yaml` (DESPUÉS del último deployment existente):

```yaml
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: advertisingservice
  namespace: okla
spec:
  replicas: 1
  selector:
    matchLabels:
      app: advertisingservice
  template:
    metadata:
      labels:
        app: advertisingservice
    spec:
      containers:
        - name: advertisingservice
          image: ghcr.io/gregorymorenoiem/advertisingservice:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 8080
          env:
            - name: ASPNETCORE_URLS
              value: "http://+:8080"
            - name: ASPNETCORE_ENVIRONMENT
              value: "Production"
          envFrom:
            - secretRef:
                name: okla-secrets
          readinessProbe:
            httpGet:
              path: /health/ready
              port: 8080
            initialDelaySeconds: 20
            periodSeconds: 10
            failureThreshold: 3
          livenessProbe:
            httpGet:
              path: /health/live
              port: 8080
            initialDelaySeconds: 40
            periodSeconds: 30
          resources:
            requests:
              memory: "256Mi"
              cpu: "100m"
            limits:
              memory: "512Mi"
              cpu: "500m"
      imagePullSecrets:
        - name: registry-credentials
```

### Agregar en `k8s/services.yaml`:

```yaml
---
apiVersion: v1
kind: Service
metadata:
  name: advertisingservice
  namespace: okla
spec:
  selector:
    app: advertisingservice
  ports:
    - protocol: TCP
      port: 8080
      targetPort: 8080
```

### Agregar en `.github/workflows/smart-cicd.yml`:

En la variable `SERVICES`, añadir `advertisingservice` a la lista (mantener el orden alfabético).

---

## 🧪 PARTE 16: TESTS OBLIGATORIOS

### Backend (xUnit + FluentAssertions + Moq):

```csharp
// AdvertisingService.Tests/Domain/AdCampaignTests.cs
AdCampaign_Activate_ShouldSetStatusToActive()
AdCampaign_RecordView_WhenPerViewAndNotDepleted_ShouldIncrementViewsConsumed()
AdCampaign_RecordView_WhenPerViewAndDepleted_ShouldSetStatusToCompleted()
AdCampaign_RecordClick_ShouldIncrementClicks()
AdCampaign_GetCtr_WhenZeroViews_ShouldReturnZero()
AdCampaign_IsExpired_WhenEndDatePast_ShouldReturnTrue()

// AdvertisingService.Tests/Application/RotationEngineTests.cs
RotationEngine_WeightedRandom_WhenNoCampaigns_ShouldReturnFallbackVehicles()
RotationEngine_WeightedRandom_ShouldSelectHigherScoreCampaigns_MoreFrequently()
RotationEngine_RoundRobin_ShouldRotateAllCampaigns_BeforeRepeat()
RotationEngine_CTROptimized_ShouldOrderByCTR_Descending()
RotationEngine_BudgetPriority_ShouldOrderByRemainingBudget_Descending()
RotationEngine_ShouldExclude_ExpiredCampaigns()
RotationEngine_ShouldExclude_DepletedPerViewCampaigns()

// AdvertisingService.Tests/Application/QualityScoreTests.cs
QualityScore_MaximumPhotos_ShouldScore0Point30()
QualityScore_AllFieldsComplete_ShouldScore1Point00()
QualityScore_MinimalFields_ShouldScoreAbove0()

// AdvertisingService.Tests/Infrastructure/DI_StartupTest.cs
Application_DI_Container_Resolves_All_Services()  // ← OBLIGATORIO
```

### Frontend (Vitest + Testing Library):

```typescript
// src/components/homepage/featured-ad-card.test.tsx
FeaturedAdCard_ShowsDestacadoBadge_WhenIsPaidAdIsTrue();
FeaturedAdCard_HidesBadge_WhenIsPaidAdIsFalse();
FeaturedAdCard_TracksImpression_OnMount_WhenCampaignIdExists();
FeaturedAdCard_DoesNotTrackImpression_Twice_SameCampaignSameSession();
FeaturedAdCard_TracksClick_BeforeNavigation();

// src/app/(main)/dashboard/publicidad/algoritmo/page.test.tsx
RotationConfigForm_ShowsWeightSliders_WhenWeightedRandomSelected();
RotationConfigForm_HidesWeightSliders_WhenRoundRobinSelected();
RotationConfigForm_ShowsError_WhenWeightsDoNotSumTo100();
RotationConfigForm_Submits_WhenValid();
```

---

## ✅ CRITERIOS DE ACEPTACIÓN

1. [ ] El homepage muestra la sección "⭐ Destacados" con vehículos reales de campañas activas (o fallback si no hay campañas)
2. [ ] El carrusel de marcas carga los datos dinámicos de `AdvertisingService` manteniendo el scroll infinito
3. [ ] Las 6 categorías muestran las imágenes/gradientes configuradas desde el panel admin
4. [ ] Un vendedor puede ir a `/mis-vehiculos/{id}/boost/` y completar el flujo de pago para destacar su vehículo
5. [ ] La campaña se activa automáticamente al recibir el evento `billing.payment.completed` de RabbitMQ
6. [ ] Las impresiones se registran: una por campaña por sesión (deduplicado en Redis)
7. [ ] Los clicks se registran y el budget se actualiza si `pricingModel = PerView`
8. [ ] La rotación se refresca automáticamente según el intervalo configurado en BD
9. [ ] Un admin puede acceder a `/dashboard/publicidad/algoritmo` y cambiar el tipo de algoritmo y pesos
10. [ ] Al guardar la configuración del algoritmo, los pesos se validan (suma = 100%) antes de enviar
11. [ ] Un admin puede subir una nueva imagen para cada categoría desde `/dashboard/contenido/categorias`
12. [ ] Un admin puede subir un logo para cada marca desde `/dashboard/contenido/marcas`
13. [ ] El reporte diario de publicidad llega por email a las 8:00 AM RD a todos los owners con campañas
14. [ ] El dealer puede ver el dashboard de publicidad en `/dealer/publicidad` con métricas reales
15. [ ] El DI container de `AdvertisingService` resuelve todos los servicios al iniciar (test `Application_DI_Container_Resolves_All_Services` pasa)
16. [ ] Los health checks `/health`, `/health/ready`, `/health/live` responden sin bloquear el pod
17. [ ] El servicio está en el CI/CD (`smart-cicd.yml`) y se construye/despliega con el workflow existente
18. [ ] Todos los endpoints públicos de tracking (`/api/advertising/tracking/*`) tienen rate limiting activo
19. [ ] Si `AdvertisingService` está caído, el homepage degrada elegantemente usando los datos hardcodeados como fallback final

---

## 📋 ORDEN DE IMPLEMENTACIÓN RECOMENDADO

Seguir este orden para minimizar bloqueos entre pasos:

1. **Backend primero:**
   - Crear estructura del proyecto `AdvertisingService` con Clean Architecture
   - Implementar entidades de dominio y enums
   - Crear `AdvertisingDbContext` con seeding de `RotationConfig`, `BrandConfig` y `CategoryImageConfig`
   - Implementar repositorios
   - Implementar `AdRotationEngine` (los 4 algoritmos)
   - Implementar handlers: `CreateCampaignCommandHandler`, `GetHomepageRotationQueryHandler`, `TrackImpressionCommandHandler`, `TrackClickCommandHandler`
   - Implementar `RotationRefreshJob` y `DailyAdReportJob`
   - Implementar `BillingPaymentCompletedConsumer`
   - Implementar todos los controllers
   - Completar `Program.cs` con el orden canónico exacto
   - Agregar rutas al Gateway
   - Escribir tests del DI container primero, luego tests de negocio

2. **Frontend después:**
   - Crear `src/types/advertising.ts`
   - Crear `src/services/advertising.ts`
   - Crear `src/hooks/use-ad-session.ts`
   - Modificar `brand-slider.tsx` para aceptar datos dinámicos
   - Modificar `category-cards.tsx` para aceptar datos dinámicos
   - Crear `featured-ad-card.tsx` con tracking
   - Modificar `homepage-client.tsx` para usar los nuevos queries
   - Modificar `page.tsx` (server component) para prefetch
   - Actualizar `/mis-vehiculos/[id]/boost/page.tsx` para conectar al backend real
   - Crear `/mis-vehiculos/publicidad/page.tsx`
   - Crear `/dealer/publicidad/page.tsx`
   - Actualizar `/dealer/inventario/page.tsx`
   - Crear `/dashboard/publicidad/page.tsx`
   - Crear `/dashboard/publicidad/algoritmo/page.tsx`
   - Crear `/dashboard/contenido/categorias/page.tsx`
   - Crear `/dashboard/contenido/marcas/page.tsx`
   - Actualizar navegación del dashboard admin y portal dealer

3. **Deployment al final:**
   - Agregar `advertisingservice` al `k8s/deployments.yaml` y `k8s/services.yaml`
   - Agregar al workflow `smart-cicd.yml`
   - Crear base de datos `advertising_db` en el PostgreSQL del cluster
   - Aplicar migrations
   - Crear K8s secret si necesita variables adicionales

---

_Prompt creado: Febrero 20, 2026_  
_Para OKLA — Plataforma de vehículos de República Dominicana_  
_Stack: .NET 8 + Next.js 16 + PostgreSQL + Redis + RabbitMQ + Kubernetes (DOKS)_
