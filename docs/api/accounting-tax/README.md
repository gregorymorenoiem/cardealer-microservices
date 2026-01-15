# 🧾 APIs de Contabilidad, Facturación e Impuestos (República Dominicana)

**Categoría:** Accounting, Tax & Compliance  
**Fase:** 3 (Operaciones Avanzadas)  
**Prioridad:** 🔴 CRÍTICA (Obligatorio por ley)  
**País:** República Dominicana 🇩🇴  
**Última Actualización:** Enero 15, 2026

---

## 📋 RESUMEN EJECUTIVO

Esta documentación cubre todas las APIs necesarias para automatizar los procesos financieros, contables y tributarios de OKLA en República Dominicana. Incluye integración con la **DGII** (Dirección General de Impuestos Internos), **TSS** (Tesorería de la Seguridad Social), bancos locales, y proveedores de facturación electrónica.

### 🎯 Objetivos de Integración

1. **Facturación Electrónica (e-CF):** Emisión automática de comprobantes fiscales electrónicos
2. **Validación de NCF:** Verificar comprobantes recibidos de proveedores
3. **Consulta RNC/Cédula:** Validar contribuyentes antes de transacciones
4. **Declaraciones Fiscales:** Automatizar envío de IT-1, IR-17, etc.
5. **Conciliación Bancaria:** Conectar con bancos para reconciliar pagos
6. **Nómina y TSS:** Integrar con seguridad social para empleados

---

## 🏛️ ENTIDADES GUBERNAMENTALES

### 1. DGII - Dirección General de Impuestos Internos

| Servicio                       | Endpoint Base                                    | Autenticación       | Estado        |
| ------------------------------ | ------------------------------------------------ | ------------------- | ------------- |
| **e-CF (Factura Electrónica)** | `https://ecf.dgii.gov.do/`                       | Certificado Digital | ✅ Producción |
| **Consulta NCF**               | `https://dgii.gov.do/app/WebApps/ConsultasWeb2/` | API Key             | ✅ Producción |
| **Consulta RNC**               | `https://dgii.gov.do/app/WebApps/ConsultasWeb/`  | Público             | ✅ Producción |
| **Oficina Virtual**            | `https://ofv.dgii.gov.do/`                       | Usuario/Contraseña  | ✅ Producción |

### 2. TSS - Tesorería de la Seguridad Social

| Servicio                                          | Endpoint Base                   | Autenticación | Estado        |
| ------------------------------------------------- | ------------------------------- | ------------- | ------------- |
| **SUIR (Sistema Único de Información y Recaudo)** | `https://suir.gob.do/`          | Certificado   | ✅ Producción |
| **Consulta Empleadores**                          | `https://tss.gob.do/consultas/` | Público       | ✅ Producción |

### 3. SISALRIL - Superintendencia de Salud y Riesgos Laborales

| Servicio               | Endpoint Base                        | Autenticación | Estado        |
| ---------------------- | ------------------------------------ | ------------- | ------------- |
| **Afiliaciones ARS**   | `https://sisalril.gov.do/`           | Certificado   | ✅ Producción |
| **Consulta Afiliados** | `https://sisalril.gov.do/consultas/` | Público       | ✅ Producción |

---

## 🧾 PROVEEDORES DE FACTURACIÓN ELECTRÓNICA (e-CF)

Proveedores autorizados por DGII para emisión de comprobantes fiscales electrónicos:

| Proveedor          | Sitio Web             | Costo Mensual   | API REST    | Soporte         |
| ------------------ | --------------------- | --------------- | ----------- | --------------- |
| **Facturedo**      | facturedo.com         | RD$2,500-15,000 | ✅ Sí       | 24/7            |
| **TribuFácil**     | tribufacil.com        | RD$1,500-8,000  | ✅ Sí       | Horario oficina |
| **ComprobantesRD** | comprobantesrd.com    | RD$2,000-10,000 | ✅ Sí       | 24/7            |
| **FacturaDigital** | facturadigital.com.do | RD$1,800-9,000  | ✅ Sí       | Email           |
| **E-Factura**      | e-factura.com.do      | RD$3,000-12,000 | ✅ Sí       | 24/7            |
| **DGII Directo**   | dgii.gov.do           | GRATIS          | ⚠️ Limitado | DGII            |

### Recomendación para OKLA

**Opción Principal:** Facturedo  
**Razón:** API REST completa, documentación clara, soporte 24/7, pricing competitivo

**Opción Backup:** Integración directa con DGII  
**Razón:** Sin costo mensual, pero requiere más desarrollo

---

## 🏦 APIS BANCARIAS (Open Banking RD)

### Bancos con APIs Disponibles

| Banco                | API Disponible     | Tipo | Uso Principal             |
| -------------------- | ------------------ | ---- | ------------------------- |
| **Banco Popular**    | ✅ API Corporativa | REST | Consultas, Transferencias |
| **Banreservas**      | ✅ API Empresarial | REST | Consultas, Pagos          |
| **BHD León**         | ✅ API Business    | REST | Conciliación              |
| **Scotiabank**       | ⚠️ Limitada        | SOAP | Solo consultas            |
| **Banco Caribe**     | ⚠️ En desarrollo   | -    | 2026                      |
| **Banco Santa Cruz** | ⚠️ Limitada        | SOAP | Solo consultas            |

### Procesadores de Pago

| Procesador               | API     | Comisión     | Uso en OKLA    |
| ------------------------ | ------- | ------------ | -------------- |
| **AZUL (Banco Popular)** | ✅ REST | 2.5-3.5%     | Pagos tarjeta  |
| **CardNet**              | ✅ REST | 2.5-3.5%     | Pagos tarjeta  |
| **ACH Dominicana**       | ✅ API  | RD$15/transf | Transferencias |

---

## 📊 SOFTWARE CONTABLE (Integraciones)

| Software             | API     | Costo              | Popularidad RD    |
| -------------------- | ------- | ------------------ | ----------------- |
| **Alegra**           | ✅ REST | $25-99/mes         | ⭐⭐⭐⭐⭐        |
| **Siigo**            | ✅ REST | $30-150/mes        | ⭐⭐⭐⭐          |
| **Contabilidad.do**  | ✅ REST | RD$1,500-5,000/mes | ⭐⭐⭐⭐          |
| **QuickBooks**       | ✅ REST | $15-100/mes        | ⭐⭐⭐            |
| **Xero**             | ✅ REST | $12-70/mes         | ⭐⭐⭐            |
| **SAP Business One** | ✅ REST | $$$$               | ⭐⭐ (Enterprise) |

### Recomendación para OKLA

**Opción MVP:** Alegra (más popular en RD, API completa, pricing accesible)  
**Opción Enterprise:** SAP Business One (cuando escale)

---

## 📑 ÍNDICE DE DOCUMENTACIÓN

Esta carpeta contiene la documentación detallada de cada API:

| #   | Documento                                                                      | Contenido                       | Líneas |
| --- | ------------------------------------------------------------------------------ | ------------------------------- | ------ |
| 1   | [DGII_ECF_API.md](./DGII_ECF_API.md)                                           | Facturación electrónica e-CF    | 800+   |
| 2   | [DGII_NCF_RNC_API.md](./DGII_NCF_RNC_API.md)                                   | Validación NCF y consulta RNC   | 500+   |
| 3   | [DGII_DECLARACIONES_API.md](./DGII_DECLARACIONES_API.md)                       | IT-1, IR-17, IR-3 y otras       | 600+   |
| 4   | [FACTURACION_ELECTRONICA_PROVIDERS.md](./FACTURACION_ELECTRONICA_PROVIDERS.md) | Facturedo, TribuFácil, etc.     | 700+   |
| 5   | [BANKING_APIS.md](./BANKING_APIS.md)                                           | Banco Popular, Banreservas, ACH | 600+   |
| 6   | [TSS_SUIR_API.md](./TSS_SUIR_API.md)                                           | Seguridad social, nómina        | 500+   |
| 7   | [ACCOUNTING_SOFTWARE_INTEGRATIONS.md](./ACCOUNTING_SOFTWARE_INTEGRATIONS.md)   | Alegra, QuickBooks, Xero        | 600+   |

---

## 🔄 FLUJO DE INTEGRACIÓN COMPLETO

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      FLUJO CONTABLE OKLA                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  1️⃣ VENTA DE VEHÍCULO                                                        │
│  ├─> Cliente paga (AZUL/CardNet/Transferencia)                              │
│  ├─> Sistema genera e-CF automáticamente                                    │
│  ├─> e-CF se envía a DGII para validación                                   │
│  ├─> DGII retorna código de autorización                                    │
│  └─> Cliente recibe factura electrónica por email                           │
│                                                                              │
│  2️⃣ REGISTRO CONTABLE                                                        │
│  ├─> Transacción se registra en Alegra (software contable)                  │
│  ├─> Cuentas por cobrar/pagar actualizadas                                  │
│  ├─> Inventario de vehículos actualizado                                    │
│  └─> Reportes financieros en tiempo real                                    │
│                                                                              │
│  3️⃣ CONCILIACIÓN BANCARIA                                                    │
│  ├─> API bancaria obtiene movimientos diarios                               │
│  ├─> Sistema concilia automáticamente                                       │
│  ├─> Discrepancias alertadas a contabilidad                                 │
│  └─> Balance verificado diariamente                                         │
│                                                                              │
│  4️⃣ DECLARACIONES FISCALES (Mensual)                                         │
│  ├─> Sistema genera IT-1 automáticamente                                    │
│  ├─> Revisa por contador antes de envío                                     │
│  ├─> Envío a DGII vía API                                                   │
│  └─> Confirmación y archivo de acuse                                        │
│                                                                              │
│  5️⃣ NÓMINA Y TSS (Quincenal/Mensual)                                         │
│  ├─> Cálculo automático de deducciones                                      │
│  ├─> Generación de planilla TSS                                             │
│  ├─> Envío a SUIR vía API                                                   │
│  └─> Pago de aportes a TSS                                                  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🛡️ REQUISITOS LEGALES EN RD

### Obligaciones de OKLA como Empresa

| Requisito                  | Frecuencia | Formulario | Fecha Límite                |
| -------------------------- | ---------- | ---------- | --------------------------- |
| **ITBIS (Ventas)**         | Mensual    | IT-1       | Día 20 del mes siguiente    |
| **Retenciones**            | Mensual    | IR-17      | Día 10 del mes siguiente    |
| **ISR Personas Jurídicas** | Anual      | IR-2       | 120 días post cierre fiscal |
| **Activos**                | Anual      | IR-1       | Junto con IR-2              |
| **TSS Empleados**          | Mensual    | SUIR       | Día 3 del mes siguiente     |
| **Comprobantes Fiscales**  | Continuo   | e-CF       | En tiempo real              |

### Comprobantes Fiscales Electrónicos (e-CF)

OKLA debe emitir estos tipos de e-CF:

| Tipo                          | Código | Uso en OKLA                         |
| ----------------------------- | ------ | ----------------------------------- |
| **Factura de Crédito Fiscal** | 31     | Ventas a empresas (B2B)             |
| **Factura de Consumo**        | 32     | Ventas a consumidores finales (B2C) |
| **Nota de Débito**            | 33     | Cargos adicionales                  |
| **Nota de Crédito**           | 34     | Devoluciones, descuentos            |
| **Compras**                   | 41     | Registro de compras                 |
| **Gastos Menores**            | 43     | Gastos sin comprobante              |
| **Regímenes Especiales**      | 44     | Zona franca, etc.                   |
| **Gubernamental**             | 45     | Ventas al gobierno                  |

---

## 💻 ARQUITECTURA TÉCNICA

### Microservicio: AccountingTaxService

```
AccountingTaxService/
├── AccountingTaxService.Api/
│   ├── Controllers/
│   │   ├── EcfController.cs           # Facturación electrónica
│   │   ├── NcfController.cs           # Validación NCF
│   │   ├── RncController.cs           # Consulta RNC
│   │   ├── DeclarationsController.cs  # Declaraciones fiscales
│   │   ├── BankingController.cs       # Conciliación bancaria
│   │   └── TssController.cs           # Seguridad social
│   └── Program.cs
├── AccountingTaxService.Application/
│   ├── Features/
│   │   ├── Ecf/Commands/              # Emitir, Anular e-CF
│   │   ├── Ecf/Queries/               # Consultar e-CF
│   │   ├── Ncf/Queries/               # Validar NCF
│   │   ├── Rnc/Queries/               # Consultar RNC
│   │   ├── Declarations/Commands/     # Enviar declaraciones
│   │   ├── Banking/Queries/           # Movimientos bancarios
│   │   └── Tss/Commands/              # Enviar planillas TSS
│   └── DTOs/
├── AccountingTaxService.Domain/
│   ├── Entities/
│   │   ├── ElectronicInvoice.cs       # e-CF
│   │   ├── TaxDeclaration.cs          # Declaraciones
│   │   ├── BankTransaction.cs         # Transacciones bancarias
│   │   └── TssPayroll.cs              # Planilla TSS
│   ├── Enums/
│   │   ├── EcfType.cs                 # Tipos de e-CF
│   │   ├── DeclarationType.cs         # IT-1, IR-17, etc.
│   │   └── TaxStatus.cs               # Estados
│   └── Interfaces/
│       ├── IDgiiService.cs            # Interfaz DGII
│       ├── IEcfService.cs             # Interfaz e-CF
│       ├── IBankingService.cs         # Interfaz bancaria
│       └── ITssService.cs             # Interfaz TSS
└── AccountingTaxService.Infrastructure/
    ├── Services/
    │   ├── DgiiEcfService.cs          # Implementación DGII
    │   ├── FacturedoService.cs        # Proveedor alternativo
    │   ├── BancoPopularService.cs     # API bancaria
    │   └── TssService.cs              # API TSS
    └── Persistence/
```

---

## 📡 ENDPOINTS PRINCIPALES (AccountingTaxService)

### Facturación Electrónica (e-CF)

| Método | Endpoint                            | Descripción           |
| ------ | ----------------------------------- | --------------------- |
| `POST` | `/api/ecf/emit`                     | Emitir nuevo e-CF     |
| `POST` | `/api/ecf/cancel/{ecfNumber}`       | Anular e-CF           |
| `GET`  | `/api/ecf/{ecfNumber}`              | Consultar e-CF        |
| `GET`  | `/api/ecf/by-date`                  | Listar e-CF por fecha |
| `POST` | `/api/ecf/send-to-dgii`             | Enviar lote a DGII    |
| `GET`  | `/api/ecf/dgii-status/{trackingId}` | Estado en DGII        |

### Validación NCF/RNC

| Método | Endpoint                  | Descripción           |
| ------ | ------------------------- | --------------------- |
| `GET`  | `/api/ncf/validate/{ncf}` | Validar NCF           |
| `GET`  | `/api/rnc/{rnc}`          | Consultar RNC         |
| `GET`  | `/api/rnc/by-name/{name}` | Buscar RNC por nombre |
| `GET`  | `/api/cedula/{cedula}`    | Consultar cédula      |

### Declaraciones Fiscales

| Método | Endpoint                    | Descripción                 |
| ------ | --------------------------- | --------------------------- |
| `POST` | `/api/declarations/it1`     | Generar/Enviar IT-1         |
| `POST` | `/api/declarations/ir17`    | Generar/Enviar IR-17        |
| `POST` | `/api/declarations/ir2`     | Generar/Enviar IR-2 (anual) |
| `GET`  | `/api/declarations/{id}`    | Consultar declaración       |
| `GET`  | `/api/declarations/pending` | Declaraciones pendientes    |

### Conciliación Bancaria

| Método | Endpoint                           | Descripción           |
| ------ | ---------------------------------- | --------------------- |
| `GET`  | `/api/banking/accounts`            | Listar cuentas        |
| `GET`  | `/api/banking/transactions`        | Obtener movimientos   |
| `POST` | `/api/banking/reconcile`           | Ejecutar conciliación |
| `GET`  | `/api/banking/balance/{accountId}` | Obtener balance       |

### TSS (Seguridad Social)

| Método | Endpoint                         | Descripción          |
| ------ | -------------------------------- | -------------------- |
| `POST` | `/api/tss/payroll`               | Generar planilla     |
| `POST` | `/api/tss/submit`                | Enviar a SUIR        |
| `GET`  | `/api/tss/employees`             | Listar empleados TSS |
| `GET`  | `/api/tss/contributions/{month}` | Aportes por mes      |

---

## 💰 COSTOS ESTIMADOS

### Costos Fijos Mensuales

| Concepto                     | Costo                   | Notas                     |
| ---------------------------- | ----------------------- | ------------------------- |
| **Proveedor e-CF**           | RD$3,000-5,000          | Facturedo o similar       |
| **Software Contable**        | RD$2,000-4,000          | Alegra o similar          |
| **Certificado Digital**      | RD$500/mes (amortizado) | Renovación anual RD$6,000 |
| **Desarrollo/Mantenimiento** | -                       | Interno                   |
| **TOTAL**                    | RD$5,500-9,500/mes      | ~$100-170 USD             |

### Costos Variables

| Concepto               | Costo               | Volumen Esperado |
| ---------------------- | ------------------- | ---------------- |
| **e-CF adicionales**   | RD$2-5/unidad       | 500-2,000/mes    |
| **Consultas RNC**      | GRATIS              | Ilimitado        |
| **Transferencias ACH** | RD$15/transf        | 50-200/mes       |
| **TOTAL VARIABLE**     | RD$1,750-13,000/mes | Variable         |

### ROI Estimado

| Beneficio                | Ahorro Mensual                   |
| ------------------------ | -------------------------------- |
| **Tiempo de contador**   | RD$20,000 (20h × RD$1,000/h)     |
| **Errores evitados**     | RD$10,000 (multas, correcciones) |
| **Eficiencia operativa** | RD$15,000                        |
| **TOTAL AHORRO**         | RD$45,000/mes                    |
| **COSTO**                | RD$10,000-22,500/mes             |
| **ROI**                  | **3-4x**                         |

---

## 🚀 PLAN DE IMPLEMENTACIÓN

### Fase 1: Fundamentos (Semana 1-2)

- [ ] Obtener certificado digital de DGII
- [ ] Registrar empresa en portal e-CF
- [ ] Configurar ambiente de pruebas DGII
- [ ] Implementar consulta RNC/NCF

### Fase 2: Facturación (Semana 3-4)

- [ ] Integrar con proveedor e-CF (Facturedo)
- [ ] Implementar emisión de e-CF
- [ ] Implementar anulación de e-CF
- [ ] Pruebas end-to-end

### Fase 3: Declaraciones (Semana 5-6)

- [ ] Implementar generación IT-1
- [ ] Implementar generación IR-17
- [ ] Implementar envío a DGII
- [ ] Automatizar recordatorios

### Fase 4: Bancario (Semana 7-8)

- [ ] Integrar API Banco Popular
- [ ] Implementar conciliación automática
- [ ] Dashboard de tesorería

### Fase 5: TSS (Semana 9-10)

- [ ] Integrar con SUIR
- [ ] Automatizar planilla mensual
- [ ] Cálculo de deducciones

---

## 📚 DOCUMENTOS DETALLADOS

Los siguientes documentos contienen la implementación completa de cada API:

1. **[DGII_ECF_API.md](./DGII_ECF_API.md)** - Facturación electrónica completa
2. **[DGII_NCF_RNC_API.md](./DGII_NCF_RNC_API.md)** - Validaciones fiscales
3. **[DGII_DECLARACIONES_API.md](./DGII_DECLARACIONES_API.md)** - IT-1, IR-17, IR-2
4. **[FACTURACION_ELECTRONICA_PROVIDERS.md](./FACTURACION_ELECTRONICA_PROVIDERS.md)** - Proveedores autorizados
5. **[BANKING_APIS.md](./BANKING_APIS.md)** - APIs bancarias RD
6. **[TSS_SUIR_API.md](./TSS_SUIR_API.md)** - Seguridad social
7. **[ACCOUNTING_SOFTWARE_INTEGRATIONS.md](./ACCOUNTING_SOFTWARE_INTEGRATIONS.md)** - Alegra, QuickBooks

---

## 📞 CONTACTOS IMPORTANTES

| Entidad           | Teléfono       | Email                 | Web                |
| ----------------- | -------------- | --------------------- | ------------------ |
| **DGII**          | (809) 689-3444 | info@dgii.gov.do      | dgii.gov.do        |
| **TSS**           | (809) 472-0026 | info@tss.gob.do       | tss.gob.do         |
| **SISALRIL**      | (809) 227-4050 | info@sisalril.gov.do  | sisalril.gov.do    |
| **Facturedo**     | (809) 555-0000 | soporte@facturedo.com | facturedo.com      |
| **Banco Popular** | (809) 544-5555 | empresas@bpd.com.do   | popularenlinea.com |

---

**Estado:** 📋 ÍNDICE COMPLETO  
**Próximo:** Crear documentos detallados de cada API  
**Última Actualización:** Enero 15, 2026
