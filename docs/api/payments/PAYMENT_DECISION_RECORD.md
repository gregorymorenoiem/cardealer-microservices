# 📊 PAYMENT GATEWAY DECISION RECORD

**Decisión:** Implementar AZUL + STRIPE en paralelo para OKLA  
**Fecha:** Enero 14, 2026  
**Status:** ✅ APROBADO PARA IMPLEMENTACIÓN  
**Sprint:** Actual (MVP Payment)

---

## 🎯 Problema

OKLA necesita procesar pagos de:

1. **Compradores locales** (Dominicana) - AZUL es mejor
2. **Compradores internacionales** - STRIPE es mejor
3. **Dealers con suscripciones** (RD y global) - Ambas

**Decisión anterior:** ❌ Solo STRIPE (incompleto para mercado local)  
**Nueva decisión:** ✅ AZUL + STRIPE (cobertura 100%)

---

## ✅ Opción Seleccionada: Hybrid Approach

### Por qué AMBAS:

```
┌─────────────────────────────────────────────────────────────┐
│                    COBERTURA DE PAGOS                        │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  USUARIOS DOMINICANOS (70% mercado inicial)                │
│  ├─ Método preferido: AZUL                                  │
│  │  ├─ Móvil (Orange Money, Claro Money)                   │
│  │  ├─ Tarjeta crédito/débito RD                           │
│  │  ├─ ACH local (transfer bancaria)                       │
│  │  └─ Comisión baja: 2.5%                                 │
│  │                                                          │
│  └─ Fallback: STRIPE (Apple/Google Pay)                    │
│                                                              │
│  USUARIOS INTERNACIONALES (30% mercado futura)            │
│  ├─ Método ÚNICO: STRIPE                                   │
│  │  ├─ Cobertura: 190+ países                             │
│  │  ├─ Métodos: Tarjeta, Apple Pay, Google Pay, etc.      │
│  │  ├─ SEPA, iDEAL, Giropay, WeChat, Alipay              │
│  │  └─ Comisión: 2.9% + $0.30 (tarjeta)                  │
│  │                                                          │
│  └─ Fallback: AZUL (si tiene tarjeta dominicana)          │
│                                                              │
│  DEALERS DOMINICANOS (Suscripción)                        │
│  ├─ Principal: AZUL                                        │
│  │  └─ Comisión mensual baja (2.5%)                       │
│  │                                                          │
│  └─ Fallback: STRIPE (si paga en USD)                     │
│                                                              │
│  DEALERS INTERNACIONALES (Suscripción)                   │
│  ├─ Principal: STRIPE                                      │
│  │  └─ Multi-currency, multi-país                        │
│  │                                                          │
│  └─ Fallback: AZUL (si paga en DOP/RD)                    │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 💡 Ventajas de esta Decisión

### Para OKLA (Negocio)

| Ventaja               | Impacto                              | ROI                              |
| --------------------- | ------------------------------------ | -------------------------------- |
| **Máxima conversión** | Cada usuario usa su método preferido | +15-20% conversión esperada      |
| **Menor churn**       | No "rechazar pago" por método        | -5% lost transactions            |
| **Competitividad**    | Parecer global desde día 1           | Diferencial frente a competencia |
| **Negotiating power** | Presión en comisiones con ambas      | -0.5% en comisiones potencial    |
| **Risk distribution** | No depender de un provider           | Mitiga riesgo de downtime        |

### Para Usuarios

| Segmento             | Ventaja                      | UX                      |
| -------------------- | ---------------------------- | ----------------------- |
| **Dominicanos**      | Pagan con Móvil (no tarjeta) | ⭐⭐⭐⭐⭐ Seamless     |
| **Internacionales**  | Pagan con Apple/Google Pay   | ⭐⭐⭐⭐⭐ Super rápido |
| **Dealers locales**  | Menores comisiones           | ⭐⭐⭐⭐ Mejor margen   |
| **Dealers globales** | Multi-currency nativo        | ⭐⭐⭐⭐⭐ Sin fricción |

### Para Equipo Dev

| Ventaja                    | Descripción                    |
| -------------------------- | ------------------------------ |
| **Documentación completa** | ✅ Ya creada (620+750 líneas)  |
| **Arquitectura clara**     | ✅ Clean Architecture lista    |
| **Ejemplos de código**     | ✅ C# samples en documentación |
| **Testing patterns**       | ✅ Patrones claros para ambas  |
| **Paralelizable**          | ✅ Ambas sin dependencias      |

---

## 📋 Opciones Rechazadas

### ❌ Opción 1: SOLO AZUL

```
Problema: Usuarios internacionales NO pueden pagar
- Sin Apple Pay
- Sin Google Pay
- Sin métodos internacionales (SEPA, iDEAL, etc.)
- Limitado a mercado RD
Resultado: Pérdida de 20-30% potencial revenue
```

### ❌ Opción 2: SOLO STRIPE

```
Problema: Usuarios dominicanos pagan comisión extra
- No soporta Móvil Money (Orange, Claro)
- Comisión más alta: 3.2% vs 2.5% AZUL
- Peor experiencia para usuario local
- No aprovecha infraestructura local
Impacto: 10-15% más caro para usuario dominicano
```

### ❌ Opción 3: Implementar solo AZUL ahora, STRIPE después

```
Problema: Demora en go-to-market
- Perder oportunidad de usuarios internacionales
- Dealers globales ven como "local only"
- Competidores ya tienen STRIPE
Riesgo: Pérdida de primeros clientes premium
```

---

## 🚀 Estrategia de Implementación

### Timeline: 4 Semanas (MVP Payment)

```
Week 1: ✅ Scaffolding + Documentación (DONE)
Week 2: 🔄 Controllers + Tests (THIS WEEK)
Week 3: ⏳ Integration + Docker testing
Week 4: ⏳ Production deployment + E2E
```

### Arquitectura: Hybrid Processor

```csharp
// El BillingService (o nuevo PaymentService) elige proveedor
public class PaymentProcessor
{
    public async Task<PaymentResult> ProcessAsync(
        Order order,
        PaymentMethod method)
    {
        // 1. Detectar país
        var country = order.Buyer.Country;

        // 2. Detectar método
        var paymentType = method.Type;

        // 3. Elegir procesador óptimo
        if (IsLocalPaymentOptimal(country, paymentType))
        {
            return await _azulService.ChargeAsync(order);
        }
        else if (IsInternationalPaymentOptimal(country, paymentType))
        {
            return await _stripeService.ChargeAsync(order);
        }

        // 4. Fallback logic
        return await FallbackPaymentAsync(order);
    }
}
```

---

## 🔄 Flujo de Implementación

### Fase 1: Controllers Básicos (Week 2)

**AzulPaymentService:**

- POST `/api/charge` - Cobro simple
- POST `/api/refund` - Reembolso
- POST `/api/subscriptions` - Suscripcción

**StripePaymentService:**

- POST `/api/payment_intents` - Create Intent
- POST `/api/payment_intents/{id}/confirm` - Confirmar
- POST `/api/subscriptions` - Suscripción

### Fase 2: Webhooks + Tests (Week 3)

- Webhook handlers para ambas
- Unit tests (15+ AZUL, 20+ STRIPE)
- Integration tests
- Docker validation

### Fase 3: Production Ready (Week 4)

- Deployment a DOKS
- Monitoring
- Alertas
- Runbooks

---

## 📊 Comparación de Proveedores Finales

| Aspecto                  | AZUL       | STRIPE        | Winner |
| ------------------------ | ---------- | ------------- | ------ |
| **Cobertura local (RD)** | ⭐⭐⭐⭐⭐ | ⭐⭐          | AZUL   |
| **Cobertura global**     | ⭐         | ⭐⭐⭐⭐⭐    | STRIPE |
| **Métodos de pago**      | 5          | 15+           | STRIPE |
| **Comisión (tarjeta)**   | 2.5%       | 2.9%+$0.30    | AZUL   |
| **Velocidad deposito**   | 24-48h     | 1-2 días      | AZUL   |
| **Apple/Google Pay**     | ❌         | ✅            | STRIPE |
| **Antifraud**            | Básico     | Avanzado (ML) | STRIPE |
| **Facturación**          | Básica     | Completa      | STRIPE |
| **3D Secure 2.0**        | ⚠️         | ✅            | STRIPE |
| **Documentación**        | Buena      | Excelente     | STRIPE |

---

## 🎓 Decisión de Arquitectura

### Single vs Multi-Gateway

**Seleccionado: Multi-Gateway con Smart Routing**

```
                     ┌──────────────────┐
                     │  BillingService  │
                     └────────┬─────────┘
                              │
                    ┌─────────┴──────────┐
                    │ Smart Processor    │
                    │ (Routing Logic)    │
                    └─────────┬──────────┘
                              │
                ┌─────────────┼──────────────┐
                │             │              │
         ┌──────▼─────┐ ┌─────▼────┐   ┌────▼──────┐
         │   AZUL     │ │ STRIPE   │   │ PayPal    │
         │ (Futuro)   │ │ (MVP)    │   │ (Futuro)  │
         └────────────┘ └──────────┘   └───────────┘
```

**Beneficios:**

- ✅ Cada gateway = responsabilidad única
- ✅ Fácil de agregar nuevas pasarelas
- ✅ Testing independiente por gateway
- ✅ Failover automático

---

## 💰 Impacto Financiero (Año 1)

### Hipótesis de Números

```
Transacciones mensuales esperadas (Año 1):
- Mes 1-3: 500 txn ($150K volumen)
- Mes 4-6: 1,000 txn ($300K volumen)
- Mes 7-12: 3,000 txn ($900K volumen)

Promedio anual: ~1,500 txn/mes = $450K/mes
```

### Comisiones Comparadas

**Escenario SOLO STRIPE:**

```
Volumen: $450K/mes
Comisión promedio: 3.2% (tarjeta + international)
Costo anual: $172,800
```

**Escenario AZUL + STRIPE (SELECCIONADO):**

```
Volumen: $450K/mes
- 60% local (AZUL): $270K × 2.5% = $6,750/mes
- 40% intl (STRIPE): $180K × 3.2% = $5,760/mes
Total mensual: $12,510
Costo anual: $150,120

AHORRO ANUAL: $22,680 (13% de descuento)
```

---

## ⚠️ Riesgos Mitigados

| Riesgo                  | Probabilidad | Impacto             | Mitigación               |
| ----------------------- | ------------ | ------------------- | ------------------------ |
| **Downtime AZUL**       | Baja         | Alto (sin pagos RD) | Fallback a STRIPE        |
| **Cambios API STRIPE**  | Muy baja     | Medio               | Monitoreo de updates     |
| **Fraude**              | Baja-Media   | Muy alto            | Usar antifraud de STRIPE |
| **Compliance regional** | Baja         | Alto                | AZUL expertise local     |
| **Costo impredecible**  | Baja         | Medio               | Fijo en sandboxes        |

---

## ✅ Criterios de Éxito

### MVP Payment (Semana 4)

- [ ] Ambos servicios compilados sin errores
- [ ] 90% de endpoints implementados
- [ ] Tests en sandbox pasando
- [ ] Webhooks funcionales
- [ ] Deployed a DOKS
- [ ] Health checks respondiendo

### Production (Semana 8)

- [ ] Live credentials en ambiente prod
- [ ] 100% de endpoints testeados
- [ ] Monitoring activo (Prometheus + Grafana)
- [ ] Alertas configuradas
- [ ] Runbooks documentados
- [ ] <1% error rate
- [ ] Transacciones procesándose diariamente

---

## 📝 Aprobaciones Requeridas

| Rol                  | Aprobación           | Status     |
| -------------------- | -------------------- | ---------- |
| **Engineering Lead** | Arquitectura válida  | ✅ Ready   |
| **Product Manager**  | Alineado con roadmap | ✅ Ready   |
| **CFO**              | ROI aceptable        | ✅ Ready   |
| **Security**         | PCI-DSS compliance   | ⏳ Pending |

---

## 🚀 Siguiente Paso

**Start Phase 2: Controllers Implementation**

```bash
# AZUL Implementation
backend/AzulPaymentService/
├── Domain/Entities/         ← Create transaction entities
├── Application/DTOs/        ← Create request/response DTOs
├── Infrastructure/          ← Create AzulHttpClient
└── Api/Controllers/         ← Create PaymentsController

# STRIPE Implementation
backend/StripePaymentService/
├── Domain/Entities/         ← Create PI entities
├── Application/DTOs/        ← Create request/response DTOs
├── Infrastructure/          ← Create StripeClientService
└── Api/Controllers/         ← Create 4 controllers
```

**Estimado:** 40 SP (AzulPaymentService) + 50 SP (StripePaymentService) = 90 SP

---

## 📚 Documentación Asociada

1. ✅ [AZUL_API_DOCUMENTATION.md](AZUL_API_DOCUMENTATION.md)
2. ✅ [STRIPE_API_DOCUMENTATION.md](STRIPE_API_DOCUMENTATION.md)
3. ✅ [AZUL_vs_STRIPE_COMPARISON.md](AZUL_vs_STRIPE_COMPARISON.md)
4. ✅ [PAYMENT_SERVICES_IMPLEMENTATION_ROADMAP.md](PAYMENT_SERVICES_IMPLEMENTATION_ROADMAP.md)

---

## 🏆 Conclusión

**DECISIÓN FINAL:** ✅ Implementar **AZUL + STRIPE en paralelo**

Esta decisión:

- ✅ Maximiza conversión de usuarios
- ✅ Soporta mercado local + global
- ✅ Ahorra $22K anuales en comisiones
- ✅ Diferencia competitiva clara
- ✅ Prepara escalabilidad futura

**Riesgo:** BAJO (ambas APIs maduras, documentadas, con ejemplos)  
**Complejidad:** MEDIA (arquitectura clara, sin interdependencias)  
**Value:** ALTO (crítico para MVP viabilidad)

---

_Decision Record creado: Enero 14, 2026_  
_Aprobado para: MVP Payment Implementation_  
_Timeline: 4 semanas_
