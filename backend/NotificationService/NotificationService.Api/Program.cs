using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Extensions;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Providers;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Domain.Interfaces;
using Serilog;
using Serilog.Enrichers.Span;
using System.Reflection;
using FluentValidation;
using NotificationService.Shared;
using CarDealer.Shared.Database;
using CarDealer.Shared.Secrets;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using NotificationService.Infrastructure.BackgroundServices;
using NotificationService.Infrastructure.Metrics;
using Polly;
using Polly.CircuitBreaker;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using NotificationService.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CarDealer.Shared.ErrorHandling.Extensions;
using CarDealer.Shared.Audit.Extensions;

const string ServiceName = "NotificationService";
const string ServiceVersion = "1.0.0";

// Configurar Serilog con TraceId/SpanId enrichment
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging();

// Simple Health Check
builder.Services.AddHealthChecks();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "http://localhost:5173" };

        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ✅ USAR DEPENDENCY INJECTION DE INFRASTRUCTURE (INCLUYE RABBITMQ)
builder.Services.AddInfrastructure(builder.Configuration);

// ============= TRANSVERSAL SERVICES =============
// Error Handling (→ ErrorService)
builder.Services.AddStandardErrorHandling(builder.Configuration, ServiceName);

// Audit (→ AuditService via RabbitMQ)
builder.Services.AddAuditPublisher(builder.Configuration);

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

// 🔧 Register Teams Provider
builder.Services.AddHttpClient<ITeamsProvider, TeamsProvider>();

// 🔧 Register RabbitMQ Consumers as Hosted Services
builder.Services.AddHostedService<ErrorCriticalEventConsumer>();
builder.Services.AddHostedService<UserRegisteredNotificationConsumer>();
builder.Services.AddHostedService<VehicleCreatedNotificationConsumer>();
builder.Services.AddHostedService<PaymentReceiptNotificationConsumer>();

// Dead Letter Queue
builder.Services.AddSingleton<IDeadLetterQueue, InMemoryDeadLetterQueue>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<InMemoryDeadLetterQueue>>();
    return new InMemoryDeadLetterQueue(logger, maxRetries: 5);
});
builder.Services.AddHostedService<DeadLetterQueueProcessor>();

// Metrics
builder.Services.AddSingleton<NotificationServiceMetrics>();

// Polly 8.x Circuit Breaker
builder.Services.AddResiliencePipeline("notification-circuit-breaker", pipelineBuilder =>
{
    pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        MinimumThroughput = 10,
        BreakDuration = TimeSpan.FromSeconds(30)
    });
});

// OpenTelemetry
var serviceName = "NotificationService";
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName, serviceVersion))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(serviceName);

        if (builder.Environment.IsDevelopment())
        {
            tracing.AddConsoleExporter();
        }
        else
        {
            tracing.AddOtlpExporter();
            tracing.SetSampler(new TraceIdRatioBasedSampler(0.1)); // 10% sampling in production
        }
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter(serviceName);

        if (builder.Environment.IsDevelopment())
        {
            metrics.AddConsoleExporter();
        }
        else
        {
            metrics.AddOtlpExporter();
        }
    });

// Database Context (multi-provider configuration)
builder.Services.AddDatabaseProvider<ApplicationDbContext>(builder.Configuration);

// MediatR - Cargar assemblies de Application
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("NotificationService.Application")));

// FluentValidation - Validators
builder.Services.AddValidatorsFromAssembly(Assembly.Load("NotificationService.Application"));

// Configure settings
builder.Services.Configure<NotificationSettings>(
    builder.Configuration.GetSection("NotificationSettings"));

// ========== JWT AUTHENTICATION ==========
var jwtKey = builder.Configuration["Jwt:Key"] ?? "clave-super-secreta-desarrollo-32-caracteres-aaa";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AuthService-Dev";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CarGurus-Dev";

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

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("NotificationServiceRead", policy =>
    {
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("NotificationServiceAdmin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("role", "admin", "notification-admin");
    });
});

var app = builder.Build();

// Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations for NotificationService...");
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Global Error Handling (first in pipeline)
app.UseGlobalErrorHandling();

// Audit middleware
app.UseAuditMiddleware();

// Enable CORS
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// Service Discovery Auto-Registration
app.UseMiddleware<ServiceRegistrationMiddleware>();

// Health check endpoint
app.MapGet("/health", () => Results.Ok("NotificationService is healthy"));

app.MapControllers();

Log.Information("NotificationService starting up with ErrorService middleware and RabbitMQ Consumer...");
app.Run();

// Expose Program class for integration testing
public partial class Program { }