using CarDealer.Shared.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using VehicleIntelligenceService.Application.Features.Pricing.Queries;
using VehicleIntelligenceService.Domain.Interfaces;
using VehicleIntelligenceService.Infrastructure.Persistence;
using VehicleIntelligenceService.Infrastructure.Persistence.Repositories;
using VehicleIntelligenceService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=postgres_db;Port=5432;Database=vehicleintelligenceservice;Username=postgres;Password=postgres";

builder.Services.AddDbContext<VehicleIntelligenceDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories
builder.Services.AddScoped<IPriceAnalysisRepository, PriceAnalysisRepository>();
builder.Services.AddScoped<IDemandPredictionRepository, DemandPredictionRepository>();

// Services
builder.Services.AddScoped<IPricingEngine, PricingEngine>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetPriceSuggestionQuery).Assembly);
});

// SecurityValidation — ensures FluentValidation validators (NoSqlInjection, NoXss) run in MediatR pipeline
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(VehicleIntelligenceService.Application.Behaviors.ValidationBehavior<,>));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key must be configured via environment/settings. Do NOT use hardcoded keys.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "VehicleIntelligenceService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "VehicleIntelligenceServiceUsers";

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

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://okla.com.do",
                "https://www.okla.com.do"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VehicleIntelligenceService API",
        Version = "v1",
        Description = "API para sugerencias de pricing, demanda y tiempo estimado de venta"
    });
});

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database schema exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VehicleIntelligenceDbContext>();
    try
    {
        await db.Database.EnsureCreatedAsync();
        Console.WriteLine("✅ VehicleIntelligenceService database ready");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ VehicleIntelligenceService database init failed: {ex.Message}");
    }
}

app.Run();
