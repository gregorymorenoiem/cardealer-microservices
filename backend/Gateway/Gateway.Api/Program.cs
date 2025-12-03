using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Serilog;
using Serilog.Enrichers.Span;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Gateway.Metrics;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using Gateway.Api.Middleware;

// Configurar Serilog con TraceId/SpanId enrichment
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// 1. Determinar entorno
var isDevelopment = builder.Environment.IsDevelopment();
var configFile = isDevelopment ? "ocelot.dev.json" : "ocelot.prod.json";
//var configFile = isDevelopment ? "ocelot.dev.json" : "ocelot.dev.json";

// 2. Cargar configuración
builder.Configuration.AddJsonFile(configFile, optional: false, reloadOnChange: true);

// 3. Configuración esencial
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Metrics
builder.Services.AddSingleton<GatewayMetrics>();

// OpenTelemetry
var serviceName = "Gateway";
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

// 4. CORS simplificado
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        if (isDevelopment)
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins("https://inelcasrl.com.do")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .SetPreflightMaxAge(TimeSpan.FromHours(1));
        }
    });
});

// 5. Configuración Ocelot con Polly
builder.Services
    .AddOcelot(builder.Configuration)
    .AddPolly();

// 6. Swagger para Ocelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// 7. Configure Consul Client
var consulAddress = builder.Configuration["Consul:Address"] ?? "http://localhost:8500";
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(config =>
{
    config.Address = new Uri(consulAddress);
}));

// 8. Configure Service Discovery
builder.Services.AddScoped<IServiceRegistry, ConsulServiceRegistry>();
builder.Services.AddScoped<IServiceDiscovery, ConsulServiceDiscovery>();
builder.Services.AddScoped<IHealthChecker, HttpHealthChecker>();

var app = builder.Build();

// 7. Middleware pipeline
// CORS debe ir primero antes de cualquier otro middleware
app.UseCors("ReactPolicy");

// 8. Health check middleware BEFORE Ocelot to intercept /health requests
app.UseHealthCheckMiddleware();

// 9. Use routing for other endpoints
app.UseRouting();

// Solo usar Swagger si no estamos en Testing
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerForOcelotUI();
}

// Service Discovery Registration
app.UseMiddleware<ServiceRegistrationMiddleware>();

// 10. Ocelot como último middleware
await app.UseOcelot();

app.Run();

// Make Program class accessible for testing
public partial class Program { }