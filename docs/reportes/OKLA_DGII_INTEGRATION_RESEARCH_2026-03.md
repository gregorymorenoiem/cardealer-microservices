# 🏛️ OKLA — Investigación: Integración DGII y Servicios Gubernamentales RD

**Fecha:** 2026-03-06
**Autor:** CPSO (Copilot)
**Clasificación:** Investigación Estratégica — Diferenciación Competitiva

---

## 1. Resumen Ejecutivo

La integración con servicios de la DGII (Dirección General de Impuestos Internos) y otras entidades gubernamentales de la República Dominicana representa la **oportunidad de diferenciación más significativa** de OKLA frente a Facebook Marketplace y SuperCarros. Ningún competidor local ofrece verificación vehicular integrada, cálculo automático de impuestos, ni historial oficial de propiedad.

---

## 2. Servicios DGII Relevantes para OKLA

### 2.1 Consulta de Vehículos por Placa/Chasis

- **URL:** dgii.gov.do → Herramientas → Consultas de Vehículos
- **Datos disponibles:** Marca, modelo, año, color, tipo de combustible, cilindraje, estado de la placa, propietario (parcial)
- **Integración OKLA:** Validar que el vehículo listado coincida con los datos oficiales. Previene fraude de vehículos con datos falsos.
- **UX:** Badge "✅ Verificado DGII" en listings verificados.

### 2.2 Impuesto de Primera Placa

- **Aplica a:** Vehículos importados que se registran por primera vez en RD
- **Base imponible:** Valor CIF × tasa (17% vehículos nuevos, escalada para usados según año)
- **Integración OKLA:** Calculadora automática para compradores — "¿Cuánto me costará matricular este vehículo?"
- **Impacto:** Reduce fricción en la decisión de compra, especialmente para vehículos importados

### 2.3 Impuesto de Transferencia Vehicular

- **Aplica a:** Toda compraventa de vehículos usados
- **Tasa:** 2% del valor del vehículo según tasación DGII (mínimo RD$500)
- **Integración OKLA:** Calcular automáticamente el costo de transferencia basado en el precio del listing
- **UX:** "Costo total estimado" que incluye precio + impuesto de transferencia + gastos notariales

### 2.4 Marbete (Renovación de Placa Anual)

- **Estado:** Si el marbete está vigente, el vehículo está al día con impuestos
- **Integración OKLA:** Verificar si el vehículo tiene marbete vigente antes de la compra
- **Señal de confianza:** Marbete vencido = señal de alerta para compradores

### 2.5 Validación de Cédula/RNC

- **Para vendedores individuales:** Validar cédula del propietario
- **Para dealers:** Validar RNC (Registro Nacional del Contribuyente)
- **Integración OKLA:** Refuerza el KYC existente con verificación gubernamental

---

## 3. Otras Entidades Gubernamentales Relevantes

### 3.1 INTRANT (Instituto Nacional de Tránsito y Transporte Terrestre)

- **Servicios:** Licencias de conducir, infracciones de tránsito, historial del conductor
- **Integración OKLA:** Verificar que el vendedor tenga licencia vigente, historial de infracciones del vehículo

### 3.2 Dirección General de Aduanas (DGA)

- **Servicios:** Verificación de importación, aranceles pagados, estado de despacho
- **Integración OKLA:** Para vehículos importados, verificar que los aranceles estén pagados. Previene compra de vehículos con deuda aduanera.

### 3.3 TSS (Tesorería de la Seguridad Social)

- **Relevancia:** Verificación de empleo/ingresos para financiamiento integrado
- **Integración OKLA:** Pre-calificación de financiamiento basada en datos oficiales

### 3.4 JCE (Junta Central Electoral)

- **Servicios:** Validación de cédula de identidad y electoral
- **Integración OKLA:** Verificación de identidad del comprador/vendedor

---

## 4. Métodos de Integración Técnica

### 4.1 Web Scraping (Corto plazo — no recomendado para producción)

- La DGII no ofrece APIs públicas documentadas
- Web scraping es frágil y puede violar términos de servicio
- Solo para prototipado y validación de concepto

### 4.2 Convenio Institucional (Mediano plazo — RECOMENDADO)

- Establecer convenio formal con la DGII para acceso a datos
- Modelo: Similar a como bancos y aseguradoras acceden a datos DGII
- Requisitos: Carta de intención, demostración de seguridad de datos, cumplimiento con Ley 172-13 (Protección de datos personales)
- Timeline: 3-6 meses de negociación

### 4.3 Intermediarios Autorizados (Corto-mediano plazo)

- Empresas como **CEVALDOM**, **DataCrédito**, **TransUnion RD** tienen acceso a datos gubernamentales
- Partnership con un proveedor de datos podría acelerar la integración
- Costo: Fee por consulta (estimado RD$15-50 por consulta)

### 4.4 API de Servicios Digitales del Estado (Largo plazo)

- El gobierno RD avanza hacia servicios digitales abiertos (Portal GOB.DO)
- La OGTIC (Oficina Gubernamental de Tecnologías de la Información) impulsa interoperabilidad
- OKLA podría posicionarse como early adopter cuando las APIs estén disponibles

---

## 5. Modelo de Implementación Técnica

### 5.1 Nuevo Microservicio: GovernmentIntegrationService

```
backend/GovernmentIntegrationService/
├── GovernmentIntegrationService.Api/
│   ├── Controllers/
│   │   ├── VehicleVerificationController.cs
│   │   ├── TaxCalculatorController.cs
│   │   └── IdentityVerificationController.cs
│   └── Program.cs
├── GovernmentIntegrationService.Application/
│   ├── Features/
│   │   ├── VehicleVerification/
│   │   │   ├── Queries/VerifyVehicleByPlateQuery.cs
│   │   │   └── DTOs/VehicleVerificationResult.cs
│   │   ├── TaxCalculation/
│   │   │   ├── Queries/CalculateTransferTaxQuery.cs
│   │   │   └── DTOs/TaxCalculationResult.cs
│   │   └── IdentityVerification/
│   │       ├── Queries/VerifyCedulaQuery.cs
│   │       └── DTOs/IdentityVerificationResult.cs
│   └── Interfaces/
│       ├── IDgiiClient.cs
│       ├── IIntrantClient.cs
│       └── ICachingLayer.cs
├── GovernmentIntegrationService.Domain/
│   ├── Entities/
│   │   ├── VehicleRecord.cs
│   │   └── VerificationResult.cs
│   └── Enums/
│       ├── VerificationStatus.cs
│       └── TaxType.cs
└── GovernmentIntegrationService.Infrastructure/
    ├── Clients/
    │   ├── DgiiHttpClient.cs
    │   ├── IntrantHttpClient.cs
    │   └── MockGovernmentClient.cs (para desarrollo)
    ├── Caching/
    │   └── RedisVerificationCache.cs
    └── Persistence/
        └── VerificationDbContext.cs
```

### 5.2 Caching Strategy

- Datos DGII cambian infrecuentemente → Cache agresivo
- TTL de verificación de placa: 24 horas
- TTL de cálculo de impuestos: 7 días (tasas cambian anualmente)
- TTL de verificación de cédula: 30 días
- Invalidación manual disponible para el admin

### 5.3 Rate Limiting

- Consultas gubernamentales deben rate-limitearse para no abrumar servicios externos
- Máximo 10 consultas/minuto a DGII
- Cola de consultas con prioridad (verificación de compra > verificación de listing)

---

## 6. Features para Usuarios

### 6.1 Para Compradores

1. **Verificación de Vehículo** — "Este vehículo está verificado con datos oficiales de la DGII"
2. **Calculadora de Costos Totales** — Precio + transferencia + notario + marbete
3. **Historial de Propiedad** — Cuántos dueños anteriores ha tenido
4. **Alerta de Deuda** — Si el vehículo tiene deudas pendientes (marbete, multas)
5. **Pre-aprobación de Financiamiento** — Basada en verificación de identidad e ingresos

### 6.2 Para Vendedores

1. **Listing Verificado** — Badge de confianza que aumenta conversión
2. **Precio Sugerido Basado en Datos DGII** — Tasación oficial como referencia
3. **Trámite Express** — Asistencia con documentación de transferencia
4. **Calculadora de Ganancia Neta** — Precio de venta - impuestos - comisiones

### 6.3 Para Dealers

1. **Verificación Masiva** — API para verificar inventario completo
2. **Alertas de Cambio de Estado** — Notificación cuando un vehículo cambia de estado en DGII
3. **Reportes de Mercado con Datos Oficiales** — Pricing basado en tasaciones DGII
4. **Facturación Fiscal** — Integración con e-CF (Comprobante Fiscal Electrónico)

---

## 7. Análisis Competitivo

| Feature                   | OKLA (Propuesto) | SuperCarros | FB Marketplace |
| ------------------------- | ---------------- | ----------- | -------------- |
| Verificación DGII         | ✅ Integrada     | ❌          | ❌             |
| Calculadora de impuestos  | ✅ Automática    | ❌          | ❌             |
| Historial de propiedad    | ✅               | ❌          | ❌             |
| KYC verificado            | ✅               | Parcial     | ❌             |
| e-CF integrado            | ✅ (Propuesto)   | ❌          | ❌             |
| Pre-aprobación financiera | ✅ (Propuesto)   | ❌          | ❌             |

**Ventaja competitiva:** Ningún marketplace dominicano ofrece estas funcionalidades. OKLA sería el primero en integrar datos gubernamentales, creando una barrera de entrada significativa.

---

## 8. Modelo de Monetización

### 8.1 Verificación Premium (Por vehículo)

- **Verificación Básica** (gratis): Marca, modelo, año vs DGII
- **Verificación Completa** (RD$299): Historial de propiedad, deudas, multas, estado de marbete
- **Reporte Carfax-style** (RD$599): Todo lo anterior + tasación oficial + score de confianza

### 8.2 Para Dealers (Suscripción)

- **Plan Pro** (+RD$2,000/mes): 50 verificaciones/mes incluidas
- **Plan Enterprise** (+RD$5,000/mes): Verificaciones ilimitadas + API access

### 8.3 Servicios de Trámite (Comisión)

- **Transferencia Express** (RD$3,500): OKLA gestiona toda la documentación
- **Primera Placa** (RD$5,000): Para vehículos importados
- **Marbete Express** (RD$1,500): Renovación sin filas

### 8.4 Proyección de Revenue

| Mes | Verificaciones | Revenue Verificación | Trámites | Revenue Trámites | Total        |
| --- | -------------- | -------------------- | -------- | ---------------- | ------------ |
| 1   | 200            | RD$59,800            | 20       | RD$70,000        | RD$129,800   |
| 6   | 1,500          | RD$448,500           | 150      | RD$525,000       | RD$973,500   |
| 12  | 5,000          | RD$1,495,000         | 500      | RD$1,750,000     | RD$3,245,000 |

---

## 9. Cumplimiento Legal

### 9.1 Ley 172-13 (Protección de Datos Personales)

- Requiere consentimiento del titular para procesar datos personales
- OKLA necesita: política de privacidad actualizada, consentimiento explícito en formularios
- Los datos DGII son públicos en cuanto al vehículo, privados en cuanto al propietario

### 9.2 Norma General 06-2018 (Comprobantes Fiscales Electrónicos)

- Dealers deben emitir e-CF para ventas de vehículos
- OKLA puede facilitar la emisión como intermediario autorizado
- Requiere certificación como Proveedor de Servicios e-CF

### 9.3 Resolución 07-2007 (Transferencia de Vehículos)

- Documenta el proceso oficial de transferencia ante la DGII
- OKLA puede automatizar la generación de formularios requeridos (Formulario IR-2)

---

## 10. Roadmap de Implementación

### Fase 1: MVP (Sprint 20-22, ~6 semanas)

- Calculadora de impuestos (datos estáticos, fórmulas DGII públicas)
- Badge "Verificado" manual (admin verifica y marca)
- Landing page educativa sobre proceso de transferencia
- **Inversión:** 0 — sin integración externa

### Fase 2: Integración Parcial (Sprint 23-28, ~12 semanas)

- Convenio con proveedor de datos (DataCrédito o similar)
- Verificación automática por placa
- Historial básico de propiedad
- GovernmentIntegrationService microservice
- **Inversión:** ~RD$50,000/mes en fees de datos

### Fase 3: Integración Completa (Sprint 29-36, ~16 semanas)

- Convenio directo con DGII
- Trámites Express (transferencia, primera placa)
- e-CF integrado para dealers
- Pre-aprobación de financiamiento
- **Inversión:** ~RD$150,000/mes en fees + personal de tramitación

---

## 11. Riesgos y Mitigaciones

| Riesgo                               | Probabilidad | Impacto | Mitigación                                       |
| ------------------------------------ | ------------ | ------- | ------------------------------------------------ |
| DGII no otorga convenio              | Media        | Alto    | Usar intermediarios autorizados como alternativa |
| Cambio de tasas impositivas          | Alta         | Bajo    | Actualización periódica de tablas + alerts       |
| Datos DGII incorrectos               | Baja         | Alto    | Disclaimer legal + proceso de disputa            |
| Competidores copian la funcionalidad | Media        | Medio   | Primero al mercado + convenios exclusivos        |
| Regulación nueva restringe acceso    | Baja         | Alto    | Lobby con asociaciones de dealers, ACOFAVE       |
