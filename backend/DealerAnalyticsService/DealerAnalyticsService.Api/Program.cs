using CarDealer.Shared.Middleware;
using DealerAnalyticsService.Application.Features.Analytics.Queries;
using DealerAnalyticsService.Application.Services;
using DealerAnalyticsService.Domain.Interfaces;
using DealerAnalyticsService.Infrastructure.Messaging;
using DealerAnalyticsService.Infrastructure.Persistence;
using DealerAnalyticsService.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. Database Configuration
// ============================================
// Priority: Docker env var > appsettings ConnectionStrings > fallback
var connectionString = builder.Configuration["Database:ConnectionStrings:PostgreSQL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=postgres_db;Port=5432;Database=dealeranalyticsservice;Username=postgres;Password=password";

// Register both DbContext classes
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("DealerAnalyticsService.Infrastructure");
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    }));

builder.Services.AddDbContext<DealerAnalyticsService.Infrastructure.Persistence.DealerAnalyticsDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("DealerAnalyticsService.Infrastructure");
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    }));

// ============================================
// 2. MediatR Configuration
// ============================================
builder.Services.AddMediatR(cfg =>

// SecurityValidation — ensures FluentValidation validators (NoSqlInjection, NoXss) run in MediatR pipeline
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(DealerAnalyticsService.Application.Behaviors.ValidationBehavior<,>));
{
    cfg.RegisterServicesFromAssembly(typeof(GetDashboardAnalyticsQuery).Assembly);
});

// ============================================
// 3. Repository Pattern DI
// ============================================
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IDealerAnalyticsRepository, DealerAnalyticsRepository>();
builder.Services.AddScoped<IConversionFunnelRepository, ConversionFunnelRepository>();
builder.Services.AddScoped<IMarketBenchmarkRepository, MarketBenchmarkRepository>();
builder.Services.AddScoped<IDealerInsightRepository, DealerInsightRepository>();

// Advanced Analytics Repositories
builder.Services.AddScoped<IDealerSnapshotRepository, DealerSnapshotRepository>();
builder.Services.AddScoped<IVehiclePerformanceRepository, VehiclePerformanceRepository>();
builder.Services.AddScoped<ILeadFunnelRepository, LeadFunnelRepository>();
builder.Services.AddScoped<IDealerBenchmarkRepository, DealerBenchmarkRepository>();
builder.Services.AddScoped<IDealerAlertRepository, DealerAlertRepository>();
builder.Services.AddScoped<IInventoryAgingRepository, InventoryAgingRepository>();

// ============================================
// 3.5. Event Publishing (RabbitMQ)
// ============================================
var rabbitMqEnabled = builder.Configuration.GetValue<bool>("RabbitMQ:Enabled", false);
if (rabbitMqEnabled)
{
    builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
}
else
{
    builder.Services.AddSingleton<IEventPublisher, NullEventPublisher>();
}

// ============================================
// 3.6. Background Services
// ============================================
var enableBackgroundJobs = builder.Configuration.GetValue<bool>("Analytics:EnableBackgroundJobs", true);
if (enableBackgroundJobs)
{
    builder.Services.AddHostedService<DailySnapshotService>();
    builder.Services.AddHostedService<AlertAnalysisService>();
}

// ============================================
// 4. CORS Configuration
// ============================================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(, policy =>
    {
        var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        if (isDev)
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(
                    "https://okla.com.do",
                    "https://www.okla.com.do",
                    "https://api.okla.com.do")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// ============================================
// 5. JWT Authentication
// ============================================
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key must be configured via environment/settings. Do NOT use hardcoded keys.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AuthService-Dev";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "OKLA-Dev";

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ============================================
// 6. Controllers & Swagger
// ============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Dealer Analytics Service API",
        Version = "v1",
        Description = "API para analytics y métricas de dealers en OKLA"
    });

    // JWT Bearer configuration
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// ============================================
// 7. Health Checks
// ============================================
builder.Services.AddHealthChecks();

var app = builder.Build();

// ============================================
// 8. Middleware Pipeline
// ============================================

// Swagger UI in all environments for testing

// OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dealer Analytics API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// ============================================
// 9. Database Migration (Development Only)
// ============================================
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    
    // Migrate AnalyticsDbContext (creates DB + its tables)
    var analyticsDb = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    try
    {
        analyticsDb.Database.EnsureCreated();
        Console.WriteLine("✅ AnalyticsDbContext database created/verified successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ AnalyticsDbContext migration failed: {ex.Message}");
    }
    
    // Create tables for DealerAnalyticsDbContext (DB already exists, need to create its tables separately)
    var dealerAnalyticsDb = scope.ServiceProvider.GetRequiredService<DealerAnalyticsService.Infrastructure.Persistence.DealerAnalyticsDbContext>();
    try
    {
        var creator = dealerAnalyticsDb.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
        try
        {
            creator.CreateTables();
            Console.WriteLine("✅ DealerAnalyticsDbContext tables created successfully");
        }
        catch
        {
            // Tables may already exist - this is fine
            Console.WriteLine("ℹ️ DealerAnalyticsDbContext tables already exist");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ DealerAnalyticsDbContext setup failed: {ex.Message}");
    }
}

app.Run();
