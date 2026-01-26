# 🏛️ DGII - Integración Fiscal - Matriz de Procesos

> **Entidad:** Dirección General de Impuestos Internos  
> **Última actualización:** Enero 25, 2026  
> **Estado:** 🟢 ACTIVO (Obligatorio)  
> **Estado de Implementación:** 🟡 50% Backend | 🔴 0% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                  | Backend       | UI Access         | Observación            |
| ------------------------ | ------------- | ----------------- | ---------------------- |
| DGII-RNC-001 Validar RNC | ✅ KYCService | ✅ DealerRegister | Validación en registro |
| DGII-NCF-001 Generar NCF | 🟡 Parcial    | 🔴 Falta          | Sin UI de NCF          |
| DGII-607-001 Formato 607 | 🔴 Pendiente  | 🔴 Falta          | Sin generador          |
| DGII-EFACT-001 E-Factura | 🔴 Pendiente  | 🔴 Falta          | Pendiente              |

### Rutas UI Existentes ✅

- `/dealer/register` → Validación de RNC

### Rutas UI Faltantes 🔴

- `/admin/fiscal/ncf` → Gestión de secuencias NCF
- `/admin/fiscal/607` → Generación de formato 607
- `/admin/fiscal/606` → Generación de formato 606
- `/invoices` → Facturas electrónicas

**Verificación Backend:** Validación RNC funcional, NCF parcial 🟡

---

## 📊 Resumen de Implementación

| Componente                       | Total | Implementado | Pendiente | Estado         |
| -------------------------------- | ----- | ------------ | --------- | -------------- |
| **DGII-RNC-\*** (Validación RNC) | 3     | 3            | 0         | ✅ Completo    |
| **DGII-NCF-\*** (Comprobantes)   | 4     | 2            | 2         | 🟡 Parcial     |
| **DGII-606-\*** (Compras)        | 3     | 0            | 3         | 🔴 Pendiente   |
| **DGII-607-\*** (Ventas)         | 3     | 0            | 3         | 🔴 Pendiente   |
| **DGII-EFACT-\*** (E-Factura)    | 4     | 0            | 4         | 🔴 Pendiente   |
| **Tests**                        | 15    | 5            | 10        | 🟡 Parcial     |
| **TOTAL**                        | 32    | 10           | 22        | 🟡 50% Backend |

---

## 1. Información General

### 1.1 Modelo de Negocio OKLA

> **OKLA es una plataforma de anuncios clasificados** (similar a SuperCarros.com).
>
> **Servicios facturables (con NCF/ITBIS):**
>
> - Publicación de anuncios: $29/anuncio
> - Suscripciones dealers: $49-$299/mes
> - Boosts de publicaciones
>
> **OKLA NO participa** en transacciones de vehículos. Los dealers y compradores las realizan directamente.

### 1.2 Descripción

Integración con los sistemas de la DGII para cumplimiento fiscal en República Dominicana. Incluye validación de RNC/Cédula, generación de NCF, reportes 606/607/608, y facturación electrónica para los servicios de publicidad que OKLA ofrece.

### 1.2 Marco Regulatorio

| Ley/Norma     | Descripción           |
| ------------- | --------------------- |
| Ley 11-92     | Código Tributario     |
| Ley 253-12    | Comprobantes Fiscales |
| Norma 06-2018 | Factura Electrónica   |
| Norma 08-2019 | Secuencias de NCF     |

### 1.3 Obligaciones Fiscales de OKLA

| Obligación                  | Frecuencia | Plazo                    |
| --------------------------- | ---------- | ------------------------ |
| Declaración de ITBIS (IT-1) | Mensual    | Día 20 del mes siguiente |
| Reporte 606 (Compras)       | Mensual    | Día 15 del mes siguiente |
| Reporte 607 (Ventas)        | Mensual    | Día 15 del mes siguiente |
| Reporte 608 (Anulaciones)   | Mensual    | Día 15 del mes siguiente |
| Declaración ISR             | Anual      | Abril 30                 |

---

## 2. Endpoints API

### 2.1 DGIIController

| Método | Endpoint                             | Descripción         | Auth | Roles |
| ------ | ------------------------------------ | ------------------- | ---- | ----- |
| `GET`  | `/api/dgii/validate-rnc/{rnc}`       | Validar RNC         | ✅   | User  |
| `GET`  | `/api/dgii/validate-cedula/{cedula}` | Validar cédula      | ✅   | User  |
| `GET`  | `/api/dgii/ncf/validate/{ncf}`       | Validar NCF         | ✅   | Admin |
| `GET`  | `/api/dgii/ncf/sequences`            | Ver secuencias NCF  | ✅   | Admin |
| `POST` | `/api/dgii/ncf/request`              | Solicitar secuencia | ✅   | Admin |

### 2.2 ReportsController

| Método | Endpoint                          | Descripción           | Auth | Roles |
| ------ | --------------------------------- | --------------------- | ---- | ----- |
| `POST` | `/api/dgii/reports/606`           | Generar reporte 606   | ✅   | Admin |
| `POST` | `/api/dgii/reports/607`           | Generar reporte 607   | ✅   | Admin |
| `POST` | `/api/dgii/reports/608`           | Generar reporte 608   | ✅   | Admin |
| `GET`  | `/api/dgii/reports/history`       | Historial de reportes | ✅   | Admin |
| `GET`  | `/api/dgii/reports/{id}/download` | Descargar reporte     | ✅   | Admin |

### 2.3 EFacturaController

| Método | Endpoint                         | Descripción           | Auth | Roles  |
| ------ | -------------------------------- | --------------------- | ---- | ------ |
| `POST` | `/api/dgii/efactura/send`        | Enviar e-factura      | ✅   | System |
| `GET`  | `/api/dgii/efactura/{id}/status` | Estado de e-factura   | ✅   | Admin  |
| `GET`  | `/api/dgii/efactura/pending`     | E-facturas pendientes | ✅   | Admin  |

---

## 3. Validación de RNC

### 3.1 Estructura del RNC

```
RNC Persona Jurídica: 9 dígitos (1-01234567)
RNC Persona Natural: 11 dígitos (001-1234567-8)

Formato:
┌─────────────────────────────────────────┐
│         PERSONA JURÍDICA                │
│         1-01234567                      │
│         │ ││││││││                      │
│         │ └──────── Secuencia (7)       │
│         └────────── Tipo (1=PJ)         │
├─────────────────────────────────────────┤
│         PERSONA NATURAL (Cédula)        │
│         001-1234567-8                   │
│         │││ │││││││ │                   │
│         │││ │││││││ └ Verificador       │
│         │││ └────── Secuencia (7)       │
│         └── Municipio (3)               │
└─────────────────────────────────────────┘
```

### 3.2 Proceso de Validación

| Paso | Acción                     | Sistema     | Validación     |
| ---- | -------------------------- | ----------- | -------------- |
| 1    | Recibir RNC/Cédula         | API         | Input          |
| 2    | Limpiar caracteres         | DGIIService | Solo números   |
| 3    | Validar longitud           | DGIIService | 9 u 11         |
| 4    | Validar dígito verificador | DGIIService | Algoritmo Luhn |
| 5    | Buscar en caché            | Redis       | TTL 24h        |
| 6    | Si no en caché             | DGIIService | Consultar API  |
| 7    | Llamar API DGII            | HTTP        | dgii.gov.do    |
| 8    | Parsear respuesta          | DGIIService | JSON/XML       |
| 9    | Guardar en caché           | Redis       | 24h            |
| 10   | Retornar resultado         | Response    | RNCInfo        |

### 3.3 Response de Validación

```json
{
  "rnc": "131-12345-6",
  "valid": true,
  "name": "OKLA TECHNOLOGIES SRL",
  "commercialName": "OKLA",
  "status": "ACTIVO",
  "paymentRegime": "NORMAL",
  "activity": "Servicios de tecnología",
  "address": "Av. 27 de Febrero #123, Santo Domingo",
  "incorporationDate": "2024-01-15",
  "lastUpdated": "2026-01-21"
}
```

---

## 4. Comprobantes Fiscales (NCF)

### 4.1 Tipos de NCF

| Código | Tipo                        | Uso                       |
| ------ | --------------------------- | ------------------------- |
| B01    | Factura de Consumidor Final | Ventas a consumidores     |
| B02    | Factura de Crédito Fiscal   | Ventas a contribuyentes   |
| B03    | Nota de Débito              | Aumentar valor de factura |
| B04    | Nota de Crédito             | Disminuir valor / Anular  |
| B11    | Comprobante de Compras      | Compras a proveedores     |
| B13    | Gastos del Exterior         | Pagos internacionales     |
| B14    | Comprobante Gubernamental   | Ventas al gobierno        |
| B15    | Régimen Especial            | Zonas francas             |
| B16    | Exportación                 | Ventas al exterior        |
| B17    | Único de Ingresos           | Pequeño contribuyente     |

### 4.2 Gestión de Secuencias

```csharp
public class NCFSequence
{
    public Guid Id { get; set; }
    public string Type { get; set; }           // B01, B02, etc.
    public string Prefix { get; set; }         // B01
    public long StartNumber { get; set; }      // 00000001
    public long EndNumber { get; set; }        // 00001000
    public long CurrentNumber { get; set; }    // 00000150
    public DateTime AuthorizationDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsActive { get; set; }
    public int RemainingCount => (int)(EndNumber - CurrentNumber);
}
```

### 4.3 Proceso de Generación de NCF

| Paso | Acción                   | Sistema          | Validación           |
| ---- | ------------------------ | ---------------- | -------------------- |
| 1    | Solicitar NCF            | InvoicingService | Para factura X       |
| 2    | Determinar tipo          | DGIIService      | B01 o B02            |
| 3    | Obtener secuencia activa | Database         | IsActive = true      |
| 4    | Verificar no expirada    | DGIIService      | ExpirationDate > hoy |
| 5    | Verificar disponibilidad | DGIIService      | Remaining > 0        |
| 6    | Bloquear registro        | Database         | Para concurrencia    |
| 7    | Incrementar contador     | Database         | CurrentNumber++      |
| 8    | Formatear NCF            | DGIIService      | B0100000151          |
| 9    | Liberar bloqueo          | Database         | Commit               |
| 10   | Retornar NCF             | Response         | String               |

### 4.4 Alerta de Agotamiento

```csharp
public async Task CheckNCFAvailability()
{
    var sequences = await _repository.GetActiveSequences();

    foreach (var seq in sequences)
    {
        if (seq.RemainingCount < 100)
        {
            await _notificationService.SendAlert(new NCFAlert
            {
                Type = seq.Type,
                Remaining = seq.RemainingCount,
                ExpirationDate = seq.ExpirationDate,
                Urgency = seq.RemainingCount < 20 ? "HIGH" : "MEDIUM"
            });
        }
    }
}
```

---

## 5. Reportes DGII

### 5.1 Formato 606 - Compras

**Archivo:** `606MMAAAA.txt`

| Campo                 | Posición | Longitud | Descripción  |
| --------------------- | -------- | -------- | ------------ |
| RNC Informante        | 1-11     | 11       | RNC de OKLA  |
| Período               | 12-17    | 6        | AAAAMM       |
| Cantidad Registros    | 18-29    | 12       | Total líneas |
| Total Monto Facturado | 30-45    | 16       | Suma compras |
| Total ITBIS Facturado | 46-61    | 16       | Suma ITBIS   |

**Detalle:**

```
RNC Cedula|Tipo Bienes|NCF|NCF Modificado|Fecha|Monto|ITBIS|ITBIS Ret|ISR|Tipo Pago|Fecha Pago
```

### 5.2 Formato 607 - Ventas

**Archivo:** `607MMAAAA.txt`

| Campo                | Descripción                                      |
| -------------------- | ------------------------------------------------ |
| RNC/Cédula Cliente   | Identificación del cliente                       |
| Tipo ID              | 1=RNC, 2=Cédula, 3=Pasaporte                     |
| NCF                  | Número de comprobante                            |
| NCF Modificado       | Si es NC/ND                                      |
| Tipo Ingreso         | 1=Operativo, 2=Financiero                        |
| Fecha Comprobante    | DD/MM/AAAA                                       |
| Fecha Retención      | Si aplica                                        |
| Monto Facturado      | Subtotal                                         |
| ITBIS Facturado      | 18%                                              |
| ITBIS Retenido       | Si tercero                                       |
| Monto Retenido Renta | ISR si aplica                                    |
| Tipo Pago            | 1=Efectivo, 2=Cheque, 3=Transferencia, 4=Tarjeta |

### 5.3 Formato 608 - Anulaciones

**Archivo:** `608MMAAAA.txt`

```
NCF|Tipo Anulacion|Fecha Anulacion
B0100000001|01|15/01/2026
```

**Tipos de Anulación:**

- 01: Deterioro de factura pre-impresa
- 02: Errores de impresión
- 03: Impresión defectuosa
- 04: Duplicidad de factura
- 05: Corrección de información
- 06: Cambio de productos
- 07: Devolución de productos
- 08: Omisión de productos
- 09: Errores en secuencia NCF

---

## 6. Factura Electrónica (e-CF)

### 6.1 Estructura XML

```xml
<?xml version="1.0" encoding="UTF-8"?>
<ECF xmlns="http://dgii.gov.do/ecf/2019">
  <Encabezado>
    <IdDoc>
      <TipoeCF>31</TipoeCF>
      <eNCF>E310000000001</eNCF>
      <FechaVencimientoSecuencia>2027-01-01</FechaVencimientoSecuencia>
    </IdDoc>
    <Emisor>
      <RNCEmisor>131123456</RNCEmisor>
      <RazonSocialEmisor>OKLA Technologies SRL</RazonSocialEmisor>
    </Emisor>
    <Comprador>
      <RNCComprador>101234567</RNCComprador>
      <RazonSocialComprador>Cliente SRL</RazonSocialComprador>
    </Comprador>
    <Totales>
      <MontoTotal>11800.00</MontoTotal>
      <MontoNoGravado>0.00</MontoNoGravado>
      <MontoGravado>10000.00</MontoGravado>
      <ITBIS>1800.00</ITBIS>
    </Totales>
  </Encabezado>
  <DetallesItem>
    <Item>
      <NumeroLinea>1</NumeroLinea>
      <IndicadorFacturacion>1</IndicadorFacturacion>
      <NombreItem>Suscripción Plan Pro - Enero 2026</NombreItem>
      <CantidadItem>1</CantidadItem>
      <MontoItem>10000.00</MontoItem>
      <IndicadorTasaITBIS>3</IndicadorTasaITBIS>
      <MontoITBIS>1800.00</MontoITBIS>
    </Item>
  </DetallesItem>
  <FirmaDigital>
    <!-- Firma electrónica del emisor -->
  </FirmaDigital>
</ECF>
```

### 6.2 Flujo de e-Factura

| Paso | Acción              | Sistema             | Validación             |
| ---- | ------------------- | ------------------- | ---------------------- |
| 1    | Crear factura       | InvoicingService    | Con NCF electrónico    |
| 2    | Generar XML         | DGIIService         | Formato e-CF           |
| 3    | Firmar digitalmente | CryptoService       | Certificado X.509      |
| 4    | Validar XSD         | DGIIService         | Esquema DGII           |
| 5    | Enviar a DGII       | HTTP                | API e-CF               |
| 6    | Recibir TrackId     | Response            | Para seguimiento       |
| 7    | Consultar estado    | Polling             | Cada 5 min             |
| 8    | Recibir aprobación  | DGII                | Código de verificación |
| 9    | Actualizar factura  | Database            | Status = Approved      |
| 10   | Enviar al cliente   | NotificationService | Con código QR          |

---

## 7. ITBIS (Impuesto sobre Transferencias de Bienes)

### 7.1 Cálculo

```csharp
public class ITBISCalculator
{
    private const decimal STANDARD_RATE = 0.18m;    // 18%
    private const decimal REDUCED_RATE = 0.16m;     // 16% (algunos bienes)
    private const decimal EXEMPT_RATE = 0.00m;      // Exento

    public ITBISResult Calculate(decimal subtotal, ItemType type)
    {
        decimal rate = type switch
        {
            ItemType.Service => STANDARD_RATE,
            ItemType.DigitalService => STANDARD_RATE,
            ItemType.Export => EXEMPT_RATE,
            _ => STANDARD_RATE
        };

        decimal itbis = Math.Round(subtotal * rate, 2);

        return new ITBISResult
        {
            Subtotal = subtotal,
            Rate = rate,
            ITBISAmount = itbis,
            Total = subtotal + itbis
        };
    }
}
```

### 7.2 Retenciones

| Concepto            | % Retención | Condición               |
| ------------------- | ----------- | ----------------------- |
| ITBIS a terceros    | 30%         | Compras > $50,000/mes   |
| ISR a terceros      | 10%         | Servicios profesionales |
| ISR a no residentes | 27%         | Pagos al exterior       |

---

## 8. Configuración

```json
{
  "DGII": {
    "ApiUrl": "https://api.dgii.gov.do/v1",
    "eCFUrl": "https://ecf.dgii.gov.do/v1",
    "Credentials": {
      "RNC": "${DGII_RNC}",
      "Username": "${DGII_USER}",
      "Password": "${DGII_PASSWORD}"
    },
    "Certificate": {
      "Path": "/certs/dgii-cert.pfx",
      "Password": "${DGII_CERT_PASSWORD}"
    },
    "NCFAlertThreshold": 100,
    "CacheExpirationHours": 24,
    "RetryPolicy": {
      "MaxRetries": 3,
      "DelaySeconds": 5
    }
  }
}
```

---

## 9. Métricas

```
# Validaciones
dgii_rnc_validations_total{result="valid|invalid|error"}
dgii_ncf_generated_total{type="B01|B02|B04"}
dgii_ncf_remaining{type="B01|B02|B04"}

# Reportes
dgii_reports_generated_total{type="606|607|608"}
dgii_reports_submitted_total
dgii_reports_errors_total

# e-Factura
dgii_ecf_sent_total
dgii_ecf_approved_total
dgii_ecf_rejected_total{reason="..."}
dgii_ecf_processing_time_seconds
```

---

## 10. Eventos RabbitMQ

| Evento                  | Exchange      | Payload                     |
| ----------------------- | ------------- | --------------------------- |
| `dgii.ncf.generated`    | `dgii.events` | `{ ncf, type, invoiceId }`  |
| `dgii.ncf.low`          | `dgii.alerts` | `{ type, remaining }`       |
| `dgii.report.generated` | `dgii.events` | `{ type, period }`          |
| `dgii.ecf.submitted`    | `dgii.events` | `{ trackId }`               |
| `dgii.ecf.approved`     | `dgii.events` | `{ ncf, verificationCode }` |
| `dgii.ecf.rejected`     | `dgii.events` | `{ ncf, reason }`           |

---

## 📚 Referencias

- [DGII](https://dgii.gov.do) - Portal de la DGII
- [e-CF](https://ecf.dgii.gov.do) - Facturación electrónica
- [04-invoicing-service.md](../05-PAGOS-FACTURACION/04-invoicing-service.md) - Facturación
- [01-ley-155-17.md](01-ley-155-17.md) - Ley de lavado de activos
