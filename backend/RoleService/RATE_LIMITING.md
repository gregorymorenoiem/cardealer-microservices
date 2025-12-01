# Rate Limiting - ErrorService

## Descripción

El ErrorService implementa un sistema robusto de **Rate Limiting** para proteger el servicio contra abuso, ataques DDoS y garantizar un uso justo de los recursos.

## Características

### ✅ Características Implementadas

1. **Rate Limiting Global**
   - Límite de requests por IP
   - Ventana de tiempo configurable
   - Respuesta con HTTP 429 (Too Many Requests)

2. **Rate Limiting por Endpoint**
   - Diferentes límites para diferentes endpoints
   - Configuración granular mediante atributos
   - Límites más altos para operaciones read, más bajos para write

3. **Rate Limiting por Cliente**
   - Soporte para API Keys
   - Identificación mediante header `X-Client-Id`
   - Políticas personalizadas por cliente

4. **Whitelist de IPs**
   - IPs excluidas del rate limiting
   - Configurable por ambiente
   - Soporta múltiples IPs

5. **Bypass de Rate Limiting**
   - Health checks sin limitación
   - Atributo `[AllowRateLimitBypass]` para endpoints específicos

6. **Logging**
   - Eventos de rate limit excedido registrados
   - Información de cliente IP y endpoint
   - Auditoría completa

## Configuración

### Por Ambiente

#### Development
```json
"RateLimiting": {
  "Enabled": true,
  "MaxRequests": 1000,
  "WindowSeconds": 60,
  "EnableLogging": true,
  "WhitelistedIps": ["127.0.0.1", "::1", "localhost"]
}
```

#### Production
```json
"RateLimiting": {
  "Enabled": true,
  "MaxRequests": 50,
  "WindowSeconds": 60,
  "EnableLogging": true,
  "WhitelistedIps": ["127.0.0.1"]
}
```

## Límites por Endpoint

| Endpoint | Método | Límite | Ventana |
|----------|--------|--------|---------|
| `/api/errors` | POST | 200 req | 60 seg |
| `/api/errors` | GET | 150 req | 60 seg |
| `/api/errors/{id}` | GET | 200 req | 60 seg |
| `/api/errors/stats` | GET | 100 req | 60 seg |
| `/api/errors/services` | GET | 150 req | 60 seg |
| `/api/health` | GET | ∞ (bypass) | - |
| `/health` | GET | ∞ (bypass) | - |

## Uso

### Atributos Disponibles

#### 1. `[RateLimit]` - Rate Limiting Global
```csharp
[HttpPost]
[RateLimit(maxRequests: 200, windowSeconds: 60)]
public async Task<ActionResult> LogError([FromBody] LogErrorRequest request)
{
    // Máximo 200 requests en 60 segundos
}
```

#### 2. `[ClientRateLimit]` - Rate Limiting por Cliente
```csharp
[HttpPost]
[ClientRateLimit(maxRequests: 50, windowSeconds: 60)]
public async Task<ActionResult> LogError([FromBody] LogErrorRequest request)
{
    // Máximo 50 requests por cliente en 60 segundos
}
```

#### 3. `[AllowRateLimitBypass]` - Bypass del Rate Limiting
```csharp
[HttpGet]
[AllowRateLimitBypass]
public ActionResult<ApiResponse<string>> Get()
{
    // Este endpoint no tiene límite de rate
}
```

## Headers HTTP

### Identificación de Cliente

El servicio identifica a los clientes en el siguiente orden de prioridad:

1. Header `X-Client-Id` (mayor prioridad)
   ```
   X-Client-Id: my-api-client-123
   ```

2. Token Bearer en Authorization
   ```
   Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
   ```

3. IP del cliente (fallback)
   ```
   Remote IP Address
   ```

### Respuesta 429 (Too Many Requests)

```json
{
  "timestamp": "2025-11-28T10:30:45Z",
  "statusCode": 429,
  "message": "Rate limit exceeded. Maximum allowed requests have been reached.",
  "errors": null
}
```

Headers adicionales:
```
Retry-After: 45
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1732790445
```

## Configuración Personalizada

### Modificar límites globales

Editar `appsettings.json`:
```json
"RateLimiting": {
  "MaxRequests": 100,
  "WindowSeconds": 60
}
```

### Modificar límites por endpoint

En el controlador:
```csharp
[RateLimit(maxRequests: 300, windowSeconds: 120)]
public async Task<ActionResult> CustomEndpoint()
{
    // Máximo 300 requests en 120 segundos
}
```

### Agregar IPs a whitelist

En `appsettings.json`:
```json
"RateLimiting": {
  "WhitelistedIps": [
    "127.0.0.1",
    "192.168.1.10",
    "10.0.0.0/8"
  ]
}
```

## Monitoreo y Debugging

### Logs

El rate limiting registra eventos cuando:
- Un límite es excedido
- Una IP es rechazada
- Un cliente es identificado

Ejemplo en logs:
```
[WRN] Rate limit exceeded for IP 192.168.1.100. Endpoint: GET /api/errors/services
[INF] Rate limiting initialized with 100 requests per 60 seconds
```

### Verificar estado

```csharp
// Obtener configuración actual
var config = app.ApplicationServices.GetService<RateLimitingConfiguration>();
Console.WriteLine($"Límite global: {config.MaxRequests} requests/{config.WindowSeconds}s");
```

## Mejores Prácticas

### Para Clientes de API

1. **Implementar retry con backoff exponencial**
   ```csharp
   for (int i = 0; i < maxRetries; i++)
   {
       try
       {
           var response = await client.GetAsync(url);
           if (response.StatusCode != 429) break;
           
           int retryAfter = int.Parse(response.Headers.GetValues("Retry-After").First());
           await Task.Delay(retryAfter * 1000 * (int)Math.Pow(2, i));
       }
       catch (HttpRequestException ex) { }
   }
   ```

2. **Usar X-Client-Id header**
   ```csharp
   client.DefaultRequestHeaders.Add("X-Client-Id", "my-service-v1");
   ```

3. **Implementar caché local**
   - Cachear resultados de GET requests
   - Reducir número de requests

### Para Administradores

1. **Ajustar límites según carga**
   - Monitor CPU y memoria
   - Aumentar límites si hay recursos disponibles

2. **Whitelist de IPs internas**
   - Agregar IPs de otros microservicios
   - Excluir load balancers

3. **Monitorear eventos 429**
   - Alertas cuando clientes son bloqueados
   - Análisis de patrones de ataque

## Troubleshooting

### Problema: "429 Too Many Requests"

**Soluciones:**
1. Verificar X-Client-Id header está correcto
2. Aumentar `MaxRequests` en configuración
3. Aumentar `WindowSeconds`
4. Agregar IP a whitelist si es necesario

### Problema: Algunos endpoints no tienen límite

**Verificar:**
1. ¿Tiene el atributo `[AllowRateLimitBypass]`?
2. ¿Es una IP en whitelist?
3. ¿Está habilitado el rate limiting en config?

### Problema: Rate Limit no se aplica

**Verificar:**
1. `Enabled: true` en appsettings.json
2. Middleware está configurado en Program.cs
3. Atributo está presente en el endpoint
4. Cache de builder se limpió

## API Reference

### RateLimitingConfiguration

```csharp
public class RateLimitingConfiguration
{
    public int MaxRequests { get; set; } = 100;        // Máximo de requests
    public int WindowSeconds { get; set; } = 60;       // Ventana de tiempo
    public bool Enabled { get; set; } = true;          // Habilitar/Deshabilitar
    public bool EnableLogging { get; set; } = true;    // Log eventos
    public List<string> WhitelistedIps { get; set; }   // IPs excluidas
}
```

## Archivos Relacionados

- `ErrorService.Shared/RateLimiting/RateLimitingConfiguration.cs` - Configuración
- `ErrorService.Shared/RateLimiting/RateLimitAttributes.cs` - Atributos
- `ErrorService.Shared/Extensions/RateLimitingExtensions.cs` - Extensiones
- `ErrorService.Shared/Middleware/RateLimitBypassMiddleware.cs` - Middleware
- `ErrorService.Api/Program.cs` - Registro de servicios

## Recursos

- [AspNetCoreRateLimit Documentation](https://github.com/stefanprodan/AspNetCoreRateLimit)
- [HTTP 429 Status Code](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/429)
- [RFC 6585 - Additional HTTP Status Codes](https://tools.ietf.org/html/rfc6585)
