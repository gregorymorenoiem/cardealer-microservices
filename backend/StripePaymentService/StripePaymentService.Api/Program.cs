using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using CarDealer.Shared.Idempotency.Extensions;
using CarDealer.Shared.Logging.Extensions;
using CarDealer.Shared.ErrorHandling.Extensions;
using CarDealer.Shared.Observability.Extensions;
using CarDealer.Shared.Audit.Extensions;
using StripePaymentService.Application.Features.PaymentIntent.Commands;
using StripePaymentService.Application.Features.Subscription.Commands;
using StripePaymentService.Application.Validators;
using StripePaymentService.Domain.Interfaces;
using StripePaymentService.Infrastructure.Persistence;
using StripePaymentService.Infrastructure.Repositories;
using StripePaymentService.Infrastructure.Services;

const string ServiceName = "StripePaymentService";
const string ServiceVersion = "1.0.0";

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

    // Add services
    builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=StripePaymentService;User Id=postgres;Password=postgres";

builder.Services.AddDbContext<StripeDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
    })
);

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreatePaymentIntentCommand).Assembly);
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreatePaymentIntentValidator>();

// Repositories
builder.Services.AddScoped<IStripePaymentIntentRepository, StripePaymentIntentRepository>();
builder.Services.AddScoped<IStripeCustomerRepository, StripeCustomerRepository>();
builder.Services.AddScoped<IStripeSubscriptionRepository, StripeSubscriptionRepository>();

// Services
var stripeApiKey = builder.Configuration["Stripe:ApiKey"] ?? throw new InvalidOperationException("Stripe:ApiKey not configured");
var stripeWebhookSecret = builder.Configuration["Stripe:WebhookSecret"] ?? throw new InvalidOperationException("Stripe:WebhookSecret not configured");

builder.Services.AddHttpClient<StripeHttpClient>()
    .AddTypedClient((client, provider) =>
    {
        var logger = provider.GetRequiredService<ILogger<StripeHttpClient>>();
        return new StripeHttpClient(client, stripeApiKey, logger);
    });

builder.Services.AddSingleton(provider =>
{
    var logger = provider.GetRequiredService<ILogger<StripeWebhookValidationService>>();
    return new StripeWebhookValidationService(stripeWebhookSecret, logger);
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Stripe Payment Service API",
        Version = "v1",
        Description = "Payment processing service for Stripe integration"
    });

    // Security scheme for JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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
    .AddDbContextCheck<StripeDbContext>();

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
        options.KeyPrefix = "stripe:idempotency";
        options.ValidateRequestHash = true;
        options.ProcessingTimeoutSeconds = 120;
    });
    Log.Information("Idempotency enabled for StripePaymentService");
}

    var app = builder.Build();

    // ============= MIDDLEWARE =============
    // Global exception handling (first in pipeline)
    app.UseGlobalErrorHandling();

    // Audit middleware for automatic request auditing
    app.UseAuditMiddleware();

    // Migrations
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<StripeDbContext>();
        db.Database.Migrate();
        Log.Information("Database migration completed for {ServiceName}", ServiceName);
    }

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");

    // Idempotency middleware
    if (idempotencyEnabled)
    {
        app.UseIdempotency();
    }

    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

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
