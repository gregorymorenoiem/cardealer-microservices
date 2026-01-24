using UserService.Domain.Interfaces;
using UserService.Infrastructure.Messaging;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Services;
using UserService.Infrastructure.Services.Messaging;
using UserService.Shared.Extensions;
using UserService.Shared.Middleware;
using UserService.Shared.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using CarDealer.Shared.Database;
using CarDealer.Shared.Secrets;
using CarDealer.Shared.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using FluentValidation;
using UserService.Application.Behaviors;
using MediatR;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using UserService.Api.Middleware;
using CarDealer.Shared.Logging.Extensions;
using CarDealer.Shared.ErrorHandling.Extensions;
using CarDealer.Shared.Observability.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Secret Provider for externalized secrets
builder.Services.AddSecretProvider();

// ============================================================================
// FASE 2: OBSERVABILITY - Logging centralizado con Serilog + Seq
// ============================================================================
builder.UseStandardSerilog("UserService", options =>
{
    options.SeqEnabled = true;
    options.SeqServerUrl = builder.Configuration["Logging:Seq:ServerUrl"] ?? "http://seq:5341";
    options.FileEnabled = builder.Configuration.GetValue<bool>("Logging:File:Enabled", false);
    options.FilePath = builder.Configuration["Logging:File:Path"] ?? "logs/userservice-.log";
    options.RabbitMQEnabled = builder.Configuration.GetValue<bool>("RabbitMQ:Enabled", false);
    options.RabbitMQHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
    options.RabbitMQPort = builder.Configuration.GetValue<int>("RabbitMQ:Port", 5672);
    options.RabbitMQUser = builder.Configuration["RabbitMQ:User"] ?? "guest";
    options.RabbitMQPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger con soporte JWT
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UserService API",
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

// Configurar autenticación JWT with externalized secrets
var jwtSettings = builder.Configuration.GetSection("Jwt");
var (jwtKey, jwtIssuer, jwtAudience) = MicroserviceSecretsConfiguration.GetJwtConfig(builder.Configuration);
var secretKey = jwtKey ?? throw new InvalidOperationException("JWT Key not configured");

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
        ValidIssuer = jwtIssuer ?? jwtSettings["Issuer"],
        ValidAudience = jwtAudience ?? jwtSettings["Audience"],
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
    // Política para acceso general al UserService
    options.AddPolicy("UserServiceAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("service", "UserService", "all");
    });

    // Política para operaciones administrativas
    options.AddPolicy("UserServiceAdmin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("role", "admin", "UserService-admin");
    });

    // Política para solo lectura
    options.AddPolicy("UserServiceRead", policy =>
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
builder.Services.AddScoped<IUserRepository, UserService.Infrastructure.Persistence.UserRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserService.Infrastructure.Persistence.UserRoleRepository>();
builder.Services.AddScoped<IRoleRepository, UserService.Infrastructure.Persistence.EfRoleRepository>();
builder.Services.AddScoped<IDealerRepository, UserService.Infrastructure.Persistence.DealerRepository>();
builder.Services.AddScoped<ISellerProfileRepository, UserService.Infrastructure.Repositories.SellerProfileRepository>();
builder.Services.AddScoped<IIdentityDocumentRepository, UserService.Infrastructure.Persistence.IdentityDocumentRepository>();
builder.Services.AddScoped<IDealerEmployeeRepository, UserService.Infrastructure.Persistence.Repositories.DealerEmployeeRepository>();
builder.Services.AddScoped<IDealerOnboardingRepository, UserService.Infrastructure.Persistence.Repositories.DealerOnboardingRepository>();
builder.Services.AddScoped<IDealerModuleRepository, UserService.Infrastructure.Persistence.Repositories.DealerModuleRepository>();
builder.Services.AddScoped<IModuleRepository, UserService.Infrastructure.Persistence.Repositories.ModuleRepository>();
builder.Services.AddScoped<IUserOnboardingRepository, UserService.Infrastructure.Repositories.UserOnboardingRepository>();
builder.Services.AddScoped<IErrorReporter, UserService.Infrastructure.Services.ErrorReporter>();

// Application Services - External clients
builder.Services.AddHttpClient<UserService.Application.Interfaces.IRoleServiceClient, UserService.Infrastructure.External.RoleServiceClient>(client =>
{
    var roleServiceUrl = builder.Configuration["ServiceUrls:RoleService"] ?? "https://localhost:45952";
    client.BaseAddress = new Uri(roleServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddStandardResilienceHandler(options =>
{
    // Retry 3 times with exponential backoff
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
    options.Retry.UseJitter = true;

    // Circuit breaker: open after 5 failures, stay open for 30s
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(20); // Must be >= 2x AttemptTimeout
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
    options.CircuitBreaker.MinimumThroughput = 5;

    // Timeout of 10 seconds per attempt
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);

    // Total timeout of 30 seconds
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<UserService.Application.Interfaces.IAuditServiceClient, UserService.Infrastructure.External.AuditServiceClient>(client =>
{
    var auditServiceUrl = builder.Configuration["ServiceUrls:AuditService"] ?? "https://localhost:7287";
    client.BaseAddress = new Uri(auditServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<UserService.Application.Interfaces.INotificationServiceClient, UserService.Infrastructure.External.NotificationServiceClient>(client =>
{
    var notificationServiceUrl = builder.Configuration["ServiceUrls:NotificationService"] ?? "https://localhost:45954";
    client.BaseAddress = new Uri(notificationServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<UserService.Application.Interfaces.IErrorServiceClient, UserService.Infrastructure.External.ErrorServiceClient>(client =>
{
    var errorServiceUrl = builder.Configuration["ServiceUrls:ErrorService"] ?? "https://localhost:45952";
    client.BaseAddress = new Uri(errorServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Métricas personalizadas (Singleton para compartir estado)
builder.Services.AddSingleton<UserService.Application.Metrics.UserServiceMetrics>();

// Dead Letter Queue para eventos fallidos (Singleton, en memoria)
// TEMPORARY: Commented out for testing without RabbitMQ
// builder.Services.AddSingleton<UserService.Infrastructure.Messaging.IDeadLetterQueue>(sp =>
//     new UserService.Infrastructure.Messaging.InMemoryDeadLetterQueue(maxRetries: 5));

// Event Publisher for RabbitMQ (con DLQ integrado)
// NOTE: Using NoOpEventPublisher for development. Enable RabbitMQ in production.
var useRabbitMq = builder.Configuration.GetValue<bool>("RabbitMQ:Enabled", false);
if (useRabbitMq)
{
    builder.Services.AddSingleton<UserService.Infrastructure.Messaging.IDeadLetterQueue>(sp =>
        new UserService.Infrastructure.Messaging.InMemoryDeadLetterQueue(maxRetries: 5));
    builder.Services.AddSingleton<UserService.Infrastructure.Messaging.RabbitMqEventPublisher>();
    builder.Services.AddSingleton<IEventPublisher>(sp =>
        sp.GetRequiredService<UserService.Infrastructure.Messaging.RabbitMqEventPublisher>());
    builder.Services.AddHostedService<UserService.Infrastructure.Messaging.DeadLetterQueueProcessor>();

    // Consumer for UserRegisteredEvent from AuthService - syncs users automatically
    builder.Services.AddHostedService<UserService.Infrastructure.Services.Messaging.UserRegisteredEventConsumer>();
}
else
{
    // Use NoOp publisher for development without RabbitMQ
    builder.Services.AddSingleton<IEventPublisher, UserService.Infrastructure.Messaging.NoOpEventPublisher>();
}

// Agregar MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(UserService.Application.UseCases.Users.CreateUser.CreateUserCommand).Assembly));

// Registrar FluentValidation (si hay validadores)
// builder.Services.AddValidatorsFromAssembly(
//     typeof(UserService.Application.UseCases.Users.CreateUser.CreateUserCommand).Assembly);

// Agregar behavior de validación para MediatR
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ============================================================================
// FASE 2: OBSERVABILITY - OpenTelemetry Tracing + Metrics
// ============================================================================
builder.Services.AddStandardObservability("UserService", options =>
{
    options.TracingEnabled = true;
    options.MetricsEnabled = true;
    options.OtlpEndpoint = builder.Configuration["Observability:Otlp:Endpoint"] ?? "http://jaeger:4317";
    options.SamplingRatio = builder.Configuration.GetValue<double>("Observability:SamplingRatio", builder.Environment.IsProduction() ? 0.1 : 1.0);
    options.PrometheusEnabled = builder.Configuration.GetValue<bool>("Observability:Prometheus:Enabled", true);
    options.ExcludedPaths = new[] { "/health", "/metrics", "/swagger" };
});

// ============================================================================
// FASE 2: OBSERVABILITY - Error Handling centralizado
// ============================================================================
builder.Services.AddStandardErrorHandling(options =>
{
    options.ServiceName = "UserService";
    options.Environment = builder.Environment.EnvironmentName;
    options.PublishToErrorService = builder.Configuration.GetValue<bool>("ErrorHandling:PublishToErrorService", true);
    options.RabbitMQHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
    options.RabbitMQPort = builder.Configuration.GetValue<int>("RabbitMQ:Port", 5672);
    options.RabbitMQUser = builder.Configuration["RabbitMQ:User"] ?? "guest";
    options.RabbitMQPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";
    options.IncludeStackTrace = builder.Environment.IsDevelopment();
});

// Configurar Rate Limiting
var rateLimitingConfig = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingConfiguration>()
    ?? new RateLimitingConfiguration();
builder.Services.AddCustomRateLimiting(rateLimitingConfig);

// Configurar RabbitMQ
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<UserServiceRabbitMQSettings>(builder.Configuration.GetSection("UserService"));

// Registrar el consumidor RabbitMQ como hosted service
// TEMPORARY: Commented out for testing without RabbitMQ
// builder.Services.AddHostedService<RabbitMQErrorConsumer>();

var app = builder.Build();

// ============================================================================
// MIDDLEWARE PIPELINE
// ============================================================================

// FASE 2: Global Error Handling - PRIMERO para capturar todas las excepciones
app.UseGlobalErrorHandling();

// FASE 2: Request Logging con enrichment de TraceId, UserId, CorrelationId
app.UseRequestLogging();

// Swagger en desarrollo
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

// Service Discovery Auto-Registration (only if Consul is enabled)
var consulEnabled = app.Configuration.GetValue<bool>("Consul:Enabled", false);
if (consulEnabled)
{
    app.UseMiddleware<ServiceRegistrationMiddleware>();
}

// Middleware para capturar respuestas (ResponseCaptureMiddleware local)
app.UseMiddleware<ResponseCaptureMiddleware>();

// NOTA: ErrorHandling local reemplazado por UseGlobalErrorHandling() de Fase 2

app.MapControllers();

// Health check endpoint - acceso anónimo
app.MapGet("/health", [AllowAnonymous] () => Results.Ok(new
{
    service = "UserService",
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

Log.Information("UserService starting up...");
app.Run();

// Make Program class accessible for integration testing
public partial class Program { }
