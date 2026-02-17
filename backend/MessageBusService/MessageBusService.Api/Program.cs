using CarDealer.Shared.Middleware;
using MessageBusService.Application.Interfaces;
using MessageBusService.Infrastructure.Data;
using MessageBusService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using MessageBusService.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

// Add HttpClient
builder.Services.AddHttpClient();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MessageBusDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure RabbitMQ
var rabbitMQConfig = builder.Configuration.GetSection("RabbitMQ");
var factory = new ConnectionFactory
{
    HostName = rabbitMQConfig["Host"] ?? "localhost",
    Port = int.Parse(rabbitMQConfig["Port"] ?? "5672"),
    UserName = rabbitMQConfig["Username"] ?? throw new InvalidOperationException("RabbitMQ:Username is not configured"),
    Password = rabbitMQConfig["Password"] ?? throw new InvalidOperationException("RabbitMQ:Password is not configured"),
    VirtualHost = rabbitMQConfig["VirtualHost"] ?? "/"
};

builder.Services.AddSingleton<IConnection>(sp => factory.CreateConnection());

// Register MediatR
builder.Services.AddMediatR(cfg =>

// SecurityValidation — ensures FluentValidation validators (NoSqlInjection, NoXss) run in MediatR pipeline
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(MessageBusService.Application.Behaviors.ValidationBehavior<,>));
{
    cfg.RegisterServicesFromAssembly(typeof(MessageBusService.Application.Commands.PublishMessageCommand).Assembly);
});

// Register Application Services
builder.Services.AddScoped<IMessagePublisher, RabbitMQPublisher>();
builder.Services.AddScoped<IMessageSubscriber, RabbitMQSubscriber>();
builder.Services.AddScoped<IDeadLetterManager, DeadLetterManager>();

// Register Saga Services
builder.Services.AddScoped<MessageBusService.Application.Interfaces.ISagaRepository, MessageBusService.Infrastructure.Repositories.SagaRepository>();
builder.Services.AddScoped<MessageBusService.Application.Interfaces.ISagaOrchestrator, MessageBusService.Infrastructure.Services.SagaOrchestrator>();

// Register Saga Step Executors
builder.Services.AddScoped<MessageBusService.Application.Interfaces.ISagaStepExecutor, MessageBusService.Infrastructure.Services.RabbitMQSagaStepExecutor>();
builder.Services.AddScoped<MessageBusService.Application.Interfaces.ISagaStepExecutor, MessageBusService.Infrastructure.Services.HttpSagaStepExecutor>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        if (isDev)
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(
                    "https://okla.com.do",
                    "https://www.okla.com.do",
                    "https://api.okla.com.do")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// Configure Consul Client
var consulAddress = builder.Configuration["Consul:Address"] ?? "http://localhost:8500";
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(config =>
{
    config.Address = new Uri(consulAddress);
}));

// Configure Service Discovery
builder.Services.AddScoped<IServiceRegistry, ConsulServiceRegistry>();
builder.Services.AddScoped<IServiceDiscovery, ConsulServiceDiscovery>();
builder.Services.AddScoped<IHealthChecker, HttpHealthChecker>();

var app = builder.Build();

// Configure the HTTP request pipeline
// OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Service Discovery Registration
app.UseMiddleware<ServiceRegistrationMiddleware>();

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "MessageBusService" }));

app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
