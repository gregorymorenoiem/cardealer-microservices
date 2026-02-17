using CarDealer.Shared.Resilience.Configuration;

namespace CarDealer.Shared.Resilience.Profiles;

/// <summary>
/// Perfiles de resiliencia predefinidos para diferentes tipos de servicios.
/// Cada perfil tiene configuraciones optimizadas según la criticidad del servicio.
/// </summary>
public static class ResilienceProfiles
{
    /// <summary>
    /// Perfil para servicios CRÍTICOS (AuthService, Gateway, BillingService).
    /// - Menos reintentos pero más rápidos
    /// - Circuit breaker más sensible
    /// - Bulkhead con mayor concurrencia
    /// </summary>
    public static ResilienceOptions Critical => new()
    {
        Enabled = true,
        Retry = new RetryOptions
        {
            MaxRetries = 2,
            DelaySeconds = 1,
            UseExponentialBackoff = true,
            UseJitter = true,
            RetryStatusCodes = new[] { 408, 429, 502, 503, 504 }
        },
        CircuitBreaker = new CircuitBreakerOptions
        {
            FailureRatio = 0.3, // 30% — más sensible
            MinimumThroughput = 5,
            SamplingDurationSeconds = 15,
            BreakDurationSeconds = 15
        },
        Timeout = new TimeoutOptions
        {
            TimeoutSeconds = 10,
            TotalTimeoutSeconds = 30
        },
        Bulkhead = new BulkheadOptions
        {
            Enabled = true,
            MaxParallelization = 50,
            MaxQueuingActions = 100
        },
        Fallback = new FallbackOptions
        {
            Enabled = true,
            UseCachedResponse = true,
            CacheTtlSeconds = 60
        }
    };

    /// <summary>
    /// Perfil para servicios ESTÁNDAR (UserService, RoleService, ContactService, etc.).
    /// - Reintentos normales con backoff
    /// - Circuit breaker estándar
    /// - Bulkhead moderado
    /// </summary>
    public static ResilienceOptions Standard => new()
    {
        Enabled = true,
        Retry = new RetryOptions
        {
            MaxRetries = 3,
            DelaySeconds = 2,
            UseExponentialBackoff = true,
            UseJitter = true,
            RetryStatusCodes = new[] { 408, 429, 500, 502, 503, 504 }
        },
        CircuitBreaker = new CircuitBreakerOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            SamplingDurationSeconds = 30,
            BreakDurationSeconds = 30
        },
        Timeout = new TimeoutOptions
        {
            TimeoutSeconds = 30,
            TotalTimeoutSeconds = 120
        },
        Bulkhead = new BulkheadOptions
        {
            Enabled = true,
            MaxParallelization = 25,
            MaxQueuingActions = 50
        },
        Fallback = new FallbackOptions
        {
            Enabled = true,
            UseCachedResponse = true,
            CacheTtlSeconds = 300
        }
    };

    /// <summary>
    /// Perfil para servicios de BACKGROUND/BATCH (NotificationService, AuditService, etc.).
    /// - Más reintentos con delays más largos
    /// - Circuit breaker más tolerante
    /// - Timeouts más largos
    /// </summary>
    public static ResilienceOptions Background => new()
    {
        Enabled = true,
        Retry = new RetryOptions
        {
            MaxRetries = 5,
            DelaySeconds = 5,
            UseExponentialBackoff = true,
            UseJitter = true,
            RetryStatusCodes = new[] { 408, 429, 500, 502, 503, 504 }
        },
        CircuitBreaker = new CircuitBreakerOptions
        {
            FailureRatio = 0.7,
            MinimumThroughput = 20,
            SamplingDurationSeconds = 60,
            BreakDurationSeconds = 60
        },
        Timeout = new TimeoutOptions
        {
            TimeoutSeconds = 60,
            TotalTimeoutSeconds = 300
        },
        Bulkhead = new BulkheadOptions
        {
            Enabled = true,
            MaxParallelization = 10,
            MaxQueuingActions = 100
        },
        Fallback = new FallbackOptions
        {
            Enabled = false
        }
    };

    /// <summary>
    /// Perfil para servicios de MEDIA/UPLOAD (MediaService).
    /// - Timeouts muy largos para uploads
    /// - Pocos reintentos (uploads son costosos)
    /// - Bulkhead bajo para proteger ancho de banda
    /// </summary>
    public static ResilienceOptions MediaUpload => new()
    {
        Enabled = true,
        Retry = new RetryOptions
        {
            MaxRetries = 1,
            DelaySeconds = 3,
            UseExponentialBackoff = false,
            UseJitter = true,
            RetryStatusCodes = new[] { 408, 502, 503, 504 }
        },
        CircuitBreaker = new CircuitBreakerOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 5,
            SamplingDurationSeconds = 30,
            BreakDurationSeconds = 30
        },
        Timeout = new TimeoutOptions
        {
            TimeoutSeconds = 120,
            TotalTimeoutSeconds = 300
        },
        Bulkhead = new BulkheadOptions
        {
            Enabled = true,
            MaxParallelization = 10,
            MaxQueuingActions = 20
        },
        Fallback = new FallbackOptions
        {
            Enabled = false
        }
    };

    /// <summary>
    /// Perfil para llamadas entre servicios internos (Audit, Idempotency, Error).
    /// - Fire-and-forget optimizado
    /// - Circuit breaker tolerante (no bloquear el caller)
    /// - Timeouts cortos
    /// </summary>
    public static ResilienceOptions InternalService => new()
    {
        Enabled = true,
        Retry = new RetryOptions
        {
            MaxRetries = 2,
            DelaySeconds = 1,
            UseExponentialBackoff = true,
            UseJitter = true,
            RetryStatusCodes = new[] { 408, 429, 502, 503, 504 }
        },
        CircuitBreaker = new CircuitBreakerOptions
        {
            FailureRatio = 0.7, // Muy tolerante — no bloquear al caller
            MinimumThroughput = 15,
            SamplingDurationSeconds = 60,
            BreakDurationSeconds = 15
        },
        Timeout = new TimeoutOptions
        {
            TimeoutSeconds = 5,
            TotalTimeoutSeconds = 15
        },
        Bulkhead = new BulkheadOptions
        {
            Enabled = true,
            MaxParallelization = 30,
            MaxQueuingActions = 60
        },
        Fallback = new FallbackOptions
        {
            Enabled = true,
            UseCachedResponse = false,
            DefaultMessage = "Internal service temporarily unavailable — operation queued."
        }
    };

    /// <summary>
    /// Obtiene el perfil por nombre
    /// </summary>
    public static ResilienceOptions GetProfile(string profileName) => profileName.ToLowerInvariant() switch
    {
        "critical" => Critical,
        "standard" => Standard,
        "background" => Background,
        "media" or "mediaupload" => MediaUpload,
        "internal" or "internalservice" => InternalService,
        _ => Standard
    };
}
