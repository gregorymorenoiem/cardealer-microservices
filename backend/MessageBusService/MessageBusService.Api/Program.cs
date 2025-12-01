using MessageBusService.Application.Interfaces;
using MessageBusService.Infrastructure.Data;
using MessageBusService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

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
    UserName = rabbitMQConfig["Username"] ?? "guest",
    Password = rabbitMQConfig["Password"] ?? "guest",
    VirtualHost = rabbitMQConfig["VirtualHost"] ?? "/"
};

builder.Services.AddSingleton<IConnection>(sp => factory.CreateConnection());

// Register MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(MessageBusService.Application.Commands.PublishMessageCommand).Assembly);
});

// Register Application Services
builder.Services.AddScoped<IMessagePublisher, RabbitMQPublisher>();
builder.Services.AddScoped<IMessageSubscriber, RabbitMQSubscriber>();
builder.Services.AddScoped<IDeadLetterManager, DeadLetterManager>();

// Add CORS
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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
