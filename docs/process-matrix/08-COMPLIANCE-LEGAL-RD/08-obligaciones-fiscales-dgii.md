# 💰 Obligaciones Fiscales DGII - Formatos 606/607/608 - Matriz de Procesos

> **Marco Legal:** Ley 11-92 (Código Tributario) y Ley 253-12 (Comprobantes Fiscales)  
> **Regulador:** Dirección General de Impuestos Internos (DGII)  
> **Última actualización:** Enero 25, 2026  
> **Estado:** 🟢 ACTIVO (Obligatorio)  
> **Estado de Implementación:** 🟡 50% Backend | 🔴 0% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                          | Backend      | UI Access | Observación       |
| -------------------------------- | ------------ | --------- | ----------------- |
| DGII-NCF-001 Generación NCF      | 🟡 Parcial   | 🔴 Falta  | Sin configuración |
| DGII-606-001 Formato Compras     | 🔴 Pendiente | 🔴 Falta  | Sin generador     |
| DGII-607-001 Formato Ventas      | 🔴 Pendiente | 🔴 Falta  | Sin generador     |
| DGII-608-001 Anulaciones         | 🔴 Pendiente | 🔴 Falta  | Sin registro      |
| DGII-ECF-001 Factura Electrónica | 🔴 Pendiente | 🔴 Falta  | Sin integración   |

### Rutas UI Existentes ✅

- Ninguna específica para DGII

### Rutas UI Faltantes 🔴

- `/admin/fiscal/ncf-config` → Configuración de secuencias NCF
- `/admin/fiscal/formato-606` → Generador Formato 606
- `/admin/fiscal/formato-607` → Generador Formato 607
- `/admin/fiscal/formato-608` → Registro de anulaciones
- `/admin/fiscal/dashboard` → Dashboard fiscal

**Verificación Backend:** InvoicingService con NCF básico 🟡

---

## 📊 Resumen de Implementación

| Componente                     | Total | Implementado | Pendiente | Estado         |
| ------------------------------ | ----- | ------------ | --------- | -------------- |
| **DGII-NCF-\*** (Comprobantes) | 5     | 3            | 2         | 🟡 Parcial     |
| **DGII-606-\*** (Compras)      | 4     | 0            | 4         | 🔴 Pendiente   |
| **DGII-607-\*** (Ventas)       | 4     | 0            | 4         | 🔴 Pendiente   |
| **DGII-608-\*** (Anulaciones)  | 3     | 0            | 3         | 🔴 Pendiente   |
| **DGII-ECF-\*** (e-CF)         | 5     | 0            | 5         | 🔴 Pendiente   |
| **DGII-ITBIS-\*** (ITBIS)      | 3     | 2            | 1         | 🟡 Parcial     |
| **Tests**                      | 20    | 8            | 12        | 🟡 Parcial     |
| **TOTAL**                      | 44    | 13           | 31        | 🟡 50% Backend |

---

## 1. Información General

### 1.1 Modelo de Negocio OKLA - Contexto Fiscal

> **OKLA es una plataforma de anuncios clasificados** (similar a SuperCarros.com).
>
> **Ingresos de OKLA (sujetos a ITBIS):**
>
> - Publicación de anuncios individuales: $29/anuncio
> - Suscripciones de dealers: $49-$299/mes
> - Boosts y promociones de publicaciones
>
> **OKLA NO participa** en las transacciones de vehículos. Los dealers y compradores las realizan directamente.
>
> **Todas las obligaciones fiscales DGII SÍ aplican a OKLA** como empresa prestadora de servicios digitales.

### 1.2 Descripción

La DGII requiere que todos los contribuyentes presenten información mensual sobre sus operaciones comerciales. OKLA, como plataforma de servicios digitales, debe cumplir con la emisión de comprobantes fiscales y los reportes mensuales por sus ingresos de publicidad y suscripciones.

### 1.2 Calendario de Obligaciones

| Día del Mes  | Obligación                | Frecuencia |
| ------------ | ------------------------- | ---------- |
| **10**       | IR-17 (Retenciones)       | Mensual    |
| **15**       | Formato 606 (Compras)     | Mensual    |
| **15**       | Formato 607 (Ventas)      | Mensual    |
| **15**       | Formato 608 (Anulaciones) | Mensual    |
| **20**       | IT-1 (ITBIS)              | Mensual    |
| **30 Abril** | IR-2 (ISR Anual)          | Anual      |

### 1.3 Datos de DGII

| Campo               | Valor                                   |
| ------------------- | --------------------------------------- |
| **Nombre**          | Dirección General de Impuestos Internos |
| **Web**             | dgii.gov.do                             |
| **Oficina Virtual** | oficinavirtual.dgii.gov.do              |
| **Teléfono**        | (809) 689-3444                          |
| **Portal NCF**      | dgii.gov.do/ncf                         |

---

## 2. Comprobantes Fiscales (NCF)

### 2.1 Tipos de NCF

| Tipo                        | Código | Descripción            | Uso en OKLA           |
| --------------------------- | ------ | ---------------------- | --------------------- |
| **Factura Crédito Fiscal**  | B01    | Para contribuyentes    | Dealers (empresas)    |
| **Factura de Consumo**      | B02    | Para consumidor final  | Usuarios individuales |
| **Nota de Débito**          | B03    | Aumenta monto factura  | Ajustes               |
| **Nota de Crédito**         | B04    | Reduce monto factura   | Devoluciones          |
| **Compras**                 | B11    | Proveedores informales | Proveedores sin NCF   |
| **Registro Único Ingresos** | B13    | Consolidación ventas   | Ventas menores        |
| **Regímenes Especiales**    | B14    | Exportaciones, ZF      | No aplica             |
| **Gubernamental**           | B15    | Ventas al gobierno     | Si aplica             |
| **e-CF Crédito Fiscal**     | E31    | Electrónica empresas   | Futuro                |
| **e-CF Consumo**            | E32    | Electrónica consumidor | Futuro                |

### 2.2 Estructura del NCF

```
NCF: B0100000001
     ││└──────── Secuencial (8 dígitos)
     │└───────── Tipo de comprobante (01 = Crédito Fiscal)
     └────────── Prefijo obligatorio (B)

e-CF: E310000000001
      │││└──────── Secuencial (10 dígitos)
      ││└───────── Tipo (31 = e-CF Crédito Fiscal)
      │└────────── Serie
      └─────────── Prefijo electrónico (E)
```

### 2.3 Autorización de NCF

**Proceso en DGII:**

1. Solicitar secuencias en Oficina Virtual
2. DGII autoriza rango (ej: B0100000001 - B0100001000)
3. Configurar en sistema
4. Usar en orden secuencial
5. Solicitar más antes de agotar

---

## 3. Procesos de Implementación

### 3.1 DGII-NCF: Gestión de Comprobantes

#### DGII-NCF-001: Generación de NCF

| Campo           | Valor                          |
| --------------- | ------------------------------ |
| **Proceso**     | DGII-NCF-001                   |
| **Nombre**      | Generación Automática de NCF   |
| **Descripción** | Asignar NCF a cada transacción |
| **Estado**      | 🟡 Parcial                     |

**Implementación Actual:**

```csharp
// InvoicingService - NCF básico
public async Task<string> GenerateNCF(NCFType type)
{
    var sequence = await _ncfSequenceRepository.GetNext(type);
    var ncf = $"B{(int)type:D2}{sequence:D8}";
    await _ncfSequenceRepository.IncrementUsed(type);
    return ncf;
}
```

**Falta:**

- UI de configuración de secuencias
- Alertas cuando quedan pocas secuencias
- Validación de NCF en factura

#### DGII-NCF-002: Configuración de Secuencias

| Campo       | Valor                            |
| ----------- | -------------------------------- |
| **Proceso** | DGII-NCF-002                     |
| **Nombre**  | Administración de Secuencias NCF |
| **Ruta**    | `/admin/fiscal/ncf-config`       |
| **Estado**  | 🔴 Pendiente                     |

**UI Propuesta:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│  ⚙️ CONFIGURACIÓN DE SECUENCIAS NCF                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Tipo: B01 - Factura Crédito Fiscal                                     │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Rango autorizado:    B0100000001 - B0100001000                   │  │
│  │ Último usado:        B0100000547                                 │  │
│  │ Disponibles:         453 (45.3%)                                 │  │
│  │ [████████████████████░░░░░░░░░░░░░░░░░░░]                        │  │
│  │                                                                   │  │
│  │ ⚠️ Solicitar más secuencias cuando queden < 100                  │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  Tipo: B02 - Factura de Consumo                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Rango autorizado:    B0200000001 - B0200005000                   │  │
│  │ Último usado:        B0200003821                                 │  │
│  │ Disponibles:         1,179 (23.6%)                               │  │
│  │ [██████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░]                        │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  [+ Agregar Nuevo Rango]   [📊 Ver Historial]                          │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

### 3.2 DGII-606: Formato de Compras

#### DGII-606-001: Generación Formato 606

| Campo           | Valor                                    |
| --------------- | ---------------------------------------- |
| **Proceso**     | DGII-606-001                             |
| **Nombre**      | Generador de Formato 606                 |
| **Descripción** | Reporte mensual de compras a proveedores |
| **Plazo**       | Día 15 del mes siguiente                 |
| **Estado**      | 🔴 Pendiente                             |

**Campos del Formato 606:**

| Campo                         | Descripción                   | Tipo     |
| ----------------------------- | ----------------------------- | -------- |
| RNC/Cédula                    | Identificación del proveedor  | Texto    |
| Tipo ID                       | 1=RNC, 2=Cédula, 3=Pasaporte  | Numérico |
| Tipo Bienes/Servicios         | Código de categoría           | Numérico |
| NCF                           | Comprobante fiscal            | Texto    |
| NCF Modificado                | Si aplica nota crédito/débito | Texto    |
| Fecha Comprobante             | Fecha de la factura           | Fecha    |
| Fecha Pago                    | Fecha de pago efectivo        | Fecha    |
| Monto Servicios               | Valor de servicios            | Numérico |
| Monto Bienes                  | Valor de bienes               | Numérico |
| Monto Total                   | Suma total                    | Numérico |
| ITBIS Facturado               | ITBIS cobrado                 | Numérico |
| ITBIS Retenido                | ITBIS retenido (si aplica)    | Numérico |
| ITBIS Sujeto Proporcionalidad | Si aplica                     | Numérico |
| ITBIS a Costo                 | Si aplica                     | Numérico |
| ITBIS Adelantado              | Si aplica                     | Numérico |
| ISR Retenido                  | Retención ISR (si aplica)     | Numérico |
| Forma Pago                    | 01=Efectivo, 02=Cheque, etc.  | Numérico |

**Categorías de Compras (Tipo Bienes/Servicios):**

| Código | Descripción               |
| ------ | ------------------------- |
| 01     | Gastos de personal        |
| 02     | Gastos por servicios      |
| 03     | Gastos de arrendamiento   |
| 04     | Gastos por activos fijos  |
| 05     | Gastos de representación  |
| 06     | Otras deducciones         |
| 07     | Gastos financieros        |
| 08     | Gastos extraordinarios    |
| 09     | Compras de activos fijos  |
| 10     | Compras de inventario     |
| 11     | Compras de crédito fiscal |

#### DGII-606-002: Exportación TXT

| Campo                 | Valor                      |
| --------------------- | -------------------------- | --- |
| **Proceso**           | DGII-606-002               |
| **Formato de Salida** | TXT delimitado por pipes ( | )   |
| **Estado**            | 🔴 Pendiente               |

**Ejemplo de línea 606:**

```
101234567|1|02|B0100000001||20260115|20260120|5000.00|0.00|5000.00|900.00|0.00|0.00|0.00|0.00|0.00|02
```

---

### 3.3 DGII-607: Formato de Ventas

#### DGII-607-001: Generación Formato 607

| Campo           | Valor                     |
| --------------- | ------------------------- |
| **Proceso**     | DGII-607-001              |
| **Nombre**      | Generador de Formato 607  |
| **Descripción** | Reporte mensual de ventas |
| **Plazo**       | Día 15 del mes siguiente  |
| **Estado**      | 🔴 Pendiente              |

**Campos del Formato 607:**

| Campo                    | Descripción                   | Tipo     |
| ------------------------ | ----------------------------- | -------- |
| RNC/Cédula               | Identificación del cliente    | Texto    |
| Tipo ID                  | 1=RNC, 2=Cédula, 3=Pasaporte  | Numérico |
| NCF                      | Comprobante fiscal emitido    | Texto    |
| NCF Modificado           | Si aplica nota crédito/débito | Texto    |
| Tipo Ingreso             | Código de tipo de ingreso     | Numérico |
| Fecha Comprobante        | Fecha de la factura           | Fecha    |
| Fecha Retención          | Si aplica                     | Fecha    |
| Monto Facturado          | Total facturado               | Numérico |
| ITBIS Facturado          | ITBIS cobrado                 | Numérico |
| ITBIS Retenido Terceros  | Si aplica                     | Numérico |
| ITBIS Percibido          | Si aplica                     | Numérico |
| Retención Renta Terceros | ISR retenido                  | Numérico |
| ISR Percibido            | Si aplica                     | Numérico |
| ISC                      | Impuesto Selectivo Consumo    | Numérico |
| Otros Impuestos          | Si aplican                    | Numérico |
| Monto Propina Legal      | Si aplica                     | Numérico |
| Forma Pago               | 01=Efectivo, 02=Cheque, etc.  | Numérico |

**Tipos de Ingreso:**

| Código | Descripción                   | Uso en OKLA   |
| ------ | ----------------------------- | ------------- |
| 01     | Ingresos por operaciones      | ✅ Comisiones |
| 02     | Ingresos financieros          | Intereses     |
| 03     | Ingresos extraordinarios      | Otros         |
| 04     | Ingresos por arrendamientos   | No aplica     |
| 05     | Ingresos por venta de activos | No aplica     |

#### DGII-607-002: Exportación TXT

| Campo                 | Valor                      |
| --------------------- | -------------------------- | --- |
| **Proceso**           | DGII-607-002               |
| **Formato de Salida** | TXT delimitado por pipes ( | )   |
| **Estado**            | 🔴 Pendiente               |

**Ejemplo de línea 607:**

```
101234567|1|B0200000001||01|20260115||2500.00|450.00|0.00|0.00|0.00|0.00|0.00|0.00|0.00|04
```

---

### 3.4 DGII-608: Formato de Anulaciones

#### DGII-608-001: Registro de Anulaciones

| Campo           | Valor                             |
| --------------- | --------------------------------- |
| **Proceso**     | DGII-608-001                      |
| **Nombre**      | Registro de Comprobantes Anulados |
| **Descripción** | Reportar NCF anulados             |
| **Plazo**       | Día 15 del mes siguiente          |
| **Estado**      | 🔴 Pendiente                      |

**Campos del Formato 608:**

| Campo          | Descripción                            |
| -------------- | -------------------------------------- |
| NCF            | Comprobante anulado                    |
| Fecha          | Fecha de anulación                     |
| Tipo Anulación | 01=Deterioro, 02=Error impresión, etc. |

**Tipos de Anulación:**

| Código | Descripción                       |
| ------ | --------------------------------- |
| 01     | Deterioro de factura pre-impresa  |
| 02     | Error de impresión                |
| 03     | Impresión defectuosa              |
| 04     | Factura duplicada                 |
| 05     | Factura no corresponde al cliente |
| 06     | Otros                             |

---

### 3.5 DGII-ECF: Factura Electrónica

#### DGII-ECF-001: Integración e-CF

| Campo           | Valor                             |
| --------------- | --------------------------------- |
| **Proceso**     | DGII-ECF-001                      |
| **Nombre**      | Facturación Electrónica DGII      |
| **Descripción** | Integrar con sistema e-CF de DGII |
| **Estado**      | 🔴 Pendiente                      |

**Requisitos para e-CF:**

| Requisito            | Descripción                  |
| -------------------- | ---------------------------- |
| Certificado digital  | Firma electrónica autorizada |
| Software certificado | Homologado por DGII          |
| Conexión API         | WebService DGII              |
| Almacenamiento       | 10 años con integridad       |

**Flujo de e-CF:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    FLUJO DE FACTURA ELECTRÓNICA                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1️⃣ OKLA genera factura                                                │
│   ├── Datos del cliente                                                 │
│   ├── Items facturados                                                  │
│   └── Cálculo de ITBIS                                                  │
│                                                                         │
│   2️⃣ Firma electrónica                                                  │
│   ├── Hash del documento                                                │
│   ├── Firma con certificado                                             │
│   └── Timestamp certificado                                             │
│                                                                         │
│   3️⃣ Envío a DGII                                                       │
│   ├── POST a WebService                                                 │
│   ├── Validación automática                                             │
│   └── Respuesta con e-NCF                                               │
│                                                                         │
│   4️⃣ Almacenamiento                                                     │
│   ├── XML firmado                                                       │
│   ├── Respuesta DGII                                                    │
│   └── PDF para cliente                                                  │
│                                                                         │
│   5️⃣ Envío al cliente                                                   │
│   ├── Email con PDF                                                     │
│   ├── Link de descarga                                                  │
│   └── Código de verificación                                            │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Dashboard Fiscal

### 4.1 UI Principal

**Ruta:** `/admin/fiscal/dashboard`

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📊 DASHBOARD FISCAL - Enero 2026                                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  📅 PRÓXIMOS VENCIMIENTOS                                               │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ ⏰ Día 10 (5 días) - IR-17 Retenciones         [Ver Detalle]     │  │
│  │ ⏰ Día 15 (10 días) - Formatos 606/607/608    [Generar]          │  │
│  │ ⏰ Día 20 (15 días) - IT-1 ITBIS              [Ver Detalle]      │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  💰 RESUMEN DEL MES                                                     │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐           │
│  │ VENTAS          │ │ COMPRAS         │ │ ITBIS A PAGAR   │           │
│  │ RD$ 2,450,000   │ │ RD$ 850,000     │ │ RD$ 288,000     │           │
│  │ 245 facturas    │ │ 42 facturas     │ │ (ITBIS V - C)   │           │
│  └─────────────────┘ └─────────────────┘ └─────────────────┘           │
│                                                                         │
│  📄 COMPROBANTES EMITIDOS                                               │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ B01 Crédito Fiscal:     45  │ B04 Notas Crédito:    3           │  │
│  │ B02 Consumo:           200  │ B14 Especiales:       0           │  │
│  │ B03 Notas Débito:        2  │ Anulados:             5           │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  [Generar 606]  [Generar 607]  [Generar 608]  [Ver IT-1]               │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Alertas Automáticas

| Alerta          | Trigger              | Acción                  |
| --------------- | -------------------- | ----------------------- |
| 📧 Email día 8  | 2 días antes IR-17   | Recordatorio            |
| 📧 Email día 12 | 3 días antes 606/607 | Recordatorio + Borrador |
| 📧 Email día 17 | 3 días antes ITBIS   | Recordatorio            |
| 🚨 Urgente      | Día del vencimiento  | Alerta crítica          |

---

## 5. Endpoints API

### 5.1 NCFController

| Método | Endpoint                         | Descripción             | Auth   |
| ------ | -------------------------------- | ----------------------- | ------ |
| `GET`  | `/api/fiscal/ncf/sequences`      | Ver secuencias activas  | Admin  |
| `POST` | `/api/fiscal/ncf/sequences`      | Agregar nueva secuencia | Admin  |
| `GET`  | `/api/fiscal/ncf/next/{type}`    | Obtener próximo NCF     | System |
| `GET`  | `/api/fiscal/ncf/validate/{ncf}` | Validar NCF             | Admin  |

### 5.2 FormatsController

| Método | Endpoint                   | Descripción       | Auth  |
| ------ | -------------------------- | ----------------- | ----- |
| `GET`  | `/api/fiscal/606/preview`  | Vista previa 606  | Admin |
| `GET`  | `/api/fiscal/606/download` | Descargar TXT 606 | Admin |
| `GET`  | `/api/fiscal/607/preview`  | Vista previa 607  | Admin |
| `GET`  | `/api/fiscal/607/download` | Descargar TXT 607 | Admin |
| `GET`  | `/api/fiscal/608/preview`  | Vista previa 608  | Admin |
| `GET`  | `/api/fiscal/608/download` | Descargar TXT 608 | Admin |

### 5.3 TaxController

| Método | Endpoint                    | Descripción           | Auth  |
| ------ | --------------------------- | --------------------- | ----- |
| `GET`  | `/api/fiscal/itbis/summary` | Resumen ITBIS del mes | Admin |
| `GET`  | `/api/fiscal/dashboard`     | Dashboard fiscal      | Admin |
| `GET`  | `/api/fiscal/deadlines`     | Próximos vencimientos | Admin |

---

## 6. Servicio Backend

```csharp
// FiscalService.cs
public interface IFiscalService
{
    // NCF
    Task<string> GenerateNCF(NCFType type);
    Task<NCFSequenceInfo[]> GetSequences();
    Task AddSequence(NCFType type, string from, string to);

    // Formatos
    Task<Format606Result> Generate606(int year, int month);
    Task<Format607Result> Generate607(int year, int month);
    Task<Format608Result> Generate608(int year, int month);
    Task<byte[]> ExportToTxt(FormatType type, int year, int month);

    // ITBIS
    Task<ITBISSummary> GetITBISSummary(int year, int month);
    Task<TaxDeadline[]> GetUpcomingDeadlines();

    // e-CF
    Task<ECFResult> SendToDigii(Invoice invoice);
    Task<ECFStatus> CheckStatus(string encf);
}
```

---

## 7. Cronograma de Implementación

### Fase 1: Q1 2026 - NCF Mejorado 🟡

- [x] Generación básica de NCF
- [ ] UI de configuración de secuencias
- [ ] Alertas de secuencias bajas
- [ ] Validación de NCF

### Fase 2: Q1 2026 - Formatos 606/607/608 🔴

- [ ] Backend para generación de formatos
- [ ] Exportación a TXT
- [ ] UI de vista previa
- [ ] Dashboard fiscal

### Fase 3: Q2 2026 - Automatización 🔴

- [ ] Recordatorios automáticos
- [ ] Generación programada
- [ ] Validación de datos

### Fase 4: Q3 2026 - e-CF 🔴

- [ ] Obtener certificado digital
- [ ] Integrar API DGII
- [ ] Almacenamiento de XML
- [ ] Envío automático

---

## 8. Sanciones por Incumplimiento

| Infracción             | Sanción                  |
| ---------------------- | ------------------------ |
| No presentar 606/607   | 5-30 salarios mínimos    |
| Presentación tardía    | 2-10 salarios mínimos    |
| Información incorrecta | 5-30 salarios mínimos    |
| No emitir NCF          | 10-50 salarios mínimos   |
| Evasión fiscal         | 2-6 años de prisión      |
| NCF falso              | 2-6 años de prisión      |
| Mora en pago ITBIS     | 10% + 4% mensual interés |

---

## 9. Referencias

| Recurso              | URL                        |
| -------------------- | -------------------------- |
| Oficina Virtual DGII | oficinavirtual.dgii.gov.do |
| Manual de Formatos   | dgii.gov.do/formatosEnvio  |
| Ley 11-92            | congreso.gob.do            |
| Ley 253-12           | dgii.gov.do/ley253         |
| Norma 06-2018 (e-CF) | dgii.gov.do/ecf            |
| Portal NCF           | dgii.gov.do/ncf            |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Febrero 25, 2026  
**Responsable:** Equipo de Contabilidad + Desarrollo OKLA  
**Prioridad:** 🔴 ALTA (Vencimientos mensuales obligatorios)
