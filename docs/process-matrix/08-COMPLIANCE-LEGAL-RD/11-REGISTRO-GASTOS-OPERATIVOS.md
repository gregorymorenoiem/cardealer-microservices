# 📊 Procedimiento de Registro de Gastos Operativos - OKLA S.R.L.

> **Empresa:** OKLA S.R.L.  
> **RNC:** 1-33-32590-1  
> **Fecha de Creación:** Enero 25, 2026  
> **Propósito:** Establecer procedimientos de registro de gastos para cumplimiento DGII

---

## 📋 RESUMEN EJECUTIVO

Este documento define los procedimientos para registrar TODOS los gastos operativos de OKLA de forma que:

1. ✅ Se cumplan las obligaciones con DGII
2. ✅ Se generen reportes automáticamente (606, 607, 608, IT-1)
3. ✅ Se tenga información lista para auditorías
4. ✅ Se optimice la carga tributaria legalmente

---

## 1. CLASIFICACIÓN DE PROVEEDORES

### 1.1 Proveedores Internacionales (Sin RNC)

Estos proveedores **NO tienen RNC** y están fuera de República Dominicana:

```
┌─────────────────────────────────────────────────────────────────────────┐
│               PROVEEDORES INTERNACIONALES DE OKLA                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  CATEGORÍA         │ PROVEEDOR       │ PAÍS       │ PAGO TÍPICO         │
│  ─────────────────────────────────────────────────────────────────────  │
│                                                                         │
│  🖥️ HOSTING/CLOUD                                                       │
│  ├── Digital Ocean   │ USA            │ Tarjeta de crédito              │
│  ├── AWS             │ USA            │ Tarjeta de crédito              │
│  ├── Google Cloud    │ USA            │ Tarjeta de crédito              │
│  └── Cloudflare      │ USA            │ Tarjeta de crédito              │
│                                                                         │
│  💻 DESARROLLO                                                          │
│  ├── GitHub          │ USA            │ Tarjeta de crédito              │
│  ├── JetBrains       │ República Checa│ Tarjeta de crédito              │
│  ├── Postman         │ USA            │ Tarjeta de crédito              │
│  └── VS Code (gratis)│ USA            │ -                               │
│                                                                         │
│  💳 PASARELAS DE PAGO                                                   │
│  ├── Stripe          │ USA/Irlanda    │ Descuento automático            │
│  └── PayPal          │ USA            │ Descuento automático            │
│                                                                         │
│  📢 PUBLICIDAD DIGITAL                                                  │
│  ├── Google Ads      │ USA/Irlanda    │ Tarjeta de crédito              │
│  ├── Facebook Ads    │ USA/Irlanda    │ Tarjeta de crédito              │
│  ├── Instagram Ads   │ USA            │ Tarjeta de crédito              │
│  ├── TikTok Ads      │ Singapur       │ Tarjeta de crédito              │
│  └── LinkedIn Ads    │ USA            │ Tarjeta de crédito              │
│                                                                         │
│                                                                         │
│  📧 COMUNICACIONES                                                      │
│  ├── Twilio          │ USA            │ Tarjeta/Descuento               │
│  ├── SendGrid        │ USA            │ Tarjeta de crédito              │
│  └── Mailchimp       │ USA            │ Tarjeta de crédito              │
│                                                                         │
│  🤖 INTELIGENCIA ARTIFICIAL                                             │
│  ├── OpenAI (ChatGPT)│ USA            │ Tarjeta de crédito              │
│  ├── Anthropic Claude│ USA            │ Tarjeta de crédito              │
│  └── Google AI       │ USA            │ Tarjeta de crédito              │
│                                                                         │
│  📊 SOFTWARE/HERRAMIENTAS                                               │
│  ├── Microsoft 365   │ USA            │ Tarjeta de crédito              │
│  ├── Adobe CC        │ USA            │ Tarjeta de crédito              │
│  ├── Figma           │ USA            │ Tarjeta de crédito              │
│  ├── Notion          │ USA            │ Tarjeta de crédito              │
│  └── Slack           │ USA            │ Tarjeta de crédito              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Proveedores Locales (Con RNC)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                 PROVEEDORES LOCALES DE OKLA                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  CATEGORÍA           │ PROVEEDOR          │ RNC        │ NCF            │
│  ─────────────────────────────────────────────────────────────────────  │
│                                                                         │
│  💳 PASARELAS LOCALES                                                   │
│  └── AZUL Banco Popular│ 101-00001-1     │ Emite B01                    │
│                                                                         │
│  🏦 BANCOS                                                              │
│  ├── Banco Popular    │ 101-00001-1      │ Emite B01                    │
│  ├── Banreservas      │ 401-000-000      │ Emite B01                    │
│  └── BHD León         │ 101-XXXXXXX      │ Emite B01                    │
│                                                                         │
│  📞 TELECOMUNICACIONES                                                  │
│  ├── Claro            │ 101-XXXXXXX      │ Emite B01                    │
│  └── Altice           │ 101-XXXXXXX      │ Emite B01                    │
│                                                                         │
│  🌐 DOMINIOS LOCALES                                                    │
│  └── NIC.do           │ 130529842        │ Emite B01 (dominio .do)      │
│      (www.nic.do - Registrador oficial de dominios .do en RD)           │
│                                                                         │
│  ⚡ ELECTRICIDAD                                                        │
│  ├── Edenorte         │ 430-XXXXXXX      │ Emite B01 (exento ITBIS)     │
│  ├── Edesur           │ 430-XXXXXXX      │ Emite B01 (exento ITBIS)     │
│  └── Edeeste          │ 430-XXXXXXX      │ Emite B01 (exento ITBIS)     │
│                                                                         │
│  👨‍💼 PROFESIONALES (Personas Físicas)                                  │
│  ├── Contador         │ Cédula/RNC       │ Emite B01 (retener 10%)      │
│  ├── Abogado          │ Cédula/RNC       │ Emite B01 (retener 10%)      │
│  └── Consultores      │ Cédula/RNC       │ Emite B01 (retener 10%)      │
│                                                                         │
│  🏢 ALQUILER                                                            │
│  ├── Persona física   │ Cédula           │ Emite B02 (retener 10%)      │
│  └── Empresa          │ RNC              │ Emite B01                    │
│                                                                         │
│  🛒 COMPRAS LOCALES                                                     │
│  ├── Office Depot     │ 101-XXXXXXX      │ Emite B01                    │
│  ├── iCon             │ 101-XXXXXXX      │ Emite B01                    │
│  └── Otras tiendas    │ RNC              │ Emite B01/B02                │
│                                                                         │
│  👷 FREELANCERS LOCALES                                                 │
│  ├── Diseñadores      │ Cédula/RNC       │ Emite B01/B02 (retener 10%)  │
│  ├── Desarrolladores  │ Cédula/RNC       │ Emite B01/B02 (retener 10%)  │
│  └── Marketing        │ Cédula/RNC       │ Emite B01/B02 (retener 10%)  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. PROCEDIMIENTO DE REGISTRO DE GASTOS

### 2.1 Flujo General

```
┌─────────────────────────────────────────────────────────────────────────┐
│                FLUJO DE REGISTRO DE GASTOS                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1. OCURRE EL GASTO                                                     │
│     ├── Cargo en tarjeta de crédito                                     │
│     ├── Transferencia bancaria                                          │
│     ├── Cheque                                                          │
│     └── Efectivo (raro, evitar)                                         │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  2. OBTENER DOCUMENTO SOPORTE                                           │
│     ├── Internacional: Invoice/Receipt del proveedor                    │
│     ├── Local: NCF (B01/B02) del proveedor                              │
│     └── Si no hay NCF local: Solicitar o emitir B11                     │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  3. REGISTRAR EN SISTEMA                                                │
│     ├── ExpenseTrackingService (API)                                    │
│     ├── Subir documento a S3/MediaService                               │
│     ├── Clasificar tipo de gasto (01-11)                                │
│     └── Marcar origen (Local/Internacional)                             │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  4. VALIDACIÓN CONTABLE                                                 │
│     ├── Contador revisa registro                                        │
│     ├── Verifica NCF en DGII (si local)                                 │
│     └── Aprueba para inclusión en 606                                   │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  5. GENERACIÓN AUTOMÁTICA                                               │
│     ├── Día 1-5: Cierre del mes                                         │
│     ├── Día 6-10: Generar Formato 606                                   │
│     └── Día 15: Enviar a DGII                                           │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Procedimiento para Gastos Internacionales

```
┌─────────────────────────────────────────────────────────────────────────┐
│           PROCEDIMIENTO: GASTOS INTERNACIONALES                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  PASO 1: Capturar el Gasto                                              │
│  ────────────────────────                                               │
│  • Al recibir cargo en tarjeta de crédito, descargar invoice            │
│  • Fuentes: Email de confirmación, dashboard del proveedor              │
│  • Formato: PDF preferido, screenshot si no hay PDF                     │
│                                                                         │
│  PASO 2: Registrar en Sistema                                           │
│  ───────────────────────────                                            │
│                                                                         │
│  POST /api/expenses                                                     │
│  {                                                                      │
│    "type": "INTERNATIONAL",                                             │
│    "provider": {                                                        │
│      "name": "Digital Ocean",                                           │
│      "country": "USA",                                                  │
│      "hasRNC": false                                                    │
│    },                                                                   │
│    "invoice": {                                                         │
│      "number": "INV-2026-001234",                                       │
│      "date": "2026-01-15",                                              │
│      "amountUSD": 100.00,                                               │
│      "exchangeRate": 60.50,                                             │
│      "amountDOP": 6050.00                                               │
│    },                                                                   │
│    "category": "02",  // Servicios                                      │
│    "subcategory": "HOSTING",                                            │
│    "paymentMethod": "03",  // Tarjeta de crédito                        │
│    "documentUrl": "s3://okla-expenses/2026/01/do-inv-001234.pdf"        │
│  }                                                                      │
│                                                                         │
│  PASO 3: Asignar NCF B13 Interno                                        │
│  ───────────────────────────────                                        │
│  • Sistema genera NCF B13 secuencial para control interno               │
│  • Este NCF se usa en el Formato 606                                    │
│  • Ejemplo: B1300000001, B1300000002, etc.                              │
│                                                                         │
│  PASO 4: Documentación Requerida                                        │
│  ───────────────────────────────                                        │
│  ✅ Invoice/Receipt del proveedor (PDF)                                 │
│  ✅ Statement de tarjeta de crédito del mes                             │
│  ✅ Tasa de cambio del día (Banco Central RD)                           │
│  ✅ Evidencia de uso empresarial del servicio                           │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.3 Procedimiento para Gastos Locales

```
┌─────────────────────────────────────────────────────────────────────────┐
│              PROCEDIMIENTO: GASTOS LOCALES (RD)                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  PASO 1: Solicitar NCF                                                  │
│  ─────────────────────                                                  │
│  • SIEMPRE solicitar factura con NCF al proveedor                       │
│  • Proporcionar RNC de OKLA: 1-33-32590-1                               │
│  • Verificar que NCF sea válido (consultar en DGII)                     │
│                                                                         │
│  PASO 2: Verificar Validez del NCF                                      │
│  ────────────────────────────────                                       │
│                                                                         │
│  // Verificación automática en sistema                                  │
│  GET /api/dgii/verify-ncf/{ncf}                                         │
│  Response: {                                                            │
│    "ncf": "B0100000123",                                                │
│    "valid": true,                                                       │
│    "rnc": "101234567",                                                  │
│    "businessName": "Proveedor XYZ SRL",                                 │
│    "status": "ACTIVE"                                                   │
│  }                                                                      │
│                                                                         │
│  PASO 3: Determinar Retención                                           │
│  ────────────────────────────                                           │
│  • Si proveedor es PERSONA FÍSICA + Servicio profesional → 10% ISR      │
│  • Si proveedor es EMPRESA (SRL, SA, EIRL) → NO retener                 │
│  • Si es alquiler a persona física → 10% ISR                            │
│                                                                         │
│  PASO 4: Registrar en Sistema                                           │
│  ───────────────────────────                                            │
│                                                                         │
│  POST /api/expenses                                                     │
│  {                                                                      │
│    "type": "LOCAL",                                                     │
│    "provider": {                                                        │
│      "rnc": "101234567",                                                │
│      "name": "Contador Juan Pérez",                                     │
│      "type": "PERSONA_FISICA",                                          │
│      "hasRNC": true                                                     │
│    },                                                                   │
│    "ncf": {                                                             │
│      "number": "B0100000789",                                           │
│      "type": "B01",                                                     │
│      "date": "2026-01-25"                                               │
│    },                                                                   │
│    "amounts": {                                                         │
│      "subtotal": 15000.00,                                              │
│      "itbis": 2700.00,                                                  │
│      "total": 17700.00,                                                 │
│      "isrWithheld": 1500.00,  // 10% retención                          │
│      "netPayable": 16200.00                                             │
│    },                                                                   │
│    "category": "02",  // Servicios                                      │
│    "paymentMethod": "02",  // Transferencia                             │
│    "documentUrl": "s3://okla-expenses/2026/01/contador-b0100000789.pdf" │
│  }                                                                      │
│                                                                         │
│  PASO 5: Pagar y Registrar Retención                                    │
│  ────────────────────────────────                                       │
│  • Pagar al proveedor: $16,200 (total - retención)                      │
│  • Retención de $1,500 se declara en IR-17 día 10                       │
│  • Guardar comprobante de transferencia                                 │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 3. CATÁLOGO DE GASTOS DE OKLA

### 3.1 Gastos Recurrentes Mensuales

| ID  | Proveedor     | Servicio             | USD  | RD$ (~60) | Tipo | NCF | Retención |
| --- | ------------- | -------------------- | ---- | --------- | ---- | --- | --------- |
| 001 | Digital Ocean | Hosting/Servers      | $100 | $6,000    | 02   | B13 | N/A       |
| 002 | GitHub        | Repos + Copilot      | $21  | $1,260    | 02   | B13 | N/A       |
| 003 | Stripe        | Comisiones (~3%)     | Var. | ~$15,000  | 07   | B13 | N/A       |
| 004 | Google Ads    | Publicidad           | $500 | $30,000   | 02   | B13 | N/A       |
| 005 | Facebook Ads  | Publicidad           | $350 | $21,000   | 02   | B13 | N/A       |
| 006 | AZUL Popular  | Comisiones (~2.5%)   | -    | ~$8,000   | 07   | B01 | No        |
| 007 | Banco Popular | Mantenimiento cuenta | -    | $1,500    | 07   | B01 | No        |
| 008 | Claro         | Internet fibra       | -    | $3,500    | 02   | B01 | No        |
| 009 | Contador      | Servicios contables  | -    | $15,000   | 02   | B01 | 10%       |
| 010 | Edenorte      | Electricidad         | -    | $2,500    | 06   | B01 | No        |
| 011 | Dueño Oficina | Alquiler             | -    | $25,000   | 03   | B02 | 10%       |

### 3.2 Gastos Variables

| ID  | Proveedor        | Servicio             | Frecuencia | USD     | Tipo | NCF | Retención |
| --- | ---------------- | -------------------- | ---------- | ------- | ---- | --- | --------- |
| 101 | Twilio           | SMS/Verificación     | Variable   | $20-100 | 02   | B13 | N/A       |
| 102 | SendGrid         | Emails transaccional | Variable   | $15-50  | 02   | B13 | N/A       |
| 103 | OpenAI           | ChatGPT API          | Variable   | $20-200 | 02   | B13 | N/A       |
| 104 | Abogado          | Consultoría legal    | Ocasional  | -       | 02   | B01 | 10%       |
| 105 | Freelance Diseño | Proyectos            | Ocasional  | -       | 02   | B01 | 10%       |
| 106 | Office Depot     | Suministros          | Ocasional  | -       | 09   | B01 | No        |

### 3.3 Gastos Anuales

| ID  | Proveedor     | Servicio            | USD  | RD$     | Tipo | NCF | Fecha Típica | Nota                             |
| --- | ------------- | ------------------- | ---- | ------- | ---- | --- | ------------ | -------------------------------- |
| 201 | NIC.do        | Dominio okla.com.do | -    | $2,500  | 02   | B01 | Enero        | Proveedor LOCAL (RNC requerido)  |
| 202 | Let's Encrypt | SSL Certificate     | $0   | $0      | -    | -   | Auto-renewal | GRATIS - Cloudflare/DO lo provee |
| 203 | JetBrains     | IDE License         | $199 | $12,000 | 02   | B13 | Marzo        | Internacional                    |
| 204 | Adobe CC      | Creative Cloud      | $600 | $36,000 | 02   | B13 | Febrero      | Internacional                    |
| 205 | DGII          | Renovación RNC      | -    | $0      | -    | -   | Anual        | Sin costo                        |

---

## 4. DOCUMENTACIÓN REQUERIDA POR TIPO

### 4.1 Para Gastos Internacionales

```
┌─────────────────────────────────────────────────────────────────────────┐
│         DOCUMENTOS REQUERIDOS - GASTOS INTERNACIONALES                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  DOCUMENTO PRINCIPAL:                                                   │
│  ✅ Invoice/Receipt del proveedor                                       │
│     • En inglés está OK                                                 │
│     • Debe mostrar: Nombre proveedor, fecha, monto, descripción         │
│     • Formato PDF preferido                                             │
│                                                                         │
│  DOCUMENTOS DE SOPORTE:                                                 │
│  ✅ Statement de tarjeta de crédito                                     │
│     • Debe coincidir el cargo con el invoice                            │
│     • Mostrar fecha de cargo y monto                                    │
│                                                                         │
│  ✅ Tasa de cambio del día                                              │
│     • Del Banco Central de RD (www.bancentral.gov.do)                   │
│     • Screenshot o PDF del día de la transacción                        │
│                                                                         │
│  ✅ Evidencia de uso empresarial                                        │
│     • Para hosting: Dashboard mostrando dominios de OKLA                │
│     • Para publicidad: Reportes de campaña de OKLA                      │
│     • Para software: Pantalla mostrando uso para OKLA                   │
│                                                                         │
│  ALMACENAMIENTO:                                                        │
│  📁 S3: /expenses/international/{year}/{month}/{provider}/              │
│     ├── invoice.pdf                                                     │
│     ├── statement.pdf                                                   │
│     ├── exchange-rate.pdf                                               │
│     └── evidence.pdf                                                    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Para Gastos Locales

```
┌─────────────────────────────────────────────────────────────────────────┐
│            DOCUMENTOS REQUERIDOS - GASTOS LOCALES                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  DOCUMENTO PRINCIPAL:                                                   │
│  ✅ Factura con NCF válido                                              │
│     • NCF verificado en consulta DGII                                   │
│     • Debe incluir: RNC proveedor, fecha, detalle, ITBIS                │
│     • Formato: Físico escaneado o digital                               │
│                                                                         │
│  DOCUMENTOS DE SOPORTE:                                                 │
│  ✅ Comprobante de pago                                                 │
│     • Transferencia: Screenshot o PDF del banco                         │
│     • Cheque: Copia del cheque                                          │
│     • Tarjeta: Statement del mes                                        │
│                                                                         │
│  ✅ Para retenciones:                                                   │
│     • Certificado de retención emitido al proveedor                     │
│     • Copia del IR-17 donde se declaró la retención                     │
│                                                                         │
│  ✅ Verificación NCF:                                                   │
│     • Screenshot de consulta en DGII                                    │
│     • O respuesta de API de verificación                                │
│                                                                         │
│  ALMACENAMIENTO:                                                        │
│  📁 S3: /expenses/local/{year}/{month}/{provider}/                      │
│     ├── factura-{ncf}.pdf                                               │
│     ├── comprobante-pago.pdf                                            │
│     ├── verificacion-ncf.pdf                                            │
│     └── certificado-retencion.pdf (si aplica)                           │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 5. ESQUEMA DE BASE DE DATOS

### 5.1 Tabla: expense_providers

```sql
CREATE TABLE expense_providers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Identificación
    name VARCHAR(200) NOT NULL,
    legal_name VARCHAR(200),
    rnc VARCHAR(15),                    -- NULL si internacional
    identification_type VARCHAR(10),     -- RNC, CEDULA, PASSPORT, NULL

    -- Clasificación
    provider_type VARCHAR(20) NOT NULL,  -- INTERNATIONAL, LOCAL_COMPANY, LOCAL_PERSON
    country VARCHAR(3) DEFAULT 'DOM',    -- ISO 3166-1 alpha-3

    -- Contacto
    email VARCHAR(200),
    phone VARCHAR(20),
    address TEXT,

    -- Fiscal
    requires_retention BOOLEAN DEFAULT FALSE,
    retention_rate DECIMAL(5,2) DEFAULT 0,
    default_expense_type VARCHAR(2),     -- 01-11
    default_ncf_type VARCHAR(3),         -- B01, B02, B13

    -- Auditoría
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    created_by UUID,
    is_active BOOLEAN DEFAULT TRUE
);

-- Índices
CREATE INDEX idx_providers_rnc ON expense_providers(rnc);
CREATE INDEX idx_providers_type ON expense_providers(provider_type);
CREATE INDEX idx_providers_name ON expense_providers(name);
```

### 5.2 Tabla: expenses

```sql
CREATE TABLE expenses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Relaciones
    provider_id UUID REFERENCES expense_providers(id),

    -- Identificación del gasto
    expense_number VARCHAR(20) NOT NULL UNIQUE,  -- EXP-2026-000001

    -- Documento origen
    original_invoice_number VARCHAR(100),        -- Invoice del proveedor
    original_invoice_date DATE NOT NULL,

    -- NCF (para 606)
    ncf_received VARCHAR(15),                    -- NCF del proveedor (si local)
    ncf_internal VARCHAR(15),                    -- NCF B13 interno (si internacional)
    ncf_type VARCHAR(3),                         -- B01, B02, B11, B13

    -- Clasificación DGII
    expense_type VARCHAR(2) NOT NULL,            -- 01-11 (código 606)
    expense_category VARCHAR(50),                -- HOSTING, ADVERTISING, etc.

    -- Montos
    currency VARCHAR(3) DEFAULT 'DOP',           -- DOP, USD
    original_amount DECIMAL(18,2) NOT NULL,      -- Monto en moneda original
    exchange_rate DECIMAL(10,4) DEFAULT 1,       -- Tasa de cambio si USD
    amount_dop DECIMAL(18,2) NOT NULL,           -- Monto en RD$

    -- Impuestos
    subtotal DECIMAL(18,2) NOT NULL,
    itbis_amount DECIMAL(18,2) DEFAULT 0,
    itbis_rate DECIMAL(5,2) DEFAULT 0,           -- 0, 16, 18
    itbis_withheld DECIMAL(18,2) DEFAULT 0,
    isr_withheld DECIMAL(18,2) DEFAULT 0,
    isr_rate DECIMAL(5,2) DEFAULT 0,             -- 0, 10, 27
    total DECIMAL(18,2) NOT NULL,
    net_payable DECIMAL(18,2) NOT NULL,          -- Lo que se paga al proveedor

    -- Pago
    payment_status VARCHAR(20) DEFAULT 'PENDING', -- PENDING, PAID, PARTIAL
    payment_date DATE,
    payment_method VARCHAR(2),                    -- 01-07 (código 606)
    payment_reference VARCHAR(100),               -- Número transferencia/cheque

    -- Descripción
    description TEXT,
    notes TEXT,

    -- Documentos en S3
    invoice_document_url VARCHAR(500),
    payment_document_url VARCHAR(500),
    supporting_documents JSONB,                   -- Array de URLs adicionales

    -- Período fiscal
    fiscal_month INT NOT NULL,                    -- 1-12
    fiscal_year INT NOT NULL,                     -- 2026

    -- Inclusión en reportes
    included_in_606 BOOLEAN DEFAULT FALSE,
    format_606_id UUID,                           -- Referencia al reporte 606
    included_in_ir17 BOOLEAN DEFAULT FALSE,       -- Si tiene retención
    format_ir17_id UUID,

    -- Estados
    status VARCHAR(20) DEFAULT 'DRAFT',          -- DRAFT, VERIFIED, APPROVED, REPORTED
    verified_by UUID,
    verified_at TIMESTAMP,
    approved_by UUID,
    approved_at TIMESTAMP,

    -- Auditoría
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    created_by UUID NOT NULL
);

-- Índices
CREATE INDEX idx_expenses_provider ON expenses(provider_id);
CREATE INDEX idx_expenses_fiscal ON expenses(fiscal_year, fiscal_month);
CREATE INDEX idx_expenses_ncf ON expenses(ncf_received);
CREATE INDEX idx_expenses_status ON expenses(status);
CREATE INDEX idx_expenses_payment ON expenses(payment_status);
CREATE INDEX idx_expenses_type ON expenses(expense_type);
```

### 5.3 Tabla: expense_documents

```sql
CREATE TABLE expense_documents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    expense_id UUID REFERENCES expenses(id) ON DELETE CASCADE,

    -- Documento
    document_type VARCHAR(50) NOT NULL,  -- INVOICE, PAYMENT, NCF_VERIFICATION, RETENTION_CERT, EXCHANGE_RATE
    document_url VARCHAR(500) NOT NULL,
    file_name VARCHAR(200),
    file_size_bytes BIGINT,
    mime_type VARCHAR(100),

    -- Metadatos
    description TEXT,
    extracted_data JSONB,               -- Datos extraídos automáticamente (OCR)

    -- Auditoría
    uploaded_at TIMESTAMP DEFAULT NOW(),
    uploaded_by UUID NOT NULL
);

CREATE INDEX idx_expense_docs_expense ON expense_documents(expense_id);
CREATE INDEX idx_expense_docs_type ON expense_documents(document_type);
```

---

## 6. API ENDPOINTS

### 6.1 CRUD de Gastos

```yaml
# Crear gasto
POST /api/expenses
Request:
  provider_id: UUID
  original_invoice_number: string
  original_invoice_date: date
  ncf_received: string (optional, for local)
  expense_type: string (01-11)
  currency: string (DOP/USD)
  original_amount: decimal
  exchange_rate: decimal (optional)
  itbis_amount: decimal
  isr_withheld: decimal
  description: string
Response:
  expense: ExpenseDto
  ncf_internal: string (B13 generated if international)

# Listar gastos del período
GET /api/expenses?year=2026&month=1&status=APPROVED
Response:
  expenses: ExpenseDto[]
  summary:
    total_count: int
    total_amount: decimal
    total_itbis: decimal
    total_isr_withheld: decimal

# Obtener gasto por ID
GET /api/expenses/{id}
Response:
  expense: ExpenseDto
  documents: DocumentDto[]

# Aprobar gasto para 606
POST /api/expenses/{id}/approve
Response:
  expense: ExpenseDto

# Subir documento
POST /api/expenses/{id}/documents
Request (multipart):
  file: binary
  document_type: string
Response:
  document: DocumentDto
```

### 6.2 Gestión de Proveedores

```yaml
# Crear proveedor
POST /api/expense-providers
Request:
  name: string
  rnc: string (optional)
  provider_type: INTERNATIONAL | LOCAL_COMPANY | LOCAL_PERSON
  country: string
  requires_retention: boolean
  retention_rate: decimal
  default_expense_type: string

# Listar proveedores
GET /api/expense-providers?type=INTERNATIONAL
Response:
  providers: ProviderDto[]

# Verificar RNC en DGII
GET /api/expense-providers/verify-rnc/{rnc}
Response:
  valid: boolean
  rnc: string
  business_name: string
  status: string
```

### 6.3 Generación de Reportes

```yaml
# Generar preview del 606
GET /api/dgii/606/preview?year=2026&month=1
Response:
  lines: Format606LineDto[]
  summary:
    total_records: int
    total_services: decimal
    total_goods: decimal
    total_itbis: decimal
    total_isr_withheld: decimal

# Generar archivo 606 oficial
POST /api/dgii/606/generate
Request:
  year: int
  month: int
  include_pending: boolean (default: false)
Response:
  file_url: string (TXT file in S3)
  format_id: UUID
  record_count: int

# Consultar estado de reportes
GET /api/dgii/reports?year=2026&month=1
Response:
  format_606: ReportStatusDto
  format_607: ReportStatusDto
  format_608: ReportStatusDto
  ir17: ReportStatusDto
```

---

## 7. CALENDARIO DE REGISTRO

### 7.1 Proceso Diario

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    PROCESO DIARIO DE REGISTRO                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  🌅 MAÑANA (9:00 AM):                                                   │
│  ───────────────────                                                    │
│  • Revisar cargos del día anterior en tarjetas de crédito               │
│  • Revisar transferencias bancarias recibidas/enviadas                  │
│  • Descargar invoices de proveedores internacionales                    │
│                                                                         │
│  📝 REGISTRO (10:00 AM - 12:00 PM):                                     │
│  ─────────────────────────────────                                      │
│  • Registrar cada gasto en el sistema                                   │
│  • Clasificar tipo de gasto (01-11)                                     │
│  • Subir documentos de soporte                                          │
│  • Aplicar tasa de cambio del día                                       │
│                                                                         │
│  ✅ VALIDACIÓN (Fin del día):                                           │
│  ─────────────────────────────                                          │
│  • Verificar que todos los gastos están registrados                     │
│  • Conciliar con estado de cuenta bancario                              │
│  • Marcar gastos como "VERIFICADO"                                      │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Proceso Mensual

```
┌─────────────────────────────────────────────────────────────────────────┐
│                   CALENDARIO MENSUAL DE REGISTRO                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  DÍA 1-3: CIERRE DE MES ANTERIOR                                        │
│  ───────────────────────────────                                        │
│  • Registrar últimos gastos del mes anterior                            │
│  • Verificar que todos los gastos tienen documentos                     │
│  • Conciliar con estados de cuenta bancarios                            │
│  • Enviar TSS (nómina) en SUIR                                          │
│                                                                         │
│  DÍA 4-5: VALIDACIÓN CONTABLE                                           │
│  ────────────────────────────                                           │
│  • Contador revisa todos los registros                                  │
│  • Verifica NCF locales en DGII                                         │
│  • Aprueba gastos para inclusión en 606                                 │
│  • Pago de TSS                                                          │
│                                                                         │
│  DÍA 6-9: GENERACIÓN DE REPORTES                                        │
│  ────────────────────────────                                           │
│  • Generar preview de Formato 606                                       │
│  • Revisar totales y categorías                                         │
│  • Corregir errores encontrados                                         │
│  • Generar preview de IR-17                                             │
│                                                                         │
│  DÍA 10: ENVÍO IR-17                                                    │
│  ────────────────────                                                   │
│  • Presentar y pagar IR-17 (retenciones)                                │
│  • Guardar acuse de recibo                                              │
│                                                                         │
│  DÍA 11-14: VALIDACIÓN FINAL                                            │
│  ─────────────────────────────                                          │
│  • Revisión final de 606, 607, 608                                      │
│  • Validar con herramienta DGII                                         │
│                                                                         │
│  DÍA 15: ENVÍO FORMATOS                                                 │
│  ────────────────────────                                               │
│  • Enviar 606, 607, 608 a DGII                                          │
│  • Guardar acuses de recibo                                             │
│                                                                         │
│  DÍA 16-19: PREPARACIÓN IT-1                                            │
│  ─────────────────────────────                                          │
│  • Calcular ITBIS a pagar o crédito fiscal                              │
│  • Revisar cálculos                                                     │
│                                                                         │
│  DÍA 20: ENVÍO IT-1                                                     │
│  ───────────────────                                                    │
│  • Presentar IT-1 (Declaración ITBIS)                                   │
│  • Pagar ITBIS (si aplica)                                              │
│  • Guardar acuse                                                        │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 8. CONTROLES Y VALIDACIONES

### 8.1 Validaciones Automáticas

```yaml
Al registrar gasto:
  - ✅ Proveedor existe en catálogo
  - ✅ Monto > 0
  - ✅ Fecha no futura
  - ✅ Si local: NCF válido y verificado en DGII
  - ✅ Si internacional: Invoice number único
  - ✅ Tipo de gasto válido (01-11)
  - ✅ Documento de soporte subido

Al aprobar gasto:
  - ✅ Documentos completos
  - ✅ Monto conciliado con banco
  - ✅ Retención calculada correctamente
  - ✅ Tasa de cambio aplicada (si USD)

Al generar 606:
  - ✅ Todos los gastos aprobados
  - ✅ No hay duplicados de NCF
  - ✅ Totales cuadran
  - ✅ Formato cumple especificación DGII
```

### 8.2 Alertas Automáticas

```yaml
Alertas diarias:
  - ⚠️ Gastos sin documento de soporte
  - ⚠️ Gastos pendientes de aprobación > 3 días
  - ⚠️ NCF no verificados

Alertas semanales:
  - ⚠️ Resumen de gastos de la semana
  - ⚠️ Gastos por categoría vs presupuesto

Alertas mensuales:
  - 🔴 Día 3: Recordatorio TSS
  - 🔴 Día 8: Recordatorio IR-17
  - 🔴 Día 13: Recordatorio 606/607/608
  - 🔴 Día 18: Recordatorio IT-1
```

---

## 📋 CHECKLIST DE IMPLEMENTACIÓN

### Backend

- [ ] Crear tabla expense_providers
- [ ] Crear tabla expenses
- [ ] Crear tabla expense_documents
- [ ] API CRUD de proveedores
- [ ] API CRUD de gastos
- [ ] API de documentos (upload a S3)
- [ ] Integración DGII para verificar NCF
- [ ] Generación automática de NCF B13
- [ ] Generación de Formato 606
- [ ] Cálculo automático de retenciones
- [ ] Sistema de alertas

### Frontend (Admin)

- [ ] Formulario de registro de gastos
- [ ] Lista de gastos con filtros
- [ ] Vista de aprobación de gastos
- [ ] Dashboard de gastos por categoría
- [ ] Generador de reportes 606
- [ ] Calendario de obligaciones

---

**Documento creado:** Enero 25, 2026  
**Próxima revisión:** Cuando cambie estructura de gastos o regulaciones DGII  
**Responsable:** Equipo de Desarrollo + Contador
