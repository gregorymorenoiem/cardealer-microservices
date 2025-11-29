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
using CarDealer.Shared.Database;

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

// Database Context (multi-provider configuration)
builder.Services.AddDatabaseProvider<ApplicationDbContext>(builder.Configuration);

// MediatR - Cargar assemblies de Application
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("NotificationService.Application")));

// FluentValidation - Validators
builder.Services.AddValidatorsFromAssembly(Assembly.Load("NotificationService.Application"));

// Configure settings
builder.Services.Configure<NotificationSettings>(
    builder.Configuration.GetSection("NotificationSettings"));

var app = builder.Build();

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