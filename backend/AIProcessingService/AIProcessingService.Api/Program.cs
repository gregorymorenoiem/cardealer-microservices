using CarDealer.Shared.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
using CarDealer.Shared.Secrets;
using CarDealer.Shared.Logging.Extensions;
using CarDealer.Shared.Observability.Extensions;
using CarDealer.Shared.ErrorHandling.Extensions;
using CarDealer.Shared.Audit.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using MassTransit;
using Serilog;
using AIProcessingService.Infrastructure.Persistence;
using AIProcessingService.Infrastructure.Persistence.Repositories;
using AIProcessingService.Domain.Interfaces;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.ResponseCompression;

const string ServiceName = "AIProcessingService";
const string ServiceVersion = "1.1.0";

try
{

var builder = WebApplication.CreateBuilder(args);

// ============= STRUCTURED LOGGING =============
builder.UseStandardSerilog(ServiceName);

// ============= SECRETS PROVIDER =============
builder.Services.AddSecretProvider();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger (Development only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = $"{ServiceName} API",
        Version = "v1",
        Description = "AI-powered image processing for vehicle photos (SAM2, CLIP, YOLO)"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// ============= OBSERVABILITY =============
builder.Services.AddStandardObservability(builder.Configuration, ServiceName, ServiceVersion);

// ============= ERROR HANDLING =============
builder.Services.AddStandardErrorHandling(builder.Configuration, ServiceName);

// ============= AUDIT =============
builder.Services.AddAuditPublisher(builder.Configuration);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

builder.Services.AddDbContext<AIProcessingDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories
builder.Services.AddScoped<IImageProcessingJobRepository, ImageProcessingJobRepository>();
builder.Services.AddScoped<ISpin360JobRepository, Spin360JobRepository>();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AIProcessingService.Application.Features.Commands.ProcessImageCommand).Assembly));

// SecurityValidation — ensures FluentValidation validators (NoSqlInjection, NoXss) run in MediatR pipeline
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(AIProcessingService.Application.Behaviors.ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssembly(typeof(AIProcessingService.Application.Features.Commands.ProcessImageCommand).Assembly);

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? throw new InvalidOperationException("RabbitMQ:Username is not configured");
        var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? throw new InvalidOperationException("RabbitMQ:Password is not configured");
        
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });
        
        cfg.ConfigureEndpoints(context);
    });
});

// ============= JWT AUTHENTICATION (centralized config) =============
var (jwtKey, jwtIssuer, jwtAudience) = MicroserviceSecretsConfiguration.GetJwtConfig(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// ============= CORS (restricted) =============
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? (builder.Environment.IsDevelopment()
        ? new[] { "http://localhost:3000", "http://localhost:5173" }
        : new[] { "https://okla.com.do", "https://www.okla.com.do" });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization", "X-CSRF-Token", "X-Requested-With")
              .AllowCredentials();
    });
});

// ============= RATE LIMITING (per-IP) =============
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("fixed", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
    options.AddPolicy("ai-processing", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc6585#section-4",
            title = "Demasiadas solicitudes",
            status = 429,
            detail = "Has excedido el límite de solicitudes. Intenta de nuevo más tarde."
        }, cancellationToken);
    };
});

// Health Checks (with tags for triple pattern)
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "database", tags: new[] { "ready", "external" })
    .AddRabbitMQ(
        rabbitConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"] ?? throw new InvalidOperationException("RabbitMQ:Username is not configured")}:{builder.Configuration["RabbitMQ:Password"] ?? throw new InvalidOperationException("RabbitMQ:Password is not configured")}@{builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq"}:5672",
        name: "rabbitmq",
        tags: new[] { "ready", "external" });

// ============= RESPONSE COMPRESSION (Brotli + Gzip) =============
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "text/json",
        "application/problem+json"
    });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);

var app = builder.Build();

// ============= MIDDLEWARE PIPELINE =============

// Global Error Handling — FIRST to catch all exceptions
app.UseGlobalErrorHandling();

// Request Logging with TraceId, UserId enrichment
app.UseRequestLogging();

// OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

// Response Compression — early, after error handling
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Forwarded Headers — required for correct client IP behind K8s/LB
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors();
app.UseRateLimiter();

if (!app.Environment.IsProduction())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Audit Middleware
app.UseAuditMiddleware();

app.MapControllers();

// ============= HEALTH CHECKS (Triple Pattern) =============
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => !check.Tags.Contains("external")
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Apply migrations on startup when configured
var autoMigrate = builder.Configuration.GetValue<bool>("Database:AutoMigrate", false)
    || app.Environment.IsDevelopment();
if (autoMigrate)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AIProcessingDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        Log.Information("Database migration completed for {ServiceName}", ServiceName);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database migration error for {ServiceName}", ServiceName);
    }
}

Log.Information("Starting {ServiceName} v{ServiceVersion} — AI Image Processing Platform", ServiceName, ServiceVersion);
await app.RunAsync();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application {ServiceName} terminated unexpectedly", "AIProcessingService");
}
finally
{
    Log.CloseAndFlush();
}

// Make the implicit Program class public so it can be accessed by tests
public partial class Program { }
