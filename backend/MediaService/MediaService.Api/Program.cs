using MediaService.Application;
using MediaService.Infrastructure;
using MediaService.Infrastructure.Extensions;
using MediaService.Infrastructure.Middleware;
using MediaService.Infrastructure.Messaging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MediaService.Infrastructure.HealthChecks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Enrichers.Span;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using MediaService.Domain.Interfaces;
using MediaService.Infrastructure.BackgroundServices;
using MediaService.Infrastructure.Metrics;
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

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add application and infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Dead Letter Queue
builder.Services.AddSingleton<IDeadLetterQueue, InMemoryDeadLetterQueue>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<InMemoryDeadLetterQueue>>();
    return new InMemoryDeadLetterQueue(logger, maxRetries: 5);
});
builder.Services.AddHostedService<DeadLetterQueueProcessor>();

// Metrics
builder.Services.AddSingleton<MediaServiceMetrics>();

// Polly 8.x Circuit Breaker
builder.Services.AddResiliencePipeline("media-circuit-breaker", pipelineBuilder =>
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
var serviceName = "MediaService";
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


builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Add RabbitMQ configuration
builder.Services.Configure<RabbitMQSettings>(options =>
{
    var rabbitMQConfig = builder.Configuration.GetSection("RabbitMQ");
    options.HostName = rabbitMQConfig["HostName"] ?? "localhost";
    options.Port = int.Parse(rabbitMQConfig["Port"] ?? "5672");
    options.UserName = rabbitMQConfig["UserName"] ?? "guest";
    options.Password = rabbitMQConfig["Password"] ?? "guest";
    options.VirtualHost = rabbitMQConfig["VirtualHost"] ?? "/";
    options.MediaEventsExchange = rabbitMQConfig["MediaEventsExchange"] ?? "media.events";
    options.MediaCommandsExchange = rabbitMQConfig["MediaCommandsExchange"] ?? "media.commands";
    options.MediaUploadedQueue = rabbitMQConfig["MediaUploadedQueue"] ?? "media.uploaded.queue";
    options.MediaProcessedQueue = rabbitMQConfig["MediaProcessedQueue"] ?? "media.processed.queue";
    options.MediaDeletedQueue = rabbitMQConfig["MediaDeletedQueue"] ?? "media.deleted.queue";
    options.ProcessMediaQueue = rabbitMQConfig["ProcessMediaQueue"] ?? "process.media.queue";
    options.MediaUploadedRoutingKey = rabbitMQConfig["MediaUploadedRoutingKey"] ?? "media.uploaded";
    options.MediaProcessedRoutingKey = rabbitMQConfig["MediaProcessedRoutingKey"] ?? "media.processed";
    options.MediaDeletedRoutingKey = rabbitMQConfig["MediaDeletedRoutingKey"] ?? "media.deleted";
    options.ProcessMediaRoutingKey = rabbitMQConfig["ProcessMediaRoutingKey"] ?? "media.process";
});

// Add RabbitMQ services
builder.Services.AddSingleton<IRabbitMQMediaProducer, RabbitMQMediaProducer>();
builder.Services.AddHostedService<RabbitMQMediaConsumer>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", HealthStatus.Unhealthy)
    .AddCheck<StorageHealthCheck>("storage", HealthStatus.Degraded);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use custom middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthorization();
app.MapControllers();

// Map health checks
app.MapHealthChecks("/health");

app.Run();