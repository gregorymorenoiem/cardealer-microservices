using RoleService.Domain.Interfaces;
using RoleService.Infrastructure.Messaging;
using RoleService.Infrastructure.Persistence;
// using RoleService.Infrastructure.Services;
// using RoleService.Infrastructure.Services.Messaging;
using RoleService.Shared.Extensions;
using RoleService.Shared.Middleware;
using RoleService.Shared.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Enrichers.Span;
using CarDealer.Shared.Database;
using CarDealer.Shared.Secrets;
using CarDealer.Shared.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using FluentValidation;
using RoleService.Application.Behaviors;
using MediatR;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using RoleService.Api.Middleware;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Diagnostics;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add Secret Provider for externalized secrets
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

// Configurar Swagger con soporte JWT
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RoleService API",
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

// Configurar autenticación JWT con secretos externalizados
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
    // Política para acceso general al RoleService
    options.AddPolicy("RoleServiceAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("service", "RoleService", "all");
    });

    // Política para operaciones administrativas
    options.AddPolicy("RoleServiceAdmin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("role", "admin", "RoleService-admin");
    });

    // Política para solo lectura
    options.AddPolicy("RoleServiceRead", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

// Database Context (multi-provider configuration)
builder.Services.AddDatabaseProvider<ApplicationDbContext>(builder.Configuration);

// Service Discovery Configuration
builder.Services.AddSingleton<IConsulClient>(sp =>
{
    var consulAddress = builder.Configuration["Consul:Address"] ?? "http://localhost:8500";
    return new ConsulClient(config => config.Address = new Uri(consulAddress));
});

builder.Services.AddScoped<IServiceRegistry, ConsulServiceRegistry>();
builder.Services.AddScoped<IServiceDiscovery, ConsulServiceDiscovery>();
builder.Services.AddHttpClient("HealthCheck");
builder.Services.AddScoped<IHealthChecker, HttpHealthChecker>();

// Application Services - Repositories
builder.Services.AddScoped<IRoleRepository, RoleService.Infrastructure.Repositories.EfRoleRepository>();
builder.Services.AddScoped<IPermissionRepository, RoleService.Infrastructure.Repositories.EfPermissionRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RoleService.Infrastructure.Repositories.EfRolePermissionRepository>();
builder.Services.AddScoped<IRoleLogRepository, RoleService.Infrastructure.Persistence.EfRoleLogRepository>();

// User Context Service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<RoleService.Application.Interfaces.IUserContextService, RoleService.Infrastructure.Services.UserContextService>();

// External Service Clients
builder.Services.AddHttpClient<RoleService.Application.Interfaces.IAuditServiceClient, RoleService.Infrastructure.External.AuditServiceClient>(client =>
{
    var auditServiceUrl = builder.Configuration["ServiceUrls:AuditService"] ?? "https://localhost:7287";
    client.BaseAddress = new Uri(auditServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<RoleService.Application.Interfaces.INotificationServiceClient, RoleService.Infrastructure.External.NotificationServiceClient>(client =>
{
    var notificationServiceUrl = builder.Configuration["ServiceUrls:NotificationService"] ?? "https://localhost:45954";
    client.BaseAddress = new Uri(notificationServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<RoleService.Application.Interfaces.IErrorServiceClient, RoleService.Infrastructure.External.ErrorServiceClient>(client =>
{
    var errorServiceUrl = builder.Configuration["ServiceUrls:ErrorService"] ?? "https://localhost:45952";
    client.BaseAddress = new Uri(errorServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Other Services
// builder.Services.AddScoped<IRoleReporter, RoleReporter>();

// Métricas personalizadas (Singleton para compartir estado)
builder.Services.AddSingleton<RoleService.Application.Metrics.RoleServiceMetrics>();

// Dead Letter Queue para eventos fallidos (Singleton, en memoria)
// Comentado temporalmente - RabbitMQ no está corriendo
// builder.Services.AddSingleton<RoleService.Infrastructure.Messaging.IDeadLetterQueue>(sp =>
//     new RoleService.Infrastructure.Messaging.InMemoryDeadLetterQueue(maxRetries: 5));

// Event Publisher for RabbitMQ (con DLQ integrado)
// builder.Services.AddSingleton<RoleService.Infrastructure.Messaging.RabbitMqEventPublisher>();
// builder.Services.AddSingleton<IEventPublisher>(sp =>
//     sp.GetRequiredService<RoleService.Infrastructure.Messaging.RabbitMqEventPublisher>());

// Background Service para procesar DLQ
// builder.Services.AddHostedService<RoleService.Infrastructure.Messaging.DeadLetterQueueProcessor>();

// Agregar MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(RoleService.Application.UseCases.Roles.CreateRole.CreateRoleCommand).Assembly));

// Registrar FluentValidation
builder.Services.AddValidatorsFromAssembly(
    typeof(RoleService.Application.UseCases.Roles.CreateRole.CreateRoleCommandValidator).Assembly);

// Agregar behavior de validación para MediatR
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Configurar OpenTelemetry
var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "RoleService";
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
        .AddSource("RoleService.*")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter("RoleService.*")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(otlpEndpoint);
        }));

// Configurar el manejo de errores
builder.Services.AddErrorHandling("RoleService");

// Configurar Rate Limiting
var rateLimitingConfig = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingConfiguration>()
    ?? new RateLimitingConfiguration();
builder.Services.AddCustomRateLimiting(rateLimitingConfig);

// Configurar RabbitMQ
// builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
// builder.Services.Configure<RoleServiceRabbitMQSettings>(builder.Configuration.GetSection("RoleService"));

// Registrar el consumidor RabbitMQ como hosted service
// builder.Services.AddHostedService<RabbitMQErrorConsumer>();

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

// Service Discovery Auto-Registration
app.UseMiddleware<ServiceRegistrationMiddleware>();

// Middleware para capturar respuestas
app.UseMiddleware<ResponseCaptureMiddleware>();

// Middleware para manejo de errores
app.UseErrorHandling();

app.MapControllers();

// Health check endpoint - acceso anónimo
app.MapGet("/health", [AllowAnonymous] () => Results.Ok(new
{
    service = "RoleService",
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

Log.Information("RoleService starting up...");
app.Run();

// Make Program class accessible for integration testing
public partial class Program { }
