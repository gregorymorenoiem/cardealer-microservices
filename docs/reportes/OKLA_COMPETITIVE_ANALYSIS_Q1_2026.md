# 📊 Análisis Competitivo — Mercado Automotriz Digital RD (Q1 2026)

**Fecha**: 6 de marzo de 2026
**Preparado por**: CPSO (Chief Product & Strategy Officer)
**Objetivo**: Benchmarking de competidores para informar el roadmap de OKLA

---

## 1. Resumen Ejecutivo

El mercado de marketplaces automotrices en República Dominicana está **subdesarrollado digitalmente**. Solo existen 2 competidores relevantes (Corotos y SuperCarros), ambos con tecnología legacy. No hay presencia de gigantes regionales como Kavak, OLX está inactivo, y AutoPlaza no tiene tracción.

**OKLA tiene una ventaja tecnológica significativa** con 25 microservicios, IA integrada, KYC, comparador de vehículos, y CRM para dealers. El momento para capturar mercado es ahora.

---

## 2. Competidores Analizados

### 2.1 SuperCarros (supercarros.com) — Competidor Principal

| Atributo              | Detalle                                                          |
| --------------------- | ---------------------------------------------------------------- |
| **Antigüedad**        | ~25 años (el más antiguo del mercado)                            |
| **Tráfico estimado**  | ~66,000 visitas/día                                              |
| **Modelo de negocio** | Listados gratuitos básicos + destacados pagos (~US$54/destacado) |
| **Tecnología**        | Website legacy, responsive pero no SPA                           |
| **App mobile**        | Sí (básica)                                                      |

**Features de SuperCarros:**

- ✅ Búsqueda por marca/modelo/año/precio/provincia
- ✅ **Calculadora de financiamiento** con entidades financieras locales (Asociación La Nacional, APAP, etc.)
- ✅ Panel básico para dealers
- ✅ Categorías: autos, SUVs, camionetas, motocicletas, camiones
- ✅ Historial de precios (parcial)
- ❌ Sin comparador de vehículos
- ❌ Sin KYC/verificación de vendedores
- ❌ Sin IA o chatbot
- ❌ Sin deal rating / precio justo
- ❌ Sin CRM avanzado para dealers
- ❌ Sin analytics para dealers

### 2.2 Corotos (corotos.com.do) — Marketplace Generalista

| Atributo              | Detalle                                          |
| --------------------- | ------------------------------------------------ |
| **Tipo**              | Clasificados generales (no solo autos)           |
| **Tráfico estimado**  | Alto (top 10 de sitios en RD)                    |
| **Modelo de negocio** | Listados gratuitos + planes premium + publicidad |
| **Tecnología**        | Website moderno, responsive                      |

**Features de Corotos (sección vehículos):**

- ✅ Búsqueda por categoría/marca/modelo
- ✅ Filtros por condición, precio, ubicación
- ✅ Badge de verificación básico (cuenta verificada)
- ✅ Contacto directo via WhatsApp/teléfono
- ✅ App mobile
- ❌ Sin comparador
- ❌ Sin financiamiento integrado
- ❌ Sin IA
- ❌ Sin CRM para dealers
- ❌ Sin analytics
- ❌ Sin deal rating

### 2.3 Competidores Ausentes del Mercado RD

| Plataforma                          | Estado en RD                                                   |
| ----------------------------------- | -------------------------------------------------------------- |
| **Kavak**                           | ❌ NO opera en RD (solo México, Argentina, Brasil, Chile, EAU) |
| **OLX**                             | ❌ Dominio inactivo en RD (redirige o sin contenido)           |
| **AutoPlaza**                       | ❌ Sin tracción aparente                                       |
| **CarGurus / AutoTrader / TrueCar** | ❌ Solo operan en mercados anglosajones                        |

---

## 3. Matriz Comparativa de Features

| Feature                            |        SuperCarros        |     Corotos     |            **OKLA**            | Ventaja OKLA |
| ---------------------------------- | :-----------------------: | :-------------: | :----------------------------: | :----------: |
| Búsqueda avanzada                  |         ✅ Básica         |    ✅ Básica    | ✅ **IA + filtros avanzados**  |      🏆      |
| Comparador de vehículos            |            ❌             |       ❌        |      ✅ Hasta 3 vehículos      |      🏆      |
| KYC / Verificación de identidad    |            ❌             | ⚠️ Badge básico |  ✅ **Documento + liveness**   |      🏆      |
| Chatbot con IA                     |            ❌             |       ❌        |      ✅ **RAG + agentes**      |      🏆      |
| Deal Rating / Precio justo         |            ❌             |       ❌        |   ✅ **VehicleIntelligence**   |      🏆      |
| CRM para dealers                   |      ⚠️ Panel básico      |       ❌        |      ✅ **CRM completo**       |      🏆      |
| Analytics para dealers             |            ❌             |       ❌        |     ✅ **DealerAnalytics**     |      🏆      |
| Reviews y ratings                  |            ❌             |    ⚠️ Básico    |      ✅ **ReviewService**      |      🏆      |
| Notificaciones push                |            ❌             |       ❌        |       ✅ **PWA + push**        |      🏆      |
| **Calculadora financiamiento**     | ✅ **Con bancos locales** |       ❌        |        ⚠️ **Pendiente**        |    ⚠️ Gap    |
| **Integración bancos/financieras** |            ✅             |       ❌        |               ❌               |    🔴 Gap    |
| App mobile nativa                  |         ✅ Básica         |       ✅        |           ✅ Flutter           |  🟡 Paridad  |
| 360° fotos                         |            ❌             |       ❌        |               ✅               |      🏆      |
| Eliminación de fondo IA            |            ❌             |       ❌        |      ✅ **AIProcessing**       |      🏆      |
| SEO landing pages                  |            ⚠️             |       ⚠️        | ✅ **Por marca/modelo/ciudad** |      🏆      |

---

## 4. Análisis SWOT de OKLA vs Mercado

### Fortalezas (Strengths)

- **Stack tecnológico superior**: 25 microservicios .NET 8 + Next.js 16 vs websites legacy
- **IA integrada**: Chatbot, búsqueda semántica, recomendaciones, deal rating — ningún competidor tiene esto
- **Seguridad robusta**: KYC con liveness, CSRF, XSS prevention, JWT, rate limiting
- **Ecosistema dealer**: CRM + Analytics + Billing — herramientas profesionales para concesionarios
- **Arquitectura escalable**: Kubernetes, microservicios, event-driven — listo para escalar al Caribe
- **Precio competitivo**: US$29/listado vs US$54/destacado en SuperCarros

### Debilidades (Weaknesses)

- **Sin financiamiento integrado**: Gap crítico vs SuperCarros — el financiamiento es la feature #1 que buscan compradores en RD
- **Marca nueva**: SuperCarros tiene 25 años de reconocimiento
- **Tráfico**: Sin base de usuarios establecida
- **Flutter vs React Native**: La app mobile usa Flutter en lugar de React Native (desalineación con docs de arquitectura)
- **Deuda técnica**: 8 servicios en Tier 4 sin extensiones compartidas

### Oportunidades (Opportunities)

- **Financiamiento in-app**: Integrar con APAP, Asociación La Nacional, BanReservas, Popular — alto impacto en conversión
- **Inspección vehicular**: Servicio de inspección pre-compra — ningún competidor lo ofrece
- **Historial vehicular**: Integración con DGII/Aduanas para reporte de importación y placas
- **Seguros integrados**: Cotización de seguros directamente en la plataforma
- **Expansión Caribe**: Haití, Puerto Rico, Jamaica — ningún marketplace regional dominante
- **B2B data**: Venta de market insights a dealers y marcas (pricing intelligence)

### Amenazas (Threats)

- **SuperCarros** podría modernizar su stack (improbable a corto plazo pero posible)
- **Kavak** podría entrar al mercado RD (opera en el Caribe ya con EAU)
- **Meta Marketplace**: Facebook Marketplace crece como canal informal de venta de vehículos en RD
- **WhatsApp Business**: Muchos dealers operan 100% via WhatsApp, resistencia a adoptar plataformas

---

## 5. Recomendaciones Estratégicas (Q2 2026)

### 🔴 Prioridad Crítica

**1. Calculadora de financiamiento / Integración con financieras**

- **Justificación**: Es el feature #1 de SuperCarros y el principal diferenciador que les falta a todos los competidores modernos
- **Alcance**: Calculadora de cuotas mensuales + integración API con al menos 3 financieras dominicanas
- **Impacto estimado**: +40% conversión en listings de vehículos > US$15,000
- **Effort**: ~2 sprints (BillingService ya existe como base)

### 🟡 Prioridad Alta

**2. Historial vehicular (DGII/Aduanas)**

- Integrar con la DGII para verificar placas, impuestos pagos, y estado legal del vehículo
- Ningún competidor ofrece esto — sería un moat significativo
- Effort: ~3 sprints (requiere convenio con DGII)

**3. Inspección vehicular certificada**

- Servicio de inspección mecánica pre-compra con reporte digital
- Partnership con talleres certificados
- Effort: ~2 sprints + partnerships

### 🟢 Prioridad Media

**4. Seguros integrados**

- Cotización en línea con aseguradoras dominicanas
- Comisión por referido como revenue adicional

**5. Programa de referidos**

- Viral loop: invita a un amigo vendedor → descuento en próximo listing
- Reduce CAC significativamente

---

## 6. Oportunidades de Monetización Adicionales

| Fuente                             | Revenue Estimado       | Complejidad      |
| ---------------------------------- | ---------------------- | ---------------- |
| Financiamiento (comisión por lead) | US$15-50/lead aprobado | Alta             |
| Seguros (referido)                 | US$10-30/póliza        | Media            |
| Inspección vehicular               | US$20-40/inspección    | Media            |
| Featured listings (dealers)        | US$30-100/listing/mes  | Baja (ya existe) |
| Market data reports (B2B)          | US$500-2,000/reporte   | Media            |
| Publicidad contextual (marcas)     | US$5-15 CPM            | Baja             |

---

## 7. Conclusión

OKLA tiene **la mejor posición tecnológica** del mercado dominicano automotriz digital. La brecha principal es el **financiamiento integrado** — feature que SuperCarros usa como su ventaja competitiva #1 y que OKLA debe implementar en Q2 2026 para eliminar la única ventaja funcional del incumbente.

El mercado está en un punto de inflexión: los incumbentes operan con tecnología de hace 10-15 años, no hay competidores globales (Kavak no está en RD), y los compradores dominicanos están adoptando rápidamente herramientas digitales. **El window of opportunity es ahora.**

---

_Próximo análisis programado: Q2 2026 (junio)_
