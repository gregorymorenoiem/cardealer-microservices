using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Extensions;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Providers;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Domain.Interfaces;
using Serilog;
using CarDealer.Shared.Logging.Extensions;
using CarDealer.Shared.Middleware;
using CarDealer.Shared.Messaging;
using CarDealer.Shared.Configuration;
using System.Reflection;
using FluentValidation;
using NotificationService.Shared;
using CarDealer.Shared.Database;
using CarDealer.Shared.Secrets;

using NotificationService.Infrastructure.BackgroundServices;
using NotificationService.Infrastructure.Metrics;
using Polly;
using Polly.CircuitBreaker;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using NotificationService.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CarDealer.Shared.ErrorHandling.Extensions;
using CarDealer.Shared.Observability.Extensions;
using CarDealer.Shared.Audit.Extensions;

const string ServiceName = "NotificationService";

// Bootstrap logger using shared library
Log.Logger = SerilogExtensions.CreateBootstrapLogger(ServiceName);

var builder = WebApplication.CreateBuilder(args);

// ============= CENTRALIZED LOGGING (Serilog → Seq) =============
builder.UseStandardSerilog(ServiceName);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddLogging();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "http://localhost:5173" };

        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              // Security: Restrict to specific headers (OWASP)
              .WithHeaders("Content-Type", "Authorization", "X-CSRF-Token", "X-Requested-With", "X-Idempotency-Key")
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

// 🔧 Register RabbitMQ Consumers as Hosted Services
builder.Services.AddHostedService<ErrorCriticalEventConsumer>();
builder.Services.AddHostedService<UserRegisteredNotificationConsumer>();
builder.Services.AddHostedService<VehicleCreatedNotificationConsumer>();
builder.Services.AddHostedService<PaymentReceiptNotificationConsumer>();

// Dead Letter Queue — PostgreSQL-backed (survives pod restarts during auto-scaling)
builder.Services.AddPostgreSqlDeadLetterQueue(builder.Configuration, "NotificationService");
builder.Services.AddHostedService<DeadLetterQueueProcessor>();

// Shared RabbitMQ connection (1 connection per pod instead of N per class)
builder.Services.AddSharedRabbitMqConnection(builder.Configuration);

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

// ============= OBSERVABILITY (OpenTelemetry → shared library) =============
builder.Services.AddStandardObservability(builder.Configuration, ServiceName, "1.0.0");

// Database Context (multi-provider configuration)
builder.Services.AddDatabaseProvider<ApplicationDbContext>(builder.Configuration);

// MediatR - Cargar assemblies de Application
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("NotificationService.Application")));

// FluentValidation - Validators
builder.Services.AddValidatorsFromAssembly(Assembly.Load("NotificationService.Application"));

// ValidationBehavior — ensures FluentValidation validators (NoSqlInjection, NoXss) run automatically in MediatR pipeline
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(NotificationService.Application.Behaviors.ValidationBehavior<,>));

// Configure settings
builder.Services.Configure<NotificationSettings>(
    builder.Configuration.GetSection("NotificationSettings"));

// ========== JWT AUTHENTICATION (from centralized secrets, NOT hardcoded) ==========
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
            ClockSkew = TimeSpan.Zero // No tolerance — tokens expire exactly at exp claim
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

    Log.Information("JWT Authentication configured successfully for {ServiceName}", ServiceName);
}
catch (InvalidOperationException ex)
{
    Log.Fatal("JWT Authentication FAILED to configure for {ServiceName}: {Message}. Service will NOT start without proper JWT configuration.", ServiceName, ex.Message);
    throw; // Fail fast — do NOT start without auth (NIST IA-5)
}

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

// ============= RATE LIMITING =============
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 60;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });
    options.OnRejected = async (context, ct) =>
    {
        Log.Warning("Rate limit exceeded for {RemoteIp} on {Path}",
            context.HttpContext.Connection.RemoteIpAddress,
            context.HttpContext.Request.Path);
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", ct);
    };
});

// Audit Service Client for centralized audit logging
builder.Services.AddHttpClient<NotificationService.Application.Interfaces.IAuditServiceClient, NotificationService.Infrastructure.External.AuditServiceClient>(client =>
{
    var auditServiceUrl = builder.Configuration["ServiceUrls:AuditService"] ?? "http://auditservice:8080";
    client.BaseAddress = new Uri(auditServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Apply database migrations conditionally (disabled in production to avoid race conditions with HPA replicas)
var autoMigrate = app.Configuration.GetValue<bool>("Database:AutoMigrate", true);
if (autoMigrate)
{
    using var scope = app.Services.CreateScope();
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
else
{
    Log.Information("Database auto-migration disabled for {ServiceName}. Run migrations via CI/CD pipeline.", ServiceName);
}

// ============= MIDDLEWARE PIPELINE (Canonical Order — Microsoft/OWASP) =============
// 1. Global Error Handling — ALWAYS FIRST
app.UseGlobalErrorHandling();

// 2. Security Headers (OWASP) — early in pipeline
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

// 3. Request Logging
app.UseRequestLogging();

// 4. HTTPS Redirection — only outside K8s (TLS terminates at Ingress in production)
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// 5. Swagger — development only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 6. CORS — before auth
app.UseCors();

// 7. Rate Limiting
app.UseRateLimiter();

// 8. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 7. Audit middleware — after auth (has userId context)
app.UseAuditMiddleware();

// 8. Service Discovery Auto-Registration
app.UseMiddleware<ServiceRegistrationMiddleware>();

// 9. Endpoints
app.MapHealthChecks("/health");
app.MapControllers();

Log.Information("NotificationService starting up with ErrorService middleware and RabbitMQ Consumer...");
app.Run();

// Expose Program class for integration testing
public partial class Program { }