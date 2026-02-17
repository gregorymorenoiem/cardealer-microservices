# 🔍 Análisis Competitivo: CarGurus y Plataformas Líderes vs OKLA

> **Documento:** Análisis de Gap Competitivo  
> **Versión:** 1.0  
> **Fecha:** Enero 21, 2026  
> **Autor:** Equipo de Producto OKLA  
> **Clasificación:** Interno - Estratégico

---

## 📋 Resumen Ejecutivo

Este documento presenta un análisis exhaustivo de las funcionalidades de las principales plataformas de compra-venta de vehículos a nivel mundial, identificando oportunidades de mejora para OKLA en el mercado dominicano.

### Plataformas Analizadas

| Plataforma          | País         | Modelo de Negocio                  | Usuarios Mensuales |
| ------------------- | ------------ | ---------------------------------- | ------------------ |
| **CarGurus**        | EE.UU.       | Marketplace + Pricing Intelligence | 30M+               |
| **Cars.com**        | EE.UU.       | Marketplace tradicional            | 25M+               |
| **AutoTrader**      | EE.UU./UK    | Marketplace premium                | 20M+               |
| **Carvana**         | EE.UU.       | 100% online, delivery              | 15M+               |
| **Kavak**           | México/LATAM | Compra/venta garantizada           | 5M+                |
| **Seminuevos.com**  | México       | Marketplace tradicional            | 3M+                |
| **SuperCarros.com** | RD           | Marketplace local                  | ~500K              |

---

## 🎯 Hallazgos Clave

### Funcionalidades Implementables en RD

Se identificaron **17 procesos** que pueden implementarse en República Dominicana, clasificados por prioridad:

| Prioridad | Cantidad | Story Points | Impacto en UX |
| --------- | -------- | ------------ | ------------- |
| 🔴 ALTA   | 6        | 27 SP        | Crítico       |
| 🟡 MEDIA  | 9        | 32 SP        | Importante    |
| 🟢 BAJA   | 2        | 10 SP        | Nice-to-have  |
| **TOTAL** | **17**   | **69 SP**    | -             |

### Funcionalidades NO Implementables en RD

Se identificaron **8 procesos** que NO son viables en República Dominicana debido a limitaciones regulatorias, de infraestructura o de mercado.

---

## ✅ PROCESOS ALTA PRIORIDAD (Implementar Inmediatamente)

### 1. Deal Rating / Precio Justo

| Campo             | Valor                                                                                         |
| ----------------- | --------------------------------------------------------------------------------------------- |
| **Código**        | PRICE-001                                                                                     |
| **Origen**        | CarGurus                                                                                      |
| **Descripción**   | Algoritmo que califica el precio como "Great Deal", "Good Deal", "Fair", "High", "Overpriced" |
| **Impacto**       | Diferenciador clave - principal feature de CarGurus                                           |
| **Esfuerzo**      | 3 SP                                                                                          |
| **Documentación** | [20-PRICING-INTELLIGENCE/01-deal-rating.md](20-PRICING-INTELLIGENCE/01-deal-rating.md)        |

### 2. Valuación Instantánea (IMV)

| Campo             | Valor                                                                                                      |
| ----------------- | ---------------------------------------------------------------------------------------------------------- |
| **Código**        | VALUE-001                                                                                                  |
| **Origen**        | CarGurus, Kavak                                                                                            |
| **Descripción**   | Calculadora que indica cuánto vale un vehículo específico ahora mismo                                      |
| **Impacto**       | Herramienta de captación de vendedores                                                                     |
| **Esfuerzo**      | 3 SP                                                                                                       |
| **Documentación** | [20-PRICING-INTELLIGENCE/02-valuacion-instantanea.md](20-PRICING-INTELLIGENCE/02-valuacion-instantanea.md) |

### 3. Dealer Reviews & Ratings

| Campo             | Valor                                                                                    |
| ----------------- | ---------------------------------------------------------------------------------------- |
| **Código**        | REVIEW-001, REVIEW-002, REVIEW-003                                                       |
| **Origen**        | CarGurus, Cars.com, Google                                                               |
| **Descripción**   | Sistema de reviews verificados de compradores sobre dealers                              |
| **Impacto**       | Confianza y transparencia - crítico para marketplace                                     |
| **Esfuerzo**      | 5 SP                                                                                     |
| **Documentación** | [21-REVIEWS-REPUTACION/01-dealer-reviews.md](21-REVIEWS-REPUTACION/01-dealer-reviews.md) |

### 4. OKLA Certified Pre-Owned

| Campo             | Valor                                                                                      |
| ----------------- | ------------------------------------------------------------------------------------------ |
| **Código**        | CERT-001                                                                                   |
| **Origen**        | AutoTrader, Cars.com                                                                       |
| **Descripción**   | Programa de certificación con criterios estrictos y garantía                               |
| **Impacto**       | Diferenciación premium - mayor confianza                                                   |
| **Esfuerzo**      | 5 SP                                                                                       |
| **Documentación** | [15-CONFIANZA-SEGURIDAD/05-okla-certified.md](15-CONFIANZA-SEGURIDAD/05-okla-certified.md) |

### 5. Chat en Tiempo Real

| Campo             | Valor                                                                                        |
| ----------------- | -------------------------------------------------------------------------------------------- |
| **Código**        | CHAT-001                                                                                     |
| **Origen**        | Cars.com, WhatsApp Business                                                                  |
| **Descripción**   | Chat en tiempo real con vendedor desde el listing                                            |
| **Impacto**       | Mejor conversión - comunicación inmediata                                                    |
| **Esfuerzo**      | 8 SP                                                                                         |
| **Documentación** | [22-COMUNICACION-REALTIME/01-chat-realtime.md](22-COMUNICACION-REALTIME/01-chat-realtime.md) |

### 6. Filtros Avanzados de Búsqueda

| Campo             | Valor                                                                                      |
| ----------------- | ------------------------------------------------------------------------------------------ |
| **Código**        | SEARCH-001                                                                                 |
| **Origen**        | CarGurus                                                                                   |
| **Descripción**   | Filtros: Deal Rating, Days on Market, Price Drops, New Listings                            |
| **Impacto**       | Mejor UX de búsqueda - más engagement                                                      |
| **Esfuerzo**      | 3 SP                                                                                       |
| **Documentación** | [04-BUSQUEDA-FILTROS/03-filtros-avanzados.md](04-BUSQUEDA-FILTROS/03-filtros-avanzados.md) |

---

## 🟡 PROCESOS MEDIA PRIORIDAD (Roadmap Q2-Q3 2026)

| #   | Proceso                | Código        | Origen      | Esfuerzo | Documentación                               |
| --- | ---------------------- | ------------- | ----------- | -------- | ------------------------------------------- |
| 1   | Historial de Precios   | PRICE-002     | CarGurus    | 2 SP     | [ROADMAP-FUTUROS.md](00-ROADMAP-FUTUROS.md) |
| 2   | Tendencias de Mercado  | ANALYTICS-001 | CarGurus    | 5 SP     | [ROADMAP-FUTUROS.md](00-ROADMAP-FUTUROS.md) |
| 3   | Cita Virtual (Video)   | VIRTUAL-001   | Cars.com    | 3 SP     | [ROADMAP-FUTUROS.md](00-ROADMAP-FUTUROS.md) |
| 4   | Recomendaciones ML     | REC-001       | Todos       | 5 SP     | [ROADMAP-FUTUROS.md](00-ROADMAP-FUTUROS.md) |
| 5   | Comparación con TCO    | COMPARE-002   | CarGurus    | 3 SP     | [ROADMAP-FUTUROS.md](00-ROADMAP-FUTUROS.md) |
| 6   | Perfil Dealer Mejorado | DEALER-001    | AutoTrader  | 3 SP     | [ROADMAP-FUTUROS.md](00-ROADMAP-FUTUROS.md) |
| 7   | Validación Fotos AI    | MEDIA-001     | Kavak       | 5 SP     | [ROADMAP-FUTUROS.md](00-ROADMAP-FUTUROS.md) |
| 8   | Niveles Verificación   | TRUST-007     | Kavak       | 3 SP     | [ROADMAP-FUTUROS.md](00-ROADMAP-FUTUROS.md) |
| 9   | Performance Dashboard  | PERF-001      | eBay Motors | 3 SP     | [ROADMAP-FUTUROS.md](00-ROADMAP-FUTUROS.md) |

---

## 🟢 PROCESOS BAJA PRIORIDAD (Roadmap Q4 2026+)

| #   | Proceso               | Código      | Origen   | Esfuerzo | Documentación                               |
| --- | --------------------- | ----------- | -------- | -------- | ------------------------------------------- |
| 1   | Guías de Compra (CMS) | CONTENT-001 | Cars.com | 5 SP     | [ROADMAP-FUTUROS.md](00-ROADMAP-FUTUROS.md) |
| 2   | Garantía Satisfacción | TRUST-008   | Carvana  | 2 SP     | [ROADMAP-FUTUROS.md](00-ROADMAP-FUTUROS.md) |

---

## ❌ PROCESOS NO VIABLES EN RD

### Limitaciones Regulatorias

| Proceso                 | Plataforma | Razón de Exclusión                            | Alternativa OKLA                   |
| ----------------------- | ---------- | --------------------------------------------- | ---------------------------------- |
| In-House Financing      | CarMax     | Requiere licencia SIB como entidad financiera | Integración con bancos RD          |
| Instant Online Purchase | Carvana    | Transferencia DGII/INTRANT requiere presencia | Reserva online + cierre presencial |

### Limitaciones de Infraestructura

| Proceso             | Plataforma | Razón de Exclusión                          | Alternativa OKLA                       |
| ------------------- | ---------- | ------------------------------------------- | -------------------------------------- |
| Home Delivery       | Carvana    | No hay logística de transporte de vehículos | Pick-up en dealer o punto de encuentro |
| Vending Machine     | Carvana    | No viable logísticamente                    | N/A                                    |
| EV Charging Locator | Cars.com   | Infraestructura EV muy limitada             | Implementar cuando crezca adopción     |

### Limitaciones de Datos

| Proceso            | Plataforma | Razón de Exclusión                        | Alternativa OKLA                     |
| ------------------ | ---------- | ----------------------------------------- | ------------------------------------ |
| Carfax Integration | CarGurus   | Carfax no opera en RD                     | OKLA Historia con datos DGII/INTRANT |
| Accident History   | Carfax     | No existe base centralizada de accidentes | Self-disclosure + inspección         |
| Extended Warranty  | AutoTrader | Proveedores (EasyCare) no operan en RD    | Alianzas con aseguradoras locales    |

---

## 📊 Impacto Proyectado

### Métricas Objetivo con Implementación

| Métrica                | Actual   | Con Alta Prioridad | Con Todas       |
| ---------------------- | -------- | ------------------ | --------------- |
| **Tasa de conversión** | 2.1%     | 3.5% (+67%)        | 4.5% (+114%)    |
| **Tiempo en sitio**    | 4:30 min | 6:00 min (+33%)    | 8:00 min (+78%) |
| **Páginas por sesión** | 5.2      | 7.0 (+35%)         | 9.0 (+73%)      |
| **NPS**                | 42       | 55 (+31%)          | 65 (+55%)       |
| **Bounce rate**        | 45%      | 35% (-22%)         | 28% (-38%)      |

### ROI Estimado

| Inversión            | Q1 2026 | Q2 2026 | Q3 2026 | Q4 2026 |
| -------------------- | ------- | ------- | ------- | ------- |
| **Desarrollo**       | $15K    | $20K    | $15K    | $10K    |
| **Incremento leads** | +500    | +1,200  | +1,800  | +2,500  |
| **Valor leads**      | $25K    | $60K    | $90K    | $125K   |
| **ROI**              | 67%     | 200%    | 500%    | 1150%   |

---

## 🚀 Plan de Implementación

### Sprint Inmediato (Q1 2026)

```
Semana 1-2: PRICE-001 (Deal Rating) + SEARCH-001 (Filtros Avanzados)
Semana 3-4: VALUE-001 (Valuación Instantánea)
Semana 5-6: REVIEW-001,002,003 (Dealer Reviews)
Semana 7-8: CERT-001 (OKLA Certified)
Semana 9-12: CHAT-001 (Chat en Tiempo Real)
```

### Dependencias Técnicas

```
PricingIntelligenceService (nuevo) → PRICE-001, VALUE-001
ReviewService (existente, extender) → REVIEW-001,002,003
TrustService (existente, extender) → CERT-001
ChatService (nuevo) → CHAT-001
VehiclesSaleService (extender) → SEARCH-001
```

---

## 📁 Estructura de Documentación

```
docs/process-matrix/
├── 00-ANALISIS-COMPETITIVO.md          # Este documento
├── 00-ROADMAP-FUTUROS.md               # Procesos media/baja prioridad
│
├── 04-BUSQUEDA-FILTROS/
│   └── 03-filtros-avanzados.md         # SEARCH-001 ⭐
│
├── 15-CONFIANZA-SEGURIDAD/
│   └── 05-okla-certified.md            # CERT-001 ⭐
│
├── 20-PRICING-INTELLIGENCE/            # NUEVA CATEGORÍA
│   ├── 01-deal-rating.md               # PRICE-001 ⭐
│   └── 02-valuacion-instantanea.md     # VALUE-001 ⭐
│
├── 21-REVIEWS-REPUTACION/              # NUEVA CATEGORÍA
│   └── 01-dealer-reviews.md            # REVIEW-001,002,003 ⭐
│
└── 22-COMUNICACION-REALTIME/           # NUEVA CATEGORÍA
    └── 01-chat-realtime.md             # CHAT-001 ⭐
```

---

## ✅ Conclusiones

1. **OKLA puede alcanzar paridad competitiva** con CarGurus implementando los 6 procesos de alta prioridad
2. **El Deal Rating es el diferenciador #1** - debe ser prioridad absoluta
3. **El Chat en tiempo real** mejorará significativamente la conversión
4. **El programa OKLA Certified** posicionará la plataforma como premium
5. **Las limitaciones de RD** (regulatorias, infraestructura) son manejables con alternativas locales

---

## 📎 Anexos

- [A] Documentación de procesos alta prioridad (ver carpetas correspondientes)
- [B] Roadmap de procesos media/baja prioridad
- [C] Benchmarks de competidores (datos confidenciales)

---

_Documento preparado por el Equipo de Producto OKLA_  
_Próxima revisión: Abril 2026_
