using Microsoft.EntityFrameworkCore;
using AuthService.Infrastructure.Extensions;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Messaging;
using AuthService.Infrastructure.BackgroundServices;
using AuthService.Infrastructure.Metrics;
using AuthService.Domain.Interfaces;
using Serilog;
using Serilog.Enrichers.Span;
using System.Reflection;
using FluentValidation;
using AuthService.Shared;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Cors;
using System.Threading.RateLimiting;
using AuthService.Infrastructure.Services.Messaging;
using AuthService.Infrastructure.Middleware;
using AuthService.Domain.Interfaces.Services;
using AuthService.Infrastructure.External;
using AuthService.Infrastructure.Services.Notification;
using Microsoft.Extensions.Options;
using CarDealer.Shared.Database;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using AuthService.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog con TraceId/SpanId enrichment
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan() // ✅ NUEVO: Correlación con OpenTelemetry traces
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddLogging();

// OpenTelemetry con Sampling Strategy
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddSource("AuthService")
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("AuthService"))
            .SetSampler(new TraceIdRatioBasedSampler(
                builder.Environment.IsProduction() ? 0.1 : 1.0)) // ✅ NUEVO: 10% prod, 100% dev
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Exporter:Otlp:Endpoint"] ?? "http://localhost:4317");
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter("AuthService")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Exporter:Otlp:Endpoint"] ?? "http://localhost:4317");
            });
    });

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthPolicy", limiterOptions =>
    {
        var rateLimitSettings = builder.Configuration.GetSection("Security:RateLimit").Get<RateLimitSettings>();
        limiterOptions.PermitLimit = rateLimitSettings?.RequestsPerMinute ?? 60;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});

// CORS 
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new string[0];
        var allowedMethods = builder.Configuration.GetSection("Cors:AllowedMethods").Get<string[]>() ?? new string[0];
        var allowedHeaders = builder.Configuration.GetSection("Cors:AllowedHeaders").Get<string[]>() ?? new string[0];
        var allowCredentials = builder.Configuration.GetValue<bool>("Cors:AllowCredentials");

        policy.WithOrigins(allowedOrigins)
              .WithMethods(allowedMethods)
              .WithHeaders(allowedHeaders);

        if (allowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});

// TODA LA CONFIGURACIÓN EN UN SOLO LUGAR
builder.Services.AddInfrastructure(builder.Configuration);

// Service Discovery Configuration - with fallback to NoOp when Consul is disabled
var consulEnabled = builder.Configuration.GetValue<bool>("Consul:Enabled", false);
if (consulEnabled)
{
    builder.Services.AddSingleton<IConsulClient>(sp =>
    {
        var consulAddress = builder.Configuration["Consul:Address"] ?? "http://localhost:8500";
        return new ConsulClient(config => config.Address = new Uri(consulAddress));
    });

    builder.Services.AddScoped<IServiceRegistry, ConsulServiceRegistry>();
    builder.Services.AddScoped<IServiceDiscovery, ConsulServiceDiscovery>();
    builder.Services.AddHttpClient("HealthCheck");
    builder.Services.AddScoped<IHealthChecker, HttpHealthChecker>();
}
else
{
    // Use NoOp implementations when Consul is disabled
    builder.Services.AddScoped<IServiceRegistry, NoOpServiceRegistry>();
    builder.Services.AddScoped<IServiceDiscovery, NoOpServiceDiscovery>();
}

// Database Context (multi-provider configuration)
builder.Services.AddDatabaseProvider<ApplicationDbContext>(builder.Configuration);

// ✅ NUEVO: Custom Metrics
builder.Services.AddSingleton<AuthServiceMetrics>();

// MediatR & FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("AuthService.Application")));
builder.Services.AddValidatorsFromAssembly(Assembly.Load("AuthService.Application"));

// Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<NotificationServiceSettings>(builder.Configuration.GetSection("NotificationService"));

// RabbitMQ Configuration - Conditional based on Enabled flag
var rabbitMqEnabled = builder.Configuration.GetValue<bool>("RabbitMQ:Enabled", false);
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<ErrorServiceRabbitMQSettings>(builder.Configuration.GetSection("ErrorService"));
builder.Services.Configure<NotificationServiceRabbitMQSettings>(builder.Configuration.GetSection("NotificationService"));

if (rabbitMqEnabled)
{
    // RabbitMQ enabled - use real implementations with DLQ
    builder.Services.AddSingleton<IDeadLetterQueue, InMemoryDeadLetterQueue>();
    builder.Services.AddHostedService<DeadLetterQueueProcessor>();
    builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
    builder.Services.AddSingleton<IErrorEventProducer, RabbitMQErrorProducer>();
    builder.Services.AddSingleton<INotificationEventProducer, RabbitMQNotificationProducer>();
    Log.Information("RabbitMQ ENABLED - Using real messaging implementations");
}
else
{
    // RabbitMQ disabled - use NoOp implementations
    builder.Services.AddSingleton<IDeadLetterQueue, InMemoryDeadLetterQueue>(); // Still needed for DI
    builder.Services.AddSingleton<IEventPublisher, NoOpEventPublisher>();
    builder.Services.AddSingleton<IErrorEventProducer, NoOpErrorProducer>();
    builder.Services.AddSingleton<INotificationEventProducer, NoOpNotificationProducer>();
    Log.Information("RabbitMQ DISABLED - Using NoOp messaging implementations");
}

// Registrar AuthNotificationService con el constructor CORRECTO (5 parámetros)
builder.Services.AddScoped<IAuthNotificationService>(provider =>
{
    var notificationClient = provider.GetRequiredService<NotificationServiceClient>();
    var notificationProducer = provider.GetRequiredService<INotificationEventProducer>();
    var settings = provider.GetRequiredService<IOptions<NotificationServiceSettings>>();
    var rabbitMqSettings = provider.GetRequiredService<IOptions<NotificationServiceRabbitMQSettings>>();
    var logger = provider.GetRequiredService<ILogger<AuthNotificationService>>();

    return new AuthNotificationService(
        notificationClient,
        notificationProducer,
        settings,
        rabbitMqSettings,
        logger
    );
});

// 🚨 CONSTRUIR LA APLICACIÓN
var app = builder.Build();

// Migraciones (solo para proveedores relacionales, no InMemory)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var authContext = services.GetRequiredService<ApplicationDbContext>();

        // Check if we should auto-migrate (disabled for InMemory testing)
        var configuration = services.GetRequiredService<IConfiguration>();
        var autoMigrate = configuration.GetValue<bool>("Database:AutoMigrate", true);

        // Only migrate if we're using a relational provider and auto-migrate is enabled
        if (autoMigrate && authContext.Database.IsRelational())
        {
            authContext.Database.Migrate();
            Log.Information("AuthService database migrations applied successfully.");
        }
        else if (!authContext.Database.IsRelational())
        {
            authContext.Database.EnsureCreated();
            Log.Information("AuthService using non-relational database (InMemory), EnsureCreated called.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error during database migration");
    }
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(); // CORS debe ir primero
app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Service Discovery Auto-Registration
app.UseMiddleware<ServiceRegistrationMiddleware>();

// Health Checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapControllers();

Log.Information("AuthService starting up...");
app.Run();

public partial class Program { }
