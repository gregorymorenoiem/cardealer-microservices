using ConfigurationService.Application.Interfaces;
using ConfigurationService.Infrastructure.Data;
using ConfigurationService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using ConfigurationService.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient for health checks
builder.Services.AddHttpClient();

// Database
builder.Services.AddDbContext<ConfigurationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Service Discovery Configuration
builder.Services.AddSingleton<IConsulClient>(sp =>
{
    var consulAddress = builder.Configuration["Consul:Address"] ?? "http://localhost:8500";
    return new ConsulClient(config => config.Address = new Uri(consulAddress));
});

builder.Services.AddScoped<IServiceRegistry, ConsulServiceRegistry>();
builder.Services.AddScoped<IServiceDiscovery, ConsulServiceDiscovery>();
builder.Services.AddHttpClient("HealthCheck");
builder.Services.AddScoped<IHealthChecker, HttpHealthChecker>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ConfigurationService.Application.Commands.CreateConfigurationCommand).Assembly);
});

// Application Services
var encryptionKey = builder.Configuration["Encryption:Key"] ?? "DefaultKey123!ChangeMe";
builder.Services.AddSingleton<IEncryptionService>(new AesEncryptionService(encryptionKey));
builder.Services.AddScoped<ConfigurationService.Application.Interfaces.IConfigurationManager, ConfigurationService.Infrastructure.Services.ConfigurationManager>();
builder.Services.AddScoped<ISecretManager, SecretManager>();
builder.Services.AddScoped<IFeatureFlagManager, FeatureFlagManager>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "ConfigurationService" }));

// Service Discovery Auto-Registration
app.UseMiddleware<ServiceRegistrationMiddleware>();

app.MapControllers();

app.Run();

// Make the implicit Program class public for integration tests
public partial class Program { }
