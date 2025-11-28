using MediaService.Application;
using MediaService.Infrastructure;
using MediaService.Infrastructure.Extensions;
using MediaService.Infrastructure.Middleware;
using MediaService.Infrastructure.Messaging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MediaService.Infrastructure.HealthChecks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add application and infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);


builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Add RabbitMQ configuration
builder.Services.Configure<RabbitMQSettings>(options =>
{
    var rabbitMQConfig = builder.Configuration.GetSection("RabbitMQ");
    options.HostName = rabbitMQConfig["HostName"] ?? "localhost";
    options.Port = int.Parse(rabbitMQConfig["Port"] ?? "5672");
    options.UserName = rabbitMQConfig["UserName"] ?? "guest";
    options.Password = rabbitMQConfig["Password"] ?? "guest";
    options.VirtualHost = rabbitMQConfig["VirtualHost"] ?? "/";
    options.MediaEventsExchange = rabbitMQConfig["MediaEventsExchange"] ?? "media.events";
    options.MediaCommandsExchange = rabbitMQConfig["MediaCommandsExchange"] ?? "media.commands";
    options.MediaUploadedQueue = rabbitMQConfig["MediaUploadedQueue"] ?? "media.uploaded.queue";
    options.MediaProcessedQueue = rabbitMQConfig["MediaProcessedQueue"] ?? "media.processed.queue";
    options.MediaDeletedQueue = rabbitMQConfig["MediaDeletedQueue"] ?? "media.deleted.queue";
    options.ProcessMediaQueue = rabbitMQConfig["ProcessMediaQueue"] ?? "process.media.queue";
    options.MediaUploadedRoutingKey = rabbitMQConfig["MediaUploadedRoutingKey"] ?? "media.uploaded";
    options.MediaProcessedRoutingKey = rabbitMQConfig["MediaProcessedRoutingKey"] ?? "media.processed";
    options.MediaDeletedRoutingKey = rabbitMQConfig["MediaDeletedRoutingKey"] ?? "media.deleted";
    options.ProcessMediaRoutingKey = rabbitMQConfig["ProcessMediaRoutingKey"] ?? "media.process";
});

// Add RabbitMQ services
builder.Services.AddSingleton<IRabbitMQMediaProducer, RabbitMQMediaProducer>();
builder.Services.AddHostedService<RabbitMQMediaConsumer>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", HealthStatus.Unhealthy)
    .AddCheck<StorageHealthCheck>("storage", HealthStatus.Degraded);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use custom middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthorization();
app.MapControllers();

// Map health checks
app.MapHealthChecks("/health");

app.Run();