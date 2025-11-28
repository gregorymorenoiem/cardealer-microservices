using Microsoft.EntityFrameworkCore;
using AuthService.Infrastructure.Extensions;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Messaging;
using AuthService.Domain.Interfaces;
using Serilog;
using System.Reflection;
using FluentValidation;
using AuthService.Shared;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Cors;
using System.Threading.RateLimiting;
using AuthService.Infrastructure.Services.Messaging;
using AuthService.Infrastructure.Middleware;
using AuthService.Domain.Interfaces.Services;
using AuthService.Infrastructure.External;
using AuthService.Infrastructure.Services.Notification;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthPolicy", limiterOptions =>
    {
        var rateLimitSettings = builder.Configuration.GetSection("Security:RateLimit").Get<RateLimitSettings>();
        limiterOptions.PermitLimit = rateLimitSettings?.RequestsPerMinute ?? 60;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});

// CORS 
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new string[0];
        var allowedMethods = builder.Configuration.GetSection("Cors:AllowedMethods").Get<string[]>() ?? new string[0];
        var allowedHeaders = builder.Configuration.GetSection("Cors:AllowedHeaders").Get<string[]>() ?? new string[0];
        var allowCredentials = builder.Configuration.GetValue<bool>("Cors:AllowCredentials");

        policy.WithOrigins(allowedOrigins)
              .WithMethods(allowedMethods)
              .WithHeaders(allowedHeaders);

        if (allowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});

// TODA LA CONFIGURACIÓN EN UN SOLO LUGAR
builder.Services.AddInfrastructure(builder.Configuration);

// DbContext
var authConn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(authConn))
    throw new InvalidOperationException("La cadena DefaultConnection no está configurada.");

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseNpgsql(authConn).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

// Event Publisher for RabbitMQ
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

// MediatR & FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("AuthService.Application")));
builder.Services.AddValidatorsFromAssembly(Assembly.Load("AuthService.Application"));

// Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<NotificationServiceSettings>(builder.Configuration.GetSection("NotificationService"));

// Configurar RabbitMQ
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<ErrorServiceRabbitMQSettings>(builder.Configuration.GetSection("ErrorService"));
builder.Services.Configure<NotificationServiceRabbitMQSettings>(builder.Configuration.GetSection("NotificationService"));

// Registrar servicios de RabbitMQ
builder.Services.AddSingleton<IErrorEventProducer, RabbitMQErrorProducer>();
builder.Services.AddSingleton<INotificationEventProducer, RabbitMQNotificationProducer>();

// Registrar AuthNotificationService con el constructor CORRECTO (5 parámetros)
builder.Services.AddScoped<IAuthNotificationService>(provider =>
{
    var notificationClient = provider.GetRequiredService<NotificationServiceClient>();
    var notificationProducer = provider.GetRequiredService<INotificationEventProducer>();
    var settings = provider.GetRequiredService<IOptions<NotificationServiceSettings>>();
    var rabbitMqSettings = provider.GetRequiredService<IOptions<NotificationServiceRabbitMQSettings>>();
    var logger = provider.GetRequiredService<ILogger<AuthNotificationService>>();

    return new AuthNotificationService(
        notificationClient,
        notificationProducer,
        settings,
        rabbitMqSettings,
        logger
    );
});

// 🚨 CONSTRUIR LA APLICACIÓN
var app = builder.Build();

// Migraciones
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var authContext = services.GetRequiredService<ApplicationDbContext>();
        authContext.Database.Migrate();
        Log.Information("AuthService database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error during database migration");
    }
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Health Checks (ahora configurado en Infrastructure)
app.MapHealthChecks("/health");

app.MapControllers();

Log.Information("AuthService starting up...");
app.Run();
