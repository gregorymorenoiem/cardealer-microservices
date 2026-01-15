# 💰 Pricing APIs - Valoración y Precios

**Categoría:** Pricing & Valuation  
**APIs:** 4 (KBB, Black Book, Edmunds, NADA Guides)  
**Fase:** 2-3 (Diferenciación)  
**Impacto:** 40% más conversiones con pricing correcto

---

## 📋 APIs en Esta Categoría

| API                        | Tipo              | Costo             | Prioridad | Implementación    |
| -------------------------- | ----------------- | ----------------- | --------- | ----------------- |
| **Kelley Blue Book (KBB)** | Market valuation  | $2,000-$5,000/mes | 🔴 ALTA   | Fase 2, Semana 6  |
| **Black Book**             | Wholesale/Retail  | $1,500-$4,000/mes | 🟠 MEDIA  | Fase 3, Semana 8  |
| **Edmunds**                | TMV + Incentivos  | $3,000-$8,000/mes | 🟠 MEDIA  | Fase 3, Semana 8  |
| **NADA Guides**            | Trucks/Commercial | $2,500-$6,000/mes | 🟡 BAJA   | Fase 3, Semana 10 |

---

## 🎯 Caso de Uso en OKLA

### Problema Actual:

- Dealers no saben qué precio poner
- Precios muy altos → sin ventas
- Precios muy bajos → pérdida de ingresos
- Compradores desconfían de precios "aleatorios"

### Solución:

```
Dealer publica vehículo
    ↓
Elige plan (Free/Starter/Pro/Enterprise)
    ↓
Escanea VIN
    ↓
Sistema sugiere precio automáticamente:
   - KBB: "Valor de mercado: $12,500"
   - Black Book: "Wholesale: $10,800, Retail: $13,200"
   - Edmunds: "TMV: $12,300, Incentivos aplicables"
    ↓
Dealer acepta sugerencia o ajusta
    ↓
Badge "Below KBB" si está 5% abajo
    ↓
Comprador ve precio confiable → +40% conversiones
```

---

## 💡 Valor por Plan

| Tier           | KBB | Black Book | Edmunds | NADA | Valor                            |
| -------------- | --- | ---------- | ------- | ---- | -------------------------------- |
| **Free**       | ❌  | ❌         | ❌      | ❌   | Dealers publican precio manual   |
| **Starter**    | ❌  | ❌         | ❌      | ❌   | + Foto enhancement               |
| **Pro**        | ✅  | ❌         | ❌      | ❌   | "Precio por debajo de KBB" badge |
| **Enterprise** | ✅  | ✅         | ✅      | ✅   | Premium pricing intelligence     |

---

## 📊 Especificaciones Técnicas

### KBB API

```
Endpoint:     GET https://api.kbb.com/v1/values/{vin}
Auth:         API Key
Rate Limit:   1,000 req/min
Response:     {
                value: 12500,
                low: 12000,
                high: 13000,
                trend: "stable",
                depreciation: "-8%/year"
              }
```

### Black Book API

```
Endpoint:     GET https://api.blackbook.com/api/valuation/{vin}
Auth:         OAuth 2.0
Rate Limit:   500 req/min
Response:     {
                wholesale: 10800,
                retail: 13200,
                score: 85,
                condition: "average"
              }
```

### Edmunds API

```
Endpoint:     GET https://api.edmunds.com/v2/api/tmv/get
Auth:         API Key
Rate Limit:   100 req/min
Response:     {
                tmv: 12300,
                tmvCalculated: true,
                incentivesSummary: {...}
              }
```

### NADA Guides API

```
Endpoint:     GET https://nadaguidesapi.nada.com/values
Auth:         API Key
Rate Limit:   600 req/min
Response:     {
                currentAvgRetail: 15200,
                currentAvgWholesale: 13400,
                equipment: [...]
              }
```

---

## 🔌 Integración en OKLA

### Microservicio Responsable:

**VehiclesSaleService** → PricingService

### Flujo de Datos:

```
Frontend: PublishVehicle
    ↓
Backend: POST /api/vehicles/suggest-price
    ↓
PricingService.GetSuggestedPriceAsync(vin, trim, mileage)
    ↓
Llama a APIs (KBB, Black Book, etc.)
    ↓
Promedia valores
    ↓
Retorna: {
  kbb: 12500,
  blackBook: { wholesale: 10800, retail: 13200 },
  edmunds: 12300,
  suggested: 12350,
  confidence: 0.95
}
    ↓
Frontend muestra sugerencia
    ↓
Dealer acepta o ajusta
    ↓
Guarda en DB con `pricing_source` y `pricing_confidence`
```

### Base de Datos:

```sql
-- Nueva tabla
CREATE TABLE vehicle_pricing (
    id UUID PRIMARY KEY,
    vehicle_id UUID REFERENCES vehicles(id),
    published_price DECIMAL(10,2),
    suggested_price DECIMAL(10,2),
    pricing_source VARCHAR(50), -- 'kbb', 'blackbook', etc.
    pricing_confidence DECIMAL(3,2), -- 0-1
    kbb_value DECIMAL(10,2),
    black_book_wholesale DECIMAL(10,2),
    black_book_retail DECIMAL(10,2),
    edmunds_tmv DECIMAL(10,2),
    nada_retail DECIMAL(10,2),
    price_difference DECIMAL(10,2),
    below_market_pct DECIMAL(5,2), -- % below KBB
    created_at TIMESTAMP,
    updated_at TIMESTAMP
);

-- Índices
CREATE INDEX idx_vehicle_pricing_vehicle_id ON vehicle_pricing(vehicle_id);
```

---

## 💰 ROI y Análisis de Costos

### Costo Mensual:

```
KBB:       $2,000-$5,000
Black Book: $1,500-$4,000
Edmunds:   $3,000-$8,000
NADA:      $2,500-$6,000
─────────────────────────
Total:     $9,000-$23,000/mes
           (pero se puede negociar por volumen a $5-8K)
```

### Revenue:

```
100 dealers activos:
├─ Free:           0 × $0 = $0
├─ Starter ($49):  30 × $49 = $1,470
├─ Pro ($129):     50 × $129 = $6,450
└─ Enterprise ($299): 20 × $299 = $5,980
                                  ────────
Total MRR:                        $13,900

API Cost: $7,500/mes (con descuento)
Margin: $6,400/mes (46%)

Scaled to 1,000 dealers: $139,000/mes, $64,000 profit
```

### Impacto en Métricas:

```
Sin pricing API:
├─ Avg. price deviation: 15-25% from market
├─ Buyer confidence: 40%
├─ Conversions: Baseline

Con KBB + APIs:
├─ Price deviation: <5% from market (95% accuracy)
├─ Buyer confidence: +60% (confianza en precio)
├─ Conversions: +40%
├─ Days on inventory: -25%
└─ Customer satisfaction: +45%

ROI: Por cada $1 invertido en APIs, se generan $5-10 en revenue adicional
```

---

## 🔧 Setup y Configuración

### Obtener Credenciales:

#### KBB:

1. Contactar: developer@kbb.com
2. Documento: Propuesta de integración OKLA
3. Volumen estimado: 100 vehículos/día
4. Timeline: 2-4 semanas (requiere negociación)
5. Docs: https://www.kbb.com/company/developer/

#### Black Book:

1. Contactar: partnerships@blackbook.com
2. Setup: API Key + mTLS certificate
3. Timeline: 1-2 semanas
4. Docs: https://www.blackbook.com/api

#### Edmunds:

1. Register: https://developer.edmunds.com/
2. Get API Key (instant)
3. Testing: Sandbox disponible
4. Docs: https://developer.edmunds.com/

#### NADA:

1. Contactar: support@nadaguidesapi.nada.com
2. Enterprise tier
3. Timeline: 2-3 semanas
4. Docs: https://nadaguidesapi.nada.com/

### Configuración en appsettings.json:

```json
{
  "PricingApis": {
    "Kbb": {
      "ApiKey": "****",
      "BaseUrl": "https://api.kbb.com/v1",
      "Enabled": true,
      "TimeoutSeconds": 5,
      "RetryAttempts": 3
    },
    "BlackBook": {
      "ApiKey": "****",
      "Certificate": "/certs/blackbook.pfx",
      "BaseUrl": "https://api.blackbook.com/api",
      "Enabled": true,
      "TimeoutSeconds": 5
    },
    "Edmunds": {
      "ApiKey": "****",
      "BaseUrl": "https://api.edmunds.com/v2",
      "Enabled": true,
      "TimeoutSeconds": 5
    },
    "Nada": {
      "ApiKey": "****",
      "BaseUrl": "https://nadaguidesapi.nada.com",
      "Enabled": true,
      "TimeoutSeconds": 5
    },
    "CacheDuration": 86400, // 24 horas
    "AveragingStrategy": "weighted" // peso por confianza
  }
}
```

---

## 📡 Ejemplos de Código

### C# - Backend Integration

```csharp
public interface IPricingService
{
    Task<PricingSuggestion> GetSuggestedPriceAsync(string vin, int mileage, CancellationToken ct);
    Task<KbbValuation> GetKbbValuationAsync(string vin, CancellationToken ct);
    Task<BlackBookValuation> GetBlackBookValuationAsync(string vin, CancellationToken ct);
}

public class PricingService : IPricingService
{
    private readonly IKbbClient _kbb;
    private readonly IBlackBookClient _blackBook;
    private readonly IEdmundsClient _edmunds;
    private readonly INadaClient _nada;
    private readonly ILogger<PricingService> _logger;

    public async Task<PricingSuggestion> GetSuggestedPriceAsync(
        string vin, int mileage, CancellationToken ct)
    {
        try
        {
            // Ejecutar todas las APIs en paralelo
            var tasks = new[]
            {
                _kbb.GetValuationAsync(vin, ct),
                _blackBook.GetValuationAsync(vin, ct),
                _edmunds.GetValuationAsync(vin, ct),
                _nada.GetValuationAsync(vin, ct)
            };

            await Task.WhenAll(tasks);

            // Combinar resultados
            var suggestion = new PricingSuggestion
            {
                KbbValue = tasks[0].Result?.Value,
                BlackBookWholesale = tasks[1].Result?.Wholesale,
                BlackBookRetail = tasks[1].Result?.Retail,
                EdmundsTmv = tasks[2].Result?.Tmv,
                NadaRetail = tasks[3].Result?.Retail,
                SuggestedPrice = CalculateWeightedAverage(tasks),
                Confidence = CalculateConfidence(tasks)
            };

            return suggestion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener precio sugerido");
            throw;
        }
    }

    private decimal CalculateWeightedAverage(Task<Valuation>[] tasks)
    {
        // Ponderar por confianza de cada API
        // KBB (40%), Black Book (30%), Edmunds (20%), NADA (10%)
        var weights = new[] { 0.40m, 0.30m, 0.20m, 0.10m };
        var values = new[]
        {
            tasks[0].Result?.Value ?? 0,
            tasks[1].Result?.Retail ?? 0,
            tasks[2].Result?.Tmv ?? 0,
            tasks[3].Result?.Retail ?? 0
        };

        return values.Zip(weights, (v, w) => v * w).Sum();
    }
}
```

### TypeScript - Frontend Integration

```typescript
export interface PricingSuggestion {
  kbbValue: number;
  blackBookWholesale: number;
  blackBookRetail: number;
  edmundsTmv: number;
  nadaRetail: number;
  suggestedPrice: number;
  confidence: number; // 0-1
  sources: string[];
}

export class PricingService {
  constructor(private http: HttpClient) {}

  suggestPrice(vin: string, mileage: number): Observable<PricingSuggestion> {
    return this.http.post<PricingSuggestion>("/api/vehicles/suggest-price", {
      vin,
      mileage,
    });
  }
}

// En componente
export const PublishVehicle = () => {
  const [suggestedPrice, setSuggestedPrice] =
    useState<PricingSuggestion | null>(null);

  const handleVinScanned = async (vin: string) => {
    const suggestion = await pricingService.suggestPrice(vin, formData.mileage);
    setSuggestedPrice(suggestion);
  };

  return (
    <div>
      <input
        placeholder="Escanea VIN"
        onBlur={(e) => handleVinScanned(e.target.value)}
      />

      {suggestedPrice && (
        <div className="pricing-suggestion">
          <h3>
            Precio Sugerido: ${suggestedPrice.suggestedPrice.toLocaleString()}
          </h3>

          <div className="sources">
            <p>📊 KBB: ${suggestedPrice.kbbValue.toLocaleString()}</p>
            <p>
              💰 Black Book (Retail): $
              {suggestedPrice.blackBookRetail.toLocaleString()}
            </p>
            <p>📈 Edmunds TMV: ${suggestedPrice.edmundsTmv.toLocaleString()}</p>
            <p>🚗 NADA: ${suggestedPrice.nadaRetail.toLocaleString()}</p>
          </div>

          <div className="confidence">
            Confianza: {(suggestedPrice.confidence * 100).toFixed(0)}%
          </div>

          <button
            onClick={() => acceptSuggestedPrice(suggestedPrice.suggestedPrice)}
          >
            Usar precio sugerido
          </button>
          <button>Ajustar manualmente</button>
        </div>
      )}
    </div>
  );
};
```

---

## 🧪 Testing

### Unit Tests:

```csharp
[Fact]
public async Task GetSuggestedPrice_WithValidVin_ReturnsSuggestion()
{
    // Arrange
    var vin = "1HGCM82633A123456";
    var service = new PricingService(_kbbMock, _bbMock, _edmundsMock, _nadaMock, _logger);

    // Act
    var result = await service.GetSuggestedPriceAsync(vin, 50000, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.Confidence > 0.8);
    Assert.InRange(result.SuggestedPrice, 10000, 20000);
}

[Fact]
public async Task GetSuggestedPrice_WithApiFailure_UsesAlternative()
{
    // Arrange - KBB throws exception
    _kbbMock.Setup(x => x.GetValuationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new HttpRequestException());

    // Act
    var result = await service.GetSuggestedPriceAsync("1HGCM82633A123456", 50000, CancellationToken.None);

    // Assert - Uses other sources
    Assert.NotNull(result);
    Assert.Null(result.KbbValue);
    Assert.NotNull(result.BlackBookRetail);
}
```

---

## 🚨 Troubleshooting

| Error                   | Causa                     | Solución                                   |
| ----------------------- | ------------------------- | ------------------------------------------ |
| **404 Not Found**       | VIN inválido              | Validar VIN con NHTSA antes                |
| **401 Unauthorized**    | API Key expirada          | Renovar credenciales                       |
| **429 Rate Limit**      | Demasiadas solicitudes    | Implementar queue/backoff                  |
| **Timeout**             | APIs lentas               | Aumentar timeout o cache resultados        |
| **Inconsistent prices** | APIs con datos diferentes | Usar weighted average, documentar variance |

---

## 📞 Contactos y Recursos

| API            | Contact                        | Docs                              | Support            |
| -------------- | ------------------------------ | --------------------------------- | ------------------ |
| **KBB**        | developer@kbb.com              | https://kbb.com/company/developer | Enterprise Support |
| **Black Book** | partnerships@blackbook.com     | https://blackbook.com/api         | Partner Portal     |
| **Edmunds**    | support@edmunds.com            | https://developer.edmunds.com     | Developers Forum   |
| **NADA**       | support@nadaguidesapi.nada.com | https://nadaguidesapi.nada.com    | Email Support      |

---

## 🎯 Próximos Pasos

1. **Semana 6:** Comenzar integración de KBB
2. **Semana 8:** Agregar Black Book y Edmunds
3. **Semana 10:** Integrar NADA para dealers comerciales
4. **Semana 12:** Optimización y caching

---

**Última actualización:** Enero 15, 2026  
**Status:** 📋 Documentación completada - Ready for implementation
