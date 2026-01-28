using MediatR;
using FluentValidation;
using Serilog;
using Microsoft.EntityFrameworkCore;
using CarDealer.Shared.Idempotency.Extensions;
using CarDealer.Shared.Logging.Extensions;
using CarDealer.Shared.ErrorHandling.Extensions;
using CarDealer.Shared.Observability.Extensions;
using CarDealer.Shared.Audit.Extensions;
using PaymentService.Application.Validators;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.Repositories;
using PaymentService.Infrastructure.Services;
using PaymentService.Infrastructure.Services.Settings;
using PaymentService.Infrastructure.Services.Providers;

const string ServiceName = "PaymentService";
const string ServiceVersion = "2.0.0";

// Bootstrap logger for startup
Log.Logger = SerilogExtensions.CreateBootstrapLogger(ServiceName);

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

    // ==================== PAYMENT GATEWAY INFRASTRUCTURE ====================
    // Register core payment gateway services
    builder.Services.AddScoped<IPaymentGatewayRegistry, PaymentGatewayRegistry>();
    builder.Services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

    // Register individual payment gateway providers
    builder.Services.AddScoped<AzulPaymentProvider>();
    builder.Services.AddScoped<CardNETPaymentProvider>();
    builder.Services.AddScoped<PixelPayPaymentProvider>();
    builder.Services.AddScoped<FygaroPaymentProvider>();
    builder.Services.AddScoped<PayPalPaymentProvider>();

    // ==================== PAYMENT GATEWAY INITIALIZATION ====================
    // Initialize and register providers with registry on application startup
    var serviceProvider = builder.Services.BuildServiceProvider();
    var registry = serviceProvider.GetRequiredService<IPaymentGatewayRegistry>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Attempt to register each provider
        var azulProvider = serviceProvider.GetRequiredService<AzulPaymentProvider>();
        registry.Register(azulProvider);
        logger.LogInformation("✅ AZUL payment provider registered successfully");

        var cardnetProvider = serviceProvider.GetRequiredService<CardNETPaymentProvider>();
        registry.Register(cardnetProvider);
        logger.LogInformation("✅ CardNET payment provider registered successfully");

        var pixelpayProvider = serviceProvider.GetRequiredService<PixelPayPaymentProvider>();
        registry.Register(pixelpayProvider);
        logger.LogInformation("✅ PixelPay payment provider registered successfully");

        var fygaroProvider = serviceProvider.GetRequiredService<FygaroPaymentProvider>();
        registry.Register(fygaroProvider);
        logger.LogInformation("✅ Fygaro payment provider registered successfully");

        var paypalProvider = serviceProvider.GetRequiredService<PayPalPaymentProvider>();
        registry.Register(paypalProvider);
        logger.LogInformation("✅ PayPal payment provider registered successfully (International)");

        // Log summary of registered providers
        var registeredProviders = registry.GetAll();
        logger.LogInformation("📊 Total providers registered: {ProviderCount}", registeredProviders.Count);
        
        foreach (var provider in registeredProviders)
        {
            var configErrors = provider.ValidateConfiguration();
            var status = configErrors.Count == 0 ? "✅ CONFIGURED" : "⚠️  NOT CONFIGURED";
            logger.LogInformation("   • {ProviderName}: {Status}", provider.Name, status);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error during payment provider registration");
        throw;
    }

// ==================== LEGACY REPOSITORIES ====================

builder.Services.AddScoped<IAzulTransactionRepository, AzulTransactionRepository>();
builder.Services.AddScoped<IAzulSubscriptionRepository, AzulSubscriptionRepository>();

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

// Redis cache (opcional pero recomendado)
var redisConnection = builder.Configuration["Redis:Connection"];
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "PaymentService:";
    });
    Log.Information("Redis cache habilitado para tasas de cambio");
}

// Services
builder.Services.AddScoped<AzulWebhookValidationService>();
builder.Services.AddHttpClient<AzulHttpClient>();

// MediatR
builder.Services.AddMediatR(config =>
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
            .WithOrigins("http://localhost:8080", "https://api.okla.com.do")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

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

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AzulDbContext>("Database");

// Idempotency for payment protection
var idempotencyEnabled = builder.Configuration.GetValue<bool>("Idempotency:Enabled", true);
if (idempotencyEnabled)
{
    builder.Services.AddIdempotency(options =>
    {
        options.Enabled = true;
        options.RedisConnection = builder.Configuration["Redis:Connection"] ?? "localhost:6379";
        options.HeaderName = "X-Idempotency-Key";
        options.DefaultTtlSeconds = 86400;
        options.RequireIdempotencyKey = true;
        options.KeyPrefix = "azul:idempotency";
        options.ValidateRequestHash = true;
        options.ProcessingTimeoutSeconds = 120;
    });
    Log.Information("Idempotency enabled for AzulPaymentService");
}

// ============= BUILD =============
    var app = builder.Build();

    // ============= MIDDLEWARE =============
    // Global exception handling (first in pipeline)
    app.UseGlobalErrorHandling();

    // Audit middleware for automatic request auditing
    app.UseAuditMiddleware();

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
