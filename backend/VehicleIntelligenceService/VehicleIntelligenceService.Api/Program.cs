using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using VehicleIntelligenceService.Application.Features.Demand.Queries;
using VehicleIntelligenceService.Application.Features.Pricing.Queries;
using VehicleIntelligenceService.Domain.Entities;
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
builder.Services.AddScoped<IPriceSuggestionRepository, PriceSuggestionRepository>();
builder.Services.AddScoped<ICategoryDemandRepository, CategoryDemandRepository>();

// Services
builder.Services.AddScoped<IVehiclePricingEngine, VehiclePricingEngine>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetPriceSuggestionQuery).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetDemandByCategoryQuery).Assembly);
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-min-32-chars-long-12345678";
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
        Description = "API para sugerencias de pricing, demanda y tiempo estimado de venta (Sprint 18)"
    });
});

// Health Checks
builder.Services.AddHealthChecks().AddDbContextCheck<VehicleIntelligenceDbContext>("database");

var app = builder.Build();

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

// Ensure database schema exists (many services in this repo don't ship migrations)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VehicleIntelligenceDbContext>();
    try
    {
        await db.Database.EnsureCreatedAsync();

        // Seed baseline demand categories if empty
        if (!await db.CategoryDemandSnapshots.AnyAsync())
        {
            db.CategoryDemandSnapshots.AddRange(
                new CategoryDemandSnapshot { Category = "SUV", DemandScore = 78, Trend = "up" },
                new CategoryDemandSnapshot { Category = "Sedan", DemandScore = 60, Trend = "stable" },
                new CategoryDemandSnapshot { Category = "Camioneta", DemandScore = 82, Trend = "up" },
                new CategoryDemandSnapshot { Category = "Deportivo", DemandScore = 65, Trend = "stable" },
                new CategoryDemandSnapshot { Category = "Electrico", DemandScore = 70, Trend = "up" }
            );
            await db.SaveChangesAsync();
        }

        Console.WriteLine("✅ VehicleIntelligenceService database ready");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ VehicleIntelligenceService database init failed: {ex.Message}");
    }
}

app.Run();

