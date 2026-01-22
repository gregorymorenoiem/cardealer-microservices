using MediatR;
using FluentValidation;
using Serilog;
using Microsoft.EntityFrameworkCore;
using CarDealer.Shared.Idempotency.Extensions;
using CarDealer.Shared.Logging.Extensions;
using CarDealer.Shared.ErrorHandling.Extensions;
using CarDealer.Shared.Observability.Extensions;
using CarDealer.Shared.Audit.Extensions;
using AzulPaymentService.Application.Validators;
using AzulPaymentService.Domain.Interfaces;
using AzulPaymentService.Infrastructure.Persistence;
using AzulPaymentService.Infrastructure.Repositories;
using AzulPaymentService.Infrastructure.Services;

const string ServiceName = "AzulPaymentService";
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

    // ============= SERVICES =============
    // Database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<AzulDbContext>(options =>
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
        }));

// Repositories
builder.Services.AddScoped<IAzulTransactionRepository, AzulTransactionRepository>();
builder.Services.AddScoped<IAzulSubscriptionRepository, AzulSubscriptionRepository>();

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
        Title = "AZUL Payment Service",
        Version = "v1",
        Description = "Servicio de pagos integrado con AZUL (Banco Popular RD)",
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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AZUL Payment Service V1");
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
