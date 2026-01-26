# 💰 Procedimiento Fiscal DGII para OKLA S.R.L.

> **Empresa:** OKLA S.R.L.  
> **RNC:** 1-33-32590-1  
> **Registro Mercantil:** 196339PSD  
> **Fecha de Creación:** Enero 25, 2026  
> **Estado:** ✅ DOCUMENTO MAESTRO DE PROCEDIMIENTOS FISCALES

---

## 📋 RESUMEN EJECUTIVO

Este documento define los procedimientos fiscales específicos que OKLA S.R.L. debe cumplir ante la DGII, basados en su **modelo de negocio real** como plataforma de anuncios clasificados de vehículos.

### Modelo de Negocio OKLA

```
┌─────────────────────────────────────────────────────────────────────────┐
│                   MODELO DE NEGOCIO OKLA S.R.L.                         │
│                   (Plataforma de Anuncios Clasificados)                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   OKLA VENDE SERVICIOS DE PUBLICIDAD:                                   │
│   ═══════════════════════════════════                                   │
│                                                                         │
│   1. Publicación de anuncios individuales: $29 + ITBIS                  │
│   2. Suscripciones mensuales para dealers:                              │
│      • Starter: $49/mes + ITBIS                                         │
│      • Pro: $129/mes + ITBIS                                            │
│      • Enterprise: $299/mes + ITBIS                                     │
│   3. Boosts y promociones de publicaciones                              │
│   4. Servicios premium adicionales                                      │
│                                                                         │
│   ❌ OKLA NO compra ni vende vehículos                                  │
│   ❌ OKLA NO cobra comisiones por ventas de vehículos                   │
│   ❌ OKLA NO es intermediario financiero                                │
│   ❌ OKLA NO procesa pagos entre compradores y vendedores               │
│                                                                         │
│   Las transacciones de vehículos ocurren DIRECTAMENTE                   │
│   entre dealers/vendedores y compradores.                               │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 1. TIPOS DE NCF QUE APLICAN A OKLA

### 1.1 NCF para Ventas (Ingresos de OKLA)

| NCF     | Nombre               | Uso en OKLA                         | Obligatorio |
| ------- | -------------------- | ----------------------------------- | ----------- |
| **B01** | Crédito Fiscal       | Dealers y empresas **con RNC**      | ✅ Sí       |
| **B02** | Consumidor Final     | Individuos **sin RNC** o con cédula | ✅ Sí       |
| **B04** | Nota de Crédito      | Devoluciones, correcciones          | ✅ Sí       |
| **B15** | Gubernamental        | Ventas a entidades de gobierno      | ⚠️ Si surge |
| B14     | Regímenes Especiales | Zonas francas                       | ❌ No       |
| B16     | Exportación          | Servicios al exterior               | ⚠️ Si surge |

### 1.2 NCF para Compras (Gastos de OKLA)

| NCF     | Nombre              | Uso en OKLA                           | Obligatorio |
| ------- | ------------------- | ------------------------------------- | ----------- |
| **B11** | Comprobante Compras | Proveedores informales sin NCF        | ✅ Sí       |
| **B13** | Gastos del Exterior | Stripe, AWS, Digital Ocean, etc.      | ✅ Sí       |
| B03     | Nota de Débito      | Corrección de facturas de proveedores | ⚠️ Raro     |

### 1.3 Flujo de Decisión para Emisión de NCF

```
┌─────────────────────────────────────────────────────────────────────────┐
│                  FLUJO DE SELECCIÓN DE NCF (VENTAS)                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   ┌─────────────────────┐                                               │
│   │ CLIENTE SOLICITA    │                                               │
│   │ FACTURA POR SERVICIO │                                              │
│   └─────────┬───────────┘                                               │
│             │                                                           │
│             ▼                                                           │
│   ┌─────────────────────┐                                               │
│   │ ¿CLIENTE TIENE RNC? │                                               │
│   └─────────┬───────────┘                                               │
│             │                                                           │
│     ┌───────┴───────┐                                                   │
│     ▼               ▼                                                   │
│  ┌──────┐       ┌──────┐                                                │
│  │ SÍ   │       │ NO   │                                                │
│  └──┬───┘       └──┬───┘                                                │
│     │              │                                                    │
│     ▼              ▼                                                    │
│  ┌──────────┐  ┌──────────┐                                             │
│  │ ¿Es      │  │ Emitir   │                                             │
│  │ gobierno?│  │ B02      │                                             │
│  └────┬─────┘  │ Consumo  │                                             │
│       │        └──────────┘                                             │
│   ┌───┴───┐                                                             │
│   ▼       ▼                                                             │
│ ┌────┐ ┌────┐                                                           │
│ │ SÍ │ │ NO │                                                           │
│ └─┬──┘ └─┬──┘                                                           │
│   │      │                                                              │
│   ▼      ▼                                                              │
│ ┌──────┐ ┌──────┐                                                       │
│ │Emitir│ │Emitir│                                                       │
│ │ B15  │ │ B01  │                                                       │
│ │Gubern│ │Crédit│                                                       │
│ └──────┘ └──────┘                                                       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. REPORTES OBLIGATORIOS A LA DGII

### 2.1 Resumen de Reportes Mensuales

| Reporte               | Formulario | Día Límite | Contenido                   | Estado OKLA  |
| --------------------- | ---------- | ---------- | --------------------------- | ------------ |
| **IR-17**             | IR-17      | Día 10     | Retenciones a proveedores   | 🔴 Pendiente |
| **Formato 606**       | 606        | Día 15     | Compras del mes anterior    | 🔴 Pendiente |
| **Formato 607**       | 607        | Día 15     | Ventas del mes anterior     | 🔴 Pendiente |
| **Formato 608**       | 608        | Día 15     | NCF anulados del mes        | 🔴 Pendiente |
| **Declaración ITBIS** | IT-1       | Día 20     | ITBIS cobrado vs pagado     | 🔴 Manual    |
| **Pago ITBIS**        | -          | Día 20     | Transferencia/cheque a DGII | 🔴 Manual    |

### 2.2 Reportes Anuales

| Reporte               | Formulario | Fecha Límite          | Contenido               |
| --------------------- | ---------- | --------------------- | ----------------------- |
| **Declaración ISR**   | IR-2       | 30 Abril (120 días)   | Impuesto sobre la renta |
| **Cierre Fiscal**     | -          | Según cierre contable | Estados financieros     |
| **Renovación RM**     | -          | Julio 2027            | Registro Mercantil      |
| **Actualización RNC** | -          | Si hay cambios        | Datos en DGII           |

---

## 3. FORMATO 606 (COMPRAS) - PROCEDIMIENTO

### 3.1 Qué Reportar en el 606

**Compras y gastos operativos de OKLA que van en el 606:**

#### Gastos Internacionales (NCF B13 - Gastos del Exterior)

| Categoría              | Proveedor              | Monto Aprox. Mensual | NCF | ITBIS | Tipo Gasto       |
| ---------------------- | ---------------------- | -------------------- | --- | ----- | ---------------- |
| **Hosting/Cloud**      | Digital Ocean          | $50-200 USD          | B13 | N/A   | 02 (Servicios)   |
| **Repositorio**        | GitHub                 | $4-21 USD            | B13 | N/A   | 02 (Servicios)   |
| **Pasarela Pagos**     | Stripe (comisiones)    | 3.5% de ventas       | B13 | N/A   | 07 (Financieros) |
| **Publicidad Digital** | Google Ads             | Variable             | B13 | N/A   | 02 (Servicios)   |
| **Publicidad Digital** | Facebook/Meta Ads      | Variable             | B13 | N/A   | 02 (Servicios)   |
| **Publicidad Digital** | Instagram Ads          | Variable             | B13 | N/A   | 02 (Servicios)   |
| **APIs Externas**      | Twilio, SendGrid, etc. | Variable             | B13 | N/A   | 02 (Servicios)   |
| **Software**           | Microsoft 365, Adobe   | $10-50 USD           | B13 | N/A   | 02 (Servicios)   |
| **Herramientas Dev**   | JetBrains, Postman Pro | Variable             | B13 | N/A   | 02 (Servicios)   |

#### Gastos Locales (RD) - Con NCF y ITBIS

| Categoría                | Proveedor                             | NCF Recibido | ITBIS  | Retención ISR | Tipo Gasto         |
| ------------------------ | ------------------------------------- | ------------ | ------ | ------------- | ------------------ |
| **Pasarela Pagos Local** | AZUL Banco Popular (comisiones)       | B01          | 18%    | No            | 07 (Financieros)   |
| **Comisiones Bancarias** | Banco (transferencias, mantenimiento) | B01          | 18%    | No            | 07 (Financieros)   |
| **Dominio .do**          | NIC.do (nic.do)                       | B01          | 18%    | No            | 02 (Servicios)     |
| **Nómina/Empleados**     | Empleados                             | TSS/IR-3     | N/A    | Sí (IR-3)     | 01 (Personal)      |
| **Servicios Legales**    | Abogado/Bufete                        | B01          | 18%    | 10%           | 02 (Servicios)     |
| **Contabilidad**         | Contador                              | B01          | 18%    | 10%           | 02 (Servicios)     |
| **Internet**             | Claro, Altice                         | B01          | 18%    | No            | 02 (Servicios)     |
| **Telefonía**            | Claro, Altice                         | B01          | 18%    | No            | 02 (Servicios)     |
| **Alquiler Oficina**     | Arrendador (persona física)           | B02          | 18%    | 10%           | 03 (Arrendamiento) |
| **Alquiler Oficina**     | Arrendador (empresa)                  | B01          | 18%    | No            | 03 (Arrendamiento) |
| **Electricidad**         | Edenorte, Edesur, Edeeste             | B01          | Exento | No            | 06 (Otras)         |
| **Equipos Tecnológicos** | Tiendas locales                       | B01          | 18%    | No            | 09 (Activos Fijos) |
| **Publicidad Local**     | Periódicos, radio, influencers        | B01/B02      | 18%    | 10%\*         | 02 (Servicios)     |
| **Freelancers Locales**  | Diseñadores, devs                     | B01/B02      | 18%    | 10%           | 02 (Servicios)     |

> \*Retención 10% si el monto supera RD$50,000 y es persona física

#### Detalle de Gastos por Empleados (Nómina)

| Concepto               | Responsable | Formulario     | Fecha Límite      |
| ---------------------- | ----------- | -------------- | ----------------- |
| Salarios               | OKLA        | Nómina interna | Quincenal/Mensual |
| TSS (Seguridad Social) | OKLA        | TSS            | Día 3 del mes     |
| AFP (Pensiones)        | OKLA        | TSS            | Día 3 del mes     |
| SFS (Salud)            | OKLA        | TSS            | Día 3 del mes     |
| Riesgos Laborales      | OKLA        | TSS            | Día 3 del mes     |
| ISR Empleados          | OKLA        | IR-3           | Día 10 del mes    |
| Bonificación           | OKLA        | -              | Según contrato    |
| Vacaciones             | OKLA        | -              | Según ley         |
| Regalia Pascual        | OKLA        | -              | Diciembre 20      |

**IMPORTANTE:** Los gastos de nómina NO van en el formato 606. Van en:

- **TSS**: Sistema de Seguridad Social (mensual)
- **IR-3**: Retención de salarios (mensual, día 10)

#### Códigos de Tipo de Gasto (Campo 3 del 606)

| Código | Descripción               | Ejemplos para OKLA                             |
| ------ | ------------------------- | ---------------------------------------------- |
| **01** | Gastos de personal        | No va en 606 (va en TSS/IR-3)                  |
| **02** | Gastos por servicios      | Hosting, APIs, publicidad, contabilidad, legal |
| **03** | Gastos de arrendamiento   | Alquiler oficina                               |
| **04** | Gastos por activos fijos  | Depreciación (si aplica)                       |
| **05** | Gastos de representación  | Viáticos, entretenimiento clientes             |
| **06** | Otras deducciones         | Electricidad, agua, gas                        |
| **07** | Gastos financieros        | Comisiones Stripe, AZUL, bancos                |
| **08** | Gastos extraordinarios    | Multas, penalidades                            |
| **09** | Compras de activos fijos  | Laptops, servidores, muebles                   |
| **10** | Compras de inventario     | N/A para OKLA                                  |
| **11** | Gastos con crédito fiscal | Compras para reventa (N/A)                     |

### 3.2 Campos del Formato 606

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      ESTRUCTURA FORMATO 606                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Campo 1:  RNC/Cédula del Proveedor (9-11 dígitos)                      │
│  Campo 2:  Tipo de Identificación (1=RNC, 2=Cédula, 3=Pasaporte)        │
│  Campo 3:  Tipo de Bienes/Servicios (código 01-11)                      │
│  Campo 4:  NCF del Comprobante (B0100000001)                            │
│  Campo 5:  NCF Modificado (si aplica nota crédito)                      │
│  Campo 6:  Fecha del Comprobante (YYYYMMDD)                             │
│  Campo 7:  Fecha de Pago (YYYYMMDD)                                     │
│  Campo 8:  Monto de Servicios (sin ITBIS)                               │
│  Campo 9:  Monto de Bienes (sin ITBIS)                                  │
│  Campo 10: Monto Total (servicios + bienes)                             │
│  Campo 11: ITBIS Facturado                                              │
│  Campo 12: ITBIS Retenido                                               │
│  Campo 13: ITBIS Sujeto a Proporcionalidad                              │
│  Campo 14: ITBIS llevado al Costo                                       │
│  Campo 15: ITBIS por Adelantar                                          │
│  Campo 16: ISR Retenido                                                 │
│  Campo 17: Forma de Pago (01=Efectivo, 02=Cheque, 03=Tarjeta, etc.)     │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 3.3 Ejemplos de Líneas 606 (Gastos Reales de OKLA)

```
# ══════════════════════════════════════════════════════════════════════════
# GASTOS INTERNACIONALES (B13 - Sin RNC, sin ITBIS)
# ══════════════════════════════════════════════════════════════════════════

# Digital Ocean - Hosting ($100 USD = ~RD$6,000)
|0|3|02|B1300000001||20260115|20260115|6000.00|0.00|6000.00|0.00|0.00|0.00|0.00|0.00|0.00|03|

# GitHub - Repositorio ($21 USD = ~RD$1,260)
|0|3|02|B1300000002||20260115|20260115|1260.00|0.00|1260.00|0.00|0.00|0.00|0.00|0.00|0.00|03|

# Stripe - Comisiones del mes (~RD$15,000)
|0|3|07|B1300000003||20260131|20260131|15000.00|0.00|15000.00|0.00|0.00|0.00|0.00|0.00|0.00|03|

# Google Ads - Publicidad (~RD$30,000)
|0|3|02|B1300000004||20260131|20260131|30000.00|0.00|30000.00|0.00|0.00|0.00|0.00|0.00|0.00|03|

# Facebook Ads - Publicidad (~RD$20,000)
|0|3|02|B1300000005||20260131|20260131|20000.00|0.00|20000.00|0.00|0.00|0.00|0.00|0.00|0.00|03|

# ══════════════════════════════════════════════════════════════════════════
# GASTOS LOCALES (Con NCF y ITBIS)
# ══════════════════════════════════════════════════════════════════════════

# NIC.do - Dominio okla.com.do (~RD$2,500/año + ITBIS)
# NIC.do es la entidad local que administra dominios .do
130529842|1|02|B0100012345||20260110|20260110|2500.00|0.00|2500.00|450.00|0.00|0.00|0.00|0.00|0.00|03|

# ══════════════════════════════════════════════════════════════════════════
# OTROS GASTOS LOCALES
# ══════════════════════════════════════════════════════════════════════════

# AZUL Banco Popular - Comisiones (~RD$8,000 + ITBIS)
101234567|1|07|B0100000543||20260131|20260131|8000.00|0.00|8000.00|1440.00|0.00|0.00|0.00|0.00|0.00|03|

# Contador - Honorarios (~RD$15,000 + ITBIS, con retención ISR 10%)
102345678|1|02|B0100000789||20260125|20260125|15000.00|0.00|15000.00|2700.00|0.00|0.00|0.00|0.00|1500.00|02|

# Abogado - Servicios legales (~RD$25,000 + ITBIS, con retención ISR 10%)
103456789|1|02|B0100000321||20260120|20260125|25000.00|0.00|25000.00|4500.00|0.00|0.00|0.00|0.00|2500.00|02|

# Claro - Internet oficina (~RD$3,500 + ITBIS)
101654321|1|02|B0100098765||20260115|20260115|3500.00|0.00|3500.00|630.00|0.00|0.00|0.00|0.00|0.00|03|

# Alquiler oficina a persona física (~RD$20,000 + ITBIS, con retención ISR 10%)
00112345678|2|03|B0200005432||20260105|20260105|20000.00|0.00|20000.00|3600.00|0.00|0.00|0.00|0.00|2000.00|02|

# Tienda local - Laptop (~RD$45,000 + ITBIS)
109876543|1|09|B0100054321||20260110|20260110|0.00|45000.00|45000.00|8100.00|0.00|0.00|0.00|0.00|0.00|03|

# Freelancer diseñador local (~RD$12,000 + ITBIS, con retención ISR 10%)
00198765432|2|02|B0200012345||20260128|20260128|12000.00|0.00|12000.00|2160.00|0.00|0.00|0.00|0.00|1200.00|02|
```

### 3.4 Formas de Pago (Campo 17)

| Código | Forma de Pago          | Uso Típico en OKLA                         |
| ------ | ---------------------- | ------------------------------------------ |
| **01** | Efectivo               | Raro                                       |
| **02** | Cheque/Transferencia   | Proveedores locales grandes                |
| **03** | Tarjeta Crédito/Débito | Servicios internacionales, compras menores |
| **04** | Compra a Crédito       | Proveedores con cuenta                     |
| **05** | Permuta                | N/A                                        |
| **06** | Nota de Crédito        | Ajustes                                    |
| **07** | Mixto                  | Combinación                                |

---

## 4. FORMATO 607 (VENTAS) - PROCEDIMIENTO

### 4.1 Qué Reportar en el 607

**Ventas típicas de OKLA que van en el 607:**

| Tipo de Venta                  | NCF Emitido | ITBIS | Ejemplo Monto    |
| ------------------------------ | ----------- | ----- | ---------------- |
| Suscripción Dealer (con RNC)   | **B01**     | 18%   | $129 + $23.22    |
| Suscripción Dealer (sin RNC)   | **B02**     | 18%   | $129 + $23.22    |
| Publicación Individual con RNC | **B01**     | 18%   | $29 + $5.22      |
| Publicación Individual sin RNC | **B02**     | 18%   | $29 + $5.22      |
| Boost de Publicación           | **B01/B02** | 18%   | Variable         |
| Venta a Gobierno               | **B15**     | 18%   | Si aplica        |
| Devolución/Nota Crédito        | **B04**     | 18%   | (Monto negativo) |

### 4.2 Campos del Formato 607

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      ESTRUCTURA FORMATO 607                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Campo 1:  RNC/Cédula del Cliente (9-11 dígitos, 0 si anónimo)          │
│  Campo 2:  Tipo de Identificación (1=RNC, 2=Cédula, 3=Pasaporte)        │
│  Campo 3:  NCF del Comprobante Emitido (B0100000001)                    │
│  Campo 4:  NCF Modificado (si es nota crédito)                          │
│  Campo 5:  Tipo de Ingreso (código según actividad)                     │
│  Campo 6:  Fecha del Comprobante (YYYYMMDD)                             │
│  Campo 7:  Fecha de Retención (si aplica)                               │
│  Campo 8:  Monto Facturado (sin ITBIS)                                  │
│  Campo 9:  ITBIS Facturado                                              │
│  Campo 10: ITBIS Retenido por Terceros (si aplica)                      │
│  Campo 11: ITBIS Percibido                                              │
│  Campo 12: Retención de Renta por Terceros (si aplica)                  │
│  Campo 13: ISR Percibido                                                │
│  Campo 14: Impuesto Selectivo Consumo (N/A para OKLA)                   │
│  Campo 15: Otros Impuestos/Tasas                                        │
│  Campo 16: Monto Propina Legal (N/A)                                    │
│  Campo 17: Efectivo                                                     │
│  Campo 18: Cheque/Transferencia/Depósito                                │
│  Campo 19: Tarjeta Débito/Crédito                                       │
│  Campo 20: Venta a Crédito                                              │
│  Campo 21: Bonos o Certificados de Regalo (N/A)                         │
│  Campo 22: Permuta (N/A)                                                │
│  Campo 23: Otras Formas de Venta                                        │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.3 Ejemplos de Líneas 607

```
# Suscripción Plan Pro a dealer con RNC (tarjeta de crédito)
131325901|1|B0100000789||02|20260115||129.00|23.22|0.00|0.00|0.00|0.00|0.00|0.00|0.00|0.00|0.00|152.22|0.00|0.00|0.00|0.00|

# Publicación individual a persona sin RNC (tarjeta)
0|2|B0200001234||02|20260118||29.00|5.22|0.00|0.00|0.00|0.00|0.00|0.00|0.00|0.00|0.00|34.22|0.00|0.00|0.00|0.00|

# Nota de crédito por devolución de suscripción
131325901|1|B0400000012|B0100000789|02|20260125||129.00|23.22|0.00|0.00|0.00|0.00|0.00|0.00|0.00|0.00|0.00|152.22|0.00|0.00|0.00|0.00|
```

---

## 5. FORMATO 608 (ANULACIONES) - PROCEDIMIENTO

### 5.1 Cuándo Usar el 608

El formato 608 reporta NCF **anulados antes de entregar al cliente**:

| Situación                          | ¿Va en 608? | Observación             |
| ---------------------------------- | ----------- | ----------------------- |
| NCF emitido pero anulado antes     | ✅ Sí       | Error de emisión        |
| Factura entregada y devuelta       | ❌ No       | Emitir Nota Crédito B04 |
| Secuencia NCF no usada             | ❌ No       | Solo si se emitió       |
| NCF duplicado por error de sistema | ✅ Sí       | Anular el duplicado     |

### 5.2 Estructura del Formato 608

```
Campo 1: NCF Anulado (B0100000XXX)
Campo 2: Tipo de Anulación (01=Deterioro, 02=Error imprenta, 03=Error numeración, 04=Duplicidad, 05=Corrección datos)
Campo 3: Fecha de Anulación (YYYYMMDD)
```

---

## 6. CÁLCULO DE ITBIS MENSUAL

### 6.1 Fórmula Básica

```
┌─────────────────────────────────────────────────────────────────────────┐
│                   CÁLCULO DE ITBIS A PAGAR                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   ITBIS Cobrado (Ventas)                                                │
│   - ITBIS Pagado (Compras deducibles)                                   │
│   - ITBIS Retenido por terceros                                         │
│   - ITBIS Adelantado                                                    │
│   ═══════════════════════════════════════                               │
│   = ITBIS A PAGAR (o Crédito Fiscal si negativo)                        │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 6.2 Ejemplo Mes de Enero 2026

```
┌─────────────────────────────────────────────────────────────────────────┐
│           CÁLCULO ITBIS - ENERO 2026 (Ejemplo)                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  VENTAS (ITBIS Cobrado):                                                │
│  ──────────────────────────                                             │
│  50 suscripciones x $129       = $6,450.00                              │
│  ITBIS 18%                     = $1,161.00                              │
│                                                                         │
│  100 publicaciones x $29       = $2,900.00                              │
│  ITBIS 18%                     = $  522.00                              │
│                                                                         │
│  TOTAL ITBIS COBRADO           = $1,683.00                              │
│                                                                         │
│  COMPRAS (ITBIS Pagado Deducible):                                      │
│  ──────────────────────────                                             │
│  Contador $8,000 x 18%         = $1,440.00                              │
│  Internet $3,000 x 18%         = $  540.00                              │
│  Hosting (Exterior)            = $    0.00 (no ITBIS)                   │
│  Stripe (Exterior)             = $    0.00 (no ITBIS)                   │
│                                                                         │
│  TOTAL ITBIS DEDUCIBLE         = $1,980.00                              │
│                                                                         │
│  RESULTADO:                                                             │
│  ──────────────────────────                                             │
│  ITBIS Cobrado                   $1,683.00                              │
│  ITBIS Deducible               - $1,980.00                              │
│  ═════════════════════════════════════════                              │
│  CRÉDITO FISCAL                 ($  297.00)                             │
│                                                                         │
│  ➡️ No hay pago este mes. Crédito se arrastra al siguiente.             │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 7. RETENCIONES (IR-17)

### 7.1 Cuándo OKLA Debe Retener

| Tipo de Pago                     | Retención ISR | Base Legal         |
| -------------------------------- | ------------- | ------------------ |
| Servicios profesionales (RNC)    | 10%           | Art. 309 Ley 11-92 |
| Servicios técnicos (persona)     | 10%           | Art. 309           |
| Alquiler a persona física        | 10%           | Art. 309           |
| Pagos al exterior (sin convenio) | 27%           | Art. 305           |
| Intereses a instituciones        | 10%           | Art. 309           |

### 7.2 Cuándo NO Retener

| Situación                              | Razón                   |
| -------------------------------------- | ----------------------- |
| Proveedor es persona jurídica (SRL/SA) | No aplica retención     |
| Facturas de servicios públicos         | Exentos                 |
| Compras de bienes (no servicios)       | Solo servicios retienen |
| Pagos a Stripe/Digital Ocean           | Exterior (otro régimen) |

### 7.3 Ejemplos de Retenciones OKLA

```
┌─────────────────────────────────────────────────────────────────────────┐
│                EJEMPLOS IR-17 - ENERO 2026                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1. CONTADOR (Persona Física con RNC):                                  │
│     ─────────────────────────────────                                   │
│     Factura:           RD$10,000.00                                     │
│     ITBIS 18%:         RD$ 1,800.00                                     │
│     Retención 10%:     RD$ 1,000.00 (sobre base, NO sobre ITBIS)        │
│     Pagar al contador: RD$10,800.00 (factura + ITBIS - retención)       │
│     Declarar en IR-17: RD$ 1,000.00                                     │
│                                                                         │
│  2. ABOGADO (Persona Física con RNC):                                   │
│     ─────────────────────────────────                                   │
│     Factura:           RD$15,000.00                                     │
│     ITBIS 18%:         RD$ 2,700.00                                     │
│     Retención 10%:     RD$ 1,500.00                                     │
│     Pagar al abogado:  RD$16,200.00                                     │
│     Declarar en IR-17: RD$ 1,500.00                                     │
│                                                                         │
│  3. FREELANCER DISEÑO (Persona Física con RNC):                         │
│     ─────────────────────────────────                                   │
│     Factura:           RD$8,000.00                                      │
│     ITBIS 18%:         RD$1,440.00                                      │
│     Retención 10%:     RD$  800.00                                      │
│     Pagar al freelance:RD$8,640.00                                      │
│     Declarar en IR-17: RD$  800.00                                      │
│                                                                         │
│  4. ALQUILER OFICINA (Persona Física):                                  │
│     ─────────────────────────────────                                   │
│     Alquiler mensual:  RD$25,000.00                                     │
│     ITBIS:             NO APLICA (exento en alquiler vivienda)          │
│     Retención 10%:     RD$ 2,500.00                                     │
│     Pagar al dueño:    RD$22,500.00                                     │
│     Declarar en IR-17: RD$ 2,500.00                                     │
│                                                                         │
│  ═══════════════════════════════════════════════════════════════════    │
│  TOTAL RETENCIONES ENERO: RD$5,800.00                                   │
│  FECHA LÍMITE PAGO IR-17: 10 de Febrero 2026                            │
│                                                                         │
│  ⚠️ IMPORTANTE: La retención se calcula sobre el MONTO BASE,            │
│     NUNCA sobre el ITBIS.                                               │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.4 Retenciones a Pagos al Exterior

```
┌─────────────────────────────────────────────────────────────────────────┐
│              PAGOS AL EXTERIOR - ¿RETENER?                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  🌐 PROVEEDORES INTERNACIONALES DE OKLA:                                │
│                                                                         │
│  ┌────────────────────┬──────────────┬───────────────────────────────┐  │
│  │ Proveedor          │ País         │ Retención Art. 305            │  │
│  ├────────────────────┼──────────────┼───────────────────────────────┤  │
│  │ Digital Ocean      │ USA          │ 27% si aplica (ver nota)      │  │
│  │ GitHub             │ USA          │ 27% si aplica (ver nota)      │  │
│  │ Stripe             │ USA/Irlanda  │ 27% si aplica (ver nota)      │  │
│  │ Google Ads         │ USA/Irlanda  │ 27% si aplica (ver nota)      │  │
│  │ Facebook Ads       │ USA/Irlanda  │ 27% si aplica (ver nota)      │  │
│  │ OpenAI API         │ USA          │ 27% si aplica (ver nota)      │  │
│  │ AWS                │ USA          │ 27% si aplica (ver nota)      │  │
│  └────────────────────┴──────────────┴───────────────────────────────┘  │
│                                                                         │
│  📋 NOTAS IMPORTANTES:                                                  │
│                                                                         │
│  1. Art. 305 Código Tributario indica 27% retención a pagos exterior    │
│                                                                         │
│  2. SIN EMBARGO, en la PRÁCTICA para pequeñas empresas:                 │
│     - Si pagas con tarjeta de crédito dominicana, el banco              │
│       ya reporta la transacción internacional                           │
│     - DGII no ha establecido mecanismo práctico para que               │
│       empresas pequeñas retengan a Google/Stripe/etc.                   │
│     - Estas empresas no aceptan pago con retención                      │
│                                                                         │
│  3. RECOMENDACIÓN PRÁCTICA:                                             │
│     - Reportar en Formato 606 con tipo gasto 05 (exterior)              │
│     - Consultar con contador sobre retención                            │
│     - Mantener documentación de pagos (statements tarjeta)              │
│     - En caso de fiscalización, tener evidencia del pago                │
│                                                                         │
│  4. PROVEEDORES CON CONVENIO PARA EVITAR DOBLE TRIBUTACIÓN:             │
│     - USA tiene convenio con RD (verificar aplicabilidad)               │
│     - Puede reducir retención con documentación W-8BEN-E                │
│                                                                         │
│  ⚠️ CONSULTAR CON ESPECIALISTA EN TRIBUTACIÓN INTERNACIONAL            │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.5 Estructura del IR-17

El IR-17 se presenta en la Oficina Virtual de DGII:

```
Sección A: Retenciones a Asalariados (No aplica si nómina < RD$416,220/año)
Sección B: Retenciones a Terceros (ESTE ES EL DE OKLA)
  - B1: Alquileres
  - B2: Honorarios por servicios
  - B3: Premios
  - B4: Pagos al exterior
  - B5: Otras retenciones

Sección C: Retenciones por dividendos
Sección D: Retenciones en operaciones con instituciones financieras
```

---

## 8. PROCEDIMIENTO MENSUAL COMPLETO

### 8.1 Calendario Mensual

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    PROCEDIMIENTO MENSUAL DGII                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  DÍA 1-5: CIERRE CONTABLE                                               │
│  ├── Verificar todas las facturas de venta (NCF emitidos)              │
│  ├── Verificar todas las facturas de compra (NCF recibidos)            │
│  ├── Conciliar pasarelas de pago (Stripe, AZUL)                        │
│  └── Revisar secuencias NCF usadas                                     │
│                                                                         │
│  DÍA 6-9: PREPARACIÓN DE REPORTES                                       │
│  ├── Generar Formato 606 desde sistema contable                        │
│  ├── Generar Formato 607 desde BillingService                          │
│  ├── Revisar NCF anulados para 608                                     │
│  ├── Calcular retenciones IR-17                                        │
│  └── Validar archivos con validador DGII                               │
│                                                                         │
│  DÍA 10: ENVÍO IR-17                                                    │
│  └── Presentar y pagar retenciones en Oficina Virtual                  │
│                                                                         │
│  DÍA 11-14: VALIDACIÓN FINAL                                            │
│  ├── Corregir errores en formatos                                      │
│  └── Preparar respaldos                                                │
│                                                                         │
│  DÍA 15: ENVÍO FORMATOS                                                 │
│  ├── Enviar Formato 606 a DGII                                         │
│  ├── Enviar Formato 607 a DGII                                         │
│  ├── Enviar Formato 608 a DGII (si hay anulaciones)                    │
│  └── Descargar y archivar acuses de recibo                             │
│                                                                         │
│  DÍA 16-19: CÁLCULO ITBIS                                               │
│  ├── Consolidar ITBIS cobrado                                          │
│  ├── Consolidar ITBIS pagado deducible                                 │
│  └── Calcular monto a pagar o crédito fiscal                           │
│                                                                         │
│  DÍA 20: DECLARACIÓN Y PAGO ITBIS                                       │
│  ├── Presentar IT-1 en Oficina Virtual                                 │
│  └── Pagar si hay monto a favor de DGII                                │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Responsables

| Tarea                  | Responsable        | Backup           |
| ---------------------- | ------------------ | ---------------- |
| Cierre contable        | Contador externo   | Gerente          |
| Generación de formatos | Sistema (auto)     | Contador         |
| Envío a DGII           | Contador           | Gerente          |
| Pago de impuestos      | Gerente            | Socio autorizado |
| Archivo de documentos  | Sistema + Contador | -                |

---

## 9. MANEJO DE EMPLEADOS Y NÓMINA

### 9.1 Obligaciones Laborales de OKLA

```
┌─────────────────────────────────────────────────────────────────────────┐
│                 OBLIGACIONES LABORALES MENSUALES                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  📋 INSTITUCIONES A LAS QUE SE REPORTA:                                 │
│                                                                         │
│  1. TSS (Tesorería de la Seguridad Social)                              │
│     ├── Plataforma: SUIR (Sistema Único de Info. de Recaudación)        │
│     ├── Fecha límite: Día 3 del mes siguiente                           │
│     ├── Contenido: Nómina del mes, novedades                            │
│     └── Pago: AFP, SFS, Riesgos Laborales                               │
│                                                                         │
│  2. DGII (Dirección General de Impuestos Internos)                      │
│     ├── IR-17 Sección A: Retenciones a asalariados (si aplica)          │
│     ├── Fecha límite: Día 10 del mes siguiente                          │
│     └── Solo si algún empleado excede RD$416,220/año                    │
│                                                                         │
│  3. MINISTERIO DE TRABAJO                                               │
│     ├── Reporte DGT-3: Planilla de personal (anual)                     │
│     ├── Fecha límite: Enero de cada año                                 │
│     └── Bonificación: Pago antes del 20 de diciembre                    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 9.2 Aportes a la Seguridad Social (TSS)

```
┌─────────────────────────────────────────────────────────────────────────┐
│               TABLA DE APORTES TSS 2026                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  CONCEPTO              │ EMPLEADOR │ EMPLEADO  │ TOTAL    │ TOPE       │
│  ──────────────────────┼───────────┼───────────┼──────────┼────────────│
│  AFP (Pensiones)       │   7.10%   │   2.87%   │   9.97%  │ 20 SM*     │
│  SFS (Salud)           │   7.09%   │   3.04%   │  10.13%  │ 10 SM*     │
│  Riesgos Laborales     │   1.10%** │    0%     │   1.10%  │ 4 SM*      │
│  INFOTEP               │   1.00%   │    0%     │   1.00%  │ Sin tope   │
│  ──────────────────────┼───────────┼───────────┼──────────┼────────────│
│  TOTAL MÍNIMO          │  16.29%   │   5.91%   │  22.20%  │            │
│                                                                         │
│  *SM = Salario Mínimo ($21,000 sector no sectorizado)                   │
│  **Varía según clasificación de riesgo (1.10% es Clase I - oficinas)    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 9.3 Ejemplo de Nómina OKLA

```
┌─────────────────────────────────────────────────────────────────────────┐
│           EJEMPLO NÓMINA - ENERO 2026 (3 empleados)                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  EMPLEADO 1: Desarrollador Senior                                       │
│  ───────────────────────────────────                                    │
│  Salario Bruto:                    RD$85,000.00                         │
│  (-) AFP Empleado (2.87%):         RD$ 2,439.50                         │
│  (-) SFS Empleado (3.04%):         RD$ 2,584.00                         │
│  (-) ISR Retenido:                 RD$ 6,825.00 (ver tabla ISR)         │
│  ════════════════════════════════════════════                           │
│  Salario Neto:                     RD$73,151.50                         │
│                                                                         │
│  Aportes Patronales:                                                    │
│  (+) AFP Empleador (7.10%):        RD$ 6,035.00                         │
│  (+) SFS Empleador (7.09%):        RD$ 6,026.50                         │
│  (+) Riesgos Lab. (1.10%):         RD$   935.00                         │
│  (+) INFOTEP (1.00%):              RD$   850.00                         │
│  ════════════════════════════════════════════                           │
│  Total Costo Empleador:            RD$98,846.50                         │
│                                                                         │
│  EMPLEADO 2: Diseñador UI/UX                                            │
│  ───────────────────────────────────                                    │
│  Salario Bruto:                    RD$55,000.00                         │
│  (-) AFP Empleado (2.87%):         RD$ 1,578.50                         │
│  (-) SFS Empleado (3.04%):         RD$ 1,672.00                         │
│  (-) ISR Retenido:                 RD$ 1,620.00                         │
│  ════════════════════════════════════════════                           │
│  Salario Neto:                     RD$50,129.50                         │
│                                                                         │
│  Aportes Patronales:                                                    │
│  (+) AFP Empleador (7.10%):        RD$ 3,905.00                         │
│  (+) SFS Empleador (7.09%):        RD$ 3,899.50                         │
│  (+) Riesgos Lab. (1.10%):         RD$   605.00                         │
│  (+) INFOTEP (1.00%):              RD$   550.00                         │
│  ════════════════════════════════════════════                           │
│  Total Costo Empleador:            RD$63,959.50                         │
│                                                                         │
│  EMPLEADO 3: Soporte/Marketing                                          │
│  ───────────────────────────────────                                    │
│  Salario Bruto:                    RD$35,000.00                         │
│  (-) AFP Empleado (2.87%):         RD$ 1,004.50                         │
│  (-) SFS Empleado (3.04%):         RD$ 1,064.00                         │
│  (-) ISR Retenido:                 RD$     0.00 (bajo umbral)           │
│  ════════════════════════════════════════════                           │
│  Salario Neto:                     RD$32,931.50                         │
│                                                                         │
│  Aportes Patronales:                                                    │
│  (+) AFP Empleador (7.10%):        RD$ 2,485.00                         │
│  (+) SFS Empleador (7.09%):        RD$ 2,481.50                         │
│  (+) Riesgos Lab. (1.10%):         RD$   385.00                         │
│  (+) INFOTEP (1.00%):              RD$   350.00                         │
│  ════════════════════════════════════════════                           │
│  Total Costo Empleador:            RD$40,701.50                         │
│                                                                         │
│  ═══════════════════════════════════════════════════════════════════    │
│  RESUMEN MENSUAL:                                                       │
│  Total Salarios Brutos:            RD$175,000.00                        │
│  Total ISR Retenido (IR-3):        RD$  8,445.00 → IR-17 día 10         │
│  Total TSS Empleador:              RD$ 28,507.50 → SUIR día 3           │
│  COSTO TOTAL NÓMINA:               RD$203,507.50                        │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 9.4 Escala ISR para Asalariados 2026

| Ingreso Anual         | Tasa | Cálculo                             |
| --------------------- | ---- | ----------------------------------- |
| Hasta RD$416,220      | 0%   | Exento                              |
| RD$416,220 - $624,329 | 15%  | (Ingreso - 416,220) x 0.15          |
| RD$624,329 - $867,123 | 20%  | 31,216 + (Ingreso - 624,329) x 0.20 |
| Más de RD$867,123     | 25%  | 79,776 + (Ingreso - 867,123) x 0.25 |

### 9.5 Calendario Laboral Mensual

```
┌─────────────────────────────────────────────────────────────────────────┐
│                CALENDARIO OBLIGACIONES LABORALES                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  DÍA 3:  📋 Enviar nómina en SUIR (TSS)                                 │
│          ├── Subir archivo de novedades                                 │
│          └── Generar factura de TSS                                     │
│                                                                         │
│  DÍA 5:  💰 Pagar TSS (AFP, SFS, RL, INFOTEP)                           │
│          ├── Pago en bancos autorizados                                 │
│          └── Guardar comprobante                                        │
│                                                                         │
│  DÍA 10: 📊 Presentar IR-17                                             │
│          ├── Sección A: Retenciones a empleados                         │
│          ├── Sección B: Retenciones a terceros                          │
│          └── Pagar en línea o banco                                     │
│                                                                         │
│  DÍA 15: 📄 Pagar nómina a empleados                                    │
│          ├── Quincenal o según contrato                                 │
│          └── Retener ISR y TSS de nómina                                │
│                                                                         │
│  DÍA 30: 📋 Última quincena (si pago quincenal)                         │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 9.6 Empleados vs Nómina en Formato 606

**⚠️ IMPORTANTE:** Los gastos de nómina (salarios, TSS) **NO** van en el Formato 606.

```
┌─────────────────────────────────────────────────────────────────────────┐
│         ¿QUÉ VA EN 606? vs ¿QUÉ NO VA?                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ❌ NO VA EN FORMATO 606:                                               │
│  ────────────────────────                                               │
│  - Salarios de empleados                                                │
│  - Aportes patronales TSS (AFP, SFS, RL)                                │
│  - Bonificaciones laborales                                             │
│  - Vacaciones, prestaciones                                             │
│  - INFOTEP                                                              │
│                                                                         │
│  ✅ SÍ VA EN FORMATO 606:                                               │
│  ────────────────────────                                               │
│  - Servicios profesionales (contador, abogado)                          │
│  - Freelancers (diseñadores, desarrolladores externos)                  │
│  - Hosting, dominio, software                                           │
│  - Publicidad (Google, Facebook)                                        │
│  - Servicios públicos (luz, internet, agua)                             │
│  - Alquileres                                                           │
│  - Comisiones bancarias                                                 │
│  - Compras de equipos y suministros                                     │
│                                                                         │
│  📋 RAZÓN: La nómina ya se reporta en:                                  │
│  - SUIR (TSS) para seguridad social                                     │
│  - IR-17 Sección A para retenciones ISR a empleados                     │
│  - IR-2 (anual) como gasto deducible                                    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 10. AUTOMATIZACIÓN EN BILLINGSERVICE

### 9.1 Endpoints Requeridos (Por Implementar)

| Endpoint                           | Función                         | Estado       |
| ---------------------------------- | ------------------------------- | ------------ |
| `POST /api/dgii/format/606`        | Generar formato 606 del período | 🔴 Pendiente |
| `POST /api/dgii/format/607`        | Generar formato 607 del período | 🔴 Pendiente |
| `POST /api/dgii/format/608`        | Generar formato 608 del período | 🔴 Pendiente |
| `GET /api/dgii/ncf/sequences`      | Ver secuencias NCF disponibles  | 🟡 Parcial   |
| `POST /api/dgii/ncf/emit`          | Emitir NCF para transacción     | 🟡 Parcial   |
| `GET /api/dgii/itbis/calculate`    | Calcular ITBIS del período      | 🔴 Pendiente |
| `GET /api/dgii/retentions/pending` | Retenciones pendientes          | 🔴 Pendiente |

### 9.2 Tablas de Base de Datos Requeridas

```sql
-- Secuencias NCF
CREATE TABLE ncf_sequences (
    id UUID PRIMARY KEY,
    type VARCHAR(3) NOT NULL,        -- B01, B02, B04, etc.
    current_number BIGINT NOT NULL,
    range_start BIGINT NOT NULL,
    range_end BIGINT NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW()
);

-- NCF Emitidos
CREATE TABLE ncf_issued (
    id UUID PRIMARY KEY,
    ncf VARCHAR(15) NOT NULL UNIQUE,
    type VARCHAR(3) NOT NULL,
    invoice_id UUID NOT NULL,
    customer_rnc VARCHAR(15),
    amount DECIMAL(18,2) NOT NULL,
    itbis DECIMAL(18,2) NOT NULL,
    issue_date TIMESTAMP NOT NULL,
    status VARCHAR(20) DEFAULT 'ISSUED',  -- ISSUED, VOIDED
    voided_date TIMESTAMP NULL,
    voided_reason VARCHAR(100) NULL
);

-- NCF Recibidos (Compras)
CREATE TABLE ncf_received (
    id UUID PRIMARY KEY,
    ncf VARCHAR(15) NOT NULL,
    supplier_rnc VARCHAR(15) NOT NULL,
    supplier_name VARCHAR(200),
    expense_type VARCHAR(2) NOT NULL,  -- 01-11
    amount DECIMAL(18,2) NOT NULL,
    itbis DECIMAL(18,2) NOT NULL,
    itbis_withheld DECIMAL(18,2) DEFAULT 0,
    isr_withheld DECIMAL(18,2) DEFAULT 0,
    issue_date TIMESTAMP NOT NULL,
    payment_date TIMESTAMP,
    payment_method VARCHAR(2)          -- 01=Efectivo, 02=Cheque, etc.
);

-- Formatos generados
CREATE TABLE dgii_formats (
    id UUID PRIMARY KEY,
    format_type VARCHAR(3) NOT NULL,  -- 606, 607, 608
    period_month INT NOT NULL,
    period_year INT NOT NULL,
    file_content TEXT NOT NULL,
    record_count INT NOT NULL,
    total_amount DECIMAL(18,2),
    total_itbis DECIMAL(18,2),
    generated_at TIMESTAMP DEFAULT NOW(),
    submitted_at TIMESTAMP NULL,
    dgii_receipt VARCHAR(100) NULL
);
```

---

## 11. DOCUMENTOS DE RESPALDO

### 11.1 Archivos a Conservar (10 años)

| Documento                 | Formato   | Ubicación |
| ------------------------- | --------- | --------- |
| Facturas emitidas (PDF)   | PDF       | S3 + BD   |
| Facturas recibidas (scan) | PDF       | S3        |
| Formatos 606/607/608      | TXT       | S3 + BD   |
| Acuses DGII               | PDF       | S3        |
| Comprobantes de pago      | PDF       | S3        |
| Libros contables          | Excel/PDF | S3        |
| Estados financieros       | PDF       | S3        |

### 11.2 Estructura de Archivos

```
/fiscal/
├── 2026/
│   ├── 01-enero/
│   │   ├── 606_202601.txt
│   │   ├── 607_202601.txt
│   │   ├── 608_202601.txt
│   │   ├── it1_202601.pdf
│   │   ├── ir17_202601.pdf
│   │   └── acuses/
│   ├── 02-febrero/
│   └── ...
├── facturas_emitidas/
│   ├── 2026/
│   │   ├── B01/
│   │   └── B02/
└── facturas_recibidas/
    └── 2026/
```

---

## 📋 CHECKLIST MENSUAL

```markdown
## Checklist Fiscal Mensual - OKLA S.R.L.

### Semana 1 (Días 1-5)

- [ ] Cerrar período contable anterior
- [ ] Conciliar ventas con pasarelas (Stripe/AZUL)
- [ ] Verificar NCF emitidos vs facturas
- [ ] Verificar facturas de compra recibidas

### Semana 2 (Días 6-15)

- [ ] Generar archivo 606 (Compras)
- [ ] Validar 606 con validador DGII
- [ ] Generar archivo 607 (Ventas)
- [ ] Validar 607 con validador DGII
- [ ] Revisar NCF anulados para 608
- [ ] Calcular retenciones IR-17
- [ ] **DÍA 10:** Enviar y pagar IR-17
- [ ] **DÍA 15:** Enviar 606, 607, 608

### Semana 3 (Días 16-20)

- [ ] Calcular ITBIS del período
- [ ] Determinar monto a pagar o crédito
- [ ] **DÍA 20:** Presentar IT-1
- [ ] **DÍA 20:** Pagar ITBIS (si aplica)

### Archivo

- [ ] Descargar acuses de DGII
- [ ] Archivar en S3/sistema
- [ ] Actualizar registro contable
```

---

**Documento creado:** Enero 25, 2026  
**Próxima revisión:** Cuando cambien regulaciones DGII  
**Responsable:** Contador de OKLA + Gerente
