# 🗓️ Roadmap - AZUL Payment API

**API:** AZUL (Banco Popular RD)  
**Proveedor:** Banco Popular Dominicano  
**Estado actual:** ✅ En Producción  
**Versión:** 2.0

---

## 📅 Timeline General

| Fase       | Periodo | Estado         | Descripción           |
| ---------- | ------- | -------------- | --------------------- |
| **Fase 1** | Q4 2025 | ✅ Completado  | Research & Setup      |
| **Fase 2** | Q1 2026 | ✅ Completado  | Pagos básicos         |
| **Fase 3** | Q1 2026 | 🚧 En Progreso | Optimización          |
| **Fase 4** | Q2 2026 | 📝 Planificado | Advanced features     |
| **Fase 5** | Q3 2026 | 📝 Planificado | Subscription handling |

---

## ✅ Fase 1: Research & Setup (Q4 2025) - COMPLETADO

### Objetivos

- Investigar API de AZUL
- Obtener credenciales sandbox
- Setup inicial

### Entregables Completados

#### 1.1 Research ✅

- [x] Documentación oficial de AZUL
- [x] Análisis de costos y comisiones
- [x] Comparación con Stripe
- [x] Decisión: Usar AZUL para tarjetas dominicanas

#### 1.2 Credenciales ✅

- [x] Registro en portal de desarrolladores
- [x] Obtener StoreId y ApiKey (sandbox)
- [x] Configurar certificados SSL
- [x] Whitelisting de IPs

#### 1.3 Configuración Inicial ✅

- [x] Crear AzulPaymentService en backend
- [x] Instalar Newtonsoft.Json para JSON
- [x] Configurar appsettings con credenciales
- [x] Setup de logging específico

**Sprint:** Sprint 4 - Research AZUL  
**Fecha de completado:** Diciembre 2025

---

## ✅ Fase 2: Pagos Básicos (Q1 2026) - COMPLETADO

### Objetivos

- Implementar flujo completo de pagos
- Procesar pagos con tarjetas locales
- Testing exhaustivo

### Entregables Completados

#### 2.1 Sale Transaction ✅

- [x] Endpoint POST /api/azul/sale
- [x] Crear DataVault (tokenización)
- [x] Procesar venta con DataVault
- [x] Manejo de errores y retry logic
- [x] Logging de transacciones

#### 2.2 Refund Transaction ✅

- [x] Endpoint POST /api/azul/refund
- [x] Validar transaction original
- [x] Partial refunds
- [x] Full refunds
- [x] Update estado en DB

#### 2.3 Transaction Verification ✅

- [x] Endpoint GET /api/azul/verify/{azulTxId}
- [x] Consultar estado en AZUL
- [x] Reconciliación automática
- [x] Manejo de discrepancias

#### 2.4 Testing ✅

- [x] Unit tests (xUnit)
- [x] Integration tests
- [x] Sandbox testing con tarjetas de prueba
- [x] Load testing (100 tx/seg)

**Sprint:** Sprint 4 - AZUL Integration  
**Fecha de completado:** Enero 10, 2026

---

## 🚧 Fase 3: Optimización (Q1 2026) - EN PROGRESO

### Objetivos

- Mejorar tasas de éxito
- Reducir latencia
- Manejo robusto de errores

### Entregables

#### 3.1 Retry Logic Inteligente 🚧

- [x] Implementar Polly para retry
- [x] Backoff exponencial
- [ ] Circuit breaker para evitar cascading failures
- [ ] Fallback a Stripe si AZUL falla

#### 3.2 Idempotencia 🚧

- [x] Guardar IdempotencyKey en Redis
- [x] Detectar duplicados
- [x] Retornar respuesta cacheada
- [ ] TTL configurable (24h)

#### 3.3 Validaciones Pre-pago 🚧

- [ ] Validar tarjeta antes de procesar
- [ ] BIN validation (primeros 6 dígitos)
- [ ] Luhn algorithm para checksum
- [ ] Blacklist de tarjetas

#### 3.4 Performance 🚧

- [ ] Connection pooling con HttpClient
- [ ] Reducir timeout de 30s a 15s
- [ ] Async/await en todos los endpoints
- [ ] Cachear resultados de verificación (5 min)

**Sprint:** Sprint 18 - AZUL Optimization  
**Fecha estimada:** Febrero 2026

---

## 📝 Fase 4: Advanced Features (Q2 2026) - PLANIFICADO

### Objetivos

- Features avanzados de AZUL
- Mejorar experiencia de usuario
- Seguridad adicional

### Entregables

#### 4.1 Webhooks de AZUL 📝

- [ ] Configurar webhook endpoint
- [ ] Recibir notificaciones de transacciones
- [ ] Validar firma de webhook
- [ ] Actualizar estado en tiempo real

#### 4.2 3D Secure 📝

- [ ] Implementar 3DS 2.0
- [ ] Challenge flow para high-risk transactions
- [ ] Frictionless flow para low-risk
- [ ] Fallback a 3DS 1.0 si necesario

#### 4.3 Tokenización Mejorada 📝

- [ ] Guardar DataVault tokens permanentemente
- [ ] Permitir "Save card for future"
- [ ] One-click payments para returning users
- [ ] PCI compliance audit

#### 4.4 Reporting & Reconciliation 📝

- [ ] Dashboard de transacciones AZUL
- [ ] Exportar reportes CSV/Excel
- [ ] Reconciliación automática diaria
- [ ] Alertas de discrepancias

**Sprint:** Sprints 22-23  
**Fecha estimada:** Abril-Mayo 2026

---

## 📝 Fase 5: Subscriptions (Q3 2026) - PLANIFICADO

### Objetivos

- Suscripciones recurrentes con AZUL
- Alternativa a Stripe para dealers locales
- Cobros automáticos mensuales

### Entregables

#### 5.1 Recurring Payments 📝

- [ ] Guardar DataVault token a largo plazo
- [ ] Scheduler para cobros mensuales
- [ ] Manejo de pagos fallidos
- [ ] Retry automático (3 intentos)

#### 5.2 Subscription Management 📝

- [ ] Crear subscription
- [ ] Update payment method
- [ ] Cancel subscription
- [ ] Pause/Resume subscription

#### 5.3 Invoice Generation 📝

- [ ] Generar invoice pre-cobro
- [ ] Enviar invoice por email
- [ ] Notificar pago exitoso
- [ ] Notificar pago fallido

#### 5.4 Compliance 📝

- [ ] Notificar dealers 5 días antes de cobro
- [ ] Opción de cancelar antes de próximo cobro
- [ ] Terms & Conditions específicos para AZUL
- [ ] Audit trail completo

**Sprint:** Sprints 28-29  
**Fecha estimada:** Julio-Agosto 2026

---

## 🎯 Métricas de Éxito

### KPIs por Fase

| Fase       | KPI                    | Target | Actual  |
| ---------- | ---------------------- | ------ | ------- |
| **Fase 2** | Tasa de éxito de pagos | >90%   | 94% ✅  |
| **Fase 2** | Latencia promedio      | <3s    | 2.8s ✅ |
| **Fase 3** | Reducción de errores   | -30%   | -15% 🚧 |
| **Fase 4** | 3DS adoption           | >80%   | -       |
| **Fase 5** | Subscriptions activas  | 100+   | -       |

---

## 📊 Comparación AZUL vs Stripe

| Aspecto                      | AZUL         | Stripe            |
| ---------------------------- | ------------ | ----------------- |
| **Comisión**                 | ~2.5%        | ~3.5%             |
| **Depósito**                 | 24-48h       | 7 días            |
| **Tarjetas locales**         | ✅ Excelente | ⚠️ Algunas fallan |
| **Tarjetas internacionales** | ❌ No        | ✅ Sí             |
| **Suscripciones**            | ⚠️ Manual    | ✅ Nativo         |
| **Dashboard**                | ⚠️ Básico    | ✅ Avanzado       |
| **API Quality**              | ⚠️ Medio     | ✅ Excelente      |
| **Soporte**                  | 🇩🇴 Local     | 🌍 Internacional  |

**Estrategia:** Usar AZUL como gateway principal para RD, Stripe como backup.

---

## 🚀 Próximos Pasos (Enero 2026)

### Inmediato (Sprint 18)

1. ✅ Completar retry logic con Polly
2. 🚧 Implementar circuit breaker
3. 🚧 BIN validation
4. 🚧 Testing de fallback a Stripe

### Corto Plazo (Febrero-Marzo 2026)

1. Configurar webhooks de AZUL
2. Implementar 3D Secure
3. Mejorar tokenización
4. Dashboard de transacciones

### Mediano Plazo (Q2 2026)

1. Recurring payments para subscriptions
2. Invoice generation
3. Reconciliación automática
4. Advanced reporting

---

## 📚 Referencias Técnicas

### Documentación AZUL

- [Portal Desarrolladores](https://desarrolladores.azul.com.do)
- [API Reference](https://desarrolladores.azul.com.do/docs/api)
- [Sandbox Testing](https://desarrolladores.azul.com.do/sandbox)
- [Error Codes](https://desarrolladores.azul.com.do/docs/errors)

### Implementación OKLA

- [AZUL_API_DOCUMENTATION.md](AZUL_API_DOCUMENTATION.md)
- [AzulPaymentService README](../../../backend/AzulPaymentService/README.md)
- Sprint 4 Research: [SPRINT_4_AZUL_INTEGRATION_RESEARCH.md](../../SPRINT_4_AZUL_INTEGRATION_RESEARCH.md)
- Sprint 4 Completed: [SPRINT_4_COMPLETED.md](../../SPRINT_4_COMPLETED.md)

### Compliance

- [PCI DSS Requirements](https://www.pcisecuritystandards.org/)
- [Ley 172-13 RD - Protección de Datos](https://indotel.gob.do)

---

## ⚠️ Riesgos y Mitigación

| Riesgo                    | Probabilidad | Impacto | Mitigación                      |
| ------------------------- | ------------ | ------- | ------------------------------- |
| **Downtime de AZUL**      | Media        | Alto    | Fallback automático a Stripe    |
| **Rate limiting**         | Baja         | Medio   | Throttling, queue de pagos      |
| **Fraude con tarjetas**   | Media        | Alto    | BIN validation, 3DS obligatorio |
| **API changes sin aviso** | Alta         | Medio   | Version pinning, alertas        |
| **Reconciliación manual** | Alta         | Bajo    | Automatizar con webhooks        |

---

## 💡 Ideas Futuras (Backlog)

- [ ] **AZUL Cash** - Pagos en efectivo en puntos autorizados
- [ ] **Transferencia bancaria** directa (ACH local)
- [ ] **Pagos QR** para dealers físicos
- [ ] **Installments** (cuotas) con tarjetas locales
- [ ] **Cashback** en tarjetas participantes
- [ ] **Analytics de decline reasons**
- [ ] **Smart routing** (AZUL vs Stripe basado en BIN)

---

## 📞 Contacto AZUL

- **Email:** soporte-desarrolladores@azul.com.do
- **Teléfono:** +1 (809) 123-4567
- **Horario:** Lun-Vie 9am-6pm AST
- **SLA:** 48h para issues críticos

---

**Última actualización:** Enero 15, 2026  
**Próxima revisión:** Marzo 1, 2026  
**Responsable:** Equipo de Payments + Billing
