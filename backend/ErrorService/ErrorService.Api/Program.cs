using ErrorService.Domain.Interfaces;
using ErrorService.Infrastructure.Persistence;
using ErrorService.Infrastructure.Services;
using ErrorService.Infrastructure.Services.Messaging;
using ErrorService.Shared.Extensions;
using ErrorService.Shared.Middleware;
using ErrorService.Shared.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Application Services
builder.Services.AddScoped<IErrorLogRepository, EfErrorLogRepository>();
builder.Services.AddScoped<IErrorReporter, ErrorReporter>();

// Agregar MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ErrorService.Application.UseCases.LogError.LogErrorCommand).Assembly));

// Configurar el manejo de errores
builder.Services.AddErrorHandling("ErrorService");

// Configurar Rate Limiting
var rateLimitingConfig = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingConfiguration>()
    ?? new RateLimitingConfiguration();
builder.Services.AddCustomRateLimiting(rateLimitingConfig);

// Configurar RabbitMQ
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<ErrorServiceRabbitMQSettings>(builder.Configuration.GetSection("ErrorService"));

// Registrar el consumidor RabbitMQ como hosted service
builder.Services.AddHostedService<RabbitMQErrorConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware para bypass de Rate Limiting (debe estar antes del middleware de rate limiting)
app.UseMiddleware<RateLimitBypassMiddleware>();

// Middleware para Rate Limiting (debe estar antes de otros middlewares)
app.UseCustomRateLimiting(rateLimitingConfig);

app.UseHttpsRedirection();

// Middleware para capturar respuestas
app.UseMiddleware<ResponseCaptureMiddleware>();

// Middleware para manejo de errores
app.UseErrorHandling();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok("ErrorService is healthy"));

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Log.Information("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying database migrations.");
    }
}

Log.Information("ErrorService starting up...");
app.Run();