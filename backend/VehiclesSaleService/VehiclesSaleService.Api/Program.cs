using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VehiclesSaleService.Domain.Interfaces;
using VehiclesSaleService.Infrastructure.Persistence;
using VehiclesSaleService.Infrastructure.Repositories;
using VehiclesSaleService.Infrastructure.Messaging;
using CarDealer.Shared.Secrets;
using CarDealer.Shared.Configuration;
using CarDealer.Shared.MultiTenancy;
using CarDealer.Shared.Logging.Extensions;
using CarDealer.Shared.ErrorHandling.Extensions;
using CarDealer.Shared.Observability.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// FASE 2: OBSERVABILITY - Logging centralizado con Serilog + Seq
// ============================================================================
builder.UseStandardSerilog("VehiclesSaleService", options =>
{
    options.SeqEnabled = true;
    options.SeqServerUrl = builder.Configuration["Logging:Seq:ServerUrl"] ?? "http://seq:5341";
    options.FileEnabled = builder.Configuration.GetValue<bool>("Logging:File:Enabled", false);
    options.FilePath = builder.Configuration["Logging:File:Path"] ?? "logs/vehiclessaleservice-.log";
    options.RabbitMQEnabled = builder.Configuration.GetValue<bool>("RabbitMQ:Enabled", false);
    options.RabbitMQHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
    options.RabbitMQPort = builder.Configuration.GetValue<int>("RabbitMQ:Port", 5672);
    options.RabbitMQUser = builder.Configuration["RabbitMQ:User"] ?? "guest";
    options.RabbitMQPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";
});

// ========================================
// SECRET PROVIDER
// ========================================

builder.Services.AddSecretProvider();

// ========================================
// MULTI-TENANCY
// ========================================

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// ========================================
// CONFIGURATION
// ========================================

// Configure routing to use lowercase URLs
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = false;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "VehiclesSaleService API",
        Version = "v1",
        Description = "API for vehicle sales marketplace - buy and sell cars, trucks, motorcycles, boats, and more"
    });
});

// ========================================
// DATABASE
// ========================================

// Priority: Environment variable > Docker config > appsettings.json
var connectionString = builder.Configuration["Database:ConnectionStrings:PostgreSQL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? MicroserviceSecretsConfiguration.GetDatabaseConnectionString(builder.Configuration, "VehiclesSaleService");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });
});

// ========================================
// DEPENDENCY INJECTION
// ========================================

builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IVehicleCatalogRepository, VehicleCatalogRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();

// RabbitMQ Event Publisher
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

// ========================================
// JWT AUTHENTICATION
// ========================================

var jwtSecret = builder.Configuration["Jwt:Key"] 
    ?? builder.Configuration["JWT:Secret"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? "DefaultDevSecretKeyThatIsAtLeast32Characters!";

var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? builder.Configuration["JWT:Issuer"] 
    ?? "CarDealerPlatform";

var jwtAudience = builder.Configuration["Jwt:Audience"] 
    ?? builder.Configuration["JWT:Audience"] 
    ?? "CarDealerAPI";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = false, // Allow any audience since Gateway forwards requests
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization();

// ========================================
// CORS
// ========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ========================================
// HEALTH CHECKS
// ========================================

builder.Services.AddHealthChecks();

// ============================================================================
// FASE 2: OBSERVABILITY - OpenTelemetry Tracing + Metrics
// ============================================================================
builder.Services.AddStandardObservability("VehiclesSaleService", options =>
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
    options.ServiceName = "VehiclesSaleService";
    options.Environment = builder.Environment.EnvironmentName;
    options.PublishToErrorService = builder.Configuration.GetValue<bool>("ErrorHandling:PublishToErrorService", true);
    options.RabbitMQHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
    options.RabbitMQPort = builder.Configuration.GetValue<int>("RabbitMQ:Port", 5672);
    options.RabbitMQUser = builder.Configuration["RabbitMQ:User"] ?? "guest";
    options.RabbitMQPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";
    options.IncludeStackTrace = builder.Environment.IsDevelopment();
});

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

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// ========================================
// DATABASE MIGRATION
// ========================================

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("✅ Database migration completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database migration failed: {ex.Message}");
    }
}

// ========================================
// START
// ========================================

Console.WriteLine("🚀 VehiclesSaleService API starting...");
Console.WriteLine($"📦 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"🗄️  Database: {connectionString}");

app.Run();
