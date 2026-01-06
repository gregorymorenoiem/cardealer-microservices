using Microsoft.EntityFrameworkCore;
using VehiclesSaleService.Domain.Interfaces;
using VehiclesSaleService.Infrastructure.Persistence;
using VehiclesSaleService.Infrastructure.Repositories;
using VehiclesSaleService.Infrastructure.Messaging;
using CarDealer.Shared.Secrets;
using CarDealer.Shared.Configuration;
using CarDealer.Shared.MultiTenancy;

var builder = WebApplication.CreateBuilder(args);

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

var connectionString = MicroserviceSecretsConfiguration.GetDatabaseConnectionString(
    builder.Configuration,
    "DefaultConnection");

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

// RabbitMQ Event Publisher
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

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

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgres", tags: new[] { "db", "postgresql" });

var app = builder.Build();

// ========================================
// MIDDLEWARE
// ========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
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
