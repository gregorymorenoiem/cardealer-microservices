using RateLimitingService.Api.Middleware;
using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;
using RateLimitingService.Core.Services;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "RateLimitingService")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Rate Limiting Service API",
        Version = "v1",
        Description = "Distributed rate limiting service with sliding window algorithm"
    });
});

// Configure Rate Limiting Options
builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection(RateLimitOptions.SectionName));

// Configure Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(redisConnection);
    configuration.AbortOnConnectFail = false;
    configuration.ConnectRetry = 3;
    configuration.ConnectTimeout = 5000;
    return ConnectionMultiplexer.Connect(configuration);
});

// Register services
builder.Services.AddScoped<IRateLimitingService, RedisRateLimitingService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddRedis(redisConnection, name: "redis", tags: new[] { "db", "redis" });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rate Limiting Service API v1");
    });
}

app.UseSerilogRequestLogging();

app.UseCors();

// Note: Rate limiting middleware is available but not applied to this service's own endpoints
// Use app.UseRateLimiting() in other services that want to use this service's policies

app.MapControllers();

app.MapHealthChecks("/health");

// Log startup
Log.Information("RateLimitingService starting on port {Port}",
    builder.Configuration.GetValue<int>("Port", 15097));

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
