using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Extensions;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Providers;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Domain.Interfaces;
using Serilog;
using System.Reflection;
using FluentValidation;
using NotificationService.Shared;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging();

// Simple Health Check
builder.Services.AddHealthChecks();

// ✅ USAR DEPENDENCY INJECTION DE INFRASTRUCTURE (INCLUYE RABBITMQ)
builder.Services.AddInfrastructure(builder.Configuration);

// 🔧 Register Teams Provider
builder.Services.AddHttpClient<ITeamsProvider, TeamsProvider>();

// 🔧 Register ErrorCriticalEvent Consumer as Hosted Service
builder.Services.AddHostedService<ErrorCriticalEventConsumer>();

// 1) DbContext del NotificationService
var notificationConn = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"[DEBUG] NotificationService Connection = '{notificationConn}'");
if (string.IsNullOrWhiteSpace(notificationConn))
{
    throw new InvalidOperationException("La cadena DefaultConnection no está configurada.");
}

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseNpgsql(notificationConn)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);

// MediatR - Cargar assemblies de Application
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("NotificationService.Application")));

// FluentValidation - Validators
builder.Services.AddValidatorsFromAssembly(Assembly.Load("NotificationService.Application"));

// Configure settings
builder.Services.Configure<NotificationSettings>(
    builder.Configuration.GetSection("NotificationSettings"));

var app = builder.Build();

// **Aplicar migraciones para NotificationService**
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Migraciones del NotificationService
        var notificationContext = services.GetRequiredService<ApplicationDbContext>();
        notificationContext.Database.Migrate();
        Log.Information("NotificationService database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok("NotificationService is healthy"));

app.MapControllers();

Log.Information("NotificationService starting up with ErrorService middleware and RabbitMQ Consumer...");
app.Run();