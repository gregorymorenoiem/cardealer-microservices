# 🗓️ Roadmap - Stripe Payment API

**API:** Stripe Payment Platform  
**Proveedor:** Stripe Inc.  
**Estado actual:** ✅ En Producción  
**Versión:** v2024-01-15

---

## 📅 Timeline General

| Fase       | Periodo | Estado         | Descripción                       |
| ---------- | ------- | -------------- | --------------------------------- |
| **Fase 1** | Q4 2025 | ✅ Completado  | Setup básico + Payments           |
| **Fase 2** | Q1 2026 | ✅ Completado  | Subscriptions + Webhooks          |
| **Fase 3** | Q1 2026 | ❌ Descartado  | ~~Connect~~ (No aplica al modelo) |
| **Fase 4** | Q2 2026 | 📝 Planificado | Advanced features                 |
| **Fase 5** | Q3 2026 | 📝 Planificado | Optimizaciones                    |

---

## ✅ Fase 1: Setup Básico (Q4 2025) - COMPLETADO

### Objetivos

- Configuración inicial de Stripe
- Pagos one-time básicos
- Testing en sandbox

### Entregables Completados

#### 1.1 Configuración Inicial ✅

- [x] Crear cuenta Stripe (sandbox + production)
- [x] Obtener API keys (publishable + secret)
- [x] Configurar webhooks endpoints
- [x] Instalar Stripe.NET SDK v43+

#### 1.2 Payment Intents API ✅

- [x] Crear PaymentIntent
- [x] Confirmar pago
- [x] Capturar pago
- [x] Manejo de 3D Secure

#### 1.3 Pagos de Compradores ✅

- [x] Pago de listados promocionados
- [x] Pago de servicios adicionales
- [x] Refunds básicos

**Sprint:** Sprint 4 - Pagos  
**Fecha de completado:** Diciembre 2025

---

## ✅ Fase 2: Subscripciones (Q1 2026) - COMPLETADO

### Objetivos

- Suscripciones mensuales para dealers
- Planes escalonados (Starter, Pro, Enterprise)
- Early Bird Program

### Entregables Completados

#### 2.1 Products & Prices ✅

- [x] Crear productos en Stripe Dashboard:
  - Starter ($49/mes)
  - Pro ($129/mes)
  - Enterprise ($299/mes)
- [x] Configurar precios recurrentes
- [x] Crear coupons para Early Bird (20% off)

#### 2.2 Customers API ✅

- [x] Crear customer en Stripe cuando dealer se registra
- [x] Vincular User ID con Stripe Customer ID
- [x] Guardar payment methods
- [x] Update customer metadata

#### 2.3 Subscriptions API ✅

- [x] Crear subscription
- [x] Aplicar trial period (3 meses Early Bird)
- [x] Update subscription (upgrade/downgrade plan)
- [x] Cancel subscription
- [x] Reactivar subscription

#### 2.4 Webhooks ✅

- [x] `invoice.payment_succeeded` - Confirmar pago mensual
- [x] `invoice.payment_failed` - Suspender dealer
- [x] `customer.subscription.updated` - Sync plan changes
- [x] `customer.subscription.deleted` - Desactivar cuenta
- [x] Validar webhook signature
- [x] Idempotencia con Redis

**Sprint:** Sprint 5 - Dealer Dashboard  
**Fecha de completado:** Enero 8, 2026

---

## ❌ Fase 3: DESCARTADA - Stripe Connect NO APLICA

> **⚠️ ACLARACIÓN IMPORTANTE DEL MODELO DE NEGOCIO:**
>
> OKLA es una plataforma de **PUBLICIDAD**, no un marketplace transaccional.
>
> - **Dealers PAGAN a OKLA** por suscripciones mensuales (RD$2,900-14,900/mes)
> - **La venta del vehículo es EXTERNA** a OKLA (dealer ↔ comprador directo)
> - **OKLA NO procesa pagos de vehículos** ni cobra comisiones por venta
>
> Por lo tanto, **Stripe Connect NO es necesario** ya que:
>
> - No hay split payments entre OKLA y dealers
> - No hay transferencias a dealers por ventas
> - Los dealers son CLIENTES que pagan, no comerciantes que reciben

### ❌ Entregables CANCELADOS (No aplican al modelo de negocio)

Los siguientes items fueron planificados incorrectamente y se descartan:

- ~~Stripe Connect account~~
- ~~Onboarding de dealers (KYC para pagos)~~
- ~~Split payments (70% dealer, 30% plataforma)~~
- ~~Direct charges / Destination charges~~
- ~~Transferir fondos a dealers~~
- ~~Dashboard de earnings para dealers~~
- ~~Comisiones por venta~~

### ✅ Modelo Correcto de Pagos OKLA

```
┌────────────────────────────────────────────────────────────────────┐
│                   FLUJO DE PAGOS OKLA                              │
├────────────────────────────────────────────────────────────────────┤
│                                                                     │
│   DEALER ───[Paga suscripción]───> OKLA (via Azul/Stripe)         │
│           RD$2,900-14,900/mes                                       │
│                                                                     │
│   COMPRADOR ───[Paga vehículo]───> DEALER (EXTERNO a OKLA)        │
│              Transacción directa, fuera de la plataforma           │
│                                                                     │
│   OKLA recibe ingresos SOLO de suscripciones de dealers           │
│   OKLA NO participa en transacciones de vehículos                  │
│                                                                     │
└────────────────────────────────────────────────────────────────────┘
```

**Estado:** ❌ DESCARTADO  
**Razón:** No aplica al modelo de negocio de OKLA

---

## 📝 Fase 4: Advanced Features (Q2 2026) - PLANIFICADO

### Objetivos

- Features avanzados de Stripe
- Optimizar conversión
- Métodos de pago adicionales

### Entregables

#### 4.1 Stripe Checkout 📝

- [ ] Migrar a Stripe Checkout (hosted payment page)
- [ ] Personalizar branding
- [ ] Multi-currency support
- [ ] Tax calculation automático

#### 4.2 Payment Methods Adicionales 📝

- [ ] Apple Pay integration
- [ ] Google Pay integration
- [ ] ACH bank transfers (USA)
- [ ] SEPA (Europa) si aplicable

#### 4.3 Billing Portal 📝

- [ ] Dealers self-service para:
  - Actualizar payment method
  - Ver historial de facturas
  - Descargar invoices
  - Cambiar plan
  - Cancelar suscripción

#### 4.4 Dunning (recuperación de pagos fallidos) 📝

- [ ] Retry automático de pagos fallidos
- [ ] Smart retries (días estratégicos)
- [ ] Notificaciones antes de suspensión
- [ ] Grace period configurable

**Sprint:** Sprints 22-23  
**Fecha estimada:** Abril-Mayo 2026

---

## 📝 Fase 5: Optimizaciones (Q3 2026) - PLANIFICADO

### Objetivos

- Optimizar tasas de conversión
- Reducir churn
- Analytics avanzado

### Entregables

#### 5.1 Subscription Management 📝

- [ ] Prorate upgrades/downgrades
- [ ] Add-ons (listados extra, featured, etc.)
- [ ] Usage-based billing (por leads, contactos)
- [ ] Quantity-based pricing (por sucursal)

#### 5.2 Fraud Prevention 📝

- [ ] Stripe Radar (detección de fraude)
- [ ] 3D Secure obligatorio para altos montos
- [ ] Velocity checks (límites por periodo)
- [ ] Blacklist de tarjetas

#### 5.3 Analytics & Reporting 📝

- [ ] Dashboard de métricas Stripe:
  - MRR (Monthly Recurring Revenue)
  - Churn rate
  - LTV (Lifetime Value)
  - Conversion funnel
- [ ] Integración con Google Analytics
- [ ] Custom reports en admin panel

#### 5.4 Testing & QA 📝

- [ ] Tests automatizados de webhooks
- [ ] Simular scenarios de pago
- [ ] Load testing de checkout
- [ ] A/B testing de pricing

**Sprint:** Sprints 26-27  
**Fecha estimada:** Julio-Agosto 2026

---

## 🎯 Métricas de Éxito

### KPIs por Fase

| Fase       | KPI                    | Target | Actual        |
| ---------- | ---------------------- | ------ | ------------- |
| **Fase 1** | Tasa de éxito de pagos | >95%   | 98% ✅        |
| **Fase 2** | Dealers suscritos      | 50+    | 23 🚧         |
| **Fase 2** | MRR                    | $5,000 | $2,500 🚧     |
| **Fase 3** | ~~Split payments~~     | N/A    | ❌ Descartado |
| **Fase 4** | Churn rate             | <5%    | -             |
| **Fase 5** | Fraud rate             | <0.5%  | -             |

---

## 🚀 Próximos Pasos (Enero 2026)

### Inmediato (Sprint 18)

1. ✅ Verificar que Stripe está funcionando correctamente
2. ✅ Migrar cobros de suscripciones a Azul (default para tarjetas RD)
3. 📝 Mantener Stripe como backup para tarjetas internacionales
4. 📝 Optimizar experiencia de checkout

### Corto Plazo (Febrero 2026)

1. Implementar Stripe Checkout como alternativa
2. Agregar Apple Pay y Google Pay
3. Billing Portal para dealers
4. Documentación actualizada

> **NOTA:** Stripe Connect fue descartado porque OKLA no procesa transacciones
> de vehículos. Los dealers PAGAN a OKLA por publicidad, no reciben pagos.

### Mediano Plazo (Q2 2026)

1. Migrar a Stripe Checkout
2. Agregar Apple Pay y Google Pay
3. Implementar Billing Portal
4. Dunning strategy

---

## 📚 Referencias Técnicas

### Documentación Stripe

- [Payment Intents API](https://stripe.com/docs/api/payment_intents)
- [Subscriptions](https://stripe.com/docs/billing/subscriptions/overview)
- [Connect](https://stripe.com/docs/connect)
- [Webhooks](https://stripe.com/docs/webhooks)
- [Stripe Checkout](https://stripe.com/docs/payments/checkout)

### Implementación OKLA

- [STRIPE_API_DOCUMENTATION.md](STRIPE_API_DOCUMENTATION.md)
- [BillingService README](../../../backend/BillingService/README.md)
- Sprint 4 Completed: [SPRINT_4_COMPLETED.md](../../SPRINT_4_COMPLETED.md)
- Sprint 5 Completed: [SPRINT_5_DEALER_DASHBOARD_COMPLETED.md](../../SPRINT_5_DEALER_DASHBOARD_COMPLETED.md)

---

## ⚠️ Riesgos y Mitigación

| Riesgo                    | Probabilidad | Impacto | Mitigación                         |
| ------------------------- | ------------ | ------- | ---------------------------------- |
| **Pagos fallidos**        | Media        | Alto    | Dunning strategy, retry automático |
| **Fraude con tarjetas**   | Baja         | Alto    | Stripe Radar, 3D Secure            |
| **Churn alto de dealers** | Media        | Alto    | Billing Portal, customer success   |
| **Downtime de Stripe**    | Muy Baja     | Alto    | Fallback a AZUL, queue de pagos    |

---

## 💡 Ideas Futuras (Backlog)

- [ ] **Pagos en cuotas** (installments) para compradores
- [ ] **Wallet OKLA** (saldo prepago)
- [ ] **Cashback program** para compradores frecuentes
- [ ] **Referral bonuses** para dealers
- [ ] **Dynamic pricing** basado en demanda
- [ ] **Cryptocurrency payments** (Bitcoin, USDC)
- [ ] **Buy Now Pay Later** (BNPL) integration

---

**Última actualización:** Enero 15, 2026  
**Próxima revisión:** Marzo 1, 2026  
**Responsable:** Equipo de Payments + Billing
