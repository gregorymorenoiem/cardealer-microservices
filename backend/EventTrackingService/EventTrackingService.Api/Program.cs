using CarDealer.Shared.Middleware;
using EventTrackingService.Application.Features.Events.Commands;
using EventTrackingService.Application.Features.Events.Queries;
using EventTrackingService.Domain.Interfaces;
using EventTrackingService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// SERVICES CONFIGURATION
// ============================================

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
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

// Controllers
builder.Services.AddControllers();

// MediatR (CQRS)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IngestEventCommand).Assembly);
});

// SecurityValidation — ensures FluentValidation validators (NoSqlInjection, NoXss) run in MediatR pipeline
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(EventTrackingService.Application.Behaviors.ValidationBehavior<,>));

// Event Repository - Use PostgreSQL for production-ready persistent storage
// Supports auto-scaling: all replicas share the same database
var eventDbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["Database:ConnectionStrings:PostgreSQL"]
    ?? "Host=localhost;Port=5432;Database=eventtrackingservice;Username=postgres;Password=password";

builder.Services.AddSingleton<IEventRepository>(sp =>
    new PostgreSqlEventRepository(
        eventDbConnectionString,
        sp.GetRequiredService<ILogger<PostgreSqlEventRepository>>()));

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

// OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Tracking API V1");
    c.RoutePrefix = string.Empty; // Swagger en root
});

// CORS (debe ir antes de UseRouting)
app.UseCors();

app.UseRouting();

app.MapControllers();
app.MapHealthChecks("/health");

// ============================================
// RUN APPLICATION
// ============================================

// Use ASPNETCORE_URLS if set, otherwise default to port 80 for Docker
var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (string.IsNullOrEmpty(urls))
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
    app.Urls.Add($"http://0.0.0.0:{port}");
}

app.Logger.LogInformation("🚀 EventTrackingService starting");
app.Logger.LogInformation("📊 Using {RepositoryType} event repository", "PostgreSQL");

app.Run();
