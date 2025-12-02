using FeatureToggleService.Application;
using FeatureToggleService.Infrastructure;
using HealthChecks.NpgSql;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "FeatureToggleService")
    .WriteTo.Console()
    .WriteTo.File("logs/featuretoggle-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Feature Toggle Service API",
        Version = "v1",
        Description = "Microservicio para gestión de Feature Flags y toggles de funcionalidades",
        Contact = new OpenApiContact
        {
            Name = "CarDealer Team",
            Email = "dev@cardealer.com"
        }
    });
});

// Application & Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health Checks
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString!, name: "postgresql", tags: new[] { "db", "postgresql" });

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Feature Toggle Service API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseSerilogRequestLogging();
app.UseCors("AllowSpecificOrigins");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Auto-migrate in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<FeatureToggleService.Infrastructure.Data.FeatureToggleDbContext>();
    await context.Database.EnsureCreatedAsync();
}

Log.Information("Feature Toggle Service starting on {Urls}", string.Join(", ", app.Urls));

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Feature Toggle Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
