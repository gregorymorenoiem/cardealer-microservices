using Microsoft.EntityFrameworkCore;
using PropertiesSaleService.Domain.Interfaces;
using PropertiesSaleService.Infrastructure.Persistence;
using PropertiesSaleService.Infrastructure.Repositories;
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
        Title = "PropertiesSaleService API",
        Version = "v1",
        Description = "API for real estate sales marketplace - buy houses, condos, land, and commercial properties"
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

builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

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

Console.WriteLine("🚀 PropertiesSaleService API starting...");
Console.WriteLine($"📦 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"🗄️  Database: {connectionString}");

app.Run();
