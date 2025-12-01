using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Extensions;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Providers;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Domain.Interfaces;
using Serilog;
using Serilog.Enrichers.Span;
using System.Reflection;
using FluentValidation;
using NotificationService.Shared;
using CarDealer.Shared.Database;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using NotificationService.Infrastructure.BackgroundServices;
using NotificationService.Infrastructure.Metrics;
using Polly;
using Polly.CircuitBreaker;

// Configurar Serilog con TraceId/SpanId enrichment
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

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

// Dead Letter Queue
builder.Services.AddSingleton<IDeadLetterQueue, InMemoryDeadLetterQueue>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<InMemoryDeadLetterQueue>>();
    return new InMemoryDeadLetterQueue(logger, maxRetries: 5);
});
builder.Services.AddHostedService<DeadLetterQueueProcessor>();

// Metrics
builder.Services.AddSingleton<NotificationServiceMetrics>();

// Polly 8.x Circuit Breaker
builder.Services.AddResiliencePipeline("notification-circuit-breaker", pipelineBuilder =>
{
    pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        MinimumThroughput = 10,
        BreakDuration = TimeSpan.FromSeconds(30)
    });
});

// OpenTelemetry
var serviceName = "NotificationService";
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName, serviceVersion))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(serviceName);

        if (builder.Environment.IsDevelopment())
        {
            tracing.AddConsoleExporter();
        }
        else
        {
            tracing.AddOtlpExporter();
            tracing.SetSampler(new TraceIdRatioBasedSampler(0.1)); // 10% sampling in production
        }
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter(serviceName);

        if (builder.Environment.IsDevelopment())
        {
            metrics.AddConsoleExporter();
        }
        else
        {
            metrics.AddOtlpExporter();
        }
    });

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