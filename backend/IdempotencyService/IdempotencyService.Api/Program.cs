using IdempotencyService.Api.Extensions;
using IdempotencyService.Api.Middleware;
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

// Add idempotency services with automatic filter
builder.Services.AddIdempotency(builder.Configuration);

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Idempotency Service API",
        Version = "v1",
        Description = "Service for managing request idempotency with automatic middleware and attribute-based control",
        Contact = new OpenApiContact
        {
            Name = "CarDealer Team",
            Email = "dev@cardealer.com"
        }
    });

    // Add idempotency header to Swagger
    c.OperationFilter<IdempotencyHeaderOperationFilter>();
});

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

// Optional: Use idempotency middleware for all requests
// By default, we use attribute-based idempotency (IdempotentAttribute)
// Uncomment to enable middleware for ALL requests:
// app.UseIdempotencyMiddleware(options => options.UseMiddleware = true);

app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("IdempotencyService starting...");

app.Run();

