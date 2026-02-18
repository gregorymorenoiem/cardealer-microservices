using CarDealer.Shared.Middleware;
using MediatR;
using FluentValidation;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CarDealer.Shared.Idempotency.Extensions;
using CarDealer.Shared.Logging.Extensions;
using CarDealer.Shared.ErrorHandling.Extensions;
using CarDealer.Shared.Observability.Extensions;
using CarDealer.Shared.Audit.Extensions;
using CarDealer.Shared.Configuration;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using PaymentService.Application.Validators;
using PaymentService.Application.Services;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.Repositories;
using PaymentService.Infrastructure.Services;
using PaymentService.Infrastructure.Services.Settings;
using PaymentService.Infrastructure.Services.Providers;
using PaymentService.Api;

const string ServiceName = "PaymentService";
const string ServiceVersion = "2.0.0";

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ============= CENTRALIZED LOGGING (Serilog → Seq) =============
    builder.UseStandardSerilog(ServiceName);

    // ============= OBSERVABILITY (OpenTelemetry → Jaeger) =============
    builder.Services.AddStandardObservability(builder.Configuration, ServiceName, ServiceVersion);

    // ============= ERROR HANDLING (→ ErrorService) =============
    builder.Services.AddStandardErrorHandling(builder.Configuration, ServiceName);

    // ============= AUDIT (→ AuditService via RabbitMQ) =============
    builder.Services.AddAuditPublisher(builder.Configuration);

    // ============= CONFIGURATION SERVICE CLIENT =============
    builder.Services.AddConfigurationServiceClient(builder.Configuration, ServiceName);

    // ============= SERVICES =============
    // Database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<AzulDbContext>(options =>
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
        }));

    // ==================== PAYMENT GATEWAY SETTINGS CONFIGURATION ====================
    // Load configuration sections for each payment gateway provider
    builder.Services.Configure<AzulSettings>(builder.Configuration.GetSection("PaymentGateway:Azul"));
    builder.Services.Configure<CardNETSettings>(builder.Configuration.GetSection("PaymentGateway:CardNET"));
    builder.Services.Configure<PixelPaySettings>(builder.Configuration.GetSection("PaymentGateway:PixelPay"));
    builder.Services.Configure<FygaroSettings>(builder.Configuration.GetSection("PaymentGateway:Fygaro"));
    builder.Services.Configure<PayPalSettings>(builder.Configuration.GetSection("PaymentGateway:PayPal"));

    // ==================== HTTP CLIENTS FOR PAYMENT PROVIDERS ====================
    // Register typed HttpClients for each payment provider
    builder.Services.AddHttpClient<AzulPaymentProvider>();
    builder.Services.AddHttpClient<CardNETPaymentProvider>();
    builder.Services.AddHttpClient<PixelPayPaymentProvider>();
    builder.Services.AddHttpClient<FygaroPaymentProvider>();
    builder.Services.AddHttpClient<PayPalPaymentProvider>();

    // ==================== PAYMENT GATEWAY INFRASTRUCTURE ====================
    // Register core payment gateway services (Singleton to persist registered providers across requests)
    builder.Services.AddSingleton<IPaymentGatewayRegistry, PaymentGatewayRegistry>();
    builder.Services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

    // Gateway availability reads billing.{provider}_enabled from ConfigurationService (admin panel)
    builder.Services.AddScoped<IGatewayAvailabilityService, GatewayAvailabilityService>();

    // ==================== PAYMENT GATEWAY INITIALIZATION ====================
    // Initialize and register providers with registry after app is built
    builder.Services.AddHostedService<PaymentProviderRegistrationService>();

    // ==================== LEGACY REPOSITORIES ====================
    builder.Services.AddScoped<IAzulTransactionRepository, AzulTransactionRepository>();
    builder.Services.AddScoped<IAzulSubscriptionRepository, AzulSubscriptionRepository>();

    // ==================== SAVED PAYMENT METHODS ====================
    builder.Services.AddScoped<ISavedPaymentMethodRepository, SavedPaymentMethodRepository>();

    // ==================== TOKENIZATION SERVICE ====================
    builder.Services.AddScoped<ITokenizationService, TokenizationService>();

    // ==================== EXCHANGE RATE (BCRD - DGII Compliance) ====================
    // Configuración del Banco Central
    builder.Services.Configure<BancoCentralSettings>(builder.Configuration.GetSection("BancoCentral"));

    // Repositorios de tasas de cambio
    builder.Services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
    builder.Services.AddScoped<ICurrencyConversionRepository, CurrencyConversionRepository>();

    // Cliente HTTP del Banco Central
    builder.Services.AddHttpClient<BancoCentralApiClient>();

    // Servicio de tasas de cambio
    builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();

    // Background job para actualización diaria de tasas (8:30 AM hora RD)
    builder.Services.AddHostedService<ExchangeRateRefreshJob>();

    // Redis cache (required for TokenizationService session management)
    var redisConnection = builder.Configuration["Redis:Connection"] 
        ?? builder.Configuration["Redis:ConnectionString"] 
        ?? "redis:6379,abortConnect=false";
    
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "PaymentService:";
    });
    Log.Information("Redis cache enabled for PaymentService: {Connection}", redisConnection);

    // Services
    builder.Services.AddScoped<AzulWebhookValidationService>();
    builder.Services.AddHttpClient<AzulHttpClient>();

    // MediatR
    builder.Services.AddMediatR(config =>

// SecurityValidation — ensures FluentValidation validators (NoSqlInjection, NoXss) run in MediatR pipeline
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(PaymentService.Application.Behaviors.ValidationBehavior<,>));
        config.RegisterServicesFromAssembly(typeof(Program).Assembly));

    // FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<ChargeRequestValidator>();

    // Controllers
    builder.Services.AddControllers();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowApiGateway", policyBuilder =>
        {
            policyBuilder
                .WithOrigins("http://localhost:8080", "http://localhost:3000", "https://api.okla.com.do", "https://okla.com.do")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // ============= JWT AUTHENTICATION =============
    var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key must be configured via environment/settings. Do NOT use hardcoded keys.");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AuthService-Dev";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "OKLA-Dev";

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
                Log.Debug("JWT Token validated for user: {User}", context.Principal?.Identity?.Name ?? "Unknown");
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // Swagger
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Payment Service - Multi-Provider Gateway",
        Version = "v2.0.0",
        Description = "Servicio de pagos genérico con soporte para múltiples proveedores: AZUL, CardNET, PixelPay, Fygaro, PayPal (Internacional)",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "OKLA Team",
            Email = "dev@okla.com.do"
        }
    });

    // Agregar seguridad JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// ========================================
// RATE LIMITING
// ========================================
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("PaymentPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = builder.Configuration.GetValue<int>("Security:RateLimit:RequestsPerMinute", 30);
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AzulDbContext>("Database");

// Idempotency for payment protection
var idempotencyEnabled = builder.Configuration.GetValue<bool>("Idempotency:Enabled", true);
if (idempotencyEnabled)
{
    var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379,abortConnect=false";
    builder.Services.AddIdempotency(options =>
    {
        options.Enabled = true;
        options.RedisConnection = redisConnectionString;
        options.HeaderName = "X-Idempotency-Key";
        options.DefaultTtlSeconds = 86400;
        options.RequireIdempotencyKey = false; // Allow requests without idempotency key for GET methods
        options.KeyPrefix = "payment:idempotency";
        options.ValidateRequestHash = true;
        options.ProcessingTimeoutSeconds = 120;
    });
    Log.Information("Idempotency enabled for PaymentService with Redis: {Connection}", redisConnectionString);
}

// ============= BUILD =============
    var app = builder.Build();

    // ============= MIDDLEWARE =============
    // Global exception handling (first in pipeline)
    app.UseGlobalErrorHandling();

    // Audit middleware for automatic request auditing
    app.UseAuditMiddleware();

    // OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

if (app.Environment.IsDevelopment())
{

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service Multi-Provider v2.0.0");
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowApiGateway");
    app.UseRateLimiter();

    // Idempotency middleware
    if (idempotencyEnabled)
    {
        app.UseIdempotency();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    // ============= DATABASE MIGRATION =============
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AzulDbContext>();
            dbContext.Database.Migrate();
            Log.Information("Database migration completed for {ServiceName}", ServiceName);
        }
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Error during database migration for {ServiceName}", ServiceName);
    }

    // ============= RUN =============
    Log.Information("Starting {ServiceName} v{ServiceVersion}", ServiceName, ServiceVersion);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application {ServiceName} terminated unexpectedly", ServiceName);
}
finally
{
    Log.CloseAndFlush();
}
