# POLÍTICA 09: RESILIENCIA Y MANEJO DE ERRORES

**Versión**: 1.0  
**Última Actualización**: 2025-11-30  
**Estado**: OBLIGATORIO ✅  
**Responsable**: Equipo de Arquitectura CarDealer

---

## 📋 RESUMEN EJECUTIVO

**POLÍTICA CRÍTICA**: Todos los microservicios deben implementar Circuit Breaker + Retry Policies + Timeout + Fallback usando Polly 8.4.2. Los errores transitorios (red, timeouts, 5xx) DEBEN manejarse con reintentos automáticos. Los errores permanentes (4xx) NO deben reintentarse.

**Objetivo**: Garantizar que los microservicios continúen funcionando ante fallos transitorios, eviten cascadas de errores, y se recuperen automáticamente cuando los servicios dependientes vuelvan a estar disponibles.

**Alcance**: Aplica a TODOS los microservicios del ecosistema CarDealer.

---

## 🎯 PATRONES DE RESILIENCIA OBLIGATORIOS

### Matriz de Resiliencia

| Patrón | Tecnología | Propósito | Obligatorio |
|--------|------------|-----------|-------------|
| **Circuit Breaker** | Polly 8.4.2 | Evitar llamadas a servicios caídos | ✅ SÍ |
| **Retry** | Polly 8.4.2 | Reintentar errores transitorios | ✅ SÍ |
| **Timeout** | Polly 8.4.2 | Evitar esperas infinitas | ✅ SÍ |
| **Bulkhead Isolation** | Polly 8.4.2 | Limitar concurrencia | ✅ SÍ |
| **Fallback** | Polly 8.4.2 | Respuesta alternativa | ✅ SÍ |
| **Dead Letter Queue** | RabbitMQ | Mensajes fallidos | ✅ SÍ |
| **Global Exception Handler** | ASP.NET Core | Capturar excepciones no manejadas | ✅ SÍ |
| **Health Checks** | ASP.NET Core | Monitoreo de dependencias | ✅ SÍ |

---

## 🔄 PATRÓN 1: CIRCUIT BREAKER

### ¿Qué es Circuit Breaker?

Circuit Breaker previene llamadas repetidas a un servicio que está fallando, evitando cascadas de errores y permitiendo que el servicio se recupere.

**Estados**:
- **Closed**: Funcionamiento normal
- **Open**: Servicio caído, rechaza llamadas
- **Half-Open**: Prueba si el servicio se recuperó

---

### Configuración de Circuit Breaker (Polly 8.4.2)

```csharp
// Program.cs
using Polly;
using Polly.CircuitBreaker;

var builder = WebApplication.CreateBuilder(args);

// Configurar Circuit Breaker para HttpClient
builder.Services.AddHttpClient("ErrorServiceClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddResilienceHandler("ErrorServiceResiliencePipeline", resilienceBuilder =>
{
    // 1. CIRCUIT BREAKER
    resilienceBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        // Número de fallos consecutivos antes de abrir el circuito
        FailureRatio = 0.5,  // 50% de fallos
        MinimumThroughput = 10,  // Mínimo 10 requests en la ventana
        
        // Duración de la ventana de muestreo
        SamplingDuration = TimeSpan.FromSeconds(30),
        
        // Tiempo que el circuito permanece abierto
        BreakDuration = TimeSpan.FromSeconds(30),
        
        // Predicado: qué errores cuentan como fallo
        ShouldHandle = new PredicateBuilder()
            .Handle<HttpRequestException>()
            .Handle<TaskCanceledException>()
            .HandleResult<HttpResponseMessage>(response =>
                response.StatusCode >= System.Net.HttpStatusCode.InternalServerError),
        
        // Eventos
        OnOpened = args =>
        {
            Console.WriteLine($"Circuit breaker opened at {args.Context.OperationKey}");
            return ValueTask.CompletedTask;
        },
        
        OnClosed = args =>
        {
            Console.WriteLine($"Circuit breaker closed at {args.Context.OperationKey}");
            return ValueTask.CompletedTask;
        },
        
        OnHalfOpened = args =>
        {
            Console.WriteLine($"Circuit breaker half-opened at {args.Context.OperationKey}");
            return ValueTask.CompletedTask;
        }
    });
    
    // 2. RETRY (Exponential Backoff)
    resilienceBuilder.AddRetry(new Polly.Retry.RetryStrategyOptions<HttpResponseMessage>
    {
        // Máximo 3 reintentos
        MaxRetryAttempts = 3,
        
        // Exponential backoff: 2^retry * 1s = 1s, 2s, 4s
        BackoffType = DelayBackoffType.Exponential,
        Delay = TimeSpan.FromSeconds(1),
        
        // Predicado: qué errores reintentar
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(response =>
                response.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                response.StatusCode >= System.Net.HttpStatusCode.InternalServerError),
        
        // Evento
        OnRetry = args =>
        {
            Console.WriteLine(
                $"Retry {args.AttemptNumber} after {args.RetryDelay.TotalSeconds}s delay. " +
                $"Outcome: {args.Outcome.Result?.StatusCode}");
            return ValueTask.CompletedTask;
        }
    });
    
    // 3. TIMEOUT
    resilienceBuilder.AddTimeout(new Polly.Timeout.TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(10),
        
        OnTimeout = args =>
        {
            Console.WriteLine($"Timeout after {args.Timeout.TotalSeconds}s");
            return ValueTask.CompletedTask;
        }
    });
});
```

---

### Uso de HttpClient con Resiliencia

```csharp
// ErrorNotificationHandler.cs
using System.Net.Http;

namespace NotificationService.Application.Handlers
{
    public class ErrorNotificationHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ErrorNotificationHandler> _logger;
        
        public ErrorNotificationHandler(
            IHttpClientFactory httpClientFactory,
            ILogger<ErrorNotificationHandler> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        
        public async Task NotifyErrorServiceAsync(ErrorDto error)
        {
            try
            {
                // HttpClient con Circuit Breaker + Retry + Timeout
                var client = _httpClientFactory.CreateClient("ErrorServiceClient");
                
                var response = await client.PostAsJsonAsync("/api/errors", error);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Error notification sent successfully to ErrorService");
                }
                else
                {
                    _logger.LogWarning(
                        "ErrorService returned {StatusCode}", 
                        response.StatusCode);
                }
            }
            catch (BrokenCircuitException ex)
            {
                // Circuit abierto - servicio caído
                _logger.LogError(
                    ex,
                    "Circuit breaker is open. ErrorService is unavailable.");
                
                // Fallback: guardar en Dead Letter Queue
                await SaveToDeadLetterQueueAsync(error);
            }
            catch (TimeoutRejectedException ex)
            {
                // Timeout
                _logger.LogError(
                    ex,
                    "Request to ErrorService timed out");
                
                await SaveToDeadLetterQueueAsync(error);
            }
            catch (HttpRequestException ex)
            {
                // Error de red
                _logger.LogError(
                    ex,
                    "Network error calling ErrorService");
                
                await SaveToDeadLetterQueueAsync(error);
            }
        }
        
        private async Task SaveToDeadLetterQueueAsync(ErrorDto error)
        {
            // Guardar en DLQ para procesamiento posterior
            _logger.LogInformation(
                "Saving error to Dead Letter Queue for later processing");
            
            // Implementación de DLQ (RabbitMQ)
        }
    }
}
```

---

## ⏱️ PATRÓN 2: RETRY POLICIES

### Estrategias de Retry

| Estrategia | Cuándo Usar | Ejemplo |
|------------|-------------|---------|
| **Fixed Delay** | Errores predecibles | 1s, 1s, 1s |
| **Exponential Backoff** | Errores transitorios | 1s, 2s, 4s, 8s |
| **Jitter** | Evitar thundering herd | 1s±0.5s, 2s±1s |

---

### Retry con Exponential Backoff + Jitter

```csharp
// Program.cs
builder.Services.AddResilienceHandler("DatabaseRetry", resilienceBuilder =>
{
    resilienceBuilder.AddRetry(new Polly.Retry.RetryStrategyOptions
    {
        MaxRetryAttempts = 5,
        BackoffType = DelayBackoffType.Exponential,
        Delay = TimeSpan.FromSeconds(1),
        
        // Jitter: ±25% de variación aleatoria
        UseJitter = true,
        
        ShouldHandle = new PredicateBuilder()
            .Handle<Npgsql.NpgsqlException>(ex =>
                ex.IsTransient || ex.Message.Contains("timeout")),
        
        OnRetry = args =>
        {
            Console.WriteLine(
                $"Database retry {args.AttemptNumber}/{args.MaxAttempts} " +
                $"after {args.RetryDelay.TotalSeconds:F2}s");
            return ValueTask.CompletedTask;
        }
    });
});
```

---

### Retry Selectivo (NO reintentar 4xx)

```csharp
// ❌ INCORRECTO - Reintentar todos los errores
resilienceBuilder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
{
    MaxRetryAttempts = 3,
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .HandleResult(response => !response.IsSuccessStatusCode)  // ❌ MAL
});

// ✅ CORRECTO - Reintentar solo errores transitorios
resilienceBuilder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
{
    MaxRetryAttempts = 3,
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .HandleResult(response =>
            // Timeout (408)
            response.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
            // Too Many Requests (429)
            response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
            // Server Errors (5xx)
            response.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
});
```

**REGLA**: NO reintentar errores 4xx (Bad Request, Unauthorized, Not Found) porque son permanentes.

---

## ⏰ PATRÓN 3: TIMEOUT POLICIES

### Tipos de Timeout

| Tipo | Propósito | Ejemplo |
|------|-----------|---------|
| **Request Timeout** | Tiempo máximo por request | 10s |
| **Operation Timeout** | Tiempo total (con reintentos) | 30s |
| **Connection Timeout** | Tiempo para establecer conexión | 5s |

---

### Timeout en HttpClient

```csharp
// Program.cs
builder.Services.AddHttpClient("ExternalApiClient", client =>
{
    // Connection timeout (tiempo para conectar)
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddResilienceHandler("ExternalApiPipeline", resilienceBuilder =>
{
    // Request timeout (tiempo por request individual)
    resilienceBuilder.AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(10),
        
        OnTimeout = args =>
        {
            Console.WriteLine(
                $"Request timeout after {args.Timeout.TotalSeconds}s. " +
                $"Operation: {args.Context.OperationKey}");
            return ValueTask.CompletedTask;
        }
    });
});
```

---

### Timeout en Repository (Database)

```csharp
// ErrorRepository.cs
using Npgsql;

namespace ErrorService.Infrastructure.Persistence
{
    public class ErrorRepository : IErrorRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        
        public async Task<ErrorLog?> GetByIdAsync(Guid id)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            
            // Timeout a nivel de comando (15 segundos)
            using var command = new NpgsqlCommand(
                "SELECT * FROM error_logs WHERE id = @Id",
                connection as NpgsqlConnection)
            {
                CommandTimeout = 15  // ✅ Timeout de 15 segundos
            };
            
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapToErrorLog(reader);
            }
            
            return null;
        }
    }
}
```

---

## 🚧 PATRÓN 4: BULKHEAD ISOLATION

### ¿Qué es Bulkhead?

Bulkhead limita la concurrencia para evitar que un recurso lento (base de datos, API externa) bloquee todo el sistema.

---

### Configuración de Bulkhead

```csharp
// Program.cs
builder.Services.AddResilienceHandler("DatabaseBulkhead", resilienceBuilder =>
{
    resilienceBuilder.AddConcurrencyLimiter(new Polly.RateLimiting.ConcurrencyLimiterOptions
    {
        // Máximo 50 operaciones concurrentes
        PermitLimit = 50,
        
        // Cola de espera de 100 operaciones
        QueueLimit = 100,
        
        OnRejected = args =>
        {
            Console.WriteLine(
                $"Bulkhead rejected request. Queue full.");
            return ValueTask.CompletedTask;
        }
    });
});
```

---

### Uso de Bulkhead

```csharp
// ErrorService - Limitar concurrencia de escritura en DB
public class LogErrorCommandHandler : IRequestHandler<LogErrorCommand, Guid>
{
    private readonly IErrorRepository _repository;
    private readonly ResiliencePipeline _bulkheadPipeline;
    
    public LogErrorCommandHandler(
        IErrorRepository repository,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _repository = repository;
        _bulkheadPipeline = pipelineProvider.GetPipeline("DatabaseBulkhead");
    }
    
    public async Task<Guid> Handle(LogErrorCommand request, CancellationToken cancellationToken)
    {
        // Ejecutar dentro del bulkhead
        return await _bulkheadPipeline.ExecuteAsync(async ct =>
        {
            var errorLog = new ErrorLog
            {
                Id = Guid.NewGuid(),
                ServiceName = request.ServiceName,
                ExceptionType = request.ExceptionType,
                Message = request.Message,
                StackTrace = request.StackTrace,
                StatusCode = request.StatusCode,
                Timestamp = DateTime.UtcNow
            };
            
            await _repository.AddAsync(errorLog);
            
            return errorLog.Id;
        }, cancellationToken);
    }
}
```

---

## 🔄 PATRÓN 5: FALLBACK STRATEGIES

### Fallback en HttpClient

```csharp
// Program.cs
builder.Services.AddHttpClient("WeatherApiClient")
    .AddResilienceHandler("WeatherApiPipeline", resilienceBuilder =>
    {
        // 1. Timeout
        resilienceBuilder.AddTimeout(TimeSpan.FromSeconds(5));
        
        // 2. Retry
        resilienceBuilder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = 2
        });
        
        // 3. Circuit Breaker
        resilienceBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            FailureRatio = 0.5,
            MinimumThroughput = 5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(30)
        });
    });

// WeatherService.cs
public class WeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WeatherService> _logger;
    
    public async Task<WeatherData> GetWeatherAsync(string city)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("WeatherApiClient");
            var response = await client.GetAsync($"/weather?city={city}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WeatherData>();
            }
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning(ex, "Weather API circuit is open");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Weather API");
        }
        
        // ✅ FALLBACK: Devolver datos cacheados o por defecto
        return GetCachedWeatherOrDefault(city);
    }
    
    private WeatherData GetCachedWeatherOrDefault(string city)
    {
        _logger.LogInformation("Returning fallback weather data for {City}", city);
        
        return new WeatherData
        {
            City = city,
            Temperature = 20.0,
            Condition = "Unknown",
            IsFallback = true
        };
    }
}
```

---

## 🚨 PATRÓN 6: GLOBAL EXCEPTION HANDLER

### Middleware de Excepciones

```csharp
// GlobalExceptionMiddleware.cs
using System.Net;
using System.Text.Json;

namespace ErrorService.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        
        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception: {Message}",
                    ex.Message);
                
                await HandleExceptionAsync(context, ex);
            }
        }
        
        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var (statusCode, message) = exception switch
            {
                ValidationException => (HttpStatusCode.BadRequest, "Validation error"),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
                NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
                BrokenCircuitException => (HttpStatusCode.ServiceUnavailable, "Service temporarily unavailable"),
                TimeoutRejectedException => (HttpStatusCode.RequestTimeout, "Request timeout"),
                _ => (HttpStatusCode.InternalServerError, "Internal server error")
            };
            
            context.Response.StatusCode = (int)statusCode;
            
            var response = new
            {
                error = message,
                detail = exception.Message,
                traceId = context.TraceIdentifier,
                timestamp = DateTime.UtcNow
            };
            
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await context.Response.WriteAsync(json);
        }
    }
}
```

```csharp
// Program.cs
var app = builder.Build();

// Registrar middleware ANTES de los endpoints
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapControllers();
```

---

## 💀 PATRÓN 7: DEAD LETTER QUEUE (RabbitMQ)

### Configuración de DLQ

```csharp
// RabbitMqConfiguration.cs
using RabbitMQ.Client;

namespace NotificationService.Infrastructure.Messaging
{
    public static class RabbitMqConfiguration
    {
        public static void ConfigureDeadLetterQueue(IModel channel)
        {
            // 1. Declarar Dead Letter Exchange
            channel.ExchangeDeclare(
                exchange: "dlx.notifications",
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);
            
            // 2. Declarar Dead Letter Queue
            channel.QueueDeclare(
                queue: "notifications.dlq",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            
            // 3. Bind DLQ a DLX
            channel.QueueBind(
                queue: "notifications.dlq",
                exchange: "dlx.notifications",
                routingKey: "notifications");
            
            // 4. Declarar cola principal con DLX configurado
            var queueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "dlx.notifications" },
                { "x-dead-letter-routing-key", "notifications" },
                { "x-message-ttl", 86400000 }  // 24 horas
            };
            
            channel.QueueDeclare(
                queue: "notifications",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArgs);
            
            channel.QueueBind(
                queue: "notifications",
                exchange: "notifications.exchange",
                routingKey: "notifications");
        }
    }
}
```

---

### Consumer con Retry y DLQ

```csharp
// NotificationConsumer.cs
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Infrastructure.Messaging
{
    public class NotificationConsumer
    {
        private readonly IModel _channel;
        private readonly ILogger<NotificationConsumer> _logger;
        private const int MaxRetryAttempts = 3;
        
        public void StartConsuming()
        {
            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += async (sender, eventArgs) =>
            {
                var retryCount = GetRetryCount(eventArgs.BasicProperties);
                
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    // Procesar mensaje
                    await ProcessMessageAsync(message);
                    
                    // ACK - mensaje procesado exitosamente
                    _channel.BasicAck(eventArgs.DeliveryTag, false);
                    
                    _logger.LogInformation(
                        "Message processed successfully. DeliveryTag={DeliveryTag}",
                        eventArgs.DeliveryTag);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error processing message. RetryCount={RetryCount}",
                        retryCount);
                    
                    if (retryCount < MaxRetryAttempts)
                    {
                        // Reintentar: NACK + requeue
                        _channel.BasicNack(
                            eventArgs.DeliveryTag,
                            multiple: false,
                            requeue: true);
                        
                        // Incrementar contador de reintentos
                        IncrementRetryCount(eventArgs.BasicProperties);
                        
                        _logger.LogWarning(
                            "Message requeued for retry {RetryCount}/{MaxRetries}",
                            retryCount + 1,
                            MaxRetryAttempts);
                    }
                    else
                    {
                        // Máximo de reintentos alcanzado: enviar a DLQ
                        _channel.BasicNack(
                            eventArgs.DeliveryTag,
                            multiple: false,
                            requeue: false);  // ✅ NO requeue = va a DLQ
                        
                        _logger.LogError(
                            "Message sent to DLQ after {MaxRetries} failed attempts",
                            MaxRetryAttempts);
                    }
                }
            };
            
            _channel.BasicConsume(
                queue: "notifications",
                autoAck: false,  // ✅ Manual ACK
                consumer: consumer);
        }
        
        private int GetRetryCount(IBasicProperties properties)
        {
            if (properties.Headers != null &&
                properties.Headers.TryGetValue("x-retry-count", out var value))
            {
                return (int)value;
            }
            
            return 0;
        }
        
        private void IncrementRetryCount(IBasicProperties properties)
        {
            properties.Headers ??= new Dictionary<string, object>();
            
            var currentCount = GetRetryCount(properties);
            properties.Headers["x-retry-count"] = currentCount + 1;
        }
        
        private async Task ProcessMessageAsync(string message)
        {
            // Lógica de procesamiento
            await Task.Delay(100);  // Simulación
        }
    }
}
```

---

## 🏥 PATRÓN 8: HEALTH CHECKS

### Configuración de Health Checks

```csharp
// Program.cs
using Microsoft.Extensions.Diagnostics.HealthChecks;

builder.Services.AddHealthChecks()
    // PostgreSQL
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "postgresql" })
    
    // RabbitMQ
    .AddRabbitMQ(
        rabbitConnectionString: builder.Configuration.GetConnectionString("RabbitMQ")!,
        name: "rabbitmq",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "messaging", "rabbitmq" })
    
    // Custom health check
    .AddCheck<ErrorServiceHealthCheck>("errorservice-custom");

var app = builder.Build();

// Endpoints de health checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds,
                exception = entry.Value.Exception?.Message
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false  // Solo verifica que la app esté ejecutándose
});
```

---

### Custom Health Check

```csharp
// ErrorServiceHealthCheck.cs
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ErrorService.Api.HealthChecks
{
    public class ErrorServiceHealthCheck : IHealthCheck
    {
        private readonly IErrorRepository _repository;
        private readonly ILogger<ErrorServiceHealthCheck> _logger;
        
        public ErrorServiceHealthCheck(
            IErrorRepository repository,
            ILogger<ErrorServiceHealthCheck> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Verificar conectividad con base de datos
                var canConnect = await _repository.CanConnectAsync();
                
                if (!canConnect)
                {
                    return HealthCheckResult.Unhealthy(
                        "Cannot connect to database");
                }
                
                // Verificar que hay menos de 1000 errores en la última hora
                var recentErrorsCount = await _repository.CountRecentErrorsAsync(
                    TimeSpan.FromHours(1));
                
                if (recentErrorsCount > 1000)
                {
                    return HealthCheckResult.Degraded(
                        $"High error rate: {recentErrorsCount} errors in last hour",
                        data: new Dictionary<string, object>
                        {
                            { "errorCount", recentErrorsCount },
                            { "threshold", 1000 }
                        });
                }
                
                return HealthCheckResult.Healthy(
                    "ErrorService is healthy",
                    data: new Dictionary<string, object>
                    {
                        { "errorCount", recentErrorsCount }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                
                return HealthCheckResult.Unhealthy(
                    "Health check failed",
                    exception: ex);
            }
        }
    }
}
```

---

## 🔄 AUTO-RECOVERY MECHANISMS

### Reconexión Automática a RabbitMQ

```csharp
// RabbitMqConnectionFactory.cs
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ErrorService.Infrastructure.Messaging
{
    public class RabbitMqConnectionFactory : IDisposable
    {
        private readonly ILogger<RabbitMqConnectionFactory> _logger;
        private readonly ConnectionFactory _connectionFactory;
        private IConnection? _connection;
        private IModel? _channel;
        private bool _disposed;
        
        public RabbitMqConnectionFactory(
            IConfiguration configuration,
            ILogger<RabbitMqConnectionFactory> logger)
        {
            _logger = logger;
            
            _connectionFactory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"],
                Port = int.Parse(configuration["RabbitMQ:Port"]!),
                UserName = configuration["RabbitMQ:Username"],
                Password = configuration["RabbitMQ:Password"],
                
                // Auto-recovery
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                
                // Timeouts
                RequestedHeartbeat = TimeSpan.FromSeconds(60),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(30)
            };
        }
        
        public IModel GetChannel()
        {
            if (_channel == null || _channel.IsClosed)
            {
                EnsureConnection();
                _channel = _connection!.CreateModel();
                
                _logger.LogInformation("RabbitMQ channel created");
            }
            
            return _channel;
        }
        
        private void EnsureConnection()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _connection = _connectionFactory.CreateConnection();
                
                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;
                _connection.ConnectionUnblocked += OnConnectionUnblocked;
                
                _logger.LogInformation(
                    "RabbitMQ connection established to {Host}:{Port}",
                    _connectionFactory.HostName,
                    _connectionFactory.Port);
            }
        }
        
        private void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            _logger.LogWarning(
                "RabbitMQ connection shutdown. Reason: {Reason}",
                e.ReplyText);
        }
        
        private void OnCallbackException(object? sender, CallbackExceptionEventArgs e)
        {
            _logger.LogError(
                e.Exception,
                "RabbitMQ callback exception: {Message}",
                e.Exception.Message);
        }
        
        private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
        {
            _logger.LogWarning(
                "RabbitMQ connection blocked. Reason: {Reason}",
                e.Reason);
        }
        
        private void OnConnectionUnblocked(object? sender, EventArgs e)
        {
            _logger.LogInformation("RabbitMQ connection unblocked");
        }
        
        public void Dispose()
        {
            if (_disposed)
                return;
            
            _channel?.Close();
            _channel?.Dispose();
            
            _connection?.Close();
            _connection?.Dispose();
            
            _disposed = true;
            
            _logger.LogInformation("RabbitMQ connection disposed");
        }
    }
}
```

---

## 📊 MONITOREO DE RESILIENCIA

### Métricas de Resiliencia

```csharp
// ResilienceMetrics.cs
using System.Diagnostics.Metrics;

namespace ErrorService.Infrastructure.Metrics
{
    public class ResilienceMetrics
    {
        private readonly Counter<long> _circuitBreakerOpened;
        private readonly Counter<long> _circuitBreakerClosed;
        private readonly Counter<long> _retryAttempts;
        private readonly Counter<long> _timeouts;
        private readonly Counter<long> _fallbacks;
        
        public ResilienceMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("ErrorService.Resilience");
            
            _circuitBreakerOpened = meter.CreateCounter<long>(
                "circuit_breaker_opened_total",
                description: "Total number of times circuit breaker opened");
            
            _circuitBreakerClosed = meter.CreateCounter<long>(
                "circuit_breaker_closed_total",
                description: "Total number of times circuit breaker closed");
            
            _retryAttempts = meter.CreateCounter<long>(
                "retry_attempts_total",
                description: "Total number of retry attempts");
            
            _timeouts = meter.CreateCounter<long>(
                "timeouts_total",
                description: "Total number of timeouts");
            
            _fallbacks = meter.CreateCounter<long>(
                "fallbacks_total",
                description: "Total number of fallback executions");
        }
        
        public void RecordCircuitBreakerOpened(string serviceName)
        {
            _circuitBreakerOpened.Add(1, new KeyValuePair<string, object?>("service", serviceName));
        }
        
        public void RecordCircuitBreakerClosed(string serviceName)
        {
            _circuitBreakerClosed.Add(1, new KeyValuePair<string, object?>("service", serviceName));
        }
        
        public void RecordRetryAttempt(string operation, int attemptNumber)
        {
            _retryAttempts.Add(1,
                new KeyValuePair<string, object?>("operation", operation),
                new KeyValuePair<string, object?>("attempt", attemptNumber));
        }
        
        public void RecordTimeout(string operation)
        {
            _timeouts.Add(1, new KeyValuePair<string, object?>("operation", operation));
        }
        
        public void RecordFallback(string operation)
        {
            _fallbacks.Add(1, new KeyValuePair<string, object?>("operation", operation));
        }
    }
}
```

---

## ✅ CHECKLIST DE RESILIENCIA

### Circuit Breaker
- [ ] Circuit Breaker configurado en HttpClients
- [ ] FailureRatio apropiado (típicamente 0.5)
- [ ] MinimumThroughput configurado (mínimo 10)
- [ ] BreakDuration configurado (30-60 segundos)
- [ ] Eventos de circuit breaker logueados
- [ ] Métricas de circuit breaker exportadas

### Retry Policies
- [ ] Retry con exponential backoff configurado
- [ ] MaxRetryAttempts apropiado (3-5)
- [ ] Jitter habilitado para evitar thundering herd
- [ ] Solo errores transitorios se reintentan (5xx, 408, 429)
- [ ] Errores 4xx NO se reintentan
- [ ] Eventos de retry logueados

### Timeout
- [ ] Timeout configurado en HttpClients
- [ ] Timeout configurado en comandos de base de datos
- [ ] Timeout apropiado según operación (5-30s)
- [ ] Eventos de timeout logueados

### Bulkhead Isolation
- [ ] Bulkhead configurado para operaciones concurrentes
- [ ] PermitLimit apropiado según carga esperada
- [ ] QueueLimit configurado
- [ ] Rejection handler implementado

### Fallback
- [ ] Fallback implementado para servicios críticos
- [ ] Datos cacheados disponibles como fallback
- [ ] Fallback logueado para monitoreo

### Dead Letter Queue
- [ ] DLQ configurado en RabbitMQ
- [ ] Máximo de reintentos definido (3-5)
- [ ] Mensajes fallidos enviados a DLQ
- [ ] Proceso para revisar DLQ implementado

### Global Exception Handler
- [ ] GlobalExceptionMiddleware registrado
- [ ] Todas las excepciones mapeadas a status codes HTTP
- [ ] TraceId incluido en respuestas de error
- [ ] Excepciones logueadas con contexto completo

### Health Checks
- [ ] Health checks para PostgreSQL
- [ ] Health checks para RabbitMQ
- [ ] Custom health checks implementados
- [ ] Endpoints `/health`, `/health/ready`, `/health/live`
- [ ] Health checks usados en Kubernetes liveness/readiness

### Auto-Recovery
- [ ] AutomaticRecoveryEnabled en RabbitMQ
- [ ] NetworkRecoveryInterval configurado
- [ ] Eventos de reconexión logueados
- [ ] Heartbeat configurado

### Monitoreo
- [ ] Métricas de resiliencia exportadas (Prometheus)
- [ ] Alertas configuradas para circuit breaker abierto
- [ ] Alertas configuradas para alta tasa de reintentos
- [ ] Dashboard de resiliencia en Grafana

---

## 🎯 TESTING DE RESILIENCIA

### Test de Circuit Breaker

```csharp
// CircuitBreakerTests.cs
using Xunit;
using Polly.CircuitBreaker;

namespace ErrorService.Tests.Resilience
{
    public class CircuitBreakerTests
    {
        [Fact]
        public async Task CircuitBreaker_Opens_After_Consecutive_Failures()
        {
            // Arrange
            var failureCount = 0;
            var circuitBreakerOpened = false;
            
            var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 3,
                    SamplingDuration = TimeSpan.FromSeconds(10),
                    BreakDuration = TimeSpan.FromSeconds(5),
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .HandleResult(r => !r.IsSuccessStatusCode),
                    OnOpened = args =>
                    {
                        circuitBreakerOpened = true;
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
            
            // Act - Simular 5 fallos consecutivos
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    await pipeline.ExecuteAsync(async ct =>
                    {
                        failureCount++;
                        return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
                    });
                }
                catch (BrokenCircuitException)
                {
                    // Circuit abierto
                }
            }
            
            // Assert
            Assert.True(circuitBreakerOpened);
            Assert.True(failureCount >= 3);
        }
    }
}
```

---

## 📚 RECURSOS Y REFERENCIAS

- **Microservicio de Referencia**: `ErrorService/Program.cs`
- **Polly Documentation**: [https://www.pollydocs.org/](https://www.pollydocs.org/)
- **RabbitMQ DLQ**: [https://www.rabbitmq.com/dlx.html](https://www.rabbitmq.com/dlx.html)
- **Health Checks**: [Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- **Resilience Patterns**: [Microsoft Azure Architecture](https://docs.microsoft.com/en-us/azure/architecture/patterns/category/resiliency)

---

**Fecha de Vigencia**: 2025-11-30  
**Aprobado por**: Equipo de Arquitectura CarDealer  
**Revisión**: Trimestral

**NOTA**: Resiliencia es OBLIGATORIA. PRs sin Circuit Breaker + Retry en llamadas externas son automáticamente RECHAZADOS.
