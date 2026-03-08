# 🏁 Análisis Competitivo: OKLA vs SuperCarros vs Facebook Marketplace

**Fecha:** 2026-03-06  
**Autor:** CPSO (Chief Product & Strategy Officer)  
**Tipo:** Investigación Estratégica

---

## 1. Resumen Ejecutivo

SuperCarros.com domina el mercado dominicano con **66,000+ visitantes diarios** y **381,859 búsquedas/día**. OKLA debe diferenciarse con tecnología superior (IA, UX moderna) y no competir en volumen puro a corto plazo.

---

## 2. Análisis de SuperCarros (Competidor #1)

### 2.1 Fortalezas

| Feature                           | Descripción                              | Impacto                |
| --------------------------------- | ---------------------------------------- | ---------------------- |
| **Tráfico masivo**                | 66K visitantes diarios, 381K búsquedas   | Red effects dominante  |
| **SuperCarrosTV**                 | Videos de vehículos integrados           | Engagement alto        |
| **Calculadora de financiamiento** | Integración con entidades financieras DR | Conveniencia comprador |
| **25+ años operando** (2001-2026) | Brand recognition, SEO maduro            | Confianza              |
| **Directorio de Dealers**         | Red de dealers establecida               | Supply-side moat       |
| **Sección Eléctricos**            | Categoría dedicada para EVs              | Forward-looking        |
| **Blog activo**                   | Content marketing, SEO                   | Tráfico orgánico       |
| **Paginación eficiente**          | 100 páginas de resultados                | Catálogo amplio        |

### 2.2 Debilidades Identificadas

| Debilidad                         | Oportunidad para OKLA                                   |
| --------------------------------- | ------------------------------------------------------- |
| **UI anticuada**                  | Framework web legacy (jQuery) vs Next.js 16 moderno     |
| **Sin IA**                        | No tiene búsqueda NLP ni recomendaciones personalizadas |
| **Sin comparación**               | No permite comparar vehículos lado a lado               |
| **Fotos básicas**                 | Sin procesamiento IA de imágenes                        |
| **Búsqueda por formulario**       | Solo filtros dropdown, sin NLP                          |
| **Sin verificación de identidad** | No tiene KYC para sellers                               |
| **Sin historial de precios**      | No muestra evolución de precios                         |
| **Diseño no responsive**          | URL separada para móvil (m.supercarros.com)             |
| **Sin chatbot**                   | No tiene asistente inteligente                          |
| **Sin reviews/ratings**           | No hay sistema de reputación                            |

---

## 3. Análisis de Facebook Marketplace (Competidor #2)

### 3.1 Fortalezas

| Feature                        | Impacto                             |
| ------------------------------ | ----------------------------------- |
| **Base de usuarios masiva**    | Todos ya tienen cuenta              |
| **Messenger integrado**        | Chat directo con vendedor           |
| **Gratis para publicar**       | Sin barrera de entrada              |
| **Algoritmo de recomendación** | Muestra listados relevantes en feed |
| **Social proof**               | Perfil del vendedor visible         |

### 3.2 Debilidades

| Debilidad                       | Oportunidad para OKLA                           |
| ------------------------------- | ----------------------------------------------- |
| **Cero verificación**           | Scams frecuentes → KYC de OKLA es diferenciador |
| **Sin filtros automotrices**    | No filtra por marca/modelo/año eficientemente   |
| **Sin VIN decode**              | No verifica historial del vehículo              |
| **Mezclado con todo**           | No es especializado → UX confusa                |
| **Sin financiamiento**          | No integra calculadoras ni bancos               |
| **Sin historial de inspección** | No hay confianza en el estado del vehículo      |

---

## 4. Ventajas Competitivas Actuales de OKLA

### 4.1 Tecnología Superior

| Feature OKLA                       | Status  | Ventaja vs Competencia               |
| ---------------------------------- | ------- | ------------------------------------ |
| **Búsqueda IA (SearchAgent)**      | ✅ Live | Claude NLP en español dominicano     |
| **Recomendaciones IA (RecoAgent)** | ✅ Live | Claude Sonnet 4.5 personalizado      |
| **Procesamiento de imágenes IA**   | ✅ Live | Background removal, segmentación     |
| **Chatbot LLM**                    | ✅ Live | Soporte 24/7 con Claude              |
| **Comparación de vehículos**       | ✅ Live | Side-by-side comparison              |
| **VIN Decode**                     | ✅ Live | Verificación de historial            |
| **KYC verification**               | ✅ Live | Identidad verificada → confianza     |
| **PWA + Flutter mobile**           | ✅ Live | Experiencia nativa mobile            |
| **Review system**                  | ✅ Live | Reputación de vendedores             |
| **Dealer Analytics**               | ✅ Live | Dashboard para dealers profesionales |

### 4.2 Arquitectura

| Aspecto             | OKLA                    | SuperCarros                           |
| ------------------- | ----------------------- | ------------------------------------- |
| **Stack**           | .NET 8 + Next.js 16     | Legacy (jQuery + servidor monolítico) |
| **Arquitectura**    | 24+ microservicios      | Monolito                              |
| **Infraestructura** | Kubernetes (DOKS)       | VPS tradicional (probable)            |
| **Escalabilidad**   | Horizontal auto-scaling | Limitada                              |
| **SEO**             | SSR/SSG con Next.js     | Server-rendered legacy                |

---

## 5. Features Prioritarias para Diferenciación

### 5.1 P0 — Must Have (Sprint 21-23)

#### 🏦 Calculadora de Financiamiento

- **Por qué**: SuperCarros la tiene, OKLA no. Es la #1 feature que falta.
- **Integración**: Banpopular, BHD, Scotiabank, Asociación Popular
- **Implementación**: Widget en detalle de vehículo + página standalone
- **Estimación**: 2-3 sprints (service nuevo `FinancingService`)

#### 📊 Historial de Precios

- **Por qué**: Diferenciador único que ningún competidor tiene
- **Feature**: Gráfico de evolución del precio de un vehículo en el mercado DR
- **Data source**: Scraping público + data propia
- **Estimación**: 1-2 sprints

#### 🎥 Video en Listados

- **Por qué**: SuperCarrosTV demuestra que el video convierte
- **Feature**: Upload de video corto (30-60s) en cada listado
- **Implementación**: MediaService ya existe → extender para video
- **Estimación**: 1 sprint

### 5.2 P1 — High Impact (Sprint 24-26)

#### 📱 Notificaciones Push de Precio

- **Feature**: "El Toyota Corolla que buscas bajó de precio"
- **Diferenciador**: Engagement superior a SuperCarros (solo tiene email)
- **Implementación**: NotificationService + FCM/APNs

#### 🔍 Búsqueda por Imagen

- **Feature**: "Sube una foto y encuentra vehículos similares"
- **Diferenciador**: Ningún competidor lo tiene
- **Implementación**: AIProcessingService + embedding similarity

#### 📋 Inspección Virtual

- **Feature**: Checklist de inspección + fotos verificadas del vehículo
- **Diferenciador**: Confianza > cualquier competidor
- **Badge**: "OKLA Verified" en listados inspeccionados

### 5.3 P2 — Nice to Have (Sprint 27+)

#### 🏷️ Market Value Estimator

- **Feature**: "Tu vehículo vale entre RD$X y RD$Y"
- **Data**: Modelos ML entrenados con data de mercado DR
- **Referencia**: KBB / Edmunds para DR

#### 🤝 Integración DGII

- **Feature**: Verificación de matrícula, impuestos al día
- **Documentación**: Ya investigada en OKLA_DGII_INTEGRATION_RESEARCH_2026-03.md

#### 🌐 Expansión Caribe

- **Mercados**: Puerto Rico, Cuba (diáspora), Haití
- **Multi-tenancy**: Ya soportado en arquitectura

---

## 6. Estrategia de Posicionamiento

### 6.1 Mensaje Central

> **"OKLA: El marketplace de vehículos más inteligente y seguro de RD"**

### 6.2 Pilares de Diferenciación

1. **IA Integrada** — Búsqueda NLP, recomendaciones personalizadas, chatbot 24/7
2. **Confianza** — KYC verificado, reviews, inspección virtual, DGII
3. **Experiencia Moderna** — App nativa, PWA, comparación, alertas inteligentes
4. **Analytics para Dealers** — Dashboard completo, insights de mercado, ROI

### 6.3 Pricing Strategy (vs SuperCarros)

| Feature                | SuperCarros        | OKLA Propuesta                    |
| ---------------------- | ------------------ | --------------------------------- |
| Publicación básica     | Gratis con límites | Gratis (match)                    |
| Publicación destacada  | ~RD$1,500-3,000    | $29 USD (~RD$1,700) - competitivo |
| Plan Dealer Básico     | No público         | $49 USD/mes                       |
| Plan Dealer Pro        | No público         | $149 USD/mes                      |
| Plan Dealer Enterprise | No público         | $299 USD/mes                      |
| IA features            | N/A                | Incluido en planes Pro+           |

---

## 7. Métricas de Éxito

| Métrica                         | Meta Q2 2026 | Meta Q4 2026 |
| ------------------------------- | ------------ | ------------ |
| DAU (Daily Active Users)        | 5,000        | 20,000       |
| Listings activos                | 2,000        | 10,000       |
| Dealers registrados             | 50           | 200          |
| Conversión (listing → contacto) | 8%           | 12%          |
| NPS                             | 40+          | 55+          |
| Revenue mensual                 | $5,000 USD   | $25,000 USD  |

---

## 8. Conclusión

OKLA tiene una **ventaja tecnológica clara** sobre SuperCarros pero carece de volumen de mercado. La estrategia debe ser:

1. **Corto plazo**: Igualar features esenciales (calculadora financiamiento, video)
2. **Medio plazo**: Explotar ventajas IA que SuperCarros no puede replicar fácilmente
3. **Largo plazo**: Construir red effects con dealers y compradores verificados

La calculadora de financiamiento es el **#1 gap crítico** que debe cerrarse antes de Q2 2026.
