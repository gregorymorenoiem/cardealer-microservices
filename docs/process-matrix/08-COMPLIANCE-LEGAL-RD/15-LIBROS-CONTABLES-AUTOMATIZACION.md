# 📚 Libros Contables y Automatización para Auditoría - OKLA S.R.L.

> **Empresa:** OKLA S.R.L.  
> **RNC:** 1-33-32590-1  
> **Registro Mercantil:** 196339PSD  
> **Fecha de Creación:** Enero 25, 2026  
> **Actualización:** Enero 25, 2026 (Integración e-CF)  
> **Propósito:** Gestión automatizada de libros contables con facturación electrónica y envío automático a DGII

---

## 📋 RESUMEN EJECUTIVO

Este documento define los procedimientos y sistemas para gestionar los libros contables requeridos por la DGII, con automatización completa para responder a solicitudes de auditoría en tiempo récord, **integrado con facturación electrónica (e-CF)** para envío automático de reportes.

### Objetivo

```
┌─────────────────────────────────────────────────────────────────────────┐
│          AUTOMATIZACIÓN DE LIBROS CONTABLES + e-CF                      │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  🎯 META PRINCIPAL:                                                     │
│  Cuando un auditor de DGII solicite libros contables, el sistema       │
│  debe generar y entregar AUTOMÁTICAMENTE todos los reportes            │
│  requeridos en MENOS DE 1 HORA.                                         │
│                                                                         │
│  📚 LIBROS REQUERIDOS POR DGII:                                         │
│  ├── 📖 Libro Diario                                                    │
│  ├── 📖 Libro Mayor                                                     │
│  ├── 📖 Libro de Inventarios y Balances                                 │
│  ├── 📖 Libro de Compras (→ Formato 606 automático)                    │
│  ├── 📖 Libro de Ventas (→ Formato 607 desde e-CF)                     │
│  ├── 📖 Libro de Retenciones (→ IR-17 automático)                      │
│  └── 📖 Libro de Banco                                                  │
│                                                                         │
│  🔄 AUTOMATIZACIÓN CON e-CF:                                            │
│  ├── ✅ e-CF emitidos → DGII en tiempo real                            │
│  ├── ✅ Formato 607 → Generado automáticamente por DGII                │
│  ├── ✅ Formato 606 → Envío electrónico automático                     │
│  ├── ✅ IT-1 (ITBIS) → Pre-llenado + envío electrónico                 │
│  ├── ✅ IR-17 → Envío electrónico automático                           │
│  ├── ✅ Formato 609 → Envío electrónico automático                     │
│  └── ✅ Paquete auditoría → 1 clic = todos los libros                  │
│                                                                         │
│  ⏱️ AHORRO DE TIEMPO:                                                   │
│  Antes (manual): 10-15 horas/mes                                        │
│  Ahora (e-CF):   ~30 minutos/mes (95% menos)                           │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Integración con Facturación Electrónica

```
┌─────────────────────────────────────────────────────────────────────────┐
│          FLUJO: TRANSACCIÓN → LIBROS → DGII (AUTOMÁTICO)                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1️⃣ VENTA EN OKLA                                                       │
│     └── Cliente paga $29 listing + $5.22 ITBIS                         │
│                                                                         │
│               ▼ (Automático - 0 segundos)                               │
│                                                                         │
│  2️⃣ e-CF EMITIDO                                                        │
│     ├── ECFService genera E32                                          │
│     ├── Firmado digitalmente                                           │
│     └── Enviado a DGII en < 5 seg ✅                                   │
│                                                                         │
│               ▼ (Automático - 0 segundos)                               │
│                                                                         │
│  3️⃣ ASIENTO CONTABLE REGISTRADO                                         │
│     ├── Débito: Bancos $34.22                                          │
│     ├── Crédito: Ingresos $29.00                                       │
│     └── Crédito: ITBIS por Pagar $5.22                                 │
│                                                                         │
│               ▼ (Automático - 0 segundos)                               │
│                                                                         │
│  4️⃣ LIBROS ACTUALIZADOS                                                 │
│     ├── Libro Diario ✅                                                │
│     ├── Libro Mayor ✅                                                 │
│     ├── Libro de Ventas ✅                                             │
│     └── Libro de Banco ✅                                              │
│                                                                         │
│               ▼ (Fin de mes - Job automático)                           │
│                                                                         │
│  5️⃣ REPORTES ENVIADOS A DGII                                            │
│     ├── Formato 607 → Ya en DGII (de e-CF) ✅                          │
│     ├── Formato 606 → Enviado electrónicamente ✅                      │
│     ├── IT-1 → Enviado electrónicamente ✅                             │
│     └── IR-17 → Enviado electrónicamente ✅                            │
│                                                                         │
│  ⏱️ INTERVENCIÓN HUMANA: 0 segundos por transacción                    │
│  ⏱️ FIN DE MES: 30 minutos para verificar resúmenes                    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 1. LIBROS CONTABLES REQUERIDOS

### 1.1 Libros Obligatorios para OKLA

```
┌─────────────────────────────────────────────────────────────────────────┐
│                 LIBROS CONTABLES OBLIGATORIOS                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  📖 LIBRO DIARIO                                                        │
│  ─────────────────                                                      │
│  • Registro cronológico de TODAS las operaciones                        │
│  • Cada asiento: fecha, descripción, débito, crédito                   │
│  • Base: Partida doble                                                  │
│  • Actualización: Diaria/Automática                                     │
│  • Fuente: Todas las transacciones del sistema                         │
│                                                                         │
│  📖 LIBRO MAYOR                                                         │
│  ─────────────────                                                      │
│  • Agrupa movimientos por cuenta contable                               │
│  • Muestra saldo de cada cuenta                                        │
│  • Actualización: Automática al registrar en Diario                    │
│  • Catálogo: Plan de cuentas DGII                                      │
│                                                                         │
│  📖 LIBRO DE INVENTARIOS Y BALANCES                                     │
│  ───────────────────────────────────                                    │
│  • Balance de apertura (inicio del período)                            │
│  • Balance de cierre (fin del período)                                 │
│  • Inventario de activos fijos                                         │
│  • Para OKLA: Principalmente activos digitales                         │
│                                                                         │
│  📖 LIBRO DE COMPRAS (GASTOS) → FORMATO 606                             │
│  ───────────────────────────────                                        │
│  • Todas las compras con detalle                                       │
│  • NCF/e-CF del proveedor                                              │
│  • ITBIS pagado (crédito fiscal)                                       │
│  • Retenciones realizadas                                              │
│  • ✅ Genera Formato 606 automáticamente                               │
│  • ✅ Envío electrónico a DGII                                         │
│                                                                         │
│  📖 LIBRO DE VENTAS → FORMATO 607 (AUTOMÁTICO DESDE e-CF)               │
│  ────────────────────                                                   │
│  • Todas las ventas con detalle                                        │
│  • e-CF emitido en tiempo real                                         │
│  • ITBIS cobrado                                                       │
│  • ✅ Formato 607 PRE-GENERADO por DGII desde e-CF                     │
│  • ✅ Solo verificar en Oficina Virtual                                │
│                                                                         │
│  📖 LIBRO DE RETENCIONES → IR-17 (AUTOMÁTICO)                           │
│  ─────────────────────────                                              │
│  • Retenciones ISR realizadas (10% servicios profesionales)            │
│  • Retenciones recibidas de clientes                                   │
│  • ✅ Genera IR-17 automáticamente                                     │
│  • ✅ Envío electrónico a DGII                                         │
│  • Alimenta IR-17                                                       │
│                                                                         │
│  📖 LIBRO DE BANCO                                                      │
│  ──────────────────                                                     │
│  • Movimientos de cada cuenta bancaria                                 │
│  • Conciliación con estados de cuenta                                  │
│  • Cheques emitidos y cobrados                                         │
│  • Transferencias enviadas y recibidas                                 │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Plan de Cuentas Contables (Catálogo)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                 PLAN DE CUENTAS - OKLA S.R.L.                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1. ACTIVOS                                                             │
│  ───────────                                                            │
│  1.1 Activos Corrientes                                                 │
│      1.1.01 Caja                                                        │
│      1.1.02 Bancos                                                      │
│             1.1.02.01 Banco Popular                                     │
│             1.1.02.02 BHD León                                          │
│      1.1.03 Cuentas por Cobrar                                          │
│      1.1.04 ITBIS Pagado (Crédito Fiscal)                              │
│      1.1.05 Anticipos a Proveedores                                     │
│      1.1.06 Retenciones por Aplicar                                     │
│                                                                         │
│  1.2 Activos No Corrientes                                              │
│      1.2.01 Equipos de Computación                                      │
│      1.2.02 Mobiliario y Equipo                                         │
│      1.2.03 Depreciación Acumulada (-)                                  │
│      1.2.04 Activos Intangibles (Software, Dominio)                    │
│                                                                         │
│  2. PASIVOS                                                             │
│  ───────────                                                            │
│  2.1 Pasivos Corrientes                                                 │
│      2.1.01 Cuentas por Pagar Proveedores                              │
│      2.1.02 ITBIS por Pagar                                             │
│      2.1.03 ISR por Pagar (Retenciones)                                │
│      2.1.04 Impuestos por Pagar                                         │
│      2.1.05 Sueldos por Pagar                                           │
│      2.1.06 TSS por Pagar                                               │
│      2.1.07 Ingresos Diferidos (Suscripciones prepagadas)              │
│                                                                         │
│  2.2 Pasivos No Corrientes                                              │
│      2.2.01 Préstamos por Pagar                                         │
│                                                                         │
│  3. PATRIMONIO                                                          │
│  ──────────────                                                         │
│  3.1.01 Capital Social                                                  │
│  3.1.02 Reserva Legal                                                   │
│  3.1.03 Utilidades Retenidas                                           │
│  3.1.04 Utilidad del Ejercicio                                         │
│                                                                         │
│  4. INGRESOS                                                            │
│  ────────────                                                           │
│  4.1 Ingresos Operacionales                                             │
│      4.1.01 Ingresos por Listings                                       │
│      4.1.02 Ingresos por Suscripciones Dealers                         │
│      4.1.03 Ingresos por Boosts                                         │
│      4.1.04 Ingresos por Publicidad                                     │
│                                                                         │
│  4.2 Otros Ingresos                                                     │
│      4.2.01 Intereses Ganados                                           │
│      4.2.02 Otros Ingresos                                              │
│                                                                         │
│  5. COSTOS Y GASTOS                                                     │
│  ───────────────────                                                    │
│  5.1 Gastos Operacionales                                               │
│      5.1.01 Gastos de Personal                                          │
│             5.1.01.01 Sueldos y Salarios                                │
│             5.1.01.02 Bonificaciones                                    │
│             5.1.01.03 TSS Patronal                                      │
│             5.1.01.04 Regalía Pascual                                   │
│      5.1.02 Gastos de Hosting/Servidores                               │
│      5.1.03 Gastos de Publicidad                                        │
│      5.1.04 Gastos de Software/Licencias                               │
│      5.1.05 Gastos de Telecomunicaciones                               │
│      5.1.06 Servicios Profesionales                                     │
│             5.1.06.01 Honorarios Contables                             │
│             5.1.06.02 Honorarios Legales                               │
│      5.1.07 Alquiler                                                    │
│      5.1.08 Electricidad                                                │
│      5.1.09 Comisiones Pasarelas de Pago                               │
│             5.1.09.01 Comisiones Stripe                                │
│             5.1.09.02 Comisiones AZUL                                  │
│      5.1.10 Gastos Bancarios                                            │
│      5.1.11 Depreciación y Amortización                                │
│                                                                         │
│  5.2 Gastos Financieros                                                 │
│      5.2.01 Intereses Pagados                                           │
│      5.2.02 Comisiones Bancarias                                        │
│      5.2.03 Diferencial Cambiario                                       │
│                                                                         │
│  6. CUENTAS DE ORDEN                                                    │
│  ────────────────────                                                   │
│  6.1.01 e-CF Emitidos                                                   │
│  6.1.02 e-CF Recibidos                                                  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. ARQUITECTURA DEL SISTEMA CONTABLE

### 2.1 Diagrama de Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                 ARQUITECTURA - AccountingService                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐              │
│  │ BillingService│───▶│AccountingService│◀───│ ECFService  │              │
│  │  (Pagos)     │    │  (Contabilidad) │    │ (Facturas)  │              │
│  └──────────────┘    └───────┬────────┘    └──────────────┘              │
│                              │                                          │
│  ┌──────────────┐            │            ┌──────────────┐              │
│  │ExpenseService│────────────┼────────────│ PayrollService│              │
│  │  (Gastos)    │            │            │  (Nómina)     │              │
│  └──────────────┘            │            └──────────────┘              │
│                              │                                          │
│                              ▼                                          │
│                    ┌──────────────────┐                                 │
│                    │    PostgreSQL    │                                 │
│                    │  (Libros Contables)│                               │
│                    └────────┬─────────┘                                 │
│                             │                                           │
│           ┌─────────────────┼─────────────────┐                         │
│           ▼                 ▼                 ▼                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                  │
│  │ Libro Diario │  │ Libro Mayor  │  │Libro Compras │                  │
│  └──────────────┘  └──────────────┘  └──────────────┘                  │
│           │                 │                 │                         │
│           └─────────────────┼─────────────────┘                         │
│                             ▼                                           │
│                    ┌──────────────────┐                                 │
│                    │ Report Generator │                                 │
│                    │  (Excel/PDF)     │                                 │
│                    └────────┬─────────┘                                 │
│                             │                                           │
│                             ▼                                           │
│                    ┌──────────────────┐                                 │
│                    │    S3 Storage    │                                 │
│                    │ (Reportes/Docs)  │                                 │
│                    └──────────────────┘                                 │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Eventos que Generan Asientos Contables

```
┌─────────────────────────────────────────────────────────────────────────┐
│           EVENTOS → ASIENTOS CONTABLES                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  💰 VENTA DE LISTING ($29 + ITBIS)                                      │
│  ──────────────────────────────────                                     │
│  Débito:  1.1.02 Bancos               $34.22                           │
│  Crédito: 4.1.01 Ingresos Listings    $29.00                           │
│  Crédito: 2.1.02 ITBIS por Pagar       $5.22                           │
│                                                                         │
│  💳 VENTA SUSCRIPCIÓN DEALER ($129 + ITBIS)                             │
│  ────────────────────────────────────────────                           │
│  Débito:  1.1.02 Bancos               $152.22                          │
│  Crédito: 4.1.02 Ingresos Suscripc.   $129.00                          │
│  Crédito: 2.1.02 ITBIS por Pagar       $23.22                          │
│                                                                         │
│  🖥️ GASTO HOSTING DIGITAL OCEAN ($100 USD = RD$6,000)                   │
│  ──────────────────────────────────────────────────                     │
│  Débito:  5.1.02 Gastos Hosting       $6,000.00                        │
│  Crédito: 1.1.02 Bancos               $6,000.00                        │
│  (Sin ITBIS - Gasto del exterior)                                      │
│                                                                         │
│  👨‍💼 PAGO A CONTADOR (RD$15,000 + ITBIS - 10% Ret.)                     │
│  ──────────────────────────────────────────────────                     │
│  Débito:  5.1.06.01 Honorarios Contab.$15,000.00                       │
│  Débito:  1.1.04 ITBIS Pagado          $2,700.00                       │
│  Crédito: 1.1.02 Bancos               $16,200.00  (neto pagado)        │
│  Crédito: 2.1.03 ISR por Pagar         $1,500.00  (10% retención)      │
│                                                                         │
│  🏢 PAGO ALQUILER PERSONA FÍSICA (RD$25,000 + ITBIS - 10% Ret.)         │
│  ────────────────────────────────────────────────────────────           │
│  Débito:  5.1.07 Alquiler             $25,000.00                       │
│  Débito:  1.1.04 ITBIS Pagado          $4,500.00                       │
│  Crédito: 1.1.02 Bancos               $27,000.00  (neto pagado)        │
│  Crédito: 2.1.03 ISR por Pagar         $2,500.00  (10% retención)      │
│                                                                         │
│  💸 COMISIÓN STRIPE (3.5% de $1,000)                                    │
│  ─────────────────────────────────────                                  │
│  Débito:  5.1.09.01 Comisiones Stripe  $35.00                          │
│  Crédito: 1.1.02 Bancos                $35.00                          │
│  (Descuento automático del depósito)                                   │
│                                                                         │
│  💸 COMISIÓN AZUL (2.5% de $1,000)                                      │
│  ──────────────────────────────────                                     │
│  Débito:  5.1.09.02 Comisiones AZUL    $29.50  ($25 + $4.50 ITBIS)     │
│  Débito:  1.1.04 ITBIS Pagado           $4.50                          │
│  Crédito: 1.1.02 Bancos                $29.50                          │
│                                                                         │
│  📧 PUBLICIDAD GOOGLE ADS ($500 USD = RD$30,000)                        │
│  ─────────────────────────────────────────────────                      │
│  Débito:  5.1.03 Gastos Publicidad    $30,000.00                       │
│  Crédito: 1.1.02 Bancos               $30,000.00                       │
│  (Sin ITBIS - Gasto del exterior)                                      │
│                                                                         │
│  📄 NOTA DE CRÉDITO (Reembolso $29)                                     │
│  ──────────────────────────────────                                     │
│  Débito:  4.1.01 Ingresos Listings    $29.00                           │
│  Débito:  2.1.02 ITBIS por Pagar       $5.22                           │
│  Crédito: 1.1.02 Bancos               $34.22                           │
│                                                                         │
│  👷 NÓMINA MENSUAL (1 empleado $50,000)                                 │
│  ─────────────────────────────────────                                  │
│  Débito:  5.1.01.01 Sueldos           $50,000.00                       │
│  Débito:  5.1.01.03 TSS Patronal       $7,100.00  (14.2%)              │
│  Crédito: 2.1.05 Sueldos por Pagar    $46,200.00  (neto a empleado)    │
│  Crédito: 2.1.06 TSS por Pagar        $10,900.00  (empleado + patronal)│
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 3. BASE DE DATOS CONTABLE

### 3.1 Schema Completo

```sql
-- ═══════════════════════════════════════════════════════════════════════
-- CATÁLOGO DE CUENTAS
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE chart_of_accounts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(20) NOT NULL UNIQUE,        -- 1.1.02.01
    name VARCHAR(200) NOT NULL,               -- Banco Popular
    parent_code VARCHAR(20),                  -- 1.1.02
    account_type VARCHAR(20) NOT NULL,        -- ACTIVO, PASIVO, PATRIMONIO, INGRESO, GASTO
    nature VARCHAR(10) NOT NULL,              -- DEBITO, CREDITO
    level INTEGER NOT NULL,                   -- 1, 2, 3, 4
    is_active BOOLEAN DEFAULT true,
    allows_transactions BOOLEAN DEFAULT true, -- Solo hojas permiten transacciones
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insertar cuentas principales
INSERT INTO chart_of_accounts (code, name, account_type, nature, level, allows_transactions) VALUES
-- ACTIVOS
('1', 'ACTIVOS', 'ACTIVO', 'DEBITO', 1, false),
('1.1', 'Activos Corrientes', 'ACTIVO', 'DEBITO', 2, false),
('1.1.01', 'Caja', 'ACTIVO', 'DEBITO', 3, true),
('1.1.02', 'Bancos', 'ACTIVO', 'DEBITO', 3, false),
('1.1.02.01', 'Banco Popular', 'ACTIVO', 'DEBITO', 4, true),
('1.1.02.02', 'BHD León', 'ACTIVO', 'DEBITO', 4, true),
('1.1.03', 'Cuentas por Cobrar', 'ACTIVO', 'DEBITO', 3, true),
('1.1.04', 'ITBIS Pagado', 'ACTIVO', 'DEBITO', 3, true),
('1.1.05', 'Anticipos a Proveedores', 'ACTIVO', 'DEBITO', 3, true),
('1.1.06', 'Retenciones por Aplicar', 'ACTIVO', 'DEBITO', 3, true),
-- PASIVOS
('2', 'PASIVOS', 'PASIVO', 'CREDITO', 1, false),
('2.1', 'Pasivos Corrientes', 'PASIVO', 'CREDITO', 2, false),
('2.1.01', 'Cuentas por Pagar', 'PASIVO', 'CREDITO', 3, true),
('2.1.02', 'ITBIS por Pagar', 'PASIVO', 'CREDITO', 3, true),
('2.1.03', 'ISR por Pagar', 'PASIVO', 'CREDITO', 3, true),
('2.1.04', 'Impuestos por Pagar', 'PASIVO', 'CREDITO', 3, true),
('2.1.05', 'Sueldos por Pagar', 'PASIVO', 'CREDITO', 3, true),
('2.1.06', 'TSS por Pagar', 'PASIVO', 'CREDITO', 3, true),
('2.1.07', 'Ingresos Diferidos', 'PASIVO', 'CREDITO', 3, true),
-- PATRIMONIO
('3', 'PATRIMONIO', 'PATRIMONIO', 'CREDITO', 1, false),
('3.1.01', 'Capital Social', 'PATRIMONIO', 'CREDITO', 3, true),
('3.1.02', 'Reserva Legal', 'PATRIMONIO', 'CREDITO', 3, true),
('3.1.03', 'Utilidades Retenidas', 'PATRIMONIO', 'CREDITO', 3, true),
('3.1.04', 'Utilidad del Ejercicio', 'PATRIMONIO', 'CREDITO', 3, true),
-- INGRESOS
('4', 'INGRESOS', 'INGRESO', 'CREDITO', 1, false),
('4.1', 'Ingresos Operacionales', 'INGRESO', 'CREDITO', 2, false),
('4.1.01', 'Ingresos por Listings', 'INGRESO', 'CREDITO', 3, true),
('4.1.02', 'Ingresos por Suscripciones', 'INGRESO', 'CREDITO', 3, true),
('4.1.03', 'Ingresos por Boosts', 'INGRESO', 'CREDITO', 3, true),
('4.1.04', 'Ingresos por Publicidad', 'INGRESO', 'CREDITO', 3, true),
-- GASTOS
('5', 'COSTOS Y GASTOS', 'GASTO', 'DEBITO', 1, false),
('5.1', 'Gastos Operacionales', 'GASTO', 'DEBITO', 2, false),
('5.1.01', 'Gastos de Personal', 'GASTO', 'DEBITO', 3, false),
('5.1.01.01', 'Sueldos y Salarios', 'GASTO', 'DEBITO', 4, true),
('5.1.01.02', 'Bonificaciones', 'GASTO', 'DEBITO', 4, true),
('5.1.01.03', 'TSS Patronal', 'GASTO', 'DEBITO', 4, true),
('5.1.02', 'Gastos de Hosting', 'GASTO', 'DEBITO', 3, true),
('5.1.03', 'Gastos de Publicidad', 'GASTO', 'DEBITO', 3, true),
('5.1.04', 'Gastos de Software', 'GASTO', 'DEBITO', 3, true),
('5.1.05', 'Telecomunicaciones', 'GASTO', 'DEBITO', 3, true),
('5.1.06', 'Servicios Profesionales', 'GASTO', 'DEBITO', 3, false),
('5.1.06.01', 'Honorarios Contables', 'GASTO', 'DEBITO', 4, true),
('5.1.06.02', 'Honorarios Legales', 'GASTO', 'DEBITO', 4, true),
('5.1.07', 'Alquiler', 'GASTO', 'DEBITO', 3, true),
('5.1.08', 'Electricidad', 'GASTO', 'DEBITO', 3, true),
('5.1.09', 'Comisiones Pasarelas', 'GASTO', 'DEBITO', 3, false),
('5.1.09.01', 'Comisiones Stripe', 'GASTO', 'DEBITO', 4, true),
('5.1.09.02', 'Comisiones AZUL', 'GASTO', 'DEBITO', 4, true),
('5.1.10', 'Gastos Bancarios', 'GASTO', 'DEBITO', 3, true);

-- ═══════════════════════════════════════════════════════════════════════
-- LIBRO DIARIO
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE journal_entries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    entry_number SERIAL UNIQUE,               -- Número secuencial
    entry_date DATE NOT NULL,                 -- Fecha del asiento
    description VARCHAR(500) NOT NULL,        -- Descripción
    reference_type VARCHAR(50),               -- PAYMENT, EXPENSE, INVOICE, PAYROLL
    reference_id UUID,                        -- ID de la transacción origen
    total_debit DECIMAL(18,2) NOT NULL,
    total_credit DECIMAL(18,2) NOT NULL,
    status VARCHAR(20) DEFAULT 'POSTED',      -- DRAFT, POSTED, REVERSED
    period_year INTEGER NOT NULL,
    period_month INTEGER NOT NULL,
    is_closing_entry BOOLEAN DEFAULT false,   -- Asiento de cierre
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100),
    reversed_by_entry_id UUID,                -- Si fue reversado
    notes TEXT
);

CREATE TABLE journal_entry_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    journal_entry_id UUID NOT NULL REFERENCES journal_entries(id),
    line_number INTEGER NOT NULL,
    account_code VARCHAR(20) NOT NULL REFERENCES chart_of_accounts(code),
    description VARCHAR(300),
    debit_amount DECIMAL(18,2) DEFAULT 0,
    credit_amount DECIMAL(18,2) DEFAULT 0,
    CONSTRAINT check_debit_or_credit CHECK (
        (debit_amount > 0 AND credit_amount = 0) OR
        (debit_amount = 0 AND credit_amount > 0)
    )
);

-- ═══════════════════════════════════════════════════════════════════════
-- LIBRO MAYOR (Vista materializada para performance)
-- ═══════════════════════════════════════════════════════════════════════

CREATE MATERIALIZED VIEW general_ledger AS
SELECT
    coa.code AS account_code,
    coa.name AS account_name,
    coa.account_type,
    coa.nature,
    je.period_year,
    je.period_month,
    SUM(jel.debit_amount) AS total_debits,
    SUM(jel.credit_amount) AS total_credits,
    CASE
        WHEN coa.nature = 'DEBITO' THEN SUM(jel.debit_amount) - SUM(jel.credit_amount)
        ELSE SUM(jel.credit_amount) - SUM(jel.debit_amount)
    END AS balance
FROM journal_entry_lines jel
JOIN journal_entries je ON jel.journal_entry_id = je.id
JOIN chart_of_accounts coa ON jel.account_code = coa.code
WHERE je.status = 'POSTED'
GROUP BY
    coa.code,
    coa.name,
    coa.account_type,
    coa.nature,
    je.period_year,
    je.period_month;

CREATE UNIQUE INDEX idx_gl_account_period
ON general_ledger(account_code, period_year, period_month);

-- Función para refrescar el libro mayor
CREATE OR REPLACE FUNCTION refresh_general_ledger()
RETURNS TRIGGER AS $$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY general_ledger;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_refresh_general_ledger
AFTER INSERT OR UPDATE OR DELETE ON journal_entries
FOR EACH STATEMENT
EXECUTE FUNCTION refresh_general_ledger();

-- ═══════════════════════════════════════════════════════════════════════
-- LIBRO DE COMPRAS (Para Formato 606)
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE purchase_ledger (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    entry_date DATE NOT NULL,
    supplier_rnc VARCHAR(15),
    supplier_name VARCHAR(200) NOT NULL,
    supplier_type VARCHAR(20) NOT NULL,       -- LOCAL, INTERNACIONAL
    ncf_ecf VARCHAR(30),                       -- NCF o e-CF del proveedor
    invoice_date DATE NOT NULL,
    payment_date DATE,
    subtotal DECIMAL(18,2) NOT NULL,
    itbis_amount DECIMAL(18,2) DEFAULT 0,
    isr_withheld DECIMAL(18,2) DEFAULT 0,     -- Retención ISR (10%)
    total DECIMAL(18,2) NOT NULL,
    expense_type_code VARCHAR(2) NOT NULL,    -- 01-11
    payment_method_code VARCHAR(2) NOT NULL,  -- 01-05
    expense_id UUID,                          -- Referencia a ExpenseService
    journal_entry_id UUID REFERENCES journal_entries(id),
    period_year INTEGER NOT NULL,
    period_month INTEGER NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_purchase_period ON purchase_ledger(period_year, period_month);

-- ═══════════════════════════════════════════════════════════════════════
-- LIBRO DE VENTAS (Para Formato 607)
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE sales_ledger (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    entry_date DATE NOT NULL,
    customer_rnc VARCHAR(15),
    customer_name VARCHAR(200) NOT NULL,
    ecf_number VARCHAR(20) NOT NULL,           -- e-CF emitido
    ecf_type INTEGER NOT NULL,                 -- 31, 32, 34
    invoice_date DATE NOT NULL,
    subtotal DECIMAL(18,2) NOT NULL,
    itbis_amount DECIMAL(18,2) NOT NULL,
    total DECIMAL(18,2) NOT NULL,
    income_type_code VARCHAR(2) NOT NULL,     -- 01-04
    payment_id UUID,                          -- Referencia a BillingService
    electronic_invoice_id UUID,               -- Referencia a ECFService
    journal_entry_id UUID REFERENCES journal_entries(id),
    period_year INTEGER NOT NULL,
    period_month INTEGER NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_sales_period ON sales_ledger(period_year, period_month);

-- ═══════════════════════════════════════════════════════════════════════
-- LIBRO DE RETENCIONES (Para IR-17)
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE withholding_ledger (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    entry_date DATE NOT NULL,
    -- Retención Realizada (OKLA retiene a proveedor)
    supplier_rnc VARCHAR(15),
    supplier_name VARCHAR(200),
    supplier_type VARCHAR(20),                -- PERSONA_FISICA, EMPRESA
    service_type VARCHAR(50),                 -- PROFESIONAL, ALQUILER, etc.
    base_amount DECIMAL(18,2),
    withholding_rate DECIMAL(5,2),            -- 10%
    isr_withheld DECIMAL(18,2),
    -- Retención Recibida (Cliente retiene a OKLA) - raro pero posible
    is_received BOOLEAN DEFAULT false,
    customer_rnc VARCHAR(15),
    isr_received DECIMAL(18,2),
    -- Referencias
    purchase_ledger_id UUID REFERENCES purchase_ledger(id),
    sales_ledger_id UUID REFERENCES sales_ledger(id),
    journal_entry_id UUID REFERENCES journal_entries(id),
    period_year INTEGER NOT NULL,
    period_month INTEGER NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_withholding_period ON withholding_ledger(period_year, period_month);

-- ═══════════════════════════════════════════════════════════════════════
-- LIBRO DE BANCO
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE bank_ledger (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    bank_account_code VARCHAR(20) NOT NULL,   -- 1.1.02.01, 1.1.02.02
    bank_name VARCHAR(100) NOT NULL,
    transaction_date DATE NOT NULL,
    value_date DATE,                          -- Fecha valor
    reference_number VARCHAR(50),             -- Número de referencia
    description VARCHAR(300) NOT NULL,
    transaction_type VARCHAR(20) NOT NULL,    -- DEPOSITO, RETIRO, TRANSFERENCIA, COMISION
    debit_amount DECIMAL(18,2) DEFAULT 0,
    credit_amount DECIMAL(18,2) DEFAULT 0,
    balance DECIMAL(18,2),                    -- Saldo después de transacción
    is_reconciled BOOLEAN DEFAULT false,
    reconciliation_date DATE,
    bank_statement_line_id UUID,              -- Match con estado de cuenta
    journal_entry_id UUID REFERENCES journal_entries(id),
    period_year INTEGER NOT NULL,
    period_month INTEGER NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_bank_account_period ON bank_ledger(bank_account_code, period_year, period_month);

-- ═══════════════════════════════════════════════════════════════════════
-- PERÍODOS CONTABLES
-- ═══════════════════════════════════════════════════════════════════════

CREATE TABLE accounting_periods (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    period_year INTEGER NOT NULL,
    period_month INTEGER NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    status VARCHAR(20) DEFAULT 'OPEN',        -- OPEN, CLOSED, LOCKED
    closed_at TIMESTAMP,
    closed_by VARCHAR(100),
    UNIQUE(period_year, period_month)
);

-- Inicializar períodos 2026
INSERT INTO accounting_periods (period_year, period_month, start_date, end_date) VALUES
(2026, 1, '2026-01-01', '2026-01-31'),
(2026, 2, '2026-02-01', '2026-02-28'),
(2026, 3, '2026-03-01', '2026-03-31'),
(2026, 4, '2026-04-01', '2026-04-30'),
(2026, 5, '2026-05-01', '2026-05-31'),
(2026, 6, '2026-06-01', '2026-06-30'),
(2026, 7, '2026-07-01', '2026-07-31'),
(2026, 8, '2026-08-01', '2026-08-31'),
(2026, 9, '2026-09-01', '2026-09-30'),
(2026, 10, '2026-10-01', '2026-10-31'),
(2026, 11, '2026-11-01', '2026-11-30'),
(2026, 12, '2026-12-01', '2026-12-31');

-- ═══════════════════════════════════════════════════════════════════════
-- ÍNDICES ADICIONALES
-- ═══════════════════════════════════════════════════════════════════════

CREATE INDEX idx_je_period ON journal_entries(period_year, period_month);
CREATE INDEX idx_je_date ON journal_entries(entry_date);
CREATE INDEX idx_je_reference ON journal_entries(reference_type, reference_id);
CREATE INDEX idx_jel_account ON journal_entry_lines(account_code);
```

---

## 4. SERVICIO DE CONTABILIDAD

### 4.1 AccountingService - Estructura

```csharp
// AccountingService.Application/Services/JournalEntryService.cs

public class JournalEntryService : IJournalEntryService
{
    private readonly IAccountingDbContext _context;
    private readonly ILogger<JournalEntryService> _logger;

    /// <summary>
    /// Crear asiento contable desde un pago (venta)
    /// </summary>
    public async Task<JournalEntry> CreateFromPaymentAsync(PaymentCompletedEvent payment)
    {
        var entry = new JournalEntry
        {
            EntryDate = payment.PaymentDate,
            Description = $"Venta {payment.ProductType} - {payment.CustomerName}",
            ReferenceType = "PAYMENT",
            ReferenceId = payment.PaymentId,
            PeriodYear = payment.PaymentDate.Year,
            PeriodMonth = payment.PaymentDate.Month
        };

        // Líneas del asiento
        var lines = new List<JournalEntryLine>();

        // Débito: Bancos (total recibido)
        lines.Add(new JournalEntryLine
        {
            LineNumber = 1,
            AccountCode = GetBankAccountCode(payment.PaymentMethod), // 1.1.02.01 o 1.1.02.02
            Description = $"Cobro {payment.ECFNumber}",
            DebitAmount = payment.Total
        });

        // Crédito: Ingresos (subtotal sin ITBIS)
        lines.Add(new JournalEntryLine
        {
            LineNumber = 2,
            AccountCode = GetIncomeAccountCode(payment.ProductType), // 4.1.01, 4.1.02, etc.
            Description = $"{payment.ProductType}",
            CreditAmount = payment.Subtotal
        });

        // Crédito: ITBIS por Pagar
        lines.Add(new JournalEntryLine
        {
            LineNumber = 3,
            AccountCode = "2.1.02", // ITBIS por Pagar
            Description = "ITBIS 18%",
            CreditAmount = payment.TaxAmount
        });

        entry.Lines = lines;
        entry.TotalDebit = lines.Sum(l => l.DebitAmount);
        entry.TotalCredit = lines.Sum(l => l.CreditAmount);

        // Validar partida doble
        if (entry.TotalDebit != entry.TotalCredit)
            throw new AccountingException("Asiento no cuadra: débitos ≠ créditos");

        await _context.JournalEntries.AddAsync(entry);

        // Registrar en libro de ventas
        await CreateSalesLedgerEntryAsync(payment, entry.Id);

        // Registrar en libro de banco
        await CreateBankLedgerEntryAsync(payment, entry.Id);

        await _context.SaveChangesAsync();

        return entry;
    }

    /// <summary>
    /// Crear asiento contable desde un gasto
    /// </summary>
    public async Task<JournalEntry> CreateFromExpenseAsync(ExpenseRegisteredEvent expense)
    {
        var entry = new JournalEntry
        {
            EntryDate = expense.PaymentDate,
            Description = $"Gasto {expense.Category} - {expense.SupplierName}",
            ReferenceType = "EXPENSE",
            ReferenceId = expense.ExpenseId,
            PeriodYear = expense.PaymentDate.Year,
            PeriodMonth = expense.PaymentDate.Month
        };

        var lines = new List<JournalEntryLine>();
        var lineNumber = 1;

        // Débito: Cuenta de gasto
        lines.Add(new JournalEntryLine
        {
            LineNumber = lineNumber++,
            AccountCode = GetExpenseAccountCode(expense.Category),
            Description = expense.Description,
            DebitAmount = expense.Subtotal
        });

        // Débito: ITBIS Pagado (si aplica)
        if (expense.ITBISAmount > 0)
        {
            lines.Add(new JournalEntryLine
            {
                LineNumber = lineNumber++,
                AccountCode = "1.1.04", // ITBIS Pagado
                Description = "ITBIS crédito fiscal",
                DebitAmount = expense.ITBISAmount
            });
        }

        // Crédito: Bancos (neto pagado)
        lines.Add(new JournalEntryLine
        {
            LineNumber = lineNumber++,
            AccountCode = GetBankAccountCode(expense.PaymentMethod),
            Description = $"Pago a {expense.SupplierName}",
            CreditAmount = expense.NetPaid
        });

        // Crédito: ISR por Pagar (si hubo retención)
        if (expense.ISRWithheld > 0)
        {
            lines.Add(new JournalEntryLine
            {
                LineNumber = lineNumber++,
                AccountCode = "2.1.03", // ISR por Pagar
                Description = "Retención ISR 10%",
                CreditAmount = expense.ISRWithheld
            });
        }

        entry.Lines = lines;
        entry.TotalDebit = lines.Sum(l => l.DebitAmount);
        entry.TotalCredit = lines.Sum(l => l.CreditAmount);

        // Validar partida doble
        if (Math.Abs(entry.TotalDebit - entry.TotalCredit) > 0.01m)
            throw new AccountingException($"Asiento no cuadra: {entry.TotalDebit} ≠ {entry.TotalCredit}");

        await _context.JournalEntries.AddAsync(entry);

        // Registrar en libro de compras
        await CreatePurchaseLedgerEntryAsync(expense, entry.Id);

        // Registrar retención (si aplica)
        if (expense.ISRWithheld > 0)
            await CreateWithholdingLedgerEntryAsync(expense, entry.Id);

        // Registrar en libro de banco
        await CreateBankLedgerEntryAsync(expense, entry.Id);

        await _context.SaveChangesAsync();

        return entry;
    }
}
```

---

## 5. GENERACIÓN AUTOMÁTICA DE REPORTES

### 5.1 API de Reportes Contables

```csharp
// AccountingService.Api/Controllers/AccountingReportsController.cs

[ApiController]
[Route("api/accounting/reports")]
[Authorize(Roles = "Admin,Accountant")]
public class AccountingReportsController : ControllerBase
{
    private readonly IAccountingReportService _reportService;

    /// <summary>
    /// Generar Libro Diario
    /// </summary>
    [HttpGet("journal")]
    public async Task<IActionResult> GetJournalReport(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] string format = "excel")
    {
        var report = await _reportService.GenerateJournalReportAsync(year, month);

        if (format == "pdf")
            return File(report.PdfBytes, "application/pdf", $"libro-diario-{year}-{month:D2}.pdf");

        return File(report.ExcelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"libro-diario-{year}-{month:D2}.xlsx");
    }

    /// <summary>
    /// Generar Libro Mayor
    /// </summary>
    [HttpGet("general-ledger")]
    public async Task<IActionResult> GetGeneralLedgerReport(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] string? accountCode = null)
    {
        var report = await _reportService.GenerateGeneralLedgerReportAsync(year, month, accountCode);
        return File(report.ExcelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"libro-mayor-{year}-{month:D2}.xlsx");
    }

    /// <summary>
    /// Generar Libro de Compras
    /// </summary>
    [HttpGet("purchases")]
    public async Task<IActionResult> GetPurchaseLedgerReport(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var report = await _reportService.GeneratePurchaseLedgerReportAsync(year, month);
        return File(report.ExcelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"libro-compras-{year}-{month:D2}.xlsx");
    }

    /// <summary>
    /// Generar Libro de Ventas
    /// </summary>
    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesLedgerReport(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var report = await _reportService.GenerateSalesLedgerReportAsync(year, month);
        return File(report.ExcelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"libro-ventas-{year}-{month:D2}.xlsx");
    }

    /// <summary>
    /// Generar Balance de Comprobación
    /// </summary>
    [HttpGet("trial-balance")]
    public async Task<IActionResult> GetTrialBalanceReport(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var report = await _reportService.GenerateTrialBalanceReportAsync(year, month);
        return File(report.ExcelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"balance-comprobacion-{year}-{month:D2}.xlsx");
    }

    /// <summary>
    /// Generar Estado de Resultados
    /// </summary>
    [HttpGet("income-statement")]
    public async Task<IActionResult> GetIncomeStatementReport(
        [FromQuery] int year,
        [FromQuery] int? month = null)  // null = anual
    {
        var report = await _reportService.GenerateIncomeStatementReportAsync(year, month);
        var filename = month.HasValue
            ? $"estado-resultados-{year}-{month:D2}.xlsx"
            : $"estado-resultados-{year}.xlsx";
        return File(report.ExcelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            filename);
    }

    /// <summary>
    /// Generar Balance General
    /// </summary>
    [HttpGet("balance-sheet")]
    public async Task<IActionResult> GetBalanceSheetReport(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var report = await _reportService.GenerateBalanceSheetReportAsync(year, month);
        return File(report.ExcelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"balance-general-{year}-{month:D2}.xlsx");
    }

    /// <summary>
    /// Generar TODOS los libros para un período (para auditoría)
    /// </summary>
    [HttpPost("audit-package")]
    public async Task<IActionResult> GenerateAuditPackage(
        [FromBody] AuditPackageRequest request)
    {
        // request.StartYear, request.StartMonth, request.EndYear, request.EndMonth

        var package = await _reportService.GenerateCompleteAuditPackageAsync(
            request.StartYear, request.StartMonth,
            request.EndYear, request.EndMonth);

        return Ok(new {
            packageId = package.Id,
            downloadUrl = package.ZipFileUrl,
            generatedAt = package.GeneratedAt,
            reports = package.IncludedReports
        });
    }
}
```

### 5.2 Generador de Paquete para Auditoría

```csharp
// AccountingService.Application/Services/AuditPackageService.cs

public class AuditPackageService : IAuditPackageService
{
    public async Task<AccountingAuditPackage> GenerateCompleteAuditPackageAsync(
        int startYear, int startMonth,
        int endYear, int endMonth)
    {
        var packageId = Guid.NewGuid();
        var tempDir = Path.Combine(Path.GetTempPath(), packageId.ToString());
        Directory.CreateDirectory(tempDir);

        var includedReports = new List<string>();

        try
        {
            // 1. Generar Libro Diario por cada mes
            var journalDir = Path.Combine(tempDir, "01-LIBRO-DIARIO");
            Directory.CreateDirectory(journalDir);

            for (var date = new DateTime(startYear, startMonth, 1);
                 date <= new DateTime(endYear, endMonth, 1);
                 date = date.AddMonths(1))
            {
                var report = await _reportService.GenerateJournalReportAsync(date.Year, date.Month);
                var filename = $"libro-diario-{date:yyyy-MM}.xlsx";
                await File.WriteAllBytesAsync(Path.Combine(journalDir, filename), report.ExcelBytes);
                includedReports.Add($"01-LIBRO-DIARIO/{filename}");
            }

            // 2. Generar Libro Mayor por cada mes
            var ledgerDir = Path.Combine(tempDir, "02-LIBRO-MAYOR");
            Directory.CreateDirectory(ledgerDir);

            for (var date = new DateTime(startYear, startMonth, 1);
                 date <= new DateTime(endYear, endMonth, 1);
                 date = date.AddMonths(1))
            {
                var report = await _reportService.GenerateGeneralLedgerReportAsync(date.Year, date.Month);
                var filename = $"libro-mayor-{date:yyyy-MM}.xlsx";
                await File.WriteAllBytesAsync(Path.Combine(ledgerDir, filename), report.ExcelBytes);
                includedReports.Add($"02-LIBRO-MAYOR/{filename}");
            }

            // 3. Libro de Compras
            var purchasesDir = Path.Combine(tempDir, "03-LIBRO-COMPRAS");
            Directory.CreateDirectory(purchasesDir);

            for (var date = new DateTime(startYear, startMonth, 1);
                 date <= new DateTime(endYear, endMonth, 1);
                 date = date.AddMonths(1))
            {
                var report = await _reportService.GeneratePurchaseLedgerReportAsync(date.Year, date.Month);
                var filename = $"libro-compras-{date:yyyy-MM}.xlsx";
                await File.WriteAllBytesAsync(Path.Combine(purchasesDir, filename), report.ExcelBytes);
                includedReports.Add($"03-LIBRO-COMPRAS/{filename}");
            }

            // 4. Libro de Ventas
            var salesDir = Path.Combine(tempDir, "04-LIBRO-VENTAS");
            Directory.CreateDirectory(salesDir);

            for (var date = new DateTime(startYear, startMonth, 1);
                 date <= new DateTime(endYear, endMonth, 1);
                 date = date.AddMonths(1))
            {
                var report = await _reportService.GenerateSalesLedgerReportAsync(date.Year, date.Month);
                var filename = $"libro-ventas-{date:yyyy-MM}.xlsx";
                await File.WriteAllBytesAsync(Path.Combine(salesDir, filename), report.ExcelBytes);
                includedReports.Add($"04-LIBRO-VENTAS/{filename}");
            }

            // 5. Balance de Comprobación mensual
            var trialDir = Path.Combine(tempDir, "05-BALANCES-COMPROBACION");
            Directory.CreateDirectory(trialDir);

            for (var date = new DateTime(startYear, startMonth, 1);
                 date <= new DateTime(endYear, endMonth, 1);
                 date = date.AddMonths(1))
            {
                var report = await _reportService.GenerateTrialBalanceReportAsync(date.Year, date.Month);
                var filename = $"balance-comprobacion-{date:yyyy-MM}.xlsx";
                await File.WriteAllBytesAsync(Path.Combine(trialDir, filename), report.ExcelBytes);
                includedReports.Add($"05-BALANCES-COMPROBACION/{filename}");
            }

            // 6. Estados Financieros
            var financialsDir = Path.Combine(tempDir, "06-ESTADOS-FINANCIEROS");
            Directory.CreateDirectory(financialsDir);

            // Estado de resultados por año
            var years = Enumerable.Range(startYear, endYear - startYear + 1);
            foreach (var year in years)
            {
                var income = await _reportService.GenerateIncomeStatementReportAsync(year, null);
                await File.WriteAllBytesAsync(
                    Path.Combine(financialsDir, $"estado-resultados-{year}.xlsx"),
                    income.ExcelBytes);
                includedReports.Add($"06-ESTADOS-FINANCIEROS/estado-resultados-{year}.xlsx");
            }

            // Balance general al cierre del período
            var balance = await _reportService.GenerateBalanceSheetReportAsync(endYear, endMonth);
            await File.WriteAllBytesAsync(
                Path.Combine(financialsDir, $"balance-general-{endYear}-{endMonth:D2}.xlsx"),
                balance.ExcelBytes);
            includedReports.Add($"06-ESTADOS-FINANCIEROS/balance-general-{endYear}-{endMonth:D2}.xlsx");

            // 7. Libro de Retenciones
            var withholdingDir = Path.Combine(tempDir, "07-LIBRO-RETENCIONES");
            Directory.CreateDirectory(withholdingDir);

            for (var date = new DateTime(startYear, startMonth, 1);
                 date <= new DateTime(endYear, endMonth, 1);
                 date = date.AddMonths(1))
            {
                var report = await _reportService.GenerateWithholdingLedgerReportAsync(date.Year, date.Month);
                var filename = $"libro-retenciones-{date:yyyy-MM}.xlsx";
                await File.WriteAllBytesAsync(Path.Combine(withholdingDir, filename), report.ExcelBytes);
                includedReports.Add($"07-LIBRO-RETENCIONES/{filename}");
            }

            // 8. Libro de Banco
            var bankDir = Path.Combine(tempDir, "08-LIBRO-BANCO");
            Directory.CreateDirectory(bankDir);

            for (var date = new DateTime(startYear, startMonth, 1);
                 date <= new DateTime(endYear, endMonth, 1);
                 date = date.AddMonths(1))
            {
                var report = await _reportService.GenerateBankLedgerReportAsync(date.Year, date.Month);
                var filename = $"libro-banco-{date:yyyy-MM}.xlsx";
                await File.WriteAllBytesAsync(Path.Combine(bankDir, filename), report.ExcelBytes);
                includedReports.Add($"08-LIBRO-BANCO/{filename}");
            }

            // 9. Generar índice
            await GenerateIndexDocumentAsync(tempDir, includedReports,
                startYear, startMonth, endYear, endMonth);

            // 10. Comprimir todo
            var zipPath = Path.Combine(Path.GetTempPath(),
                $"libros-contables-okla-{startYear}{startMonth:D2}-{endYear}{endMonth:D2}.zip");
            ZipFile.CreateFromDirectory(tempDir, zipPath);

            // 11. Subir a S3
            var zipKey = $"audit-packages/{packageId}/libros-contables.zip";
            var zipUrl = await _s3.UploadFileAsync(zipKey, zipPath);

            // 12. Registrar paquete
            var package = new AccountingAuditPackage
            {
                Id = packageId,
                StartYear = startYear,
                StartMonth = startMonth,
                EndYear = endYear,
                EndMonth = endMonth,
                ZipFileUrl = zipUrl,
                IncludedReports = includedReports,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = _currentUser.Id
            };

            await _repository.AddAsync(package);

            return package;
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
```

---

## 6. DASHBOARD DE CONTABILIDAD

### 6.1 Frontend - Dashboard Contable

```typescript
// frontend/web/src/pages/admin/AccountingDashboard.tsx

export const AccountingDashboard = () => {
  const [selectedYear, setSelectedYear] = useState(2026);
  const [selectedMonth, setSelectedMonth] = useState(1);

  const { data: summary } = useQuery({
    queryKey: ['accounting-summary', selectedYear, selectedMonth],
    queryFn: () => accountingService.getMonthlySummary(selectedYear, selectedMonth)
  });

  return (
    <div className="p-6 space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold">Contabilidad</h1>

        {/* Selector de período */}
        <div className="flex gap-2">
          <Select value={selectedYear} onValueChange={setSelectedYear}>
            <SelectTrigger className="w-24">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {[2025, 2026, 2027].map(y => (
                <SelectItem key={y} value={y}>{y}</SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select value={selectedMonth} onValueChange={setSelectedMonth}>
            <SelectTrigger className="w-32">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {MONTHS.map((m, i) => (
                <SelectItem key={i} value={i + 1}>{m}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* Resumen del período */}
      <div className="grid grid-cols-4 gap-4">
        <StatCard
          title="Ingresos"
          value={formatCurrency(summary?.totalIncome)}
          icon={<TrendingUp className="text-green-500" />}
          change={summary?.incomeChange}
        />
        <StatCard
          title="Gastos"
          value={formatCurrency(summary?.totalExpenses)}
          icon={<TrendingDown className="text-red-500" />}
          change={summary?.expensesChange}
        />
        <StatCard
          title="ITBIS por Pagar"
          value={formatCurrency(summary?.itbisToPay)}
          icon={<Receipt />}
        />
        <StatCard
          title="Utilidad Neta"
          value={formatCurrency(summary?.netIncome)}
          icon={<DollarSign className={summary?.netIncome > 0 ? 'text-green-500' : 'text-red-500'} />}
        />
      </div>

      {/* Acciones Rápidas - Libros Contables */}
      <Card>
        <CardHeader>
          <CardTitle>📚 Libros Contables</CardTitle>
          <CardDescription>
            Genera reportes para el período seleccionado
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-4 gap-4">
            <Button
              variant="outline"
              onClick={() => downloadReport('journal')}
              className="flex flex-col h-24 items-center justify-center"
            >
              <Book className="h-8 w-8 mb-2" />
              <span>Libro Diario</span>
            </Button>

            <Button
              variant="outline"
              onClick={() => downloadReport('general-ledger')}
              className="flex flex-col h-24 items-center justify-center"
            >
              <BookOpen className="h-8 w-8 mb-2" />
              <span>Libro Mayor</span>
            </Button>

            <Button
              variant="outline"
              onClick={() => downloadReport('purchases')}
              className="flex flex-col h-24 items-center justify-center"
            >
              <ShoppingCart className="h-8 w-8 mb-2" />
              <span>Libro Compras</span>
            </Button>

            <Button
              variant="outline"
              onClick={() => downloadReport('sales')}
              className="flex flex-col h-24 items-center justify-center"
            >
              <DollarSign className="h-8 w-8 mb-2" />
              <span>Libro Ventas</span>
            </Button>

            <Button
              variant="outline"
              onClick={() => downloadReport('trial-balance')}
              className="flex flex-col h-24 items-center justify-center"
            >
              <Scale className="h-8 w-8 mb-2" />
              <span>Balance Comprobación</span>
            </Button>

            <Button
              variant="outline"
              onClick={() => downloadReport('withholdings')}
              className="flex flex-col h-24 items-center justify-center"
            >
              <FileText className="h-8 w-8 mb-2" />
              <span>Libro Retenciones</span>
            </Button>

            <Button
              variant="outline"
              onClick={() => downloadReport('bank')}
              className="flex flex-col h-24 items-center justify-center"
            >
              <Building className="h-8 w-8 mb-2" />
              <span>Libro Banco</span>
            </Button>

            <Button
              variant="default"
              onClick={() => setShowAuditModal(true)}
              className="flex flex-col h-24 items-center justify-center bg-blue-600"
            >
              <Archive className="h-8 w-8 mb-2" />
              <span>📦 Paquete Auditoría</span>
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Estados Financieros */}
      <Card>
        <CardHeader>
          <CardTitle>📊 Estados Financieros</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-3 gap-4">
            <Button
              variant="outline"
              onClick={() => downloadReport('income-statement')}
            >
              <FileSpreadsheet className="mr-2" />
              Estado de Resultados
            </Button>

            <Button
              variant="outline"
              onClick={() => downloadReport('balance-sheet')}
            >
              <FileSpreadsheet className="mr-2" />
              Balance General
            </Button>

            <Button
              variant="outline"
              onClick={() => downloadReport('cash-flow')}
            >
              <FileSpreadsheet className="mr-2" />
              Flujo de Efectivo
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Modal para generar paquete de auditoría */}
      <AuditPackageModal
        open={showAuditModal}
        onClose={() => setShowAuditModal(false)}
        onGenerate={handleGenerateAuditPackage}
      />
    </div>
  );
};
```

---

## 7. RESPUESTA AUTOMÁTICA A AUDITORÍA

### 7.1 Flujo de Respuesta a Auditor DGII

```
┌─────────────────────────────────────────────────────────────────────────┐
│          RESPUESTA AUTOMÁTICA A SOLICITUD DE AUDITORÍA                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1️⃣ AUDITOR DGII SOLICITA LIBROS CONTABLES                              │
│     • Requerimiento: "Presentar libros contables Enero-Diciembre 2026"  │
│     • Plazo: 5 días hábiles                                             │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  2️⃣ ADMIN ACCEDE AL DASHBOARD                                           │
│     • Navega a: /admin/accounting                                       │
│     • Click en "📦 Paquete Auditoría"                                    │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  3️⃣ SELECCIONA PERÍODO                                                  │
│     • Fecha inicio: Enero 2026                                          │
│     • Fecha fin: Diciembre 2026                                         │
│     • Click "Generar Paquete"                                           │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  4️⃣ SISTEMA GENERA AUTOMÁTICAMENTE                                      │
│     • Libro Diario (12 meses)                                           │
│     • Libro Mayor (12 meses)                                            │
│     • Libro de Compras (12 meses)                                       │
│     • Libro de Ventas (12 meses)                                        │
│     • Libro de Retenciones (12 meses)                                   │
│     • Libro de Banco (12 meses)                                         │
│     • Balances de Comprobación (12 meses)                               │
│     • Estado de Resultados anual                                        │
│     • Balance General al cierre                                         │
│     • Índice de documentos                                              │
│     ⏱️ Tiempo: < 5 minutos                                              │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  5️⃣ DESCARGA ZIP                                                        │
│     • Archivo: libros-contables-okla-202601-202612.zip                  │
│     • Tamaño: ~5-20 MB                                                  │
│     • Formato: Excel + PDF                                              │
│                                                                         │
│                        ▼                                                │
│                                                                         │
│  6️⃣ ENTREGA A DGII                                                      │
│     • Copiar a USB o enviar por email                                   │
│     • Imprimir carta de entrega                                         │
│     • Guardar copia del requerimiento                                   │
│     ⏱️ Tiempo total desde solicitud: < 1 hora                           │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Estructura del Paquete ZIP

```
libros-contables-okla-202601-202612.zip
│
├── INDICE-DOCUMENTOS.xlsx              ← Índice general
├── CARTA-ENTREGA.docx                   ← Template de carta
│
├── 01-LIBRO-DIARIO/
│   ├── libro-diario-2026-01.xlsx
│   ├── libro-diario-2026-02.xlsx
│   └── ... (12 archivos)
│
├── 02-LIBRO-MAYOR/
│   ├── libro-mayor-2026-01.xlsx
│   └── ... (12 archivos)
│
├── 03-LIBRO-COMPRAS/
│   ├── libro-compras-2026-01.xlsx
│   └── ... (12 archivos)
│
├── 04-LIBRO-VENTAS/
│   ├── libro-ventas-2026-01.xlsx
│   └── ... (12 archivos)
│
├── 05-BALANCES-COMPROBACION/
│   ├── balance-comprobacion-2026-01.xlsx
│   └── ... (12 archivos)
│
├── 06-ESTADOS-FINANCIEROS/
│   ├── estado-resultados-2026.xlsx
│   └── balance-general-2026-12.xlsx
│
├── 07-LIBRO-RETENCIONES/
│   ├── libro-retenciones-2026-01.xlsx
│   └── ... (12 archivos)
│
└── 08-LIBRO-BANCO/
    ├── libro-banco-2026-01.xlsx
    └── ... (12 archivos)
```

---

## 8. CHECKLIST DE IMPLEMENTACIÓN

### Base de Datos

- [ ] Crear tabla chart_of_accounts
- [ ] Crear tabla journal_entries
- [ ] Crear tabla journal_entry_lines
- [ ] Crear vista materializada general_ledger
- [ ] Crear tabla purchase_ledger
- [ ] Crear tabla sales_ledger
- [ ] Crear tabla withholding_ledger
- [ ] Crear tabla bank_ledger
- [ ] Crear tabla accounting_periods
- [ ] Insertar plan de cuentas inicial
- [ ] Crear índices

### Backend - AccountingService

- [ ] Crear microservicio con Clean Architecture
- [ ] Implementar JournalEntryService
- [ ] Implementar event handlers (PaymentCompleted, ExpenseRegistered)
- [ ] Implementar generadores de reportes Excel
- [ ] Implementar generadores de reportes PDF
- [ ] Implementar AuditPackageService
- [ ] Configurar controllers y endpoints
- [ ] Health checks

### Backend - Integración e-CF (Envío Automático)

- [ ] Integrar con ECFService para recibir e-CF emitidos
- [ ] Implementar AutomaticReportSubmissionService
- [ ] Implementar DGIIReportWebService (606, 609, IT-1, IR-17)
- [ ] Configurar job mensual de envío automático (día 10)
- [ ] Implementar firma digital para Web Services DGII
- [ ] Configurar reintentos automáticos en caso de fallo
- [ ] Implementar notificaciones de resultado

### Frontend

- [ ] Dashboard de contabilidad
- [ ] Selector de período
- [ ] Botones de descarga por libro
- [ ] Modal de generación de paquete auditoría
- [ ] Visualizador de asientos
- [ ] Plan de cuentas (CRUD)
- [ ] Panel de estado de envío a DGII
- [ ] Historial de reportes enviados

### Integración

- [ ] Event handlers conectados a BillingService
- [ ] Event handlers conectados a ExpenseService
- [ ] Event handlers conectados a PayrollService
- [ ] Event handlers conectados a ECFService
- [ ] Generación automática de asientos
- [ ] Sincronización con e-CF para Formato 607

### Envío Automático a DGII

- [ ] Web Service Formato 606 (Compras)
- [ ] Web Service Formato 609 (Compras Exterior)
- [ ] Web Service IT-1 (ITBIS Mensual)
- [ ] Web Service IR-17 (Retenciones)
- [ ] Certificado digital configurado
- [ ] Ambiente de pruebas validado
- [ ] Ambiente de producción activado

### Pruebas

- [ ] Tests de asientos contables
- [ ] Tests de generación de reportes
- [ ] Tests de cuadre (débitos = créditos)
- [ ] Tests de paquete auditoría
- [ ] Tests de envío a DGII (mock)
- [ ] Pruebas end-to-end en ambiente DGII test

---

## 📋 RESUMEN

### Libros Contables Automatizados

| Libro                | Fuente de Datos     | Actualización | Formato Output | Envío DGII     |
| -------------------- | ------------------- | ------------- | -------------- | -------------- |
| Diario               | Todos los eventos   | Tiempo real   | Excel/PDF      | -              |
| Mayor                | Vista materializada | Automática    | Excel          | -              |
| Compras              | ExpenseService      | Tiempo real   | Excel          | ✅ F606 Auto   |
| Ventas               | BillingService+e-CF | Tiempo real   | Excel          | ✅ F607 (e-CF) |
| Retenciones          | Expense + Sales     | Tiempo real   | Excel          | ✅ IR-17 Auto  |
| Banco                | Bank transactions   | Diaria        | Excel          | -              |
| Balance Comprobación | Mayor               | On-demand     | Excel          | -              |
| Estado Resultados    | Cuentas 4 y 5       | On-demand     | Excel/PDF      | -              |
| Balance General      | Cuentas 1,2,3       | On-demand     | Excel/PDF      | -              |

### Reportes con Envío Automático a DGII

| Reporte      | Frecuencia | Envío           | Intervención Humana |
| ------------ | ---------- | --------------- | ------------------- |
| e-CF         | Por venta  | Tiempo real     | 0 segundos          |
| Formato 607  | Mensual    | Auto desde e-CF | 5 min verificación  |
| Formato 606  | Mensual    | Web Service     | 10 min revisión     |
| Formato 609  | Mensual    | Web Service     | 0 min               |
| IT-1 (ITBIS) | Mensual    | Web Service     | 5 min verificación  |
| IR-17 (Ret.) | Mensual    | Web Service     | 5 min verificación  |

### Tiempos de Respuesta

| Solicitud                | Tiempo        |
| ------------------------ | ------------- |
| Libro individual (1 mes) | < 10 segundos |
| Paquete completo (1 año) | < 5 minutos   |
| Respuesta a auditoría    | < 1 hora      |
| Envío mensual a DGII     | Automático    |

### Comparación: Antes vs Después (e-CF)

| Tarea             | Antes (Manual) | Ahora (e-CF)  | Ahorro  |
| ----------------- | -------------- | ------------- | ------- |
| Emisión factura   | 5 min/factura  | 0 seg (auto)  | 100%    |
| Formato 607       | 2-4 horas/mes  | 5 min verif.  | 95%     |
| Formato 606       | 2-4 horas/mes  | 10 min verif. | 90%     |
| IT-1 ITBIS        | 1-2 horas/mes  | 5 min verif.  | 90%     |
| IR-17 Retenciones | 1-2 horas/mes  | 5 min verif.  | 90%     |
| **TOTAL MENSUAL** | **10-15 hrs**  | **~30 min**   | **95%** |

---

**Documento creado:** Enero 25, 2026  
**Actualización:** Enero 25, 2026 (Integración e-CF)  
**Próxima revisión:** Antes de implementación  
**Responsable:** Equipo de Desarrollo + Contador
