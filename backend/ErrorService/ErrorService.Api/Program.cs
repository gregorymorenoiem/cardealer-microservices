using ErrorService.Domain.Interfaces;
using ErrorService.Infrastructure.Messaging;
using ErrorService.Infrastructure.Persistence;
using ErrorService.Infrastructure.Services;
using ErrorService.Infrastructure.Services.Messaging;
using ErrorService.Shared.Extensions;
using ErrorService.Shared.Middleware;
using ErrorService.Shared.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using CarDealer.Shared.Middleware;
using CarDealer.Shared.Logging.Extensions;
using CarDealer.Shared.ErrorHandling.Extensions;
using Serilog.Enrichers.Span;
using CarDealer.Shared.Database;
using CarDealer.Shared.Secrets;
using CarDealer.Shared.Configuration;
using CarDealer.Shared.Audit.Extensions;
using CarDealer.Shared.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using FluentValidation;
using ErrorService.Application.Behaviors;
using MediatR;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using ErrorService.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Secret Provider for externalized configuration
builder.Services.AddSecretProvider();

// Configurar Serilog con enriquecimiento de TraceId
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan() // Agregar TraceId, SpanId de OpenTelemetry
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j} TraceId={TraceId} SpanId={SpanId}{NewLine}{Exception}")
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "https://okla.com.do" };

        policy.WithOrigins(allowedOrigins)
              // Security: Restrict to specific HTTP methods and headers (OWASP)
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization", "X-CSRF-Token", "X-Requested-With", "X-Idempotency-Key")
              .AllowCredentials();
    });
});

// Configurar Swagger con soporte JWT
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ErrorService API",
        Version = "v1",
        Description = "API para gestión centralizada de errores en CarDealer Microservices"
    });

    // Configurar autenticación JWT en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT en el formato: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configurar autenticación JWT - using secrets
var (jwtKey, jwtIssuer, jwtAudience) = MicroserviceSecretsConfiguration.GetJwtConfig(builder.Configuration);
var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = jwtSettings.GetValue<bool>("ValidateIssuer", true),
        ValidateAudience = jwtSettings.GetValue<bool>("ValidateAudience", true),
        ValidateLifetime = jwtSettings.GetValue<bool>("ValidateLifetime", true),
        ValidateIssuerSigningKey = jwtSettings.GetValue<bool>("ValidateIssuerSigningKey", true),
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // No tolerance — tokens expire exactly at exp claim
    };

    // Logging para debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Log.Debug("JWT Token validated for user: {User}",
                context.Principal?.Identity?.Name ?? "Unknown");
            return Task.CompletedTask;
        }
    };
});

// Configurar políticas de autorización
builder.Services.AddAuthorization(options =>
{
    // Política para acceso general al ErrorService
    options.AddPolicy("ErrorServiceAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("service", "errorservice", "all");
    });

    // Política para operaciones administrativas
    options.AddPolicy("ErrorServiceAdmin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("role", "admin", "errorservice-admin");
    });

    // Política para solo lectura
    options.AddPolicy("ErrorServiceRead", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

// Database Context (multi-provider configuration)
builder.Services.AddDatabaseProvider<ApplicationDbContext>(builder.Configuration);

// ========== SERVICE DISCOVERY ==========

// Consul Client
builder.Services.AddSingleton<IConsulClient>(sp => new ConsulClient(config =>
{
    config.Address = new Uri(builder.Configuration["Consul:Address"] ?? "http://localhost:8500");
}));

// Service Discovery Services
builder.Services.AddScoped<IServiceRegistry, ConsulServiceRegistry>();
builder.Services.AddScoped<IServiceDiscovery, ConsulServiceDiscovery>();
builder.Services.AddHttpClient("HealthCheck");
builder.Services.AddScoped<IHealthChecker, HttpHealthChecker>();

// ========================================

// Application Services
builder.Services.AddScoped<IErrorLogRepository, EfErrorLogRepository>();
builder.Services.AddScoped<IErrorReporter, ErrorReporter>();

// Métricas personalizadas (Singleton para compartir estado)
builder.Services.AddSingleton<ErrorService.Application.Metrics.ErrorServiceMetrics>();

// Dead Letter Queue para eventos fallidos (PostgreSQL-backed, survives pod restarts)
builder.Services.AddPostgreSqlDeadLetterQueue(builder.Configuration, "ErrorService");

// Shared RabbitMQ connection (1 connection per pod instead of N per class)
builder.Services.AddSharedRabbitMqConnection(builder.Configuration);

// RabbitMQ Configuration - Conditional based on Enabled flag
var rabbitMqEnabled = builder.Configuration.GetValue<bool>("RabbitMQ:Enabled", false);

if (rabbitMqEnabled)
{
    // RabbitMQ enabled - use real implementations with DLQ
    Log.Information("🐰 RabbitMQ ENABLED - Using real messaging implementations");
    builder.Services.AddSingleton<ErrorService.Infrastructure.Messaging.RabbitMqEventPublisher>();
    builder.Services.AddSingleton<IEventPublisher>(sp =>
        sp.GetRequiredService<ErrorService.Infrastructure.Messaging.RabbitMqEventPublisher>());
    
    // Background Service para procesar DLQ
    builder.Services.AddHostedService<ErrorService.Infrastructure.Messaging.DeadLetterQueueProcessor>();
}
else
{
    // RabbitMQ disabled - use NoOp implementation
    Log.Information("🚫 RabbitMQ DISABLED - Using NoOp messaging implementation (events will be logged only)");
    builder.Services.AddSingleton<IEventPublisher, ErrorService.Infrastructure.Messaging.NoOpEventPublisher>();
}

// Agregar MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ErrorService.Application.UseCases.LogError.LogErrorCommand).Assembly));

// Registrar FluentValidation
builder.Services.AddValidatorsFromAssembly(
    typeof(ErrorService.Application.UseCases.LogError.LogErrorCommandValidator).Assembly);

// Agregar behavior de validación para MediatR
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Configurar OpenTelemetry
var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "ErrorService";
var serviceVersion = builder.Configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";
var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName,
            ["service.namespace"] = "cardealer"
        }))
    .WithTracing(tracing => tracing
        .SetSampler(new ParentBasedSampler(
            // Estrategia de muestreo basada en ratio
            // En producción: captura 10% de traces normales, 100% de errores
            new TraceIdRatioBasedSampler(
                builder.Environment.IsProduction() ? 0.1 : 1.0))) // Dev: 100%, Prod: 10%
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = context =>
            {
                // Filtrar health checks para reducir ruido
                return !context.Request.Path.StartsWithSegments("/health");
            };
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddSource("ErrorService.*")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter("ErrorService.*")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
        }));

// Configurar el manejo de errores
builder.Services.AddErrorHandling("ErrorService");

// Configurar Audit Publisher
builder.Services.AddAuditPublisher(builder.Configuration);

// Configurar Rate Limiting
var rateLimitingConfig = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingConfiguration>()
    ?? new RateLimitingConfiguration();
builder.Services.AddCustomRateLimiting(rateLimitingConfig);

// Configurar RabbitMQ
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<ErrorServiceRabbitMQSettings>(builder.Configuration.GetSection("ErrorService"));

// Registrar el consumidor RabbitMQ como hosted service (solo si RabbitMQ está habilitado)
if (rabbitMqEnabled)
{
    builder.Services.AddHostedService<RabbitMQErrorConsumer>();
    Log.Information("🐰 RabbitMQErrorConsumer registered as hosted service");
}
else
{
    Log.Information("🚫 RabbitMQErrorConsumer NOT registered - RabbitMQ is disabled");
}

var app = builder.Build();

// ============= MIDDLEWARE PIPELINE (Canonical Order — Microsoft/OWASP) =============
// 1. Global error handling — ALWAYS FIRST (shared library)
app.UseGlobalErrorHandling();

// 2. Request Logging (shared library)
app.UseRequestLogging();

// 3. Security Headers (OWASP) — early in pipeline
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

// 4. Rate Limiting bypass + enforcement
app.UseMiddleware<RateLimitBypassMiddleware>();
app.UseCustomRateLimiting(rateLimitingConfig);

// 5. HTTPS Redirection — only outside K8s (TLS terminates at Ingress in production)
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// 6. Swagger — development only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 7. CORS — before auth
app.UseCors();

// 8. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 9. Audit middleware — after auth (has userId context)
app.UseAuditMiddleware();

// 10. Response capture middleware
app.UseMiddleware<ResponseCaptureMiddleware>();

// 11. Service Discovery Auto-Registration
app.UseMiddleware<ServiceRegistrationMiddleware>();

// 12. Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

// Apply migrations conditionally (disabled in production to avoid race conditions with HPA replicas)
var autoMigrate = app.Configuration.GetValue<bool>("Database:AutoMigrate", true);
if (autoMigrate)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Log.Information("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying database migrations.");
    }
}
else
{
    Log.Information("Database auto-migration disabled for ErrorService. Run migrations via CI/CD pipeline.");
}

Log.Information("ErrorService starting up...");
app.Run();

// Make Program class accessible for integration testing
public partial class Program { }

