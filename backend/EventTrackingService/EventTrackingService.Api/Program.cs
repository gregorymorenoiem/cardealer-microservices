using EventTrackingService.Application.Features.Events.Commands;
using EventTrackingService.Application.Features.Events.Queries;
using EventTrackingService.Domain.Interfaces;
using EventTrackingService.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// SERVICES CONFIGURATION
// ============================================

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Controllers
builder.Services.AddControllers();

// MediatR (CQRS)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IngestEventCommand).Assembly);
});

// ClickHouse Repository
var clickHouseConnectionString = builder.Configuration.GetConnectionString("ClickHouse") 
    ?? "Host=localhost;Port=8123;Database=okla_events;User=default;Password=;Compression=true";

builder.Services.AddScoped<IEventRepository>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<ClickHouseEventRepository>>();
    return new ClickHouseEventRepository(clickHouseConnectionString, logger);
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OKLA Event Tracking API",
        Version = "v1",
        Description = "API para rastrear eventos de usuario y generar analytics en tiempo real",
        Contact = new OpenApiContact
        {
            Name = "OKLA Team",
            Email = "dev@okla.com.do"
        }
    });
});

// Health Checks
builder.Services.AddHealthChecks();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE
// ============================================

// Swagger en Development y Production (para debugging)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Tracking API V1");
    c.RoutePrefix = string.Empty; // Swagger en root
});

// CORS (debe ir antes de UseRouting)
app.UseCors("AllowAll");

app.UseRouting();

app.MapControllers();
app.MapHealthChecks("/health");

// ============================================
// RUN APPLICATION
// ============================================

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Logger.LogInformation("🚀 EventTrackingService starting on port {Port}", port);
app.Logger.LogInformation("📊 ClickHouse Connection: {Connection}", clickHouseConnectionString.Split(';')[0]);

app.Run();
