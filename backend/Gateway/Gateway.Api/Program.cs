using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Serilog;
using Serilog.Enrichers.Span;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using Gateway.Api.Middleware;
using Gateway.Domain.Interfaces;
using Gateway.Infrastructure.Services;
using Gateway.Application.UseCases;
using CarDealer.Shared.Configuration;

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

// Clean Architecture - Domain Services
builder.Services.AddScoped<IRoutingService, RoutingService>();
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

// Clean Architecture - Application Use Cases
builder.Services.AddScoped<CheckRouteExistsUseCase>();
builder.Services.AddScoped<ResolveDownstreamPathUseCase>();
builder.Services.AddScoped<CheckServiceHealthUseCase>();
builder.Services.AddScoped<GetServicesHealthUseCase>();
builder.Services.AddScoped<RecordRequestMetricsUseCase>();
builder.Services.AddScoped<RecordDownstreamCallMetricsUseCase>();

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

// 4. CORS configurado para múltiples frontends
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        if (isDevelopment)
        {
            // Permitir múltiples puertos de desarrollo
            policy.WithOrigins(
                    "http://localhost:5173",    // Vite default
                    "http://localhost:5174",    // Frontend original
                    "http://localhost:3000",    // React alternative
                    "http://localhost:4200",    // Angular (si aplica)
                    "http://localhost:8080"     // Frontend Docker
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Producción
            policy.WithOrigins(
                    "https://inelcasrl.com.do",
                    "https://www.inelcasrl.com.do",
                    "https://cardealer.app",
                    "https://www.cardealer.app"
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .SetPreflightMaxAge(TimeSpan.FromHours(1));
        }
    });
});

// 4.1 JWT Authentication from externalized secrets
try
{
    var (jwtKey, jwtIssuer, jwtAudience) = MicroserviceSecretsConfiguration.GetJwtConfig(builder.Configuration);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

    Log.Information("JWT Authentication configured successfully for Gateway");
}
catch (InvalidOperationException ex)
{
    Log.Warning("JWT Authentication not configured: {Message}. Routes requiring auth will fail.", ex.Message);
}

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

// 9.1 Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

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