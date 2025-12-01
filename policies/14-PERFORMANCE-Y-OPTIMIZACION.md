# POLÍTICA 14: PERFORMANCE Y OPTIMIZACIÓN

**Versión**: 1.0  
**Última Actualización**: 2025-11-30  
**Estado**: OBLIGATORIO ✅  
**Responsable**: Equipo de Arquitectura CarDealer

---

## 📋 RESUMEN EJECUTIVO

**POLÍTICA CRÍTICA**: Todos los microservicios deben cumplir con SLAs de performance estrictos: API response time p95 <1s, database queries <200ms, throughput mínimo 100 req/s, y zero memory leaks. Performance testing con BenchmarkDotNet y load testing con k6 son OBLIGATORIOS antes de deployment a producción.

**Objetivo**: Garantizar que los microservicios respondan rápidamente bajo carga, utilicen recursos eficientemente, y escalen horizontalmente sin degradación.

**Alcance**: Aplica a TODOS los microservicios del ecosistema CarDealer.

---

## 🎯 PERFORMANCE SLAs

### Service Level Agreements

| Métrica | Target | Límite | Medición |
|---------|--------|--------|----------|
| **API Response Time (p50)** | <200ms | <500ms | Percentil 50 |
| **API Response Time (p95)** | <500ms | <1000ms | Percentil 95 |
| **API Response Time (p99)** | <1000ms | <2000ms | Percentil 99 |
| **Database Query Time** | <100ms | <200ms | Promedio |
| **Throughput** | >100 req/s | >50 req/s | Requests por segundo |
| **Error Rate** | <0.1% | <1% | Errores / Total requests |
| **CPU Usage** | <50% | <70% | Promedio |
| **Memory Usage** | <60% | <80% | Promedio |
| **Startup Time** | <10s | <30s | Cold start |

---

## 🚀 OPTIMIZACIÓN DE DATABASE QUERIES

### 1. Evitar N+1 Queries

#### ❌ PROHIBIDO - N+1 Problem

```csharp
// ❌ MAL - Carga lazy, genera N+1 queries
public async Task<List<ErrorLogDto>> GetAllErrors()
{
    var errors = await _context.ErrorLogs.ToListAsync();
    
    // Para cada error, hace una query adicional (N queries)
    foreach (var error in errors)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == error.ServiceId);
        error.Service = service;
    }
    
    return errors;
}
```

#### ✅ CORRECTO - Eager Loading

```csharp
// ✅ BIEN - Una sola query con JOIN
public async Task<List<ErrorLogDto>> GetAllErrors()
{
    var errors = await _context.ErrorLogs
        .Include(e => e.Service)           // Eager loading
        .Include(e => e.User)              // Multiple includes
        .ThenInclude(u => u.Department)    // Nested include
        .ToListAsync();
    
    return _mapper.Map<List<ErrorLogDto>>(errors);
}
```

---

### 2. Usar AsNoTracking para Read-Only Queries

```csharp
// ❌ MAL - Tracking innecesario para read-only
public async Task<List<ErrorLogDto>> GetAllErrors()
{
    var errors = await _context.ErrorLogs
        .Include(e => e.Service)
        .ToListAsync();  // EF Core trackea todas las entidades
    
    return _mapper.Map<List<ErrorLogDto>>(errors);
}

// ✅ BIEN - AsNoTracking para mejor performance
public async Task<List<ErrorLogDto>> GetAllErrors()
{
    var errors = await _context.ErrorLogs
        .AsNoTracking()  // 30-40% más rápido
        .Include(e => e.Service)
        .ToListAsync();
    
    return _mapper.Map<List<ErrorLogDto>>(errors);
}
```

**Performance Gain**: 30-40% más rápido, 50% menos memoria

---

### 3. Proyecciones en vez de Entidades Completas

```csharp
// ❌ MAL - Carga toda la entidad con todos los campos
public async Task<List<ErrorSummaryDto>> GetErrorSummaries()
{
    var errors = await _context.ErrorLogs
        .AsNoTracking()
        .ToListAsync();  // Carga StackTrace, Message completo, etc.
    
    return errors.Select(e => new ErrorSummaryDto
    {
        Id = e.Id,
        ServiceName = e.ServiceName,
        Timestamp = e.Timestamp
    }).ToList();
}

// ✅ BIEN - Solo selecciona campos necesarios
public async Task<List<ErrorSummaryDto>> GetErrorSummaries()
{
    var summaries = await _context.ErrorLogs
        .AsNoTracking()
        .Select(e => new ErrorSummaryDto
        {
            Id = e.Id,
            ServiceName = e.ServiceName,
            Timestamp = e.Timestamp
        })
        .ToListAsync();  // Solo 3 columnas en SELECT
    
    return summaries;
}
```

**Performance Gain**: 50-70% menos datos transferidos

---

### 4. Paginación Obligatoria

```csharp
// ❌ MAL - Sin paginación
public async Task<List<ErrorLogDto>> GetAllErrors()
{
    return await _context.ErrorLogs
        .AsNoTracking()
        .ToListAsync();  // Puede retornar millones de registros
}

// ✅ BIEN - Paginación implementada
public async Task<PaginatedResult<ErrorLogDto>> GetAllErrors(
    int pageNumber = 1, 
    int pageSize = 20)
{
    // Validación
    pageSize = Math.Min(pageSize, 100);  // Máximo 100 por página
    
    var totalCount = await _context.ErrorLogs.CountAsync();
    
    var errors = await _context.ErrorLogs
        .AsNoTracking()
        .OrderByDescending(e => e.Timestamp)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PaginatedResult<ErrorLogDto>
    {
        Items = _mapper.Map<List<ErrorLogDto>>(errors),
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
    };
}
```

---

### 5. Índices de Base de Datos

```sql
-- Migration para agregar índices
CREATE INDEX idx_error_logs_timestamp 
    ON error_logs(timestamp DESC);

CREATE INDEX idx_error_logs_service_name 
    ON error_logs(service_name);

CREATE INDEX idx_error_logs_status_code 
    ON error_logs(status_code);

-- Índice compuesto para queries comunes
CREATE INDEX idx_error_logs_service_timestamp 
    ON error_logs(service_name, timestamp DESC);

-- Índice parcial para errores críticos
CREATE INDEX idx_error_logs_critical 
    ON error_logs(timestamp DESC) 
    WHERE status_code >= 500;
```

```csharp
// EF Core Migration
public partial class AddIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "idx_error_logs_timestamp",
            table: "error_logs",
            column: "timestamp",
            descending: true);
        
        migrationBuilder.CreateIndex(
            name: "idx_error_logs_service_name",
            table: "error_logs",
            column: "service_name");
        
        migrationBuilder.CreateIndex(
            name: "idx_error_logs_service_timestamp",
            table: "error_logs",
            columns: new[] { "service_name", "timestamp" },
            descending: new[] { false, true });
    }
}
```

---

### 6. Query Execution Plan Analysis

```sql
-- PostgreSQL: Analizar plan de ejecución
EXPLAIN ANALYZE
SELECT e.id, e.service_name, e.timestamp
FROM error_logs e
WHERE e.service_name = 'NotificationService'
  AND e.timestamp >= NOW() - INTERVAL '7 days'
ORDER BY e.timestamp DESC
LIMIT 20;

-- Buscar Sequential Scans (malo) vs Index Scans (bueno)
```

---

## 🔄 ASYNC/AWAIT BEST PRACTICES

### 1. No Bloquear Threads

```csharp
// ❌ MAL - Bloquea thread
public IActionResult GetError(Guid id)
{
    var error = _repository.GetByIdAsync(id).Result;  // ❌ Deadlock risk
    return Ok(error);
}

public IActionResult GetError2(Guid id)
{
    var error = _repository.GetByIdAsync(id).GetAwaiter().GetResult();  // ❌ Bloquea
    return Ok(error);
}

public IActionResult GetError3(Guid id)
{
    Task.Run(async () => await _repository.GetByIdAsync(id)).Wait();  // ❌ Overhead
    return Ok(error);
}

// ✅ BIEN - Async todo el camino
public async Task<IActionResult> GetError(Guid id)
{
    var error = await _repository.GetByIdAsync(id);
    return Ok(error);
}
```

---

### 2. ConfigureAwait en Libraries

```csharp
// En library code (no ASP.NET Core controllers)
public async Task<ErrorLog> GetByIdAsync(Guid id)
{
    using var connection = await _connectionFactory
        .CreateConnectionAsync()
        .ConfigureAwait(false);  // No necesita SynchronizationContext
    
    var error = await connection
        .QuerySingleOrDefaultAsync<ErrorLog>(
            "SELECT * FROM error_logs WHERE id = @Id",
            new { Id = id })
        .ConfigureAwait(false);
    
    return error;
}

// En ASP.NET Core controllers NO es necesario ConfigureAwait(false)
public async Task<IActionResult> GetError(Guid id)
{
    var error = await _repository.GetByIdAsync(id);  // OK sin ConfigureAwait
    return Ok(error);
}
```

---

### 3. Paralelizar Operaciones Independientes

```csharp
// ❌ MAL - Secuencial (1.5 segundos)
public async Task<DashboardDto> GetDashboard()
{
    var totalErrors = await _repository.GetTotalCountAsync();      // 500ms
    var recentErrors = await _repository.GetRecentErrorsAsync();   // 500ms
    var errorsByService = await _repository.GetByServiceAsync();   // 500ms
    
    return new DashboardDto
    {
        TotalErrors = totalErrors,
        RecentErrors = recentErrors,
        ErrorsByService = errorsByService
    };
}

// ✅ BIEN - Paralelo (500ms)
public async Task<DashboardDto> GetDashboard()
{
    var totalErrorsTask = _repository.GetTotalCountAsync();
    var recentErrorsTask = _repository.GetRecentErrorsAsync();
    var errorsByServiceTask = _repository.GetByServiceAsync();
    
    await Task.WhenAll(
        totalErrorsTask,
        recentErrorsTask,
        errorsByServiceTask);
    
    return new DashboardDto
    {
        TotalErrors = await totalErrorsTask,
        RecentErrors = await recentErrorsTask,
        ErrorsByService = await errorsByServiceTask
    };
}
```

**Performance Gain**: 3x más rápido

---

## 💾 CACHING STRATEGIES

### 1. In-Memory Caching (IMemoryCache)

```csharp
// Configuración
public void ConfigureServices(IServiceCollection services)
{
    services.AddMemoryCache(options =>
    {
        options.SizeLimit = 1024;  // 1024 entradas máximo
        options.CompactionPercentage = 0.25;  // Compactar al 75% de uso
    });
}

// Uso
public class ErrorService
{
    private readonly IMemoryCache _cache;
    private readonly IErrorRepository _repository;
    
    public async Task<ErrorStatistics> GetStatisticsAsync()
    {
        var cacheKey = "error_statistics";
        
        // Intentar obtener del cache
        if (!_cache.TryGetValue(cacheKey, out ErrorStatistics statistics))
        {
            // No está en cache, calcular
            statistics = await _repository.CalculateStatisticsAsync();
            
            // Guardar en cache por 5 minutos
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetPriority(CacheItemPriority.High)
                .SetSize(1);  // Cuenta como 1 entrada
            
            _cache.Set(cacheKey, statistics, cacheOptions);
        }
        
        return statistics;
    }
    
    // Invalidar cache cuando se crea nuevo error
    public async Task<Guid> LogErrorAsync(LogErrorCommand command)
    {
        var errorId = await _repository.AddAsync(command);
        
        // Invalidar cache de estadísticas
        _cache.Remove("error_statistics");
        
        return errorId;
    }
}
```

---

### 2. Distributed Caching (Redis)

```csharp
// Configuración
public void ConfigureServices(IServiceCollection services)
{
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = Configuration.GetConnectionString("Redis");
        options.InstanceName = "ErrorService:";
    });
}

// Uso
public class ErrorService
{
    private readonly IDistributedCache _cache;
    private readonly IErrorRepository _repository;
    
    public async Task<ErrorLog?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"error:{id}";
        
        // Intentar obtener del cache
        var cachedJson = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedJson))
        {
            return JsonSerializer.Deserialize<ErrorLog>(cachedJson);
        }
        
        // No está en cache, consultar DB
        var error = await _repository.GetByIdAsync(id);
        
        if (error != null)
        {
            // Guardar en cache por 1 hora
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            
            var json = JsonSerializer.Serialize(error);
            await _cache.SetStringAsync(cacheKey, json, options);
        }
        
        return error;
    }
}
```

---

### 3. Response Caching Middleware

```csharp
// Program.cs
builder.Services.AddResponseCaching();

var app = builder.Build();

app.UseResponseCaching();
app.UseAuthorization();

// Controller
[ApiController]
[Route("api/[controller]")]
public class ErrorsController : ControllerBase
{
    // Cache por 60 segundos
    [HttpGet("statistics")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetStatistics()
    {
        var stats = await _service.GetStatisticsAsync();
        return Ok(stats);
    }
    
    // No cachear
    [HttpPost]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> LogError([FromBody] LogErrorCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
}
```

---

## 🔥 HTTP CLIENT OPTIMIZATION

### 1. Usar IHttpClientFactory

```csharp
// ❌ MAL - HttpClient creado manualmente
public class NotificationService
{
    public async Task NotifyAsync(string message)
    {
        using var client = new HttpClient();  // ❌ Socket exhaustion
        await client.PostAsync("http://api/notify", content);
    }
}

// ✅ BIEN - IHttpClientFactory
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpClient("NotificationClient", client =>
    {
        client.BaseAddress = new Uri("http://api/");
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("User-Agent", "ErrorService/1.0");
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))  // Reciclar handlers
    .AddPolicyHandler(GetRetryPolicy())           // Polly integration
    .AddPolicyHandler(GetCircuitBreakerPolicy());
}

public class NotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public NotificationService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task NotifyAsync(string message)
    {
        var client = _httpClientFactory.CreateClient("NotificationClient");
        await client.PostAsync("notify", content);
    }
}
```

---

### 2. Connection Pooling

```csharp
// PostgreSQL connection pooling
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=errorservice;Username=postgres;Password=pass;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Connection Lifetime=300"
}

// RabbitMQ connection pooling
public class RabbitMqConnectionPool
{
    private readonly ConcurrentBag<IConnection> _connections = new();
    private readonly ConnectionFactory _factory;
    private readonly int _maxConnections = 10;
    
    public IConnection GetConnection()
    {
        if (_connections.TryTake(out var connection) && connection.IsOpen)
        {
            return connection;
        }
        
        if (_connections.Count < _maxConnections)
        {
            return _factory.CreateConnection();
        }
        
        // Wait and retry
        Thread.Sleep(100);
        return GetConnection();
    }
    
    public void ReturnConnection(IConnection connection)
    {
        if (connection.IsOpen)
        {
            _connections.Add(connection);
        }
    }
}
```

---

## 📊 BENCHMARKING CON BENCHMARKDOTNET

### 1. Setup BenchmarkDotNet

```xml
<!-- ErrorService.Benchmarks/ErrorService.Benchmarks.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="BenchmarkDotNet" Version="0.13.10" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ErrorService.Application\ErrorService.Application.csproj" />
  </ItemGroup>
</Project>
```

---

### 2. Benchmark Example

```csharp
// ErrorService.Benchmarks/RepositoryBenchmarks.cs
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace ErrorService.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 5)]
    public class RepositoryBenchmarks
    {
        private IErrorRepository _repository;
        private List<Guid> _errorIds;
        
        [GlobalSetup]
        public void Setup()
        {
            // Setup repository con datos de prueba
            _repository = new ErrorRepository(connectionFactory);
            _errorIds = SeedTestData(100);
        }
        
        [Benchmark(Baseline = true)]
        public async Task<ErrorLog> GetById_WithTracking()
        {
            var id = _errorIds[Random.Shared.Next(_errorIds.Count)];
            return await _repository.GetByIdAsync(id);
        }
        
        [Benchmark]
        public async Task<ErrorLog> GetById_AsNoTracking()
        {
            var id = _errorIds[Random.Shared.Next(_errorIds.Count)];
            return await _repository.GetByIdAsNoTrackingAsync(id);
        }
        
        [Benchmark]
        public async Task<ErrorSummaryDto> GetById_Projection()
        {
            var id = _errorIds[Random.Shared.Next(_errorIds.Count)];
            return await _repository.GetSummaryByIdAsync(id);
        }
        
        [Benchmark]
        public async Task<List<ErrorLog>> GetAll_EagerLoading()
        {
            return await _repository.GetAllWithIncludesAsync();
        }
        
        [Benchmark]
        public async Task<List<ErrorLog>> GetAll_LazyLoading()
        {
            return await _repository.GetAllLazyAsync();
        }
    }
    
    // Program.cs
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RepositoryBenchmarks>();
        }
    }
}
```

---

### 3. Ejecutar Benchmarks

```bash
# Modo Release (OBLIGATORIO)
dotnet run -c Release --project ErrorService.Benchmarks

# Resultados ejemplo:
# |                    Method |      Mean |    Error |   StdDev |    Median | Ratio | Gen0 | Allocated |
# |-------------------------- |----------:|---------:|---------:|----------:|------:|-----:|----------:|
# |      GetById_WithTracking | 1.234 ms | 0.024 ms | 0.018 ms | 1.230 ms |  1.00 | 12.0 |   45.2 KB |
# |   GetById_AsNoTracking    | 0.876 ms | 0.017 ms | 0.013 ms | 0.872 ms |  0.71 |  8.0 |   28.4 KB |
# |      GetById_Projection   | 0.543 ms | 0.011 ms | 0.009 ms | 0.541 ms |  0.44 |  4.0 |   12.1 KB |
```

---

## 🧪 LOAD TESTING CON K6

### 1. Instalación de k6

```bash
# Windows (Chocolatey)
choco install k6

# Linux
sudo gpg -k
sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

---

### 2. Load Test Script

```javascript
// tests/load/errorservice-load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

// Test configuration
export const options = {
  stages: [
    { duration: '2m', target: 50 },   // Ramp-up to 50 users
    { duration: '5m', target: 50 },   // Stay at 50 users
    { duration: '2m', target: 100 },  // Ramp-up to 100 users
    { duration: '5m', target: 100 },  // Stay at 100 users
    { duration: '2m', target: 200 },  // Spike to 200 users
    { duration: '5m', target: 200 },  // Stay at 200 users
    { duration: '5m', target: 0 },    // Ramp-down to 0
  ],
  thresholds: {
    'http_req_duration': ['p(95)<1000'],  // 95% requests < 1s
    'http_req_failed': ['rate<0.01'],     // Error rate < 1%
    'errors': ['rate<0.01'],
  },
};

const BASE_URL = 'https://errorservice.cardealer.com';
const AUTH_TOKEN = __ENV.AUTH_TOKEN || 'your-jwt-token';

export function setup() {
  // Login and get token if needed
  const loginRes = http.post(`${BASE_URL}/api/auth/login`, JSON.stringify({
    username: 'loadtest',
    password: 'LoadTest123!'
  }), {
    headers: { 'Content-Type': 'application/json' },
  });
  
  return { token: loginRes.json('token') };
}

export default function(data) {
  const headers = {
    'Authorization': `Bearer ${data.token}`,
    'Content-Type': 'application/json',
  };
  
  // Test 1: Get all errors (paginated)
  const listRes = http.get(`${BASE_URL}/api/errors?pageNumber=1&pageSize=20`, {
    headers: headers,
  });
  
  check(listRes, {
    'list status 200': (r) => r.status === 200,
    'list response time < 500ms': (r) => r.timings.duration < 500,
  }) || errorRate.add(1);
  
  sleep(1);
  
  // Test 2: Get error by ID
  const errorId = listRes.json('items.0.id');
  if (errorId) {
    const getRes = http.get(`${BASE_URL}/api/errors/${errorId}`, {
      headers: headers,
    });
    
    check(getRes, {
      'get status 200': (r) => r.status === 200,
      'get response time < 200ms': (r) => r.timings.duration < 200,
    }) || errorRate.add(1);
  }
  
  sleep(1);
  
  // Test 3: Create new error
  const payload = JSON.stringify({
    serviceName: 'LoadTestService',
    exceptionType: 'LoadTestException',
    message: `Load test error ${Date.now()}`,
    stackTrace: 'at LoadTest...',
    statusCode: 500
  });
  
  const createRes = http.post(`${BASE_URL}/api/errors`, payload, {
    headers: headers,
  });
  
  check(createRes, {
    'create status 201': (r) => r.status === 201,
    'create response time < 1s': (r) => r.timings.duration < 1000,
  }) || errorRate.add(1);
  
  sleep(1);
  
  // Test 4: Get statistics
  const statsRes = http.get(`${BASE_URL}/api/errors/statistics`, {
    headers: headers,
  });
  
  check(statsRes, {
    'stats status 200': (r) => r.status === 200,
    'stats response time < 500ms': (r) => r.timings.duration < 500,
  }) || errorRate.add(1);
  
  sleep(2);
}

export function teardown(data) {
  // Cleanup if needed
}
```

---

### 3. Ejecutar Load Test

```bash
# Test básico
k6 run tests/load/errorservice-load-test.js

# Test con custom duration
k6 run --duration 10m --vus 100 tests/load/errorservice-load-test.js

# Test con output a InfluxDB
k6 run --out influxdb=http://localhost:8086/k6 tests/load/errorservice-load-test.js

# Test con thresholds específicos
k6 run --thresholds 'http_req_duration=p(95)<500' tests/load/errorservice-load-test.js
```

---

### 4. Resultados k6

```
     ✓ list status 200
     ✓ list response time < 500ms
     ✓ get status 200
     ✓ get response time < 200ms
     ✓ create status 201
     ✓ create response time < 1s
     ✓ stats status 200
     ✓ stats response time < 500ms

     checks.........................: 100.00% ✓ 48532      ✗ 0
     data_received..................: 125 MB  520 kB/s
     data_sent......................: 38 MB   158 kB/s
     http_req_blocked...............: avg=1.23ms   min=0s      med=0s      max=145ms   p(90)=0s      p(95)=0s
     http_req_connecting............: avg=453µs    min=0s      med=0s      max=89ms    p(90)=0s      p(95)=0s
     http_req_duration..............: avg=287ms    min=45ms    med=234ms   max=982ms   p(90)=512ms   p(95)=687ms
     http_req_failed................: 0.00%   ✓ 0          ✗ 12133
     http_req_receiving.............: avg=234µs    min=18µs    med=156µs   max=12ms    p(90)=456µs   p(95)=678µs
     http_req_sending...............: avg=123µs    min=12µs    med=87µs    max=8ms     p(90)=234µs   p(95)=345µs
     http_req_tls_handshaking.......: avg=0s       min=0s      med=0s      max=0s      p(90)=0s      p(95)=0s
     http_req_waiting...............: avg=286ms    min=44ms    med=233ms   max=981ms   p(90)=511ms   p(95)=686ms
     http_reqs......................: 12133   50.55/s
     iteration_duration.............: avg=5.15s    min=5.04s   med=5.12s   max=6.23s   p(90)=5.45s   p(95)=5.67s
     iterations.....................: 3033    12.64/s
     vus............................: 100     min=10       max=200
     vus_max........................: 200     min=200      max=200
```

---

## 🎯 PERFORMANCE PROFILING

### 1. dotnet-trace (CPU Profiling)

```bash
# Instalar dotnet-trace
dotnet tool install --global dotnet-trace

# Iniciar aplicación
dotnet run --project ErrorService.Api

# Encontrar PID
dotnet-trace ps

# Capturar trace por 60 segundos
dotnet-trace collect --process-id <PID> --duration 00:01:00

# Analizar con PerfView o speedscope.app
# Subir trace.nettrace a https://www.speedscope.app/
```

---

### 2. dotnet-counters (Real-time Metrics)

```bash
# Instalar dotnet-counters
dotnet tool install --global dotnet-counters

# Monitorear métricas en tiempo real
dotnet-counters monitor --process-id <PID>

# Métricas específicas
dotnet-counters monitor \
  --process-id <PID> \
  --counters System.Runtime,Microsoft.AspNetCore.Hosting

# Ejemplo de output:
# [System.Runtime]
#     CPU Usage (%)                                   23
#     Working Set (MB)                               156
#     GC Heap Size (MB)                               45
#     Gen 0 GC Count                                 234
#     Gen 1 GC Count                                  12
#     Gen 2 GC Count                                   3
#     ThreadPool Thread Count                         28
#     Monitor Lock Contention Count                    5
#     ThreadPool Queue Length                          0
#     
# [Microsoft.AspNetCore.Hosting]
#     Requests Per Second                           98.5
#     Total Requests                              123456
#     Current Requests                                12
#     Failed Requests                                  5
```

---

### 3. dotnet-dump (Memory Analysis)

```bash
# Instalar dotnet-dump
dotnet tool install --global dotnet-dump

# Capturar memory dump
dotnet-dump collect --process-id <PID>

# Analizar dump
dotnet-dump analyze core_<timestamp>

# Comandos útiles dentro de dotnet-dump:
> dumpheap -stat                 # Ver estadísticas de heap
> dumpheap -mt <MethodTable>     # Ver objetos de un tipo
> gcroot <ObjectAddress>         # Ver qué mantiene vivo un objeto
> eeheap -gc                     # Estadísticas de GC heap
> threads                        # Ver threads
> clrstack                       # Ver stack trace
```

---

## 🧹 MEMORY OPTIMIZATION

### 1. Dispose Pattern

```csharp
// ✅ BIEN - IDisposable implementado correctamente
public class ErrorRepository : IErrorRepository, IDisposable
{
    private readonly NpgsqlConnection _connection;
    private bool _disposed = false;
    
    public ErrorRepository(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
    }
    
    public async Task<ErrorLog> GetByIdAsync(Guid id)
    {
        await _connection.OpenAsync();
        // ... query logic
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _connection?.Dispose();
            }
            
            // Free unmanaged resources (if any)
            
            _disposed = true;
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    ~ErrorRepository()
    {
        Dispose(false);
    }
}

// Uso con using statement
using (var repository = new ErrorRepository(connectionString))
{
    var error = await repository.GetByIdAsync(id);
}

// O usando declaration (C# 8+)
using var repository = new ErrorRepository(connectionString);
var error = await repository.GetByIdAsync(id);
```

---

### 2. Object Pooling

```csharp
// ArrayPool para evitar allocations
using System.Buffers;

public class LogProcessor
{
    public async Task ProcessLogs(List<string> logs)
    {
        // ❌ MAL - Crea array cada vez
        var buffer = new byte[8192];
        
        // ✅ BIEN - Usa ArrayPool
        var buffer = ArrayPool<byte>.Shared.Rent(8192);
        try
        {
            // Usar buffer
            await ProcessBuffer(buffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}

// ObjectPool para objetos reutilizables
public class StringBuilderPool
{
    private static readonly ObjectPool<StringBuilder> _pool = 
        new DefaultObjectPoolProvider()
            .Create(new StringBuilderPooledObjectPolicy());
    
    public static string BuildString(Action<StringBuilder> build)
    {
        var sb = _pool.Get();
        try
        {
            build(sb);
            return sb.ToString();
        }
        finally
        {
            _pool.Return(sb);
        }
    }
}
```

---

### 3. Span<T> y Memory<T>

```csharp
// ❌ MAL - Crea substring (allocation)
public string ExtractServiceName(string message)
{
    var index = message.IndexOf(':');
    return message.Substring(0, index);  // Allocation
}

// ✅ BIEN - Usa ReadOnlySpan<char> (no allocation)
public ReadOnlySpan<char> ExtractServiceName(ReadOnlySpan<char> message)
{
    var index = message.IndexOf(':');
    return message.Slice(0, index);  // No allocation
}

// Ejemplo con parsing
public bool TryParseErrorCode(string input, out int errorCode)
{
    ReadOnlySpan<char> span = input.AsSpan();
    return int.TryParse(span, out errorCode);
}
```

---

## ✅ PERFORMANCE CHECKLIST

### Database
- [ ] No hay N+1 queries
- [ ] AsNoTracking() en read-only queries
- [ ] Proyecciones en vez de entidades completas
- [ ] Paginación implementada (máximo 100 items)
- [ ] Índices en columnas filtradas/ordenadas
- [ ] Connection pooling configurado
- [ ] Query timeout configurado (<200ms)

### Async/Await
- [ ] No hay .Result o .Wait()
- [ ] Async todo el camino (controllers, services, repositories)
- [ ] ConfigureAwait(false) en library code
- [ ] Operaciones independientes paralelizadas
- [ ] CancellationToken propagado

### Caching
- [ ] IMemoryCache para datos frecuentes
- [ ] IDistributedCache (Redis) para datos compartidos
- [ ] Response caching en endpoints read-only
- [ ] Cache invalidation strategy implementada
- [ ] TTL apropiado configurado

### HTTP
- [ ] IHttpClientFactory usado
- [ ] Connection pooling habilitado
- [ ] Timeout configurado
- [ ] Circuit breaker + Retry policies

### Memory
- [ ] IDisposable implementado
- [ ] using statements para recursos
- [ ] No hay memory leaks (verificado con dotnet-dump)
- [ ] ArrayPool usado para buffers grandes
- [ ] Span<T> usado donde apropiado

### Testing
- [ ] BenchmarkDotNet benchmarks ejecutados
- [ ] Load testing con k6 (100 req/s mínimo)
- [ ] Profiling con dotnet-trace
- [ ] Memory analysis con dotnet-dump
- [ ] SLAs cumplidos (p95 <1s)

---

## 📚 RECURSOS Y REFERENCIAS

- **BenchmarkDotNet**: [https://benchmarkdotnet.org/](https://benchmarkdotnet.org/)
- **k6 Load Testing**: [https://k6.io/docs/](https://k6.io/docs/)
- **dotnet-trace**: [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace)
- **EF Core Performance**: [Microsoft Docs](https://docs.microsoft.com/en-us/ef/core/performance/)
- **Span<T> Performance**: [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.span-1)

---

**Fecha de Vigencia**: 2025-11-30  
**Aprobado por**: Equipo de Arquitectura CarDealer  
**Revisión**: Trimestral

**NOTA**: Performance es NO NEGOCIABLE. PRs que no cumplan SLAs (p95 >1s) o tengan N+1 queries son automáticamente RECHAZADOS.
