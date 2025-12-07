# Plan de Sprints - Featured Listings & Monetización

## 📅 Roadmap General
**Duración Total:** 8 sprints (16 semanas / ~4 meses)
**Objetivo:** Sistema completo de monetización mediante featured listings operacional

---

## 🏃 Sprint 1: Foundation & Data Model (Semana 1-2)

### Objetivos
Establecer la base de datos y tipos necesarios para soportar featured listings.

### Tareas

#### Backend
- [ ] **Actualizar Schema de Listing**
  ```typescript
  - Agregar campos: tier, featuredUntil, featuredPosition, featuredPages
  - Agregar campos: qualityScore, engagementScore, conversionRate
  - Migration script para DB existente
  ```
  **Estimado:** 1 día

- [ ] **Actualizar Schema de Dealer**
  ```typescript
  - Agregar campos: subscriptionType, subscriptionStart/End, maxFeaturedListings
  - Agregar campos: monthlyBilling, autoRenew
  - Relación con Payment history
  ```
  **Estimado:** 1 día

- [ ] **Crear Tabla: FeaturedPositionAssignment**
  ```typescript
  - Tracking de posiciones compradas
  - Fechas start/end
  - Precio pagado, status
  ```
  **Estimado:** 0.5 día

- [ ] **Seed Data con Listings Featured**
  ```typescript
  - Crear 10-15 listings premium de ejemplo
  - 3-4 dealers con diferentes tiers
  - Datos realistas para testing
  ```
  **Estimado:** 0.5 día

#### Frontend (Types)
- [ ] **Crear Types/Interfaces**
  ```typescript
  - ListingTier type
  - DealerSubscription interface
  - FeaturedPosition interface
  - RankingFactors interface
  ```
  **Estimado:** 0.5 día

### Entregables
✅ DB actualizada con nuevos schemas
✅ Seed data con featured listings
✅ Types TypeScript definidos
✅ Migration scripts documentados

**Story Points:** 8
**Riesgo:** Bajo

---

## 🏃 Sprint 2: Ranking Algorithm & Core Logic (Semana 3-4)

### Objetivos
Implementar el algoritmo de ranking que determina el orden de los listings.

### Tareas

#### Backend
- [ ] **Implementar Ranking Algorithm**
  ```typescript
  - Función calculateListingScore()
  - Factores: premium boost, dealer tier, quality, engagement
  - Unit tests completos
  ```
  **Estimado:** 2 días

- [ ] **API: Get Ranked Listings**
  ```typescript
  GET /api/listings?sort=rank&category=vehicles&page=1
  - Aplicar ranking algorithm
  - Pagination
  - Filters compatibility
  ```
  **Estimado:** 1 día

- [ ] **Fairness Rules Implementation**
  ```typescript
  - Max 50% premium en first page
  - Rotation logic (24h cycle)
  - Relevance filter (search match required)
  - Quality threshold enforcement
  ```
  **Estimado:** 2 días

- [ ] **Background Job: Position Rotation**
  ```typescript
  - Cron job diario para rotar posiciones premium
  - Update featuredPosition para dealers en mismo slot
  - Logging de rotaciones
  ```
  **Estimado:** 1 día

#### Testing
- [ ] **Unit Tests para Ranking**
  - Test diferentes combinaciones de scores
  - Test fairness rules
  - Test edge cases
  **Estimado:** 1 día

### Entregables
✅ Algoritmo de ranking funcional y testeado
✅ API que retorna listings ordenados correctamente
✅ Fairness rules aplicándose
✅ Sistema de rotación automático

**Story Points:** 13
**Riesgo:** Medio (complejidad del algoritmo)

---

## 🏃 Sprint 3: UI Components - Featured Listings (Semana 5-6)

### Objetivos
Crear los componentes visuales para mostrar featured listings con elegancia.

### Tareas

#### Frontend Components
- [ ] **FeaturedListingCard Component**
  ```tsx
  - Badge "Destacado" con diseño sutil
  - Border gradient azul-emerald
  - Glow effect en hover
  - Dealer badge "Certificado"
  - Responsive design
  ```
  **Estimado:** 2 días

- [ ] **Badge Components**
  ```tsx
  - DestacadoBadge (⭐ Destacado)
  - PremiumBadge (💎 Premium)
  - CertificadoBadge (✓ Certificado)
  - TopDealerBadge (🏆 Top Dealer)
  ```
  **Estimado:** 1 día

- [ ] **HeroCarousel Component**
  ```tsx
  - Hero-sized cards para top 3
  - Auto-rotate cada 5 segundos
  - Indicators de posición
  - Touch/swipe support mobile
  ```
  **Estimado:** 2 días

- [ ] **PremiumListingGrid Component**
  ```tsx
  - Grid que mezcla premium + organic
  - Respeta fairness rules (50% max)
  - Skeleton loading states
  - Infinite scroll support
  ```
  **Estimado:** 1.5 días

#### Storybook
- [ ] **Stories para cada componente**
  - Diferentes estados (featured, premium, basic)
  - Diferentes devices
  - Dark/light mode
  **Estimado:** 0.5 día

### Entregables
✅ Componentes visuales completos y testeados
✅ Storybook con todos los estados
✅ Responsive y mobile-friendly
✅ Matches diseño Okla (elegante, sutil)

**Story Points:** 13
**Riesgo:** Bajo

---

## 🏃 Sprint 4: Integration - HomePage & BrowsePage (Semana 7-8)

### Objetivos
Integrar los featured listings en las páginas principales del marketplace.

### Tareas

#### HomePage Integration
- [ ] **Hero Section con Featured Carousel**
  ```tsx
  - Fetch top 3 featured listings
  - Integrar HeroCarousel
  - Analytics tracking (impressions)
  ```
  **Estimado:** 1 día

- [ ] **Sección "Destacados de la Semana"**
  ```tsx
  - Primeras 4 posiciones = featured
  - Resto = organic high-quality
  - Badge visual differentiation
  ```
  **Estimado:** 1 día

- [ ] **Secciones por Categoría**
  ```tsx
  - Vehículos: 3 featured + organic
  - Rentas: 2 featured + organic
  - Propiedades: 3 featured + organic
  - Hospedaje: 2 featured + organic
  ```
  **Estimado:** 1.5 días

#### BrowsePage Integration
- [ ] **Search Results con Featured**
  ```tsx
  - Posiciones 1-2: premium
  - Cada 4 items: 1 premium intercalado
  - Mantener relevancia de búsqueda
  ```
  **Estimado:** 1.5 días

- [ ] **Filter Interaction**
  ```tsx
  - Featured listings respetan filtros
  - Re-ranking al aplicar filters
  - Loading states
  ```
  **Estimado:** 1 día

#### Analytics
- [ ] **Impression Tracking**
  ```tsx
  - IntersectionObserver para viewability
  - Track featured vs organic impressions
  - Send to analytics endpoint
  ```
  **Estimado:** 1 día

### Entregables
✅ HomePage con featured listings integrados
✅ BrowsePage con positioning premium
✅ Tracking de impressions funcionando
✅ UX fluida y natural

**Story Points:** 13
**Riesgo:** Medio (integración con código existente)

---

## 🏃 Sprint 5: DetailPage & Cross-Selling (Semana 9-10)

### Objetivos
Implementar featured listings en página de detalle para cross-selling.

### Tareas

#### DetailPage Integration
- [ ] **"Similares" Section con Featured**
  ```tsx
  - Primeras 2 posiciones = featured
  - Resto = similares orgánicos
  - Matching inteligente (same category, price range)
  ```
  **Estimado:** 1.5 días

- [ ] **"Del Mismo Dealer" Section**
  ```tsx
  - Top 3 listings del dealer actual
  - Featured primero si tiene
  - Link a dealer store
  ```
  **Estimado:** 1 día

- [ ] **Dealer CTA Banner**
  ```tsx
  - Card nativo "Ver más de [Dealer]"
  - Logo, rating, listing count
  - Solo para premium/enterprise dealers
  ```
  **Estimado:** 1 día

#### Click Tracking
- [ ] **Conversion Tracking**
  ```tsx
  - Track clicks featured → detail
  - Track detail → contact/lead
  - Calculate conversion rate por listing
  ```
  **Estimado:** 1.5 días

#### API Improvements
- [ ] **Related Listings API**
  ```typescript
  GET /api/listings/:id/related?includeSponsored=true
  - Smart matching algorithm
  - Featured boost dentro de "related"
  ```
  **Estimado:** 1 día

### Entregables
✅ Cross-selling con featured listings
✅ Conversion tracking completo
✅ Dealer upselling natural
✅ Related listings inteligente

**Story Points:** 10
**Riesgo:** Bajo

---

## 🏃 Sprint 6: Dealer Dashboard & Self-Service (Semana 11-12)

### Objetivos
Dashboard para que dealers gestionen sus featured listings y vean analytics.

### Tareas

#### Dashboard - Overview
- [ ] **Dashboard Homepage**
  ```tsx
  - KPI cards: Views, Clicks, Leads, Revenue
  - Graph: Performance últimos 30 días
  - Quick actions: Feature listing, Upgrade plan
  ```
  **Estimado:** 2 días

- [ ] **Featured Listings Management**
  ```tsx
  - Lista de listings actuales
  - Status: active, expired, paused
  - Acciones: Renew, Pause, Analytics
  ```
  **Estimado:** 1.5 días

#### Self-Service Features
- [ ] **Feature a Listing Flow**
  ```tsx
  - Select listing from inventory
  - Choose tier (Featured, Premium)
  - Select duration (7, 14, 30 días)
  - Choose pages (Home, Browse, Detail)
  - Preview & Confirm
  ```
  **Estimado:** 2 días

- [ ] **Position Selector (Premium)**
  ```tsx
  - Visual map of available positions
  - HomePage: Hero, Destacados, Categories
  - Pricing shown per position
  - Calendar view for availability
  ```
  **Estimado:** 1.5 días

#### Analytics Dashboard
- [ ] **Listing Performance View**
  ```tsx
  - Individual listing analytics
  - Impressions, clicks, CTR
  - Leads generated
  - Heatmap de clicks
  - Compare: featured vs organic period
  ```
  **Estimado:** 2 días

### Entregables
✅ Dashboard funcional para dealers
✅ Self-service para comprar featured
✅ Analytics detallado por listing
✅ Position management UI

**Story Points:** 13
**Riesgo:** Medio (complejidad de UI)

---

## 🏃 Sprint 7: Payment System & Subscriptions (Semana 13-14)

### Objetivos
Sistema de pagos y suscripciones para monetización real.

### Tareas

#### Payment Integration
- [ ] **Stripe Integration**
  ```typescript
  - Setup Stripe account
  - Product catalog (Featured, Premium, Enterprise)
  - Webhook handlers
  ```
  **Estimado:** 2 días

- [ ] **Checkout Flow**
  ```tsx
  - Payment form con Stripe Elements
  - Confirmation page
  - Email receipt
  ```
  **Estimado:** 1.5 días

#### Subscription Management
- [ ] **Subscription Creation API**
  ```typescript
  POST /api/subscriptions
  - Create subscription in Stripe
  - Update dealer tier
  - Grant featured slots
  ```
  **Estimado:** 1 día

- [ ] **Auto-Renewal System**
  ```typescript
  - Cron job para renovaciones
  - Email 7 días antes de expirar
  - Auto-charge si autoRenew=true
  - Handle failed payments
  ```
  **Estimado:** 1.5 días

- [ ] **Cancellation & Refunds**
  ```typescript
  - Cancel subscription API
  - Prorated refund calculation
  - Graceful downgrade (featured → basic)
  ```
  **Estimado:** 1 día

#### Billing Dashboard
- [ ] **Billing History Page**
  ```tsx
  - Payment history table
  - Download invoices
  - Update payment method
  - Upgrade/downgrade plan
  ```
  **Estimado:** 1.5 días

### Entregables
✅ Stripe integration completa
✅ Sistema de suscripciones funcional
✅ Auto-renewal y billing management
✅ Checkout flow completo

**Story Points:** 13
**Riesgo:** Alto (pagos críticos, testing exhaustivo)

---

## 🏃 Sprint 8: Admin Tools & Optimization (Semana 15-16)

### Objetivos
Herramientas de administración y optimización final del sistema.

### Tareas

#### Admin Dashboard
- [ ] **Featured Listings Overview**
  ```tsx
  - Tabla de todas las featured positions
  - Status, dealer, expiration, revenue
  - Filtros y búsqueda
  ```
  **Estimado:** 1.5 días

- [ ] **Dealer Management**
  ```tsx
  - Lista de dealers con subscriptions
  - Acciones: Upgrade, Extend, Cancel
  - Performance metrics por dealer
  ```
  **Estimado:** 1 día

- [ ] **Position Management**
  ```tsx
  - Visual editor de positions
  - Set pricing por position
  - Enable/disable positions
  - Bulk operations
  ```
  **Estimado:** 1.5 días

#### Reporting & Analytics
- [ ] **Revenue Dashboard**
  ```tsx
  - MRR (Monthly Recurring Revenue)
  - Revenue by tier
  - Churn rate
  - Projections
  ```
  **Estimado:** 1.5 días

- [ ] **Performance Reports**
  ```tsx
  - Best performing positions
  - Avg CTR by position
  - Conversion rates
  - ROI analysis para dealers
  ```
  **Estimado:** 1 día

#### Optimization
- [ ] **A/B Testing Framework**
  ```typescript
  - Test different badge designs
  - Test position layouts
  - Track conversion impact
  ```
  **Estimado:** 1.5 días

- [ ] **Automated Upselling**
  ```typescript
  - Email campaigns para organic dealers
  - In-dashboard upsell prompts
  - Performance-based recommendations
  ```
  **Estimado:** 1 día

### Entregables
✅ Admin tools completos
✅ Revenue tracking y reporting
✅ A/B testing framework
✅ Sistema de upselling automático

**Story Points:** 13
**Riesgo:** Bajo

---

## 📊 Resumen de Sprints

| Sprint | Enfoque | Story Points | Riesgo | Duración |
|--------|---------|--------------|--------|----------|
| 1 | Foundation & Data Model | 8 | Bajo | 2 sem |
| 2 | Ranking Algorithm | 13 | Medio | 2 sem |
| 3 | UI Components | 13 | Bajo | 2 sem |
| 4 | HomePage & BrowsePage | 13 | Medio | 2 sem |
| 5 | DetailPage & Cross-Sell | 10 | Bajo | 2 sem |
| 6 | Dealer Dashboard | 13 | Medio | 2 sem |
| 7 | Payment System | 13 | Alto | 2 sem |
| 8 | Admin & Optimization | 13 | Bajo | 2 sem |
| **TOTAL** | | **96** | | **16 sem** |

---

## 🎯 Milestones Clave

### Milestone 1: MVP Funcional (Sprint 4)
✅ Featured listings visibles en HomePage y BrowsePage
✅ Componentes UI completos
✅ Ranking algorithm funcionando
✅ Analytics básico

**Lanzamiento Interno:** Semana 8

### Milestone 2: Self-Service Ready (Sprint 6)
✅ Dealers pueden gestionar featured listings
✅ Dashboard con analytics detallado
✅ Preview antes de comprar

**Beta con 5-10 Dealers:** Semana 12

### Milestone 3: Full Production (Sprint 7)
✅ Sistema de pagos operacional
✅ Suscripciones y auto-renewal
✅ Billing management completo

**Lanzamiento Público:** Semana 14

### Milestone 4: Optimización Continua (Sprint 8+)
✅ Admin tools para gestión
✅ A/B testing activo
✅ Automated upselling

**Operación a Escala:** Semana 16+

---

## 🚀 Pre-requisitos por Sprint

### Sprint 1
- ✅ Acceso a DB
- ✅ Environment setup
- ✅ Documentación de schema actual

### Sprint 2
- ✅ Sprint 1 completado
- ✅ Unit testing framework configurado

### Sprint 3
- ✅ Diseño UI aprobado
- ✅ Component library setup (Storybook)

### Sprint 4
- ✅ Sprint 2 y 3 completados
- ✅ API de listings funcionando

### Sprint 5
- ✅ Sprint 4 completado
- ✅ Analytics endpoint disponible

### Sprint 6
- ✅ Sprint 5 completado
- ✅ Authentication system para dealers

### Sprint 7
- ✅ Stripe account aprobado
- ✅ Legal terms & conditions finalizados
- ✅ Tax compliance setup

### Sprint 8
- ✅ Sistema en beta con usuarios reales
- ✅ Feedback de primeros dealers

---

## ⚠️ Riesgos y Mitigaciones

### Riesgo Alto: Sprint 7 (Payments)
**Problema:** Integración de pagos puede tener issues inesperados
**Mitigación:**
- Testing exhaustivo en sandbox
- Manejo de todos los edge cases (failed payment, refunds)
- Rollback plan si hay issues críticos
- Buffer de 1-2 días extras en estimación

### Riesgo Medio: Sprint 2 (Algorithm)
**Problema:** Fairness rules pueden ser complejas de implementar
**Mitigación:**
- Unit tests completos antes de integration
- Edge cases documentados
- Manual testing con diferentes scenarios

### Riesgo Medio: Sprint 4 (Integration)
**Problema:** Código existente puede tener conflicts
**Mitigación:**
- Code review detallado
- Branch de prueba antes de merge
- Rollback plan

---

## 📈 Métricas de Éxito

### Sprint 4 (MVP)
- ✅ 100% de featured listings se muestran correctamente
- ✅ 0 errores en ranking algorithm
- ✅ Page load time < 2 segundos con featured

### Sprint 6 (Self-Service)
- ✅ Dealers pueden completar purchase flow en < 5 minutos
- ✅ 90%+ dealer satisfaction con dashboard
- ✅ Analytics data accuracy 100%

### Sprint 7 (Payments)
- ✅ 100% success rate en pagos test
- ✅ 0 failed auto-renewals en primeros 30 días
- ✅ PCI compliance aprobado

### Sprint 8 (Production)
- ✅ 20+ dealers pagando por featured
- ✅ $10K+ MRR
- ✅ <5% churn rate mensual
- ✅ 3x+ CTR featured vs organic

---

## 🔄 Proceso de Sprint

### Daily Standup (15 min)
- ¿Qué hice ayer?
- ¿Qué haré hoy?
- ¿Hay blockers?

### Sprint Planning (2 horas)
- Review tareas del sprint
- Estimar story points
- Asignar responsables
- Definir DoD (Definition of Done)

### Sprint Review (1 hora)
- Demo de features completados
- Feedback de stakeholders
- Ajustar prioridades si necesario

### Sprint Retrospective (1 hora)
- ¿Qué salió bien?
- ¿Qué mejorar?
- Action items para próximo sprint

---

## 📝 Definition of Done

### Para cada tarea:
- [ ] Código implementado y funcional
- [ ] Unit tests escritos y pasando (>80% coverage)
- [ ] Code review aprobado por al menos 1 persona
- [ ] Documentación actualizada
- [ ] Sin errores de linting/TypeScript
- [ ] Testeado en Chrome, Safari, Firefox
- [ ] Testeado en mobile (iOS + Android)
- [ ] Performance: no aumenta load time en >500ms
- [ ] Accessibility: cumple WCAG 2.1 AA

### Para cada sprint:
- [ ] Todas las tareas completadas
- [ ] Demo funcionando en staging
- [ ] Stakeholder approval
- [ ] Documentación de features nuevos
- [ ] Release notes actualizadas

---

## 🎉 Quick Start

### Para comenzar Sprint 1:
```bash
# Crear branch del sprint
git checkout -b sprint-1-foundation

# Setup DB
cd backend
npm run db:migrate

# Crear seed data
npm run db:seed:featured

# Verificar
npm run test:db
```

### Para cada nuevo sprint:
```bash
# Merge sprint anterior
git checkout feature/featured-listings-monetization
git merge sprint-N-name

# Crear branch nuevo sprint
git checkout -b sprint-N+1-name

# Pull latest
git pull origin feature/featured-listings-monetization
```

---

**¿Listo para empezar Sprint 1?** 🚀
