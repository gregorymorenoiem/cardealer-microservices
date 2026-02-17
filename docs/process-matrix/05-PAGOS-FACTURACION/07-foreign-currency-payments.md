# 💱 Pagos en Moneda Extranjera - Cumplimiento DGII

## 📋 Información General

| Campo                  | Valor                                 |
| ---------------------- | ------------------------------------- |
| **Código de Proceso**  | FX-PAYMENTS                           |
| **Servicio Principal** | PaymentService                        |
| **Puerto**             | 15105                                 |
| **Prioridad**          | Alta                                  |
| **Estado Backend**     | ✅ 100%                               |
| **Estado Frontend**    | 🔄 50%                                |
| **Normativa**          | Norma General 06-2018 DGII, Ley 11-92 |

---

## 🎯 Objetivo

Procesar pagos en moneda extranjera (USD, EUR) cumpliendo con los requisitos fiscales de la República Dominicana, utilizando las tasas oficiales del Banco Central (BCRD) y generando los registros de auditoría requeridos por la DGII.

---

## 📜 Marco Legal

### Requisitos DGII

1. **Facturación en DOP**: Todas las facturas electrónicas (NCF) deben emitirse en Pesos Dominicanos
2. **Tasa Oficial BCRD**: Usar la tasa de cambio publicada por el Banco Central
3. **Registro de Conversión**: Documentar la tasa aplicada para cada transacción
4. **ITBIS 18%**: Calcular sobre el monto convertido a DOP
5. **Formato 607**: Reportar ventas en DOP en el reporte mensual

### Fuente de Tasas

- **Primaria**: API del Banco Central de la República Dominicana
- **URL**: https://api.bancentral.gov.do/
- **Registro**: Requiere solicitar API key en el portal del BCRD

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           FLUJO DE PAGO USD/EUR                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1️⃣ CLIENTE INICIA PAGO                                                    │
│  └─> POST /api/payments/charge                                             │
│      { "amount": 100, "currency": "USD", "gateway": "PixelPay" }           │
│                                                                             │
│  2️⃣ OBTENER TASA DE CAMBIO                                                 │
│  └─> ExchangeRateService.GetCurrentRateAsync("USD")                        │
│      ├─> Verificar caché Redis                                             │
│      ├─> Consultar base de datos                                           │
│      └─> Si no existe → Consultar API BCRD                                 │
│                                                                             │
│  3️⃣ CONVERTIR A DOP                                                        │
│  └─> ExchangeRateService.ConvertToDopAsync(100, "USD", transactionId)     │
│      ├─> $100 USD × 58.50 = 5,850 DOP                                      │
│      ├─> ITBIS 18% = 1,053 DOP                                             │
│      ├─> Total = 6,903 DOP                                                 │
│      └─> Guardar CurrencyConversion (auditoría)                            │
│                                                                             │
│  4️⃣ PROCESAR PAGO                                                          │
│  └─> PaymentGateway.ChargeAsync(request)                                   │
│      └─> Cobrar en moneda original (USD)                                   │
│                                                                             │
│  5️⃣ GENERAR NCF                                                            │
│  └─> NCF en DOP con referencia a la conversión                            │
│      └─> Actualizar CurrencyConversion.Ncf                                 │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Endpoints

### Tasas de Cambio

| Método | Endpoint                                | Auth     | Descripción             |
| ------ | --------------------------------------- | -------- | ----------------------- |
| `GET`  | `/api/exchangerates/current/{currency}` | ❌       | Tasa actual USD/EUR     |
| `GET`  | `/api/exchangerates/current`            | ❌       | Todas las tasas         |
| `GET`  | `/api/exchangerates/history/{currency}` | ❌       | Historial de tasas      |
| `POST` | `/api/exchangerates/convert`            | ❌       | Convertir monto         |
| `GET`  | `/api/exchangerates/quote`              | ❌       | Cotización sin registro |
| `POST` | `/api/exchangerates/refresh`            | ✅ Admin | Forzar actualización    |
| `GET`  | `/api/exchangerates/conversions/{txId}` | ✅       | Registro de conversión  |

### Request: Convertir a DOP

```json
POST /api/exchangerates/convert
{
  "amount": 100.00,
  "currency": "USD",
  "transactionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"  // opcional
}
```

### Response: Conversión

```json
{
  "originalAmount": 100.0,
  "originalCurrency": "USD",
  "convertedAmountDop": 5850.0,
  "appliedRate": 58.5,
  "rateDate": "2026-01-28",
  "rateSource": "BancoCentralApi",
  "itbisDop": 1053.0,
  "itbisRate": 0.18,
  "totalWithItbisDop": 6903.0,
  "conversionRecordId": "9a1b2c3d-4e5f-6789-abcd-ef0123456789"
}
```

---

## 🗄️ Modelo de Datos

### ExchangeRate (Tasas de Cambio)

```sql
CREATE TABLE "ExchangeRates" (
    "Id" UUID PRIMARY KEY,
    "RateDate" DATE NOT NULL,
    "SourceCurrency" VARCHAR(3) NOT NULL,  -- USD, EUR
    "TargetCurrency" VARCHAR(3) NOT NULL DEFAULT 'DOP',
    "BuyRate" DECIMAL(18,6) NOT NULL,      -- Tasa de compra
    "SellRate" DECIMAL(18,6) NOT NULL,     -- Tasa de venta
    "Source" INTEGER NOT NULL,             -- 1=BCRD_API, 2=WebScrape, etc.
    "BcrdReferenceId" VARCHAR(100),        -- ID del BCRD
    "FetchedAt" TIMESTAMP NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "Metadata" TEXT,                       -- JSON respuesta BCRD
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP NOT NULL
);

-- Solo una tasa activa por moneda/fecha
CREATE UNIQUE INDEX ON "ExchangeRates"
    ("SourceCurrency", "RateDate", "IsActive")
    WHERE "IsActive" = TRUE;
```

### CurrencyConversion (Auditoría DGII)

```sql
CREATE TABLE "CurrencyConversions" (
    "Id" UUID PRIMARY KEY,
    "PaymentTransactionId" UUID NOT NULL,  -- FK a transacción de pago
    "ExchangeRateId" UUID NOT NULL,        -- FK a ExchangeRates
    "OriginalCurrency" VARCHAR(3) NOT NULL,
    "OriginalAmount" DECIMAL(18,2) NOT NULL,
    "ConvertedAmountDop" DECIMAL(18,2) NOT NULL,
    "AppliedRate" DECIMAL(18,6) NOT NULL,  -- Copia para auditoría
    "RateDate" DATE NOT NULL,
    "RateSource" INTEGER NOT NULL,
    "ConversionType" INTEGER NOT NULL,     -- 1=Purchase, 2=Refund, 3=Quote
    "ItbisDop" DECIMAL(18,2) NOT NULL,     -- 18% sobre DOP
    "TotalWithItbisDop" DECIMAL(18,2) NOT NULL,
    "Ncf" VARCHAR(19),                     -- B0100000001
    "NcfIssuedAt" TIMESTAMP,
    "Notes" VARCHAR(500),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    FOREIGN KEY ("ExchangeRateId") REFERENCES "ExchangeRates"("Id")
);
```

---

## ⚙️ Configuración

### appsettings.json

```json
{
  "BancoCentral": {
    "ApiBaseUrl": "https://api.bancentral.gov.do",
    "ApiKey": "${BCRD_API_KEY}",
    "TimeoutSeconds": 30,
    "CacheHours": 24,
    "RefreshHour": 8,
    "RefreshMinute": 30,
    "SupportedCurrencies": ["USD", "EUR"],
    "EnableWebScrapingFallback": true,
    "EnableExternalProviderFallback": true
  }
}
```

### Variables de Entorno

```bash
BCRD_API_KEY=tu-api-key-del-banco-central
```

---

## 🔄 Background Job

El servicio `ExchangeRateRefreshJob` actualiza las tasas automáticamente:

- **Horario**: 8:30 AM hora RD (después de publicación del BCRD)
- **Frecuencia**: Diaria
- **Monedas**: USD, EUR
- **Fallbacks**: Web scraping BCRD → Proveedor externo → Tasa anterior

```csharp
// El job se ejecuta automáticamente al iniciar el servicio
// y luego diariamente a las 8:30 AM hora de República Dominicana
```

---

## 🧮 Cálculos

### Conversión USD → DOP

```
Monto Original:     $100.00 USD
Tasa de Compra:     58.50 DOP/USD
─────────────────────────────────
Subtotal DOP:       5,850.00 DOP
ITBIS (18%):        1,053.00 DOP
─────────────────────────────────
TOTAL:              6,903.00 DOP
```

### Reembolso DOP → USD

```
Monto a Reembolsar: 5,850.00 DOP
Tasa de Venta:      59.00 DOP/USD
─────────────────────────────────
Reembolso USD:      $99.15 USD
```

---

## 🔐 Seguridad

1. **API Key BCRD**: Almacenada en secrets, no en código
2. **Caché Redis**: TTL 24 horas para evitar consultas excesivas
3. **Auditoría**: Cada conversión queda registrada con trazabilidad completa
4. **Fallbacks**: Múltiples fuentes para garantizar disponibilidad

---

## 📊 Reportes DGII

### Formato 607 (Ventas)

Las conversiones se incluyen automáticamente:

- Monto facturado: En DOP
- ITBIS: 18% sobre DOP
- Referencia: ID de conversión para auditoría

### Datos a Incluir

- Fecha de transacción
- Monto original (USD/EUR)
- Tasa aplicada
- Fuente de la tasa (BCRD)
- Monto en DOP
- ITBIS calculado
- NCF emitido

---

## 🧪 Tests

### Tests Implementados (20 tests)

- ✅ ExchangeRate creation
- ✅ ConvertToDop calculations
- ✅ ConvertFromDop calculations
- ✅ ITBIS 18% calculation
- ✅ CurrencyConversion creation
- ✅ ConversionResult success/failure
- ✅ Edge cases (zero, negative amounts)
- ✅ EUR conversion
- ✅ Enum values verification

```bash
# Ejecutar tests
cd backend/PaymentService
dotnet test PaymentService.Tests --filter "ExchangeRate"
```

---

## 📁 Archivos del Módulo

### Domain

- `Entities/ExchangeRate.cs` - Entidad de tasa de cambio
- `Entities/CurrencyConversion.cs` - Registro de conversión
- `Enums/ExchangeRateSource.cs` - Fuentes de tasa
- `Interfaces/IExchangeRateRepository.cs`
- `Interfaces/ICurrencyConversionRepository.cs`
- `Interfaces/IExchangeRateService.cs` + ConversionResult

### Infrastructure

- `Repositories/ExchangeRateRepository.cs`
- `Repositories/CurrencyConversionRepository.cs`
- `Services/BancoCentralApiClient.cs` - Cliente HTTP BCRD
- `Services/ExchangeRateService.cs` - Lógica de conversión
- `Services/ExchangeRateRefreshJob.cs` - Job diario
- `Services/Settings/BancoCentralSettings.cs`
- `Migrations/20260128_AddExchangeRateTables.cs`

### API

- `Controllers/ExchangeRatesController.cs` - Endpoints REST

### Tests

- `ExchangeRateTests.cs` - 20 tests unitarios

---

## 🚀 Próximos Pasos

1. [ ] Registrar API key en portal BCRD
2. [ ] Configurar variable de entorno BCRD_API_KEY
3. [ ] Migrar base de datos
4. [ ] Integrar con flujo de checkout
5. [ ] Crear UI de selector de moneda
6. [ ] Agregar precios en USD/EUR en listings

---

## 📚 Referencias

- [Banco Central RD - Tasas de Cambio](https://www.bancentral.gov.do/a/d/2532-tasas-de-cambio)
- [Portal API BCRD](https://api.bancentral.gov.do/)
- [DGII - Norma General 06-2018](https://dgii.gov.do/legislacion/normasGenerales)
- [Ley 11-92 Código Tributario](https://dgii.gov.do/legislacion/leyesTributarias)

---

**Última actualización:** Enero 28, 2026
