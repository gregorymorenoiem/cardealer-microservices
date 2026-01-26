# 📄 NCF - Comprobantes Fiscales Electrónicos - Matriz de Procesos

> **Marco Legal:** Norma General 06-2018, Ley 11-92  
> **Regulador:** Dirección General de Impuestos Internos (DGII)  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** 🟡 50% Backend | 🔴 10% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                      | Backend      | UI Access | Observación   |
| ---------------------------- | ------------ | --------- | ------------- |
| NCF-EMIT-001 Emisión factura | 🟡 Parcial   | 🔴 Falta  | Sin UI admin  |
| NCF-VALID-001 Validación     | ✅ API DGII  | 🔴 Falta  | Solo backend  |
| NCF-REPORT-001 Reportes      | 🔴 Pendiente | 🔴 Falta  | Sin dashboard |
| NCF-CREDIT-001 Nota crédito  | 🔴 Pendiente | 🔴 Falta  | Sin UI        |

### Rutas UI Existentes ✅

- Ninguna específica para NCF

### Rutas UI Faltantes 🔴

- `/admin/fiscal/ncf-config` → Configuración de NCF
- `/admin/fiscal/invoices` → Facturas emitidas
- `/admin/fiscal/credit-notes` → Notas de crédito
- `/billing/invoice/:id` → Factura del usuario

---

## 📊 Resumen de Implementación

| Componente                        | Total | Implementado | Pendiente | Estado         |
| --------------------------------- | ----- | ------------ | --------- | -------------- |
| **NCF-EMIT-\*** (Emisión)         | 5     | 2            | 3         | 🟡 Parcial     |
| **NCF-VALID-\*** (Validación)     | 3     | 2            | 1         | 🟡 Parcial     |
| **NCF-REPORT-\*** (Reportes)      | 4     | 0            | 4         | 🔴 Pendiente   |
| **NCF-CREDIT-\*** (Notas Crédito) | 4     | 0            | 4         | 🔴 Pendiente   |
| **NCF-SEQUENCE-\*** (Secuencias)  | 3     | 1            | 2         | 🟡 Parcial     |
| **Tests**                         | 15    | 4            | 11        | 🟡 Parcial     |
| **TOTAL**                         | 34    | 9            | 25        | 🟡 30% Backend |

---

## 1. Información General

### 1.0 Modelo de Negocio OKLA (Contexto para NCF)

> **IMPORTANTE:** OKLA S.R.L. (RNC: 1-33-32590-1) es una **plataforma de anuncios clasificados** de vehículos, similar a SuperCarros.com.
>
> **Servicios que OKLA factura (con NCF + ITBIS 18%):**
>
> - Publicación de anuncios individuales: $29 + ITBIS
> - Suscripciones mensuales para dealers: $49-$299/mes + ITBIS
> - Boosts y promociones de publicaciones
>
> **OKLA NO participa en transacciones de vehículos.** Los compradores pagan directamente a los dealers/vendedores.

### 1.1 Tipos de NCF Utilizados por OKLA

#### Para VENTAS (Servicios de Publicidad/Suscripciones)

| NCF     | Tipo                 | Uso en OKLA                         | Obligatorio | Estado          |
| ------- | -------------------- | ----------------------------------- | ----------- | --------------- |
| **B01** | Crédito Fiscal       | Dealers y empresas **con RNC**      | ✅ Sí       | 🟡 Parcial      |
| **B02** | Consumidor Final     | Individuos **sin RNC** o con cédula | ✅ Sí       | ✅ Implementado |
| **B04** | Nota de Crédito      | Devoluciones, correcciones          | ✅ Sí       | 🔴 Pendiente    |
| **B15** | Gubernamental        | Si vende a entidades de gobierno    | ⚠️ Si surge | 🔴 N/A          |
| B14     | Regímenes Especiales | Zonas francas                       | ❌ No       | N/A             |

#### Para COMPRAS (Gastos de OKLA)

| NCF     | Tipo                | Uso en OKLA                      | Obligatorio |
| ------- | ------------------- | -------------------------------- | ----------- |
| **B11** | Comprobante Compras | Proveedores informales sin NCF   | ✅ Sí       |
| **B13** | Gastos del Exterior | Stripe, AWS, Digital Ocean, etc. | ✅ Sí       |

#### Flujo de Selección de NCF

```
Cliente solicita factura
        │
        ▼
  ¿Tiene RNC válido?
   /            \
  SÍ             NO
   │              │
   ▼              ▼
¿Es gobierno?   Emitir B02
   │           (Consumo)
  / \
 SÍ  NO
  │   │
  ▼   ▼
B15  B01
```

### 1.2 Formato de NCF

```
B0100000001
│ ││└──────── Secuencia (8 dígitos)
│ │└───────── Tipo (01=Fiscal, 02=Consumidor, 04=Crédito)
│ └────────── Serie (siempre 0 para e-CF)
└──────────── Letra B (Comprobante Fiscal)
```

### 1.3 e-CF (Comprobante Fiscal Electrónico)

A partir de 2024, DGII implementó e-CF. OKLA debe migrar a este formato:

| Característica | NCF Tradicional        | e-CF               |
| -------------- | ---------------------- | ------------------ |
| Formato        | Papel/Digital          | XML firmado        |
| Validación     | Post-emisión           | Tiempo real        |
| Autorización   | Secuencia pre-asignada | Por transacción    |
| Consulta       | 606/607 mensual        | API en tiempo real |

---

## 2. Procesos de Implementación

### 2.1 NCF-EMIT: Emisión de Comprobantes

#### NCF-EMIT-001: Factura de Suscripción Dealer

| Campo       | Valor                          |
| ----------- | ------------------------------ |
| **Proceso** | NCF-EMIT-001                   |
| **Trigger** | Pago de suscripción exitoso    |
| **NCF**     | B01 (si RNC) o B02 (si cédula) |
| **Estado**  | 🟡 Parcial                     |

**Datos de la Factura:**

| Campo    | Valor                                     |
| -------- | ----------------------------------------- |
| Emisor   | OKLA SRL, RNC 1-32-XXXXX-X                |
| Receptor | Dealer (RNC o Cédula)                     |
| Concepto | Suscripción Plan [Starter/Pro/Enterprise] |
| Monto    | Precio del plan                           |
| ITBIS    | 18%                                       |
| Total    | Monto + ITBIS                             |

**Ejemplo de Factura:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           OKLA SRL                                      │
│                    RNC: 1-32-XXXXX-X                                    │
│                    Av. Winston Churchill #123                           │
│                    Santo Domingo, RD                                    │
│                                                                         │
│  ════════════════════════════════════════════════════════════════════  │
│                        FACTURA DE CRÉDITO FISCAL                        │
│                          NCF: B0100000127                               │
│  ════════════════════════════════════════════════════════════════════  │
│                                                                         │
│  Cliente: AutoMax Dealer                                                │
│  RNC: 1-31-XXXXX-X                                                      │
│  Dirección: Calle El Conde #456, Santiago                              │
│                                                                         │
│  Fecha de emisión: 25/01/2026                                           │
│  Fecha de vencimiento: 25/01/2026                                       │
│  Condición de pago: Contado                                             │
│                                                                         │
│  ──────────────────────────────────────────────────────────────────────│
│  DESCRIPCIÓN                    CANT.   P.UNIT      SUBTOTAL           │
│  ──────────────────────────────────────────────────────────────────────│
│  Suscripción Plan Pro              1    $129.00      $129.00           │
│  (Período: Febrero 2026)                                               │
│  ──────────────────────────────────────────────────────────────────────│
│                                          Subtotal:    $129.00           │
│                                          ITBIS 18%:    $23.22           │
│                                          ─────────────────────          │
│                                          TOTAL:       $152.22           │
│  ══════════════════════════════════════════════════════════════════════│
│                                                                         │
│  Método de pago: Tarjeta de crédito ****4532                           │
│                                                                         │
│  Esta factura cumple con los requisitos de la DGII.                    │
│  Validez fiscal: https://dgii.gov.do/ncf/consulta                      │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

#### NCF-EMIT-002: Factura de Boost/Destacado

| Campo       | Valor                         |
| ----------- | ----------------------------- |
| **Proceso** | NCF-EMIT-002                  |
| **Trigger** | Compra de boost exitosa       |
| **NCF**     | B02 (consumidor final típico) |
| **Estado**  | 🟡 Parcial                    |

#### NCF-EMIT-003: Factura de Comisión

| Campo       | Valor                         |
| ----------- | ----------------------------- |
| **Proceso** | NCF-EMIT-003                  |
| **Trigger** | Venta completada con comisión |
| **NCF**     | B01 (si dealer con RNC)       |
| **Estado**  | 🔴 Pendiente                  |

---

### 2.2 NCF-VALID: Validación

#### NCF-VALID-001: Validación de RNC del Cliente

| Campo             | Valor                        |
| ----------------- | ---------------------------- |
| **Proceso**       | NCF-VALID-001                |
| **Endpoint DGII** | api.dgii.gov.do/rnc/validate |
| **Estado**        | ✅ Implementado              |

**Validaciones:**

| Check               | Descripción                      | Automático |
| ------------------- | -------------------------------- | ---------- |
| Formato RNC         | 9-11 dígitos, dígito verificador | ✅         |
| Estado RNC          | Activo en DGII                   | ✅ API     |
| Nombre/Razón Social | Coincide con registro            | ✅ API     |
| Estado tributario   | Sin deudas (opcional)            | 🟡         |

#### NCF-VALID-002: Validación de Secuencia NCF

| Campo       | Valor                          |
| ----------- | ------------------------------ |
| **Proceso** | NCF-VALID-002                  |
| **Nombre**  | Verificar secuencia disponible |
| **Estado**  | ✅ Implementado                |

```csharp
public async Task<string> GetNextNcf(NcfType type)
{
    var sequence = await _context.NcfSequences
        .Where(s => s.Type == type && s.IsActive)
        .FirstOrDefaultAsync();

    if (sequence.CurrentNumber >= sequence.MaxNumber)
    {
        // Alertar: secuencia por agotarse
        await _alertService.SendNcfSequenceAlert(sequence);
    }

    sequence.CurrentNumber++;
    await _context.SaveChangesAsync();

    return FormatNcf(type, sequence.CurrentNumber);
}
```

---

### 2.3 NCF-CREDIT: Notas de Crédito

#### NCF-CREDIT-001: Emisión por Devolución

| Campo       | Valor              |
| ----------- | ------------------ |
| **Proceso** | NCF-CREDIT-001     |
| **NCF**     | B04                |
| **Trigger** | Reembolso aprobado |
| **Estado**  | 🔴 Pendiente       |

**Flujo:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    EMISIÓN DE NOTA DE CRÉDITO                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1️⃣ Reembolso aprobado                                                 │
│   └── Sistema verifica factura original                                │
│                                                                         │
│   2️⃣ Generar Nota de Crédito                                            │
│   ├── NCF: B04xxxxxxxx                                                  │
│   ├── Referencia: NCF de factura original                              │
│   ├── Monto: Igual o menor a factura original                          │
│   └── ITBIS: Proporcional al monto                                     │
│                                                                         │
│   3️⃣ Enviar a cliente                                                   │
│   ├── Email con PDF adjunto                                            │
│   └── Disponible en /billing/credit-notes/:id                          │
│                                                                         │
│   4️⃣ Registrar para reportes DGII                                       │
│   └── Incluir en Formato 607 del mes                                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

#### NCF-CREDIT-002: Corrección de Factura

| Campo       | Valor                     |
| ----------- | ------------------------- |
| **Proceso** | NCF-CREDIT-002            |
| **Trigger** | Error en datos de factura |
| **Estado**  | 🔴 Pendiente              |

**Casos de Corrección:**

| Error                             | Solución                        |
| --------------------------------- | ------------------------------- |
| RNC incorrecto                    | Nota de crédito + nueva factura |
| Monto incorrecto (cobro excesivo) | Nota de crédito parcial         |
| Monto incorrecto (cobro menor)    | Factura adicional               |
| Descripción incorrecta            | Nota de crédito + refacturar    |

---

### 2.4 NCF-SEQUENCE: Gestión de Secuencias

#### NCF-SEQUENCE-001: Control de Secuencias

| Campo       | Valor            |
| ----------- | ---------------- |
| **Proceso** | NCF-SEQUENCE-001 |
| **Tabla**   | ncf_sequences    |
| **Estado**  | 🟡 Parcial       |

**Modelo de Datos:**

```csharp
public class NcfSequence
{
    public Guid Id { get; set; }
    public NcfType Type { get; set; }           // B01, B02, B04
    public long CurrentNumber { get; set; }      // Último usado
    public long MinNumber { get; set; }          // Inicio de rango autorizado
    public long MaxNumber { get; set; }          // Fin de rango autorizado
    public DateTime AuthorizedUntil { get; set; } // Fecha de vencimiento
    public bool IsActive { get; set; }
    public string AuthorizationCode { get; set; } // Código DGII
    public DateTime CreatedAt { get; set; }
}
```

#### NCF-SEQUENCE-002: Alertas de Agotamiento

| Campo       | Valor                   |
| ----------- | ----------------------- |
| **Proceso** | NCF-SEQUENCE-002        |
| **Trigger** | Secuencia al 80% de uso |
| **Estado**  | 🔴 Pendiente            |

**Alertas:**

| % Uso | Alerta  | Destinatario              |
| ----- | ------- | ------------------------- |
| 80%   | Warning | Finanzas                  |
| 90%   | Urgente | Finanzas + Admin          |
| 95%   | Crítico | Todos + Bloqueo de ventas |

---

## 3. Endpoints API

### 3.1 InvoiceController (Interno)

| Método | Endpoint                              | Descripción          | Auth     | Estado |
| ------ | ------------------------------------- | -------------------- | -------- | ------ |
| `POST` | `/api/internal/invoices/generate`     | Generar factura      | Internal | 🟡     |
| `GET`  | `/api/internal/invoices/:id`          | Obtener factura      | Internal | ✅     |
| `POST` | `/api/internal/credit-notes/generate` | Generar nota crédito | Internal | 🔴     |

### 3.2 BillingController (Usuario)

| Método | Endpoint                        | Descripción        | Auth | Estado |
| ------ | ------------------------------- | ------------------ | ---- | ------ |
| `GET`  | `/api/billing/invoices`         | Mis facturas       | ✅   | 🟡     |
| `GET`  | `/api/billing/invoices/:id`     | Detalle de factura | ✅   | 🟡     |
| `GET`  | `/api/billing/invoices/:id/pdf` | Descargar PDF      | ✅   | 🔴     |

### 3.3 Admin FiscalController

| Método | Endpoint                          | Descripción         | Auth  | Estado |
| ------ | --------------------------------- | ------------------- | ----- | ------ |
| `GET`  | `/api/admin/fiscal/ncf-sequences` | Ver secuencias      | Admin | 🔴     |
| `POST` | `/api/admin/fiscal/ncf-sequences` | Agregar secuencia   | Admin | 🔴     |
| `GET`  | `/api/admin/fiscal/invoices`      | Todas las facturas  | Admin | 🔴     |
| `GET`  | `/api/admin/fiscal/reports/606`   | Generar Formato 606 | Admin | 🔴     |
| `GET`  | `/api/admin/fiscal/reports/607`   | Generar Formato 607 | Admin | 🔴     |

---

## 4. UI de Administración

### 4.1 Dashboard Fiscal

**Ruta:** `/admin/fiscal`

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📊 DASHBOARD FISCAL - Enero 2026                                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐           │
│  │ FACTURAS MES    │ │ ITBIS COBRADO   │ │ NOTAS CRÉDITO   │           │
│  │      156        │ │   $28,500       │ │      12         │           │
│  │ Total: $158,333 │ │ +15% vs mes ant │ │ Total: $3,200   │           │
│  └─────────────────┘ └─────────────────┘ └─────────────────┘           │
│                                                                         │
│  SECUENCIAS NCF                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Tipo  │ Rango              │ Usado    │ Disponible │ Estado      │  │
│  │───────│────────────────────│──────────│────────────│─────────────│  │
│  │ B01   │ 00001 - 01000      │ 127      │ 873 (87%)  │ ✅ OK       │  │
│  │ B02   │ 00001 - 05000      │ 2,456    │ 2,544 (51%)│ ✅ OK       │  │
│  │ B04   │ 00001 - 00500      │ 89       │ 411 (82%)  │ ✅ OK       │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  PRÓXIMAS OBLIGACIONES                                                  │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ 📅 10 Feb - Declaración IR-17 (Retenciones)                      │  │
│  │ 📅 15 Feb - Envío Formato 606 y 607                              │  │
│  │ 📅 20 Feb - Pago ITBIS                                           │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  [📥 Generar 606]  [📥 Generar 607]  [📥 Generar 608]                  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Configuración de Secuencias

**Ruta:** `/admin/fiscal/ncf-config`

```
┌─────────────────────────────────────────────────────────────────────────┐
│  ⚙️ CONFIGURACIÓN DE SECUENCIAS NCF                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Agregar Nueva Secuencia                                                │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Tipo de NCF:        [▼ B01 - Crédito Fiscal            ]         │  │
│  │ Número inicial:     [                                  ]         │  │
│  │ Número final:       [                                  ]         │  │
│  │ Código autorización:[                                  ]         │  │
│  │ Válido hasta:       [     /     /        ]                       │  │
│  │                                          [Agregar Secuencia]     │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  Secuencias Activas                                                     │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ B01 │ 00001-01000 │ Usado: 127 │ Vence: 31/12/2026 │ [Desactivar]│  │
│  │ B02 │ 00001-05000 │ Usado: 2456│ Vence: 31/12/2026 │ [Desactivar]│  │
│  │ B04 │ 00001-00500 │ Usado: 89  │ Vence: 31/12/2026 │ [Desactivar]│  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  📌 Nota: Las secuencias se solicitan a la DGII a través de la         │
│  Oficina Virtual (https://dgii.gov.do) o en oficinas físicas.          │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Factura para Usuario

### 5.1 Mis Facturas

**Ruta:** `/billing/invoices`

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📄 MIS FACTURAS                                                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Factura #B0200002456          │ 25/01/2026  │ $152.22 │ [📥 PDF] │  │
│  │ Suscripción Plan Pro          │ Pagada ✅   │         │ [Ver]   │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Factura #B0200002301          │ 25/12/2025  │ $152.22 │ [📥 PDF] │  │
│  │ Suscripción Plan Pro          │ Pagada ✅   │         │ [Ver]   │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Factura #B0200002150          │ 25/11/2025  │ $57.82  │ [📥 PDF] │  │
│  │ Boost Premium                 │ Pagada ✅   │         │ [Ver]   │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ¿Necesitas factura con NCF de crédito fiscal (B01)?                    │
│  Contacta a facturacion@okla.com.do con tu RNC.                        │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Cronograma de Implementación

### Fase 1: Q1 2026 - Base ✅

- [x] Generación de NCF B02
- [x] Validación de RNC con API DGII
- [x] Secuencias básicas

### Fase 2: Q1 2026 - Mejoras 🟡

- [ ] UI de facturas para usuario
- [ ] PDF de factura
- [ ] NCF B01 para empresas
- [ ] Dashboard fiscal admin

### Fase 3: Q2 2026 - Completo 🔴

- [ ] Notas de crédito B04
- [ ] Generación de Formato 606/607
- [ ] Alertas de secuencia
- [ ] Migración a e-CF

### Fase 4: Q3 2026 - e-CF 🔴

- [ ] Integración con API e-CF de DGII
- [ ] Firma digital de comprobantes
- [ ] Validación en tiempo real
- [ ] Consulta de estatus

---

## 7. Referencias

| Documento                        | Ubicación              |
| -------------------------------- | ---------------------- |
| Norma General 06-2018            | dgii.gov.do            |
| Manual e-CF                      | dgii.gov.do/ecf        |
| 08-obligaciones-fiscales-dgii.md | 08-COMPLIANCE-LEGAL-RD |
| BillingService                   | backend/BillingService |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Febrero 25, 2026  
**Responsable:** Equipo Finanzas + Desarrollo OKLA  
**Prioridad:** 🔴 ALTA (Obligación fiscal)
