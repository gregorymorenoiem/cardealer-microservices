using HealthCheckService.Infrastructure;
using HealthCheckService.Application.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Health Check Aggregator API", 
        Version = "v1",
        Description = "Centralized health monitoring for all microservices"
    });
});

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetSystemHealthQueryHandler>());

// Add Infrastructure services
builder.Services.AddInfrastructure();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Configure services to monitor
var serviceConfig = new Dictionary<string, string>
{
    { "ErrorService", builder.Configuration["Services:ErrorService"] ?? "http://errorservice" },
    { "AuthService", builder.Configuration["Services:AuthService"] ?? "http://authservice" },
    { "NotificationService", builder.Configuration["Services:NotificationService"] ?? "http://notificationservice" },
    { "SchedulerService", builder.Configuration["Services:SchedulerService"] ?? "http://schedulerservice" },
    { "AuditService", builder.Configuration["Services:AuditService"] ?? "http://auditservice" }
};

builder.Services.RegisterServicesForHealthCheck(serviceConfig);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.MapControllers();

// Root health check
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "HealthCheckService",
    timestamp = DateTime.UtcNow
})).WithName("HealthCheck");

app.Run();

