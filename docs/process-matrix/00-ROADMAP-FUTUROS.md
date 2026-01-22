# 📅 Roadmap de Procesos Futuros - OKLA

> **Documento:** Procesos Media y Baja Prioridad para Implementación Futura  
> **Versión:** 1.0  
> **Fecha:** Enero 21, 2026  
> **Estado:** Backlog Priorizado  
> **Próxima Revisión:** Abril 2026

---

## 📋 Resumen

Este documento contiene la especificación de **11 procesos** identificados en el análisis competitivo que se implementarán después de los procesos de alta prioridad.

---

## 🟡 PRIORIDAD MEDIA (Q2-Q3 2026)

### PRICE-002: Historial de Precios del Listing

```yaml
Código: PRICE-002
Origen: CarGurus, Cars.com
Categoría: 20-PRICING-INTELLIGENCE
Esfuerzo: 2 SP
Dependencias: VehiclesSaleService, PricingIntelligenceService

Descripción: |
  Mostrar gráfico con el historial de cambios de precio de un listing.
  "Este vehículo bajó de RD$1,500,000 a RD$1,350,000 (-10%)"

Funcionalidades:
  - Gráfico de línea con precios históricos
  - Indicador de tendencia (subiendo/bajando/estable)
  - Días desde último cambio de precio
  - Porcentaje de cambio total

Entidades:
  - VehiclePriceHistory:
      - Id: Guid
      - VehicleId: Guid
      - Price: decimal
      - PreviousPrice: decimal?
      - ChangePercentage: decimal
      - ChangedAt: DateTime
      - Source: PriceChangeSource (Manual, Automatic, Promotion)

Endpoints:
  - GET /api/vehicles/{id}/price-history
  - GET /api/pricing/trends/{vehicleId}

UI:
  - Componente PriceHistoryChart en VehicleDetailPage
  - Badge "Precio reducido X%" en cards

Métricas:
  - price_change_events_total
  - price_reduction_average_percent
  - days_since_price_change_avg
```

---

### ANALYTICS-001: Tendencias de Mercado

```yaml
Código: ANALYTICS-001
Origen: CarGurus
Categoría: 09-REPORTES-ANALYTICS
Esfuerzo: 5 SP
Dependencias: PricingIntelligenceService, VehiclesSaleService

Descripción: |
  Dashboard público mostrando tendencias de precios por marca/modelo.
  "Los Toyota Corolla 2020-2023 han bajado 5% en los últimos 3 meses"

Funcionalidades:
  - Tendencias por marca/modelo/año
  - Gráficos de demanda vs oferta
  - Predicción de precios (ML básico)
  - Comparación mes a mes
  - Top 10 modelos más buscados

Entidades:
  - MarketTrend:
      - Id: Guid
      - Make: string
      - Model: string
      - YearRange: string
      - AvgPrice: decimal
      - PriceChange30d: decimal
      - PriceChange90d: decimal
      - Demand: int (búsquedas)
      - Supply: int (listings activos)
      - CalculatedAt: DateTime

Endpoints:
  - GET /api/analytics/trends
  - GET /api/analytics/trends/{make}/{model}
  - GET /api/analytics/top-searched
  - GET /api/analytics/price-predictions

UI:
  - Página /tendencias-mercado
  - Widget de tendencias en homepage
  - Insights en VehicleDetailPage

Métricas:
  - market_trend_calculations_total
  - prediction_accuracy_percent
```

---

### VIRTUAL-001: Cita Virtual por Video

```yaml
Código: VIRTUAL-001
Origen: Cars.com, Carvana
Categoría: 05-AGENDAMIENTO
Esfuerzo: 3 SP
Dependencias: AppointmentService, NotificationService

Descripción: |
  Permitir agendar videollamada con el vendedor para ver el vehículo remotamente.
  Integración con WhatsApp Video, Zoom o Google Meet.

Funcionalidades:
  - Seleccionar tipo de cita: Presencial o Virtual
  - Generar link de videollamada automático
  - Recordatorios antes de la llamada
  - Grabación opcional (con consentimiento)
  - Feedback post-llamada

Entidades:
  - VirtualAppointment (extiende Appointment):
      - Platform: VideoPlatform (WhatsApp, Zoom, GoogleMeet)
      - MeetingUrl: string
      - MeetingId: string
      - RecordingEnabled: bool
      - RecordingUrl: string?
      - Duration: int (minutos)

Endpoints:
  - POST /api/appointments/virtual
  - GET /api/appointments/{id}/meeting-link
  - POST /api/appointments/{id}/start-recording

UI:
  - Toggle "Cita Virtual" en formulario de agendar
  - Botón "Unirse a videollamada" en dashboard
  - Indicador de cita virtual en calendarios

Integraciones:
  - WhatsApp Business API
  - Zoom API (opcional)
  - Google Calendar API

Métricas:
  - virtual_appointments_scheduled_total
  - virtual_appointments_completed_total
  - virtual_to_inperson_conversion_rate
```

---

### REC-001: Recomendaciones Mejoradas con ML

```yaml
Código: REC-001
Origen: Todos los competidores
Categoría: 04-BUSQUEDA-FILTROS
Esfuerzo: 5 SP
Dependencias: RecommendationService, UserBehaviorService

Descripción: |
  Sistema de recomendaciones basado en comportamiento del usuario.
  Collaborative filtering + Content-based filtering.

Funcionalidades:
  - "Basado en tu historial de búsqueda"
  - "Usuarios como tú también vieron"
  - "Similar a vehículos que guardaste"
  - Recomendaciones por email personalizadas
  - A/B testing de algoritmos

Entidades:
  - UserPreferences:
      - UserId: Guid
      - PreferredMakes: List<string>
      - PreferredBodyTypes: List<string>
      - PriceRange: (min, max)
      - YearRange: (min, max)
      - FeatureWeights: Dictionary<string, float>
      - CalculatedAt: DateTime

  - Recommendation:
      - Id: Guid
      - UserId: Guid
      - VehicleId: Guid
      - Score: float
      - Reason: RecommendationReason
      - Clicked: bool
      - ClickedAt: DateTime?

Endpoints:
  - GET /api/recommendations/for-you
  - GET /api/recommendations/similar/{vehicleId}
  - GET /api/recommendations/because-you-searched
  - POST /api/recommendations/{id}/click

Algoritmos:
  - Collaborative Filtering (usuarios similares)
  - Content-Based (características del vehículo)
  - Hybrid (combinación ponderada)
  - Popular (fallback para usuarios nuevos)

Métricas:
  - recommendation_click_through_rate
  - recommendation_to_contact_rate
  - algorithm_performance_by_type
```

---

### COMPARE-002: Comparación con Total Cost of Ownership

```yaml
Código: COMPARE-002
Origen: CarGurus, Edmunds
Categoría: Extender ComparisonService existente
Esfuerzo: 3 SP
Dependencias: ComparisonService, PricingIntelligenceService

Descripción: |
  Extender el comparador para incluir costo total de propiedad:
  - Consumo de combustible estimado
  - Seguro estimado
  - Mantenimiento estimado
  - Depreciación proyectada

Funcionalidades:
  - TCO a 3 y 5 años
  - Comparación de costos mensuales
  - Gráfico de depreciación
  - Costo por kilómetro

Entidades:
  - VehicleTCO:
      - VehicleId: Guid
      - FuelCostMonthly: decimal
      - InsuranceEstimate: decimal
      - MaintenanceYearly: decimal
      - DepreciationYear1: decimal
      - DepreciationYear3: decimal
      - DepreciationYear5: decimal
      - TotalCost3Years: decimal
      - TotalCost5Years: decimal

Endpoints:
  - GET /api/comparisons/{id}/tco
  - GET /api/vehicles/{id}/tco-estimate

UI:
  - Tab "Costo de Propiedad" en comparador
  - Gráfico de barras comparativo
  - Tabla detallada de costos

Datos Requeridos:
  - Precios de combustible RD (API o manual)
  - Tablas de depreciación por marca/modelo
  - Estimados de seguro por categoría

Métricas:
  - tco_calculations_total
  - tco_comparison_views
```

---

### DEALER-001: Perfil de Dealer Mejorado

```yaml
Código: DEALER-001
Origen: AutoTrader, Cars.com
Categoría: 02-USUARIOS-DEALERS
Esfuerzo: 3 SP
Dependencias: DealerManagementService, VehiclesSaleService

Descripción: |
  Página de perfil de dealer con todo su inventario, reviews, 
  información de contacto y ubicación en mapa.

Funcionalidades:
  - Header con logo, nombre, verificación
  - Galería de fotos del local
  - Mapa con ubicación(es)
  - Inventario completo filtrable
  - Reviews y rating promedio
  - Horarios de atención
  - Botones de contacto (llamar, WhatsApp, email)
  - Estadísticas públicas (años en OKLA, vehículos vendidos)

Endpoints:
  - GET /api/dealers/{slug}/profile
  - GET /api/dealers/{id}/inventory
  - GET /api/dealers/{id}/reviews
  - GET /api/dealers/{id}/stats

UI:
  - Página /dealer/{slug}
  - Grid de vehículos con filtros
  - Sidebar con info del dealer
  - CTA flotante de contacto

Métricas:
  - dealer_profile_views_total
  - dealer_profile_to_contact_rate
  - dealer_profile_to_listing_click_rate
```

---

### MEDIA-001: Validación de Fotos con AI

```yaml
Código: MEDIA-001
Origen: Kavak
Categoría: Extender MediaService
Esfuerzo: 5 SP
Dependencias: MediaService, Azure Cognitive Services / AWS Rekognition

Descripción: |
  AI que valida las fotos subidas:
  - ¿Es una foto real o stock?
  - ¿Muestra un vehículo?
  - ¿Calidad aceptable (resolución, iluminación)?
  - ¿Ángulos requeridos presentes?

Funcionalidades:
  - Validación automática al subir
  - Score de calidad por foto
  - Sugerencias de mejora
  - Detección de fotos duplicadas
  - Detección de marcas de agua

Entidades:
  - MediaValidation:
      - MediaId: Guid
      - IsRealPhoto: bool
      - ContainsVehicle: bool
      - QualityScore: float (0-1)
      - ResolutionOk: bool
      - LightingOk: bool
      - Issues: List<ValidationIssue>
      - ValidatedAt: DateTime

Endpoints:
  - POST /api/media/validate
  - GET /api/media/{id}/validation-result

Integraciones:
  - Azure Computer Vision API
  - O: AWS Rekognition
  - O: Google Cloud Vision

Métricas:
  - photos_validated_total
  - photos_rejected_total{reason}
  - average_quality_score
```

---

### TRUST-007: Niveles de Verificación de Vendedor

```yaml
Código: TRUST-007
Origen: Kavak, Seminuevos
Categoría: 15-CONFIANZA-SEGURIDAD
Esfuerzo: 3 SP
Dependencias: TrustService, UserService

Descripción: |
  Sistema de niveles de verificación para vendedores individuales:
  - Básico: Email verificado
  - Verificado: Cédula verificada
  - Confiable: Historial de ventas positivo
  - Premium: Verificación completa + reviews excelentes

Funcionalidades:
  - Badges por nivel en listings
  - Criterios claros para cada nivel
  - Upgrade automático al cumplir criterios
  - Beneficios por nivel (más visibilidad)

Entidades:
  - SellerVerificationLevel:
      - UserId: Guid
      - Level: VerificationLevel (Basic, Verified, Trusted, Premium)
      - EmailVerified: bool
      - PhoneVerified: bool
      - IdentityVerified: bool
      - SalesCount: int
      - AverageRating: decimal
      - ReviewCount: int
      - PromotedAt: DateTime?

Endpoints:
  - GET /api/trust/seller/{userId}/level
  - POST /api/trust/seller/verify-identity
  - GET /api/trust/seller/upgrade-requirements

UI:
  - Badges en cards de vehículos
  - Página de "Cómo verificar mi cuenta"
  - Dashboard de progreso de verificación

Métricas:
  - sellers_by_verification_level
  - verification_upgrade_rate
  - verified_seller_conversion_premium
```

---

### PERF-001: Dashboard de Performance para Vendedores

```yaml
Código: PERF-001
Origen: eBay Motors, Amazon Seller Central
Categoría: 02-USUARIOS-DEALERS (o nuevo)
Esfuerzo: 3 SP
Dependencias: DealerAnalyticsService, UserService

Descripción: |
  Métricas de rendimiento visibles para vendedores:
  - Tiempo promedio de respuesta
  - Tasa de respuesta
  - Tasa de cierre (ventas/contactos)
  - Comparación vs promedio del mercado

Funcionalidades:
  - Dashboard con KPIs principales
  - Gráficos de tendencia
  - Alertas si métricas bajan
  - Comparación con otros sellers (anónimo)
  - Tips para mejorar

Entidades:
  - SellerPerformance:
      - SellerId: Guid
      - Period: DateTime (mes)
      - AvgResponseTimeMinutes: int
      - ResponseRate: decimal
      - ContactCount: int
      - SalesCount: int
      - ConversionRate: decimal
      - ReviewRating: decimal
      - MarketComparison: PerformanceRank (Top10%, Top25%, Average, BelowAvg)

Endpoints:
  - GET /api/performance/my-stats
  - GET /api/performance/my-stats/history
  - GET /api/performance/market-average

UI:
  - Página /mi-rendimiento
  - Widget en dashboard principal
  - Notificaciones de alertas

Métricas:
  - seller_response_time_avg
  - seller_conversion_rate_avg
  - performance_dashboard_views
```

---

## 🟢 PRIORIDAD BAJA (Q4 2026+)

### CONTENT-001: Guías de Compra (CMS)

```yaml
Código: CONTENT-001
Origen: Cars.com, Edmunds
Categoría: Nueva - CONTENIDO-EDUCATIVO
Esfuerzo: 5 SP
Dependencias: Nuevo CMS o integración con headless CMS

Descripción: |
  Blog/Centro de contenido con guías educativas:
  - "Guía para comprar tu primer auto"
  - "Mejores SUVs 2026"
  - "¿Nuevo o usado? Pros y contras"
  - "Cómo negociar el precio"

Funcionalidades:
  - CMS para crear/editar artículos
  - Categorías y tags
  - Relacionar artículos con listings
  - SEO optimizado
  - Compartir en redes

Implementación:
  - Opción A: CMS propio (Strapi, Ghost)
  - Opción B: Headless CMS (Contentful, Sanity)
  - Opción C: WordPress headless

Beneficios:
  - SEO: Más páginas indexables
  - Autoridad: Posicionamiento como experto
  - Engagement: Más tiempo en sitio
  - Leads: Captar usuarios en fase de investigación

Métricas:
  - article_views_total
  - article_to_listing_click_rate
  - organic_traffic_from_content
```

---

### TRUST-008: Garantía de Satisfacción

```yaml
Código: TRUST-008
Origen: Carvana, Kavak
Categoría: 15-CONFIANZA-SEGURIDAD
Esfuerzo: 2 SP
Dependencias: TrustService, BillingService

Descripción: |
  Permitir a dealers ofrecer garantía de satisfacción:
  "7 días para devolverlo si no te gusta"
  Esto es OPCIONAL para dealers que quieran diferenciarse.

Funcionalidades:
  - Badge "Garantía de Satisfacción" en listing
  - Filtro de búsqueda
  - Términos configurables por dealer
  - Proceso de devolución documentado

Entidades:
  - SatisfactionGuarantee:
      - DealerId: Guid
      - Enabled: bool
      - DaysToReturn: int (7, 14, 30)
      - MaxKilometers: int
      - RefundType: RefundType (Full, Partial, StoreCredit)
      - Terms: string
      - CreatedAt: DateTime

Endpoints:
  - GET /api/dealers/{id}/satisfaction-guarantee
  - PUT /api/dealers/{id}/satisfaction-guarantee

UI:
  - Badge prominente en listings
  - Tooltip con términos
  - Filtro en búsqueda

Métricas:
  - dealers_with_guarantee_total
  - guarantee_filter_usage
  - guarantee_redemption_rate
```

---

## 📊 Resumen de Esfuerzo

### Por Prioridad

| Prioridad | Procesos | Story Points | Timeline   |
| --------- | -------- | ------------ | ---------- |
| 🟡 Media  | 9        | 32 SP        | Q2-Q3 2026 |
| 🟢 Baja   | 2        | 7 SP         | Q4 2026+   |
| **Total** | **11**   | **39 SP**    | ~6 meses   |

### Por Categoría

| Categoría            | Procesos | SP  |
| -------------------- | -------- | --- |
| Pricing Intelligence | 2        | 7   |
| Analytics            | 1        | 5   |
| Agendamiento         | 1        | 3   |
| Búsqueda/Filtros     | 1        | 5   |
| Comparación          | 1        | 3   |
| Dealers              | 1        | 3   |
| Media                | 1        | 5   |
| Confianza            | 2        | 5   |
| Performance          | 1        | 3   |
| Contenido            | 1        | 5   |

---

## 🔄 Proceso de Priorización

Estos procesos serán re-evaluados trimestralmente basándose en:

1. **Feedback de usuarios** - Solicitudes más frecuentes
2. **Métricas de competencia** - Features que SuperCarros implemente
3. **Recursos disponibles** - Capacidad del equipo
4. **ROI proyectado** - Impacto en conversión/revenue

---

## 📎 Referencias

- [00-ANALISIS-COMPETITIVO.md](00-ANALISIS-COMPETITIVO.md) - Análisis completo
- [Documentación de procesos alta prioridad](.) - Ver carpetas específicas

---

_Documento mantenido por el Equipo de Producto OKLA_  
_Última actualización: Enero 21, 2026_
