using IdempotencyService.Api.Middleware;
using IdempotencyService.Core.Interfaces;
using IdempotencyService.Core.Models;
using IdempotencyService.Core.Services;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "IdempotencyService")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Idempotency Service API",
        Version = "v1",
        Description = "Service for managing request idempotency with Redis-based storage",
        Contact = new OpenApiContact
        {
            Name = "CarDealer Team",
            Email = "dev@cardealer.com"
        }
    });
});

// Configure idempotency options
builder.Services.Configure<IdempotencyOptions>(
    builder.Configuration.GetSection(IdempotencyOptions.SectionName));

// Add Redis distributed cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "idempotency:";
});

// Register idempotency service
builder.Services.AddScoped<IIdempotencyService, RedisIdempotencyService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379", name: "redis");

// CORS
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Idempotency Service API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseSerilogRequestLogging();
app.UseCors();

// Use idempotency middleware for external consumers
// Note: This service exposes the middleware for other services to use
// app.UseIdempotency();

app.MapControllers();
app.MapHealthChecks("/health");

// Simple test endpoint to demonstrate idempotency
app.MapPost("/api/test/order", async (HttpContext context, IIdempotencyService idempotencyService) =>
{
    // This endpoint demonstrates how idempotency works
    var orderId = Guid.NewGuid().ToString();
    return Results.Created($"/api/test/order/{orderId}", new { orderId, createdAt = DateTime.UtcNow });
})
.WithName("CreateTestOrder")
.WithOpenApi();

Log.Information("IdempotencyService starting...");

app.Run();

