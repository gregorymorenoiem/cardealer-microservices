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
using Serilog.Enrichers.Span;
using CarDealer.Shared.Database;
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
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

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

// Configurar autenticación JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured");

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.FromMinutes(5)
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

// Application Services
builder.Services.AddScoped<IErrorLogRepository, EfErrorLogRepository>();
builder.Services.AddScoped<IErrorReporter, ErrorReporter>();

// Métricas personalizadas (Singleton para compartir estado)
builder.Services.AddSingleton<ErrorService.Application.Metrics.ErrorServiceMetrics>();

// Dead Letter Queue para eventos fallidos (Singleton, en memoria)
builder.Services.AddSingleton<ErrorService.Infrastructure.Messaging.IDeadLetterQueue>(sp =>
    new ErrorService.Infrastructure.Messaging.InMemoryDeadLetterQueue(maxRetries: 5));

// Event Publisher for RabbitMQ (con DLQ integrado)
builder.Services.AddSingleton<ErrorService.Infrastructure.Messaging.RabbitMqEventPublisher>();
builder.Services.AddSingleton<IEventPublisher>(sp =>
    sp.GetRequiredService<ErrorService.Infrastructure.Messaging.RabbitMqEventPublisher>());

// Background Service para procesar DLQ
builder.Services.AddHostedService<ErrorService.Infrastructure.Messaging.DeadLetterQueueProcessor>();

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

// Configurar Rate Limiting
var rateLimitingConfig = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingConfiguration>()
    ?? new RateLimitingConfiguration();
builder.Services.AddCustomRateLimiting(rateLimitingConfig);

// Configurar RabbitMQ
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<ErrorServiceRabbitMQSettings>(builder.Configuration.GetSection("ErrorService"));

// Registrar el consumidor RabbitMQ como hosted service
builder.Services.AddHostedService<RabbitMQErrorConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware para bypass de Rate Limiting (debe estar antes del middleware de rate limiting)
app.UseMiddleware<RateLimitBypassMiddleware>();

// Middleware para Rate Limiting (debe estar antes de otros middlewares)
app.UseCustomRateLimiting(rateLimitingConfig);

app.UseHttpsRedirection();

// Agregar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Middleware para capturar respuestas
app.UseMiddleware<ResponseCaptureMiddleware>();

// Middleware para manejo de errores
app.UseErrorHandling();

app.MapControllers();

// Health check endpoint - acceso anónimo
app.MapGet("/health", [AllowAnonymous] () => Results.Ok(new
{
    service = "ErrorService",
    status = "healthy",
    timestamp = DateTime.UtcNow
}));

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
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

Log.Information("ErrorService starting up...");
app.Run();

// Make Program class accessible for integration testing
public partial class Program { }