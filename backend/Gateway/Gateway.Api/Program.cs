using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Serilog;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using Gateway.Api.Middleware;
using Gateway.Domain.Interfaces;
using Gateway.Infrastructure.Services;
using Gateway.Application.UseCases;
using CarDealer.Shared.Configuration;
using CarDealer.Shared.RateLimiting.Extensions;
using CarDealer.Shared.RateLimiting.Models;
using CarDealer.Shared.Logging.Extensions;
using CarDealer.Shared.ErrorHandling.Extensions;
using CarDealer.Shared.Observability.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// FASE 2: OBSERVABILITY - Logging centralizado con Serilog + Seq
// ============================================================================
builder.UseStandardSerilog("Gateway", options =>
{
    options.SeqEnabled = true;
    options.SeqServerUrl = builder.Configuration["Logging:Seq:ServerUrl"] ?? "http://seq:5341";
    options.FileEnabled = builder.Configuration.GetValue<bool>("Logging:File:Enabled", false);
    options.FilePath = builder.Configuration["Logging:File:Path"] ?? "logs/gateway-.log";
    options.RabbitMQEnabled = false; // Gateway uses direct Seq, not RabbitMQ for logs
});

// 1. Determinar entorno
var isDevelopment = builder.Environment.IsDevelopment();
var configFile = isDevelopment ? "ocelot.dev.json" : "ocelot.prod.json";
//var configFile = isDevelopment ? "ocelot.dev.json" : "ocelot.dev.json";

// 2. Cargar configuración
builder.Configuration.AddJsonFile(configFile, optional: false, reloadOnChange: true);

// 3. Configuración esencial
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Clean Architecture - Domain Services
builder.Services.AddScoped<IRoutingService, RoutingService>();
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

// Clean Architecture - Application Use Cases
builder.Services.AddScoped<CheckRouteExistsUseCase>();
builder.Services.AddScoped<ResolveDownstreamPathUseCase>();
builder.Services.AddScoped<CheckServiceHealthUseCase>();
builder.Services.AddScoped<GetServicesHealthUseCase>();
builder.Services.AddScoped<RecordRequestMetricsUseCase>();
builder.Services.AddScoped<RecordDownstreamCallMetricsUseCase>();

// ============================================================================
// FASE 2: OBSERVABILITY - OpenTelemetry Tracing + Metrics
// ============================================================================
builder.Services.AddStandardObservability("Gateway", options =>
{
    options.TracingEnabled = true;
    options.MetricsEnabled = true;
    options.OtlpEndpoint = builder.Configuration["Observability:Otlp:Endpoint"] ?? "http://jaeger:4317";
    options.SamplingRatio = builder.Configuration.GetValue<double>("Observability:SamplingRatio", 0.1);
    options.PrometheusEnabled = builder.Configuration.GetValue<bool>("Observability:Prometheus:Enabled", true);
    options.ExcludedPaths = new[] { "/health", "/metrics", "/swagger" };
});

// ============================================================================
// FASE 2: OBSERVABILITY - Error Handling centralizado
// ============================================================================
builder.Services.AddStandardErrorHandling(options =>
{
    options.ServiceName = "Gateway";
    options.Environment = builder.Environment.EnvironmentName;
    options.PublishToErrorService = builder.Configuration.GetValue<bool>("ErrorHandling:PublishToErrorService", true);
    options.RabbitMQHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
    options.RabbitMQPort = builder.Configuration.GetValue<int>("RabbitMQ:Port", 5672);
    options.RabbitMQUser = builder.Configuration["RabbitMQ:User"] ?? "guest";
    options.RabbitMQPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";
    options.IncludeStackTrace = builder.Environment.IsDevelopment();
});

// 4. CORS configurado para múltiples frontends
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        if (isDevelopment)
        {
            // Permitir múltiples puertos de desarrollo
            policy.WithOrigins(
                    "http://localhost:5173",    // Vite default
                    "http://localhost:5174",    // Frontend original
                    "http://localhost:3000",    // React alternative
                    "http://localhost:4200",    // Angular (si aplica)
                    "http://localhost:8080"     // Frontend Docker
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Producción
            policy.WithOrigins(
                    "https://inelcasrl.com.do",
                    "https://www.inelcasrl.com.do",
                    "https://cardealer.app",
                    "https://www.cardealer.app"
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .SetPreflightMaxAge(TimeSpan.FromHours(1));
        }
    });
});

// 4.1 JWT Authentication from externalized secrets
try
{
    var (jwtKey, jwtIssuer, jwtAudience) = MicroserviceSecretsConfiguration.GetJwtConfig(builder.Configuration);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

    Log.Information("JWT Authentication configured successfully for Gateway");
}
catch (InvalidOperationException ex)
{
    Log.Warning("JWT Authentication not configured: {Message}. Routes requiring auth will fail.", ex.Message);
}

// 5. Configuración Ocelot con Polly
builder.Services
    .AddOcelot(builder.Configuration)
    .AddPolly();

// 6. Swagger para Ocelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// 7. Configure Consul Client
var consulAddress = builder.Configuration["Consul:Address"] ?? "http://localhost:8500";
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(config =>
{
    config.Address = new Uri(consulAddress);
}));

// 8. Configure Service Discovery
builder.Services.AddScoped<IServiceRegistry, ConsulServiceRegistry>();
builder.Services.AddScoped<IServiceDiscovery, ConsulServiceDiscovery>();
builder.Services.AddScoped<IHealthChecker, HttpHealthChecker>();

// 9. Configure Rate Limiting
var rateLimitEnabled = builder.Configuration.GetValue<bool>("RateLimiting:Enabled", true);
if (rateLimitEnabled)
{
    builder.Services.AddRateLimiting(options =>
    {
        options.Enabled = true;
        options.RedisConnection = builder.Configuration["Redis:Connection"] ?? "localhost:6379";
        options.DefaultLimit = builder.Configuration.GetValue<int>("RateLimiting:DefaultLimit", 100);
        options.DefaultWindowSeconds = builder.Configuration.GetValue<int>("RateLimiting:DefaultWindowSeconds", 60);
        options.KeyPrefix = "gateway:ratelimit";
        options.ExcludedPaths = new List<string>
        {
            "/health",
            "/swagger",
            "/metrics",
            "/.well-known"
        };
        
        // Add default API policies
        options.AddDefaultApiPolicies();
        
        Log.Information("Rate Limiting configured with Redis: {RedisConnection}", options.RedisConnection);
    });
}
else
{
    Log.Warning("Rate Limiting is DISABLED. API is unprotected against abuse.");
}

var app = builder.Build();

// ============================================================================
// MIDDLEWARE PIPELINE
// ============================================================================

// FASE 2: Global Error Handling - PRIMERO para capturar todas las excepciones
app.UseGlobalErrorHandling();

// FASE 2: Request Logging con enrichment de TraceId, UserId, CorrelationId
app.UseRequestLogging();

// CORS debe ir primero antes de cualquier otro middleware de routing
app.UseCors("ReactPolicy");

// Rate Limiting middleware (before all other processing)
if (rateLimitEnabled)
{
    app.UseRateLimiting();
    Log.Information("Rate Limiting middleware enabled");
}

// Health check middleware BEFORE Ocelot to intercept /health requests
app.UseHealthCheckMiddleware();

// Use routing for other endpoints
app.UseRouting();

// Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Solo usar Swagger si no estamos en Testing
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerForOcelotUI();
}

// Service Discovery Registration
app.UseMiddleware<ServiceRegistrationMiddleware>();

// 10. Ocelot como último middleware
await app.UseOcelot();

app.Run();

// Make Program class accessible for testing
public partial class Program { }