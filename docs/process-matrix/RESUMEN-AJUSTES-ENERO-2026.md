# 📋 RESUMEN DE AJUSTES - MATRIZ DE PROCESOS OKLA

> **Fecha:** Enero 25, 2026  
> **Propósito:** Documentar todos los ajustes realizados a la matriz de procesos para alinear la documentación con el modelo de negocio real de OKLA S.R.L.

---

## 🎯 CONTEXTO DEL AJUSTE

### Problema Identificado

La documentación original asumía incorrectamente que OKLA era:

- ❌ Un marketplace transaccional que procesaba pagos de vehículos
- ❌ Un intermediario financiero sujeto a regulación UAF
- ❌ Un cobrador de comisiones por ventas de vehículos

### Modelo de Negocio Real de OKLA

OKLA S.R.L. (RNC: 1-33-32590-1) es una **plataforma de anuncios clasificados**, similar a SuperCarros.com:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                   MODELO DE NEGOCIO CORRECTO                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   OKLA COBRA POR:                     OKLA NO PARTICIPA EN:             │
│   ═══════════════                     ═════════════════════             │
│   ✅ Publicación anuncios ($29)       ❌ Venta de vehículos             │
│   ✅ Suscripciones dealers ($49-299)  ❌ Pagos entre partes             │
│   ✅ Boosts y promociones             ❌ Transferencia de propiedad     │
│                                       ❌ Comisiones por ventas          │
│                                                                         │
│   Las transacciones de vehículos ocurren DIRECTAMENTE                   │
│   entre dealers/vendedores y compradores, FUERA de OKLA.                │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## ✅ DOCUMENTOS ACTUALIZADOS

### 1. UAF y Compliance

| Documento                                     | Ubicación                 | Cambio Realizado                                                   |
| --------------------------------------------- | ------------------------- | ------------------------------------------------------------------ |
| **05-AUDITORIA-UAF.md**                       | 25-AUDITORIA-CUMPLIMIENTO | Reescritura completa: OKLA probablemente NO es sujeto obligado UAF |
| **01-ley-155-17.md**                          | 08-COMPLIANCE-LEGAL-RD    | Agregada nota clarificando que OKLA no es intermediario financiero |
| **09-ros-reporte-operaciones-sospechosas.md** | 08-COMPLIANCE-LEGAL-RD    | Clarificado que ROS aplica a dealers, no a OKLA                    |
| **02-MATRIZ-OBLIGACIONES-LEGALES.md**         | 25-AUDITORIA-CUMPLIMIENTO | Sección UAF marcada como "Probablemente NO aplica"                 |
| **01-RESUMEN-EJECUTIVO.md**                   | 25-AUDITORIA-CUMPLIMIENTO | UAF cambiado de 🔴 10% a 🟡 N/A                                    |

### 2. DGII y Fiscal

| Documento                            | Ubicación                 | Cambio Realizado                                     |
| ------------------------------------ | ------------------------- | ---------------------------------------------------- |
| **08-obligaciones-fiscales-dgii.md** | 08-COMPLIANCE-LEGAL-RD    | Agregado contexto de modelo de negocio OKLA          |
| **03-dgii-integration.md**           | 08-COMPLIANCE-LEGAL-RD    | Clarificado que OKLA factura servicios de publicidad |
| **06-ncf-comprobantes-fiscales.md**  | 06-PAGOS-FACTURACION      | Actualizado con tipos NCF específicos para OKLA      |
| **04-AUDITORIA-DGII.md**             | 25-AUDITORIA-CUMPLIMIENTO | Confirmado que DGII SÍ aplica a OKLA                 |

### 3. Servicios Descartados

| Documento                | Ubicación            | Cambio Realizado                              |
| ------------------------ | -------------------- | --------------------------------------------- |
| **05-escrow-service.md** | 05-PAGOS-FACTURACION | Marcado como DESCARTADO - No aplica al modelo |

---

## 📄 DOCUMENTOS NUEVOS CREADOS

### 1. Datos Oficiales de la Empresa

**Archivo:** `25-AUDITORIA-CUMPLIMIENTO/00-DATOS-EMPRESA-OKLA.md`

Contiene información oficial del Registro Mercantil:

- RNC: 1-33-32590-1
- Registro Mercantil: 196339PSD
- Razón Social: OKLA S.R.L.
- Capital Social: RD$100,000.00
- Gerente: Nicauris Mateo Alcántara
- Socios: 2 (Nicauris Mateo Alcántara, Gregory Alexander Moreno Lebrón)
- Objeto Social: Comercio electrónico, servicios logísticos

### 2. Procedimiento Fiscal Completo

**Archivo:** `08-COMPLIANCE-LEGAL-RD/10-PROCEDIMIENTO-FISCAL-OKLA.md`

Documento maestro que incluye:

- Tipos de NCF que aplican a OKLA (B01, B02, B04, B11, B13)
- Formatos 606, 607, 608 con ejemplos
- Cálculo de ITBIS
- Procedimiento mensual completo
- Calendario fiscal
- Checklist mensual

---

## � GASTOS OPERATIVOS DE OKLA

### Categorías de Gastos para Formato 606

Los gastos de OKLA se categorizan según su origen y tratamiento fiscal:

#### 1. Gastos Internacionales (NCF B13 / Tipo 05)

Estos proveedores están fuera de RD y no emiten NCF:

| Proveedor     | Servicio                | Costo Estimado USD | Costo RD$ (~60) | Frecuencia |
| ------------- | ----------------------- | ------------------ | --------------- | ---------- |
| Digital Ocean | Hosting/Servers         | $100-200           | ~$6,000-12,000  | Mensual    |
| GitHub        | Repositorio + Copilot   | $4-21              | ~$1,260         | Mensual    |
| Stripe        | Comisiones (2.9%+$0.30) | Variable           | ~$15,000        | Mensual    |
| Google Ads    | Publicidad              | $200-500           | ~$30,000        | Mensual    |
| Facebook/Meta | Publicidad              | $200-400           | ~$20,000        | Mensual    |
| GoDaddy       | Dominios                | $15-30             | ~$900           | Anual      |
| OpenAI API    | IA/ChatGPT              | $20-100            | ~$3,000         | Mensual    |
| AWS/S3        | Almacenamiento          | $50-100            | ~$3,000         | Mensual    |
| Cloudflare    | CDN/Security            | $0-25              | ~$1,500         | Mensual    |

**Total Estimado Gastos Internacionales: RD$70,000-100,000/mes**

#### 2. Gastos Locales con NCF (B01/B02)

Proveedores dominicanos que emiten comprobante fiscal:

| Proveedor       | Servicio              | ITBIS  | Retención | Costo RD$ | Frecuencia |
| --------------- | --------------------- | ------ | --------- | --------- | ---------- |
| AZUL Popular    | Comisiones procesador | 18%    | No        | ~$8,000   | Mensual    |
| Banco Popular   | Mantenimiento cuenta  | 18%    | No        | ~$1,500   | Mensual    |
| Contador        | Servicios contables   | 18%    | 10%       | ~$10,000  | Mensual    |
| Abogado         | Consultoría legal     | 18%    | 10%       | ~$5,000   | Variable   |
| Claro/Altice    | Internet fibra óptica | 18%    | No        | ~$3,500   | Mensual    |
| Edesur/Edenorte | Electricidad          | Exento | No        | ~$2,000   | Mensual    |
| Office Depot    | Suministros           | 18%    | No        | ~$3,000   | Variable   |
| Dueño local     | Alquiler oficina      | Exento | 10%       | ~$25,000  | Mensual    |

**Total Estimado Gastos Locales: RD$50,000-60,000/mes**

#### 3. Gastos de Nómina (NO van en 606)

Los gastos de empleados se manejan vía TSS (SUIR) e IR-17:

| Concepto                  | 3 Empleados    | Frecuencia |
| ------------------------- | -------------- | ---------- |
| Salarios brutos           | RD$175,000     | Mensual    |
| AFP Empleador (7.10%)     | RD$12,425      | Mensual    |
| SFS Empleador (7.09%)     | RD$12,408      | Mensual    |
| Riesgos Laborales (1.10%) | RD$1,925       | Mensual    |
| INFOTEP (1.00%)           | RD$1,750       | Mensual    |
| **Costo Total Nómina**    | **RD$203,508** | Mensual    |

**Nota:** Vacaciones, bonificación y regalia se provisionan mensualmente.

#### 4. Resumen de Gastos Mensuales OKLA

```
┌─────────────────────────────────────────────────────────────────────────┐
│              ESTRUCTURA DE COSTOS MENSUALES OKLA                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  GASTOS FIJOS MENSUALES:                                                │
│  ───────────────────────                                                │
│  Nómina (3 empleados):           RD$203,508                             │
│  Gastos Internacionales:         RD$ 85,000 (promedio)                  │
│  Gastos Locales con NCF:         RD$ 55,000 (promedio)                  │
│  ─────────────────────────────────────────────                          │
│  TOTAL GASTOS FIJOS:             RD$343,508/mes                         │
│                                                                         │
│  GASTOS VARIABLES:                                                      │
│  ─────────────────                                                      │
│  Comisiones Stripe (2.9%):       Según volumen ventas                   │
│  Comisiones AZUL (2.5%):         Según volumen ventas                   │
│  Publicidad adicional:           Según campañas                         │
│  Servicios legales:              Según necesidad                        │
│                                                                         │
│  PUNTO DE EQUILIBRIO ESTIMADO:                                          │
│  ─────────────────────────────                                          │
│  Si suscripción promedio = $99/mes                                      │
│  Necesitas ~3,500 suscripciones + ads para cubrir costos                │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Tratamiento Fiscal por Tipo de Gasto

| Tipo de Gasto       | Va en 606 | ITBIS Deducible | Retención | NCF       |
| ------------------- | --------- | --------------- | --------- | --------- |
| Hosting exterior    | ✅ Sí     | ❌ No           | ⚠️ Ver    | B13       |
| Publicidad exterior | ✅ Sí     | ❌ No           | ⚠️ Ver    | B13       |
| Comisiones Stripe   | ✅ Sí     | ❌ No           | ⚠️ Ver    | B13       |
| Comisiones AZUL     | ✅ Sí     | ✅ Sí           | ❌ No     | B01       |
| Contador (persona)  | ✅ Sí     | ✅ Sí           | ✅ 10%    | B01       |
| Abogado (persona)   | ✅ Sí     | ✅ Sí           | ✅ 10%    | B01       |
| Internet/teléfono   | ✅ Sí     | ✅ Sí           | ❌ No     | B01       |
| Alquiler (persona)  | ✅ Sí     | ❌ Exento       | ✅ 10%    | B02       |
| Electricidad        | ✅ Sí     | ❌ Exento       | ❌ No     | B01       |
| Salarios            | ❌ No     | N/A             | ✅ ISR    | Via IR-17 |
| TSS/AFP/SFS         | ❌ No     | N/A             | N/A       | Via SUIR  |

---

## �💰 OBLIGACIONES FISCALES DGII DEFINITIVAS

### Tipos de NCF que OKLA Debe Usar

#### Para VENTAS (Ingresos de OKLA)

| NCF     | Nombre           | Uso                      | Obligatorio |
| ------- | ---------------- | ------------------------ | ----------- |
| **B01** | Crédito Fiscal   | Dealers/empresas con RNC | ✅ Sí       |
| **B02** | Consumidor Final | Individuos sin RNC       | ✅ Sí       |
| **B04** | Nota de Crédito  | Devoluciones             | ✅ Sí       |
| **B15** | Gubernamental    | Ventas a gobierno        | ⚠️ Si surge |

#### Para COMPRAS (Gastos de OKLA)

| NCF     | Nombre              | Uso                 | Obligatorio |
| ------- | ------------------- | ------------------- | ----------- |
| **B11** | Comprobante Compras | Proveedores sin NCF | ✅ Sí       |
| **B13** | Gastos del Exterior | Stripe, AWS, etc.   | ✅ Sí       |

### Reportes Mensuales a DGII

| Reporte         | Formulario | Día Límite | Contenido         |
| --------------- | ---------- | ---------- | ----------------- |
| **IR-17**       | IR-17      | Día 10     | Retenciones ISR   |
| **Formato 606** | 606        | Día 15     | Compras del mes   |
| **Formato 607** | 607        | Día 15     | Ventas del mes    |
| **Formato 608** | 608        | Día 15     | NCF anulados      |
| **ITBIS**       | IT-1       | Día 20     | Declaración ITBIS |

### Reportes Anuales

| Reporte           | Fecha Límite | Contenido                     |
| ----------------- | ------------ | ----------------------------- |
| **IR-2**          | 30 Abril     | Impuesto sobre la renta anual |
| **Cierre Fiscal** | Según cierre | Estados financieros           |

---

## 🚫 OBLIGACIONES QUE NO APLICAN A OKLA

### UAF (Unidad de Análisis Financiero)

| Obligación                    | ¿Aplica? | Razón                          |
| ----------------------------- | -------- | ------------------------------ |
| Registro como Sujeto Obligado | ❌ NO    | OKLA no compra/vende vehículos |
| Oficial de Cumplimiento       | ❌ NO    | No es intermediario financiero |
| Manual de Prevención LA/FT    | ❌ NO    | No procesa pagos de terceros   |
| KYC/DDC de compradores        | ❌ NO    | Las transacciones son externas |
| ROS (Reportes Sospechosos)    | ❌ NO    | No ve las transacciones        |

**Nota:** Los **DEALERS** que venden vehículos profesionalmente SÍ son sujetos obligados de la UAF.

### Servicios No Aplicables

| Servicio                  | Razón de No Aplicabilidad           |
| ------------------------- | ----------------------------------- |
| **EscrowService**         | OKLA no procesa pagos de vehículos  |
| **ComisionService**       | OKLA no cobra comisiones por ventas |
| **VehiclePaymentService** | Pagos son directos entre partes     |

---

## 📊 RESUMEN DE ESTADO POST-AJUSTE

### Obligaciones por Regulador

| Regulador          | Obligaciones | Aplica a OKLA | Estado     |
| ------------------ | ------------ | ------------- | ---------- |
| **DGII**           | 15           | ✅ SÍ         | 🟡 Parcial |
| **UAF**            | 12           | ❌ Prob. NO   | N/A        |
| **Ley 172-13**     | 10           | ✅ SÍ         | 🟡 Parcial |
| **Pro Consumidor** | 8            | ✅ SÍ         | 🟡 Parcial |
| **Ley 126-02**     | 6            | ✅ SÍ         | ✅ Cumple  |

### Implementación Técnica Pendiente

| Funcionalidad            | Estado       | Prioridad |
| ------------------------ | ------------ | --------- |
| Generación NCF B01/B02   | 🟡 Parcial   | 🔴 Alta   |
| Notas de Crédito B04     | 🔴 Pendiente | 🔴 Alta   |
| Formato 606 automático   | 🔴 Pendiente | 🔴 Alta   |
| Formato 607 automático   | 🔴 Pendiente | 🔴 Alta   |
| Formato 608 automático   | 🔴 Pendiente | 🟡 Media  |
| Dashboard Fiscal Admin   | 🔴 Pendiente | 🟡 Media  |
| PDF de facturas          | 🔴 Pendiente | 🟡 Media  |
| Alertas de secuencia NCF | 🔴 Pendiente | 🟢 Baja   |

---

## 🎯 PRÓXIMOS PASOS

### Inmediato (Antes de primer cierre fiscal)

1. [ ] Verificar RNC activo en DGII (1-33-32590-1)
2. [ ] Solicitar secuencias NCF B01, B02, B04 a DGII
3. [ ] Configurar contador para reportes mensuales
4. [ ] Implementar generación de NCF en BillingService

### Corto Plazo (Q1 2026)

1. [ ] Completar implementación de NCF en facturas
2. [ ] Crear endpoint para generación de formatos 606/607
3. [ ] Implementar UI de facturas para usuarios
4. [ ] Crear dashboard fiscal para admin

### Mediano Plazo (Q2-Q3 2026)

1. [ ] Automatizar envío de formatos a DGII
2. [ ] Implementar notas de crédito B04
3. [ ] Migrar a e-CF (factura electrónica)
4. [ ] Alertas automáticas de obligaciones fiscales

---

## 📚 DOCUMENTACIÓN DE REFERENCIA

| Documento                        | Ubicación                 | Propósito                     |
| -------------------------------- | ------------------------- | ----------------------------- |
| 00-DATOS-EMPRESA-OKLA.md         | 25-AUDITORIA-CUMPLIMIENTO | Datos oficiales empresa       |
| 10-PROCEDIMIENTO-FISCAL-OKLA.md  | 08-COMPLIANCE-LEGAL-RD    | Procedimiento fiscal completo |
| 05-AUDITORIA-UAF.md              | 25-AUDITORIA-CUMPLIMIENTO | Análisis de aplicabilidad UAF |
| 06-ncf-comprobantes-fiscales.md  | 06-PAGOS-FACTURACION      | Tipos de NCF y emisión        |
| 08-obligaciones-fiscales-dgii.md | 08-COMPLIANCE-LEGAL-RD    | Obligaciones DGII             |
| 03-CALENDARIO-FISCAL-REPORTES.md | 25-AUDITORIA-CUMPLIMIENTO | Calendario de vencimientos    |

---

**Documento creado:** Enero 25, 2026  
**Autor:** Equipo de Compliance OKLA  
**Próxima revisión:** Cuando cambien regulaciones o modelo de negocio
