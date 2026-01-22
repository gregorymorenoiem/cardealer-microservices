using Microsoft.EntityFrameworkCore;
using BillingService.Application.Services;
using BillingService.Application.Configuration;
using BillingService.Application.Interfaces;
using BillingService.Domain.Interfaces;
using BillingService.Infrastructure.Persistence;
using BillingService.Infrastructure.Repositories;
using BillingService.Infrastructure.Services;
using BillingService.Infrastructure.External;
using BillingService.Infrastructure.Messaging;
using BillingService.Infrastructure.Azul;
using CarDealer.Shared.Secrets;
using CarDealer.Shared.Configuration;
using CarDealer.Shared.Idempotency.Extensions;
using CarDealer.Shared.Logging.Extensions;
using CarDealer.Shared.ErrorHandling.Extensions;
using CarDealer.Shared.Observability.Extensions;
using CarDealer.Shared.Audit.Extensions;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Secret Provider for Docker secrets
builder.Services.AddSecretProvider();

// ============================================================================
// FASE 2: OBSERVABILITY - Logging centralizado con Serilog + Seq
// ============================================================================
builder.UseStandardSerilog("BillingService", options =>
{
    options.SeqEnabled = true;
    options.SeqServerUrl = builder.Configuration["Logging:Seq:ServerUrl"] ?? "http://seq:5341";
    options.FileEnabled = builder.Configuration.GetValue<bool>("Logging:File:Enabled", false);
    options.FilePath = builder.Configuration["Logging:File:Path"] ?? "logs/billingservice-.log";
    options.RabbitMQEnabled = builder.Configuration.GetValue<bool>("RabbitMQ:Enabled", false);
    options.RabbitMQHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
    options.RabbitMQPort = builder.Configuration.GetValue<int>("RabbitMQ:Port", 5672);
    options.RabbitMQUser = builder.Configuration["RabbitMQ:User"] ?? "guest";
    options.RabbitMQPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BillingService API", Version = "v1" });
});

// ============================================================================
// FASE 2: OBSERVABILITY - OpenTelemetry Tracing + Metrics
// ============================================================================
builder.Services.AddStandardObservability("BillingService", options =>
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
    options.ServiceName = "BillingService";
    options.Environment = builder.Environment.EnvironmentName;
    options.PublishToErrorService = builder.Configuration.GetValue<bool>("ErrorHandling:PublishToErrorService", true);
    options.RabbitMQHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
    options.RabbitMQPort = builder.Configuration.GetValue<int>("RabbitMQ:Port", 5672);
    options.RabbitMQUser = builder.Configuration["RabbitMQ:User"] ?? "guest";
    options.RabbitMQPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";
    options.IncludeStackTrace = builder.Environment.IsDevelopment();
});

// ============================================================================
// FASE 5: Audit Publisher - Eventos de auditoría para pagos
// ============================================================================
builder.Services.AddAuditPublisher(builder.Configuration);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "your-secret-key-min-32-characters-long-for-security";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "CarDealerAuth",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "CarDealerApi",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure Stripe
builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("Stripe"));

// Configure AZUL
builder.Services.Configure<AzulConfiguration>(
    builder.Configuration.GetSection("Azul"));

// Add AZUL Services
builder.Services.AddScoped<IAzulHashGenerator, AzulHashGenerator>();
builder.Services.AddScoped<IAzulPaymentService, AzulPaymentService>();

// Add DbContext with secrets support
var connectionString = MicroserviceSecretsConfiguration.GetDatabaseConnectionString(builder.Configuration, "BillingService");
builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add Repositories
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IStripeCustomerRepository, StripeCustomerRepository>();
builder.Services.AddScoped<IEarlyBirdRepository, EarlyBirdRepository>();
builder.Services.AddScoped<IAzulTransactionRepository, AzulTransactionRepository>();

// Add Stripe Services
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<BillingApplicationService>();

// Add RabbitMQ Event Publisher
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

// Add UserService Client for syncing subscriptions
var userServiceUrl = builder.Configuration["Services:UserService"] ?? "http://localhost:5020";
builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
{
    client.BaseAddress = new Uri(userServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add Health Checks
builder.Services.AddHealthChecks();

// Add Idempotency for payment protection
var idempotencyEnabled = builder.Configuration.GetValue<bool>("Idempotency:Enabled", true);
if (idempotencyEnabled)
{
    builder.Services.AddIdempotency(options =>
    {
        options.Enabled = true;
        options.RedisConnection = builder.Configuration["Redis:Connection"] ?? "localhost:6379";
        options.HeaderName = "X-Idempotency-Key";
        options.DefaultTtlSeconds = 86400; // 24 hours
        options.RequireIdempotencyKey = true;
        options.KeyPrefix = "billing:idempotency";
        options.ValidateRequestHash = true;
        options.ProcessingTimeoutSeconds = 120; // 2 minutes for payment processing
    });
    Log.Information("Idempotency enabled for BillingService - Payment protection active");
}

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

app.UseCors();
app.UseHttpsRedirection();

// Idempotency middleware - before auth and controllers
if (idempotencyEnabled)
{
    app.UseIdempotency();
}

app.UseAuthentication();
app.UseAuthorization();

// FASE 5: Audit Middleware
app.UseAuditMiddleware();

app.MapControllers();
app.MapHealthChecks("/health");

// Apply migrations on startup in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    db.Database.Migrate();
}

app.Run();

// Make the implicit Program class public so it can be accessed by tests
public partial class Program { }
